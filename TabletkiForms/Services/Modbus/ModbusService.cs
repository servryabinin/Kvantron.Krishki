using System;
using System.Threading;
using System.Threading.Tasks;
using KrishkiForms.Models;
using KrishkiForms.Services.Abstractions;

namespace KrishkiForms.Services.Modbus
{
    /// <summary>
    /// Реализация сервиса Modbus TCP.
    /// </summary>
    public class ModbusService : IModbusService
    {
        private ModbusTCP _modbusClient;
        private readonly object _lock = new object();
        private bool _isDisposed;

        public event EventHandler<bool> ConnectionStatusChanged;

        public bool IsConnected => _modbusClient?.Connected ?? false;

        public ModbusConnectionInfo CurrentConnectionInfo
        {
            get
            {
                if (_modbusClient == null)
                    return null;

                return new ModbusConnectionInfo
                {
                    IpAddress = _modbusClient.IpAddress,
                    Port = _modbusClient.Port,
                    IsConnected = _modbusClient.Connected
                };
            }
        }

        public ModbusService()
        {
            // Ничего не создаём заранее
        }

        public async Task<bool> ConnectAsync(string ipAddress, int port)
        {
            await DisconnectAsync();

            var client = new ModbusTCP(ipAddress, port);
            bool connected = await Task.Run(() => client.Connect());

            if (connected)
            {
                // Подписываемся на события
                client.ConnectionStatusChanged += OnConnectionStatusChanged;

                lock (_lock)
                {
                    _modbusClient = client;
                }

                ConnectionStatusChanged?.Invoke(this, true);
            }

            return connected;
        }

        public async Task DisconnectAsync()
        {
            if (_modbusClient == null)
                return;

            await Task.Run(() =>
            {
                _modbusClient.StopPolling();
                _modbusClient.Disconnect();
                _modbusClient.ConnectionStatusChanged -= OnConnectionStatusChanged;
            });

            lock (_lock)
            {
                _modbusClient = null;
            }

            ConnectionStatusChanged?.Invoke(this, false);
        }

        public async Task WriteRegisterAsync(int register, ushort value)
        {
            if (_modbusClient == null || !_modbusClient.Connected)
                throw new InvalidOperationException("Modbus client is not connected.");

            await Task.Run(() => _modbusClient.WriteRegister(register, value));
        }

        public async Task<ushort> ReadRegisterAsync(int register)
        {
            if (_modbusClient == null || !_modbusClient.Connected)
                throw new InvalidOperationException("Modbus client is not connected.");

            return await Task.Run(() => _modbusClient.ReadRegister(register));
        }

        public void EnableAutoReconnect()
        {
            _modbusClient?.EnableAutoReconnect();
        }

        public void DisableAutoReconnect()
        {
            _modbusClient?.DisableAutoReconnect();
        }

        public void StartPolling(int intervalMilliseconds = 5000)
        {
            _modbusClient?.StartPolling();
        }

        public void StopPolling()
        {
            _modbusClient?.StopPolling();
        }

        private void OnConnectionStatusChanged(bool isConnected)
        {
            ConnectionStatusChanged?.Invoke(this, isConnected);
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    DisconnectAsync().GetAwaiter().GetResult();
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}