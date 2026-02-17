using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenCvSharp;
using KrishkiForms.Models;

namespace KrishkiForms.Services.Abstractions
{
    /// <summary>
    /// Интерфейс сервиса для работы с камерой Hikvision.
    /// </summary>
    public interface ICameraService : IDisposable
    {
        /// <summary>
        /// Событие, возникающее при получении нового кадра.
        /// </summary>
        event EventHandler<Mat> FrameReceived;

        /// <summary>
        /// Событие изменения состояния подключения.
        /// </summary>
        event EventHandler<bool> ConnectionStatusChanged;

        /// <summary>
        /// Подключена ли в данный момент камера.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Серийный номер текущей подключённой камеры.
        /// </summary>
        string CurrentSerialNumber { get; }

        /// <summary>
        /// Информация о текущей подключённой камере.
        /// </summary>
        CameraInfo CurrentCameraInfo { get; }

        /// <summary>
        /// Получить список доступных камер в сети.
        /// </summary>
        /// <returns>Список объектов CameraInfo.</returns>
        Task<List<CameraInfo>> GetAvailableCamerasAsync();

        /// <summary>
        /// Подключиться к камере по серийному номеру.
        /// </summary>
        /// <param name="serialNumber">Серийный номер камеры.</param>
        /// <returns>True, если подключение успешно.</returns>
        Task<bool> ConnectAsync(string serialNumber);

        /// <summary>
        /// Отключиться от текущей камеры.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Запустить захват видеопотока (стрим).
        /// </summary>
        Task StartGrabbingAsync();

        /// <summary>
        /// Остановить захват видеопотока.
        /// </summary>
        Task StopGrabbingAsync();

        /// <summary>
        /// Применить настройки камеры.
        /// </summary>
        /// <param name="width">Ширина кадра.</param>
        /// <param name="height">Высота кадра.</param>
        /// <param name="exposure">Выдержка.</param>
        /// <param name="saturation">Насыщенность.</param>
        Task ApplySettingsAsync(uint width, uint height, uint exposure, uint saturation);

        /// <summary>
        /// Получить текущие настройки камеры.
        /// </summary>
        (uint width, uint height, uint exposure, uint saturation) GetCurrentSettings();
    }
}