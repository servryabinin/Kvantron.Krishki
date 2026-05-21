using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CapDefectDetector.DTO.DefectSettings;
using CapDefectDetector.Logger;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.ImageProcessing.Utils
{
    public class CapPaintDefectUtils
    {
        private double _minAreaInpaintDefect;
        private double _minInpaintWhiteThreshold;

        private double _sMin;
        private double _sMax;
        private double _vMin;
        private double _vMax;

        public CapPaintDefectUtils(PaintDefectSettings settings)
        {
            SetMinAreaInpaintDefect(settings.MinAreaInpaintDefect);
            SetMinInpaintWhiteThreshold(settings.MinInpaintWhiteThreshold);

            SetSMin(settings.SMin);
            SetSMax(settings.SMax);
            SetVMin(settings.VMin);
            SetVMax(settings.VMax);
        }

        public void SetMinAreaInpaintDefect(double value) => _minAreaInpaintDefect = value;
        public void SetMinInpaintWhiteThreshold(double value) => _minInpaintWhiteThreshold = value;
        public void SetSMin(double value) => _sMin = value;
        public void SetSMax(double value) => _sMax = value;
        public void SetVMin(double value) => _vMin = value;
        public void SetVMax(double value) => _vMax = value;

        public PaintDefectSettings GetSettings()
        {
            return new PaintDefectSettings
            {
                SMin = _sMin,
                SMax = _sMax,
                VMin = _vMin,
                VMax = _vMax,
                MinInpaintWhiteThreshold = _minInpaintWhiteThreshold,
                MinAreaInpaintDefect = _minAreaInpaintDefect
            };
        }

        public bool CheckForPaintDefects(
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
                    ErrorLogger.Log(
                        new Exception("Контур крышки пустой или содержит недостаточно точек"),
                        "CheckForPaintDefects - проверка контура");
                    return false;
                }

                using (Mat hsv = CreateHsv(image))
                {
                    token.ThrowIfCancellationRequested();

                    using (Mat capMask = CreateCapMask(image, capContour))
                    {
                        token.ThrowIfCancellationRequested();

                        using (Mat maskedHSV = ApplyMaskToHsv(hsv, capMask))
                        {
                            token.ThrowIfCancellationRequested();

                            Cv2.Split(maskedHSV, out Mat[] hsvChannels);

                            using (hsvChannels[0])
                            using (hsvChannels[1])
                            using (hsvChannels[2])
                            using (Mat whiteMask = CreateWhiteMask(maskedHSV))
                            {
                                token.ThrowIfCancellationRequested();

                                using (Mat defectsMask = CreateDefectsMask(whiteMask))
                                {
                                    using (Mat maskedDefects = ApplyDefectsMask(defectsMask, capMask))
                                    {
                                        token.ThrowIfCancellationRequested();

                                        var contours = FindContours(maskedDefects, token);

                                        var significantContours = contours
                                            .Where(c => Cv2.ContourArea(c) > _minAreaInpaintDefect)
                                            .ToList();

                                        var defects = ProcessContours(significantContours, image, token);

                                        DrawDefects(drawFrame, defects);

                                        return defects.Count > 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CheckForPaintDefects - непредвиденная ошибка");
                return false;
            }
        }

        private Mat CreateHsv(Mat image)
        {
            try
            {
                Mat hsv = new Mat();
                Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
                return hsv;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CreateHsv - ошибка HSV");
                throw;
            }
        }

        private Mat CreateCapMask(Mat image, Point[] capContour)
        {
            try
            {
                Mat mask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
                Cv2.FillPoly(mask, new[] { capContour }, new Scalar(255));
                return mask;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CreateCapMask - ошибка маски");
                throw;
            }
        }

        private Mat ApplyMaskToHsv(Mat hsv, Mat capMask)
        {
            Mat result = new Mat();
            Cv2.BitwiseAnd(hsv, hsv, result, capMask);
            return result;
        }

        private Mat CreateWhiteMask(Mat hsv)
        {
            try
            {
                Mat whiteMask = new Mat();

                Cv2.InRange(
                    hsv,
                    new Scalar(0, 255 * _sMin, 255 * _vMin),
                    new Scalar(180, 255 * _sMax, 255 * _vMax),
                    whiteMask);

                return whiteMask;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CreateWhiteMask - ошибка InRange");
                throw;
            }
        }

        private Mat CreateDefectsMask(Mat whiteMask)
        {
            Mat defectsMask = new Mat();
            Cv2.BitwiseNot(whiteMask, defectsMask);
            return defectsMask;
        }

        private Mat ApplyDefectsMask(Mat defectsMask, Mat capMask)
        {
            Mat result = new Mat();
            Cv2.BitwiseAnd(defectsMask, capMask, result);
            return result;
        }

        private Point[][] FindContours(Mat maskedDefects, CancellationToken token)
        {
            Cv2.FindContours(
                maskedDefects,
                out Point[][] contours,
                out _,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple);

            token.ThrowIfCancellationRequested();
            return contours;
        }

        private List<Point[]> ProcessContours(List<Point[]> contours, Mat image, CancellationToken token)
        {
            var result = new List<Point[]>();

            foreach (var contour in contours)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    using (Mat contourMask = Mat.Zeros(image.Size(), MatType.CV_8UC1))
                    {
                        Cv2.FillPoly(contourMask, new[] { contour }, new Scalar(255));

                        using (Mat maskedImage = new Mat())
                        {
                            Cv2.BitwiseAnd(image, image, maskedImage, contourMask);

                            Scalar meanColor = Cv2.Mean(maskedImage, contourMask);

                            if (Math.Abs(meanColor.Val2 - 255) < _minInpaintWhiteThreshold)
                            {
                                result.Add(contour);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "ProcessContours - ошибка обработки контура");
                }
            }

            return result;
        }

        private void DrawDefects(Mat drawFrame, List<Point[]> defects)
        {
            if (defects.Count == 0)
                return;

            try
            {
                Cv2.DrawContours(drawFrame, defects, -1, new Scalar(0, 0, 255), 2);

                foreach (var contour in defects)
                {
                    Rect bbox = Cv2.BoundingRect(contour);
                    Cv2.Rectangle(drawFrame, bbox, new Scalar(0, 255, 255), 2);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "DrawDefects - ошибка отрисовки");
            }
        }
    }
}