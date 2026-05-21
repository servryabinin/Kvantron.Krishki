using System;
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

        public CapObloyDefectUtils(
            double capFlashOffset,
            MorphShapes morphShape,MorphTypes morphType, int kernelSize, int morphIterations,
            double minAreaObloy)
        {
            _capFlashOffset = capFlashOffset;
            _morphShape = morphShape;
            _morphType = morphType;
            _kernelSize = kernelSize;
            _morphIterations = morphIterations;
            _minAreaObloy = minAreaObloy;

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
                {
                    ErrorLogger.Log(
                        new Exception("Контур крышки пустой или содержит слишком мало точек"),
                        "CheckForObloyDefects - проверка контура");
                    return false;
                }

                if (blurChannel1 == null || blurChannel2 == null)
                {
                    ErrorLogger.Log(
                        new Exception("Каналы blurChannel_1 или blurChannel_2 не инициализированы"),
                        "CheckForObloyDefects - проверка инициализации каналов");
                    return false;
                }

                token.ThrowIfCancellationRequested();

                var capCenter = ComputeCenter(capContour);
                var radius = ComputeRadius(capContour, capCenter);

                CreateCapMask(capRadiusMask, image, capCenter, radius);

                ApplyMorphology(blurChannel1, blurChannel2, capRadiusMask);

                var (contours, pixCount) = Analyze(blurChannel1);

                bool isDefect = CheckDefect(pixCount);

                DrawIfNeeded(isDefect, drawFrame, contours);

                return isDefect;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CheckForObloyDefects - непредвиденная ошибка");
                return false;
            }
        }

        private void InitKernel()
        {
            _elementMask = Cv2.GetStructuringElement(
                _morphShape,
                new Size(_kernelSize, _kernelSize),
                new Point(_kernelSize / 2, _kernelSize / 2));
        }

        private Point ComputeCenter(Point[] capContour)
        {
            double sumX = 0, sumY = 0;

            foreach (var pt in capContour)
            {
                sumX += pt.X;
                sumY += pt.Y;
            }

            return new Point((int)(sumX / capContour.Length), (int)(sumY / capContour.Length));
        }

        private double ComputeRadius(Point[] capContour, Point center)
        {
            double radius = 0;

            foreach (var pt in capContour)
            {
                double dx = pt.X - center.X;
                double dy = pt.Y - center.Y;
                radius += Math.Sqrt(dx * dx + dy * dy);
            }

            return radius / capContour.Length;
        }

        private void CreateCapMask(Mat capRadiusMask, Mat image, Point center, double radius)
        {
            try
            {
                GetIdealCapMask(
                    capRadiusMask,
                    center,
                    (float)(radius + 3.0f),
                    (float)(radius + 3.0f + _capFlashOffset));
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Obloy - создание маски");
                throw;
            }
        }

        private void ApplyMorphology(Mat blur1, Mat blur2, Mat capMask)
        {
            try
            {
                Cv2.BitwiseAnd(capMask, blur2, blur2);

                Cv2.MorphologyEx(blur2,blur1, _morphType,_elementMask);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Obloy - морфология");
                throw;
            }
        }

        private (Point[][] contours, int pixCount) Analyze(Mat input)
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

        private bool CheckDefect(int pixCount)
        {
            return pixCount > _minAreaObloy;
        }

        private void DrawIfNeeded(bool isDefect, Mat drawFrame, Point[][] contours)
        {
            if (!isDefect)
                return;

            try
            {
                Cv2.DrawContours(drawFrame, contours, -1, new Scalar(0, 0, 255), 2);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Obloy - draw");
            }
        }

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