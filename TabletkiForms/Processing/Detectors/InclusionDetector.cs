using OpenCvSharp;
using System;
using System.Threading;
using KrishkiForms.Processing;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size; // для ProcessingParameters

namespace KrishkiForms.Processing.Detectors
{
    internal class InclusionDetector : ICapDetector
    {
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
        public bool Detect(
            Mat gray,
            Mat image,
            Mat drawFrame,
            Point[] contour,
            ProcessingParameters param,
            CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (contour == null || contour.Length < 5)
                    return false;

                token.ThrowIfCancellationRequested();

                // Подгоняем эллипс под контур крышки
                RotatedRect ellipse;
                try
                {
                    ellipse = Cv2.FitEllipse(contour);
                }
                catch
                {
                    return false;
                }

                Point2f center = ellipse.Center;
                float radius = (float)(param.CoefCapRadiusInclusion * (ellipse.Size.Width + ellipse.Size.Height) / 4.0);

                if (drawFrame != null && !drawFrame.Empty())
                {
                    try
                    {
                        Cv2.Circle(drawFrame, (Point)center, (int)radius, new Scalar(255, 0, 0), 2);
                    }
                    catch
                    {
                        // Игнорируем ошибки рисования
                    }
                }

                // Создаем маску для области эллипса
                using Mat mask = Mat.Zeros(gray.Size(), MatType.CV_8UC1);
                Cv2.Circle(mask, (Point)center, (int)radius, Scalar.White, -1);

                using Mat croppedRegion = new Mat();
                gray.CopyTo(croppedRegion, mask);

                // Бинаризация и морфология
                using Mat binary = new Mat();
                Cv2.AdaptiveThreshold(croppedRegion, binary, 255,
                                      AdaptiveThresholdTypes.MeanC,
                                      ThresholdTypes.BinaryInv, 11, 2);

                using Mat filtered = new Mat();
                Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
                Cv2.MorphologyEx(binary, filtered, MorphTypes.Open, kernel, iterations: 1);

                // Поиск контуров включений
                Point[][] inclusionContours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(filtered, out inclusionContours, out hierarchy,
                                 RetrievalModes.List, ContourApproximationModes.ApproxSimple);

                token.ThrowIfCancellationRequested();

                bool inclusionsFound = false;

                foreach (var c in inclusionContours)
                {
                    token.ThrowIfCancellationRequested();

                    double area = Cv2.ContourArea(c);
                    if (area > param.MinAreaInclusion &&
                        area < param.MaxAreaInclusion &&
                        IsCircularContour(c, param.InclusionThreshold))
                    {
                        try
                        {
                            Rect bbox = Cv2.BoundingRect(c);
                            Cv2.Rectangle(drawFrame, bbox.TopLeft, bbox.BottomRight, new Scalar(0, 0, 255), 2);
                            inclusionsFound = true;
                        }
                        catch
                        {
                            // Игнорируем ошибки рисования
                        }
                    }
                }

                return inclusionsFound;
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

        /// <summary>
        /// Проверка, является ли контур приблизительно круглым
        /// </summary>
        private static bool IsCircularContour(Point[] contour, double threshold)
        {
            double perimeter = Cv2.ArcLength(contour, true);
            double area = Cv2.ContourArea(contour);
            double circularity = (4 * Math.PI * area) / (perimeter * perimeter);
            return circularity > threshold;
        }
    }
}