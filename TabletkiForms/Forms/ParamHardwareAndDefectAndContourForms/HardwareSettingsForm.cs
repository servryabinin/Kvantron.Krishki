using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Windows.Forms;
using EasyModbus;
using KrishkiForms.CameraAndModbusClasses;

namespace KrishkiForms.Forms.ParamHardwareAndDefectAndContourForms
{
    public partial class HardwareSettingsForm : Form
    {
        //Объекты камеры и ПР205
        private ModbusTCP modbusClient;
        private HikCamera cam;

        //Параметры для настройки камеры и отбраковщика
        private int breakingTimeRegister = 16466;
        private int cameraOffsetRegister = 16402;
        private int breakerOffsetRegister = 16399;
        private int breakerAllowRegister = 16401;
        private int startRecognizeProcessing = 16400;
        private int BreakingTime = 55;
        private int CameraOffset = 300;
        private int BreakerOffset = 2430;
        private int BreakerAllowTrue = 1;
        private int BreakerAllowFalse = 0;
        private int RecognizeProcessingAndBreakerAllowFinish = 0;

        // Пути и файлы
        private string settingsFilePath;
        private string ovalityDefectPath;
        private string paintDefectPath;
        private string inclusionDefectPath;
        private string obloyDefectPath;
        private string fileNameForOvalityDefect = $"ovality_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bmp";
        private string fileNameForPaintDefect = $"paint_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bmp";
        private string fileNameForInclusionDefect = $"inclusion_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bmp";
        private string fileNameForObloyDefect = $"obloy_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bmp";
        private string fullPathForOvalityDefect = "";
        private string fullPathForPaintDefect = "";
        private string fullPathForInclusionDefect = "";
        private string fullPathForObloyDefect = "";

        //Статусы подклчючения камеры и ПР205
        private bool cameraConnected = false;
        private bool prConnected = false;

        //Разрешение получения стрим
        bool startStreamEnable = false;

        //Многопоточность
        private CancellationTokenSource cts;

        //Цвета для визулизации
        private Color connectedColor = Color.FromArgb(229, 115, 115); // красный — отключить
        private Color disconnectedColor = Color.FromArgb(4, 85, 191); // синий — подключить


        public HardwareSettingsForm(
            ModbusTCP modbusClient,
            HikCamera cam,
            bool prConnected,
            bool cameraConnected)
        {
            InitializeComponent();

            this.modbusClient = modbusClient;
            this.cam = cam;
            this.cameraConnected = cameraConnected;
            this.prConnected = prConnected;

            InitializeHardwareStatusUI();
        }

        private void InitializeHardwareStatusUI()
        {
            // --- ПР205 ---
            if (modbusClient != null && modbusClient.Connected)
            {
                prStatus.Text = "Подключено";
                prStatus.ForeColor = Color.Green;
                connectPrButton.Text = "Отключиться от ПР";
                connectPrButton.BackColor = connectedColor;
            }
            else
            {
                prStatus.Text = "Не подключено";
                prStatus.ForeColor = Color.Red;
                connectPrButton.Text = "Подключиться к ПР";
                connectPrButton.BackColor = disconnectedColor;
            }

            // --- Камера ---
            if (cam != null && cam.Connected)
            {
                connectCameraButton.Text = "Отключиться";
                connectCameraButton.BackColor = connectedColor;
            }
            else
            {
                connectCameraButton.Text = "Подключиться";
                connectCameraButton.BackColor = disconnectedColor;
            }
        }


        #region Кнопки для подключения к камере и ПР205
        private void connectPrButton_Click(object sender, EventArgs e)
        {

            if (prConnected && modbusClient != null && modbusClient.Connected)
            {
                try
                {
                    modbusClient.Disconnect();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отключении от ПР205: {ex.Message}");
                }

                prConnected = false;
                connectPrButton.Text = "Подключиться к ПР";
                connectPrButton.BackColor = disconnectedColor;

                prStatus.Text = "Не подключено";
                prStatus.ForeColor = Color.Red;
                return;
            }

            try
            {
                string ip = prIpTextBox.Text.Trim();
                int port = int.Parse(pr205PortTb.Text.Trim());
                modbusClient = new ModbusTCP(ip, port);
                modbusClient.Connect();

                if (modbusClient.Connected)
                {
                    prConnected = true;
                    MessageBox.Show("Modbus подключение к ПР205 установлено.");

                    prStatus.Text = "Подключено";
                    prStatus.ForeColor = Color.Green;

                    connectPrButton.Text = "Отключиться от ПР";
                    connectPrButton.BackColor = connectedColor;
                }
                else
                {
                    prConnected = false;
                    MessageBox.Show("Не удалось подключиться к ПР205.");

                    prStatus.Text = "Не подключено";
                    prStatus.ForeColor = Color.Red;

                    connectPrButton.Text = "Подключиться к ПР";
                    connectPrButton.BackColor = disconnectedColor;
                }
            }
            catch (Exception ex)
            {
                prConnected = false;
                MessageBox.Show($"Ошибка подключения к ПР205: {ex.Message}");

                prStatus.Text = "Не подключено";
                prStatus.ForeColor = Color.Red;

                connectPrButton.Text = "Подключиться к ПР";
                connectPrButton.BackColor = disconnectedColor;
            }
        }

        private void connectCameraButton_Click(object sender, EventArgs e)
        {
            if (cameraConnected)
            {
                cam.Close();
                cameraConnected = false;
                connectCameraButton.Text = "Подключиться";
                connectCameraButton.BackColor = disconnectedColor;

                camStatus.Text = "Не подключено";
                camStatus.ForeColor = Color.Red;
            }
            else
            {
                if (cam.Open())
                {
                    cameraConnected = true;
                    connectCameraButton.Text = "Отключиться";
                    connectCameraButton.BackColor = connectedColor;

                    camStatus.Text = "Подключено";
                    camStatus.ForeColor = Color.Green;

                    startStreamEnable = true;
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к камере.");
                    camStatus.Text = "Не подключено";
                    camStatus.ForeColor = Color.Red;

                    cameraConnected = false;
                    connectCameraButton.Text = "Подключиться";
                    connectCameraButton.BackColor = disconnectedColor;

                    startStreamEnable = false;
                }
            }
        }
        #endregion

        #region Сохранение и загрузка настроек ПР205
        private void savePrSettings_Click(object sender, EventArgs e)
        {
            if (!ValidatePrSettings())
                return;

            try
            {
                var settings = new
                {
                    IPAddress = prIpTextBox.Text,
                    Port = pr205PortTb.Text,
                    BreakingTime = breakingTimeTb.Text,
                    CameraOffset = cameraOffsetTb.Text,
                    BreakerOffset = breakerOffsetTb.Text
                };

                string json = System.Text.Json.JsonSerializer.Serialize(settings,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    saveFileDialog.Title = "Сохранить настройки ПР205";
                    saveFileDialog.FileName = "pr205_settings.json";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveFileDialog.FileName, json);

                        Properties.Settings.Default.BreakingTime = breakingTimeTb.Text;
                        Properties.Settings.Default.CameraOffset = cameraOffsetTb.Text;
                        Properties.Settings.Default.BreakerOffset = breakerOffsetTb.Text;
                        Properties.Settings.Default.IpAdressPr = prIpTextBox.Text;
                        Properties.Settings.Default.PortPr = pr205PortTb.Text;

                        // сохраняем изменения в Settings
                        Properties.Settings.Default.Save();

                        MessageBox.Show("Настройки ПР205 успешно сохранены.", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении настроек ПР205: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadPrSettings_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    openFileDialog.Title = "Загрузить настройки ПР205";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string json = File.ReadAllText(openFileDialog.FileName);
                        var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        if (settings != null)
                        {
                            // Загружаем IP адрес
                            if (settings.ContainsKey("IPAddress"))
                                prIpTextBox.Text = settings["IPAddress"];
                            else
                                prIpTextBox.Text = "10.10.69.38"; // значение по умолчанию

                            // Загружаем порт
                            if (settings.ContainsKey("Port"))
                                pr205PortTb.Text = settings["Port"];
                            else
                                pr205PortTb.Text = "502"; // значение по умолчанию

                            // Загружаем задержку
                            if (settings.ContainsKey("BreakingTime"))
                                breakingTimeTb.Text = settings["BreakingTime"];
                            else
                                breakingTimeTb.Text = "15"; // значение по умолчанию

                            // Загружаем задержку
                            if (settings.ContainsKey("CameraOffset"))
                                cameraOffsetTb.Text = settings["CameraOffset"];
                            else
                                cameraOffsetTb.Text = "300"; // значение по умолчанию

                            // Загружаем задержку
                            if (settings.ContainsKey("BreakerOffset"))
                                breakerOffsetTb.Text = settings["BreakerOffset"];
                            else
                                breakerOffsetTb.Text = "2430"; // значение по умолчанию

                            MessageBox.Show("Настройки ПР205 успешно загружены.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось прочитать настройки из файла.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке настроек ПР205: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidatePrSettings()
        {
            // Проверка IP адреса
            if (!System.Net.IPAddress.TryParse(prIpTextBox.Text, out _))
            {
                MessageBox.Show("Неверный формат IP адреса", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка порта
            if (!int.TryParse(pr205PortTb.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Порт должен быть числом от 1 до 65535", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка задержки
            if (!int.TryParse(breakingTimeTb.Text, out int delay) || delay < 0)
            {
                MessageBox.Show("Задержка должна быть положительным числом", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка порта
            if (!int.TryParse(cameraOffsetTb.Text, out int cameraOffset) || cameraOffset < 0)
            {
                MessageBox.Show("Расстояние от датчика до камеры должно быть положительным числом", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка задержки
            if (!int.TryParse(breakerOffsetTb.Text, out int breakerOffset) || breakerOffset < 0)
            {
                MessageBox.Show("Расстояние от датчика до отбраковщика должно быть положительным числом", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
        #endregion

        #region Настройка параметров отбраковки
        private async void applyPrBreakerParamButton_Click(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();

            try
            {
                // BreakingTime
                if (!int.TryParse(breakingTimeTb.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out BreakingTime) || BreakingTime <= 0)
                    BreakingTime = 55;
                await Task.Run(() => SendBreakingTime(BreakingTime), cts.Token);

                // CameraOffset
                if (!int.TryParse(cameraOffsetTb.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out CameraOffset) || CameraOffset <= 0)
                    CameraOffset = 300;
                await Task.Run(() => SendCameraOffset(CameraOffset), cts.Token);

                // BreakerOffset
                if (!int.TryParse(breakerOffsetTb.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out BreakerOffset) || BreakerOffset <= 0)
                    BreakerOffset = 2430;
                await Task.Run(() => SendBreakerOffset(BreakerOffset), cts.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке параметров: {ex.Message}");
            }
        }

        private void SendBreakingTime(int breakingTime)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteSingleRegisterForDelayBreakerAndCameraOffset(breakingTimeRegister, breakingTime);
                }
            }
            catch (TaskCanceledException)
            {
                // отмена - ничего страшного
            }
            catch (Exception ex)
            {
                // TODO Логирование ошибки
            }
        }

        private void SendCameraOffset(int cameraOffset)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteSingleRegisterForDelayBreakerAndCameraOffset(cameraOffsetRegister, cameraOffset);
                }
            }
            catch (TaskCanceledException)
            {
                // отмена - ничего страшного
            }
            catch (Exception ex)
            {
                // TODO Логирование ошибки
            }
        }
        private void SendBreakerOffset(int breakerOffset)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteSingleRegisterForDelayBreakerAndCameraOffset(breakerOffsetRegister, breakerOffset);
                }
            }
            catch (TaskCanceledException)
            {
                // отмена - ничего страшного
            }
            catch (Exception ex)
            {
                // TODO Логирование ошибки
            }
        }

        private void breakingAllowCb_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    int valueToSend = breakingAllowCb.Checked
                        ? (int)BreakerAllowTrue
                        : (int)BreakerAllowFalse;

                    modbusClient.WriteSingleRegisterForBreaker(breakerAllowRegister, valueToSend);
                }
                else
                {
                    MessageBox.Show("Нет подключения к ПР205. Сигнал не отправлен.",
                        "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке сигнала на ПР205: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Применение настроек камеры
        private void applySettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (cam == null || !cam.Connected)
                {
                    MessageBox.Show("Камера не подключена", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!uint.TryParse(widthTb.Text, out uint width) ||
                    !uint.TryParse(heightTb.Text, out uint height) ||
                    !uint.TryParse(exposureTb.Text, out uint exposure) ||
                    !uint.TryParse(gainTb.Text, out uint gain))
                {
                    MessageBox.Show("Некорректные параметры. Убедитесь, что введены только числа.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //StopStream();

                cam.Width = width;
                cam.Height = height;
                cam.ExposureTime = exposure;
                cam.Gain = gain;

                cam.SetHeight();
                cam.SetWidth();
                cam.SetGain();
                cam.SetExposureTime();

                //StartStream();

                MessageBox.Show("Параметры успешно применены.",
                    "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка применения параметров:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        #endregion

        #region Сохранение настроек камеры
        private void loadSettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    openFileDialog.Title = "Загрузить настройки";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string json = File.ReadAllText(openFileDialog.FileName);
                        var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        if (settings != null)
                        {
                            widthTb.Text = settings.ContainsKey("Width") ? settings["Width"] : "500";
                            heightTb.Text = settings.ContainsKey("Height") ? settings["Height"] : "532";
                            exposureTb.Text = settings.ContainsKey("Exposure") ? settings["Exposure"] : "450";
                            gainTb.Text = settings.ContainsKey("Gain") ? settings["Gain"] : "3,01";

                            MessageBox.Show("Настройки успешно загружены.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось прочитать настройки из файла.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке настроек: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveSettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Простая валидация
                if (string.IsNullOrWhiteSpace(widthTb.Text) ||
                    string.IsNullOrWhiteSpace(heightTb.Text) ||
                    string.IsNullOrWhiteSpace(exposureTb.Text) ||
                    string.IsNullOrWhiteSpace(gainTb.Text))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля перед сохранением.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var settings = new
                {
                    Width = widthTb.Text,
                    Height = heightTb.Text,
                    Exposure = exposureTb.Text,
                    Gain = gainTb.Text
                };

                string json = System.Text.Json.JsonSerializer.Serialize(settings,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    saveFileDialog.Title = "Сохранить настройки";
                    saveFileDialog.FileName = "settings.json";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveFileDialog.FileName, json);

                        // ✅ Сохраняем также в Settings
                        Properties.Settings.Default.WidthFrame = widthTb.Text;
                        Properties.Settings.Default.HeightFrame = heightTb.Text;
                        Properties.Settings.Default.ExposureFrame = exposureTb.Text;
                        Properties.Settings.Default.GainFrame = gainTb.Text;
                        Properties.Settings.Default.Save();

                        MessageBox.Show("Настройки успешно сохранены.", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении настроек: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Настройка путей и папок
        private void browseOriginalButton_Click(object sender, EventArgs e)
        {
            // Если нужна папка для оригинальных изображений
            SelectFolder("Выберите папку для сохранения оригинальных изображений", ref settingsFilePath);
        }

        private void browseOvalityButton_Click(object sender, EventArgs e)
        {
            SelectFolder("Выберите папку для сохранения изображений с овальностью", ref ovalityDefectPath);

            // Обновляем TextBox если он есть
            if (ovalityPathTextBox != null)
                ovalityPathTextBox.Text = ovalityDefectPath;
        }

        private void browseInclusionButton_Click(object sender, EventArgs e)
        {
            SelectFolder("Выберите папку для сохранения изображений с вкраплениями", ref inclusionDefectPath);

            // Обновляем TextBox если он есть
            if (inclusionPathTextBox != null)
                inclusionPathTextBox.Text = inclusionDefectPath;
        }

        private void browseInpaintButton_Click(object sender, EventArgs e)
        {
            SelectFolder("Выберите папку для сохранения изображений с непрокрасом", ref paintDefectPath);

            // Обновляем TextBox если он есть
            if (inpaintPathTextBox != null)
                inpaintPathTextBox.Text = paintDefectPath;
        }

        private void browseObloyButton_Click(object sender, EventArgs e)
        {
            SelectFolder("Выберите папку для сохранения изображений с облоем", ref obloyDefectPath);

            // Обновляем TextBox если он есть
            if (obloyPathTextBox != null)
                obloyPathTextBox.Text = obloyDefectPath;
        }

        private void SelectFolder(string description, ref string pathVariable)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = description;
                folderDialog.ShowNewFolderButton = true; // Разрешить создание новых папок

                // Устанавливаем начальную папку, если путь уже существует
                if (!string.IsNullOrEmpty(pathVariable) && Directory.Exists(pathVariable))
                {
                    folderDialog.SelectedPath = pathVariable;
                }
                else
                {
                    // Или папка "Мои документы" по умолчанию
                    folderDialog.RootFolder = Environment.SpecialFolder.MyDocuments;
                }

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    pathVariable = folderDialog.SelectedPath;
                    MessageBox.Show($"Выбран путь: {pathVariable}", "Путь сохранен",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        #endregion
    }

}
