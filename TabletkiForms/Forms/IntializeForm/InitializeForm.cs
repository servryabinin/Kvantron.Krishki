using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using KrishkiForms.CameraAndModbusClasses;
using KrishkiForms.Services.Abstractions;
using KrishkiForms.Services.Camera;
using KrishkiForms.Services.Modbus;

namespace KrishkiForms
{
    public partial class InitializeForm : Form
    {
        // Публичные свойства для передачи в основную форму (теперь сервисы)
        public ICameraService CameraService { get; private set; }
        public IModbusService ModbusService { get; private set; }
        public bool CameraConnected { get; private set; }
        public bool ModbusConnected { get; private set; }

        private System.Windows.Forms.Timer initializationTimer;
        private int initializationStep = 0;

        // Словарь ошибок камеры (оставим для отображения, но будем получать код ошибки из сервиса)
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

            // Создаём экземпляры сервисов
            CameraService = new HikCameraService();
            ModbusService = new ModbusService();

            InitializeApplication();
        }

        private void InitializeApplication()
        {
            initializationTimer = new System.Windows.Forms.Timer();
            initializationTimer.Interval = 1000;
            initializationTimer.Tick += InitializationTimer_Tick;

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
                    _ = ConnectToCameraAsync(); // асинхронный запуск без ожидания
                    break;
                case 2:
                    _ = ConnectToModbusAsync();
                    break;
                case 3:
                    CompleteInitialization();
                    break;
            }
        }

        private async Task ConnectToCameraAsync()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    cameraConectLabel.Text = "[...] Подключение камеры...";
                    cameraConectLabel.ForeColor = SystemColors.ControlDarkDark;
                }));

                string sn = LocalSettings.Instance.Cam1SN;
                bool opened = await CameraService.ConnectAsync(sn);

                if (opened)
                {
                    CameraConnected = true;
                    var camInfo = CameraService.CurrentCameraInfo;

                    // Сохраняем IP
                    if (camInfo != null)
                    {
                        Properties.Settings.Default.IpAdressCamera = camInfo.IpAddress;
                        Properties.Settings.Default.Save();
                    }

                    this.Invoke(new Action(() =>
                    {
                        string ip = camInfo?.IpAddress ?? "Unknown";
                        cameraConectLabel.Text = $"[ОК] Камера подключена ({ip})";
                        cameraConectLabel.ForeColor = Color.LimeGreen;
                    }));
                }
                else
                {
                    CameraConnected = false;
                    // Здесь можно получить код ошибки, но пока выведем общее сообщение
                    // (для получения LastErrorCode нужно расширить сервис)
                    this.Invoke(new Action(() =>
                    {
                        cameraConectLabel.Text = "[Fail] Ошибка подключения к камере";
                        cameraConectLabel.ForeColor = Color.Red;
                    }));
                }
            }
            catch (Exception ex)
            {
                CameraConnected = false;
                this.Invoke(new Action(() =>
                {
                    cameraConectLabel.Text = $"[Fail] Исключение: {ex.Message}";
                    cameraConectLabel.ForeColor = Color.Red;
                }));
            }
        }

        private async Task ConnectToModbusAsync()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    prConnectLabel.Text = "[...] Подключение ПР205...";
                    prConnectLabel.ForeColor = SystemColors.ControlDarkDark;
                }));

                string ip = Properties.Settings.Default.IpAdressPr;
                string portString = Properties.Settings.Default.PortPr;
                int port = 502;
                if (!int.TryParse(portString, out port))
                    port = 502;

                bool connected = await ModbusService.ConnectAsync(ip, port);

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
                if (CameraService != null && CameraConnected)
                {
                    CameraService.DisconnectAsync().Wait(); // упрощённо
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отключении камеры: {ex.Message}");
            }
            finally
            {
                CameraConnected = false;
            }
        }

        private void DisconnectModbus()
        {
            try
            {
                if (ModbusService != null && ModbusConnected)
                {
                    ModbusService.DisconnectAsync().Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отключении Modbus: {ex.Message}");
            }
            finally
            {
                ModbusConnected = false;
            }
        }

        private void DisconnectAll()
        {
            DisconnectCamera();
            DisconnectModbus();
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            // Передаём сервисы, а не конкретные классы
            //var mainForm = new MainWorkForm(CameraService, ModbusService);
            //mainForm.Show();
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

            CameraService?.Dispose();
            ModbusService?.Dispose();
        }

        private void paramCameraConnect_Click(object sender, EventArgs e)
        {
            string currentSN = CameraService?.CurrentSerialNumber ?? "";

            // Открываем форму с сервисом
            using (var f = new CameraSettingsForm(CameraService, currentSN))
            {
                var result = f.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // Обновляем состояние после закрытия формы
                    if (CameraService.IsConnected)
                    {
                        CameraConnected = true;
                        var camInfo = CameraService.CurrentCameraInfo;
                        string ip = camInfo?.IpAddress ?? "Unknown";
                        cameraConectLabel.Text = $"[ОК] Камера подключена ({ip})";
                        cameraConectLabel.ForeColor = Color.LimeGreen;

                        // Сохраняем серийный номер и IP
                        LocalSettings.Instance.Cam1SN = CameraService.CurrentSerialNumber;
                        LocalSettings.Instance.Save();
                        Properties.Settings.Default.IpAdressCamera = ip;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        CameraConnected = false;
                        cameraConectLabel.Text = "[Fail] Камера не подключена";
                        cameraConectLabel.ForeColor = Color.Red;
                    }
                }
            }
        }

        private void paramPrConnect_Click(object sender, EventArgs e)
        {
            string currentIP = Properties.Settings.Default.IpAdressPr;
            int currentPort = 502;
            if (!int.TryParse(Properties.Settings.Default.PortPr, out currentPort))
                currentPort = 502;

            using (var f = new ModbusSettingsForm(ModbusService, currentIP, currentPort))
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    Properties.Settings.Default.IpAdressPr = f.ModbusIP;
                    Properties.Settings.Default.PortPr = f.ModbusPort.ToString();
                    Properties.Settings.Default.Save();

                    // Переподключаемся с новыми настройками
                    _ = ReconnectModbusAsync(f.ModbusIP, f.ModbusPort);
                }
            }
        }

        private async Task ReconnectModbusAsync(string ip, int port)
        {
            this.Invoke(new Action(() =>
            {
                prConnectLabel.Text = "[...] Подключение ПР205...";
                prConnectLabel.ForeColor = SystemColors.ControlDarkDark;
            }));

            try
            {
                await ModbusService.DisconnectAsync();
                bool connected = await ModbusService.ConnectAsync(ip, port);

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
        }
    }
}