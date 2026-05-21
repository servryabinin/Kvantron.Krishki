using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace CapDefectDetector.ImageProcessing.Utils.ContourProcessor
{
    public class CapBlackOrBrownContourUtils
    {
        public Mat ApplySaturationStep(Mat input, int saturation)
        {
            return SimulateCameraSaturation(input, saturation);
        }

        public Mat ApplyGrayStep(Mat input)
        {
            Mat gray = new Mat();
            Cv2.CvtColor(input, gray, ColorConversionCodes.BGR2GRAY);
            return gray;
        }

        public Mat ApplyMedianStep(Mat input, int medianFilter)
        {
            Mat result = input.Clone();
            Cv2.MedianBlur(result, result, medianFilter);
            return result;
        }

        public Mat ApplyCannyStep(Mat input, int cannyThreshold)
        {
            Mat result = input.Clone();
            Cv2.Canny(result, result, cannyThreshold, 255);
            return result;
        }

        public Point[] ApplyEllipseStep(Mat image)
        {
            Cv2.FindContours(image, out Point[][] contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            var allPoints = new List<Point>();

            foreach (var cnt in contours)
                allPoints.AddRange(cnt);

            if (allPoints.Count < 5)
                return null;

            Point[] hull = Cv2.ConvexHull(allPoints.ToArray());

            if (hull.Length < 5)
                return hull;

            RotatedRect ellipse = Cv2.FitEllipse(hull);

            return Cv2.Ellipse2Poly((Point)ellipse.Center, new Size((int)(ellipse.Size.Width / 2), (int)(ellipse.Size.Height / 2)), (int)ellipse.Angle, 0, 360, 1);
        }

        public Mat SimulateCameraSaturation(Mat img, int saturation)
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

        public Point[] CorrectContour(Point[] contour, float threshold)
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
