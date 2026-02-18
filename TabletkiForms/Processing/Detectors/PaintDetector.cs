using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using KrishkiForms.Processing;
using Point = OpenCvSharp.Point;

namespace KrishkiForms.Processing.Detectors
{
    internal class PaintDefectDetector : ICapDetector
    {
        /// <summary>
        /// Проверка овальности крышки по контуру
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
            Mat drawFrame,
            Point[] capContour,
            ProcessingParameters param,
            CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length < 5)
                    return false;

                using Mat hsv = new Mat();
                Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);

                token.ThrowIfCancellationRequested();

                // Создаем маску крышки
                using Mat capMask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
                Cv2.FillPoly(capMask, new[] { capContour }, Scalar.White);

                token.ThrowIfCancellationRequested();

                using Mat maskedHSV = new Mat();
                Cv2.BitwiseAnd(hsv, hsv, maskedHSV, capMask);

                token.ThrowIfCancellationRequested();

                // Создаем маску белого цвета для дефектов
                using Mat whiteMask = new Mat();
                Cv2.InRange(maskedHSV,
                            new Scalar(0, 255 * 0.05, 255 * 0.05),
                            new Scalar(180, 255 * 0.95, 255 * 0.95),
                            whiteMask);

                token.ThrowIfCancellationRequested();

                using Mat defectsMask = new Mat();
                Cv2.BitwiseNot(whiteMask, defectsMask);

                using Mat maskedDefects = new Mat();
                Cv2.BitwiseAnd(defectsMask, capMask, maskedDefects);

                token.ThrowIfCancellationRequested();

                // Находим контуры возможных дефектов
                Cv2.FindContours(maskedDefects, out Point[][] contours, out _,
                                 RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                var significantContours = contours
                    .Where(c => Cv2.ContourArea(c) > param.MinAreaInpaintDefect)
                    .ToList();

                token.ThrowIfCancellationRequested();

                var whiteDefects = new List<Point[]>();

                foreach (var contour in significantContours)
                {
                    token.ThrowIfCancellationRequested();

                    using Mat contourMask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
                    Cv2.FillPoly(contourMask, new[] { contour }, Scalar.White);

                    using Mat maskedImage = new Mat();
                    Cv2.BitwiseAnd(image, image, maskedImage, contourMask);

                    Scalar meanColor = Cv2.Mean(maskedImage, contourMask);

                    // Проверка на белый дефект
                    if (Math.Abs(meanColor.Val2 - 255) < param.MinInpaintWhiteThreshold)
                    {
                        whiteDefects.Add(contour);
                    }
                }

                // Отрисовка дефектов
                if (whiteDefects.Count > 0 && drawFrame != null && !drawFrame.Empty())
                {
                    Cv2.DrawContours(drawFrame, whiteDefects, -1, new Scalar(0, 0, 255), 2);

                    foreach (var contour in whiteDefects)
                    {
                        Rect bbox = Cv2.BoundingRect(contour);
                        Cv2.Rectangle(drawFrame, bbox, new Scalar(0, 255, 255), 2);
                    }
                }

                return whiteDefects.Count > 0;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}