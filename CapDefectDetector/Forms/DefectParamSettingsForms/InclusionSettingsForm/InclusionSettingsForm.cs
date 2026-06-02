using System;
using System.Linq;
using System.Windows.Forms;
using CapDefectDetector.Domain;
using CapDefectDetector.DTO.CapRecipe;
using CapDefectDetector.ImageProcessing.Utils;
using CapDefectDetector.ImageProcessing.Utils.ContourProcessor;
using CapDefectDetector.Logger;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.Forms.DefectParamSettingsForms.InclusionSettingsForm
{
    public partial class InclusionSettingsForm : Form
    {
        #region Объекты классов
        private Mat _image;

        private readonly CapInclusionDefectUtils _sourceInclusionUtils;
        private readonly CapInclusionDefectUtils _editableInclusionUtils;
        private readonly CapContourUtils _capContourUtils;
        #endregion

        #region Глобальные поля
        private Point[] _contour;
        private bool _isInclusion;
        #endregion

        public InclusionSettingsForm(Mat image, CapInclusionDefectUtils inclusionUtils, CapContourUtils capContourUtils)
        {
            InitializeComponent();

            _image = image?.Clone();

            _sourceInclusionUtils = inclusionUtils;
            _editableInclusionUtils = new CapInclusionDefectUtils(inclusionUtils.GetSettings());
            _capContourUtils = capContourUtils;

            BindEnums();
            ApplySettingsToUI();
            RunPipeline();
        }

        #region Визуализация
        private void BindEnums()
        {
            inclusionSettings_adaptiveThresholdTypesCb.Items.Clear();
            inclusionSettings_adaptiveThresholdTypesCb.Items.AddRange(Enum.GetNames(typeof(AdaptiveThresholdTypes)));

            inclusionSettings_thresholdTypesCb.Items.Clear();
            inclusionSettings_thresholdTypesCb.Items.AddRange(Enum.GetNames(typeof(ThresholdTypes)));

            inclusionSettings_morphTypesCb.Items.Clear();
            inclusionSettings_morphTypesCb.Items.AddRange(Enum.GetNames(typeof(MorphTypes)));

            inclusionSettings_morphShapesCb.Items.Clear();
            inclusionSettings_morphShapesCb.Items.AddRange(Enum.GetNames(typeof(MorphShapes)));
        }

        private void ApplySettingsToUI()
        {
            var s = _editableInclusionUtils.GetSettings();

            inclusionSettings_circleCoefNumUpD.Value = (decimal)s.CoefCapRadiusInclusion;
            inclusionSettings_adaptiveThresholdTypesCb.SelectedItem = s.AdaptiveType.ToString();
            inclusionSettings_thresholdTypesCb.SelectedItem = s.ThresholdType.ToString();
            inclusionSettings_blockSizeNumUd.Value = (decimal)s.BlockSize;
            inclusionSettings_cNumUd.Value = (decimal)s.C;
            inclusionSettings_morphTypesCb.SelectedItem = s.MorphType.ToString();
            inclusionSettings_morphShapesCb.SelectedItem = s.MorphShape.ToString();
            inclusionSettings_kernelSizeNumUd.Value = (decimal)s.KernelSize;
            inclusionSettings_iterationsNumUd.Value = (decimal)s.MorphIterations;
            inclusionSettings_inclusionCircleCoefNumUd.Value = (decimal)s.InclusionThreshold;
            inclusionSettings_inclusionMinSquareNumUd.Value = (decimal)s.MinAreaInclusion;
            inclusionSettings_inclusionMaxSquareNumUd.Value = (decimal)s.MaxAreaInclusion;
        }

        private void UpdateVisualization()
        {
            inclusionSettings_resultLabel.Text = _isInclusion ? "NG" : "OK";
            inclusionSettings_resultLabel.ForeColor = _isInclusion ? System.Drawing.Color.Red : System.Drawing.Color.Green;
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

            _isInclusion = RunInclusion(draw);

            UpdateVisualization();
        }

        private bool RunInclusion(Mat drawFrame)
        {
            if (_contour == null || _contour.Length < 5)
                return false;

            try
            {
                inclusionSettings_inclusionOriginalPb.Image = BitmapConverter.ToBitmap(drawFrame);

                Mat croppedView = drawFrame.Clone();
                Mat step1 = _editableInclusionUtils.DrawSearchArea(croppedView, _contour);
                inclusionSettings_inclusionCroppedPb.Image = BitmapConverter.ToBitmap(step1);

                Mat gray = new Mat();
                Cv2.CvtColor(drawFrame, gray, ColorConversionCodes.BGR2GRAY);

                Mat step2 = _editableInclusionUtils.AdaptiveBinarizeStep(gray, _contour);
                inclusionSettings_inclusionBinaryPb.Image = BitmapConverter.ToBitmap(step2);

                Mat step3 = _editableInclusionUtils.MorphologyStep(step2);
                inclusionSettings_inclusionMorphologyPb.Image = BitmapConverter.ToBitmap(step3);

                Mat contourView = drawFrame.Clone();
                _isInclusion = _editableInclusionUtils.DetectInclusionsStep(step3, contourView, CancellationToken.None);
                inclusionSettings_inclusionContoursPb.Image = BitmapConverter.ToBitmap(contourView);

                Mat resultView = drawFrame.Clone();
                inclusionSettings_inclusionResultPb.Image = BitmapConverter.ToBitmap(contourView);

                return _isInclusion;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "RunInclusion visualization pipeline error");
                return false;
            }
        }
        #endregion

        

        #region Обработчики событий при взаимодействии с интерфейсом
        private void inclusionSettings_circleCoefNumUpD_ValueChanged(object sender, EventArgs e)
        {
            if (!inclusionSettings_circleCoefNumUpD.Focused)
                return;

            _editableInclusionUtils.SetCoefCapRadiusInclusion((double)inclusionSettings_circleCoefNumUpD.Value);

            RunPipeline();
        }

        private void inclusionSettings_adaptiveThresholdTypesCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!inclusionSettings_adaptiveThresholdTypesCb.Focused)
                return;

            _editableInclusionUtils.SetAdaptiveType((AdaptiveThresholdTypes)inclusionSettings_adaptiveThresholdTypesCb.SelectedIndex);

            RunPipeline();
        }

        private void inclusionSettings_thresholdTypesCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!inclusionSettings_thresholdTypesCb.Focused)
                return;

            _editableInclusionUtils.SetThresholdType((ThresholdTypes)inclusionSettings_thresholdTypesCb.SelectedIndex);

            RunPipeline();
        }

        private void inclusionSettings_blockSizeNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inclusionSettings_blockSizeNumUd.Focused)
                return;

            _editableInclusionUtils.SetBlockSize((int)inclusionSettings_blockSizeNumUd.Value);

            RunPipeline();
        }

        private void inclusionSettings_cNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inclusionSettings_cNumUd.Focused)
                return;

            _editableInclusionUtils.SetC((double)inclusionSettings_cNumUd.Value);

            RunPipeline();
        }

        private void inclusionSettings_morphTypesCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!inclusionSettings_morphTypesCb.Focused)
                return;

            _editableInclusionUtils.SetMorphType((MorphTypes)inclusionSettings_morphTypesCb.SelectedIndex);

            RunPipeline();
        }

        private void inclusionSettings_kernelSizeNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inclusionSettings_kernelSizeNumUd.Focused)
                return;

            _editableInclusionUtils.SetKernelSize((int)inclusionSettings_kernelSizeNumUd.Value);

            RunPipeline();
        }

        private void inclusionSettings_morphShapesCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!inclusionSettings_morphShapesCb.Focused)
                return;

            _editableInclusionUtils.SetMorphShape((MorphShapes)inclusionSettings_morphShapesCb.SelectedIndex);

            RunPipeline();
        }

        private void inclusionSettings_iterationsNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inclusionSettings_iterationsNumUd.Focused)
                return;

            _editableInclusionUtils.SetMorphIterations((int)inclusionSettings_iterationsNumUd.Value);

            RunPipeline();
        }

        private void inclusionSettings_inclusionMinSquareNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inclusionSettings_inclusionMinSquareNumUd.Focused)
                return;

            _editableInclusionUtils.SetMinAreaInclusion((double)inclusionSettings_inclusionMinSquareNumUd.Value);

            RunPipeline();
        }

        private void inclusionSettings_inclusionMaxSquareNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inclusionSettings_inclusionMaxSquareNumUd.Focused)
                return;

            _editableInclusionUtils.SetMaxAreaInclusion((double)inclusionSettings_inclusionMaxSquareNumUd.Value);

            RunPipeline();
        }

        private void inclusionSettings_inclusionCircleCoefNumUd_ValueChanged(object sender, EventArgs e)
        {
            if (!inclusionSettings_inclusionCircleCoefNumUd.Focused)
                return;

            _editableInclusionUtils.SetInclusionThreshold((double)inclusionSettings_inclusionCircleCoefNumUd.Value);

            RunPipeline();
        }
        #endregion

        #region Обработчики кнопок
        private void inclusionSettings_loadImageBt_Click(object sender, EventArgs e)
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

        private void inclusionSettings_okBt_Click(object sender, EventArgs e)
        {
            var s = _editableInclusionUtils.GetSettings();

            _sourceInclusionUtils.SetCoefCapRadiusInclusion(s.CoefCapRadiusInclusion);
            _sourceInclusionUtils.SetAdaptiveType(s.AdaptiveType);
            _sourceInclusionUtils.SetThresholdType(s.ThresholdType);
            _sourceInclusionUtils.SetBlockSize(s.BlockSize);
            _sourceInclusionUtils.SetC(s.C);
            _sourceInclusionUtils.SetMorphType(s.MorphType);
            _sourceInclusionUtils.SetMorphShape(s.MorphShape);
            _sourceInclusionUtils.SetKernelSize(s.KernelSize);
            _sourceInclusionUtils.SetMorphIterations(s.MorphIterations);
            _sourceInclusionUtils.SetMinAreaInclusion(s.MinAreaInclusion);
            _sourceInclusionUtils.SetMaxAreaInclusion(s.MaxAreaInclusion);
            _sourceInclusionUtils.SetInclusionThreshold(s.InclusionThreshold);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void inclusionSettings_cancelBt_Click(object sender, EventArgs e)
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