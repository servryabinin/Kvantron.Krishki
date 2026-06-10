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

namespace CapDefectDetector.Forms.DefectParamSettingsForms.ObloySettingsForm
{
    public partial class ObloySettingsForm : Form
    {
        #region Объекты классов
        private Mat _image;

        private readonly CapObloyDefectUtils _sourceObloyUtils;
        private readonly CapObloyDefectUtils _editableObloyUtils;
        private readonly CapContourUtils _capContourUtils;
        #endregion

        #region Глобальные поля
        private Point[] _contour;
        private Mat _blurChannel2;
        private bool _isObloy;
        #endregion

        public ObloySettingsForm(Mat image, CapObloyDefectUtils obloyUtils, CapContourUtils capContourUtils)
        {
            InitializeComponent();

            _image = image?.Clone();

            _sourceObloyUtils = obloyUtils;
            _editableObloyUtils = new CapObloyDefectUtils(obloyUtils.GetSettings());
            _capContourUtils = capContourUtils;

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

            var result = _capContourUtils.GetCapContour(gray, _image);

            if (result?.Contour == null || result.Contour.Length < 5)
                return;

            _contour = result.Contour;
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
