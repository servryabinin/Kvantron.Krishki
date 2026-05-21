using System;
using CapDefectDetector.Logger;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.ImageProcessing.Utils
{
    public class CapOvalityUtils
    {
        private double _ovalityThreshold;

        public CapOvalityUtils(double ovalityThreshold)
        {
            _ovalityThreshold = ovalityThreshold;
        }

        public void SetThreshold(double value)
        {
            _ovalityThreshold = value;
        }

        public bool CheckOvality(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (!IsContourValid(capContour))
                {
                    LogInvalidContour();
                    return false;
                }

                token.ThrowIfCancellationRequested();

                RotatedRect ellipse = FitCapEllipse(capContour);

                float axisRatio = CalculateAxisRatio(ellipse);

                bool isOval = IsOval(axisRatio);

                TryDrawResult(drawFrame, ellipse, axisRatio, isOval);

                return isOval;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CheckOvality - ошибка при расчёте овальности крышки");
                return false;
            }
        }

        private bool IsContourValid(Point[] contour)
        {
            return contour != null && contour.Length >= 5;
        }

        private void LogInvalidContour()
        {
            ErrorLogger.Log(
                new Exception("Контур для проверки овальности пустой или содержит недостаточно точек"),
                "CheckOvality - проверка наличия контуров"
            );
        }

        private RotatedRect FitCapEllipse(Point[] contour)
        {
            return Cv2.FitEllipse(contour);
        }

        private float CalculateAxisRatio(RotatedRect ellipse)
        {
            float majorAxis = Math.Max(ellipse.Size.Width, ellipse.Size.Height);
            float minorAxis = Math.Min(ellipse.Size.Width, ellipse.Size.Height);

            if (majorAxis <= 0)
                return 0;

            return minorAxis / majorAxis;
        }

        private bool IsOval(float axisRatio)
        {
            return axisRatio < _ovalityThreshold;
        }

        private void TryDrawResult(Mat drawFrame, RotatedRect ellipse, float axisRatio, bool isOval)
        {
            try
            {
                Scalar color = isOval ? new Scalar(0, 0, 255) : new Scalar(0, 255, 0);

                Cv2.Ellipse(drawFrame, ellipse, color, 2);

                Cv2.PutText(
                    drawFrame,
                    $"Ratio: {axisRatio:F5}",
                    new Point(10, 30),
                    HersheyFonts.HersheySimplex,
                    1,
                    color,
                    2
                );
            }
            catch (Exception drawEx)
            {
                ErrorLogger.Log(drawEx, "CheckOvality - ошибка при рисовании эллипса или текста");
            }
        }
    }
}