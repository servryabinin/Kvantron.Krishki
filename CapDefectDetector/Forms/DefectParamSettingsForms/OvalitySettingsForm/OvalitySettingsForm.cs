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
        private readonly CapColorContourUtils _colorUtils;
        private readonly CapBlackOrBrownContourUtils _blackOrBrownUtils;
        #endregion

        #region Глобальные поля
        private Point[] _contour;
        private RotatedRect _ellipse;
        private float _axisRatio;
        private bool _isOval;
        private Mat _element1;
        private Mat _element2;
        #endregion

        public OvalitySettingsForm(Mat image, CapRecipe recipe, CapOvalityDefectUtils ovalityUtils, Mat element1, Mat element2)
        {
            InitializeComponent();

            _image = image?.Clone();
            _recipe = recipe;

            _sourceOvalityUtils = ovalityUtils;
            _editableOvalityUtils = new CapOvalityDefectUtils(ovalityUtils.GetSettings());

            _element1 = element1;
            _element2 = element2;

            _colorUtils = new CapColorContourUtils();
            _blackOrBrownUtils = new CapBlackOrBrownContourUtils();

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

            var result = GetCapContour(gray, _image);
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

        #region Нахождение контура
        private CapContourResult GetCapContour(Mat gray, Mat image)
        {
            if (_recipe.IsBlackOrBrown)
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
            Mat sat = _colorUtils.ApplySaturationStep(image, _recipe.CameraSaturation);
            Mat caps = _colorUtils.ApplyCapsColorStep(sat, _recipe.CapsColor, _recipe.IsColored, _recipe.IsYellow, _recipe.IsGreen);
            Mat[] channels = _colorUtils.ApplyWindowStep(caps, _recipe.Window);
            _colorUtils.ApplyMorphologyStep(channels, _element1, _element2);
            var contour = _colorUtils.GetMaxContour(channels[2]);
            contour = _colorUtils.CorrectContour(contour, _recipe.ContourCorrectionColor);

            return new CapContourResult
            {
                Contour = contour,
                Blur1 = channels[1],
                Blur2 = channels[2],
                Mask = caps
            };
        }

        private CapContourResult GetBlackOrBrownContour(Mat gray, Mat image)
        {
            Mat sat = _blackOrBrownUtils.ApplySaturationStep(image, _recipe.CameraSaturationBlackOrBrown);
            Mat g = _blackOrBrownUtils.ApplyGrayStep(sat);
            Mat blurred = _blackOrBrownUtils.ApplyMedianStep(g, _recipe.MedianFilter);
            Mat edges = _blackOrBrownUtils.ApplyCannyStep(blurred, _recipe.CannyThreshold);
            var contour = _blackOrBrownUtils.ApplyEllipseStep(edges);
            contour = _blackOrBrownUtils.CorrectContour(contour, _recipe.ContourCorrectionBlackOrBrown);

            return new CapContourResult
            {
                Contour = contour,
                Blur1 = blurred,
                Blur2 = edges,
                Mask = edges
            };
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