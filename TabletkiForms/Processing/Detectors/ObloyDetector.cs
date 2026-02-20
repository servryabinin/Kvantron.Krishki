using System;
using System.Threading;
using CapDefectDetector.Logger;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.Processing.Detectors
{
    internal class ObloyDefectDetector : ICapDetector
    {
        private Mat _capRadiusMask;
        private const double CAP_FLASH_OFFSET = 5.0;

        private readonly double _minAreaObloy;
        private readonly Mat _maskElement;

        public ObloyDefectDetector(ProcessingParameters param)
        {
            _minAreaObloy = param.MinAreaObloy;
            _maskElement = param.ElementMask;
        }

        /// <summary>
        /// Проверка облоев на крышке. 
        /// Метод использует маску, основанную на контуре крышки, для выделения области проверки. 
        /// Затем применяется морфологическая операция эрозии для удаления шумов, и выполняется поиск контуров в полученной маске. 
        /// Если количество ненулевых пикселей в маске превышает заданный порог, считается, что облой найден, и контуры дефекта отрисовываются на результирующем изображении. 
        /// В случае ошибок при обработке или если контур крышки некорректный, ошибки логируются, и метод возвращает false.
        /// </summary>
        /// <param name="gray">Грейскейл изображение</param>
        /// <param name="image">Исходное изображение</param>
        /// <param name="drawFrame">Кадр для отрисовки результата</param>
        /// <param name="capContour">Контур крышки</param>
        /// <param name="blurChannel1">Один из каналов изображения крышек hsv</param>
        /// <param name="blurChannel2">Один из каналов изображения крышек hsv</param>
        /// <param name="param">Параметры рецепта</param>
        /// <param name="token">Токен отмены</param>
        /// <returns>true если дефект найден</returns>
        public bool Detect(
            Mat gray, 
            Mat image, 
            Mat drawFrame, 
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
                { 
                    ErrorLogger.Log(new Exception("Контур крышки пустой или содержит слишком мало точек"),
                        "CheckForObloyDefects - проверка контура"); 
                    return false; 
                }

                if (blurChannel1 == null || blurChannel2 == null) 
                { 
                    ErrorLogger.Log(new Exception("Каналы blurChannel_1 или blurChannel_2 не инициализированы"),
                        "CheckForObloyDefects - проверка инициализации каналов"); 
                    return false; 
                }

                if (_maskElement == null) 
                { 
                    ErrorLogger.Log(new Exception("ElementMask NULL"), 
                        "CheckForObloyDefects - проверка ElementMask"); 
                    return false; 
                }

                double sumX = 0, sumY = 0;
                foreach (var pt in capContour) 
                { 
                    sumX += pt.X; 
                    sumY += pt.Y; 
                }
                var capCenter = new Point((int)(sumX / capContour.Length), (int)(sumY / capContour.Length));

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
                    ErrorLogger.Log(ex, "CheckForObloyDefects - ошибка при создании маски крышки");
                    return false;
                }

                try
                {
                    Cv2.BitwiseAnd(_capRadiusMask, blurChannel2, blurChannel2);
                    Cv2.MorphologyEx(blurChannel2, blurChannel1, MorphTypes.Erode, _maskElement);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForObloyDefects - ошибка при применении морфологии");
                    return false;
                }

                Point[][] obloyContours;
                HierarchyIndex[] hierarchyObloy;
                try
                {
                    Cv2.FindContours(blurChannel1, out obloyContours, out hierarchyObloy,
                                     RetrievalModes.External, ContourApproximationModes.ApproxNone);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForObloyDefects - ошибка при поиске контуров");
                    return false;
                }

                int pixCount;
                try
                {
                    pixCount = Cv2.CountNonZero(blurChannel1);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForObloyDefects - ошибка при подсчете ненулевых пикселей");
                    return false;
                }

                bool obloyFound = false;

                if (pixCount > _minAreaObloy)
                {
                    obloyFound = true;
                    try
                    {
                        Cv2.DrawContours(drawFrame, obloyContours, -1, new Scalar(0, 0, 255), 2);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "CheckForObloyDefects - ошибка при рисовании контуров дефектов");
                    }
                }

                return obloyFound;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex) { ErrorLogger.Log(ex, "CheckForObloyDefects - непредвиденная ошибка"); return false; }
        }

        /// <summary>
        /// Создаёт кольцевую бинарную маску коронки крышки.
        /// Маска представляет собой область между внутренним и внешним радиусами,
        /// используемую для поиска дефектов на кромке (например, облоя).
        /// </summary>
        /// <param name="mask">Выходная бинарная маска, в которую записывается результат.</param>
        /// <param name="center">Центр коронки на изображении.</param>
        /// <param name="innerRadius">Внутренний радиус кольца.</param>
        /// <param name="outerRadius">Внешний радиус кольца.</param>
        private void GetIdealCapMask(Mat mask, Point center, float innerRadius, float outerRadius)
        {
            mask.SetTo(0);

            using var outer = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.Black);
            using var inner = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.Black);

            Cv2.Circle(outer, center, (int)outerRadius, Scalar.White, -1);
            Cv2.Circle(inner, center, (int)innerRadius, Scalar.White, -1);
            Cv2.Subtract(outer, inner, mask);
        }
    }
}