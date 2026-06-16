using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using CapDefectDetector.DTO.CapRecipe;
using CapDefectDetector.ImageProcessing.Utils;
using CapDefectDetector.ImageProcessing.Utils.ContourProcessor;
using CapDefectDetector.Logger;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.Forms.DefectParamSettingsForms.InpaintSettingsForm
{
    public partial class InpaintSettingsForm : Form
    {
        #region Объекты классов
        private Mat _image;

        private readonly CapPaintDefectUtils _sourcePaintUtils;
        private readonly CapPaintDefectUtils _editablePaintUtils;
        private readonly CapContourUtils _capContourUtils;
        #endregion

        #region Глобальные поля
        private Point[] _contour;
        private bool _isPaint;
        #endregion


        public InpaintSettingsForm(Mat image, CapPaintDefectUtils paintUtils, CapContourUtils capContourUtils)
        {
            InitializeComponent();

            _image = image?.Clone();

            _sourcePaintUtils = paintUtils;
            _editablePaintUtils = new CapPaintDefectUtils(paintUtils.GetSettings());
            _capContourUtils = capContourUtils;

            ApplySettingsToUI();
            RunPipeline();
        }

        #region Визуализация
        private void ApplySettingsToUI()
        {
            var s = _editablePaintUtils.GetSettings();
            inpaintSettings_sMinNumUd.Value = (decimal)s.SMin;
            inpaintSettings_sMaxNumUd.Value = (decimal)s.SMax;
            inpaintSettings_vMinNumUd.Value = (decimal)s.VMin;
            inpaintSettings_vMaxNumUd.Value = (decimal)s.VMax;
            inpaintSettings_inpaintMinAreaNumUd.Value = (decimal)s.MinAreaInpaintDefect;
            inpaintSettings_inpaintWhiteThresholdNumUd.Value = (decimal)s.MinInpaintWhiteThreshold;
        }

        private void UpdateVisualization()
        {
            inpaintSettings_resultLabel.Text = _isPaint ? "NG" : "OK";
            inpaintSettings_resultLabel.ForeColor = _isPaint ? System.Drawing.Color.Red : System.Drawing.Color.Green;
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

            _isPaint = RunInpaint(draw);

            UpdateVisualization();

        }

        private bool RunInpaint(Mat drawFrame)
        {
            if (_contour == null || _contour.Length < 5)
                return false;

            try
            {
                inpaintSettings_inpaintOriginPb.Image = BitmapConverter.ToBitmap(drawFrame);

                Mat whiteMask = _editablePaintUtils.CreateWhiteMaskStep(drawFrame, _contour);
                inpaintSettings_paintWhiteMaskPb.Image =BitmapConverter.ToBitmap(whiteMask);

                Mat maskedDefects = _editablePaintUtils.CreateMaskedDefectsStep(whiteMask,drawFrame,_contour);
                inpaintSettings_paintMaskedDefectsPb.Image =BitmapConverter.ToBitmap(maskedDefects);

                Mat contoursView = drawFrame.Clone();
                _isPaint = _editablePaintUtils.DrawPaintDefectContoursStep(maskedDefects, drawFrame,contoursView, CancellationToken.None);
                inpaintSettings_paintContoursPb.Image =BitmapConverter.ToBitmap(contoursView);

                Mat resultView = drawFrame.Clone();
                inpaintSettings_inpaintResultPb.Image = BitmapConverter.ToBitmap(resultView);

                return _isPaint;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "RunInpaint visualization pipeline error");
                return false;
            }
        }
        #endregion

        #region Обработчики событий при взаимодействии с интерфейсом
        private void inpaintSettings_sMinNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inpaintSettings_sMinNumUd.Focused)
                return;

            _editablePaintUtils.SetSMin((double)inpaintSettings_sMinNumUd.Value);

            RunPipeline();
        }

        private void inpaintSettings_sMaxNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inpaintSettings_sMaxNumUd.Focused)
                return;

            _editablePaintUtils.SetSMax((double)inpaintSettings_sMaxNumUd.Value);

            RunPipeline();
        }

        private void inpaintSettings_vMinNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inpaintSettings_vMinNumUd.Focused)
                return;

            _editablePaintUtils.SetVMin((double)inpaintSettings_vMinNumUd.Value);

            RunPipeline();
        }

        private void inpaintSettings_vMaxNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inpaintSettings_vMaxNumUd.Focused)
                return;

            _editablePaintUtils.SetVMax((double)inpaintSettings_vMaxNumUd.Value);

            RunPipeline();
        }

        private void inpaintSettings_inpaintMinAreaNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inpaintSettings_inpaintMinAreaNumUd.Focused)
                return;

            _editablePaintUtils.SetMinAreaInpaintDefect((double)inpaintSettings_inpaintMinAreaNumUd.Value);

            RunPipeline();
        }

        private void inpaintSettings_inpaintWhiteThresholdNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inpaintSettings_inpaintWhiteThresholdNumUd.Focused)
                return;

            _editablePaintUtils.SetMinInpaintWhiteThreshold(
                (double)inpaintSettings_inpaintWhiteThresholdNumUd.Value);

            RunPipeline();
        }
        #endregion

        #region Обработчики кнопок
        private void inpaintSettings_loadImageForInpaintSettingsBt_Click(object sender, EventArgs e)
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

        private void inpaintSettings_okBt_Click(object sender, EventArgs e)
        {
            var s = _editablePaintUtils.GetSettings();

            _sourcePaintUtils.SetSMin(s.SMin);
            _sourcePaintUtils.SetSMax(s.SMax);
            _sourcePaintUtils.SetVMin(s.VMin);
            _sourcePaintUtils.SetVMax(s.VMax);
            _sourcePaintUtils.SetMinAreaInpaintDefect(s.MinAreaInpaintDefect);
            _sourcePaintUtils.SetMinInpaintWhiteThreshold(s.MinInpaintWhiteThreshold);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void inpaintSettings_cancelBt_Click(object sender, EventArgs e)
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
