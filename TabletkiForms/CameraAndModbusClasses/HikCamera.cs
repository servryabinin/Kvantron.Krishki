using MvCamCtrl.NET;
using OpenCvSharp;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace KrishkiForms.CameraAndModbusClasses
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

        bool isGrabbing = false;

        Thread mainThread = null;

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
        /// Функция, которая останавливает съемку с камеры
        /// </summary>
        /// <returns></returns>
        public bool EndStream()
        {
            if (Streamed)
            {
				mainThread.Interrupt();

				isGrabbing = false;
				Streamed = isGrabbing;

				int nRet = m_MyCamera.MV_CC_StopGrabbing_NET();
				if (MyCamera.MV_OK != nRet)
				{
					Console.WriteLine("Stop grabbing failed:{0:x8}", nRet);
					return false;
				}
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
        /// Функция получения изображения
        /// </summary>
        private void ReceiveImageWorkThread()
        {
            int nRet = MyCamera.MV_OK;            
            
            MyCamera.MV_FRAME_OUT stImageOut = new MyCamera.MV_FRAME_OUT();
            MyCamera.MV_CC_INPUT_FRAME_INFO stInputFrameInfo = new MyCamera.MV_CC_INPUT_FRAME_INFO();

			mainThread = new Thread(() =>
            {
                while (isGrabbing)
                {                    
                    nRet = m_MyCamera.MV_CC_GetImageBuffer_NET(ref stImageOut, 1000);

                    if (nRet == MyCamera.MV_OK)
                    {
                        stInputFrameInfo.pData = stImageOut.pBufAddr;
                        stInputFrameInfo.nDataLen = stImageOut.stFrameInfo.nFrameLen;
                        nRet = m_MyCamera.MV_CC_InputOneFrame_NET(ref stInputFrameInfo);

                        Mat m = new Mat(stImageOut.stFrameInfo.nHeight, stImageOut.stFrameInfo.nWidth, MatType.CV_8UC3, stInputFrameInfo.pData);

                        Cv2.CvtColor(m, m, ColorConversionCodes.BGRA2RGB);
                        SendImage?.Invoke(m);


                        m_MyCamera.MV_CC_FreeImageBuffer_NET(ref stImageOut);

                        GC.Collect();
                    }
                    else
                    {
                        Console.WriteLine("Get Image failed:{0:x8}", nRet);
                    }
                }
            });
			mainThread.Start();            
        }
    }
}
