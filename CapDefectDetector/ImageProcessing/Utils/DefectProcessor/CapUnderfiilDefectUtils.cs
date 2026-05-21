using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using CapDefectDetector.DTO.DefectSettings;
using CapDefectDetector.ImageProcessing.Utils.ContourProcessor;
using CapDefectDetector.Logger;
using MathNet.Numerics.IntegralTransforms;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.ImageProcessing.Utils
{
    public class CapUnderfillDefectUtils
    {

        private byte _capsColor;
        private bool _decolorizeBackground;
        private bool _normalizeBrightness;
        private bool _preserveDetails;

        private double _coefCapRadiusUnderFill;

        private double _rectHeightCoef;
        private int _innerOffset;

        private int _underfillRectWidth;

        private double _corrugationsCountForUnderFill;


        private double[] _sinTable;
        private double[] _cosTable;
        private readonly object _trigTablesLock = new object();

        private readonly CapColorContourUtils _colorUtils;

        public CapUnderfillDefectUtils(CapColorContourUtils colorUtils,UnderfillDefectSettings settings)
        {
            SetCapsColor(settings.CapsColor);
            SetDecolorizeBackground(settings.DecolorizeBackground);
            SetNormalizeBrightness(settings.NormalizeBrightness);
            SetPreserveDetails(settings.PreserveDetails);

            SetCoefCapRadiusUnderFill(settings.CoefCapRadiusUnderFill);
            SetRectHeightCoef(settings.RectHeightCoef);
            SetInnerOffset(settings.InnerOffset);

            SetUnderfillRectWidth(settings.UnderfillRectWidth);

            SetCorrugationsCountForUnderFill(settings.CorrugationsCountForUnderFill);
        }

        public void SetCapsColor(byte value) => _capsColor = value;

        public void SetDecolorizeBackground(bool value) => _decolorizeBackground = value;

        public void SetNormalizeBrightness(bool value) => _normalizeBrightness = value;

        public void SetPreserveDetails(bool value) => _preserveDetails = value;

        public void SetCoefCapRadiusUnderFill(double value) => _coefCapRadiusUnderFill = value;

        public void SetRectHeightCoef(double value) => _rectHeightCoef = value;

        public void SetInnerOffset(int value) => _innerOffset = value;

        public void SetUnderfillRectWidth(int value)
        {
            _underfillRectWidth = value;
            InitializeTrigTables();
        }

        public void SetCorrugationsCountForUnderFill(double value)
            => _corrugationsCountForUnderFill = value;

        public UnderfillDefectSettings GetSettings()
        {
            return new UnderfillDefectSettings
            {
                CapsColor = _capsColor,
                DecolorizeBackground = _decolorizeBackground,
                NormalizeBrightness = _normalizeBrightness,
                PreserveDetails = _preserveDetails,
                CoefCapRadiusUnderFill = _coefCapRadiusUnderFill,
                RectHeightCoef = _rectHeightCoef,
                InnerOffset = _innerOffset,
                UnderfillRectWidth = _underfillRectWidth,
                CorrugationsCountForUnderFill = _corrugationsCountForUnderFill
            };
        }

        private void InitializeTrigTables()
        {
            lock (_trigTablesLock)
            {
                _sinTable = new double[_underfillRectWidth];
                _cosTable = new double[_underfillRectWidth];

                double dTheta = 2 * Math.PI / _underfillRectWidth;

                for (int i = 0; i < _underfillRectWidth; i++)
                {
                    _sinTable[i] = Math.Sin(i * dTheta);
                    _cosTable[i] = Math.Cos(i * dTheta);
                }
            }
        }

        public bool CheckForUnderFillDefects(
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
                {
                    ErrorLogger.Log(new Exception("Контур для проверки недолива пустой или содержит недостаточно точек"),"CheckForUnderFillDefects - проверка наличия контура");

                    return false;
                }

                using (Mat correctedColor = CreateCorrectedImage(image))
                using (Mat correctedGray = CreateGray(correctedColor))
                {
                    token.ThrowIfCancellationRequested();

                    var (outerEllipse, innerEllipse) = CreateEllipses(capContour);

                    int rectHeight = CalculateRectHeight(outerEllipse);

                    using (Mat crownMask = CreateCrownMask(image, outerEllipse, innerEllipse))
                    using (Mat maskedGray = ApplyMask(correctedGray, crownMask))
                    using (Mat stripe = CreateStripe(maskedGray, outerEllipse, innerEllipse, rectHeight))
                    {
                        token.ThrowIfCancellationRequested();

                        float[] signal = BuildSignal(stripe, rectHeight);
                        bool hasUnderfill = AnalyzeUnderfillFFT(signal,_underfillRectWidth,_corrugationsCountForUnderFill);

                        DrawResult(drawFrame, innerEllipse, hasUnderfill);

                        return hasUnderfill;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CheckForUnderFillDefects - ошибка при определении недолива");

                return false;
            }
        }

        private Mat CreateCorrectedImage(Mat image)
        {
            try
            {
                Mat result = image.Clone();

                CapColorContourUtils.NonlinearBackgroundDecolorization(
                    result,
                    _capsColor,
                    _decolorizeBackground,
                    _normalizeBrightness,
                    _preserveDetails);

                return result;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex,"CapUnderFillUtils - ошибка коррекции цвета");
                throw;
            }
        }

        private Mat CreateGray(Mat image)
        {
            try
            {
                Mat gray = new Mat();
                Cv2.CvtColor(image,gray,ColorConversionCodes.BGR2GRAY);

                return gray;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CapUnderFillUtils - ошибка создания grayscale");
                throw;
            }
        }

        private (RotatedRect outerEllipse, RotatedRect innerEllipse) CreateEllipses(Point[] contour)
        {
            try
            {
                RotatedRect outerEllipse = Cv2.FitEllipse(contour);
                Size2f innerSize = new Size2f((float)(outerEllipse.Size.Width * _coefCapRadiusUnderFill),(float)(outerEllipse.Size.Height * _coefCapRadiusUnderFill));
                RotatedRect innerEllipse = new RotatedRect(outerEllipse.Center,innerSize,outerEllipse.Angle);

                return (outerEllipse, innerEllipse);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CapUnderFillUtils - ошибка построения эллипсов");
                throw;
            }
        }

        private int CalculateRectHeight(RotatedRect ellipse)
        {
            return (int)(Math.Max(ellipse.Size.Width, ellipse.Size.Height) * _rectHeightCoef);
        }

        private Mat CreateCrownMask(Mat image, RotatedRect outerEllipse, RotatedRect innerEllipse)
        {
            try
            {
                Mat mask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
                Cv2.Ellipse(mask, outerEllipse, Scalar.White, -1);
                Cv2.Ellipse(mask, innerEllipse, Scalar.Black, -1);

                return mask;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CapUnderFillUtils - ошибка создания crown mask");

                throw;
            }
        }

        private Mat ApplyMask(Mat gray, Mat mask)
        {
            try
            {
                Mat result = new Mat();
                Cv2.BitwiseAnd(gray, gray, result, mask);
                return result;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex,"CapUnderFillUtils - ошибка применения маски");

                throw;
            }
        }


        private Mat CreateStripe(
            Mat maskedGray,
            RotatedRect outerEllipse,
            RotatedRect innerEllipse,
            int rectHeight)
        {
            try
            {
                Mat stripe = new Mat(rectHeight,_underfillRectWidth,MatType.CV_8UC1);
                float outerRadius =(float)(Math.Max(outerEllipse.Size.Width,outerEllipse.Size.Height) / 2);
                float innerRadius =(float)(Math.Max(innerEllipse.Size.Width,innerEllipse.Size.Height) / 2);
                float meanRadius = (outerRadius + innerRadius) / 2;
                Point center = new Point((int)outerEllipse.Center.X,(int)outerEllipse.Center.Y);
                GetStripeImg(maskedGray,stripe,_sinTable,_cosTable,center,(int)meanRadius,maskedGray.Width,maskedGray.Height,_underfillRectWidth,rectHeight,_innerOffset);

                return stripe;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CapUnderFillUtils - ошибка создания stripe");

                throw;
            }
        }

        private float[] BuildSignal(Mat stripe, int rectHeight)
        {
            try
            {
                float[] signal = new float[_underfillRectWidth];
                GetWStatistics(stripe,signal,_underfillRectWidth,rectHeight);
                return signal;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CapUnderFillUtils - ошибка построения сигнала");
                throw;
            }
        }

        private void DrawResult(Mat drawFrame,RotatedRect ellipse,bool defect)
        {
            try
            {
                Scalar color =defect? new Scalar(0, 0, 255): new Scalar(0, 255, 0);
                Cv2.Ellipse(drawFrame, ellipse, color, 2);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CapUnderFillUtils - ошибка рисования результата");
            }
        }


        private bool AnalyzeUnderfillFFT(float[] signal,int length,double expectedHarmonic)
        {
            try
            {
                if (signal == null || signal.Length < length)
                    return false;

                float[] data = new float[length];
                Array.Copy(signal, data, length);

                double mean = 0;
                for (int i = 0; i < length; i++)
                    mean += data[i];
                mean /= length;

                for (int i = 0; i < length; i++)
                    data[i] -= (float)mean;

                Complex[] complexData = new Complex[length];
                for (int i = 0; i < length; i++)
                    complexData[i] = new Complex(data[i], 0);

                Fourier.Forward(complexData);

                int minHarmonic = (int)Math.Max(1, expectedHarmonic - 2);

                int maxHarmonic = (int)Math.Min(length / 2, expectedHarmonic + 2);

                double maxMagnitude = 0;
                int maxIndex = minHarmonic;

                for (int i = minHarmonic; i <= maxHarmonic; i++)
                {
                    double magnitude = complexData[i].Magnitude;
                    if (magnitude > maxMagnitude)
                    {
                        maxMagnitude = magnitude;
                        maxIndex = i;
                    }
                }

                bool result;

                if (maxIndex >= expectedHarmonic - 1 && maxIndex <= expectedHarmonic + 1)
                {
                    result = false;
                }
                else
                {
                    result = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "AnalyzeUnderfillFFT - ошибка FFT");

                return false;
            }
        }

        private static void GetWStatistics(Mat src,float[] dst,int w,int h)
        {
            Array.Clear(dst, 0, dst.Length);

            unsafe
            {
                byte* srcPtr = (byte*)src.DataPointer;
                fixed (float* dstPtr = dst)
                {
                    for (int j = 0; j < h; j++)
                    {
                        byte* rowPtr = srcPtr + j * w;
                        for (int i = 0; i < w; i++)
                            dstPtr[i] += rowPtr[i];
                    }
                }
            }
        }

        private static void GetStripeImg(
            Mat src,
            Mat dst,
            double[] sinTable,
            double[] cosTable,
            Point center,
            int radius,
            int w,
            int h,
            int width,
            int height,
            int innerOffset)
        {
            int hr = radius - height;

            for (int j = 0; j < height; j++)
            {
                int r = hr + j + innerOffset;

                for (int i = 0; i < width; i++)
                {
                    double x = sinTable[i] * r + center.X;
                    double y = cosTable[i] * r + center.Y;

                    dst.Set<byte>(j,i,Bilinear8Bit(src, x, y, w, h));
                }
            }
        }

        private static byte Bilinear8Bit(Mat img,double x,double y,int w,int h)
        {
            int u = (int)x;
            int v = (int)y;

            if (u < 0 || v < 0 || u >= w - 1 || v >= h - 1)
                return 0;

            double dx = x - u;
            double dy = y - v;

            byte p1 = img.At<byte>(v, u);
            byte p2 = img.At<byte>(v, u + 1);
            byte p3 = img.At<byte>(v + 1, u);
            byte p4 = img.At<byte>(v + 1, u + 1);

            double interpolated =
                (1 - dx) * (1 - dy) * p1 +
                dx * (1 - dy) * p2 +
                (1 - dx) * dy * p3 +
                dx * dy * p4;

            return (byte)Math.Round(interpolated);
        }
    }
}