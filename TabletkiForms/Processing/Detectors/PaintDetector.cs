using KrishkiForms.Logger;
using KrishkiForms.Processing;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Point = OpenCvSharp.Point;

namespace KrishkiForms.Processing.Detectors
{
    internal class PaintDefectDetector : ICapDetector
    {
        private readonly double _minInpaintWhiteThreshold;
        private readonly double _minAreaInpaintDefect;

        public PaintDefectDetector(ProcessingParameters param)
        {
            _minInpaintWhiteThreshold = param.MinInpaintWhiteThreshold;
            _minAreaInpaintDefect = param.MinAreaInpaintDefect;
        }

        /// <summary>
        /// Проверка непрокраса на крышке. 
        /// Ищет области внутри контура крышки, которые по цвету близки к белому и имеют площадь больше порога. 
        /// Результат отрисовывается на drawFrame, если он передан. 
        /// Дефекты выделяются красными контурами и желтыми рамками. 
        /// Возвращает true, если найден хотя бы один дефект. 
        /// В случае ошибок в процессе обработки логирует их и возвращает false.
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
                {
                    ErrorLogger.Log(new Exception("Контур крышки пустой или содержит недостаточно точек"),
                        "PaintDefectDetector.Detect - проверка контура");
                    return false;
                }

                using Mat hsv = new Mat();
                try
                {
                    Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "PaintDefectDetector.Detect - ошибка при преобразовании в HSV");
                    return false;
                }

                token.ThrowIfCancellationRequested();

                using Mat capMask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
                try
                {
                    Cv2.FillPoly(capMask, new[] { capContour }, Scalar.White);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "PaintDefectDetector.Detect - ошибка при создании маски крышки");
                    return false;
                }

                token.ThrowIfCancellationRequested();

                using Mat maskedHSV = new Mat();
                try
                {
                    Cv2.BitwiseAnd(hsv, hsv, maskedHSV, capMask);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "PaintDefectDetector.Detect - ошибка при применении маски к HSV");
                    return false;
                }

                token.ThrowIfCancellationRequested();

                using Mat whiteMask = new Mat();
                try
                {
                    Cv2.InRange(maskedHSV,
                                new Scalar(0, 255 * 0.05, 255 * 0.05),
                                new Scalar(180, 255 * 0.95, 255 * 0.95),
                                whiteMask);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "PaintDefectDetector.Detect - ошибка при создании белой маски");
                    return false;
                }

                token.ThrowIfCancellationRequested();

                using Mat defectsMask = new Mat();
                Cv2.BitwiseNot(whiteMask, defectsMask);

                using Mat maskedDefects = new Mat();
                Cv2.BitwiseAnd(defectsMask, capMask, maskedDefects);

                token.ThrowIfCancellationRequested();

                Cv2.FindContours(maskedDefects, out Point[][] contours, out _,
                                 RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                var significantContours = contours
                    .Where(c => Cv2.ContourArea(c) > _minAreaInpaintDefect)
                    .ToList();

                token.ThrowIfCancellationRequested();

                var whiteDefects = new List<Point[]>();

                foreach (var contour in significantContours)
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        using Mat contourMask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
                        Cv2.FillPoly(contourMask, new[] { contour }, Scalar.White);

                        using Mat maskedImage = new Mat();
                        Cv2.BitwiseAnd(image, image, maskedImage, contourMask);

                        Scalar meanColor = Cv2.Mean(maskedImage, contourMask);

                        if (Math.Abs(meanColor.Val2 - 255) < _minInpaintWhiteThreshold)
                        {
                            whiteDefects.Add(contour);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "PaintDefectDetector.Detect - ошибка при обработке контура дефекта");
                    }
                }

                if (whiteDefects.Count > 0 && drawFrame != null && !drawFrame.Empty())
                {
                    try
                    {
                        Cv2.DrawContours(drawFrame, whiteDefects, -1, new Scalar(0, 0, 255), 2);
                        foreach (var contour in whiteDefects)
                        {
                            Rect bbox = Cv2.BoundingRect(contour);
                            Cv2.Rectangle(drawFrame, bbox, new Scalar(0, 255, 255), 2);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "PaintDefectDetector.Detect - ошибка при рисовании контуров дефектов");
                    }
                }

                return whiteDefects.Count > 0;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "PaintDefectDetector.Detect - непредвиденная ошибка");
                return false;
            }
        }
    }
}