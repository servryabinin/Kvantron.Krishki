using System;
using System.Threading.Tasks;
using KrishkiForms.Models;

namespace KrishkiForms.Services.Abstractions
{
    /// <summary>
    /// Интерфейс сервиса для работы с Modbus TCP.
    /// </summary>
    public interface IModbusService : IDisposable
    {
        /// <summary>
        /// Событие изменения состояния подключения.
        /// </summary>
        event EventHandler<bool> ConnectionStatusChanged;

        /// <summary>
        /// Текущее состояние подключения.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Информация о текущем подключении.
        /// </summary>
        ModbusConnectionInfo CurrentConnectionInfo { get; }

        /// <summary>
        /// Подключиться к устройству Modbus.
        /// </summary>
        /// <param name="ipAddress">IP-адрес.</param>
        /// <param name="port">Порт.</param>
        /// <returns>True, если подключение успешно.</returns>
        Task<bool> ConnectAsync(string ipAddress, int port);

        /// <summary>
        /// Отключиться от устройства.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Записать значение в регистр.
        /// </summary>
        /// <param name="register">Адрес регистра.</param>
        /// <param name="value">Значение.</param>
        Task WriteRegisterAsync(int register, ushort value);

        /// <summary>
        /// Прочитать значение из регистра.
        /// </summary>
        /// <param name="register">Адрес регистра.</param>
        /// <returns>Значение регистра.</returns>
        Task<ushort> ReadRegisterAsync(int register);

        /// <summary>
        /// Включить автоматическое переподключение при потере связи.
        /// </summary>
        void EnableAutoReconnect();

        /// <summary>
        /// Отключить автоматическое переподключение.
        /// </summary>
        void DisableAutoReconnect();

        /// <summary>
        /// Запустить периодическую проверку соединения.
        /// </summary>
        void StartPolling(int intervalMilliseconds = 5000);

        /// <summary>
        /// Остановить периодическую проверку.
        /// </summary>
        void StopPolling();
    }
}