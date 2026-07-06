using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapDefectDetector.CameraAndModbusClasses;
using CapDefectDetector.ImageProcessing.Utils.ContourProcessor;
using OpenCvSharp;

namespace CapDefectDetector.Forms.BreakerSettingsForm
{
    public partial class BreakerSettingsForm : Form
    {
        #region Объекты классов
        private readonly ModuleIO _sourceModuleIO;
        private readonly ModuleIO _editableModuleIO;
        #endregion

        #region Вспомогательные объекты
        private int _cameraOffset = 0;
        private int _breakerOffset = 0;
        #endregion

        public BreakerSettingsForm(ModuleIO modbusClient)
        {
            InitializeComponent();

            _sourceModuleIO = modbusClient;
            _editableModuleIO = modbusClient;

            ApplySettingsToUI();
            RunPipeline();
        }

        #region Рассчет расстояний
        private void RunPipeline()
        {
            // мм на 1 тик
            breakerSettingsForm_mmOnStepTb.Text = _editableModuleIO.GetMmPerTick().ToString("F4");

            // тиков на 1 мм
            breakerSettingsForm_stepOnMmTb.Text = _editableModuleIO.GetTicksPerMm().ToString("F4");

            // камера (в тиках)
            _cameraOffset = _editableModuleIO.GetCameraOffsetTicks();
            breakerSettingsForm_cameraOffsetTb.Text = _cameraOffset.ToString();

            // отбраковщик (в тиках)
            _breakerOffset = _editableModuleIO.GetBreakerOffsetTicks();
            breakerSettingsForm_breakerOffsetTb.Text = _breakerOffset.ToString();
        }
        #endregion

        #region Визуализация
        private void ApplySettingsToUI()
        {
            var s = _editableModuleIO.GetSettings();
            breakerSettingsForm_diameterEncoderWheelNuD.Value = s.BreakerSettings.DiameterEncoderWheel;
            breakerSettingsForm_encoderBitrateNuD.Value = s.BreakerSettings.EncoderBitrate;
            breakerSettingsForm_distanceFromSensorToCameraNuD.Value = s.BreakerSettings.DistanceFromSensorToCamera;
            breakerSettingsForm_distanceFromSensorToBreakerNuD.Value = s.BreakerSettings.DistanceFromSensorToBreaker;
        }
        #endregion

        #region Обработчики событий при взаимодействии с интерфейсом
        private void breakerSettingsForm_diameterEncoderWheelNuD_ValueChanged(object sender, EventArgs e)
        {
            if (!breakerSettingsForm_diameterEncoderWheelNuD.Focused)
                return;

            _editableModuleIO.SetDiameterEncoderWheel((int)breakerSettingsForm_diameterEncoderWheelNuD.Value);

            RunPipeline();
        }

        private void breakerSettingsForm_encoderBitrateNuD_ValueChanged(object sender, EventArgs e)
        {
            if (!breakerSettingsForm_encoderBitrateNuD.Focused)
                return;

            _editableModuleIO.SetEncoderBitrate((int)breakerSettingsForm_encoderBitrateNuD.Value);

            RunPipeline();
        }

        private void breakerSettingsForm_distanceFromSensorToCameraNuD_ValueChanged(object sender, EventArgs e)
        {
            if (!breakerSettingsForm_distanceFromSensorToCameraNuD.Focused)
                return;

            _editableModuleIO.SetDistanceFromSensorToCamera((int)breakerSettingsForm_distanceFromSensorToCameraNuD.Value);

            RunPipeline();
        }

        private void breakerSettingsForm_distanceFromSensorToBreakerNuD_ValueChanged(object sender, EventArgs e)
        {
            if (!breakerSettingsForm_distanceFromSensorToBreakerNuD.Focused)
                return;

            _editableModuleIO.SetDistanceFromSensorToBreaker((int)breakerSettingsForm_distanceFromSensorToBreakerNuD.Value);

            RunPipeline();
        }
        #endregion

        #region Обрабочики кнопок
        private void breakerSettingsForm_okButton_Click(object sender, EventArgs e)
        {
            var s = _editableModuleIO.GetSettings();

            _sourceModuleIO.SetDiameterEncoderWheel(s.BreakerSettings.DiameterEncoderWheel);
            _sourceModuleIO.SetEncoderBitrate(s.BreakerSettings.EncoderBitrate);
            _sourceModuleIO.SetDistanceFromSensorToCamera(s.BreakerSettings.DistanceFromSensorToCamera);
            _sourceModuleIO.SetDistanceFromSensorToBreaker(s.BreakerSettings.DistanceFromSensorToBreaker);
            _sourceModuleIO.SetCameraOffset(_cameraOffset);
            _sourceModuleIO.SetBreakerOffset(_breakerOffset);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void breakerSettingsForm_cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        #endregion
    }
}
