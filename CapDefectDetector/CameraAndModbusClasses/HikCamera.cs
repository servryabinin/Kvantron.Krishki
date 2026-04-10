using MvCamCtrl.NET;
using OpenCvSharp;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CapDefectDetector.CameraAndModbusClasses
{
    public delegate void Image(Mat mat);

    public enum AcquisitionMode
    {
        SingleFrame,
        Multi,
        Continuous        
    }

    public class HikCamera
    {
        //Acquisition Control
        public AcquisitionMode AcquisitionMode { get; set; } //режим получения кадров
        public bool FrameRateControlEnable { get; set; } //включить ограничение кадров
        public float FrameRate { get; set; } = 0; //кол-во кадров в секунду
        public bool TriggerMode { get; set; } //получение кадров по триггеру
        public float ExposureTime { get; set; } = 800; //экспозиция

        //Analog Control
        public float Gain { get; set; } //усиление
        public uint AOIWidth { get; set; } = 0; //ROI
        public uint AOIHeight { get; set; } = 0; //ROI
        public uint AOIOffsetX { get; set; } //ROI
        public uint AOIOffsetY { get; set; } //ROI
        public uint Width { get; set; }
        public uint Height { get; set; }

        public uint Saturation { get; set; }

        public bool Connected { get; set; } = false; //показывает статус подключения камеры
        public bool Streamed { get; set; } = false;

        public event Image SendImage;
        public string SerialNumber { get; set; }        
        private MyCamera m_MyCamera = null;        

        // ИСПРАВЛЕНО: заменили bare bool + Thread на CancellationToken + Task
        // для надёжного управления жизненным циклом потока захвата кадров
        private volatile bool isGrabbing = false;
        private CancellationTokenSource _streamCts;
        private Task _streamTask;

        // Устаревшее поле сохранено для совместимости, управление перенесено на Task
        Thread mainThread = null;
        public string IpAdress { get; private set; }


        public uint LastErrorCode { get; private set; } = 0; // новое свойство

        public HikCamera(string serialNumber)
        {
            SerialNumber = serialNumber;
        }

        private bool SetAcquisitionMode()
        {            
            int nRet = m_MyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)AcquisitionMode);
            return MyCamera.MV_OK != nRet ? false : true;            
        }

        public bool SetFrameRateControlEnable(bool FrameRateControlEnable)
        {            
            int nRet = m_MyCamera.MV_CC_SetBoolValue_NET("AcquisitionFrameRateEnable", FrameRateControlEnable);
            return MyCamera.MV_OK != nRet ? false : true;
        }

        public bool SetFrameRate(float FrameRate)
        {
            if (FrameRate != 0)
            {                
                int nRet = m_MyCamera.MV_CC_SetFrameRate_NET(FrameRate);
                return MyCamera.MV_OK == nRet;
            }
            else return false;            
        }

        public bool SetTriggerMode()
        {            
            int nRet = m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", Convert.ToUInt32(TriggerMode));
            return MyCamera.MV_OK == nRet;
        }

        public bool SetExposureTime()
        {
            if (ExposureTime != 0)
            {               
                int nRet = m_MyCamera.MV_CC_SetExposureTime_NET(ExposureTime);
                return MyCamera.MV_OK == nRet;
            }
            else return false;            
        }

        public bool SetGain()
        {            
            int nRet = m_MyCamera.MV_CC_SetGain_NET(Gain);
            return MyCamera.MV_OK == nRet;
        }

        public bool SetHeight()
        {
            if (Height != 0)
            {
                int nRet = m_MyCamera.MV_CC_SetHeight_NET(Height);
                return MyCamera.MV_OK == nRet;
            }
            else return false;
        }

       public bool SetWidth()
        {
            if (Width != 0)
            {
                int nRet = m_MyCamera.MV_CC_SetWidth_NET(Width);
                return MyCamera.MV_OK == nRet;
            }
            else return false;
        }

        public bool SetSaturation()
        {
            if (Saturation != 0)
            {
                int nRet = m_MyCamera.MV_CC_SetSaturation_NET(Saturation);
                return MyCamera.MV_OK == nRet;
            }
            else return false;
        }

        public bool SetAOIWidth()
        {
            if (AOIWidth != 0)
            {                
                int nRet = m_MyCamera.MV_CC_SetIntValue_NET("AutoFunctionAOIWidth", AOIWidth);
                return MyCamera.MV_OK == nRet;
            }
            else return false;            
        }
        public bool SetAOIHeight()
        {
            if (AOIHeight != 0)
            {                
                int nRet = m_MyCamera.MV_CC_SetIntValue_NET("AutoFunctionAOIHeight", AOIHeight);
                return MyCamera.MV_OK == nRet;
            }
            else return false;            
        }

        public bool SetAOIOffsetX()
        {            
            int nRet = m_MyCamera.MV_CC_SetIntValue_NET("AutoFunctionAOIOffsetX", AOIOffsetX);            
            return MyCamera.MV_OK == nRet;
        }

        public bool SetAOIOffsetY()
        {            
            int nRet = m_MyCamera.MV_CC_SetIntValue_NET("AutoFunctionAOIOffsetY", AOIOffsetY);
            return MyCamera.MV_OK == nRet;
        }

        /// <summary>
        /// Функция установки настроек камеры
        /// </summary>
        /// <returns></returns>
        public bool SetSettings()
        {
            int i = 0;
            i += Convert.ToInt32(SetAcquisitionMode());
            //i += Convert.ToInt32(SetFrameRateControlEnable());
            //i += Convert.ToInt32(SetFrameRate());
            i += Convert.ToInt32(SetTriggerMode());
            i += Convert.ToInt32(SetExposureTime());
            i += Convert.ToInt32(SetGain());
            i += Convert.ToInt32(SetAOIWidth());
            i += Convert.ToInt32(SetAOIHeight());
            i += Convert.ToInt32(SetAOIOffsetX());
            i += Convert.ToInt32(SetAOIOffsetY());

            return i != 0; 
        }

        /// <summary>
        /// Функция, которая открывает устройство по serial number
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            try
            {
                MyCamera.MV_CC_DEVICE_INFO_LIST m_stDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST
                {
                    nDeviceNum = 0
                };

                int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_stDeviceList);
                if (nRet != MyCamera.MV_OK)
                {
                    LastErrorCode = (uint)nRet;
                    return false;
                }

                if (m_stDeviceList.nDeviceNum == 0)
                {
                    LastErrorCode = 2147484169; // "Нет данных"
                    return false;
                }

                bool opened = false;

                for (int i = 0; i < m_stDeviceList.nDeviceNum; i++)
                {
                    MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                    string deviceSerial = string.Empty;

                    if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                        deviceSerial = new string(gigeInfo.chSerialNumber).TrimEnd('\0');
                        uint ip = gigeInfo.nCurrentIp;
                        IpAdress = $"{(ip >> 24) & 0xFF}.{(ip >> 16) & 0xFF}.{(ip >> 8) & 0xFF}.{ip & 0xFF}";
                    }
                    else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                    {
                        MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                        deviceSerial = new string(usbInfo.chSerialNumber).TrimEnd('\0');
                    }

                    if (!string.IsNullOrEmpty(this.SerialNumber) && !string.Equals(deviceSerial, this.SerialNumber, StringComparison.OrdinalIgnoreCase))
                        continue;

                    m_MyCamera = new MyCamera();
                    nRet = m_MyCamera.MV_CC_CreateDevice_NET(ref device);
                    if (nRet != MyCamera.MV_OK)
                    {
                        LastErrorCode = (uint)nRet;
                        m_MyCamera = null;
                        continue;
                    }

                    nRet = m_MyCamera.MV_CC_OpenDevice_NET();
                    if (nRet != MyCamera.MV_OK)
                    {
                        LastErrorCode = (uint)nRet;
                        try { m_MyCamera.MV_CC_DestroyDevice_NET(); } catch { }
                        m_MyCamera = null;
                        continue;
                    }

                    // Оптимальный пакет для GigE
                    if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        int nPacketSize = m_MyCamera.MV_CC_GetOptimalPacketSize_NET();
                        if (nPacketSize > 0)
                            m_MyCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                    }

                    Connected = true;
                    opened = true;
                    LastErrorCode = 0;
                    break;
                }

                if (!opened && LastErrorCode == 0)
                    LastErrorCode = 2147484163; // дефолтная ошибка "Нет доступа"

                return opened;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HikCamera.Open exception: {ex.Message}");
                Connected = false;
                LastErrorCode = 2147484166; // "Не поддерживается"
                return false;
            }
        }


        /// <summary>
        /// Функция, которая запускает сьемку с камеры
        /// </summary>
        /// <returns></returns>
        public bool StartStream()
        {
            if (!Streamed)
            {
				int nRet = m_MyCamera.MV_CC_StartGrabbing_NET();
				if (MyCamera.MV_OK != nRet)
				{
					Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
					return false;
				}

				isGrabbing = true;
				Streamed = isGrabbing;

				ReceiveImageWorkThread();
			}
            
            return true;
        }

        /// <summary>
        /// Останавливает съёмку с камеры.
        /// ИСПРАВЛЕНО: вместо устаревшего Thread.Interrupt() используем CancellationToken
        /// и ожидаем фактического завершения потока через Task.Wait с таймаутом.
        /// </summary>
        public bool EndStream()
        {
            if (!Streamed)
                return true;

            // 1. Сигнализируем потоку захвата об остановке
            isGrabbing = false;
            _streamCts?.Cancel();

            // 2. Ждём завершения задачи максимум 3 секунды (не зависаем навсегда)
            try
            {
                _streamTask?.Wait(TimeSpan.FromSeconds(3));
            }
            catch (AggregateException)
            {
                // OperationCanceledException — нормальное завершение по токену
            }
            finally
            {
                _streamCts?.Dispose();
                _streamCts = null;
                _streamTask = null;
            }

            Streamed = false;

            int nRet = m_MyCamera.MV_CC_StopGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                Console.WriteLine("Stop grabbing failed:{0:x8}", nRet);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Функция для запуска съемки по триггеру
        /// </summary>
        /// <returns></returns>
        public bool ExecuteTrigger()
        {            
            int nRet = m_MyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            return MyCamera.MV_OK == nRet;
        }

        /// <summary>
        /// Функия отключения от устройства
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            //Close Device
            m_MyCamera.MV_CC_CloseDevice_NET();
            m_MyCamera.MV_CC_DestroyDevice_NET();
            Connected = false;
            return true;
        }

        /// <summary>
        /// Запускает фоновый поток захвата кадров с камеры.
        ///
        /// ИСПРАВЛЕНО:
        ///   1. Убран GC.Collect() — принудительный сбор мусора на каждый кадр
        ///      вызывал паузы до 50-100 мс и пропуски кадров. Теперь GC работает
        ///      штатно: Mat.Dispose() освобождает нативную память, управляемая
        ///      куча очищается автоматически в фоне без вмешательства.
        ///
        ///   2. Thread заменён на Task с TaskCreationOptions.LongRunning —
        ///      получаем выделенный поток (не из ThreadPool) с корректным
        ///      управлением через CancellationToken.
        ///
        ///   3. Остановка теперь через _streamCts.Cancel() + Task.Wait(timeout)
        ///      вместо устаревшего Thread.Interrupt(), который мог не сработать
        ///      или вызвать ThreadInterruptedException в неожиданных местах.
        /// </summary>
        private void ReceiveImageWorkThread()
        {
            _streamCts = new CancellationTokenSource();
            var token = _streamCts.Token;

            _streamTask = Task.Factory.StartNew(() =>
            {
                MyCamera.MV_FRAME_OUT stImageOut = new MyCamera.MV_FRAME_OUT();
                MyCamera.MV_CC_INPUT_FRAME_INFO stInputFrameInfo = new MyCamera.MV_CC_INPUT_FRAME_INFO();

                while (isGrabbing && !token.IsCancellationRequested)
                {
                    int nRet = m_MyCamera.MV_CC_GetImageBuffer_NET(ref stImageOut, 1000);

                    if (nRet == MyCamera.MV_OK)
                    {
                        stInputFrameInfo.pData = stImageOut.pBufAddr;
                        stInputFrameInfo.nDataLen = stImageOut.stFrameInfo.nFrameLen;
                        nRet = m_MyCamera.MV_CC_InputOneFrame_NET(ref stInputFrameInfo);

                        Mat m = new Mat(
                            (int)stImageOut.stFrameInfo.nHeight,
                            (int)stImageOut.stFrameInfo.nWidth,
                            MatType.CV_8UC3,
                            stInputFrameInfo.pData);

                        Cv2.CvtColor(m, m, ColorConversionCodes.BGRA2RGB);

                        // Вызываем подписчиков; они сами клонируют Mat если нужно
                        SendImage?.Invoke(m);

                        m_MyCamera.MV_CC_FreeImageBuffer_NET(ref stImageOut);

                        // УДАЛЕНО: GC.Collect() — вызов принудительного сбора мусора
                        // на каждый кадр катастрофически снижает производительность.
                        // Mat корректно освобождает нативную память через Dispose/финализатор.
                    }
                    else
                    {
                        // Не спамим логами — просто даём другим потокам выполниться
                        if (!token.IsCancellationRequested)
                            Thread.Sleep(1);
                    }
                }
            },
            token,
            TaskCreationOptions.LongRunning, // выделенный поток, не из ThreadPool
            TaskScheduler.Default);
        }
    }
}
