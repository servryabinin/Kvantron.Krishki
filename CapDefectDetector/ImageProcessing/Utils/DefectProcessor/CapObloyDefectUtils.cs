using System;
using CapDefectDetector.DTO.DefectSettings;
using CapDefectDetector.Logger;
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace CapDefectDetector.ImageProcessing.Utils
{
    public class CapObloyDefectUtils
    {
        private double _capFlashOffset;
        private int _morphIterations;
        private MorphShapes _morphShape;
        private MorphTypes _morphType;
        private int _kernelSize;
        private double _minAreaObloy;

        private Mat _elementMask;

        public CapObloyDefectUtils(ObloyDefectSettings settings)
        {
            SetCapFlashOffset(settings.CapFlashOffset);
            SetKernelSize(settings.KernelSize);
            SetMorphShape(settings.MorphShape);
            SetMorphType(settings.MorphType);
            SetMorphIterations(settings.MorphIterations);
            SetMinAreaObloy(settings.MinAreaObloy);

            InitKernel();
        }

        public void SetCapFlashOffset(double value) => _capFlashOffset = value;

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
                //token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length < 5)
                    return false;

                if (blurChannel1 == null || blurChannel2 == null)
                    return false;

                var center = ComputeCenter(capContour);
                var radius = ComputeRadius(capContour, center);

                CreateCapMask(capRadiusMask, image, center, radius);

                Mat andResult = ApplyCapMask(capRadiusMask, blurChannel2);

                Mat morphInput = ApplyMorphology(andResult);

                var (contours, pixCount) = Analyze(morphInput);

                bool isDefect = isObloyExist(pixCount);

                DrawIfNeeded(isDefect, drawFrame, contours);

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

        public (Point[][] contours, int pixCount) Analyze(Mat input)
        {
            Cv2.FindContours(
                input,
                out Point[][] contours,
                out _,
                RetrievalModes.External,
                ContourApproximationModes.ApproxNone);

            int pixCount = Cv2.CountNonZero(input);

            return (contours, pixCount);
        }

        public bool isObloyExist (int pixCount)
        {
            return pixCount > _minAreaObloy;
        }
                    
        public void DrawIfNeeded(bool isDefect, Mat drawFrame, Point[][] contours)
        {
            if (!isDefect) return;

            Cv2.DrawContours(drawFrame, contours, -1, new Scalar(0, 0, 255), 2);
        }

        public Point ComputeCenter(Point[] capContour)
        {
            double x = 0, y = 0;

            foreach (var pt in capContour)
            {
                x += pt.X;
                y += pt.Y;
            }

            return new Point(
                (int)(x / capContour.Length),
                (int)(y / capContour.Length));
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
            GetIdealCapMask(
                capRadiusMask,
                center,
                (float)(radius + 3f),
                (float)(radius + 3f + _capFlashOffset));
        }

        public void GetIdealCapMask(Mat mask, Point center, float innerRadius, float outerRadius)
        {
            mask.SetTo(0);

            using var outer = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.Black);
            using var inner = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.Black);

            Cv2.Circle(outer, center, (int)outerRadius, Scalar.White, -1);
            Cv2.Circle(inner, center, (int)innerRadius, Scalar.White, -1);

            Cv2.Subtract(outer, inner, mask);
        }

        public void InitKernel()
        {
            _elementMask = Cv2.GetStructuringElement(
                _morphShape,
                new Size(_kernelSize, _kernelSize),
                new Point(_kernelSize / 2, _kernelSize / 2));
        }
    }
}