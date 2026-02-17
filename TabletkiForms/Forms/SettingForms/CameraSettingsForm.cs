using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KrishkiForms.Models;
using KrishkiForms.Services.Abstractions;

namespace KrishkiForms
{
    public partial class CameraSettingsForm : Form
    {
        private readonly ICameraService _cameraService;
        private List<CameraInfo> _availableCameras;
        private string _currentSerialNumber;

        public CameraSettingsForm(ICameraService cameraService, string currentSerialNumber)
        {
            InitializeComponent();
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
            _currentSerialNumber = currentSerialNumber;

            _ = LoadCamerasAsync(); // асинхронная загрузка
        }

        private async System.Threading.Tasks.Task LoadCamerasAsync()
        {
            try
            {
                camerasDataGridView.Rows.Clear();

                _availableCameras = await _cameraService.GetAvailableCamerasAsync();

                if (_availableCameras == null || _availableCameras.Count == 0)
                {
                    MessageBox.Show("Камеры не найдены.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (var cam in _availableCameras)
                {
                    string availability;
                    if (cam.IsCurrent)
                        availability = "Мы подключены";
                    else
                        availability = cam.IsAvailable ? "✔" : "✖";

                    int rowIndex = camerasDataGridView.Rows.Add(
                        cam.IsCurrent,
                        cam.Model,
                        cam.SerialNumber,
                        cam.IpAddress,
                        cam.ConnectionType,
                        availability
                    );

                    camerasDataGridView.Rows[rowIndex].Tag = cam.SerialNumber;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка камер:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void camerasDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != camerasDataGridView.Columns["ConnectColumn"].Index)
                return;

            var row = camerasDataGridView.Rows[e.RowIndex];
            bool newChecked = Convert.ToBoolean(row.Cells["ConnectColumn"].EditedFormattedValue ?? false);
            string serial = row.Cells["SerialNumberColumn"].Value?.ToString() ?? "";

            var selectedCam = _availableCameras?.FirstOrDefault(c => c.SerialNumber == serial);
            if (selectedCam == null)
                return;

            if (!selectedCam.IsAvailable && newChecked)
            {
                MessageBox.Show("К этой камере нельзя подключиться.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                row.Cells["ConnectColumn"].Value = false;
                return;
            }

            if (newChecked)
            {
                bool success = await _cameraService.ConnectAsync(serial);
                if (success)
                {
                    await ReloadCamerasAsync();
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к выбранной камере.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    row.Cells["ConnectColumn"].Value = false;
                }
            }
            else
            {
                if (_cameraService.IsConnected && _cameraService.CurrentSerialNumber == serial)
                {
                    await _cameraService.DisconnectAsync();
                    await ReloadCamerasAsync();
                }
                else
                {
                    row.Cells["ConnectColumn"].Value = false;
                }
            }
        }

        private async System.Threading.Tasks.Task ReloadCamerasAsync()
        {
            _availableCameras = await _cameraService.GetAvailableCamerasAsync();
            camerasDataGridView.Rows.Clear();
            foreach (var cam in _availableCameras)
            {
                string availability;
                if (cam.IsCurrent)
                    availability = "Мы подключены";
                else
                    availability = cam.IsAvailable ? "✔" : "✖";

                int rowIndex = camerasDataGridView.Rows.Add(
                    cam.IsCurrent,
                    cam.Model,
                    cam.SerialNumber,
                    cam.IpAddress,
                    cam.ConnectionType,
                    availability
                );
                camerasDataGridView.Rows[rowIndex].Tag = cam.SerialNumber;
            }
            camerasDataGridView.Refresh();
        }

        private void camerasDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (camerasDataGridView.Columns[e.ColumnIndex].Name == "AvailabilityColumn")
            {
                string value = e.Value?.ToString();
                if (value == "✔")
                    e.CellStyle.ForeColor = Color.LimeGreen;
                else if (value == "✖")
                    e.CellStyle.ForeColor = Color.Red;
                else // "Мы подключены"
                    e.CellStyle.ForeColor = Color.DodgerBlue;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void refreshButton_Click(object sender, EventArgs e)
        {
            await ReloadCamerasAsync();
        }

        private void manualSNTextBox_TextChanged(object sender, EventArgs e)
        {
            // Можно оставить для ручного ввода, но тогда нужно реализовать подключение по введённому SN
            // Пока оставляем как есть
        }

        private void camerasDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (camerasDataGridView.SelectedRows.Count > 0)
            {
                string sn = camerasDataGridView.SelectedRows[0].Cells["SerialNumberColumn"].Value?.ToString() ?? "";
                manualSNTextBox.Text = sn;
            }
        }
    }
}