using OpenCvSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CapDefectDetector.Infrastructure.Camera
{
    /// <summary>
    /// Контракт для любого устройства захвата изображений.
    ///
    /// Цель интерфейса — развязать бизнес-логику от конкретного SDK камеры
    /// (Hikvision MvCameraControl). Благодаря этому:
    ///   - InspectionService не знает о HikCamera;
    ///   - можно добавить вторую камеру другого производителя без изменения логики;
    ///   - можно создать MockCamera для unit-тестов без реального железа.
    /// </summary>
    public interface ICameraDevice : IDisposable
    {
        // ─── Идентификация ───────────────────────────────────────────────────

        /// <summary>Серийный номер или уникальный идентификатор устройства.</summary>
        string DeviceId { get; }

        // ─── Состояние ───────────────────────────────────────────────────────

        bool IsConnected { get; }
        bool IsStreaming { get; }

        // ─── События ─────────────────────────────────────────────────────────

        /// <summary>
        /// Вызывается при получении нового кадра.
        ///
        /// ВАЖНО: событие вызывается из потока камеры (не UI-поток).
        /// Подписчик должен клонировать Mat если нужно хранить кадр дольше
        /// времени жизни события. Не вызывать тяжёлые операции в обработчике.
        /// </summary>
        event EventHandler<Mat> FrameArrived;

        /// <summary>Вызывается при аппаратной ошибке или потере связи.</summary>
        event EventHandler<string> ErrorOccurred;

        // ─── Жизненный цикл ──────────────────────────────────────────────────

        /// <summary>Подключается к камере. Возвращает true при успехе.</summary>
        Task<bool> ConnectAsync(CancellationToken ct = default);

        /// <summary>Отключается от камеры, освобождая аппаратный ресурс.</summary>
        Task DisconnectAsync();

        /// <summary>Запускает поток захвата кадров.</summary>
        Task StartStreamAsync(CancellationToken ct = default);

        /// <summary>
        /// Останавливает поток захвата кадров.
        /// Метод ожидает фактического завершения потока (не fire-and-forget).
        /// </summary>
        Task StopStreamAsync();

        // ─── Настройки камеры ────────────────────────────────────────────────

        /// <summary>Устанавливает время экспозиции в микросекундах.</summary>
        bool SetExposureTime(float exposureUs);

        /// <summary>Устанавливает насыщенность (0–100).</summary>
        bool SetSaturation(uint saturation);

        /// <summary>Устанавливает усиление.</summary>
        bool SetGain(float gain);
    }
}
