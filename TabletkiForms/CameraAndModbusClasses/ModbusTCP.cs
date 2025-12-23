using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Threading.Timer;

namespace KrishkiForms.CameraAndModbusClasses
{
    public class ModbusTCP
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private string ipAddress = "127.0.0.1";
        private int port = 502;
        private int connectTimeout = 1000;
        private bool connected = false;
        private int transactionNumber = 0;
        private Timer pollTimer;
        private int pollInterval = 5000; // 5 секунд
        private int connectTestRegister = 16403;

        private bool autoReconnectEnabled = true;

        // Событие для уведомления об изменении соединения
        public event Action<bool> ConnectionStatusChanged;

        public bool Connected => connected;

        public ModbusTCP(string ip, int port)
        {
            ipAddress = ip;
            this.port = port;
        }

        public void EnableAutoReconnect()
        {
            autoReconnectEnabled = true;
        }

        public void DisableAutoReconnect()
        {
            autoReconnectEnabled = false;
        }


        public bool Connect()
        {
            try
            {
                tcpClient = new TcpClient();
                IAsyncResult asyncResult = tcpClient.BeginConnect(ipAddress, port, null, null);
                if (!asyncResult.AsyncWaitHandle.WaitOne(connectTimeout))
                    throw new Exception("connection timed out");
                tcpClient.EndConnect(asyncResult);
                stream = tcpClient.GetStream();
                stream.ReadTimeout = connectTimeout;
                connected = true;
                return true;
            }
            catch
            {
                connected = false;
                return false;
            }
        }

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

                ReadSingleRegister(connectTestRegister);
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
            pollTimer = new Timer(_ => PollDevice(), null, 0, pollInterval);
        }

        /// <summary>
        /// Остановка опроса соединения
        /// </summary>
        public void StopPolling()
        {
            pollTimer?.Dispose();
        }

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


        public void WriteSingleRegisterForBreaker(int register, int state)
        {
            try
            {
                byte[] reg = BitConverter.GetBytes((ushort)register);

                byte[] data = new byte[]
                {
                    0x00, 0x1C,
                    0x00, 0x00,
                    0x00, 0x06,
                    0x01,
                    0x06,
                    reg[1], reg[0],
                    (byte)((state >> 8) & 0xFF), (byte)(state & 0xFF)
                };

                stream.Write(data, 0, data.Length);
                stream.Read(new byte[256], 0, 256);
            }
            catch
            {
                connected = false;
                throw;
            }
        }

        public void WriteSingleRegisterForDelayBreakerAndCameraOffset(int register, int state)
        {
            try
            {
                ushort transactionId = (ushort)new Random().Next(1, 65535);
                byte[] trans = BitConverter.GetBytes(transactionId);
                byte[] reg = BitConverter.GetBytes((ushort)register);
                byte[] val = BitConverter.GetBytes((ushort)state);

                byte[] data = new byte[]
                {
                    trans[1], trans[0],
                    0x00, 0x00,
                    0x00, 0x06,
                    0x01,
                    0x06,
                    reg[1], reg[0],
                    val[1], val[0]
                };

                stream.WriteTimeout = 1000;
                stream.ReadTimeout = 1000;
                stream.Write(data, 0, data.Length);

                var response = new byte[256];
                int bytesRead = stream.Read(response, 0, response.Length);
            }
            catch
            {
                connected = false;
                throw;
            }
        }

        public void WriteSingleCoil(int coilAddress, bool state)
        {
            try
            {
                if (!connected || stream == null)
                    throw new InvalidOperationException("Modbus client is not connected.");

                byte[] coil = BitConverter.GetBytes((ushort)coilAddress);
                ushort coilValue = (ushort)(state ? 0xFF00 : 0x0000);
                byte[] valueBytes = BitConverter.GetBytes(coilValue);

                byte[] request = new byte[]
                {
                    0x00, 0x02,
                    0x00, 0x00,
                    0x00, 0x06,
                    0x01,
                    0x05,
                    coil[1], coil[0],
                    valueBytes[1], valueBytes[0]
                };

                stream.Write(request, 0, request.Length);

                byte[] response = new byte[8];
                int bytesRead = stream.Read(response, 0, response.Length);

                if (bytesRead != 8 || response[7] != 0x05)
                    throw new Exception("Invalid Modbus coil write response");
            }
            catch
            {
                connected = false;
                throw;
            }
        }

        public int ReadSingleRegister(int register)
        {
            try
            {
                ushort transactionId = (ushort)Interlocked.Increment(ref transactionNumber);
                byte[] transactionBytes = BitConverter.GetBytes(transactionId);

                byte[] reg = BitConverter.GetBytes((ushort)register);
                byte[] request = new byte[] {
                    transactionBytes[1], transactionBytes[0],
                    0x00, 0x00,
                    0x00, 0x06,
                    0x01,
                    0x03,
                    reg[1], reg[0],
                    0x00, 0x01
                };

                stream.Write(request, 0, request.Length);

                byte[] response = new byte[11];
                int bytesRead = stream.Read(response, 0, response.Length);

                ushort value = (ushort)(response[9] << 8 | response[10]);

                return value;
            }
            catch
            {
                connected = false;
                throw;
            }
        }
    }
}
