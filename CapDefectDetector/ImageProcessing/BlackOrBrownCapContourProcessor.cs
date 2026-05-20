using OpenCvSharp;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace CapDefectDetector.ImageProcessing.CapContours
{
    public class BlackOrBrownCapContourProcessor : CapContourProcessorBase
    {
        private readonly int _saturation;
        private readonly int _medianFilter;
        private readonly int _cannyThreshold;

        public BlackOrBrownCapContourProcessor(int saturation, int medianFilter, int cannyThreshold)
        {
            _saturation = saturation;
            _medianFilter = medianFilter;
            _cannyThreshold = cannyThreshold;
        }

        public override Point[] GetContour(Mat gray, Mat image)
        {
            if (gray.Empty() || image.Empty())
                return null;

            Mat sat = ApplySaturationStep(image);

            Mat grayImg = ApplyGrayStep(sat);

            Mat blurred = ApplyMedianStep(grayImg);

            Mat edges = ApplyCannyStep(blurred);

            return ApplyEllipseStep(edges);
        }

        private Mat ApplySaturationStep(Mat input)
        {
            return SimulateCameraSaturation(input, _saturation);
        }

        private Mat ApplyGrayStep(Mat input)
        {
            Mat gray = new Mat();
            Cv2.CvtColor(input, gray, ColorConversionCodes.BGR2GRAY);
            return gray;
        }

        private Mat ApplyMedianStep(Mat input)
        {
            Mat result = input.Clone();
            Cv2.MedianBlur(result, result, _medianFilter);
            return result;
        }

        private Mat ApplyCannyStep(Mat input)
        {
            Mat result = input.Clone();
            Cv2.Canny(result, result, _cannyThreshold, 255);
            return result;
        }

        private Point[] ApplyEllipseStep(Mat image)
        {
            Cv2.FindContours(image,out Point[][] contours,out _,RetrievalModes.List,ContourApproximationModes.ApproxSimple);

            var allPoints = new List<Point>();

            foreach (var cnt in contours)
                allPoints.AddRange(cnt);

            if (allPoints.Count < 5)
                return null;

            Point[] hull = Cv2.ConvexHull(allPoints.ToArray());

            if (hull.Length < 5)
                return hull;

            RotatedRect ellipse = Cv2.FitEllipse(hull);

            return Cv2.Ellipse2Poly((Point)ellipse.Center,new Size((int)(ellipse.Size.Width / 2), (int)(ellipse.Size.Height / 2)),(int)ellipse.Angle,0,360,1);
        }

        protected override Mat SimulateCameraSaturation(Mat img, int saturation)
        {
            if (img.Empty())
                return null;

            float koeff = saturation / 128.0f;

            Mat imgHSV = new Mat();

            Cv2.CvtColor(img, imgHSV, ColorConversionCodes.BGR2HSV);

            Mat[] hsv = Cv2.Split(imgHSV);

            Mat h = hsv[0];
            Mat s = hsv[1];
            Mat v = hsv[2];

            unsafe
            {
                byte* satPtr = (byte*)s.DataPointer;
                int total = s.Rows * s.Cols;

                for (int i = 0; i < total; i++)
                {
                    float corrected = koeff * satPtr[i];
                    if (corrected > 255f)
                        corrected = 255f;
                    satPtr[i] = (byte)corrected;
                }
            }

            Cv2.Merge(new Mat[] { h, s, v }, imgHSV);

            Mat imgSat = new Mat();
            Cv2.CvtColor(imgHSV, imgSat, ColorConversionCodes.HSV2BGR);

            return imgSat;
        }

        protected override Point[] CorrectContour(Point[] contour, float threshold)
        {
            if (contour == null || contour.Length < 5)
                return contour;

            RotatedRect ellipse = Cv2.FitEllipse(contour);

            Point2f center = ellipse.Center;
            float a = ellipse.Size.Width / 2f;
            float b = ellipse.Size.Height / 2f;

            List<Point> fixedContour = new(contour.Length);

            foreach (var p in contour)
            {
                float dx = p.X - center.X;
                float dy = p.Y - center.Y;

                float norm = (dx * dx) / (a * a) + (dy * dy) / (b * b);

                if (norm > threshold)
                {
                    float scale = 1.0f / (float)Math.Sqrt(norm);

                    int nx = (int)(center.X + dx * scale);
                    int ny = (int)(center.Y + dy * scale);

                    fixedContour.Add(new Point(nx, ny));
                }
                else
                {
                    fixedContour.Add(p);
                }
            }

            return fixedContour.ToArray();
        }
    }
}