using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

        public bool Connected { get { return connected; } }
        public ModbusTCP(string ip, int port)
        {
            ipAddress = ip;
            this.port = port;
        }

        public bool Connect()
        {
            tcpClient = new TcpClient();
            IAsyncResult asyncResult = tcpClient.BeginConnect(ipAddress, port, null, null);
            if (!asyncResult.AsyncWaitHandle.WaitOne(connectTimeout))
                throw new Exception("connection timed out");
            tcpClient.EndConnect(asyncResult);
            stream = tcpClient.GetStream();
            stream.ReadTimeout = connectTimeout;
            connected = true;
            if (connected)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Disconnect()
        {
            tcpClient.Close();
            connected = false;
        }

        public void WriteSingleRegisterForBreaker(int register, int state)
        {
            try
            {
                byte[] reg = BitConverter.GetBytes((ushort)register); // Получаем два байта регистра (младший и старший)

                byte[] data = new byte[]
                {
                    0x00, 0x1C,             // Transaction Identifier
                    0x00, 0x00,             // Protocol Identifier
                    0x00, 0x06,             // Length
                    0x01,                   // Unit Identifier
                    0x06,                   // Function Code (Write Single Register)
                    reg[1], reg[0],         // Register address (старший, младший байт)
                    0x00, (byte)state       // Value (0 — выключить, 1 — включить)
                };

                stream.Write(data, 0, data.Length);

                var array = new byte[256];
                int num = stream.Read(array, 0, array.Length);
            }
            catch (Exception)
            {
                connected = false;
                throw;
            }
        }

        public void WriteSingleRegisterForDelayBreakerAndCameraOffset(int register, int state)
        {
            try
            {
                byte[] reg = BitConverter.GetBytes((ushort)register);
                byte[] val = BitConverter.GetBytes((ushort)state);

                byte[] data = new byte[]
                {
                    0x00, 0x1C,             // Transaction Identifier
                    0x00, 0x00,             // Protocol Identifier
                    0x00, 0x06,             // Length
                    0x01,                   // Unit Identifier
                    0x06,                   // Function Code (Write Single Register)
                    reg[1], reg[0],         // Register address (big-endian)
                    val[1], val[0]          // Value (big-endian)
                };

                stream.Write(data, 0, data.Length);

                var array = new byte[256];
                int num = stream.Read(array, 0, array.Length);
            }
            catch (Exception)
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

                // Преобразуем адрес катушки в два байта
                byte[] coil = BitConverter.GetBytes((ushort)coilAddress);

                // 0xFF00 = включить катушку, 0x0000 = выключить
                ushort coilValue = (ushort)(state ? 0xFF00 : 0x0000);
                byte[] valueBytes = BitConverter.GetBytes(coilValue);

                // Формируем Modbus TCP запрос (Function Code 05 — Write Single Coil)
                byte[] request = new byte[]
                {
                    0x00, 0x02,             // Transaction ID
                    0x00, 0x00,             // Protocol ID
                    0x00, 0x06,             // Length
                    0x01,                   // Unit ID
                    0x05,                   // Function Code (Write Single Coil)
                    coil[1], coil[0],       // Coil Address (старший, младший байт)
                    valueBytes[1], valueBytes[0]  // Coil Value (старший, младший байт)
                };

                // Отправляем запрос
                stream.Write(request, 0, request.Length);

                // Читаем ответ (8 байт)
                byte[] response = new byte[8];
                int bytesRead = stream.Read(response, 0, response.Length);

                if (bytesRead != 8 || response[7] != 0x05)
                    throw new Exception("Invalid Modbus coil write response");
            }
            catch (Exception)
            {
                connected = false;
                throw;
            }
        }



        public int ReadSingleRegister(int register)
        {
            try
            {
                // Генерируем новый transaction ID
                ushort transactionId = (ushort)Interlocked.Increment(ref transactionNumber);
                byte[] transactionBytes = BitConverter.GetBytes(transactionId);

                byte[] reg = BitConverter.GetBytes((ushort)register);
                byte[] request = new byte[] {
                    transactionBytes[1], transactionBytes[0],  // Transaction ID
                    0x00, 0x00,                               // Protocol ID
                    0x00, 0x06,                               // Length
                    0x01,                                     // Unit ID
                    0x03,                                     // Function Code (Read Holding Registers)
                    reg[1], reg[0],                           // Register address (старший, младший байт)
                    0x00, 0x01                                // Quantity of registers (1)
                };

                stream.Write(request, 0, request.Length);

                // Читаем ответ (9 байт заголовка + 2 байта данных)
                byte[] response = new byte[11];
                int bytesRead = stream.Read(response, 0, response.Length);

                // Проверяем корректность ответа:
                if (bytesRead != 11 || response[7] != 0x03 || response[8] != 0x02)
                    throw new Exception("Invalid Modbus response");

                // Извлекаем значение регистра (16-битное число)
                ushort value = (ushort)(response[9] << 8 | response[10]);

                // Возвращаем 0 или 1 (если регистр хранит булево значение)
                return value != 0 ? 1 : 0;
            }
            catch (Exception)
            {
                connected = false;
                throw;
            }
        }

        /*public int[] ReadHoldingRegisters(int register, int count)
        {
            try
            {
                var reg = BitConverter.GetBytes(register);
                byte[] data = new byte[] { 0x00, 0x03, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, reg[1], reg[0], 0x00, 0x01 };
                stream.Write(data, 0, data.Length);
                var array = new byte[256];
                int num = stream.Read(array, 0, array.Length);
                return new int[1] { array[10] };
            }
            catch (Exception)
            {
                connected = false;
                throw;
            }
        }*/
    }
}