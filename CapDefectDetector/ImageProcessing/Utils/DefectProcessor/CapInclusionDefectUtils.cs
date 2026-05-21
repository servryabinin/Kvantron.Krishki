using System;
using System.Threading;
using CapDefectDetector.DTO.DefectSettings;
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

        public CapInclusionUtils(InclusionDefectSettings settings)
        {
            SetCoefCapRadiusInclusion(settings.CoefCapRadiusInclusion);

            SetAdaptiveType(settings.AdaptiveType);
            SetThresholdType(settings.ThresholdType);
            SetBlockSize(settings.BlockSize);
            SetC(settings.C);

            SetMorphShape(settings.MorphShape);
            SetMorphType(settings.MorphType);
            SetKernelSize(settings.KernelSize);
            SetMorphIterations(settings.MorphIterations);

            SetMinAreaInclusion(settings.MinAreaInclusion);
            SetMaxAreaInclusion(settings.MaxAreaInclusion);
            SetInclusionThreshold(settings.InclusionThreshold);
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

        public InclusionDefectSettings GetSettings()
        {
            return new InclusionDefectSettings
            {
                CoefCapRadiusInclusion = _coefCapRadiusInclusion,
                AdaptiveType = _adaptiveType,
                ThresholdType = _thresholdType,
                BlockSize = _blockSize,
                C = _c,
                MorphShape = _morphShape,
                MorphType = _morphType,
                KernelSize = _kernelSize,
                MorphIterations = _morphIterations,
                MinAreaInclusion = _minAreaInclusion,
                MaxAreaInclusion = _maxAreaInclusion,
                InclusionThreshold = _inclusionThreshold
            };
        }

        public bool CheckForInclusions(
            Mat gray,
            Mat image,
            Mat drawFrame,
            CancellationToken token,
            Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length < 5)
                    return false;

                token.ThrowIfCancellationRequested();

                Mat step1 = DrawSearchArea(drawFrame, capContour);

                Mat step2 = AdaptiveBinarizeStep(gray, capContour);

                Mat step3 = MorphologyStep(step2);

                bool found = DetectInclusionsStep(step3, drawFrame, token);

                return found;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CheckForInclusions - error");
                return false;
            }
        }

        public Mat DrawSearchArea(Mat drawFrame, Point[] capContour)
        {
            RotatedRect ellipse = Cv2.FitEllipse(capContour);

            Point2f center = ellipse.Center;
            float radius = ComputeRadius(ellipse);

            Cv2.Circle(drawFrame, (Point)center, (int)radius, new Scalar(255, 0, 0), 2);

            return drawFrame;
        }

        public Mat AdaptiveBinarizeStep(Mat gray, Point[] capContour)
        {
            RotatedRect ellipse = Cv2.FitEllipse(capContour);

            Mat mask = CreateMask(gray.Size(), ellipse.Center, ComputeRadius(ellipse));

            Mat cropped = new Mat();
            gray.CopyTo(cropped, mask);

            Mat binary = new Mat();

            Cv2.AdaptiveThreshold(cropped,binary,255,_adaptiveType,_thresholdType,_blockSize,_c);

            return binary;
        }

        public Mat MorphologyStep(Mat binary)
        {
            Mat result = new Mat();

            using (var kernel = Cv2.GetStructuringElement(_morphShape,new Size(_kernelSize, _kernelSize)))
            {
                Cv2.MorphologyEx(binary,result,_morphType,kernel,iterations: _morphIterations);
            }

            return result;
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

        public bool DetectInclusionsStep(Mat filteredBinary, Mat drawFrame, CancellationToken token)
        {
            Cv2.FindContours(
                filteredBinary,
                out Point[][] contours,
                out _,
                RetrievalModes.List,
                ContourApproximationModes.ApproxSimple);

            bool found = false;

            foreach (var contour in contours)
            {
                token.ThrowIfCancellationRequested();

                double area = Cv2.ContourArea(contour);

                if (area > _minAreaInclusion &&
                    area < _maxAreaInclusion &&
                    IsCircularContour(contour))
                {
                    Rect bbox = Cv2.BoundingRect(contour);
                    Cv2.Rectangle(drawFrame, bbox.TopLeft, bbox.BottomRight, new Scalar(0, 0, 255), 2);

                    found = true;
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