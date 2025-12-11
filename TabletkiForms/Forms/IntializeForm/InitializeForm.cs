using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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

            // Запускаем процесс инициализации
            initializationTimer.Start();
        }

        private void InitializationTimer_Tick(object sender, EventArgs e)
        {
            initializationStep++;

            switch (initializationStep)
            {
                case 1:
                    // Подключение камеры
                    Task.Run(() => ConnectToCamera());
                    break;
                case 2:
                    // Подключение Modbus
                    Task.Run(() => ConnectToModbus());
                    break;
                case 3:
                    // Завершение инициализации
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

                if (Camera.Open())
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
                    // Если не удалось подключиться, зануляем объект камеры
                    Camera = null;
                    CameraConnected = false;
                    throw new Exception("Не удалось открыть камеру");
                }
            }
            catch (Exception ex)
            {
                // При любой ошибке зануляем объект камеры
                Camera = null;
                CameraConnected = false;
                this.Invoke(new Action(() =>
                {
                    cameraConectLabel.Text = $"[Fail] Ошибка камеры: {ex.Message}";
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

                ModbusClient = new ModbusTCP("10.10.69.38", 502);
                ModbusClient.Connect();

                if (ModbusClient.Connected)
                {
                    ModbusConnected = true;
                    this.Invoke(new Action(() =>
                    {
                        prConnectLabel.Text = "[ОК] ПР205 подключена";
                        prConnectLabel.ForeColor = Color.LimeGreen;
                    }));
                }
                else
                {
                    throw new Exception("Не удалось подключиться к Modbus");
                }
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

            this.Invoke(new Action(() =>
            {
                // Просто показываем все кнопки
                SetupButtons();
            }));
        }

        private void SetupButtons()
        {
            // Убедимся, что обработчики не дублируются
            btnRetry.Click -= btnRetry_Click;
            btnExit.Click -= btnExit_Click;
            btnContinue.Click -= btnContinue_Click;

            // Добавляем обработчики
            btnRetry.Click += btnRetry_Click;
            btnExit.Click += btnExit_Click;
            btnContinue.Click += btnContinue_Click;

            // Показываем все кнопки
            btnRetry.Visible = true;
            btnExit.Visible = true;
            btnContinue.Visible = true;
        }

        // Методы для разрыва соединений
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
                // Всегда зануляем объект камеры после отключения
                Camera = null;
                CameraConnected = false;
                Console.WriteLine("Камера отключена");
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
                    Console.WriteLine("Modbus отключен");
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
            if (!CameraConnected)
            {
                /*MessageBox.Show("Камера не подключена!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;*/
            }

            if (!ModbusConnected)
            {
                /*MessageBox.Show("Modbus не подключён!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;*/
            }

            // Передаем камеру и Modbus в MainWorkForm
            /*var mainForm = new MainWorkForm(Camera, ModbusClient);
            mainForm.Show();*/

            var mainForm = new MainWorkForm(Camera, ModbusClient);
            mainForm.Show();

            this.Hide(); // скрываем InitializeForm
        }


        private void btnRetry_Click(object sender, EventArgs e)
        {
            // Сначала разрываем существующие соединения
            DisconnectAll();

            // Затем перезапускаем инициализацию
            initializationStep = 0;
            cameraConectLabel.Text = "[...] Подключение камеры...";
            cameraConectLabel.ForeColor = SystemColors.ControlDarkDark;
            prConnectLabel.Text = "[...] Подключение ПР205...";
            prConnectLabel.ForeColor = SystemColors.ControlDarkDark;

            // Убеждаемся, что объекты занулены
            Camera = null;
            ModbusClient = null;
            CameraConnected = false;
            ModbusConnected = false;

            btnExit.Visible = false;
            btnRetry.Visible = false;
            btnContinue.Visible = false;

            initializationTimer.Start();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            // Разрываем все соединения перед закрытием
            DisconnectAll();

            // Закрываем приложение
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Обработчик для кнопки закрытия (на случай если она есть в дизайнере)
        private void btnParams_Click(object sender, EventArgs e)
        {
            // Разрываем все соединения перед закрытием
            DisconnectAll();

            // Закрываем приложение
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Обработчик закрытия формы (на случай закрытия через крестик)
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Если форма закрывается не для перехода к основной форме,
            // разрываем соединения
            if (this.DialogResult != DialogResult.OK)
            {
                DisconnectAll();
            }
        }

        private void paramCameraConnect_Click(object sender, EventArgs e)
        {
            string currentCameraSN = Camera?.SerialNumber ?? "";

            using (var cameraSettingsForm = new CameraSettingsForm(currentCameraSN, Camera))
            {
                if (cameraSettingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Сохраняем серийный номер в настройках
                    LocalSettings.Instance.Cam1SN = cameraSettingsForm.SelectedCameraSN;
                    LocalSettings.Instance.Save();

                    // Получаем подключенную камеру (может быть null если подключение не удалось)
                    Camera = cameraSettingsForm.SelectedCamera;
                    CameraConnected = (Camera != null);

                    if (Camera != null)
                    {
                        MessageBox.Show($"Выбрана камера: {Camera.SerialNumber}",
                                        "Настройки сохранены", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Камера не подключена",
                                        "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void paramPrConnect_Click(object sender, EventArgs e)
        {
            using (var modbusSettingsForm = new ModbusSettingsForm("10.10.69.38", 502))
            {
                if (modbusSettingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Здесь можно сохранить настройки Modbus
                    MessageBox.Show($"Modbus настроен: {modbusSettingsForm.ModbusIP}:{modbusSettingsForm.ModbusPort}",
                                  "Настройки сохранены", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}