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
using CapDefectDetector.Domain;
using CapDefectDetector.DTO.CapRecipe;
using CapDefectDetector.ImageProcessing.Utils;
using CapDefectDetector.ImageProcessing.Utils.ContourProcessor;
using CapDefectDetector.Logger;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.Forms.DefectParamSettingsForms.ObloySettingsForm
{
    public partial class ObloySettingsForm : Form
    {
        #region Объекты классов
        private Mat _image;
        private readonly CapRecipe _recipe;

        private readonly CapObloyDefectUtils _sourceObloyUtils;
        private readonly CapObloyDefectUtils _editableObloyUtils;
        private readonly CapColorContourUtils _colorUtils;
        private readonly CapBlackOrBrownContourUtils _blackOrBrownUtils;
        #endregion

        #region Глобальные поля
        private Point[] _contour;
        private Mat _blurChannel1;
        private Mat _blurChannel2;
        private Mat _capRadiusMask;
        private bool _isObloy;
        private Mat _element1;
        private Mat _element2;
        #endregion

        public ObloySettingsForm(Mat image, CapRecipe recipe, CapObloyDefectUtils obloyUtils, Mat element1, Mat element2)
        {
            InitializeComponent();

            _image = image?.Clone();
            _recipe = recipe;

            _sourceObloyUtils = obloyUtils;
            _editableObloyUtils = new CapObloyDefectUtils(obloyUtils.GetSettings());

            _element1 = element1;
            _element2 = element2;

            _colorUtils = new CapColorContourUtils();
            _blackOrBrownUtils = new CapBlackOrBrownContourUtils();

            BindEnums();
            ApplySettingsToUI();
            RunPipeline();
        }

        #region Визуализация
        private void BindEnums()
        {
            obloySettings_obloyMorphTypeCb.Items.Clear();
            obloySettings_obloyMorphTypeCb.Items.AddRange(Enum.GetNames(typeof(MorphTypes)));

            obloySettings_obloyMorphShapeCb.Items.Clear();
            obloySettings_obloyMorphShapeCb.Items.AddRange(Enum.GetNames(typeof(MorphShapes)));
        }

        private void ApplySettingsToUI()
        {
            var s = _editableObloyUtils.GetSettings();

            obloySettings_capFlashOffsetNumUd.Value = (decimal)s.CapFlashOffset;
            obloySettings_obloyMorphTypeCb.SelectedItem = s.MorphType.ToString();
            obloySettings_obloyKernelSizeNumUd.Value = (decimal)s.KernelSize;
            obloySettings_obloyMorphShapeCb.SelectedItem = s.MorphShape.ToString();
            obloySettings_obloyMorphIterNumUd.Value = (decimal)s.MorphIterations;
            obloySettings_minAreaObloyNumUd.Value = (decimal)s.MinAreaObloy;

        }

        private void UpdateVisualization()
        {
            obloySettings_resultLabel.Text = _isObloy ? "NG" : "OK";
            obloySettings_resultLabel.ForeColor = _isObloy ? System.Drawing.Color.Red : System.Drawing.Color.Green;
        }
        #endregion

        #region Pipeline облоя

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
            _blurChannel1 = result.Blur1;
            _blurChannel2 = result.Blur2;

            Mat draw = _image.Clone();

            _isObloy = RunObloy(draw);

            UpdateVisualization();
        }

        private bool RunObloy(Mat drawFrame)
        {
            if (_contour == null || _contour.Length < 5)
                return false;

            try
            {
                obloySettings_obloyOriginalPb.Image = BitmapConverter.ToBitmap(drawFrame);

                var center = _editableObloyUtils.ComputeCenter(_contour);
                var radius = _editableObloyUtils.ComputeRadius(_contour, center);

                Mat capMask = new Mat(drawFrame.Size(), MatType.CV_8UC1);
                _editableObloyUtils.CreateCapMask(capMask, drawFrame, center, radius);

                Mat capOverlay = CreateCapOverlay(drawFrame, capMask);
                obloySettings_obloyCapMaskPb.Image = BitmapConverter.ToBitmap(capOverlay);
                obloySettings_obloyBlurInputPb.Image = BitmapConverter.ToBitmap(_blurChannel2);

                Mat andResult = _editableObloyUtils.ApplyCapMask(capMask, _blurChannel2);
                obloySettings_obloyAfterAndPb.Image = BitmapConverter.ToBitmap(andResult);

                Mat morph = _editableObloyUtils.ApplyMorphology(andResult);
                obloySettings_obloyMorphPb.Image =BitmapConverter.ToBitmap(morph);

                var (contours, pixCount) = _editableObloyUtils.Analyze(morph);
                _isObloy = _editableObloyUtils.isObloyExist(pixCount);
                Mat contourView = drawFrame.Clone();
                if (_isObloy)
                {
                    _editableObloyUtils.DrawIfNeeded(_isObloy, contourView, contours);
                }
                obloySettings_obloyContoursPb.Image =BitmapConverter.ToBitmap(contourView);

                Mat result = contourView.Clone();
                obloySettings_obloyResultPb.Image = BitmapConverter.ToBitmap(result);

                return _isObloy;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "RunObloy pipeline error");
                return false;
            }
        }

        private Mat CreateCapOverlay(Mat image, Mat mask)
        {
            Mat overlay = image.Clone();

            using var blue = new Mat(image.Size(), MatType.CV_8UC3);
            blue.SetTo(new Scalar(255, 0, 0));

            using var mask8u = mask.Clone();

            Cv2.Threshold(mask8u, mask8u, 127, 255, ThresholdTypes.Binary);

            blue.CopyTo(overlay, mask8u);

            Cv2.AddWeighted(image, 1.0, overlay, 0.35, 0, overlay);

            return overlay;
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

        private void obloySettings_capFlashOffsetNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!obloySettings_capFlashOffsetNumUd.Focused)
                return;

            _editableObloyUtils.SetCapFlashOffset((double)obloySettings_capFlashOffsetNumUd.Value);

            RunPipeline();
        }

        private void obloySettings_obloyMorphTypeCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!obloySettings_obloyMorphTypeCb.Focused)
                return;

            _editableObloyUtils.SetMorphType((MorphTypes)obloySettings_obloyMorphTypeCb.SelectedIndex);

            RunPipeline();
        }

        private void obloySettings_obloyKernelSizeNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!obloySettings_obloyKernelSizeNumUd.Focused)
                return;

            _editableObloyUtils.SetKernelSize((int)obloySettings_obloyKernelSizeNumUd.Value);

            RunPipeline();
        }

        private void obloySettings_obloyMorphShapeCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!obloySettings_obloyMorphShapeCb.Focused)
                return;

            _editableObloyUtils.SetMorphShape((MorphShapes)obloySettings_obloyMorphShapeCb.SelectedIndex);

            RunPipeline();
        }

        private void obloySettings_obloyMorphIterNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!obloySettings_obloyMorphIterNumUd.Focused)
                return;

            _editableObloyUtils.SetMorphIterations((int)obloySettings_obloyMorphIterNumUd.Value);

            RunPipeline();
        }

        private void obloySettings_minAreaObloyNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!obloySettings_minAreaObloyNumUd.Focused)
                return;

            _editableObloyUtils.SetMinAreaObloy((double)obloySettings_minAreaObloyNumUd.Value);

            RunPipeline();
        }

        #endregion

        #region Обработчики кнопок
        private void obloySettings_loadImageForObloySettingsBt_Click(object sender, EventArgs e)
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

        private void obloySettings_okBt_Click(object sender, EventArgs e)
        {
            var s = _editableObloyUtils.GetSettings();

            _sourceObloyUtils.SetCapFlashOffset(s.CapFlashOffset);
            _sourceObloyUtils.SetMorphShape(s.MorphShape);
            _sourceObloyUtils.SetMorphType(s.MorphType);
            _sourceObloyUtils.SetKernelSize(s.KernelSize);
            _sourceObloyUtils.SetMorphIterations(s.MorphIterations);
            _sourceObloyUtils.SetMinAreaObloy(s.MinAreaObloy);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void obloySettings_cancelBt_Click(object sender, EventArgs e)
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
