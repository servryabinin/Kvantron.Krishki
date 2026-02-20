using CapDefectDetector.Logger;
using CapDefectDetector.Processing;
using CapDefectDetector.Processing.Utils;
using OpenCvSharp;
using System;
using System.Threading;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.Processing.Detectors
{
    internal class UnderfillDetector : ICapDetector
    {
        private double[] _sinTable;
        private double[] _cosTable;
        private readonly object _trigTablesLock = new object();
        private const int UNDERFILL_RECT_WIDTH = 1024;

        private readonly double _coefCapRadiusUnderFill;
        private readonly double _corrugationsCountForUnderFill;

        public UnderfillDetector(ProcessingParameters param)
        {
            _coefCapRadiusUnderFill = param.CoefCapRadiusUnderFill;
            _corrugationsCountForUnderFill = param.CorrugationsCountForUnderFill;
            InitializeTrigTables();
        }

        /// <summary>
        /// Инициализация таблиц синусов и косинусов для оптимизации вычислений при развороте коронки. 
        /// Таблицы заполняются один раз при создании экземпляра детектора и используются для быстрого доступа к значениям синуса и косинуса при расчете координат точек на коронке.
        /// </summary>
        private void InitializeTrigTables()
        {
            lock (_trigTablesLock)
            {
                _sinTable = new double[UNDERFILL_RECT_WIDTH];
                _cosTable = new double[UNDERFILL_RECT_WIDTH];

                double dTheta = 2 * Math.PI / UNDERFILL_RECT_WIDTH;
                for (int i = 0; i < UNDERFILL_RECT_WIDTH; i++)
                {
                    _sinTable[i] = Math.Sin(i * dTheta);
                    _cosTable[i] = Math.Cos(i * dTheta);
                }
            }
        }

        /// <summary>
        /// Проверка крышки на недолив путем анализа коронки. 
        /// Процесс включает в себя цветокоррекцию, выделение коронки, разворот коронки в прямоугольное изображение, статистический анализ строк и Фурье-анализ для выявления признаков недолива. 
        /// Результат отображается на исходном изображении путем обводки внутреннего эллипса красным (дефект) или зеленым (без дефекта).
        /// </summary>
        /// <param name="gray">Грейскейл изображение</param>
        /// <param name="image">Исходное изображение</param>
        /// <param name="drawFrame">Кадр для отрисовки результата</param>
        /// <param name="contour">Контур крышки</param>
        /// <param name="param">Параметры рецепта</param>
        /// <param name="token">Токен отмены</param>
        /// <returns>true если дефект найден</returns>
        public bool Detect(
            Mat gray, 
            Mat image, 
            Mat draw,
            Point[] capContour,
            Mat blurChannel1,
            Mat blurChannel2,
            ProcessingParameters param, 
            CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length < 5)
                    return false;

                // 1️⃣ Цветокоррекция
                Mat corrected = image.Clone();
                ContourHelper.NonlinearBackgroundDecolorization(corrected, param.CapsColor, true, true, false, param.GreenThreshold);

                Mat correctedGray = new Mat();
                Cv2.CvtColor(corrected, correctedGray, ColorConversionCodes.BGR2GRAY);

                token.ThrowIfCancellationRequested();

                // 2️⃣ Этап: подгонка эллипса
                RotatedRect outerEllipse = Cv2.FitEllipse(capContour);

                Size2f outerSize = outerEllipse.Size;
                Size2f innerSize = new Size2f(
                    (float)(outerSize.Width * _coefCapRadiusUnderFill),
                    (float)(outerSize.Height * _coefCapRadiusUnderFill)
                );

                RotatedRect innerEllipse = new RotatedRect(
                    outerEllipse.Center,
                    innerSize,
                    outerEllipse.Angle
                );

                token.ThrowIfCancellationRequested();

                // 3️⃣ Выделяем коронку
                int rectHeight = (int)(Math.Max(outerSize.Width, outerSize.Height) * 0.15);
                Mat crownMask = new Mat(corrected.Size(), MatType.CV_8UC1, Scalar.All(0));
                Cv2.Ellipse(crownMask, outerEllipse, Scalar.White, -1);
                Cv2.Ellipse(crownMask, innerEllipse, Scalar.Black, -1);

                Mat maskedGray = new Mat();
                Cv2.BitwiseAnd(correctedGray, correctedGray, maskedGray, crownMask);

                token.ThrowIfCancellationRequested();

                // 4️⃣ Разворачиваем коронку
                Mat rectifiedCrown = new Mat(rectHeight, UNDERFILL_RECT_WIDTH, MatType.CV_8UC1);
                float outerRadius = (float)(Math.Max(outerSize.Width, outerSize.Height) / 2);
                float innerRadius = (float)(Math.Max(innerSize.Width, innerSize.Height) / 2);
                float meanRadius = (outerRadius + innerRadius) / 2;

                Point center = new Point((int)outerEllipse.Center.X, (int)outerEllipse.Center.Y);
                GetStripeImg(maskedGray, rectifiedCrown, _sinTable, _cosTable, center, (int)meanRadius, maskedGray.Width, maskedGray.Height, UNDERFILL_RECT_WIDTH, rectHeight);

                token.ThrowIfCancellationRequested();

                // 5️⃣ Статистика по строкам
                float[] rowStatistics = new float[UNDERFILL_RECT_WIDTH];
                GetWStatistics(rectifiedCrown, rowStatistics, UNDERFILL_RECT_WIDTH, rectHeight);

                token.ThrowIfCancellationRequested();

                // 6️⃣ Анализ через FFT (подразумевается, что AnalyzeUnderfillFFT реализован)
                bool hasUnderfill = AnalyzeUnderfillFFT(rowStatistics, UNDERFILL_RECT_WIDTH, _corrugationsCountForUnderFill);

                // 7️⃣ Отрисовка результата
                Scalar color = hasUnderfill ? new Scalar(0, 0, 255) : new Scalar(0, 255, 0);
                Cv2.Ellipse(draw, innerEllipse, color, 2);

                // Освобождаем ресурсы
                crownMask.Dispose();
                maskedGray.Dispose();
                corrected.Dispose();
                correctedGray.Dispose();
                rectifiedCrown.Dispose();

                return hasUnderfill;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "UnderfillDetector.Detect - ошибка при определении недолива");
                return false;
            }
        }


        /// <summary>
        /// Вычисляет горизонтальную статистику развёрнутого изображения коронки.
        /// Для каждого столбца изображения суммирует значения интенсивности пикселей по всей высоте.
        /// Результат используется для анализа дефектов (например, разрывов, неоднородностей и т.п.).
        /// </summary>
        /// <param name="src">
        /// Исходное одноканальное 8-битное изображение (развёрнутая коронка),
        /// из которого вычисляется статистика.
        /// </param>
        /// <param name="dst">
        /// Выходной массив длиной <paramref name="w"/>, в который записывается сумма
        /// значений пикселей для каждого столбца изображения.
        /// Перед вычислением массив очищается.
        /// </param>
        /// <param name="w">
        /// Ширина изображения в пикселях (количество столбцов).
        /// Также соответствует длине массива <paramref name="dst"/>.
        /// </param>
        /// <param name="h">
        /// Высота изображения в пикселях (количество строк),
        /// по которым выполняется суммирование.
        /// </param>
        private static void GetWStatistics(Mat src, float[] dst, int w, int h)
        {
            Array.Clear(dst, 0, dst.Length);
            unsafe
            {
                byte* srcPtr = (byte*)src.DataPointer;
                fixed (float* dstPtr = dst)
                {
                    for (int j = 0; j < h; j++)
                    {
                        byte* rowPtr = srcPtr + j * w;
                        for (int i = 0; i < w; i++)
                            dstPtr[i] += rowPtr[i];
                    }
                }
            }
        }

        /// <summary>
        /// Формирует развёрнутое (полярное) изображение коронки крышки в прямоугольную полосу.
        /// Каждый столбец результирующего изображения соответствует углу окружности,
        /// а каждая строка — расстоянию от внутреннего радиуса к внешнему.
        /// Используется для последующего анализа дефектов коронки (облой, разрывы, включения и т.д.).
        /// </summary>
        /// <param name="src">
        /// Исходное одноканальное 8-битное изображение, содержащее коронку.
        /// </param>
        /// <param name="dst">
        /// Выходное одноканальное 8-битное изображение (развёрнутая полоса),
        /// где ось X соответствует углу, а ось Y — радиальному расстоянию.
        /// Размер должен быть не менее <paramref name="height"/> × <paramref name="width"/>.
        /// </param>
        /// <param name="sinTable">
        /// Таблица предвычисленных значений синуса для каждого угла развёртки.
        /// Размер соответствует <paramref name="width"/>.
        /// </param>
        /// <param name="cosTable">
        /// Таблица предвычисленных значений косинуса для каждого угла развёртки.
        /// Размер соответствует <paramref name="width"/>.
        /// </param>
        /// <param name="center">
        /// Центр коронки в координатах исходного изображения.
        /// </param>
        /// <param name="radius">
        /// Внешний радиус коронки, относительно которого выполняется развёртка.
        /// </param>
        /// <param name="w">
        /// Ширина исходного изображения в пикселях.
        /// Используется для билинейной интерполяции.
        /// </param>
        /// <param name="h">
        /// Высота исходного изображения в пикселях.
        /// Используется для билинейной интерполяции.
        /// </param>
        /// <param name="width">
        /// Ширина результирующего изображения (количество угловых шагов).
        /// Обычно соответствует длине окружности коронки.
        /// </param>
        /// <param name="height">
        /// Высота результирующего изображения (толщина коронки в пикселях).
        /// Определяет диапазон радиусов для развёртки.
        /// </param>
        /// 
        private static void GetStripeImg(Mat src, Mat dst, double[] sinTable, double[] cosTable,
                                         Point center, int radius, int w, int h, int width, int height)
        {
            const int INNER_OFFSET = 7;
            int hr = radius - height;
            for (int j = 0; j < height; j++)
            {
                int r = hr + j + INNER_OFFSET;
                for (int i = 0; i < width; i++)
                {
                    double x = sinTable[i] * r + center.X;
                    double y = cosTable[i] * r + center.Y;
                    dst.Set<byte>(j, i, Bilinear8Bit(src, x, y, w, h));
                }
            }
        }

        /// <summary>
        /// Выполняет билинейную интерполяцию яркости пикселя в одноканальном 8-битном изображении
        /// по дробным координатам. Используется для получения сглаженного значения при геометрических
        /// преобразованиях (например, полярной развёртке).
        /// </summary>
        /// <param name="img">Исходное одноканальное 8-битное изображение.</param>
        /// <param name="x">Дробная X-координата точки.</param>
        /// <param name="y">Дробная Y-координата точки.</param>
        /// <param name="w">Ширина изображения.</param>
        /// <param name="h">Высота изображения.</param>
        /// <returns>
        /// Интерполированное значение яркости. Возвращает 0, если координаты выходят за границы изображения.
        /// </returns>
        private static byte Bilinear8Bit(Mat img, double x, double y, int w, int h)
        {
            int u = (int)x;
            int v = (int)y;

            if (u < 0 || v < 0 || u >= w - 1 || v >= h - 1)
                return 0;

            double dx = x - u;
            double dy = y - v;

            byte p1 = img.At<byte>(v, u);
            byte p2 = img.At<byte>(v, u + 1);
            byte p3 = img.At<byte>(v + 1, u);
            byte p4 = img.At<byte>(v + 1, u + 1);

            double interpolated = (1 - dx) * (1 - dy) * p1 +
                                  dx * (1 - dy) * p2 +
                                  (1 - dx) * dy * p3 +
                                  dx * dy * p4;

            return (byte)Math.Round(interpolated);
        }

        /// <summary>
        /// Выполняет спектральный анализ сигнала методом быстрого преобразования Фурье (FFT)
        /// для обнаружения дефекта недолива коронки.
        /// Анализирует положение максимальной гармоники относительно ожидаемой частоты.
        /// Если максимум смещён от ожидаемой гармоники — фиксируется дефект.
        /// </summary>
        /// <param name="signal">Входной сигнал (профиль яркости развёрнутой коронки).</param>
        /// <param name="length">Количество отсчётов сигнала для анализа.</param>
        /// <param name="expectedHarmonic">Ожидаемый номер гармоники, соответствующий нормальной коронке.</param>
        /// <returns>
        /// true — обнаружен дефект недолива;
        /// false — дефект не обнаружен или анализ невозможен.
        /// </returns>
        private bool AnalyzeUnderfillFFT(float[] signal, int length, double expectedHarmonic)
        {
            try
            {
                if (signal == null || signal.Length < length)
                    return false;

                // Копируем сигнал для анализа
                float[] data = new float[length];
                Array.Copy(signal, data, length);

                // Центрируем сигнал (вычитаем среднее)
                double mean = 0;
                for (int i = 0; i < length; i++)
                    mean += data[i];
                mean /= length;

                for (int i = 0; i < length; i++)
                    data[i] -= (float)mean;

                // Преобразуем в комплексные числа для FFT
                System.Numerics.Complex[] complexData = new System.Numerics.Complex[length];
                for (int i = 0; i < length; i++)
                    complexData[i] = new System.Numerics.Complex(data[i], 0);

                // Выполняем FFT
                MathNet.Numerics.IntegralTransforms.Fourier.Forward(complexData);

                // Анализируем гармоники в окрестности ожидаемой
                int minHarmonic = (int)Math.Max(1, expectedHarmonic - 2);
                int maxHarmonic = (int)Math.Min(length / 2, expectedHarmonic + 2);

                double maxMagnitude = 0;
                int maxIndex = minHarmonic;

                for (int i = minHarmonic; i <= maxHarmonic; i++)
                {
                    double magnitude = complexData[i].Magnitude;
                    if (magnitude > maxMagnitude)
                    {
                        maxMagnitude = magnitude;
                        maxIndex = i;
                    }
                }

                bool result;

                if (maxIndex >= expectedHarmonic - 1 && maxIndex <= expectedHarmonic + 1)
                {
                    result = false;
                }
                else
                {
                    result = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "AnalyzeUnderfillFFT - ошибка при Фурье-анализе");
                return false;
            }
        }
    }
}