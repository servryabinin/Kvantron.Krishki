using CapDefectDetector.CameraAndModbusClasses;
using CapDefectDetector.Logger;
using OpenCvSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CapDefectDetector.Infrastructure.Camera
{
    /// <summary>
    /// Адаптер над существующим классом <see cref="HikCamera"/>,
    /// реализующий интерфейс <see cref="ICameraDevice"/>.
    ///
    /// Паттерн: Adapter (Обёртка).
    ///
    /// Зачем нужен:
    ///   HikCamera — низкоуровневая обёртка над Hikvision SDK с прямой зависимостью
    ///   от MvCameraControl.Net. Весь остальной код (InspectionService, MainPresenter)
    ///   работает только с ICameraDevice — они не знают, какая камера стоит на линии.
    ///
    /// Что добавляет по сравнению с HikCamera:
    ///   - async/await lifecycle (ConnectAsync, StopStreamAsync с реальным ожиданием);
    ///   - события ErrorOccurred с текстовым описанием;
    ///   - логирование через ErrorLogger;
    ///   - thread-safe подписка/отписка на SendImage.
    /// </summary>
    public sealed class HikCameraAdapter : ICameraDevice
    {
        private readonly HikCamera _inner;
        private bool _disposed;

        // ─── ICameraDevice ───────────────────────────────────────────────────

        public string DeviceId => _inner.SerialNumber;
        public bool IsConnected => _inner.Connected;
        public bool IsStreaming => _inner.Streamed;

        public event EventHandler<Mat>? FrameArrived;
        public event EventHandler<string>? ErrorOccurred;

        // ─── Конструктор ─────────────────────────────────────────────────────

        public HikCameraAdapter(string serialNumber)
        {
            _inner = new HikCamera(serialNumber);
        }

        // ─── Lifecycle ───────────────────────────────────────────────────────

        public Task<bool> ConnectAsync(CancellationToken ct = default)
        {
            return Task.Run(() =>
            {
                try
                {
                    bool ok = _inner.Open();
                    if (!ok)
                    {
                        string msg = $"Камера {DeviceId}: ошибка подключения, код 0x{_inner.LastErrorCode:X8}";
                        ErrorLogger.LogError(msg);
                        ErrorOccurred?.Invoke(this, msg);
                    }
                    return ok;
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, $"ConnectAsync: камера {DeviceId}");
                    ErrorOccurred?.Invoke(this, ex.Message);
                    return false;
                }
            }, ct);
        }

        public Task DisconnectAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (_inner.Streamed)
                        _inner.EndStream();

                    _inner.Close();
                    ErrorLogger.LogInfo($"Камера {DeviceId}: отключена.");
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, $"DisconnectAsync: камера {DeviceId}");
                }
            });
        }

        public Task StartStreamAsync(CancellationToken ct = default)
        {
            return Task.Run(() =>
            {
                try
                {
                    // Подписываемся на кадры от внутреннего класса
                    _inner.SendImage += OnFrameReceived;

                    bool ok = _inner.StartStream();
                    if (!ok)
                    {
                        _inner.SendImage -= OnFrameReceived;
                        string msg = $"Камера {DeviceId}: не удалось запустить стрим.";
                        ErrorLogger.LogError(msg);
                        ErrorOccurred?.Invoke(this, msg);
                    }
                    else
                    {
                        ErrorLogger.LogInfo($"Камера {DeviceId}: стрим запущен.");
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, $"StartStreamAsync: камера {DeviceId}");
                    ErrorOccurred?.Invoke(this, ex.Message);
                }
            }, ct);
        }

        public Task StopStreamAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    _inner.SendImage -= OnFrameReceived;
                    _inner.EndStream(); // уже ждёт завершения Task внутри (после наших исправлений)
                    ErrorLogger.LogInfo($"Камера {DeviceId}: стрим остановлен.");
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, $"StopStreamAsync: камера {DeviceId}");
                }
            });
        }

        // ─── Настройки ───────────────────────────────────────────────────────

        public bool SetExposureTime(float exposureUs)
        {
            _inner.ExposureTime = exposureUs;
            return _inner.SetExposureTime();
        }

        public bool SetSaturation(uint saturation)
        {
            _inner.Saturation = saturation;
            return _inner.SetSaturation();
        }

        public bool SetGain(float gain)
        {
            _inner.Gain = gain;
            return _inner.SetGain();
        }

        // ─── Обработчик кадров ───────────────────────────────────────────────

        /// <summary>
        /// Вызывается из потока камеры при получении каждого кадра.
        /// Пробрасывает Mat через событие FrameArrived.
        ///
        /// Подписчики (например FrameBuffer) должны клонировать Mat если
        /// планируют использовать его после возврата из обработчика.
        /// </summary>
        private void OnFrameReceived(Mat frame)
        {
            try
            {
                FrameArrived?.Invoke(this, frame);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogWarning($"Камера {DeviceId}: ошибка в обработчике кадра.", ex);
            }
        }

        // ─── IDisposable ─────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _inner.SendImage -= OnFrameReceived;

                if (_inner.Streamed)
                    _inner.EndStream();

                if (_inner.Connected)
                    _inner.Close();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"HikCameraAdapter.Dispose: камера {DeviceId}");
            }
        }
    }
}
