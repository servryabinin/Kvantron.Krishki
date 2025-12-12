using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using KrishkiForms.CameraAndModbusClasses;

namespace KrishkiForms
{
    public partial class InitializeForm : Form
    {
        // Публичные свойства для передачи в основную форму
        public HikCamera Camera { get; private set; }
        public ModbusTCP ModbusClient { get; private set; }
        public bool CameraConnected { get; private set; }
        public bool ModbusConnected { get; private set; }

        private System.Windows.Forms.Timer initializationTimer;
        private int initializationStep = 0;

        // Словарь ошибок камеры
        private static readonly Dictionary<uint, string> CAMERA_ERRORS = new Dictionary<uint, string>
        {
            { 2147484163u, "Нет доступа - камера уже используется другим приложением" },
            { 2147484164u, "Неверный handle устройства" },
            { 2147484165u, "Неверный параметр" },
            { 2147484166u, "Не поддерживается" },
            { 2147484167u, "Неверный режим доступа" },
            { 2147484168u, "Нет свободных ресурсов" },
            { 2147484169u, "Нет данных" },
            { 2147484170u, "Таймаут" },
            { 2147484171u, "Параметр вне диапазона" }
        };

        public InitializeForm()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            // Инициализация таймера для последовательного подключения
            initializationTimer = new System.Windows.Forms.Timer();
            initializationTimer.Interval = 1000;
            initializationTimer.Tick += InitializationTimer_Tick;

            // Скрываем кнопки до завершения инициализации
            btnExit.Visible = false;
            btnRetry.Visible = false;
            btnContinue.Visible = false;

            initializationTimer.Start();
        }

        private void InitializationTimer_Tick(object sender, EventArgs e)
        {
            initializationStep++;
            switch (initializationStep)
            {
                case 1:
                    Task.Run(() => ConnectToCamera());
                    break;
                case 2:
                    Task.Run(() => ConnectToModbus());
                    break;
                case 3:
                    CompleteInitialization();
                    break;
            }
        }

        private void ConnectToCamera()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    cameraConectLabel.Text = "[...] Подключение камеры...";
                    cameraConectLabel.ForeColor = SystemColors.ControlDarkDark;
                }));

                Camera = new HikCamera(LocalSettings.Instance.Cam1SN);
                bool opened = Camera.Open();

                if (opened)
                {
                    CameraConnected = true;
                    this.Invoke(new Action(() =>
                    {
                        cameraConectLabel.Text = "[ОК] Камера подключена";
                        cameraConectLabel.ForeColor = Color.LimeGreen;
                    }));
                }
                else
                {
                    Camera = null;
                    CameraConnected = false;

                    // Получаем код ошибки камеры
                    uint errorCode = Camera?.LastErrorCode ?? 2147484163u; // пример дефолтной ошибки
                    string errorText = CAMERA_ERRORS.ContainsKey(errorCode) ? CAMERA_ERRORS[errorCode] : "Неизвестная ошибка";

                    this.Invoke(new Action(() =>
                    {
                        cameraConectLabel.Text = $"[Fail] Ошибка камеры: {errorText} (0x{errorCode:X8})";
                        cameraConectLabel.ForeColor = Color.Red;
                    }));
                }
            }
            catch (Exception ex)
            {
                Camera = null;
                CameraConnected = false;
                this.Invoke(new Action(() =>
                {
                    cameraConectLabel.Text = $"[Fail] Исключение: {ex.Message}";
                    cameraConectLabel.ForeColor = Color.Red;
                }));
            }
        }

        private void ConnectToModbus()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    prConnectLabel.Text = "[...] Подключение ПР205...";
                    prConnectLabel.ForeColor = SystemColors.ControlDarkDark;
                }));

                // Используем значения из Settings
                string ip = Properties.Settings.Default.IpAdressPr;
                string portString = Properties.Settings.Default.PortPr;
                int port = 502; // значение по умолчанию
                if (!int.TryParse(portString, out port))
                {
                    port = 502; // или другое дефолтное значение
                }

                ModbusClient = new ModbusTCP(ip, port);
                bool connected = ModbusClient.Connect();

                this.Invoke(new Action(() =>
                {
                    if (connected)
                    {
                        ModbusConnected = true;
                        prConnectLabel.Text = $"[ОК] ПР205 подключена ({ip}:{port})";
                        prConnectLabel.ForeColor = Color.LimeGreen;
                    }
                    else
                    {
                        ModbusConnected = false;
                        prConnectLabel.Text = $"[Fail] Не удалось подключиться ({ip}:{port})";
                        prConnectLabel.ForeColor = Color.Red;
                    }
                }));
            }
            catch (Exception ex)
            {
                ModbusConnected = false;
                this.Invoke(new Action(() =>
                {
                    prConnectLabel.Text = $"[Fail] Ошибка ПР205: {ex.Message}";
                    prConnectLabel.ForeColor = Color.Red;
                }));
            }
        }

        private void CompleteInitialization()
        {
            initializationTimer.Stop();
            this.Invoke(new Action(SetupButtons));
        }

        private void SetupButtons()
        {
            btnRetry.Click -= btnRetry_Click;
            btnExit.Click -= btnExit_Click;
            btnContinue.Click -= btnContinue_Click;

            btnRetry.Click += btnRetry_Click;
            btnExit.Click += btnExit_Click;
            btnContinue.Click += btnContinue_Click;

            btnRetry.Visible = true;
            btnExit.Visible = true;
            btnContinue.Visible = true;
        }

        private void DisconnectCamera()
        {
            try
            {
                if (Camera != null && CameraConnected)
                {
                    Camera.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отключении камеры: {ex.Message}");
            }
            finally
            {
                Camera = null;
                CameraConnected = false;
            }
        }

        private void DisconnectModbus()
        {
            try
            {
                if (ModbusClient != null && ModbusConnected)
                {
                    ModbusClient.Disconnect();
                    ModbusConnected = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отключении Modbus: {ex.Message}");
            }
        }

        private void DisconnectAll()
        {
            DisconnectCamera();
            DisconnectModbus();
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            var mainForm = new MainWorkForm(Camera, ModbusClient);
            mainForm.Show();
            this.Hide();
        }

        private void btnRetry_Click(object sender, EventArgs e)
        {
            DisconnectAll();
            initializationStep = 0;
            cameraConectLabel.Text = "[...] Подключение камеры...";
            prConnectLabel.Text = "[...] Подключение ПР205...";
            btnExit.Visible = btnRetry.Visible = btnContinue.Visible = false;
            initializationTimer.Start();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            DisconnectAll();
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (this.DialogResult != DialogResult.OK)
                DisconnectAll();
        }

        private void paramCameraConnect_Click(object sender, EventArgs e)
        {
            string currentSN = Camera?.SerialNumber ?? "";

            using (var f = new CameraSettingsForm(currentSN, Camera))
            {
                var result = f.ShowDialog();

                if (result == DialogResult.OK && f.SelectedCamera != null)
                {
                    // сохраняем серийник
                    LocalSettings.Instance.Cam1SN = f.SelectedCameraSN;
                    LocalSettings.Instance.Save();

                    // обновляем камеру
                    Camera = f.SelectedCamera;
                    CameraConnected = true;

                    cameraConectLabel.Text = "[ОК] Камера подключена";
                    cameraConectLabel.ForeColor = Color.LimeGreen;
                }
                else
                {
                    // пользователь вышел крестиком — камеры нет
                    Camera = null;
                    CameraConnected = false;

                    cameraConectLabel.Text = "[Fail] Камера не выбрана";
                    cameraConectLabel.ForeColor = Color.Red;
                }
            }
        }


        private void paramPrConnect_Click(object sender, EventArgs e)
        {
            // Считываем текущие настройки из Properties.Settings
            string currentIP = Properties.Settings.Default.IpAdressPr;
            int currentPort = 502; // значение по умолчанию
            if (!int.TryParse(Properties.Settings.Default.PortPr, out currentPort))
            {
                currentPort = 502;
            }

            using (var modbusSettingsForm = new ModbusSettingsForm(currentIP, currentPort))
            {
                if (modbusSettingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Сохраняем новые значения в Settings
                    Properties.Settings.Default.IpAdressPr = modbusSettingsForm.ModbusIP;
                    Properties.Settings.Default.PortPr = modbusSettingsForm.ModbusPort.ToString();
                    Properties.Settings.Default.Save();

                    // Переподключаемся к Modbus с новыми настройками
                    Task.Run(() =>
                    {
                        this.Invoke(new Action(() =>
                        {
                            prConnectLabel.Text = "[...] Подключение ПР205...";
                            prConnectLabel.ForeColor = SystemColors.ControlDarkDark;
                        }));

                        try
                        {
                            // Создаем новый клиент
                            ModbusClient?.Disconnect();
                            ModbusClient = new ModbusTCP(modbusSettingsForm.ModbusIP, modbusSettingsForm.ModbusPort);
                            bool connected = ModbusClient.Connect();

                            this.Invoke(new Action(() =>
                            {
                                if (connected)
                                {
                                    ModbusConnected = true;
                                    prConnectLabel.Text = "[ОК] ПР205 подключена";
                                    prConnectLabel.ForeColor = Color.LimeGreen;
                                }
                                else
                                {
                                    ModbusConnected = false;
                                    prConnectLabel.Text = "[Fail] Ошибка ПР205";
                                    prConnectLabel.ForeColor = Color.Red;
                                }
                            }));
                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new Action(() =>
                            {
                                ModbusConnected = false;
                                prConnectLabel.Text = $"[Fail] Ошибка ПР205: {ex.Message}";
                                prConnectLabel.ForeColor = Color.Red;
                            }));
                        }
                    });
                }
            }
        }

    }
}
