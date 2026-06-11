using System.Net.Sockets;
using CapDefectDetector.DTO.PrSettings;
using Timer = System.Threading.Timer;

namespace CapDefectDetector.CameraAndModbusClasses
{
    public class ModuleIO
    {
        #region Объекты и константы для работы по сети
        /// <summary>
        /// Объект подключения к модулю I/O по TCP
        /// </summary>
        private TcpClient tcpClient;
        /// <summary>
        /// Поток для общения с модулем I/O
        /// </summary>
        private NetworkStream stream;
        /// <summary>
        /// Константа чтения
        /// </summary>
        private const int CONNECT_TIMEOUT = 1000;
        /// <summary>
        /// Номер сетевой транзации
        /// </summary>
        private int transactionNumber = 0;
        /// <summary>
        /// Объект для блокировки доступа к сетевому потоку при чтении/записи, чтобы избежать конфликтов при одновременных операциях чтения/записи из разных потоков
        /// </summary>
        private readonly object _ioLock = new object();
        #endregion

        #region Данные модуля I/O для работы по сети
        /// <summary>
        /// Ip адрес модуля I/O
        /// </summary>
        private string ipAddressModule = "127.0.0.1";
        /// <summary>
        /// Порт модуля I/O
        /// </summary>
        private int portModule = 502;
        #endregion

        #region Объекты для переподключения
        /// <summary>
        /// Таймер проверки содеинения с модулем I/O
        /// </summary>
        private Timer pollTimer;
        /// <summary>
        /// Константа интервала подключения 
        /// </summary>
        private const int POLL_INTERVAL = 1000; // 5 секунд
        /// <summary>
        /// Флаг подключения к модулю I/O
        /// </summary>
        private volatile bool connected = false;
        /// <summary>
        /// Флаг автоматического переподключения к модулю I/O при потере связи
        /// </summary>
        private bool autoReconnectEnabled = true;
        /// <summary>
        /// Событие изменения статуса подключения к модулю I/O: передает новый статус подключения (true - подключено, false - отключено)
        /// </summary>
        public event Action<bool> ConnectionStatusChanged;
        #endregion

        #region Регистры ПР205
        /// <summary>
        /// Регистр для проверки соединения
        /// </summary>
        private const int CONNECT_TEST_REGISTER = 16403;
        /// <summary>
        /// Регистр сетевой переменной для времени записи времени отбраковки
        /// </summary>
        private const int BREAKING_TIME_REGISTER = 16466;
        /// <summary>
        /// Регистр сетевой переменной для записи расстояние от датчика до камеры 
        /// </summary>
        private const int CAMERA_OFFSET_REGISTER = 16402;
        /// <summary>
        /// Регистр сетевой переменной для записи расстояние от датчика до камеры 
        /// </summary>
        private const int BREAKER_OFFSET_REGISTER = 16404;
        /// <summary>
        /// Регистр сетевой переменной для установки разрешения отбраковки
        /// </summary>
        private const int BREAKER_ALLOW_REGISTER = 16401;
        /// <summary>
        /// Регистр сетевой переменной для запуска процесса распознавания
        /// </summary>
        private const int START_RECOGNIZE_PROCESSING_REGISTER = 16400;
        /// <summary>
        /// Регистр сетевой переменной для включения оранжевого сигнала светофора
        /// </summary>
        private const int SEMAPHORE_ORANGE_REGISTER = 16406;
        /// <summary>
        /// Регистр сетевой переменной для включения зеленого сигнала светофора
        /// </summary>
        private const int SEMAPHORE_GREEN_REGISTER = 16407;
        #endregion

        #region Параметры для времени отбраковки, расстояние от датчика до камеры, расстояние от датчика до отбраковщика
        /// <summary>
        /// Параметр для времени отбраковки, который будет записываться в регистр BREAKING_TIME_REGISTER
        /// </summary>
        private int _breakingTimeValue = 55;
        /// <summary>
        /// Параметр для расстояния от датчика до камеры, который будет записываться в регистр CAMERA_OFFSET_REGISTER
        /// </summary>
        private int _cameraOffsetValue = 300;
        /// <summary>
        /// Параметр для расстояния от датчика до отбраковщика, который будет записываться в регистр BREAKER_OFFSET_REGISTER
        /// </summary>
        private int _breakerOffsetValue = 2430;
        #endregion

        #region Детерминированные значения для разрешения отбраковки, запуска процесса распознавания и светофора
        /// <summary>
        /// Детерминированные значения для разрешения отбраковки: 1 - физическая отбраковка разрешена
        /// </summary>
        private const int BREAKER_ALLOW_TRUE = 1;
        /// <summary>
        /// Детерминированные значения для разрешения отбраковки: 0 - физическая отбраковка запрещена
        /// </summary>
        private const int BREAKER_ALLOW_FALSE = 0;
        /// <summary>
        /// Детерминированные значения для запуска процесса распознавания: 1 - начать ставить крышки в очередь на отбраковку
        /// </summary>
        private const int RECOGNIZE_PROCESSING_START = 1;
        /// <summary>
        /// Детерминированные значения для запуска процесса распознавания: 0 - закончить ставить крышки в очередь на отбраковку
        /// </summary>
        private const int RECOGNIZE_PROCESSING_FINISH = 0;
        /// <summary>
        /// Детерминированные значения для включения сигнала светофора: 1 - включить сигнал светофора
        /// </summary>
        private const int SEMAPHORE_ON = 1;
        /// <summary>
        /// Детерминированные значения для включения сигнала светофора: 0 - выключить сигнал светофора
        /// </summary>
        private const int SEMAPHORE_OFF = 0;
        #endregion  

        /// <summary>
        /// Свойство для получения статуса подключения к модулю I/O
        /// </summary>
        public bool Connected => connected;

        /// <summary>
        /// Контсруктор класса ModuleIO, который принимает IP адрес и порт для подключения к модулю I/O
        /// </summary>
        /// <param name="ip">Ip адрес модуля I/O</param>
        /// <param name="port">Порт модуля I/O</param>
        public ModuleIO(string ip, int port)
        {
            ipAddressModule = ip;
            portModule = port;
        }

        #region Сеттеры и геттеры
        /// <summary>
        /// Установить время отбраковки
        /// </summary>
        public void SetBreakingTime(int value) => _breakingTimeValue = value;

        /// <summary>
        /// Установить расстояние от датчика до камеры
        /// </summary>
        public void SetCameraOffset(int value) => _cameraOffsetValue = value;

        /// <summary>
        /// Установить расстояние от датчика до отбраковщика
        /// </summary>
        public void SetBreakerOffset(int value) => _breakerOffsetValue = value;

        /// <summary>
        /// Получить время отбраковки
        /// </summary>
        /// <returns></returns>
        public int GetBreakingTime() => _breakingTimeValue;

        /// <summary>
        /// Получить расстояние от датчика до камеры
        /// </summary>
        /// <returns></returns>
        public int GetCameraOffset() => _cameraOffsetValue;

        /// <summary>
        /// Получить расстояние от датчика до отбраковщика
        /// </summary>
        /// <returns></returns>
        public int GetBreakerOffset() => _breakerOffsetValue;

        /// <summary>
        /// Применить все настройки ModuleIO через сеттеры
        /// </summary>
        public void ApplyLocalSettings(ModuleIOSettings settings)
        {
            SetBreakingTime(settings.BreakingTime);
            SetCameraOffset(settings.CameraOffset);
            SetBreakerOffset(settings.BreakerOffset);
        }
        #endregion

        #region Подключение/отключение, проверка соединения и опрос соединения
        /// <summary>
        /// Метод для подключения к модулю I/O: возвращает true если подключение успешно, false - если не удалось подключиться
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            try
            {
                tcpClient = new TcpClient();
                IAsyncResult asyncResult = tcpClient.BeginConnect(ipAddressModule, portModule, null, null);
                if (!asyncResult.AsyncWaitHandle.WaitOne(CONNECT_TIMEOUT))
                    throw new Exception("connection timed out");
                tcpClient.EndConnect(asyncResult);
                stream = tcpClient.GetStream();
                stream.ReadTimeout = CONNECT_TIMEOUT;
                connected = true;
                return true;
            }
            catch
            {
                connected = false;
                return false;
            }
        }

        /// <summary>
        /// Метод для отключения от модуля I/O
        /// </summary>
        public void Disconnect()
        {
            try
            {
                tcpClient?.Close();
            }
            catch { }
            connected = false;
        }

        /// <summary>
        /// Метод для проверки соединения: возвращает true если связь есть
        /// </summary>
        public bool CheckConnection()
        {
            try
            {
                if (!connected)
                {
                    if (!autoReconnectEnabled)
                        return false;

                    if (!Connect())
                        return false;
                }

                ReadRegister(CONNECT_TEST_REGISTER);
                return true;
            }
            catch
            {
                connected = false;
                return false;
            }
        }


        /// <summary>
        /// Запуск периодического опроса соединения
        /// </summary>
        public void StartPolling()
        {
            pollTimer = new Timer(_ => PollDevice(), null, 0, POLL_INTERVAL);
        }

        /// <summary>
        /// Остановка опроса соединения
        /// </summary>
        public void StopPolling()
        {
            pollTimer?.Dispose();
        }

        /// <summary>
        /// Метод для опроса соединения с модулем I/O: если автоматическое переподключение включено, то при потере связи будет предпринята попытка переподключиться. 
        /// Если статус подключения изменился, то вызывается событие ConnectionStatusChanged с новым статусом подключения
        /// </summary>
        private void PollDevice()
        {
            if (!autoReconnectEnabled)
                return;

            bool prevConnected = connected;
            connected = CheckConnection();

            if (prevConnected != connected)
            {
                ConnectionStatusChanged?.Invoke(connected);
            }
        }

        /// <summary>
        /// Метод для установки флага в true автоматического переподключения к модулю I/O при потере связи
        /// </summary>
        public void EnableAutoReconnect()
        {
            autoReconnectEnabled = true;
        }

        /// <summary>
        /// Метод для установки флага в false автоматического переподключения к модулю I/O при потере связи
        /// </summary>
        public void DisableAutoReconnect()
        {
            autoReconnectEnabled = false;
        }
        #endregion

        #region Методы для отправки сигналов на конкретные регистры для разрешения отбраковки, запуска процесса распознавания и управления светофором
        /// <summary>
        /// Разрешить физическую отбраковку
        /// </summary>
        public void AllowBreaker() => WriteRegister(BREAKER_ALLOW_REGISTER, (ushort)BREAKER_ALLOW_TRUE);

        /// <summary>
        /// Запретить физическую отбраковку
        /// </summary>
        public void DenyBreaker() => WriteRegister(BREAKER_ALLOW_REGISTER, (ushort)BREAKER_ALLOW_FALSE);

        /// <summary>
        /// Установить произвольное состояние разрешения отбраковки
        /// </summary>
        public void SetBreakerAllow(bool allow) => WriteRegister(BREAKER_ALLOW_REGISTER, (ushort)(allow ? BREAKER_ALLOW_TRUE : BREAKER_ALLOW_FALSE));

        /// <summary>
        /// Запустить процесс распознавания
        /// </summary>
        public void StartRecognizeProcessing() => WriteRegister(START_RECOGNIZE_PROCESSING_REGISTER, (ushort)RECOGNIZE_PROCESSING_START);

        /// <summary>
        /// Остановить процесс распознавания
        /// </summary>
        public void StopRecognizeProcessing() => WriteRegister(START_RECOGNIZE_PROCESSING_REGISTER, (ushort)RECOGNIZE_PROCESSING_FINISH);

        /// <summary>
        /// Включить оранжевый сигнал светофора
        /// </summary>
        public void TurnOnOrangeSemaphore() => WriteRegister(SEMAPHORE_ORANGE_REGISTER, (ushort)SEMAPHORE_ON);

        /// <summary>
        /// Выключить оранжевый сигнал светофора
        /// </summary>
        public void TurnOffOrangeSemaphore() => WriteRegister(SEMAPHORE_ORANGE_REGISTER, (ushort)SEMAPHORE_OFF);

        /// <summary>
        /// Включить зеленый сигнал светофора
        /// </summary>
        public void TurnOnGreenSemaphore() => WriteRegister(SEMAPHORE_GREEN_REGISTER, (ushort)SEMAPHORE_ON);

        /// <summary>
        /// Выключить зеленый сигнал светофора
        /// </summary>
        public void TurnOffGreenSemaphore() => WriteRegister(SEMAPHORE_GREEN_REGISTER, (ushort)SEMAPHORE_OFF);
        #endregion

        #region Запись настроек в ПР205
        /// <summary>
        /// Записать время отбраковки в ПР205
        ///</summary>
        public void ApplyBreakingTime() => WriteRegister(BREAKING_TIME_REGISTER, (ushort)_breakingTimeValue);

        /// <summary>
        /// Записать расстояние до камеры в ПР205
        /// </summary>
        public void ApplyCameraOffset() => WriteRegister(CAMERA_OFFSET_REGISTER, (ushort)_cameraOffsetValue);

        /// <summary>
        /// Записать расстояние до отбраковщика в ПР205
        /// </summary>
        public void ApplyBreakerOffset() => WriteRegister(BREAKER_OFFSET_REGISTER, (ushort)_breakerOffsetValue);

        /// <summary>
        /// Записать все настроечные параметры в ПР205
        /// </summary>
        public void ApplySettings()
        {
            ApplyBreakingTime();
            ApplyCameraOffset();
            ApplyBreakerOffset();
        }

        #endregion

        #region Запись/Чтение регистров
        /// <summary>
        /// Метод для записи значения в регистр модуля I/O: принимает номер регистра и значение для записи. Если запись не удалась, то устанавливает флаг connected в false
        /// </summary>
        /// <param name="register">Номер регистра</param>
        /// <param name="value">Значение для записи</param>
        public void WriteRegister(int register, ushort value)
        {
            try
            {
                if (!connected || stream == null)
                    throw new InvalidOperationException("Modbus client is not connected.");

                ushort transactionId = (ushort)Interlocked.Increment(ref transactionNumber);

                byte[] trans = BitConverter.GetBytes(transactionId);
                byte[] reg = BitConverter.GetBytes((ushort)register);
                byte[] val = BitConverter.GetBytes(value);

                byte[] request = new byte[]
                {
                    trans[1], trans[0],   // Transaction ID
                    0x00, 0x00,           // Protocol ID
                    0x00, 0x06,           // Length
                    0x01,                 // Unit ID
                    0x06,                 // Function code (Write Single Register)
                    reg[1], reg[0],
                    val[1], val[0]
                };

                lock (_ioLock)
                {
                    stream.Write(request, 0, request.Length);

                    byte[] response = new byte[12];
                    int bytesRead = stream.Read(response, 0, response.Length);
                }

            }
            catch
            {
                connected = false;
                throw;
            }
        }

        /// <summary>
        /// Метод для чтения значения из регистра модуля I/O: принимает номер регистра для чтения. Возвращает значение из регистра. Если чтение не удалось, то устанавливает флаг connected в false
        /// </summary>
        /// <param name="register">Номер регистра</param>
        /// <returns>Значение из регистра</returns>
        public ushort ReadRegister(int register)
        {
            try
            {
                if (!connected || stream == null)
                    throw new InvalidOperationException("Modbus client is not connected.");

                ushort transactionId = (ushort)Interlocked.Increment(ref transactionNumber);

                byte[] trans = BitConverter.GetBytes(transactionId);
                byte[] reg = BitConverter.GetBytes((ushort)register);

                byte[] request = new byte[]
                {
                    trans[1], trans[0],   // Transaction ID
                    0x00, 0x00,           // Protocol ID
                    0x00, 0x06,           // Length
                    0x01,                 // Unit ID
                    0x03,                 // Function code (Read Holding Register)
                    reg[1], reg[0],
                    0x00, 0x01            // Read 1 register
                };

                int bytesRead;
                byte[] response;
                lock (_ioLock)
                {
                    stream.Write(request, 0, request.Length);
                    response = new byte[11];
                    bytesRead = stream.Read(response, 0, response.Length);
                }

                ushort value = (ushort)(response[9] << 8 | response[10]);
                return value;
            }
            catch
            {
                connected = false;
                throw;
            }
        }
        #endregion
    }
}
