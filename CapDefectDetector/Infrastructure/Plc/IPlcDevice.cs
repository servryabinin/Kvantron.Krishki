using System;
using System.Threading;
using System.Threading.Tasks;

namespace CapDefectDetector.Infrastructure.Plc
{
    /// <summary>
    /// Контракт для любого ПЛК/контроллера, подключённого по Modbus TCP.
    ///
    /// Зачем нужен:
    ///   Весь код приложения (InspectionService, MainPresenter) работает через
    ///   этот интерфейс — они не знают о PR205, адресах регистров и протоколе.
    ///   Это позволяет:
    ///     - переключиться на другой ПЛК без изменения логики;
    ///     - тестировать InspectionService с MockPlc.
    /// </summary>
    public interface IPlcDevice : IDisposable
    {
        // ─── Состояние ───────────────────────────────────────────────────────

        bool IsConnected { get; }

        // ─── События ─────────────────────────────────────────────────────────

        /// <summary>Соединение с ПЛК потеряно.</summary>
        event EventHandler ConnectionLost;

        /// <summary>Соединение с ПЛК восстановлено.</summary>
        event EventHandler ConnectionRestored;

        // ─── Жизненный цикл ──────────────────────────────────────────────────

        Task<bool> ConnectAsync(CancellationToken ct = default);
        Task DisconnectAsync();

        // ─── Команды инспекции ───────────────────────────────────────────────

        /// <summary>
        /// Записывает результат качества в регистр ПЛК.
        /// ПЛК использует значение для управления сбрасывателем.
        /// </summary>
        Task WriteQualityStatusAsync(QualityStatus status, CancellationToken ct = default);

        /// <summary>
        /// Устанавливает разрешение работы сбрасывателя.
        /// </summary>
        Task SetBreakerAllowAsync(bool allow, CancellationToken ct = default);

        /// <summary>
        /// Сигнализирует начало или конец обработки изображения.
        /// </summary>
        Task SetProcessingStateAsync(bool started, CancellationToken ct = default);

        // ─── Параметры линии ─────────────────────────────────────────────────

        /// <summary>Записывает время отбраковки (мс).</summary>
        Task WriteBreakingTimeAsync(ushort valueMs, CancellationToken ct = default);

        /// <summary>Записывает смещение камеры от датчика (имп.).</summary>
        Task WriteCameraOffsetAsync(ushort value, CancellationToken ct = default);

        /// <summary>Записывает смещение сбрасывателя от датчика (имп.).</summary>
        Task WriteBreakerOffsetAsync(ushort value, CancellationToken ct = default);

        // ─── Низкоуровневый доступ (для отладки) ────────────────────────────

        /// <summary>Читает значение произвольного регистра Modbus.</summary>
        Task<ushort> ReadRegisterAsync(int register, CancellationToken ct = default);

        /// <summary>Пишет значение в произвольный регистр Modbus.</summary>
        Task WriteRegisterAsync(int register, ushort value, CancellationToken ct = default);
    }

    /// <summary>Статус качества, отправляемый в ПЛК после инспекции крышки.</summary>
    public enum QualityStatus : ushort
    {
        /// <summary>Нет результата (начальное состояние).</summary>
        Default = 0,
        /// <summary>Крышка прошла проверку.</summary>
        Good    = 1,
        /// <summary>Крышка отбракована.</summary>
        Bad     = 2
    }
}
