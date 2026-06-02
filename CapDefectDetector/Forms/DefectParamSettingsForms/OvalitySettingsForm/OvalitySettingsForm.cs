using System;
using System.Windows.Forms;
using CapDefectDetector.DTO.CapRecipe;
using CapDefectDetector.DTO.DefectSettings;
using CapDefectDetector.ImageProcessing.Utils;
using CapDefectDetector.ImageProcessing.Utils.ContourProcessor;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.Forms.DefectParamSettingsForms.OvalitySettingsForm
{
    public partial class OvalitySettingsForm : Form
    {
        #region Объекты классов
        private Mat _image;
        private readonly CapRecipe _recipe;

        private readonly CapOvalityDefectUtils _sourceOvalityUtils;
        private readonly CapOvalityDefectUtils _editableOvalityUtils;
        private readonly CapContourUtils _capContourUtils;
        #endregion

        #region Глобальные поля
        private Point[] _contour;
        private bool _isOval;
        #endregion

        public  OvalitySettingsForm(Mat image, CapOvalityDefectUtils ovalityUtils, CapContourUtils capContourUtils)
        {
            InitializeComponent();

            _image = image?.Clone();

            _sourceOvalityUtils = ovalityUtils;
            _editableOvalityUtils = new CapOvalityDefectUtils(ovalityUtils.GetSettings());
            _capContourUtils = capContourUtils;

            ApplySettingsToUI();
            RunPipeline();
        }

        #region Визуализация
        private void ApplySettingsToUI()
        {
            var s = _editableOvalityUtils.GetSettings();
            ovalitySettings_ovalityCoefNumUd.Value = (decimal)s.OvalityThreshold;
        }

        private void UpdateVisualization(Mat draw)
        {
            ovalitySettings_ovalityOriginPb.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(_image.Clone());

            ovalitySettings_ovalityCoefPb.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(draw.Clone());

            ovalitySettings_ovalityResultPb.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(draw);

            ovalitySettings_resultLabel.Text = _isOval ? "NG" : "OK";
            ovalitySettings_resultLabel.ForeColor = _isOval ? System.Drawing.Color.Red : System.Drawing.Color.Green;
        }

        #endregion

        #region Pipline овальности
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

            _isOval = RunOvality(draw);

            UpdateVisualization(draw);
        }

        private bool RunOvality(Mat drawFrame)
        {
            if (_contour == null || _contour.Length < 5)
                return false;

            return _editableOvalityUtils.CheckOvality(null,_image,drawFrame,CancellationToken.None,_contour);
        }

        #endregion

        #region Обработчики событий при взаимодействии с интерфейсом
        private void ovalitySettings_ovalityCoefNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!ovalitySettings_ovalityCoefNumUd.Focused)
                return;

            _editableOvalityUtils.SetThreshold((double)ovalitySettings_ovalityCoefNumUd.Value);

            RunPipeline();
        }
        #endregion

        #region Обработчики кнопок
        private void ovalitySettings_okBt_Click(object sender,EventArgs e)
        {
            var settings = _editableOvalityUtils.GetSettings();

            _sourceOvalityUtils.SetThreshold(settings.OvalityThreshold);

            DialogResult = DialogResult.OK;

            Close();
        }

        private void ovalitySettings_cancelBt_Click(object sender,EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

            Close();
        }

        private void ovalitySettings_loadImageForOvalitySettingsBt_Click(object sender,EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter ="Image Files|*.bmp;*.jpg;*.jpeg;*.png";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            _image?.Dispose();

            _image = Cv2.ImRead(dialog.FileName);

            RunPipeline();
        }
        #endregion

        #region Очистка
        private void OvalitySettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _image?.Dispose();

            ovalitySettings_ovalityOriginPb?.Image?.Dispose();
            ovalitySettings_ovalityCoefPb?.Image?.Dispose();
            ovalitySettings_ovalityResultPb?.Image?.Dispose();
        }
        #endregion
    }
}