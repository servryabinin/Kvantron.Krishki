using System.Xml.Linq;
using CapDefectDetector.DTO.CapRecipe;
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace CapDefectDetector.ImageProcessing.Utils.ContourProcessor
{
    public class CapContourUtils : IDisposable
    {
        #region Ненастраиваемые поля
        private const byte GREEN_THRESHOLD = 40;
        #endregion

        #region Настройки для цветных крышек
        private byte _capsColor;
        private int _window;
        private int _morphSize;
        private int _morphSize2;
        private int _saturationColor;
        private float _contourCorrectionColor;
        private bool _isGreenCap;
        private bool _isColoredCap;
        private bool _isYellowCap;
        private bool _isWhiteCap;
        #endregion

        #region Настройки для черных/коричневых крышек
        private int _saturationBlackOrBrown;
        private int _medianFilter;
        private int _cannyThreshold;
        private float _contourCorrectionBlackOrBrown;
        private bool _isBlackOrBrownCap;
        #endregion

        #region Морфология
        private Mat _element1;
        private Mat _element2;
        #endregion

        #region Осовобождение памяти
        private bool _disposed;
        #endregion

        public CapContourUtils(CapRecipe settings)
        {
            SetCapsColor(settings.CapsColor);
            SetWindow(settings.Window);
            SetMorphSize(settings.MorphSize);
            SetMorphSize2(settings.MorphSize2);
            SetCameraSaturation(settings.CameraSaturation);
            SetContourCorrectionColor(settings.ContourCorrectionColor);
            SetIsGreen(settings.IsGreen);
            SetIsColored(settings.IsColored);
            SetIsYellow(settings.IsYellow);
            SetIsWhite(settings.IsWhite);

            SetCameraSaturationBlackOrBrown(settings.CameraSaturationBlackOrBrown);
            SetMedianFilter(settings.MedianFilter);
            SetCannyThreshold(settings.CannyThreshold);
            SetContourCorrectionBlackOrBrown(settings.ContourCorrectionBlackOrBrown);
            SetIsBlackOrBrown(settings.IsBlackOrBrown);
        }

        #region Сеттеры
        public void SetCapsColor(byte value) => _capsColor = value;
        public void SetWindow(int value) => _window = value;
        public void SetMorphSize(int value)
        {
            _morphSize = value;
            RebuildMorphology();
        }
        public void SetMorphSize2(int value)
        {
            _morphSize2 = value;
            RebuildMorphology();
        }
        public void SetCameraSaturation(int value) => _saturationColor = value;
        public void SetContourCorrectionColor(float value) => _contourCorrectionColor = value;
        public void SetIsGreen(bool value) => _isGreenCap = value;
        public void SetIsColored(bool value) => _isColoredCap = value;
        public void SetIsYellow(bool value) => _isYellowCap = value;
        public void SetIsWhite(bool value) => _isWhiteCap = value;

        public void SetCameraSaturationBlackOrBrown(int value) => _saturationBlackOrBrown = value;
        public void SetMedianFilter(int value) => _medianFilter = value;
        public void SetCannyThreshold(int value) => _cannyThreshold = value;
        public void SetContourCorrectionBlackOrBrown(float value) => _contourCorrectionBlackOrBrown = value;
        public void SetIsBlackOrBrown(bool value) => _isBlackOrBrownCap = value;
        #endregion

        #region Геттеры для получения текущих настроек в виде CapRecipe
        public CapRecipe GetSettings()
        {
            return new CapRecipe
            {
                CapsColor = _capsColor,
                Window = _window,
                MorphSize = _morphSize,
                MorphSize2 = _morphSize2,
                CameraSaturation = _saturationColor,
                ContourCorrectionColor = _contourCorrectionColor,
                IsGreen = _isGreenCap,
                IsColored = _isColoredCap,
                IsYellow = _isYellowCap,
                IsWhite = _isWhiteCap,

                CameraSaturationBlackOrBrown = _saturationBlackOrBrown,
                MedianFilter = _medianFilter,
                CannyThreshold = _cannyThreshold,
                ContourCorrectionBlackOrBrown = _contourCorrectionBlackOrBrown,
                IsBlackOrBrown = _isBlackOrBrownCap
            };
        }

        public byte GetCapsColor() => _capsColor;
        public int GetWindow() => _window;
        public int GetMorphSize() => _morphSize;
        public int GetMorphSize2() => _morphSize2;
        public int GetCameraSaturation() => _saturationColor;
        public float GetContourCorrectionColor() => _contourCorrectionColor;
        public bool GetIsGreen() => _isGreenCap;
        public bool GetIsColored() => _isColoredCap;
        public bool GetIsYellow() => _isYellowCap;
        public bool GetIsWhite() => _isWhiteCap;

        public int GetCameraSaturationBlackOrBrown() => _saturationBlackOrBrown;
        public int GetMedianFilter() => _medianFilter;
        public int GetCannyThreshold() => _cannyThreshold;
        public float GetContourCorrectionBlackOrBrown() => _contourCorrectionBlackOrBrown;
        public bool GetIsBlackOrBrown() => _isBlackOrBrownCap;

        public Mat GetElement1() => _element1;
        public Mat GetElement2() => _element2;
        #endregion

        #region Перестройка морфологии
        private void RebuildMorphology()
        {
            _element1?.Dispose();
            _element2?.Dispose();

            _element1 = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new Size(2 * _morphSize + 1, 2 * _morphSize + 1),
                new Point(_morphSize, _morphSize));

            _element2 = Cv2.GetStructuringElement(
                MorphShapes.Cross,
                new Size(2 * _morphSize2 + 1, 2 * _morphSize2 + 1),
                new Point(_morphSize2, _morphSize2));
        }
        #endregion

        #region Общий метод по нахождению контура
        public CapContourResult GetCapContour(Mat gray, Mat image)
        {
            if (gray.Empty() || image.Empty())
                return null;

            if (_isBlackOrBrownCap)
            {
                return GetBlackOrBrownContour(gray, image);
            }
            else
            {
                return GetColorCapContour(gray, image);
            }
        }

        private CapContourResult GetColorCapContour(Mat gray, Mat image)
        {
            if (gray.Empty() || image.Empty())
                return null;

            using Mat sat = ApplySaturationStep(image, _saturationColor);
            Mat caps = ApplyCapsColorStep(sat, _capsColor, _isColoredCap, _isYellowCap, _isGreenCap);
            Mat[] channels = ApplyWindowStep(caps, _window);
            ApplyMorphologyStep(channels, _element1, _element2);
            Point[] contour = GetMaxContour(channels[2]);
            contour = CorrectContour(contour, _contourCorrectionColor);
            return new CapContourResult
            {
                Contour = contour,
                Blur1 = channels[1],
                Blur2 = channels[2],
                Mask = caps
            };
        }

        private CapContourResult GetBlackOrBrownContour(Mat grayInput, Mat image)
        {
            if (image.Empty())
                return null;

            using Mat sat = ApplySaturationStep(image, _saturationBlackOrBrown);
            using Mat gray = ApplyGrayStep(sat);
            Mat blurred = ApplyMedianStep(gray, _medianFilter);
            Mat edges = ApplyCannyStep(blurred, _cannyThreshold);
            Point[] contour = ApplyEllipseStep(edges);
            contour = CorrectContour(contour, _contourCorrectionBlackOrBrown);

            return new CapContourResult
            {
                Contour = contour,
                Blur1 = blurred,
                Blur2 = edges,
                Mask = edges
            };
        }
        #endregion

        #region Методы для нахождения цветных крышек
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
        #endregion

        #region Методы для нахождения черных/коричневых крышек
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
        #endregion

        #region Общие методы
        public Mat ApplySaturationStep(Mat input, int saturation)
        {
            return SimulateCameraSaturation(input, saturation);
        }

        public static Mat SimulateCameraSaturation(Mat img, int saturation)
        {
            if (img == null || img.Empty())
                return null;

            float koeff = saturation / 128.0f;

            using Mat hsv = new Mat();
            Cv2.CvtColor(img, hsv, ColorConversionCodes.BGR2HSV);

            unsafe
            {
                byte* data = hsv.DataPointer;
                int total = hsv.Rows * hsv.Cols;

                for (int i = 0; i < total; i++)
                {
                    int idx = i * 3;

                    float val = data[idx + 1] * koeff;
                    data[idx + 1] = (byte)Math.Min(255f, val);
                }
            }

            Mat result = new Mat();
            Cv2.CvtColor(hsv, result, ColorConversionCodes.HSV2BGR);

            return result;
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
        #endregion

        #region Освобождение памяти
        public void Dispose()
        {
            if (_disposed)
                return;

            _element1?.Dispose();
            _element2?.Dispose();

            _disposed = true;
        }
        #endregion
    }
}