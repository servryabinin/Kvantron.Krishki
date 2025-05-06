using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Kvantron.Hardware.SmartDio
{
    internal class ModbusTCP
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
            this.ipAddress = ip;
            this.port = port;
        }

        public void Connect()
        {
            tcpClient = new TcpClient();
            IAsyncResult asyncResult = tcpClient.BeginConnect(ipAddress, port, null, null);
            if (!asyncResult.AsyncWaitHandle.WaitOne(connectTimeout))
                throw new Exception("connection timed out");
            tcpClient.EndConnect(asyncResult);
            stream = tcpClient.GetStream();
            stream.ReadTimeout = connectTimeout;
            connected = true;
        }

        public void Disconnect()
        {
            tcpClient.Close();
            connected = false;
        }

        public void WriteSingleRegister(int register, int state)
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


        public int[] ReadHoldingRegisters(int register, int count)
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
        }
    }
}