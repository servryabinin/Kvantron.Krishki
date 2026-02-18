using System;
using System.Threading;
using KrishkiForms.Logger;
using KrishkiForms.Processing;
using OpenCvSharp;

using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size; // для ProcessingParameters

namespace KrishkiForms.Processing.Detectors
{
    internal class ObloyDefectDetector : ICapDetector
    {
        private Mat _capRadiusMask;
        private readonly Mat _blurChannel1;
        private readonly Mat _blurChannel2;
        private readonly Mat elementMask;
        private const double CAP_FLASH_OFFSET = 2.0;

        private readonly double _minAreaObloy;

        /// <summary>
        /// Проверка на наличие включений на крышке
        /// </summary>
        /// <param name="gray">Грейскейл изображение</param>
        /// <param name="image">Исходное изображение</param>
        /// <param name="drawFrame">Кадр для отрисовки результата</param>
        /// <param name="contour">Контур крышки</param>
        /// <param name="param">Параметры рецепта</param>
        /// <param name="token">Токен отмены</param>
        /// <returns>true если дефект найден</returns>
        public ObloyDefectDetector(Mat blurChannel1, Mat blurChannel2, Mat elementMask, double minAreaObloy)
        {
            _blurChannel1 = blurChannel1 ?? throw new ArgumentNullException(nameof(blurChannel1));
            _blurChannel2 = blurChannel2 ?? throw new ArgumentNullException(nameof(blurChannel2));
            this.elementMask = elementMask ?? throw new ArgumentNullException(nameof(elementMask));
            _minAreaObloy = minAreaObloy;
        }

        public bool Detect(Mat gray, Mat image, Mat drawFrame, Point[] capContour, ProcessingParameters param, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length < 5)
                {
                    ErrorLogger.Log(new Exception("Контур крышки пустой или содержит слишком мало точек"),
                        "ObloyDefectDetector - проверка контура");
                    return false;
                }

                if (_blurChannel1 == null || _blurChannel2 == null)
                {
                    ErrorLogger.Log(new Exception("Каналы blurChannel1 или blurChannel2 не инициализированы"),
                        "ObloyDefectDetector - проверка инициализации каналов");
                    return false;
                }

                // Вычисляем центр крышки
                Point capCenter;
                try
                {
                    double sumX = 0, sumY = 0;
                    foreach (var pt in capContour)
                    {
                        sumX += pt.X;
                        sumY += pt.Y;
                    }
                    capCenter = new Point((int)(sumX / capContour.Length), (int)(sumY / capContour.Length));
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "ObloyDefectDetector - ошибка при вычислении центра крышки");
                    return false;
                }

                // Радиус крышки
                double radius = 0;
                foreach (var pt in capContour)
                {
                    double dx = pt.X - capCenter.X;
                    double dy = pt.Y - capCenter.Y;
                    radius += Math.Sqrt(dx * dx + dy * dy);
                }
                radius /= capContour.Length;

                try
                {
                    if (_capRadiusMask == null || _capRadiusMask.Size() != image.Size())
                    {
                        _capRadiusMask?.Dispose();
                        _capRadiusMask = new Mat(image.Rows, image.Cols, MatType.CV_8UC1);
                    }

                    GetIdealCapMask(_capRadiusMask, capCenter,
                                    (float)(radius + 3.0f),
                                    (float)(radius + 3.0f + CAP_FLASH_OFFSET));
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "ObloyDefectDetector - ошибка при создании маски крышки");
                    return false;
                }

                try
                {
                    Cv2.BitwiseAnd(_capRadiusMask, _blurChannel2, _blurChannel2);
                    Cv2.MorphologyEx(_blurChannel2, _blurChannel1, MorphTypes.Erode, elementMask);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "ObloyDefectDetector - ошибка при применении морфологии");
                    return false;
                }

                Point[][] obloyContours;
                HierarchyIndex[] hierarchyObloy;
                try
                {
                    Cv2.FindContours(_blurChannel1, out obloyContours, out hierarchyObloy,
                                     RetrievalModes.External, ContourApproximationModes.ApproxNone);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "ObloyDefectDetector - ошибка при поиске контуров");
                    return false;
                }

                int pixCount;
                try
                {
                    pixCount = Cv2.CountNonZero(_blurChannel1);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "ObloyDefectDetector - ошибка при подсчете ненулевых пикселей");
                    return false;
                }

                if (pixCount > _minAreaObloy && drawFrame != null && !drawFrame.Empty())
                {
                    try
                    {
                        Cv2.DrawContours(drawFrame, obloyContours, -1, new Scalar(0, 0, 255), 2);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "ObloyDefectDetector - ошибка при рисовании контуров дефектов");
                    }
                }

                return pixCount > _minAreaObloy;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "ObloyDefectDetector - непредвиденная ошибка");
                return false;
            }
        }

        private void GetIdealCapMask(Mat mask, Point center, float innerRadius, float outerRadius)
        {
            mask.SetTo(0);
            using Mat outer = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.Black);
            using Mat inner = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.Black);

            Cv2.Circle(outer, center, (int)outerRadius, Scalar.White, -1);
            Cv2.Circle(inner, center, (int)innerRadius, Scalar.White, -1);
            Cv2.Subtract(outer, inner, mask);
        }
    }
}