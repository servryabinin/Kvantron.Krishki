namespace KrishkiForms.Models
{
    /// <summary>
    /// Информация о подключении Modbus.
    /// </summary>
    public class ModbusConnectionInfo
    {
        /// <summary>
        /// IP-адрес устройства.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Порт подключения.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Подключено ли устройство в данный момент.
        /// </summary>
        public bool IsConnected { get; set; }
    }
}