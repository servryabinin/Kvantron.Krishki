using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using KrishkiForms.Services.Abstractions;

namespace KrishkiForms
{
    public partial class ModbusSettingsForm : Form
    {
        private readonly IModbusService _modbusService;

        public string ModbusIP { get; private set; }
        public int ModbusPort { get; private set; }

        public ModbusSettingsForm(IModbusService modbusService, string currentIP, int currentPort)
        {
            InitializeComponent();
            _modbusService = modbusService ?? throw new ArgumentNullException(nameof(modbusService));

            ipTextBox.Text = currentIP;
            portNumericUpDown.Value = currentPort;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (IsValidIP(ipTextBox.Text))
            {
                ModbusIP = ipTextBox.Text;
                ModbusPort = (int)portNumericUpDown.Value;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Введите корректный IP-адрес", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool IsValidIP(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            string[] parts = ip.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (string part in parts)
            {
                if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                    return false;
            }

            return true;
        }

        private async void testConnectionButton_Click(object sender, EventArgs e)
        {
            if (!IsValidIP(ipTextBox.Text))
            {
                MessageBox.Show("Введите корректный IP-адрес", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            testConnectionButton.Enabled = false;
            testConnectionButton.Text = "Тестирование...";

            try
            {
                bool connected = await _modbusService.ConnectAsync(ipTextBox.Text, (int)portNumericUpDown.Value);

                if (connected)
                {
                    MessageBox.Show("Подключение успешно!", "Тест подключения",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await _modbusService.DisconnectAsync();
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к устройству", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                testConnectionButton.Enabled = true;
                testConnectionButton.Text = "Тест подключения";
            }
        }
    }
}