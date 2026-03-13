using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CapDefectDetector.CameraAndModbusClasses;

namespace CapDefectDetector
{
    public partial class CameraSettingsForm : Form
    {
        private List<MyCamera.MV_CC_DEVICE_INFO> availableDevices = new();
        private HikCamera currentCamera; // текущая активная камера
        public HikCamera SelectedCamera { get; private set; }
        private string currentCameraSN;  // серийный номер текущей подключенной камеры
        public string SelectedCameraSN { get; private set; } // выбранный серийник для сохранения
        public string SelectedCameraIp { get; private set; }


        public CameraSettingsForm(string currentSN, HikCamera camera)
        {
            InitializeComponent();
            currentCameraSN = currentSN;
            currentCamera = camera;

            LoadCameras();
        }

        // --- загрузка списка доступных камер ---
        private void LoadCameras()
        {
            camerasDataGridView.Rows.Clear();
            availableDevices.Clear();

            try
            {
                MyCamera.MV_CC_DEVICE_INFO_LIST deviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref deviceList);
                if (nRet != MyCamera.MV_OK)
                {
                    MessageBox.Show(Environment.CurrentDirectory);
                    MessageBox.Show(
                        $"Ошибка при поиске камер\nКод: 0x{nRet:X8}",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                    
                }
                for (int i = 0; i < deviceList.nDeviceNum; i++)
                {
                    var devicePtr = deviceList.pDeviceInfo[i];
                    var device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(devicePtr, typeof(MyCamera.MV_CC_DEVICE_INFO));

                    string modelName = "";
                    string serialNumber = "";
                    string ipAddress = "";
                    string connType = "";

                    if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        var gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                        modelName = new string(gigeInfo.chModelName).TrimEnd('\0');
                        serialNumber = new string(gigeInfo.chSerialNumber).TrimEnd('\0');

                        // <-- исправлённое получение IP (как ты предложил)
                        uint ip = gigeInfo.nCurrentIp;
                        ipAddress = $"{(ip >> 24) & 0xFF}.{(ip >> 16) & 0xFF}.{(ip >> 8) & 0xFF}.{ip & 0xFF}";

                        connType = "GigE";
                    }
                    else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                    {
                        var usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                        modelName = new string(usbInfo.chModelName).TrimEnd('\0');
                        serialNumber = new string(usbInfo.chSerialNumber).TrimEnd('\0');
                        ipAddress = "USB";
                        connType = "USB3";
                    }

                    // проверим доступность подключения / состояние
                    string availability;

                    if (!string.IsNullOrEmpty(currentCameraSN) &&
                        string.Equals(serialNumber, currentCameraSN, StringComparison.OrdinalIgnoreCase) &&
                        currentCamera != null && currentCamera.Connected)
                    {
                        availability = "Мы подключены";
                    }
                    else
                    {
                        // попытка открыть устройство в тестовом экземпляре (быстро проверить доступность)
                        bool canConnect = false;
                        try
                        {
                            var testCam = new HikCamera(serialNumber);
                            canConnect = testCam.Open();
                            if (canConnect)
                            {
                                testCam.Close();
                                availability = "✔";
                            }
                            else
                            {
                                availability = "✖";
                            }
                        }
                        catch
                        {
                            availability = "✖";
                        }
                    }

                    int rowIndex = camerasDataGridView.Rows.Add(
                        serialNumber == currentCameraSN, // чекбокс выбран, если это текущая камера
                        modelName,
                        serialNumber,
                        ipAddress,
                        connType,
                        availability
                    );

                    // сохраняем серийник в Tag для быстрого доступа
                    camerasDataGridView.Rows[rowIndex].Tag = serialNumber;

                    availableDevices.Add(device);
                }

                if (deviceList.nDeviceNum == 0)
                {
                    MessageBox.Show("Камеры не найдены.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка камер:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- логика клика по чекбоксу ---
        private void camerasDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != camerasDataGridView.Columns["ConnectColumn"].Index)
                return;

            var row = camerasDataGridView.Rows[e.RowIndex];

            // Так как CellContentClick вызывается ДО изменения значения,
            // используем EditedFormattedValue чтобы получить новое состояние.
            bool newChecked = Convert.ToBoolean(row.Cells["ConnectColumn"].EditedFormattedValue ?? false);
            string serial = row.Cells["SerialNumberColumn"].Value?.ToString() ?? "";
            string availability = row.Cells["AvailabilityColumn"].Value?.ToString() ?? "✖";

            if (availability == "✖" && newChecked)
            {
                MessageBox.Show("К этой камере нельзя подключиться.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // явно откатим состояние
                row.Cells["ConnectColumn"].Value = false;
                return;
            }

            if (newChecked)
            {
                // если уже подключены к другой — отключаемся
                if (currentCamera != null && currentCamera.Connected && !string.Equals(currentCamera.SerialNumber, serial, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        currentCamera.Close();
                        UpdateRowStatus(currentCamera.SerialNumber, "✔", false);
                    }
                    catch { }
                }

                // подключаем новую
                var newCam = new HikCamera(serial);
                if (newCam.Open())
                {
                    // закрываем старую (если осталась)
                    try
                    {
                        if (currentCamera != null && currentCamera.Connected && !string.Equals(currentCamera.SerialNumber, serial, StringComparison.OrdinalIgnoreCase))
                            currentCamera.Close();
                    }
                    catch { }

                    currentCamera = newCam;
                    currentCameraSN = serial;
                    SelectedCameraSN = serial;
                    UpdateRowStatus(serial, "Мы подключены", true);
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к выбранной камере.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    row.Cells["ConnectColumn"].Value = false;
                    UpdateRowStatus(serial, "✖", false);
                }
            }
            else
            {
                // снимаем подключение
                if (currentCamera != null && currentCamera.Connected && string.Equals(currentCamera.SerialNumber, serial, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        currentCamera.Close();
                    }
                    catch { }

                    currentCamera = null;
                    currentCameraSN = null;
                    SelectedCameraSN = null;
                    UpdateRowStatus(serial, "✔", false);
                }
            }
        }

        // --- обновление состояния строки ---
        private void UpdateRowStatus(string serial, string newStatus, bool connected)
        {
            foreach (DataGridViewRow row in camerasDataGridView.Rows)
            {
                string rowSN = row.Cells["SerialNumberColumn"].Value?.ToString() ?? "";
                if (string.Equals(rowSN, serial, StringComparison.OrdinalIgnoreCase))
                {
                    row.Cells["AvailabilityColumn"].Value = newStatus;
                    row.Cells["ConnectColumn"].Value = connected;
                }
                else
                {
                    // снимаем чекбоксы с других строк
                    row.Cells["ConnectColumn"].Value = false;
                    if (row.Cells["AvailabilityColumn"].Value?.ToString() == "Мы подключены")
                        row.Cells["AvailabilityColumn"].Value = "✔";
                }
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
            // Если камера не выбрана, устанавливаем SelectedCamera в null
            if (string.IsNullOrEmpty(SelectedCameraSN) || currentCamera == null)
            {
                SelectedCamera = null;
            }
            else
            {
                // Передаём текущую камеру наружу
                SelectedCamera = currentCamera;
            }

            DialogResult = DialogResult.OK;
            Close();
        }


        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            LoadCameras();
        }

        private void manualSNTextBox_TextChanged(object sender, EventArgs e)
        {
            SelectedCameraSN = manualSNTextBox.Text.Trim();
        }

        private void camerasDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (camerasDataGridView.SelectedRows.Count > 0)
            {
                string sn = camerasDataGridView.SelectedRows[0].Cells["SerialNumberColumn"].Value?.ToString() ?? "";
                manualSNTextBox.Text = sn;
                SelectedCameraSN = sn;
            }
        }

        private void CameraSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
           
        }
    }
}
