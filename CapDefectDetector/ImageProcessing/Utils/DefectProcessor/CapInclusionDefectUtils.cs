using System;
using System.Threading;
using CapDefectDetector.Logger;
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace CapDefectDetector.ImageProcessing.Utils
{
    public class CapInclusionUtils
    {
        private double _coefCapRadiusInclusion;

        private AdaptiveThresholdTypes _adaptiveType;
        private ThresholdTypes _thresholdType;
        private int _blockSize;
        private double _c;

        private MorphShapes _morphShape;
        private MorphTypes _morphType;
        private int _kernelSize;
        private int _morphIterations;

        private double _minAreaInclusion;
        private double _maxAreaInclusion;
        private double _inclusionThreshold;

        public CapInclusionUtils(double coefCapRadiusInclusion,
                                 AdaptiveThresholdTypes adaptiveType, ThresholdTypes thresholdType, int blockSize, double c,
                                 MorphShapes morphShape, MorphTypes morphType, int kernelSize, int morphIterations,
                                 double minAreaInclusion, double maxAreaInclusion, double inclusionThreshold)
        {
            _coefCapRadiusInclusion = coefCapRadiusInclusion;

            _adaptiveType = adaptiveType;
            _thresholdType = thresholdType;
            _blockSize = blockSize;
            _c = c;

            _morphShape = morphShape;
            _morphType = morphType;
            _kernelSize = kernelSize;
            _morphIterations = morphIterations;

            _minAreaInclusion = minAreaInclusion;
            _maxAreaInclusion = maxAreaInclusion;
            _inclusionThreshold = inclusionThreshold;
        }

        public void SetCoefCapRadiusInclusion(double value) => _coefCapRadiusInclusion = value;
        public void SetAdaptiveType(AdaptiveThresholdTypes value) => _adaptiveType = value;
        public void SetThresholdType(ThresholdTypes value) => _thresholdType = value;
        public void SetBlockSize(int value) => _blockSize = value;
        public void SetC(double value) => _c = value;
        public void SetMorphShape(MorphShapes value) => _morphShape = value;
        public void SetMorphType(MorphTypes value) => _morphType = value;
        public void SetKernelSize(int value) => _kernelSize = value;
        public void SetMorphIterations(int value) => _morphIterations = value;
        public void SetMinAreaInclusion(double value) => _minAreaInclusion = value;
        public void SetMaxAreaInclusion(double value) => _maxAreaInclusion = value;
        public void SetInclusionThreshold(double value) => _inclusionThreshold = value;

        public bool CheckForInclusions(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length < 5)
                {
                    ErrorLogger.Log(
                        new Exception("Контур для проверки включений пустой или содержит недостаточно точек"),
                        "CheckForInclusions - проверка наличия контура"
                    );
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
                    ErrorLogger.Log(ex, "CheckForInclusions - ошибка при расчёте эллипса");
                    return false;
                }

                Point2f center = ellipse.Center;
                float radius = ComputeRadius(ellipse);

                try
                {
                    Cv2.Circle(drawFrame, (Point)center, (int)radius, new Scalar(255, 0, 0), 2);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForInclusions - ошибка при рисовании эллипса");
                }

                try
                {
                    using (Mat mask = CreateMask(gray.Size(), center, radius))
                    using (Mat cropped = new Mat())
                    {
                        gray.CopyTo(cropped, mask);

                        using (Mat binary = AdaptiveBinarize(cropped))
                        using (Mat filtered = ApplyMorphology(binary))
                        {
                            return ProcessContours(filtered, drawFrame, token);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForInclusions - ошибка при обработке изображения для поиска включений");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CheckForInclusions - непредвиденная ошибка");
                return false;
            }
        }

        private float ComputeRadius(RotatedRect ellipse)
        {
            return (float)(_coefCapRadiusInclusion * (ellipse.Size.Width + ellipse.Size.Height) / 4.0);
        }

        private Mat CreateMask(Size size, Point2f center, float radius)
        {
            Mat mask = Mat.Zeros(size, MatType.CV_8UC1);
            Cv2.Circle(mask, (Point)center, (int)radius, Scalar.White, -1);
            return mask;
        }

        private Mat AdaptiveBinarize(Mat croppedRegion)
        {
            Mat binary = new Mat();

            Cv2.AdaptiveThreshold(
                croppedRegion,
                binary,
                255,
                _adaptiveType,
                _thresholdType,
                _blockSize,
                _c
            );

            return binary;
        }

        private Mat ApplyMorphology(Mat binary)
        {
            Mat result = new Mat();

            using (var kernel = Cv2.GetStructuringElement(_morphShape, new Size(_kernelSize, _kernelSize)))
            {
                Cv2.MorphologyEx(
                    binary,
                    result,
                    _morphType,
                    kernel,
                    iterations: _morphIterations
                );
            }

            return result;
        }

        private bool ProcessContours(Mat filteredBinary, Mat drawFrame, CancellationToken token)
        {
            Cv2.FindContours(filteredBinary, out Point[][] contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            bool found = false;

            foreach (var contour in contours)
            {
                token.ThrowIfCancellationRequested();

                double area = Cv2.ContourArea(contour);

                if (area > _minAreaInclusion &&
                    area < _maxAreaInclusion &&
                    IsCircularContour(contour))
                {
                    try
                    {
                        Rect bbox = Cv2.BoundingRect(contour);
                        Cv2.Rectangle(drawFrame, bbox.TopLeft, bbox.BottomRight, new Scalar(0, 0, 255), 2);
                        found = true;
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "CheckForInclusions - ошибка при рисовании прямоугольника вокруг включения");
                    }
                }
            }

            return found;
        }

        private bool IsCircularContour(Point[] contour)
        {
            double perimeter = Cv2.ArcLength(contour, true);
            double area = Cv2.ContourArea(contour);

            if (perimeter <= 0) return false;

            double circularity = (4 * Math.PI * area) / (perimeter * perimeter);
            return circularity > _inclusionThreshold;
        }
    }
}