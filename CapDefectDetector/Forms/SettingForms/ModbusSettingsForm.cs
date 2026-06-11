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

namespace CapDefectDetector
{
    public partial class ModbusSettingsForm : Form
    {
        public string ModbusIP { get; private set; }
        public int ModbusPort { get; private set; }

        public ModbusSettingsForm(string currentIP, int currentPort)
        {
            InitializeComponent();
            ipTextBox.Text = currentIP;
            portNumericUpDown.Value = currentPort;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (IsValidIP(ipTextBox.Text))
            {
                ModbusIP = ipTextBox.Text;
                ModbusPort = (int)portNumericUpDown.Value;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Введите корректный IP-адрес", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
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

        private void testConnectionButton_Click(object sender, EventArgs e)
        {
            if (!IsValidIP(ipTextBox.Text))
            {
                MessageBox.Show("Введите корректный IP-адрес", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            testConnectionButton.Enabled = false;
            testConnectionButton.Text = "Тестирование...";

            Task.Run(() =>
            {
                ModuleIO testClient = null;
                try
                {
                    testClient = new ModuleIO(ipTextBox.Text, (int)portNumericUpDown.Value);
                    bool connected = testClient.Connect();

                    this.Invoke(new Action(() =>
                    {
                        if (connected)
                        {
                            MessageBox.Show("Подключение успешно!", "Тест подключения",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось подключиться к устройству", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }));

                    if (connected)
                        testClient.Disconnect();
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
                finally
                {
                    // Вручную освобождаем ресурсы если нужно
                    testClient?.Disconnect();

                    this.Invoke(new Action(() =>
                    {
                        testConnectionButton.Enabled = true;
                        testConnectionButton.Text = "Тест подключения";
                    }));
                }
            });
        }
    }
}