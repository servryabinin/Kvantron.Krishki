using System;

namespace KrishkiForms.Models
{
    /// <summary>
    /// Информация о камере для отображения в UI и выбора.
    /// </summary>
    public class CameraInfo
    {
        /// <summary>
        /// Модель камеры.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Серийный номер.
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// IP-адрес (для GigE) или "USB" для USB-камер.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Тип подключения: "GigE" или "USB3".
        /// </summary>
        public string ConnectionType { get; set; }

        /// <summary>
        /// Доступна ли камера для подключения (удалось ли открыть тестовое соединение).
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Является ли эта камера текущей подключённой.
        /// </summary>
        public bool IsCurrent { get; set; }
    }
}