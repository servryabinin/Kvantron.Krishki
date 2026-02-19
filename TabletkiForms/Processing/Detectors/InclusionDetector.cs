using KrishkiForms.Logger;
using KrishkiForms.Processing;
using OpenCvSharp;
using System;
using System.Threading;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace KrishkiForms.Processing.Detectors
{
    internal class InclusionDetector : ICapDetector
    {
        private readonly double _minAreaInclusion;
        private readonly double _maxAreaInclusion;
        private readonly double _inclusionCircularityThreshold;
        private readonly double _coefCapRadiusInclusion;

        public InclusionDetector(ProcessingParameters param)
        {
            _minAreaInclusion = param.MinAreaInclusion;
            _maxAreaInclusion = param.MaxAreaInclusion;
            _inclusionCircularityThreshold = param.InclusionThreshold;
            _coefCapRadiusInclusion = param.CoefCapRadiusInclusion;
        }

        /// <summary>
        /// Проверка вкраплений внтури радиуса крышки, определённого через эллипс, аппроксимирующий контур крышки. 
        /// Вкрапления должны быть примерно круглой формы и попадать в заданный диапазон площади.
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
                    ErrorLogger.Log(new Exception("Контур для проверки включений пустой или содержит недостаточно точек"),
                        "InclusionDetector.Detect - проверка наличия контура");
                    return false;
                }

                token.ThrowIfCancellationRequested();

                RotatedRect ellipse;
                try
                {
                    ellipse = Cv2.FitEllipse(capContour);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "InclusionDetector.Detect - ошибка при расчёте эллипса");
                    return false;
                }

                Point2f center = ellipse.Center;
                float radius = (float)(_coefCapRadiusInclusion * (ellipse.Size.Width + ellipse.Size.Height) / 4.0);

                if (drawFrame != null && !drawFrame.Empty())
                {
                    try
                    {
                        Cv2.Circle(drawFrame, (Point)center, (int)radius, new Scalar(255, 0, 0), 2);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "InclusionDetector.Detect - ошибка при рисовании круга вокруг эллипса");
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
                Cv2.FindContours(filtered, out Point[][] inclusionContours, out _,
                                 RetrievalModes.List, ContourApproximationModes.ApproxSimple);

                token.ThrowIfCancellationRequested();

                bool inclusionsFound = false;

                foreach (var contour in inclusionContours)
                {
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        double area = Cv2.ContourArea(contour);
                        if (area > _minAreaInclusion &&
                            area < _maxAreaInclusion &&
                            IsCircularContour(contour, _inclusionCircularityThreshold))
                        {
                            if (drawFrame != null && !drawFrame.Empty())
                            {
                                try
                                {
                                    Rect bbox = Cv2.BoundingRect(contour);
                                    Cv2.Rectangle(drawFrame, bbox.TopLeft, bbox.BottomRight, new Scalar(0, 0, 255), 2);
                                }
                                catch (Exception ex)
                                {
                                    ErrorLogger.Log(ex, "InclusionDetector.Detect - ошибка при рисовании прямоугольника вокруг включения");
                                }
                            }
                            inclusionsFound = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "InclusionDetector.Detect - ошибка при обработке контура включения");
                    }
                }

                return inclusionsFound;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "InclusionDetector.Detect - непредвиденная ошибка");
                return false;
            }
        }

        /// <summary>
        /// Проверяет, является ли контур приблизительно круглым на основе коэффициента округлости (circularity).
        /// Circularity вычисляется по формуле: 4π × площадь / периметр².
        /// Значение, близкое к 1.0, соответствует идеальному кругу.
        /// </summary>
        /// <param name="contour">Контур для анализа.</param>
        /// <param name="threshold">
        /// Минимально допустимое значение circularity.
        /// Контур считается круглым, если circularity превышает данный порог.
        /// Типичные значения: 0.7–0.95.
        /// </param>
        /// <returns>
        /// true — контур близок к кругу;
        /// false — контур имеет вытянутую или неправильную форму.
        /// </returns>
        private static bool IsCircularContour(Point[] contour, double threshold)
        {
            double perimeter = Cv2.ArcLength(contour, true);
            double area = Cv2.ContourArea(contour);
            double circularity = (4 * Math.PI * area) / (perimeter * perimeter);
            return circularity > threshold;
        }
    }
}