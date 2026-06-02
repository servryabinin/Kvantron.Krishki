using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapDefectDetector.Domain;
using CapDefectDetector.DTO.CapRecipe;
using CapDefectDetector.ImageProcessing.Utils;
using CapDefectDetector.ImageProcessing.Utils.ContourProcessor;
using CapDefectDetector.Logger;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.Forms.DefectParamSettingsForms.UnderfillSettingsForm
{
    public partial class UnderfillSettingsForm : Form
    {
        #region Объекты классов
        private Mat _image;
        private readonly CapRecipe _recipe;

        private readonly CapUnderfillDefectUtils _sourceUnderfiilUtils;
        private readonly CapUnderfillDefectUtils _editableUnderfillUtils;
        private readonly CapContourUtils _capContourUtils;
        #endregion

        #region Глобальные поля
        private Point[] _contour;
        private bool _isUnderfill;
        #endregion

        public UnderfillSettingsForm(Mat image, CapUnderfillDefectUtils underfillUtils, CapContourUtils capContourUtils)
        {
            InitializeComponent();

            _image = image?.Clone();

            _sourceUnderfiilUtils = underfillUtils;
            _editableUnderfillUtils = new CapUnderfillDefectUtils(underfillUtils.GetSettings());
            _capContourUtils = capContourUtils;

            ApplySettingsToUI();
            RunPipeline();
        }

        #region Визуализация
        private void ApplySettingsToUI()
        {
            var s = _editableUnderfillUtils.GetSettings();

            underfillSettings_UnderFillcapsColorNumUd.Value = (decimal)s.CapsColor;
            underfillSettings_decolorizeBackgroundChb.Checked = (bool)s.DecolorizeBackground;
            underfillSettings_normalizeBrightnessChb.Checked = (bool)s.NormalizeBrightness;
            underfillSettings_preserveDetailsChb.Checked = (bool)s.PreserveDetails;
            underfillSettings_coefCapRadiusUnderFillNumUd.Value = (decimal)s.CoefCapRadiusUnderFill;
            underfillSettings_innerOffsetNumUd.Value = (decimal)s.InnerOffset;
            underfillSettings_rectHeightCoefNumUd.Value = (decimal)s.RectHeightCoef;
            underfillSettings_underFillRectWidthNumUd.Value = (decimal)s.UnderfillRectWidth;
            underfillSettings_corrugationsCountForUnderFillNumUd.Value = (decimal)s.CorrugationsCountForUnderFill;

            UpdateColorCorrectionUiState();
        }

        private void UpdateVisualization()
        {
            underfillSettings_resultLabel.Text = _isUnderfill ? "NG" : "OK";
            underfillSettings_resultLabel.ForeColor = _isUnderfill ? System.Drawing.Color.Red : System.Drawing.Color.Green;
        }
        #endregion

        #region Pipline вкрапления
        private void RunPipeline()
        {
            if (_image == null || _image.Empty())
                return;

            using var gray = new Mat();
            Cv2.CvtColor(_image, gray, ColorConversionCodes.BGR2GRAY);

            var result = _capContourUtils.GetCapContour(gray, _image);
            if (result?.Contour == null || result.Contour.Length < 5)
                return;

            _contour = result.Contour;

            Mat draw = _image.Clone();

            _isUnderfill = RunUnderfill(draw);

            UpdateVisualization();
        }

        private bool RunUnderfill(Mat drawFrame)
        {
            if (_contour == null || _contour.Length < 5)
                return false;

            try
            {
                underfillSettings_underfillOriginPb.Image = BitmapConverter.ToBitmap(drawFrame);

                Mat correctedColor = _editableUnderfillUtils.CreateCorrectedImage(_image);
                underfillSettings_underfillColorCorrectedPb.Image = BitmapConverter.ToBitmap(correctedColor);

                Mat correctedGray = _editableUnderfillUtils.CreateGray(correctedColor);

                var (outerEllipse, innerEllipse) = _editableUnderfillUtils.CreateEllipses(_contour);

                Mat ellipseView = drawFrame.Clone();

                Cv2.Ellipse(ellipseView, outerEllipse, new Scalar(0, 255, 0), 2);
                Cv2.Ellipse(ellipseView, innerEllipse, new Scalar(0, 0, 255), 2);

                underfillSettings_underfillEllipsePb.Image = BitmapConverter.ToBitmap(ellipseView);

                Mat crownMask = _editableUnderfillUtils.CreateCrownMask(_image, outerEllipse, innerEllipse);

                Mat maskedGray = _editableUnderfillUtils.ApplyMask(correctedGray, crownMask);

                underfillSettings_underfillMaskPb.Image = BitmapConverter.ToBitmap(maskedGray);

                int rectHeight = _editableUnderfillUtils.CalculateRectHeight(outerEllipse);

                Mat stripe = _editableUnderfillUtils.CreateStripe(maskedGray, outerEllipse, innerEllipse, rectHeight);

                underfillSettings_underfillStripePb.Image = BitmapConverter.ToBitmap(stripe);

                float[] signal = _editableUnderfillUtils.BuildSignal(stripe, rectHeight);

                Mat signalView = CreateSignalPlot(signal);

                underfillSettings_underfillSignalPb.Image = BitmapConverter.ToBitmap(signalView);

                _isUnderfill = _editableUnderfillUtils.AnalyzeUnderfillFFT(signal);

                Mat resultView = drawFrame.Clone();

                _editableUnderfillUtils.DrawResult(resultView, innerEllipse, _isUnderfill);

                underfillSettings_underfillResultPb.Image = BitmapConverter.ToBitmap(resultView);

                return _isUnderfill;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "RunUnderfill visualization pipeline error");
                return false;
            }
        }

        private Mat CreateSignalPlot(float[] signal)
        {
            int width = signal.Length;
            int height = 300;

            Mat plot = new Mat(
                new OpenCvSharp.Size(width, height),
                MatType.CV_8UC3,
                Scalar.Black);

            if (signal == null || signal.Length < 2)
                return plot;

            float min = signal.Min();
            float max = signal.Max();

            float range = max - min;

            if (range <= 0.0001f)
                range = 1;

            for (int i = 1; i < signal.Length; i++)
            {
                int y1 = height -
                         (int)(((signal[i - 1] - min) / range) * (height - 1));

                int y2 = height -
                         (int)(((signal[i] - min) / range) * (height - 1));

                Cv2.Line(
                    plot,
                    new Point(i - 1, y1),
                    new Point(i, y2),
                    new Scalar(0, 255, 0),
                    1);
            }

            return plot;
        }
        #endregion

        #region Обработчики событий при взаимодействии с интерфейсом
        private void underfillSettings_UnderFillcapsColorNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!underfillSettings_UnderFillcapsColorNumUd.Focused)
                return;

            _editableUnderfillUtils.SetCapsColor((byte)underfillSettings_UnderFillcapsColorNumUd.Value);

            RunPipeline();
        }

        private void UpdateColorCorrectionUiState()
        {
            bool decolor = underfillSettings_decolorizeBackgroundChb.Checked;
            bool normalize = underfillSettings_normalizeBrightnessChb.Checked;
            bool preserve = underfillSettings_preserveDetailsChb.Checked;

            if (!decolor)
            {
                SetEnabledAll(true);
                return;
            }

            if (normalize)
            {
                underfillSettings_preserveDetailsChb.Enabled = false;
            }
            else
            {
                underfillSettings_preserveDetailsChb.Enabled = true;
            }

            if (preserve)
            {
                underfillSettings_normalizeBrightnessChb.Enabled = false;
            }
            else
            {
                underfillSettings_normalizeBrightnessChb.Enabled = true;
            }

            underfillSettings_decolorizeBackgroundChb.Enabled = true;
        }

        private void SetEnabledAll(bool enabled)
        {
            underfillSettings_decolorizeBackgroundChb.Enabled = enabled;
            underfillSettings_normalizeBrightnessChb.Enabled = enabled;
            underfillSettings_preserveDetailsChb.Enabled = enabled;
        }

        private void underfillSettings_decolorizeBackgroundChb_CheckedChanged(object sender, EventArgs e)
        {
            if (!underfillSettings_decolorizeBackgroundChb.Focused)
                return;

            _editableUnderfillUtils.SetDecolorizeBackground(underfillSettings_decolorizeBackgroundChb.Checked);

            UpdateColorCorrectionUiState();
            RunPipeline();
        }

        private void underfillSettings_normalizeBrightnessChb_CheckedChanged(object sender, EventArgs e)
        {
            if (!underfillSettings_normalizeBrightnessChb.Focused)
                return;

            _editableUnderfillUtils.SetNormalizeBrightness(underfillSettings_normalizeBrightnessChb.Checked);

            UpdateColorCorrectionUiState();
            RunPipeline();
        }

        private void underfillSettings_preserveDetailsChb_CheckedChanged(object sender, EventArgs e)
        {
            if (!underfillSettings_preserveDetailsChb.Focused)
                return;

            _editableUnderfillUtils.SetPreserveDetails(underfillSettings_preserveDetailsChb.Checked);

            UpdateColorCorrectionUiState();
            RunPipeline();
        }

        private void underfillSettings_coefCapRadiusUnderFillNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!underfillSettings_coefCapRadiusUnderFillNumUd.Focused)
                return;

            _editableUnderfillUtils.SetCoefCapRadiusUnderFill((double)underfillSettings_coefCapRadiusUnderFillNumUd.Value);

            RunPipeline();
        }

        private void underfillSettings_innerOffsetNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!underfillSettings_innerOffsetNumUd.Focused)
                return;

            _editableUnderfillUtils.SetInnerOffset((int)underfillSettings_innerOffsetNumUd.Value);

            RunPipeline();
        }

        private void underfillSettings_rectHeightCoefNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!underfillSettings_rectHeightCoefNumUd.Focused)
                return;

            _editableUnderfillUtils.SetRectHeightCoef((double)underfillSettings_rectHeightCoefNumUd.Value);

            RunPipeline();
        }

        private void underfillSettings_underFillRectWidthNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!underfillSettings_underFillRectWidthNumUd.Focused)
                return;

            _editableUnderfillUtils.SetUnderfillRectWidth((int)underfillSettings_underFillRectWidthNumUd.Value);

            RunPipeline();
        }

        private void underfillSettings_corrugationsCountForUnderFillNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!underfillSettings_corrugationsCountForUnderFillNumUd.Focused)
                return;

            _editableUnderfillUtils.SetCorrugationsCountForUnderFill((int)underfillSettings_corrugationsCountForUnderFillNumUd.Value);

            RunPipeline();
        }


        #endregion

        #region Обработчики кнопок

        private void underfillSettings_loadImageForUnderfillSettingsBt_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            _image?.Dispose();
            _image = Cv2.ImRead(dlg.FileName);

            RunPipeline();
        }

        private void underfillSettings_okBt_Click(object sender, EventArgs e)
        {
            var s = _editableUnderfillUtils.GetSettings();

            _sourceUnderfiilUtils.SetCapsColor(s.CapsColor);
            _sourceUnderfiilUtils.SetDecolorizeBackground(s.DecolorizeBackground);
            _sourceUnderfiilUtils.SetNormalizeBrightness(s.NormalizeBrightness);
            _sourceUnderfiilUtils.SetPreserveDetails(s.PreserveDetails);

            _sourceUnderfiilUtils.SetCoefCapRadiusUnderFill(s.CoefCapRadiusUnderFill);
            _sourceUnderfiilUtils.SetRectHeightCoef(s.RectHeightCoef);
            _sourceUnderfiilUtils.SetInnerOffset(s.InnerOffset);

            _sourceUnderfiilUtils.SetUnderfillRectWidth(s.UnderfillRectWidth);

            _sourceUnderfiilUtils.SetCorrugationsCountForUnderFill(s.CorrugationsCountForUnderFill);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void underfillSettings_cancelBt_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region Очистка

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            _image?.Dispose();
        }

        #endregion
    }
}
