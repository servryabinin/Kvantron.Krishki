using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace CapDefectDetector.ImageProcessing.Utils.ContourProcessor
{
    public class CapColorContourUtils
    {
        private const byte GREEN_THRESHOLD = 40;

        public Mat ApplySaturationStep(Mat input, int saturation)
        {
            return SimulateCameraSaturation(input, saturation);
        }

        public Mat ApplyCapsColorStep(Mat input, byte capsColor, bool isColored, bool isYellow, bool isGreen)
        {
            Mat result = input.Clone();

            NonlinearBackgroundDecolorization(result, capsColor, isColored, isYellow, isGreen);

            return result;
        }

        public Mat[] ApplyWindowStep(Mat input, int window)
        {
            Mat[] channels;

            Cv2.Split(input, out channels);

            Cv2.GaussianBlur(channels[0], channels[1], new Size(window, window), 4);

            return channels;
        }

        public void ApplyMorphologyStep(Mat[] channels, Mat element1, Mat element2)
        {
            Cv2.Threshold(channels[1], channels[0], 128, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);
            Cv2.MorphologyEx(channels[0], channels[1], MorphTypes.Dilate, element1);
            Cv2.MorphologyEx(channels[1], channels[2], MorphTypes.Erode, element2);
            Cv2.MedianBlur(channels[2], channels[2], 5);
        }

        public Point[] GetMaxContour(Mat image)
        {
            Cv2.FindContours(image, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
                return null;

            int maxInd = 0;
            double maxArea = 0;

            for (int i = 0; i < contours.Length; i++)
            {
                double area = Cv2.ContourArea(contours[i]);

                if (area > maxArea)
                {
                    maxArea = area;
                    maxInd = i;
                }
            }

            return contours[maxInd];
        }

        public static void NonlinearBackgroundDecolorization(Mat img, byte nWhite, bool isColored, bool isYellowCap, bool isGreenColor)
        {
            if (img.Empty() || img.Type() != MatType.CV_8UC3)
                throw new ArgumentException("Ожидается 3-канальное 8-битное изображение.");

            int total = img.Rows * img.Cols * 3;

            unsafe
            {
                byte* data = (byte*)img.DataPointer;

                // 1️ Выбеливание, если крышка не зелёная
                if (!isGreenColor)
                {
                    for (int i = 0; i < total; i++)
                    {
                        int val = (255 * data[i]) / nWhite;
                        if (val > 255) val = 255;
                        data[i] = (byte)val;
                    }
                }
                else
                {
                    // 2️ Ветка для зелёных крышек — как в C++-коде
                    for (int i = 0; i < total; i += 3)
                    {
                        data[i + 1] = (byte)Math.Abs(
                            data[i + 1] - ((data[i] + data[i + 2]) >> 1)
                        );
                    }
                }

                // 3 Если крышка цветная
                if (isColored)
                {
                    for (int i = 0; i < total; i += 3)
                    {
                        if (!isGreenColor)
                        {
                            if (isYellowCap)
                            {
                                data[i] = (byte)((3 * data[i + 2] + data[i + 1]) >> 2);
                            }
                            else
                            {
                                data[i] = (byte)Math.Abs(data[i] - ((data[i + 1] + data[i + 2]) >> 1));
                            }
                        }
                        else
                        {
                            // Для зелёных крышек — бинаризация по синему каналу
                            if (data[i + 1] < GREEN_THRESHOLD)
                            {
                                data[i] = (byte)0x00;
                            }
                            else
                            {
                                data[i] = (byte)0xFF;
                            }
                        }

                    }
                }
            }
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
