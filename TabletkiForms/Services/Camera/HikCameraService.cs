using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MvCamCtrl.NET;
using OpenCvSharp;
using KrishkiForms.Models;
using KrishkiForms.Services.Abstractions;

namespace KrishkiForms.Services.Camera
{
    /// <summary>
    /// Реализация сервиса камеры для устройств Hikvision.
    /// </summary>
    public class HikCameraService : ICameraService
    {
        private HikCamera _camera;
        private readonly object _lock = new object();
        private bool _isDisposed;

        // События
        public event EventHandler<Mat> FrameReceived;
        public event EventHandler<bool> ConnectionStatusChanged;

        public bool IsConnected => _camera?.Connected ?? false;
        public string CurrentSerialNumber => _camera?.SerialNumber;

        public CameraInfo CurrentCameraInfo
        {
            get
            {
                if (_camera == null) return null;
                // Создаём объект на основе текущей камеры
                return new CameraInfo
                {
                    SerialNumber = _camera.SerialNumber,
                    IpAddress = _camera.IpAdress,
                    Model = "Hikvision Camera", // точную модель можно получить при открытии, но пока так
                    ConnectionType = "Unknown",
                    IsAvailable = true,
                    IsCurrent = true
                };
            }
        }

        /// <inheritdoc />
        public async Task<List<CameraInfo>> GetAvailableCamerasAsync()
        {
            return await Task.Run(() =>
            {
                var result = new List<CameraInfo>();

                try
                {
                    var deviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                    int nRet = MyCamera.MV_CC_EnumDevices_NET(
                        MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE,
                        ref deviceList);

                    if (nRet != MyCamera.MV_OK || deviceList.nDeviceNum == 0)
                        return result;

                    for (int i = 0; i < deviceList.nDeviceNum; i++)
                    {
                        var devicePtr = deviceList.pDeviceInfo[i];
                        var device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(
                            devicePtr, typeof(MyCamera.MV_CC_DEVICE_INFO));

                        string model = "";
                        string serial = "";
                        string ip = "";
                        string connType = "";

                        if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                        {
                            var gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(
                                device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                            model = new string(gigeInfo.chModelName).TrimEnd('\0');
                            serial = new string(gigeInfo.chSerialNumber).TrimEnd('\0');
                            uint ipUint = gigeInfo.nCurrentIp;
                            ip = $"{(ipUint >> 24) & 0xFF}.{(ipUint >> 16) & 0xFF}.{(ipUint >> 8) & 0xFF}.{ipUint & 0xFF}";
                            connType = "GigE";
                        }
                        else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                        {
                            var usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(
                                device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                            model = new string(usbInfo.chModelName).TrimEnd('\0');
                            serial = new string(usbInfo.chSerialNumber).TrimEnd('\0');
                            ip = "USB";
                            connType = "USB3";
                        }

                        // Проверяем доступность (пытаемся открыть и сразу закрыть)
                        bool isAvailable = false;
                        try
                        {
                            var testCam = new HikCamera(serial);
                            isAvailable = testCam.Open();
                            if (isAvailable)
                                testCam.Close();
                        }
                        catch
                        {
                            isAvailable = false;
                        }

                        bool isCurrent = (_camera != null && _camera.SerialNumber == serial);

                        result.Add(new CameraInfo
                        {
                            Model = model,
                            SerialNumber = serial,
                            IpAddress = ip,
                            ConnectionType = connType,
                            IsAvailable = isAvailable,
                            IsCurrent = isCurrent
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Логирование можно добавить позже
                    System.Diagnostics.Debug.WriteLine($"Ошибка получения списка камер: {ex}");
                }

                return result;
            });
        }

        /// <inheritdoc />
        public async Task<bool> ConnectAsync(string serialNumber)
        {
            // Если уже подключены к другой, отключаемся
            await DisconnectAsync();

            var cam = new HikCamera(serialNumber);
            bool opened = await Task.Run(() => cam.Open());

            if (!opened)
                return false;

            // Подписываемся на событие получения кадров
            cam.SendImage += OnCameraFrameReceived;

            lock (_lock)
            {
                _camera = cam;
            }

            ConnectionStatusChanged?.Invoke(this, true);
            return true;
        }

        /// <inheritdoc />
        public async Task DisconnectAsync()
        {
            if (_camera == null)
                return;

            await StopGrabbingAsync();

            _camera.SendImage -= OnCameraFrameReceived;
            await Task.Run(() => _camera.Close());

            lock (_lock)
            {
                _camera = null;
            }

            ConnectionStatusChanged?.Invoke(this, false);
        }

        /// <inheritdoc />
        public async Task StartGrabbingAsync()
        {
            if (_camera == null)
                throw new InvalidOperationException("Камера не подключена.");

            await Task.Run(() =>
            {
                if (!_camera.Streamed)
                    _camera.StartStream();
            });
        }

        /// <inheritdoc />
        public async Task StopGrabbingAsync()
        {
            if (_camera == null)
                return;

            await Task.Run(() =>
            {
                if (_camera.Streamed)
                    _camera.EndStream();
            });
        }

        /// <inheritdoc />
        public async Task ApplySettingsAsync(uint width, uint height, uint exposure, uint saturation)
        {
            if (_camera == null)
                throw new InvalidOperationException("Камера не подключена.");

            bool wasGrabbing = _camera.Streamed;
            if (wasGrabbing)
                await StopGrabbingAsync();

            await Task.Run(() =>
            {
                _camera.Width = width;
                _camera.Height = height;
                _camera.ExposureTime = exposure;
                _camera.Saturation = saturation;

                _camera.SetWidth();
                _camera.SetHeight();
                _camera.SetExposureTime();
                _camera.SetSaturation();
            });

            if (wasGrabbing)
                await StartGrabbingAsync();
        }

        /// <inheritdoc />
        public (uint width, uint height, uint exposure, uint saturation) GetCurrentSettings()
        {
            if (_camera == null)
                return (0, 0, 0, 0);

            return (_camera.Width, _camera.Height, (uint)_camera.ExposureTime, _camera.Saturation);
        }

        private void OnCameraFrameReceived(Mat frame)
        {
            // Клонируем кадр, чтобы внешний код мог распоряжаться им независимо
            var clone = frame.Clone();
            FrameReceived?.Invoke(this, clone);
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Отключаемся синхронно (в Dispose не рекомендуется делать долгие операции,
                    // но здесь мы вынуждены, чтобы корректно освободить ресурсы)
                    DisconnectAsync().GetAwaiter().GetResult();
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}