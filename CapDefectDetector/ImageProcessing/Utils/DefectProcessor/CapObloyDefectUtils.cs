using System;
using CapDefectDetector.DTO.DefectSettings;
using CapDefectDetector.Logger;
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace CapDefectDetector.ImageProcessing.Utils
{
    public class CapObloyDefectUtils : IDisposable
    {
        private double _capFlashOffset;
        private double _noiseContourArea;
        private int _morphIterations;
        private MorphShapes _morphShape;
        private MorphTypes _morphType;
        private int _kernelSize;
        private double _minAreaObloy;

        private Mat _elementMask;

        public CapObloyDefectUtils(ObloyDefectSettings settings)
        {
            SetCapFlashOffset(settings.CapFlashOffset);
            SetNoiseContourArea(settings.NoiseContourArea);
            SetKernelSize(settings.KernelSize);
            SetMorphShape(settings.MorphShape);
            SetMorphType(settings.MorphType);
            SetMorphIterations(settings.MorphIterations);
            SetMinAreaObloy(settings.MinAreaObloy);

            InitKernel();
        }

        public void SetCapFlashOffset(double value) => _capFlashOffset = value;

        public void SetNoiseContourArea(double value) => _noiseContourArea = value;

        public void SetMorphShape(MorphShapes value)
        {
            _morphShape = value;
            InitKernel();
        }

        public void SetMorphType(MorphTypes value) => _morphType = value;

        public void SetKernelSize(int value)
        {
            _kernelSize = value;
            InitKernel();
        }

        public void SetMorphIterations(int value) => _morphIterations = value;

        public void SetMinAreaObloy(double value) => _minAreaObloy = value;

        public ObloyDefectSettings GetSettings()
        {
            return new ObloyDefectSettings
            {
                CapFlashOffset = _capFlashOffset,
                NoiseContourArea = _noiseContourArea,
                MorphShape = _morphShape,
                MorphType = _morphType,
                KernelSize = _kernelSize,
                MorphIterations = _morphIterations,
                MinAreaObloy = _minAreaObloy
            };
        }

        public bool CheckForObloyDefects(
            Mat gray,
            Mat image,
            Mat drawFrame,
            CancellationToken token,
            Point[] capContour,
            Mat blurChannel1,
            Mat blurChannel2,
            Mat capRadiusMask)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length < 5)
                    return false;

                if (blurChannel1 == null || blurChannel2 == null)
                    return false;

                var center = ComputeCenter(capContour);
                var radius = ComputeRadius(capContour, center);

                if (radius <= 0) return false;

                CreateCapMask(capRadiusMask, image, center, radius);

                using Mat filteredBlur = FilterNoise(blurChannel2);

                using Mat andResult = ApplyCapMask(capRadiusMask, filteredBlur);

                using Mat morphInput = ApplyMorphology(andResult);

                List<Point[]> validContours = Analyze(morphInput);

                bool isDefect = isObloyExist(validContours);

                DrawIfNeeded(isDefect, drawFrame, validContours);

                return isDefect;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CheckForObloyDefects");
                return false;
            }
        }

        public Mat ApplyCapMask(Mat capRadiusMask, Mat blur2)
        {
            Mat resizedMask = new Mat();

            if (capRadiusMask.Size() != blur2.Size())
            {
                Cv2.Resize(capRadiusMask,resizedMask,blur2.Size(),0,0,InterpolationFlags.Nearest);
            }
            else
            {
                resizedMask = capRadiusMask.Clone();
            }

            if (resizedMask.Type() != blur2.Type())
            {
                resizedMask.ConvertTo(resizedMask, blur2.Type());
            }

            Mat result = new Mat();

            Cv2.BitwiseAnd(resizedMask, blur2, result);

            resizedMask.Dispose();

            return result;
        }

        public Mat ApplyMorphology(Mat andResult)
        {
            Mat morphed = new Mat();
            Cv2.MorphologyEx(andResult, morphed, _morphType, _elementMask);

            return morphed;
        }

        public List<Point[]> Analyze(Mat input)
        {
            Cv2.FindContours(
                input,
                out Point[][] contours,
                out _,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple);

            var validContours = new List<Point[]>();

            foreach (var c in contours)
            {
                double area = Cv2.ContourArea(c);

                if (area >= _minAreaObloy)
                {
                    validContours.Add(c);
                }
            }

            return validContours;
        }

        public bool isObloyExist(List<Point[]> validContours)
        {
            return validContours.Count > 0;
        }

        public void DrawIfNeeded(bool isDefect, Mat drawFrame, List<Point[]> validContours)
        {
            if (!isDefect) return;

            Cv2.DrawContours(drawFrame, validContours, -1, Scalar.Red, 2);
        }

        public Point ComputeCenter(Point[] contour)
        {
            var m = Cv2.Moments(contour);

            if (Math.Abs(m.M00) < 1e-6)
                return new Point(0, 0);

            double cx = m.M10 / m.M00;
            double cy = m.M01 / m.M00;

            if (double.IsNaN(cx) || double.IsNaN(cy) ||
                double.IsInfinity(cx) || double.IsInfinity(cy))
                return new Point(0, 0);

            return new Point((int)cx, (int)cy);
        }

        public double ComputeRadius(Point[] capContour, Point center)
        {
            double sum = 0;

            foreach (var pt in capContour)
            {
                double dx = pt.X - center.X;
                double dy = pt.Y - center.Y;
                sum += Math.Sqrt(dx * dx + dy * dy);
            }

            return sum / capContour.Length;
        }

        public void CreateCapMask(Mat capRadiusMask, Mat image, Point center, double radius)
        {
            if (capRadiusMask == null || capRadiusMask.Empty())
                return;

            if (radius <= 0 || double.IsNaN(radius) || double.IsInfinity(radius))
                return;

            GetIdealCapMask(
                capRadiusMask,
                center,
                (float)(radius + 3f),
                (float)(radius + 3f + _capFlashOffset));
        }

        public Mat FilterNoise(Mat blur2)
        {
            Mat filtered = Mat.Zeros(blur2.Size(), MatType.CV_8UC1);

            Cv2.FindContours(
                blur2,
                out Point[][] contours,
                out _,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple);

            foreach (var contour in contours)
            {
                if (Cv2.ContourArea(contour) < _noiseContourArea)
                    continue;

                Cv2.DrawContours(
                    filtered,
                    new[] { contour },
                    -1,
                    Scalar.White,
                    -1);
            }

            return filtered;
        }

        public void GetIdealCapMask(Mat mask, Point center, float innerRadius, float outerRadius)
        {
            if (mask == null || mask.Empty())
                return;

            if (innerRadius < 0) innerRadius = 0;
            if (outerRadius < 0) outerRadius = 0;

            if (outerRadius <= innerRadius)
                return;

            mask.SetTo(0);

            using var outer = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.Black);
            using var inner = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.Black);

            Cv2.Circle(outer, center, (int)outerRadius, Scalar.White, -1);
            Cv2.Circle(inner, center, (int)innerRadius, Scalar.White, -1);

            Cv2.Subtract(outer, inner, mask);
        }

        public void InitKernel()
        {
            _elementMask?.Dispose();

            _elementMask = Cv2.GetStructuringElement(
                _morphShape,
                new Size(_kernelSize, _kernelSize),
                new Point(_kernelSize / 2, _kernelSize / 2));
        }

        public void Dispose()
        {
            _elementMask?.Dispose();
        }
    }
}