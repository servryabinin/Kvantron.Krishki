//#define OLD_FRAME_PROCESSING

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using KrishkiForms.CameraAndModbusClasses;
using KrishkiForms.FrameProcessing;
using KrishkiForms.Hardware;
using Kvantron.Hardware.SmartDio;
using Kvantron.UI.Controls.Utils;
using MathNet.Numerics.IntegralTransforms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace KrishkiForms
{
    public partial class TestForm : Form
    {
        #region Поля и константы

        // UI элементы
        private System.Windows.Forms.ToolTip tooltip = new System.Windows.Forms.ToolTip();
        private Label labelCoordinates = new Label();
        private Color connectedColor = Color.FromArgb(229, 115, 115); // красный — отключить
        private Color disconnectedColor = Color.FromArgb(4, 85, 191); // синий — подключить
        private bool capsAreWhite = false;

        // Оборудование
        private DioModule module = null;
        private HikCamera cam;
        private ModbusTCP modbusClient;
        private int breakingTimeRegister = 16466;
        private int cameraOffsetRegister = 16402;
        private int breakerOffsetRegister = 16399;
        private int breakerAllowRegister = 16401;
        private int startRecognizeProcessing = 16400;
        private int BreakingTime = 55;
        private int CameraOffset = 300;
        private int BreakerOffset = 2430;
        private int BreakerAllowTrue = 1;
        private int BreakerAllowFalse = 0;
        private int RecognizeProcessingAndBreakerAllowFinish = 0;

        // Состояния приложения
        private bool cameraConnected = false;
        private bool prConnected = false;
        private bool currentDetectorState = false;
        private bool isFirstImageCam1 = false;
        private bool cameraError1 = false;
        private bool isStreamCam = false;
        private bool isProcessing = false;
        private bool isDrawing = false;
        private bool isROISelected = false;
        private bool isRoiProduce = false;
        private bool isConfirmVisible = false;
        private bool obduvEnabled = false;
        private bool isProcessingFromFolder = false;
        private bool isImageLoaded = false;
        private bool isStreamRunning = false;


        // Изображения и обработка
        private Mat img1 = new Mat();
        private Bitmap originalImage = null;
        private Mat croppedImage = null;
        private OpenCvSharp.Rect roi;
        private Rectangle selectedROI;
        private System.Drawing.Point startPoint;
        private Rectangle checkRect, crossRect;

        // Таймеры и многопоточность
        private CancellationTokenSource cts;
        private Task processingTask;
        private int frameCounter = 0;
        private System.Windows.Forms.Timer frameProcessingTimer;

        // Блокировки и синхронизация
#if OLD_FRAME_PROCESSING
        private readonly object frameLock = new object();
        private volatile bool newFrameAvailable = false;
        private Mat latestFrame = null;
#else
        private readonly FrameBuffer _imageQueue = new();
#endif
        private Mat pictureForOutput;

        // Счетчики дефектов
        private int ovalityCount = 0;
        private int inclusionCount = 0;
        private int paintDefectCount = 0;
        private int obloyDefectCount = 0;
        private int underfillCount = 0;
        private int blowTriggerCount = 0;
        private int ellipseReject = 0;
        private int sizeReject = 0;
        private int defectReject = 0;
        private bool cleanupRunning = false;
        private const int MAX_IMAGES_PER_DEFECT = 600;

        // Параметры обработки изображений
        private int MIN_DIAMETER_PX = 500;
        private int MAX_DIAMETER_PX = 700;
        private int HOUGH_DETECT_PARAM_1 = 170;
        private int HOUGH_DETECT_PARAM_2 = 85;
        private int STRIPED_IMAGE_HEIGHT = 30;
        private double TRESHOLD = 2.25;
        private double SCALE = 0.5;
        private const int MIN_HARMONIC_INDEX = 12;
        private const int MAX_HARMONIC_INDEX = 30;

        // LUT и цветовые параметры
        private byte[] HUE_LUT = new byte[128 * 128 * 128];
        private byte capsColor = 0;
        private int delayValue;
        private const byte BLUE_CAPS = 80;  //160
        private const byte YELLOW_CAPS = 112;  //160
        private const byte GOLD_CAPS = 112;  //160
        private const byte WHITE_CAPS = 96;  //160
        private const byte GREEN_CAPS = 255;
        private const byte GREEN_THRESHOLD = 47;
        private static bool isGreenColor = false;
        private static bool isColored = true;

        // Морфологические элементы
        private Mat element1;
        private Mat element2;
        private Mat elementMask;
        private int window = 15;
        private int morph_size = 7;
        private int morph_size_2 = 7;

        // Параметры дефектов
        private double ovalityThreshold = 0.7;
        private double minInpaintWhiteTgreshold = 150.0;
        private double minAreaInpaintDefect = 500.0;
        private double inclusionThreshold = 0.5;
        private double minAreaInclusion = 50.0;
        private double maxAreaInclusion = 500.0;
        private double minAreaObloy = 1000.0;

        // Контуры и геометрия
        private Point[] largestContourOvality;
        private Point[] largestContourObloy;
        private Point capCenter;
        private double majorAxis;
        private double minorAxis;
        private double axisRatio;

        // Маски и изображения для обработки
        private Mat CapRadiusMask = new Mat(532, 568, MatType.CV_8UC1);
        private Mat blurChannel_0;
        private Mat blurChannel_1;
        private Mat blurChannel_2;
        private const float STANDARD_CAP_MAX_RADIUS = 156.0f;
        private const float CAP_FLASH_OFFSET = 5.0f;
        private const int MIN_BINARY_PIXELS_FOR_FLASH_DECISION = 15;

        // Изображения для различных проверок
        private Mat _imageForOvality;
        private Mat _grayForOvality;
        private Mat _imageForInclusions;
        private Mat _grayForInclusions;
        private Mat _imageForPaintDefects;
        private Mat _grayForPaintDefects;
        private Mat _imageForObloyDefects;
        private Mat _grayForObloyDefects;
        private Mat _imageForUnderfill;
        private Mat _grayForUnderfill;

        // Пути и файлы
        private string settingsFilePath;
        private string ovalityDefectPath;
        private string paintDefectPath;
        private string inclusionDefectPath;
        private string obloyDefectPath;
        private string fileNameForOvalityDefect = $"ovality_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bmp";
        private string fileNameForPaintDefect = $"paint_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bmp";
        private string fileNameForInclusionDefect = $"inclusion_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bmp";
        private string fileNameForObloyDefect = $"obloy_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bmp";
        private string fullPathForOvalityDefect = "";
        private string fullPathForPaintDefect = "";
        private string fullPathForInclusionDefect = "";
        private string fullPathForObloyDefect = "";

        // Логирование
        private DateTime? _lastImageReceivedTime = null;
        private readonly string _logFilePath = "SendImageLog.txt";
        private readonly string _processTimeLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProcessTime.txt");
        private ImmutableList<string> imageFiles = [];
        private int currentImageIndex = 0;
        private int _writeZeroFailCount = 0;
        private int _writeOneFailCount = 0;
        private int currentFrameNumber = 0;
        #endregion

        #region Конструктор и инициализация

        public TestForm(HikCamera camera, ModbusTCP modbus)
        {
            // Проверяем, что камера не null и подключена
            if (camera != null)
            {
                cam = camera;
                cameraConnected = true;
            }
            else
            {
                cam = null;
                cameraConnected = false;
            }

            if (modbus != null)
            {
                modbusClient = modbus;
                prConnected = true;
            }
            else
            {
                modbusClient = null;
                prConnected = false;
            }

            InitializeComponent();
            InitializeApplication();
            SendStopSignalsToPLC();
        }

        private void InitializeApplication()
        {
            InitializeHueLUT();
            InitializeImageMatrices();
            InitializeMorphologicalElements();
            InitializePaths();
            LoadPathsFromSettings();
            Form1_Load();

            recognizeButton.Enabled = false;


            // --- ПР205 ---
            if (modbusClient != null && modbusClient.Connected)
            {
                prStatus.Text = "Подключено";
                prStatus.ForeColor = Color.Green;
                connectPrButton.Text = "Отключиться от ПР";
                connectPrButton.BackColor = connectedColor;
                startStreamButton.Enabled = true;
            }
            else
            {
                prStatus.Text = "Не подключено";
                prStatus.ForeColor = Color.Red;
                connectPrButton.Text = "Подключиться к ПР";
                connectPrButton.BackColor = disconnectedColor;
                startStreamButton.Enabled = false;
            }

            // --- Камера ---
            if (cam != null && cam.Connected)
            {
                cam.SendImage += GetImage;
                camStatus.Text = "Подключено";
                camStatus.ForeColor = Color.Green;
                connectCameraButton.Text = "Отключиться";
                connectCameraButton.BackColor = connectedColor;
            }
            else
            {
                camStatus.Text = "Не подключено";
                camStatus.ForeColor = Color.Red;
                connectCameraButton.Text = "Подключиться";
                connectCameraButton.BackColor = disconnectedColor;
            }

            StartStop(false, true);
            LocalSettings.Instance.Save();
        }


        private void InitializeImageMatrices()
        {
            _imageForOvality = new Mat();
            _grayForOvality = new Mat();
            _imageForInclusions = new Mat();
            _grayForInclusions = new Mat();
            _imageForPaintDefects = new Mat();
            _grayForPaintDefects = new Mat();
            _imageForObloyDefects = new Mat();
            _grayForObloyDefects = new Mat();
            _imageForUnderfill = new Mat();
            _grayForUnderfill = new Mat();
        }

        private void InitializeMorphologicalElements()
        {
            int morphSize = 4;
            int morphSize2 = 4;

            element1 = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new Size(2 * morphSize + 1, 2 * morphSize + 1),
                new Point(morphSize, morphSize)
            );

            element2 = Cv2.GetStructuringElement(
                MorphShapes.Cross,
                new Size(2 * morphSize2 + 1, 2 * morphSize2 + 1),
                new Point(morphSize2, morphSize2)
            );

            elementMask = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new Size(3, 3),
                new Point(1, 1)
            );
        }

        private void InitializePaths()
        {
            settingsFilePath = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\")), "файлы настроек");
            ovalityDefectPath = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\")), "дефектные крышки", "овальность");
            paintDefectPath = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\")), "дефектные крышки", "непрокрас");
            inclusionDefectPath = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\")), "дефектные крышки", "вкрапления");
            obloyDefectPath = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\")), "дефектные крышки", "облой");
        }

        private void SendStopSignalsToPLC()
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    // Сбрасываем breakerAllowRegister
                    modbusClient.WriteSingleRegisterForBreaker(breakerAllowRegister, 0);

                    // Сбрасываем startRecognizeProcessing
                    if (!isImageLoaded)
                        modbusClient.WriteSingleRegisterForBreaker(startRecognizeProcessing, 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка отправки в PLC: " + ex.Message);
            }
        }

        #endregion

        #region Подключение к оборудованию

        private void ConnectToModbus()
        {
            try
            {
                modbusClient = new ModbusTCP("10.10.69.38", 502);
                modbusClient.Connect();

                if (modbusClient.Connected)
                {
                    MessageBox.Show("Modbus подключение к ПР205 установлено.");
                    prStatus.Text = "Подключено";
                    prStatus.ForeColor = Color.Green;
                    //button10.Text = "Отключиться от ПР";
                    prConnected = true;
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к ПР205.");
                    prStatus.Text = "Не подключено";
                    prStatus.ForeColor = Color.Red;
                    //button10.Text = "Подключиться к ПР";
                    prConnected = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к ПР205: {ex.Message}");
                prStatus.Text = "Не подключено";
                prStatus.ForeColor = Color.Red;
            }
        }

        private void ConnectToCamera()
        {
            if (cam.Open())
            {
                cam.SendImage += GetImage;
                camStatus.Text = "Подключено";
                camStatus.ForeColor = Color.Green;
                connectCameraButton.Text = "Отключиться от камеры";
                cameraConnected = true;
            }
            else
            {
                MessageBox.Show("Камера 1 - ошибка");
                camStatus.Text = "Не подключено";
                camStatus.ForeColor = Color.Red;
                connectCameraButton.Text = "Подключиться к камере";
                cameraConnected = false;
                cameraError1 = true;
            }
        }

        private void ConnectToModule()
        {
            if (LocalSettings.Instance.UseModule)
            {
                try
                {
                    module = DioModule.CreateMK210(LocalSettings.Instance.ModuleIP, 502);
                    if (module != null)
                    {
                        module.Connect();
                        module.DiStateChanged += GetModuleState;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        #endregion

        #region Обработчики событий UI

        #region Кнопки управления оборудованием

        private void connectCameraButton_Click(object sender, EventArgs e)
        {


            if (cameraConnected)
            {
                cam.Close();
                cameraConnected = false;
                connectCameraButton.Text = "Подключиться";
                connectCameraButton.BackColor = disconnectedColor;

                camStatus.Text = "Не подключено";
                camStatus.ForeColor = Color.Red;

                startStreamButton.Enabled = false;
            }
            else
            {
                if (cam.Open())
                {
                    cam.SendImage += GetImage;
                    cameraConnected = true;
                    connectCameraButton.Text = "Отключиться";
                    connectCameraButton.BackColor = connectedColor;

                    camStatus.Text = "Подключено";
                    camStatus.ForeColor = Color.Green;

                    startStreamButton.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к камере.");
                    camStatus.Text = "Не подключено";
                    camStatus.ForeColor = Color.Red;

                    cameraConnected = false;
                    connectCameraButton.Text = "Подключиться";
                    connectCameraButton.BackColor = disconnectedColor;

                    startStreamButton.Enabled = false;
                }
            }
        }


        private void connectPrButton_Click(object sender, EventArgs e)
        {

            if (prConnected && modbusClient != null && modbusClient.Connected)
            {
                try
                {
                    modbusClient.Disconnect();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отключении от ПР205: {ex.Message}");
                }

                prConnected = false;
                connectPrButton.Text = "Подключиться к ПР";
                connectPrButton.BackColor = disconnectedColor;

                prStatus.Text = "Не подключено";
                prStatus.ForeColor = Color.Red;
                return;
            }

            try
            {
                string ip = prIpTextBox.Text.Trim();
                int port = int.Parse(pr205PortTb.Text.Trim());
                modbusClient = new ModbusTCP(ip, port);
                modbusClient.Connect();

                if (modbusClient.Connected)
                {
                    prConnected = true;
                    MessageBox.Show("Modbus подключение к ПР205 установлено.");

                    prStatus.Text = "Подключено";
                    prStatus.ForeColor = Color.Green;

                    connectPrButton.Text = "Отключиться от ПР";
                    connectPrButton.BackColor = connectedColor;
                }
                else
                {
                    prConnected = false;
                    MessageBox.Show("Не удалось подключиться к ПР205.");

                    prStatus.Text = "Не подключено";
                    prStatus.ForeColor = Color.Red;

                    connectPrButton.Text = "Подключиться к ПР";
                    connectPrButton.BackColor = disconnectedColor;
                }
            }
            catch (Exception ex)
            {
                prConnected = false;
                MessageBox.Show($"Ошибка подключения к ПР205: {ex.Message}");

                prStatus.Text = "Не подключено";
                prStatus.ForeColor = Color.Red;

                connectPrButton.Text = "Подключиться к ПР";
                connectPrButton.BackColor = disconnectedColor;
            }
        }


        #endregion

        #region Кнопки управления изображениями

        private void loadImageButton_Click(object sender, EventArgs e)
        {
            if (isImageLoaded)
            {
                isStreamCam = false;
                originPb.Image?.Dispose();
                originPb.Image = null;
                inclusionPb.Image?.Dispose();
                inclusionPb.Image = null;

                imageFiles = [];
                isProcessingFromFolder = false;

                // Восстанавливаем внешний вид кнопки
                loadImageButton.Text = "Загрузить";
                loadImageButton.BackColor = Color.FromArgb(66, 133, 244); // Синий

                // Разблокируем кнопку запуска потока
                startStreamButton.Enabled = true;

                isImageLoaded = false;
                recognizeButton.Enabled = false;

            }
            else
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Multiselect = true;
                    openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        imageFiles = [.. openFileDialog.FileNames];
                        currentImageIndex = 0;
                        isProcessingFromFolder = imageFiles.Count > 0;

                        if (imageFiles.Count > 0)
                        {
                            LoadAndDisplayCurrentImage();

                            // Меняем внешний вид кнопки
                            loadImageButton.Text = "Отключить";
                            loadImageButton.BackColor = Color.FromArgb(229, 115, 115); // Красный

                            // Блокируем кнопку старта потока
                            startStreamButton.Enabled = false;

                            isImageLoaded = true;
                            recognizeButton.Enabled = true;
                        }
                    }
                }
            }
        }


        private void startStreamButton_Click(object sender, EventArgs e)
        {
            if (!isStreamRunning)
            {
                StartStream();
            }
            else
            {
                StopStream();
            }
        }

        private void StartStream()
        {
            if (originalImage != null)
            {
                originalImage = null;
            }

            isStreamCam = true;
            ApplyRecognitionParameters();

            // 🔒 Блокируем кнопки подключения
            connectCameraButton.Enabled = false;
            connectPrButton.Enabled = false;
            loadImageButton.Enabled = false;

            // Блокируем настройки во время стрима
            ovalityCoef.Enabled = false;
            circleCoefTx.Enabled = false;
            minSquareInclusion.Enabled = false;
            maxSquareInclusion.Enabled = false;
            minSquareInpaint.Enabled = false;
            whiteThresoldTx.Enabled = false;
            obloyPixCount.Enabled = false;

            StartStop(true);

            startStreamButton.Text = "Остановить";
            startStreamButton.BackColor = Color.FromArgb(229, 115, 115);
            cameraStatusLabel.Text = "Запущен";
            cameraStatusLabel.ForeColor = Color.Green;
            isStreamRunning = true;
            recognizeButton.Enabled = true;

        }


        private void StopStream()
        {
            isRoiProduce = false;
            isROISelected = false;
            isStreamCam = false;

            originPb.Image?.Dispose();
            originPb.Image = null;

            // 🔓 Разблокируем кнопки подключения
            connectCameraButton.Enabled = true;
            connectPrButton.Enabled = true;
            loadImageButton.Enabled = true;


            // Разблокируем настройки
            ovalityCoef.Enabled = true;
            circleCoefTx.Enabled = true;
            minSquareInclusion.Enabled = true;
            maxSquareInclusion.Enabled = true;
            minSquareInpaint.Enabled = true;
            whiteThresoldTx.Enabled = true;
            obloyPixCount.Enabled = true;

            StartStop(false);

            startStreamButton.Text = "Запустить";
            startStreamButton.BackColor = Color.FromArgb(66, 133, 244);
            cameraStatusLabel.Text = "Не запущен";
            cameraStatusLabel.ForeColor = Color.Black;
            isStreamRunning = false;
            recognizeButton.Enabled = false;

        }


        #endregion

        #region Кнопки обработки и распознавания

        private async void recognizeButton_Click(object sender, EventArgs e)
        {
            if (isProcessing)
            {
                await StopProcessingAsync();
            }
            else
            {
                StartProcessing();
            }
        }

        private void obduvBatton_Click_1(object sender, EventArgs e)
        {
            if (modbusClient == null || !modbusClient.Connected)
            {
                MessageBox.Show("ПР205 не подключён.");
                return;
            }

            try
            {
                obduvEnabled = !obduvEnabled;
                int register = PLCData.QualityRegisterModbus;
                int state = obduvEnabled ? 1 : 0;

                modbusClient.WriteSingleRegisterForBreaker(register, state);
                //obduvBatton.Text = obduvEnabled ? "Включить обдув" : "Выключить обдув";
            }
            catch (Exception ex)
            {
                // Ошибка отправки сигнала
            }
        }

        #endregion

        #region Кнопки ROI

        private void drawRoi_Click(object sender, EventArgs e)
        {
            isDrawing = true;
            isRoiProduce = true;
            isConfirmVisible = false;
            originPb.MouseDown += OriginPictureBox_MouseDown;
            originPb.MouseMove += OriginPictureBox_MouseMove;
            originPb.MouseUp += OriginPictureBox_MouseUp;
            originPb.Paint += OriginPictureBox_Paint;
            originPb.MouseClick += OriginPictureBox_MouseClick;
        }

        private void redrawRoi_Click(object sender, EventArgs e)
        {
            isROISelected = false;
            croppedImage = null;
        }

        #endregion

        #region Кнопки настроек

        #region Настройки камеры
        private void ApplySettingsButton_Click(object sender, EventArgs e)
        {
            ApplyCameraSettings();
        }

        private void applySettingsButton_Click(object sender, EventArgs e)
        {
            ApplyCameraSettings();
        }

        private void saveSettingsButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void loadSettingsButton_Click(object sender, EventArgs e)
        {
            LoadSettings();
        }

        #endregion

        #region Настройки ПР

        private void savePrSettings_Click(object sender, EventArgs e)
        {
            if (!ValidatePrSettings())
                return;

            try
            {
                var settings = new
                {
                    IPAddress = prIpTextBox.Text,
                    Port = pr205PortTb.Text,
                    BreakingTime = breakingTimeTb.Text,
                    CameraOffset = cameraOffsetTb.Text,
                    BreakerOffset = breakerOffsetTb.Text
                };

                string json = System.Text.Json.JsonSerializer.Serialize(settings,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    saveFileDialog.Title = "Сохранить настройки ПР205";
                    saveFileDialog.FileName = "pr205_settings.json";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveFileDialog.FileName, json);

                        Properties.Settings.Default.BreakingTime = breakingTimeTb.Text;
                        Properties.Settings.Default.CameraOffset = cameraOffsetTb.Text;
                        Properties.Settings.Default.BreakerOffset = breakerOffsetTb.Text;
                        Properties.Settings.Default.IpAdressPr = prIpTextBox.Text;
                        Properties.Settings.Default.PortPr = pr205PortTb.Text;

                        // сохраняем изменения в Settings
                        Properties.Settings.Default.Save();

                        MessageBox.Show("Настройки ПР205 успешно сохранены.", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении настроек ПР205: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadPrSettings_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    openFileDialog.Title = "Загрузить настройки ПР205";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string json = File.ReadAllText(openFileDialog.FileName);
                        var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        if (settings != null)
                        {
                            // Загружаем IP адрес
                            if (settings.ContainsKey("IPAddress"))
                                prIpTextBox.Text = settings["IPAddress"];
                            else
                                prIpTextBox.Text = "10.10.69.38"; // значение по умолчанию

                            // Загружаем порт
                            if (settings.ContainsKey("Port"))
                                pr205PortTb.Text = settings["Port"];
                            else
                                pr205PortTb.Text = "502"; // значение по умолчанию

                            // Загружаем задержку
                            if (settings.ContainsKey("BreakingTime"))
                                breakingTimeTb.Text = settings["BreakingTime"];
                            else
                                breakingTimeTb.Text = "15"; // значение по умолчанию

                            // Загружаем задержку
                            if (settings.ContainsKey("CameraOffset"))
                                cameraOffsetTb.Text = settings["CameraOffset"];
                            else
                                cameraOffsetTb.Text = "300"; // значение по умолчанию

                            // Загружаем задержку
                            if (settings.ContainsKey("BreakerOffset"))
                                breakerOffsetTb.Text = settings["BreakerOffset"];
                            else
                                breakerOffsetTb.Text = "2430"; // значение по умолчанию

                            MessageBox.Show("Настройки ПР205 успешно загружены.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось прочитать настройки из файла.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке настроек ПР205: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidatePrSettings()
        {
            // Проверка IP адреса
            if (!System.Net.IPAddress.TryParse(prIpTextBox.Text, out _))
            {
                MessageBox.Show("Неверный формат IP адреса", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка порта
            if (!int.TryParse(pr205PortTb.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Порт должен быть числом от 1 до 65535", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка задержки
            if (!int.TryParse(breakingTimeTb.Text, out int delay) || delay < 0)
            {
                MessageBox.Show("Задержка должна быть положительным числом", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка порта
            if (!int.TryParse(cameraOffsetTb.Text, out int cameraOffset) || cameraOffset < 0)
            {
                MessageBox.Show("Расстояние от датчика до камеры должно быть положительным числом", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка задержки
            if (!int.TryParse(breakerOffsetTb.Text, out int breakerOffset) || breakerOffset < 0)
            {
                MessageBox.Show("Расстояние от датчика до отбраковщика должно быть положительным числом", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private async void ApplyPr_Click(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();

            try
            {
                // BreakingTime
                if (!int.TryParse(breakingTimeTb.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out BreakingTime) || BreakingTime <= 0)
                    BreakingTime = 55;
                await Task.Run(() => SendBreakingTime(BreakingTime), cts.Token);

                // CameraOffset
                if (!int.TryParse(cameraOffsetTb.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out CameraOffset) || CameraOffset <= 0)
                    CameraOffset = 300;
                await Task.Run(() => SendCameraOffset(CameraOffset), cts.Token);

                // BreakerOffset
                if (!int.TryParse(breakerOffsetTb.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out BreakerOffset) || BreakerOffset <= 0)
                    BreakerOffset = 2430;
                await Task.Run(() => SendBreakerOffset(BreakerOffset), cts.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке параметров: {ex.Message}");
            }
        }

        #endregion

        #region Настройка параметров обнаржуения дефектов

        private void saveDefectSettings_Click(object sender, EventArgs e)
        {
            if (!ValidateDefectSettings())
                return;

            try
            {
                var settings = new
                {
                    OvalityThreshold = ovalityCoef.Text,
                    InclusionThreshold = circleCoefTx.Text,
                    MinAreaInclusion = minSquareInclusion.Text,
                    MaxAreaInclusion = maxSquareInclusion.Text,
                    MinAreaInpaintDefect = minSquareInpaint.Text,
                    MinInpaintWhiteThreshold = whiteThresoldTx.Text,
                    MinAreaObloy = obloyPixCount.Text
                };

                string json = System.Text.Json.JsonSerializer.Serialize(settings,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    saveFileDialog.Title = "Сохранить настройки дефектов";
                    saveFileDialog.FileName = "defect_settings.json";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveFileDialog.FileName, json);
                        MessageBox.Show("Настройки дефектов успешно сохранены.", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // Сохраняем в Settings
                Properties.Settings.Default.OvalityThreshold = ovalityCoef.Text;
                Properties.Settings.Default.InclusionThreshold = circleCoefTx.Text;
                Properties.Settings.Default.MinAreaInclusion = minSquareInclusion.Text;
                Properties.Settings.Default.MaxAreaInclusion = maxSquareInclusion.Text;
                Properties.Settings.Default.MinAreaInpaintDefect = minSquareInpaint.Text;
                Properties.Settings.Default.MinInpaintWhiteThreshold = whiteThresoldTx.Text;
                Properties.Settings.Default.MinAreaObloy = obloyPixCount.Text;

                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении настроек дефектов: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadDefectSettings_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    openFileDialog.Title = "Загрузить настройки дефектов";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string json = File.ReadAllText(openFileDialog.FileName);
                        var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        if (settings != null)
                        {
                            ovalityCoef.Text = settings.ContainsKey("OvalityThreshold") ? settings["OvalityThreshold"] : "0.7";
                            circleCoefTx.Text = settings.ContainsKey("InclusionThreshold") ? settings["InclusionThreshold"] : "0.5";
                            minSquareInclusion.Text = settings.ContainsKey("MinAreaInclusion") ? settings["MinAreaInclusion"] : "50";
                            maxSquareInclusion.Text = settings.ContainsKey("MaxAreaInclusion") ? settings["MaxAreaInclusion"] : "500";
                            minSquareInpaint.Text = settings.ContainsKey("MinAreaInpaintDefect") ? settings["MinAreaInpaintDefect"] : "500";
                            whiteThresoldTx.Text = settings.ContainsKey("MinInpaintWhiteThreshold") ? settings["MinInpaintWhiteThreshold"] : "150";
                            obloyPixCount.Text = settings.ContainsKey("MinAreaObloy") ? settings["MinAreaObloy"] : "1000";

                            MessageBox.Show("Настройки дефектов успешно загружены.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось прочитать настройки из файла.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке настроек дефектов: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateDefectSettings()
        {
            // Проверка на корректность чисел
            if (!double.TryParse(ovalityCoef.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double ovality) || ovality <= 0 || ovality > 1)
            {
                MessageBox.Show("Параметр 'OvalityThreshold' должен быть числом от 0 до 1.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!double.TryParse(circleCoefTx.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double inclusion) || inclusion <= 0 || inclusion > 1)
            {
                MessageBox.Show("Параметр 'InclusionThreshold' должен быть числом от 0 до 1.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!double.TryParse(minSquareInclusion.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double minInclusion) || minInclusion < 0)
            {
                MessageBox.Show("Параметр 'MinAreaInclusion' должен быть положительным числом.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!double.TryParse(maxSquareInclusion.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double maxInclusion) || maxInclusion <= minInclusion)
            {
                MessageBox.Show("Параметр 'MaxAreaInclusion' должен быть больше 'MinAreaInclusion'.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!double.TryParse(minSquareInpaint.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double minInpaint) || minInpaint <= 0)
            {
                MessageBox.Show("Параметр 'MinAreaInpaintDefect' должен быть положительным числом.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!double.TryParse(whiteThresoldTx.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double whiteThreshold) || whiteThreshold <= 0)
            {
                MessageBox.Show("Параметр 'MinInpaintWhiteThreshold' должен быть положительным числом.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!double.TryParse(obloyPixCount.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double obloy) || obloy <= 0)
            {
                MessageBox.Show("Параметр 'MinAreaObloy' должен быть положительным числом.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }


        #endregion

        #endregion

        #region Прочие обработчики

        private void CloseProgramButton_Click(object sender, EventArgs e)
        {
            ShutdownApplication();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseProgramButton_Click(null, null);
        }

        private void ovalityCB_CheckedChanged(object sender, EventArgs e)
        {
            ovalityDefectPanel.BackColor = ovalityCB.Checked ? Color.FromArgb(240, 245, 255) : Color.FromArgb(255, 200, 200);
            ovalityDef.BackColor = ovalityCB.Checked ? Color.FromArgb(240, 245, 255) : Color.FromArgb(255, 200, 200);
            timeOvality.BackColor = ovalityCB.Checked ? Color.FromArgb(240, 245, 255) : Color.FromArgb(255, 200, 200);
        }

        private void inclusionCB_CheckedChanged(object sender, EventArgs e)
        {
            inclusionDefectPanel.BackColor = inclusionCB.Checked ? Color.FromArgb(240, 245, 255) : Color.FromArgb(255, 200, 200);
            inclusionDef.BackColor = inclusionCB.Checked ? Color.FromArgb(240, 245, 255) : Color.FromArgb(255, 200, 200);
            inclusionTime.BackColor = inclusionCB.Checked ? Color.FromArgb(240, 245, 255) : Color.FromArgb(255, 200, 200);
        }

        private void inpaintCB_CheckedChanged(object sender, EventArgs e)
        {
            inpaintDefectPanel.BackColor = inpaintCB.Checked ? Color.FromArgb(240, 245, 255) : Color.FromArgb(255, 200, 200);
            InpaintDef.BackColor = inpaintCB.Checked ? Color.FromArgb(240, 245, 255) : Color.FromArgb(255, 200, 200);
            inpaintTime.BackColor = inpaintCB.Checked ? Color.FromArgb(240, 245, 255) : Color.FromArgb(255, 200, 200);
        }

        private void obloyCB_CheckedChanged(object sender, EventArgs e)
        {
            obloyDefectPanel.BackColor = obloyCB.Checked ? Color.FromArgb(240, 245, 255) : Color.FromArgb(255, 200, 200);
            obloyDef.BackColor = obloyCB.Checked ? Color.FromArgb(240, 245, 255) : Color.FromArgb(255, 200, 200);
            obloyTime.BackColor = obloyCB.Checked ? Color.FromArgb(240, 245, 255) : Color.FromArgb(255, 200, 200);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetSelectedCapValue();

            if (capsAreWhite)
            {
                // Белые → отключить и сбросить
                inpaintCB.Checked = false;
                inpaintCB_CheckedChanged(null, null);
                inpaintCB.Enabled = false;
            }
            else
            {
                // Остальные цвета → снова доступно
                inpaintCB.Enabled = true;
            }
        }

        #region Обработчик выбора типа дефекта
        private void defectTypeComboBox_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // Сначала скрываем все панели
            HideAllDefectParamPanels();

            // Показываем только выбранную панель
            switch (defectTypeComboBox.SelectedItem?.ToString())
            {
                case "Овальность":
                    ShowOvalityPanel();
                    break;
                case "Облой":
                    ShowObloyPanel();
                    break;
                case "Непрокрас":
                    ShowInpaintPanel();
                    break;
                case "Вкрапления":
                    ShowInclusionPanel();
                    break;
                case "Недолив":
                    //ShowNedolivPanel();
                    break;
                default:
                    // Если ничего не выбрано или неизвестный вариант, скрываем все
                    HideAllDefectParamPanels();
                    break;
            }
        }

        private void HideAllDefectParamPanels()
        {
            ovalityParamsPanel.Visible = false;
            obloyDefectParamPanel.Visible = false;
            inpaintDefectParamPanel.Visible = false;
            inclusionParamDefectPanel.Visible = false;
            //nedolivDefectParamPanel.Visible = false;
        }

        private void ShowOvalityPanel()
        {
            ovalityParamsPanel.Visible = true;
            ovalityParamsPanel.BringToFront(); // Перемещаем на передний план
                                               //ovalityParamsPanel.Dock = DockStyle.Fill; // Или другой нужный вам layout
        }

        private void ShowObloyPanel()
        {
            obloyDefectParamPanel.Visible = true;
            obloyDefectParamPanel.BringToFront();
            //obloyDefectParamPanel.Dock = DockStyle.Fill;
        }

        private void ShowInpaintPanel()
        {
            inpaintDefectParamPanel.Visible = true;
            inpaintDefectParamPanel.BringToFront();
            //inpaintDefectParamPanel.Dock = DockStyle.Fill;
        }

        private void ShowInclusionPanel()
        {
            inclusionParamDefectPanel.Visible = true;
            inclusionParamDefectPanel.BringToFront();
            //inclusionParamDefectPanel.Dock = DockStyle.Fill;
        }

        /*private void ShowNedolivPanel()
        {
            nedol.Visible = true;
            nedolivDefectParamPanel.BringToFront();
            nedolivDefectParamPanel.Dock = DockStyle.Fill;
        }*/
        #endregion

        #endregion

        #region Обработчики кнопок для настройки путей сохранения изображения с дефектными крышками

        private void browseOriginalButton_Click(object sender, EventArgs e)
        {
            // Если нужна папка для оригинальных изображений
            SelectFolder("Выберите папку для сохранения оригинальных изображений", ref settingsFilePath);
        }

        private void browseOvalityButton_Click(object sender, EventArgs e)
        {
            SelectFolder("Выберите папку для сохранения изображений с овальностью", ref ovalityDefectPath);

            // Обновляем TextBox если он есть
            if (ovalityPathTextBox != null)
                ovalityPathTextBox.Text = ovalityDefectPath;
        }

        private void browseInclusionButton_Click(object sender, EventArgs e)
        {
            SelectFolder("Выберите папку для сохранения изображений с вкраплениями", ref inclusionDefectPath);

            // Обновляем TextBox если он есть
            if (inclusionPathTextBox != null)
                inclusionPathTextBox.Text = inclusionDefectPath;
        }

        private void browseInpaintButton_Click(object sender, EventArgs e)
        {
            SelectFolder("Выберите папку для сохранения изображений с непрокрасом", ref paintDefectPath);

            // Обновляем TextBox если он есть
            if (inpaintPathTextBox != null)
                inpaintPathTextBox.Text = paintDefectPath;
        }

        private void browseObloyButton_Click(object sender, EventArgs e)
        {
            SelectFolder("Выберите папку для сохранения изображений с облоем", ref obloyDefectPath);

            // Обновляем TextBox если он есть
            if (obloyPathTextBox != null)
                obloyPathTextBox.Text = obloyDefectPath;
        }

        private void browseUnderfillButton_Click(object sender, EventArgs e)
        {
            /*SelectFolder("Выберите папку для сохранения изображений с недоливом", ref underfillDefectPath);

            // Обновляем TextBox если он есть
            if (underfillPathTextBox != null)
                underfillPathTextBox.Text = underfillDefectPath;*/
        }

        // Общий метод для выбора папки
        private void SelectFolder(string description, ref string pathVariable)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = description;
                folderDialog.ShowNewFolderButton = true; // Разрешить создание новых папок

                // Устанавливаем начальную папку, если путь уже существует
                if (!string.IsNullOrEmpty(pathVariable) && Directory.Exists(pathVariable))
                {
                    folderDialog.SelectedPath = pathVariable;
                }
                else
                {
                    // Или папка "Мои документы" по умолчанию
                    folderDialog.RootFolder = Environment.SpecialFolder.MyDocuments;
                }

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    pathVariable = folderDialog.SelectedPath;
                    MessageBox.Show($"Выбран путь: {pathVariable}", "Путь сохранен",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void applyFoldersButton_Click(object sender, EventArgs e)
        {
            // Проверяем, что все пути заданы (опционально)
            if (string.IsNullOrEmpty(ovalityDefectPath) ||
                string.IsNullOrEmpty(inclusionDefectPath) ||
                string.IsNullOrEmpty(paintDefectPath) ||
                string.IsNullOrEmpty(obloyDefectPath))
            //string.IsNullOrEmpty(underfillDefectPath))
            {
                MessageBox.Show("Не все пути для сохранения заданы!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Создаем папки если они не существуют
            try
            {
                Directory.CreateDirectory(ovalityDefectPath);
                Directory.CreateDirectory(inclusionDefectPath);
                Directory.CreateDirectory(paintDefectPath);
                Directory.CreateDirectory(obloyDefectPath);
                //Directory.CreateDirectory(underfillDefectPath);

                MessageBox.Show("Все пути успешно применены и папки созданы!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании папок: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            SavePathsToSettings();
        }

        // Сохранение путей в настройки
        private void SavePathsToSettings()
        {
            Properties.Settings.Default.OvalityDefectPath = ovalityDefectPath;
            Properties.Settings.Default.InclusionDefectPath = inclusionDefectPath;
            Properties.Settings.Default.PaintDefectPath = paintDefectPath;
            Properties.Settings.Default.ObloyDefectPath = obloyDefectPath;
            //Properties.Settings.Default.UnderfillDefectPath = underfillDefectPath;
            Properties.Settings.Default.Save();
        }

        // Загрузка путей из настроек
        private void LoadPathsFromSettings()
        {
            // ===== Пути к дефектам =====
            ovalityDefectPath = Properties.Settings.Default.OvalityDefectPath;
            inclusionDefectPath = Properties.Settings.Default.InclusionDefectPath;
            paintDefectPath = Properties.Settings.Default.PaintDefectPath;
            obloyDefectPath = Properties.Settings.Default.ObloyDefectPath;
            // underfillDefectPath = Properties.Settings.Default.UnderfillDefectPath;

            // Обновляем TextBox'ы путей, если они есть
            if (ovalityPathTextBox != null) ovalityPathTextBox.Text = ovalityDefectPath;
            if (inclusionPathTextBox != null) inclusionPathTextBox.Text = inclusionDefectPath;
            if (inpaintPathTextBox != null) inpaintPathTextBox.Text = paintDefectPath;
            if (obloyPathTextBox != null) obloyPathTextBox.Text = obloyDefectPath;
            // if (underfillPathTextBox != null) underfillPathTextBox.Text = underfillDefectPath;

            // ===== Параметры ПР =====
            int.TryParse(Properties.Settings.Default.BreakingTime, out BreakingTime);
            int.TryParse(Properties.Settings.Default.BreakerOffset, out BreakerOffset);
            int.TryParse(Properties.Settings.Default.CameraOffset, out CameraOffset);

            if (cameraOffsetTb != null) cameraOffsetTb.Text = CameraOffset.ToString();
            if (breakerOffsetTb != null) breakerOffsetTb.Text = BreakerOffset.ToString();
            if (breakingTimeTb != null) breakingTimeTb.Text = BreakingTime.ToString();

            // ===== Параметры дефектов (автоподгрузка при запуске) =====
            if (!string.IsNullOrEmpty(Properties.Settings.Default.OvalityThreshold))
                ovalityCoef.Text = Properties.Settings.Default.OvalityThreshold;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.InclusionThreshold))
                circleCoefTx.Text = Properties.Settings.Default.InclusionThreshold;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.MinAreaInclusion))
                minSquareInclusion.Text = Properties.Settings.Default.MinAreaInclusion;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.MaxAreaInclusion))
                maxSquareInclusion.Text = Properties.Settings.Default.MaxAreaInclusion;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.MinAreaInpaintDefect))
                minSquareInpaint.Text = Properties.Settings.Default.MinAreaInpaintDefect;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.MinInpaintWhiteThreshold))
                whiteThresoldTx.Text = Properties.Settings.Default.MinInpaintWhiteThreshold;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.MinAreaObloy))
                obloyPixCount.Text = Properties.Settings.Default.MinAreaObloy;

            // ===== Параметры камеры (Width, Height, Exposure, Gain) =====
            if (!string.IsNullOrEmpty(Properties.Settings.Default.WidthFrame))
                widthTb.Text = Properties.Settings.Default.WidthFrame;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.HeightFrame))
                heightTb.Text = Properties.Settings.Default.HeightFrame;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.ExposureFrame))
                exposureTb.Text = Properties.Settings.Default.ExposureFrame;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.GainFrame))
                gainTb.Text = Properties.Settings.Default.GainFrame;
        }



        #endregion

        #endregion

        #region Методы обработки изображений

        private bool CheckOvality(Mat gray, Mat image, CancellationToken token, Point[] largestContourOvality)
        {
            token.ThrowIfCancellationRequested();

            //largestContourOvality = GetCapContour(gray, image);
            token.ThrowIfCancellationRequested();

            Cv2.DrawContours(image, new[] { largestContourOvality }, -1, new Scalar(255, 0, 0), 2);

            RotatedRect ellipse = Cv2.FitEllipse(largestContourOvality);
            majorAxis = Math.Max(ellipse.Size.Width, ellipse.Size.Height);
            minorAxis = Math.Min(ellipse.Size.Width, ellipse.Size.Height);
            axisRatio = minorAxis / majorAxis;

            token.ThrowIfCancellationRequested();
            bool isOval = axisRatio < ovalityThreshold;

            using (Mat resultImage = image.Clone())
            {
                Scalar color = isOval ? new Scalar(0, 0, 255) : new Scalar(0, 255, 0);
                Cv2.Ellipse(image, ellipse, color, 2);
                Cv2.PutText(image, $"Ratio: {axisRatio:F5}", new Point(10, 30),
                           HersheyFonts.HersheySimplex, 1, color, 2);
            }
            return isOval;
        }

        private bool CheckForInclusions(Mat gray, Mat image, CancellationToken token, Point[] bestContour)
        {
            token.ThrowIfCancellationRequested();

            //Point[] bestContour = GetCapContour(gray, image);
            token.ThrowIfCancellationRequested();

            RotatedRect ellipse = Cv2.FitEllipse(bestContour);
            Point2f ellipseCenter = ellipse.Center;
            float ellipseRadius = (float)(0.7 * (ellipse.Size.Width + ellipse.Size.Height) / 4.0);

            Cv2.Circle(image, (Point)ellipseCenter, (int)ellipseRadius, new Scalar(0, 255, 0), 2);

            using (Mat mask = Mat.Zeros(gray.Size(), MatType.CV_8UC1))
            using (Mat croppedRegion = new Mat())
            {
                Cv2.Circle(mask, (Point)ellipseCenter, (int)ellipseRadius, new Scalar(255), -1);
                gray.CopyTo(croppedRegion, mask);

                using (Mat binary = new Mat())
                using (Mat maskedBinary = new Mat())
                using (Mat filteredBinary = new Mat())
                {
                    Cv2.AdaptiveThreshold(croppedRegion, binary, 255,
                                        AdaptiveThresholdTypes.MeanC,
                                        ThresholdTypes.BinaryInv, 11, 2);

                    var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
                    Cv2.MorphologyEx(binary, filteredBinary, MorphTypes.Open, kernel, iterations: 1);

                    Point[][] inclusionContours;
                    HierarchyIndex[] inclusionHierarchy;
                    Cv2.FindContours(filteredBinary, out inclusionContours, out inclusionHierarchy,
                                   RetrievalModes.List, ContourApproximationModes.ApproxSimple);

                    token.ThrowIfCancellationRequested();

                    bool inclusionsFound = false;
                    foreach (var contour in inclusionContours)
                    {
                        token.ThrowIfCancellationRequested();

                        double area = Cv2.ContourArea(contour);
                        if (area > minAreaInclusion && area < maxAreaInclusion && IsCircularContour(contour))
                        {
                            Rect bbox = Cv2.BoundingRect(contour);
                            Cv2.Rectangle(image, bbox.TopLeft, bbox.BottomRight,
                                         new Scalar(0, 0, 255), 2);
                            inclusionsFound = true;
                        }
                    }

                    return inclusionsFound;
                }
            }
        }

        private bool CheckForPaintDefects(Mat gray, Mat image, CancellationToken token, Point[] capContour)
        {
            token.ThrowIfCancellationRequested();

            using (Mat hsv = new Mat())
            {
                Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
                token.ThrowIfCancellationRequested();

                //Point[] capContour = GetCapContour(gray, image);
                token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length == 0)
                {
                    return false;
                }

                Cv2.Polylines(image, new[] { capContour }, true, new Scalar(255, 0, 0), 2);

                using (Mat capMask = Mat.Zeros(image.Size(), MatType.CV_8UC1))
                {
                    Cv2.FillPoly(capMask, new[] { capContour }, new Scalar(255));
                    token.ThrowIfCancellationRequested();

                    using (Mat maskedHSV = new Mat())
                    {
                        Cv2.BitwiseAnd(hsv, hsv, maskedHSV, capMask);
                        token.ThrowIfCancellationRequested();

                        Mat[] hsvChannels;
                        Cv2.Split(maskedHSV, out hsvChannels);
                        using (Mat sChannel = hsvChannels[1])
                        using (Mat vChannel = hsvChannels[2])
                        using (Mat whiteMask = new Mat())
                        {
                            Cv2.InRange(hsv, new Scalar(0, 255 * 0.05, 255 * 0.05),
                                      new Scalar(180, 255 * 0.95, 255 * 0.95), whiteMask);
                            token.ThrowIfCancellationRequested();

                            using (Mat defectsMask = new Mat())
                            {
                                Cv2.BitwiseNot(whiteMask, defectsMask);
                                using (Mat maskedDefects = new Mat())
                                {
                                    Cv2.BitwiseAnd(defectsMask, capMask, maskedDefects);
                                    token.ThrowIfCancellationRequested();

                                    Point[][] contours;
                                    HierarchyIndex[] hierarchy;
                                    Cv2.FindContours(maskedDefects, out contours, out hierarchy,
                                                    RetrievalModes.External,
                                                    ContourApproximationModes.ApproxSimple);

                                    var significantContours = contours.Where(c =>
                                        Cv2.ContourArea(c) > minAreaInpaintDefect).ToList();
                                    token.ThrowIfCancellationRequested();

                                    var whiteDefects = new List<Point[]>();
                                    foreach (var contour in significantContours)
                                    {
                                        token.ThrowIfCancellationRequested();

                                        using (Mat contourMask = Mat.Zeros(image.Size(), MatType.CV_8UC1))
                                        {
                                            Cv2.FillPoly(contourMask, new[] { contour }, new Scalar(255));
                                            using (Mat maskedImage = new Mat())
                                            {
                                                Cv2.BitwiseAnd(image, image, maskedImage, contourMask);
                                                Scalar meanColor = Cv2.Mean(maskedImage, contourMask);

                                                if (Math.Abs(meanColor.Val2 - 255) < minInpaintWhiteTgreshold)
                                                {
                                                    whiteDefects.Add(contour);
                                                }
                                            }
                                        }
                                    }

                                    if (whiteDefects.Count > 0)
                                    {
                                        Cv2.DrawContours(image, whiteDefects, -1, new Scalar(0, 0, 255), 2);
                                        foreach (var contour in whiteDefects)
                                        {
                                            Rect boundingBox = Cv2.BoundingRect(contour);
                                            Cv2.Rectangle(image, boundingBox, new Scalar(0, 255, 255), 2);
                                        }
                                    }

                                    return whiteDefects.Count > 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool CheckForObloyDefects(Mat gray, Mat image, CancellationToken token, Point[] capContour)
        {
            token.ThrowIfCancellationRequested();

            if (capContour == null || capContour.Length == 0)
                return false;

            // Проверяем, что каналы уже вычислены
            if (blurChannel_1 == null || blurChannel_2 == null)
                throw new InvalidOperationException("Каналы не были инициализированы. Сначала вызовите GetCapContour().");

            // Вычисляем центр и радиус крышки по контуру
            double sumX = 0, sumY = 0;
            foreach (var pt in capContour)
            {
                sumX += pt.X;
                sumY += pt.Y;
            }
            var capCenter = new Point((int)(sumX / capContour.Length), (int)(sumY / capContour.Length));

            double radius = 0;
            foreach (var pt in capContour)
            {
                double dx = pt.X - capCenter.X;
                double dy = pt.Y - capCenter.Y;
                radius += Math.Sqrt(dx * dx + dy * dy);
            }
            radius /= capContour.Length;

            if (CapRadiusMask == null || CapRadiusMask.Size() != image.Size())
            {
                CapRadiusMask?.Dispose();
                CapRadiusMask = new Mat(image.Rows, image.Cols, MatType.CV_8UC1);
            }

            GetIdealCapMask(CapRadiusMask, capCenter,
                            (float)(radius + 3.0f),
                            (float)(radius + 3.0f + CAP_FLASH_OFFSET));

            // Используем готовые глобальные каналы
            Cv2.BitwiseAnd(CapRadiusMask, blurChannel_2, blurChannel_2);
            Cv2.MorphologyEx(blurChannel_2, blurChannel_1, MorphTypes.Erode, elementMask);

            Point[][] obloyContours;
            HierarchyIndex[] hierarchyObloy;
            Cv2.FindContours(blurChannel_1, out obloyContours, out hierarchyObloy,
                             RetrievalModes.External, ContourApproximationModes.ApproxNone);

            if (obloyContours.Length > 0)
            {
                Cv2.DrawContours(image, obloyContours, -1, new Scalar(0, 0, 255), 2);
            }

            int pixCount = Cv2.CountNonZero(blurChannel_1);
            return pixCount > minAreaObloy;
        }


        #endregion

        #region Вспомогательные методы обработки

        private Point[] GetCapContour(Mat gray, Mat image)
        {
            if (gray.Empty() || image.Empty())
                return null;

            Mat processed = image.Clone();
            NonlinearBackgroundDecolorization(processed, capsColor);

            Mat[] channels;
            Cv2.Split(processed, out channels);

            Cv2.GaussianBlur(channels[0], channels[1], new Size(window, window), 4);
            Cv2.Threshold(channels[1], channels[0], 128, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);
            Cv2.MorphologyEx(channels[0], channels[1], MorphTypes.Dilate, element1);
            Cv2.MorphologyEx(channels[1], channels[2], MorphTypes.Erode, element2);

            blurChannel_0 = channels[0];
            blurChannel_1 = channels[1];
            blurChannel_2 = channels[2];

            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(
                channels[2],
                out contours,
                out hierarchy,
                RetrievalModes.External,
                ContourApproximationModes.ApproxNone
            );

            int maxInd = 0;
            int maxLength = 0;
            for (int i = 0; i < contours.Length; i++)
            {
                if (contours[i].Length > maxLength)
                {
                    maxLength = contours[i].Length;
                    maxInd = i;
                }
            }

            return contours.Length > 0 ? contours[maxInd] : null;
        }

        private void GetIdealCapMask(Mat mask, Point center, float innerRadius, float outerRadius)
        {
            mask.SetTo(0);

            using var outer = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.Black);
            using var inner = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.Black);

            Cv2.Circle(outer, center, (int)outerRadius, Scalar.White, -1);
            Cv2.Circle(inner, center, (int)innerRadius, Scalar.White, -1);
            Cv2.Subtract(outer, inner, mask);
        }

        private bool IsCircularContour(Point[] contour)
        {
            double perimeter = Cv2.ArcLength(contour, true);
            double area = Cv2.ContourArea(contour);
            double circularity = (4 * Math.PI * area) / (perimeter * perimeter);
            return circularity > inclusionThreshold;
        }

        #endregion

        #region Методы управления приложением

        private async Task StopProcessingAsync()
        {
            cts?.Cancel();
            recognizeButton.Text = "Остановка...";
            recognizeButton.Enabled = false;

            if (modbusClient != null && modbusClient.Connected)
            {
                modbusClient.WriteSingleRegisterForBreaker(startRecognizeProcessing, 0);
            }
            currentFrameNumber = 0;

            try
            {
                await processingTask;
            }
            catch (OperationCanceledException)
            {
                // Обработка отмены - нормальная ситуация
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при остановке: {ex.Message}");
            }
            finally
            {
                isProcessing = false;
                recognizeButton.Text = "Начать анализ";
                recognizeButton.BackColor = Color.FromArgb(4, 85, 191);
                recognizeButton.Enabled = true;

                SetUiDuringRecognition(false);


                cts?.Dispose();
                cts = null;
            }
        }

        private void StartProcessing()
        {
            ApplyRecognitionParameters();

            if (modbusClient != null && modbusClient.Connected && !isImageLoaded)
            {
                modbusClient.WriteSingleRegisterForBreaker(startRecognizeProcessing, 1);
            }

            cts = new CancellationTokenSource();
            try
            {
                capsColor = GetSelectedCapValue();

                processingTask = Task.Run(() => StartContinuousProcessing(cts.Token));
                isProcessing = true;
                recognizeButton.Text = "Остановить анализ";
                recognizeButton.BackColor = Color.FromArgb(229, 115, 115);

                SetUiDuringRecognition(true);
            }
            catch
            {
                cts?.Dispose();
                throw;
            }
        }

        private void SetUiDuringRecognition(bool isLocked)
        {
            applySettingsButton.Enabled = !isLocked;
            loadSettingsButton.Enabled = !isLocked;
            saveSettingsButton.Enabled = !isLocked;
            loadDefectSettings.Enabled = !isLocked;
            saveDefectSettings.Enabled = !isLocked;
            applyFoldersButton.Enabled = !isLocked;
            browseOvalityButton.Enabled = !isLocked;
            browseInclusionButton.Enabled = !isLocked;
            browseInpaintButton.Enabled = !isLocked;
            browseObloyButton.Enabled = !isLocked;
            browseUnderfillButton.Enabled = !isLocked;
            browseOriginalButton.Enabled = !isLocked;
            applyPrBreakerParamButton.Enabled = !isLocked;
            loadPrSettings.Enabled = !isLocked;
            savePrSettings.Enabled = !isLocked;
            startStreamButton.Enabled = !isLocked;
        }

        private void ApplyCameraSettings()
        {
            try
            {
                if (!img1.Empty())
                {
                    StopStream();
                    uint width = uint.Parse(widthTb.Text);
                    uint height = uint.Parse(heightTb.Text);
                    uint exposure = uint.Parse(exposureTb.Text);
                    uint gain = uint.Parse(gainTb.Text);
                    cam.Width = width;
                    cam.Height = height;
                    cam.ExposureTime = exposure;
                    cam.Gain = gain;
                    cam.SetHeight();
                    cam.SetWidth();
                    cam.SetGain();
                    cam.SetExposureTime();

                    StartStream();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void ShutdownApplication()
        {
            try
            {
                // --- Отправляем сигнал завершения в ПР205 ---
                if (modbusClient != null && modbusClient.Connected)
                {
                    try
                    {
                        modbusClient.WriteSingleRegisterForBreaker(startRecognizeProcessing, RecognizeProcessingAndBreakerAllowFinish);
                        modbusClient.WriteSingleRegisterForBreaker(breakerAllowRegister, RecognizeProcessingAndBreakerAllowFinish);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при отправке сигнала завершения в ПР205: {ex.Message}");
                    }
                }

                // --- Закрываем камеру ---
                if (cam != null && cameraConnected)
                {
                    try
                    {
                        if (cam.Streamed)
                            cam.EndStream();
                        cam.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при закрытии камеры: {ex.Message}");
                    }
                    finally
                    {
                        cam = null;
                        cameraConnected = false;
                    }
                }

                // --- Отключаем modbus ---
                if (modbusClient != null && modbusClient.Connected)
                {
                    try
                    {
                        modbusClient.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при отключении от ПР205: {ex.Message}");
                    }
                }

#if OLD_FRAME_PROCESSING
#else
                // --- Освобождение кадрового буфера ---
                _imageQueue.Dispose();
#endif
            }
            finally
            {
                Application.Exit();
            }
        }


        #endregion

        #region Методы работы с ROI

        private void OriginPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (isDrawing && e.Button == MouseButtons.Left)
            {
                startPoint = e.Location;
                selectedROI = new Rectangle(startPoint, new System.Drawing.Size(0, 0));
            }
        }

        private void OriginPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && e.Button == MouseButtons.Left)
            {
                int x = Math.Min(startPoint.X, e.X);
                int y = Math.Min(startPoint.Y, e.Y);
                int width = Math.Abs(startPoint.X - e.X);
                int height = Math.Abs(startPoint.Y - e.Y);

                selectedROI = new Rectangle(x, y, width, height);
                originPb.Invalidate();
            }
        }

        private void OriginPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDrawing && e.Button == MouseButtons.Left)
            {
                isDrawing = false;
                originPb.MouseDown -= OriginPictureBox_MouseDown;
                originPb.MouseMove -= OriginPictureBox_MouseMove;
                originPb.MouseUp -= OriginPictureBox_MouseUp;

                if (selectedROI.Width > 0 && selectedROI.Height > 0)
                {
                    isConfirmVisible = true;

                    int btnSize = 20;
                    int btnOffset = 5;
                    int btnSpacing = 5;

                    int buttonY = selectedROI.Bottom + btnOffset;
                    int buttonX = selectedROI.Right - (btnSize * 2 + btnSpacing);

                    checkRect = new Rectangle(buttonX, buttonY, btnSize, btnSize);
                    crossRect = new Rectangle(buttonX + btnSize + btnSpacing, buttonY, btnSize, btnSize);

                    originPb.Invalidate();
                }
            }
        }

        private void OriginPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (isConfirmVisible)
            {
                if (checkRect.Contains(e.Location))
                {
                    ApplyROI();
                }
                else if (crossRect.Contains(e.Location))
                {
                    ResetROI();
                }
            }
        }

        private void ApplyROI()
        {
            if (selectedROI.Width > 0 && selectedROI.Height > 0)
            {
                int imgWidth = img1.Width;
                int imgHeight = img1.Height;
                int pbWidth = originPb.Width;
                int pbHeight = originPb.Height;

                float scaleX, scaleY;
                int offsetX = 0, offsetY = 0;

                if (originPb.SizeMode == PictureBoxSizeMode.Zoom)
                {
                    float ratioX = (float)pbWidth / imgWidth;
                    float ratioY = (float)pbHeight / imgHeight;
                    float ratio = Math.Min(ratioX, ratioY);

                    scaleX = ratio;
                    scaleY = ratio;

                    offsetX = (int)((pbWidth - (imgWidth * scaleX)) / 2);
                    offsetY = (int)((pbHeight - (imgHeight * scaleY)) / 2);
                }
                else
                {
                    scaleX = (float)imgWidth / pbWidth;
                    scaleY = (float)imgHeight / pbHeight;
                }

                int roiX = (int)((selectedROI.X - offsetX) / scaleX);
                int roiY = (int)((selectedROI.Y - offsetY) / scaleY);
                int roiWidth = (int)(selectedROI.Width / scaleX);
                int roiHeight = (int)(selectedROI.Height / scaleY);

                roiX = Math.Max(0, roiX);
                roiY = Math.Max(0, roiY);
                roiWidth = Math.Min(imgWidth - roiX, roiWidth);
                roiHeight = Math.Min(imgHeight - roiY, roiHeight);

                roi = new OpenCvSharp.Rect(roiX, roiY, roiWidth, roiHeight);

                if (roi.Width > 0 && roi.Height > 0 && roi.X + roi.Width <= imgWidth && roi.Y + roi.Height <= imgHeight)
                {
                    img1 = new Mat(img1, roi);
                }

                isROISelected = true;
                isConfirmVisible = false;
                selectedROI = Rectangle.Empty;
                originPb.Image = MatToBitmap(img1);
                originPb.Invalidate();
            }
        }

        private void ResetROI()
        {
            selectedROI = Rectangle.Empty;
            isConfirmVisible = false;
            originPb.Invalidate();
        }

        private void OriginPictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (selectedROI != Rectangle.Empty)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectedROI);
                }

                if (isConfirmVisible)
                {
                    using (SolidBrush brush = new SolidBrush(Color.White))
                    {
                        e.Graphics.FillRectangle(brush, checkRect);
                        e.Graphics.FillRectangle(brush, crossRect);
                    }

                    using (Pen penYes = new Pen(Color.Green, 2))
                    using (Pen penNot = new Pen(Color.Red, 2))
                    {
                        e.Graphics.DrawLine(penYes, checkRect.Left + 3, checkRect.Top + checkRect.Height / 2,
                                            checkRect.Left + checkRect.Width / 3, checkRect.Bottom - 3);
                        e.Graphics.DrawLine(penYes, checkRect.Left + checkRect.Width / 3, checkRect.Bottom - 3,
                                            checkRect.Right - 3, checkRect.Top + 3);

                        e.Graphics.DrawLine(penNot, crossRect.Left + 3, crossRect.Top + 3, crossRect.Right - 3, crossRect.Bottom - 3);
                        e.Graphics.DrawLine(penNot, crossRect.Right - 3, crossRect.Top + 3, crossRect.Left + 3, crossRect.Bottom - 3);
                    }
                }
            }
        }

        #endregion

        #region Вспомогательные методы

        private void InitializeHueLUT()
        {
            Parallel.For(0, 256, b =>
            {
                for (int g = 0; g < 256; g++)
                {
                    for (int r = 0; r < 256; r++)
                    {
                        int index = ((b >> 1) << 14) + ((g >> 1) << 7) + (r >> 1);
                        HUE_LUT[index] = CalculateHueFromBGR((byte)(b >> 1), (byte)(g >> 1), (byte)(r >> 1));
                    }
                }
            });
        }

        private byte CalculateHueFromBGR(byte blue, byte green, byte red)
        {
            float b = blue / 128.0f;
            float g = green / 128.0f;
            float r = red / 128.0f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            float hue = 0.0f;

            if (delta == 0.0f)
            {
                hue = 0.0f;
            }
            else
            {
                if (max == r)
                {
                    hue = (g - b) / delta;
                }
                else if (max == g)
                {
                    hue = 2.0f + (b - r) / delta;
                }
                else if (max == b)
                {
                    hue = 4.0f + (r - g) / delta;
                }

                hue *= 60.0f;

                if (hue < 0.0f)
                {
                    hue += 360.0f;
                }
            }

            return (byte)(0.5f * hue);
        }

        private byte GetSelectedCapValue()
        {
            capsAreWhite = false;
            string selected = comboBox1.SelectedItem?.ToString();
            switch (selected)
            {
                case "Желтые":
                    window = 5;  //15
                    morph_size = 1;  //11 
                    morph_size_2 = 1;  //11
                    if (cam != null)
                    {
                        cam.Saturation = 128;
                        cam.SetSaturation();
                    }
                    isGreenColor = false;
                    isColored = true;
                    return YELLOW_CAPS;
                case "Синие":
                    window = 3; //15
                    morph_size = 1; //11
                    morph_size_2 = 1;  //11
                    if (cam != null)
                    {
                        cam.Saturation = 255; 
                        cam.SetSaturation();
                    }
                    isGreenColor = false;
                    isColored = true;
                    return BLUE_CAPS;
                case "Золотые":
                    window = 5; //15
                    morph_size = 1;  //11
                    morph_size_2 = 1;  //11
                    if (cam != null)
                    {
                        cam.Saturation = 128;
                        cam.SetSaturation();
                    }
                    isGreenColor = false;
                    isColored = true;
                    return GOLD_CAPS;
                case "Белые":
                    window = 5;
                    morph_size = 1;
                    morph_size_2 = 1;
                    if (cam != null)
                    {
                        cam.Saturation = 128;
                        cam.SetSaturation();
                    }
                    isGreenColor = false;
                    isColored = false;
                    capsAreWhite = true;
                    return WHITE_CAPS;
                case "Зеленые":
                    window = 3;
                    morph_size = 2;
                    morph_size_2 = 2;
                    if (cam != null)
                    {
                        cam.Saturation = 255;
                        cam.SetSaturation();
                    }
                    isColored = true;
                    isGreenColor = true;
                    return GREEN_CAPS;
                default:
                    isColored = true;
                    if (cam != null)
                    {
                        cam.Saturation = 128;
                        cam.SetSaturation();
                    }
                    return YELLOW_CAPS;
            }
        }

        private void ApplyRecognitionParameters()
        {
            if (!double.TryParse(ovalityCoef.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out ovalityThreshold))
            {
                ovalityThreshold = 0.7;
                ovalityCoef.Text = ovalityThreshold.ToString();
            }

            if (!double.TryParse(circleCoefTx.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out inclusionThreshold))
            {
                inclusionThreshold = 0.5;
                circleCoefTx.Text = inclusionThreshold.ToString(CultureInfo.InvariantCulture);
            }

            if (!double.TryParse(minSquareInclusion.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out minAreaInclusion))
            {
                minAreaInclusion = 50;
                minSquareInclusion.Text = minAreaInclusion.ToString(CultureInfo.InvariantCulture);
            }

            if (!double.TryParse(maxSquareInclusion.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out maxAreaInclusion))
            {
                maxAreaInclusion = 500.0;
                maxSquareInclusion.Text = maxAreaInclusion.ToString(CultureInfo.InvariantCulture);
            }

            if (!double.TryParse(minSquareInpaint.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out minAreaInpaintDefect))
            {
                minAreaInpaintDefect = 500;
                minSquareInpaint.Text = minAreaInpaintDefect.ToString(CultureInfo.InvariantCulture);
            }

            if (!double.TryParse(whiteThresoldTx.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out minInpaintWhiteTgreshold))
            {
                minInpaintWhiteTgreshold = 150.0;
                whiteThresoldTx.Text = minInpaintWhiteTgreshold.ToString(CultureInfo.InvariantCulture);
            }

            if (!double.TryParse(obloyPixCount.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out minAreaObloy))
            {
                minAreaObloy = 1000;
                obloyPixCount.Text = minAreaObloy.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void SaveSettings()
        {
            try
            {
                // Простая валидация
                if (string.IsNullOrWhiteSpace(widthTb.Text) ||
                    string.IsNullOrWhiteSpace(heightTb.Text) ||
                    string.IsNullOrWhiteSpace(exposureTb.Text) ||
                    string.IsNullOrWhiteSpace(gainTb.Text))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля перед сохранением.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var settings = new
                {
                    Width = widthTb.Text,
                    Height = heightTb.Text,
                    Exposure = exposureTb.Text,
                    Gain = gainTb.Text
                };

                string json = System.Text.Json.JsonSerializer.Serialize(settings,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    saveFileDialog.Title = "Сохранить настройки";
                    saveFileDialog.FileName = "settings.json";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveFileDialog.FileName, json);

                        // ✅ Сохраняем также в Settings
                        Properties.Settings.Default.WidthFrame = widthTb.Text;
                        Properties.Settings.Default.HeightFrame = heightTb.Text;
                        Properties.Settings.Default.ExposureFrame = exposureTb.Text;
                        Properties.Settings.Default.GainFrame = gainTb.Text;
                        Properties.Settings.Default.Save();

                        MessageBox.Show("Настройки успешно сохранены.", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении настроек: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void LoadSettings()
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    openFileDialog.Title = "Загрузить настройки";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string json = File.ReadAllText(openFileDialog.FileName);
                        var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        if (settings != null)
                        {
                            widthTb.Text = settings.ContainsKey("Width") ? settings["Width"] : "500";
                            heightTb.Text = settings.ContainsKey("Height") ? settings["Height"] : "532";
                            exposureTb.Text = settings.ContainsKey("Exposure") ? settings["Exposure"] : "450";
                            gainTb.Text = settings.ContainsKey("Gain") ? settings["Gain"] : "3,01";

                            MessageBox.Show("Настройки успешно загружены.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось прочитать настройки из файла.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке настроек: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        #endregion

        #region Методы получения данных от оборудования

        public void GetImage(Mat img)
        {
            #region Логирование получения нового кадра
            /*DateTime now = DateTime.Now;

            if (_lastImageReceivedTime.HasValue)
            {
                TimeSpan interval = now - _lastImageReceivedTime.Value;
                string logEntry = $"{now:HH:mm:ss.fff} | Interval: {interval.TotalMilliseconds} ms";

                try
                {
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при записи лога: {ex.Message}");
                }
            }

            _lastImageReceivedTime = now;*/
            #endregion

            if (isStreamCam)
            {
#if OLD_FRAME_PROCESSING
                lock (frameLock)
                {
                    latestFrame?.Dispose();
                    latestFrame = img.Clone();
                    newFrameAvailable = true;
                    if (isProcessing == true)
                    {
                        currentFrameNumber = currentFrameNumber + 1;
                    }
                }
#else
                if (isProcessing)
                {
                    _imageQueue.Put(img.Clone());
                    currentFrameNumber++;
                }
#endif

                #region Сохранение всех фото крыщек
                /*// ======== СОХРАНЕНИЕ ИЗОБРАЖЕНИЙ =========
                try
                {
                    string saveDir = @"C:\Users\Kvantron\source\repos\Kvantron.Krishki\TabletkiForms\дефектные крышки\все крышки";

                    if (!Directory.Exists(saveDir))
                        Directory.CreateDirectory(saveDir);

                    string savePath = Path.Combine(saveDir, $"{number_drop_cap}.bmp");

                    img.SaveImage(savePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка сохранения изображения: {ex.Message}");
                }
                // ==========================================*/
                #endregion

                // Если ROI не выбран, показываем полное изображение с камеры
                if (!LocalSettings.Instance.UseVConcat)
                {
                    img1 = img.Clone();
                    if (isRoiProduce == true && isROISelected == true)
                    {
                        img1 = new Mat(img, roi);
                    }
                    UpdatePictureBox(originPb, img1);
                }
                else
                {
                    if (!isFirstImageCam1)
                    {
                        img1 = img.Clone();
                        if (isRoiProduce == true && isROISelected == true)
                        {
                            img1 = new Mat(img, roi);
                        }
                        UpdatePictureBox(originPb, img1);
                        isFirstImageCam1 = true;
                    }
                    else
                    {
                        Cv2.VConcat(img1, img.Clone(), img1);
                        if (isRoiProduce == true && isROISelected == true)
                        {
                            img1 = new Mat(img, roi);
                        }
                        UpdatePictureBox(originPb, img1);
                    }
                }
            }
        }

        public void GetModuleState(int number, bool state)
        {
            if (number == LocalSettings.Instance.DINumber && currentDetectorState == true)
            {
                module.SetOutput(LocalSettings.Instance.DONumber, true);

                if (!cameraError1)
                {
                    new System.Threading.Thread(new System.Threading.ThreadStart(() => cam.StartStream())).Start();
                }

                currentDetectorState = false;
            }
            else if (number == LocalSettings.Instance.DINumber && currentDetectorState == false)
            {
                System.Threading.Thread th = new System.Threading.Thread(() =>
                {
                    System.Threading.Thread.Sleep(LocalSettings.Instance.EndStreamDelay);

                    module.SetOutput(LocalSettings.Instance.DONumber, false);

                    if (!cameraError1)
                    {
                        new System.Threading.Thread(new System.Threading.ThreadStart(() => cam.EndStream())).Start();
                    }

                    currentDetectorState = true;
                });
                th.Start();
            }
        }

        #endregion

        #region Методы управления потоком обработки

        private void StartStop(bool isStart, bool isInit = false)
        {
            if (!isStart)
            {
                if (!isInit && !LocalSettings.Instance.UseModule)
                {
                    if (!cameraError1)
                    {
                        if (cam != null)
                        {
                            if (cam.Streamed)
                            {
                                cam.EndStream();
                            }
                        }
                    }
                }
                return;
            }

            isFirstImageCam1 = false;

            if (!isInit && !LocalSettings.Instance.UseModule)
            {
                if (!cameraError1)
                {
                    if (cam != null)
                    {
                        if (!cam.Streamed)
                        {
                            cam.StartStream();
                        }
                    }
                }
                img1 = new Mat();
            }
        }

        private async void StartContinuousProcessing(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {

                    Mat? frameToProcess = null;

                    if (isStreamCam)
                    {
#if OLD_FRAME_PROCESSING
                        lock (frameLock)
                        {
                            if (!newFrameAvailable) continue;
                            frameToProcess = latestFrame.Clone();
                            newFrameAvailable = false;
                        }
#else
                        frameToProcess = _imageQueue.Get(token);

#endif
                    }
                    else if (isProcessingFromFolder)
                    {
                        await Task.Delay(100);
                        if (imageFiles.Count == 0) continue;

                        try
                        {
                            frameToProcess = new Mat(imageFiles[currentImageIndex]);
                            UpdatePictureBox(originPb, frameToProcess);
                            currentImageIndex = (currentImageIndex + 1) % imageFiles.Count;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                            continue;
                        }
                    }

                    if (frameToProcess == null || frameToProcess.Empty())
                        continue;

                    using (frameToProcess)
                    using (Mat gray = new Mat())
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        Cv2.CvtColor(frameToProcess, gray, ColorConversionCodes.BGR2GRAY);
                        Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

                        Point[] capContour = GetCapContour(gray, frameToProcess);

                        if (ovalityCB.Checked)
                        {
                            frameToProcess.CopyTo(_imageForOvality);
                            gray.CopyTo(_grayForOvality);
                        }

                        if (inclusionCB.Checked)
                        {
                            frameToProcess.CopyTo(_imageForInclusions);
                            gray.CopyTo(_grayForInclusions);
                        }

                        if (inpaintCB.Checked)
                        {
                            frameToProcess.CopyTo(_imageForPaintDefects);
                            gray.CopyTo(_grayForPaintDefects);
                        }

                        if (obloyCB.Checked)
                        {
                            frameToProcess.CopyTo(_imageForObloyDefects);
                            gray.CopyTo(_grayForObloyDefects);
                        }

                        using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token))
                        {
                            timeoutCts.CancelAfter(60);

                            try
                            {
                                var ovalityTask = Task.FromResult(false);
                                var inclusionsTask = Task.FromResult(false);
                                var paintTask = Task.FromResult(false);
                                var obloyTask = Task.FromResult(false);

                                if (ovalityCB.Checked)
                                {
                                    ovalityTask = RunCheckWithTimeout(_grayForOvality, _imageForOvality, timeoutCts.Token, RunCheckOvality, capContour);
                                }

                                if (inclusionCB.Checked)
                                {
                                    inclusionsTask = RunCheckWithTimeout(_grayForInclusions, _imageForInclusions, timeoutCts.Token, RunCheckForInclusions, capContour);

                                }

                                if (inpaintCB.Checked)
                                {
                                    paintTask = RunCheckWithTimeout(_grayForPaintDefects, _imageForPaintDefects, timeoutCts.Token, RunCheckForPaintDefects, capContour);
                                }

                                if (obloyCB.Checked)
                                {
                                    obloyTask = RunCheckWithTimeout(_grayForObloyDefects, _imageForObloyDefects, timeoutCts.Token, RunCheckForObloyDefects, capContour);
                                }

                                await Task.WhenAll(ovalityTask, inclusionsTask, paintTask, obloyTask);

                                bool anyDefect = (ovalityCB.Checked && ovalityTask.Result) ||
                                                 (inclusionCB.Checked && inclusionsTask.Result) ||
                                                 (inpaintCB.Checked && paintTask.Result) ||
                                                 (obloyCB.Checked && obloyTask.Result);

                                if (anyDefect)
                                {
                                    BeginInvoke((Action)(() =>
                                    {
                                        blowTriggerCount++;
                                        generalDefectsCountTb.Text = blowTriggerCount.ToString();
                                    }));
                                    //NumberDropCapTb.Text = currentFrameNumber.ToString();
                                }
                                PLCData.QualityStatus qualityStatus = anyDefect
                                    ? PLCData.QualityStatus.Bad
                                    : PLCData.QualityStatus.Good;
                                _ = Task.Run(() => SendQualityStatus(qualityStatus), token);

                                stopwatch.Stop();
                                UpdateTextBox(generalTime, stopwatch.ElapsedMilliseconds);

                                #region Логирование времени обработки
                                // 🟢 лог времени обработки кадра
                                /*try
                                {
                                    string processEntry = $"{DateTime.Now:HH:mm:ss.fff} | ProcessTime: {stopwatch.ElapsedMilliseconds} ms";
                                    File.AppendAllText(_processTimeLogPath, processEntry + Environment.NewLine);
                                }
                                catch (Exception logEx)
                                {
                                    Debug.WriteLine($"Ошибка при записи ProcessTime лога: {logEx.Message}");
                                }*/
                                // 🔚 конец добавленного блока
                                #endregion
                            }
                            catch (OperationCanceledException)
                            {
                                // ОК
                            }
                        }
                    }
                }
            }
            catch (TaskCanceledException ex)
            {

            }
            catch (OperationCanceledException ex)
            {

            }
            catch (Exception ex)
            {
                BeginInvoke((Action)(() =>
                    MessageBox.Show($"Ошибка обработки: {ex.Message}")));
            }
        }


        private async Task<bool> RunCheckWithTimeout(Mat gray, Mat image, CancellationToken token, Func<Mat, Mat, CancellationToken, Point[], bool> checkFunc, Point[] capContour)
        {
            try
            {
                return await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    return checkFunc(gray, image, token, capContour);
                }, token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        private void SaveAndCleanupAsync(Mat image, string fullPath, string directory)
        {
            Task.Run(() =>
            {
                try
                {
                    var encodingParams = new[]
                    {
                        new ImageEncodingParam(ImwriteFlags.JpegQuality, 85)
                    };

                    Cv2.ImWrite(fullPath, image, encodingParams);
                }
                catch { /* игнорируем ошибки */ }

                CleanupOldImages(directory);
            });
        }



        private void CleanupOldImages(string directory)
        {
            try
            {
                var files = new DirectoryInfo(directory)
                    .GetFiles("*.jpg")
                    .OrderBy(f => f.CreationTime)
                    .ToList();

                if (files.Count > MAX_IMAGES_PER_DEFECT)
                {
                    var toRemove = files.Take(100).ToList();

                    foreach (var f in toRemove)
                    {
                        try { f.Delete(); } catch { }
                    }
                }
            }
            catch
            {
                // игнорируем ошибки удаления
            }
        }

        #endregion

        #region Методы проверки дефектов

        private bool RunCheckOvality(Mat gray, Mat image, CancellationToken token, Point[] capContour)
        {
            token.ThrowIfCancellationRequested();
            Stopwatch stopwatch = Stopwatch.StartNew();

            bool isOval = CheckOvality(gray, image, token, capContour);

            if (isOval)
            {
                ovalityCount++;
                UpdateTextBox(ovalityDef, ovalityCount);

                string fileName = $"{currentFrameNumber}_ovality_{ovalityCount}.jpg";
                string fullPath = Path.Combine(ovalityDefectPath, fileName);

                SaveAndCleanupAsync(image, fullPath, ovalityDefectPath);
            }

            stopwatch.Stop();
            UpdateTextBox(timeOvality, stopwatch.ElapsedMilliseconds);
            UpdatePictureBox(ovalityPb, image);

            return isOval;
        }


        private bool RunCheckForInclusions(Mat gray, Mat image, CancellationToken token, Point[] capContour)
        {
            token.ThrowIfCancellationRequested();
            Stopwatch stopwatch = Stopwatch.StartNew();

            bool hasInclusions = CheckForInclusions(gray, image, token, capContour);

            if (hasInclusions)
            {
                inclusionCount++;
                UpdateTextBox(inclusionDef, inclusionCount);

                string fileName = $"{currentFrameNumber}_inclusion_{inclusionCount}.jpg";
                string fullPath = Path.Combine(inclusionDefectPath, fileName);

                SaveAndCleanupAsync(image, fullPath, inclusionDefectPath);
            }

            stopwatch.Stop();
            UpdateTextBox(inclusionTime, stopwatch.ElapsedMilliseconds);
            UpdatePictureBox(inclusionPb, image);

            return hasInclusions;
        }


        private bool RunCheckForPaintDefects(Mat gray, Mat image, CancellationToken token, Point[] capContour)
        {
            token.ThrowIfCancellationRequested();
            Stopwatch stopwatch = Stopwatch.StartNew();

            bool hasPaintDefects = CheckForPaintDefects(gray, image, token, capContour);

            if (hasPaintDefects)
            {
                paintDefectCount++;
                UpdateTextBox(InpaintDef, paintDefectCount);

                string fileName = $"{currentFrameNumber}_paint_{paintDefectCount}.jpg";
                string fullPath = Path.Combine(paintDefectPath, fileName);

                SaveAndCleanupAsync(image, fullPath, paintDefectPath);
            }

            stopwatch.Stop();
            UpdateTextBox(inpaintTime, stopwatch.ElapsedMilliseconds);
            UpdatePictureBox(inpaintPb, image);

            return hasPaintDefects;
        }


        private bool RunCheckForObloyDefects(Mat gray, Mat image, CancellationToken token, Point[] capContour)
        {
            token.ThrowIfCancellationRequested();
            Stopwatch stopwatch = Stopwatch.StartNew();

            bool hasObloyDefects = CheckForObloyDefects(gray, image, token, capContour);

            if (hasObloyDefects)
            {
                obloyDefectCount++;
                UpdateTextBox(obloyDef, obloyDefectCount);

                string fileName = $"{currentFrameNumber}_obloy_{obloyDefectCount}.jpg";
                string fullPath = Path.Combine(obloyDefectPath, fileName);

                SaveAndCleanupAsync(image, fullPath, obloyDefectPath);
            }

            stopwatch.Stop();
            UpdateTextBox(obloyTime, stopwatch.ElapsedMilliseconds);
            UpdatePictureBox(obloyPb, image);

            return hasObloyDefects;
        }


        private bool RunCheckUnderfill(Mat gray, Mat image, CancellationToken token)
        {
            if (token.IsCancellationRequested) return false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool underfillResult = CheckUnderfill(gray, image);

            if (underfillResult)
            {
                underfillCount++;
                //UpdateTextBox(underfillDef, underfillCount);
            }

            stopwatch.Stop();
            //UpdateTextBox(timeUnderfill, stopwatch.ElapsedMilliseconds);
            //UpdatePictureBox(underfillPictureBox, image);

            return underfillResult;
        }

        #endregion

        #region Методы работы с Modbus
        /// <summary>
        /// Отправка статуса состояния <paramref name="qualityStatus"/> на ПЛК.
        /// Установка предыдущего значения регистра ПЛК не делается. Предполагается,
        /// что логика сброса, если она необходима, будет реализована в других
        /// модулях или ПЛК
        /// </summary>
        /// <param name="qualityStatus"></param>
        private void SendQualityStatus(PLCData.QualityStatus qualityStatus)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    var stopwatch = Stopwatch.StartNew();

                    // Обновление состояния обдува
                    modbusClient.WriteSingleRegisterForBreaker(PLCData.QualityRegisterModbus, (int)qualityStatus);

                    stopwatch.Stop();
                }
            }
            catch (TaskCanceledException)
            {
                // отмена - ничего страшного
            }
            catch (Exception ex)
            {
                // TODO Логирование ошибки
            }
        }

        /// <summary>
        /// Отправка на ПЛК время обдува, то есть сколько обдув будет работать по времени.
        /// </summary>
        /// /// <param name="breakingTime"></param>
        private void SendBreakingTime(int breakingTime)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteSingleRegisterForDelayBreakerAndCameraOffset(breakingTimeRegister, breakingTime);
                }
            }
            catch (TaskCanceledException)
            {
                // отмена - ничего страшного
            }
            catch (Exception ex)
            {
                // TODO Логирование ошибки
            }
        }

        /// <summary>
        /// Отправка на ПЛК расстояния от датчика до камеры, в тиках энкодера.
        /// </summary>
        /// /// <param name="cameraOffset"></param>
        private void SendCameraOffset(int cameraOffset)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteSingleRegisterForDelayBreakerAndCameraOffset(cameraOffsetRegister, cameraOffset);
                }
            }
            catch (TaskCanceledException)
            {
                // отмена - ничего страшного
            }
            catch (Exception ex)
            {
                // TODO Логирование ошибки
            }
        }

        /// <summary>
        /// Отправка на ПЛК расстояния от датчика до отбраковщика.
        /// </summary>
        /// /// <param name="breakerOffset"></param>
        private void SendBreakerOffset(int breakerOffset)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteSingleRegisterForDelayBreakerAndCameraOffset(breakerOffsetRegister, breakerOffset);
                }
            }
            catch (TaskCanceledException)
            {
                // отмена - ничего страшного
            }
            catch (Exception ex)
            {
                // TODO Логирование ошибки
            }
        }

        private void breakingAllowCb_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    int valueToSend = breakingAllowCb.Checked
                        ? (int)BreakerAllowTrue
                        : (int)BreakerAllowFalse;

                    modbusClient.WriteSingleRegisterForBreaker(breakerAllowRegister, valueToSend);
                }
                else
                {
                    MessageBox.Show("Нет подключения к ПР205. Сигнал не отправлен.",
                        "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке сигнала на ПР205: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        #endregion

        #region Методы работы с изображениями

        public Bitmap MatToBitmap(Mat mat)
        {
            try
            {
                if (!mat.Empty())
                {
                    return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
                }
                else return new Bitmap(10, 10);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Bitmap(10, 10);
            }
        }

        private void LoadAndDisplayCurrentImage()
        {
            if (imageFiles.Count == 0) return;

            try
            {
                // Отображаем текущее изображение
                using Mat imageFromFile = new(imageFiles[currentImageIndex]);
                originPb.Image = MatToBitmap(imageFromFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
            }
        }

        #endregion

        #region Методы обновления UI

        private void UpdatePictureBox(PictureBox pictureBox, Mat image)
        {
            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new Action(() =>
                {
                    pictureBox.Image?.Dispose();
                    pictureBox.Image = BitmapConverter.ToBitmap(image);
                }));
            }
            else
            {
                pictureBox.Image?.Dispose();
                pictureBox.Image = BitmapConverter.ToBitmap(image);
            }
        }

        private void UpdateTextBox(System.Windows.Forms.TextBox textBox, float value)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() =>
                {
                    textBox.Text = value.ToString();
                }));
            }
            else
            {
                textBox.Text = value.ToString();
            }
        }

        private void UpdateTextBox(System.Windows.Forms.TextBox textBox, int value)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() =>
                {
                    textBox.Text = value.ToString();
                }));
            }
            else
            {
                textBox.Text = value.ToString();
            }
        }

        #endregion

        #region Методы обработки сигналов и FFT

        public int GetDefectFeatureFromRectifiedImage(Mat img0, Mat imgHSV, int radius, Point center, int rectW, int rectH, double threshold)
        {
            Stopwatch timer = new Stopwatch();
            string timeReport = "";

            // Извлекаем канал Saturation из HSV-изображения
            timer.Restart();
            Mat[] hsvChannels = Cv2.Split(imgHSV);
            Mat imgSaturation = hsvChannels[1];
            timer.Stop();
            timeReport += $"Извлечение Saturation-канала: {timer.Elapsed.TotalMilliseconds:F2} мс\n";

            // Масштабируем изображение
            timer.Restart();
            Mat imgScaled = new Mat();
            Cv2.Resize(imgSaturation, imgScaled, new Size(), SCALE, SCALE, InterpolationFlags.Linear);
            timer.Stop();
            timeReport += $"Масштабирование: {timer.Elapsed.TotalMilliseconds:F2} мс\n";

            // --- ПОИСК КОНТУРА И ЭЛЛИПСА ---
            timer.Restart();
            Mat gray = new Mat();
            Cv2.CvtColor(img0, gray, ColorConversionCodes.BGR2GRAY);
            timer.Stop();
            timeReport += $"Определение центра и радиуса через эллипс: {timer.Elapsed.TotalMilliseconds:F2} мс\n";

            // --- СОЗДАНИЕ РАЗВЁРНУТОГО ИЗОБРАЖЕНИЯ ---
            timer.Restart();
            Mat imgRect = new Mat(rectH, rectW, MatType.CV_8UC1);
            double[] sinTable = new double[rectW];
            double[] cosTable = new double[rectW];

            InitTrigTables(sinTable, cosTable, rectW);
            GetStripeImg(img0, imgRect, sinTable, cosTable, center, radius, img0.Width, img0.Height, rectW, rectH);
            timer.Stop();
            timeReport += $"Развёртка изображения: {timer.Elapsed.TotalMilliseconds:F2} мс\n";

            // --- СТАТИСТИКА ПО СТРОКАМ ---
            timer.Restart();
            float[] dstStat = new float[rectW];
            GetWStatistics(imgRect, dstStat, rectW, rectH);
            timer.Stop();
            timeReport += $"Статистика по строкам: {timer.Elapsed.TotalMilliseconds:F2} мс\n";

            // --- ФУРЬЕ-ПРЕОБРАЗОВАНИЕ ---
            timer.Restart();
            int decision = MyCFFT(dstStat, rectW, threshold);
            timer.Stop();
            timeReport += $"Фурье-преобразование: {timer.Elapsed.TotalMilliseconds:F2} мс\n";

            return decision;
        }

        private static void InitTrigTables(double[] sinTable, double[] cosTable, int width)
        {
            double dTheta = 2 * Math.PI / width;
            for (int i = 0; i < width; i++)
            {
                sinTable[i] = Math.Sin(i * dTheta);
                cosTable[i] = Math.Cos(i * dTheta);
            }
        }

        private static void GetStripeImg(Mat src, Mat dst, double[] sinTable, double[] cosTable, Point center, int radius, int w, int h, int width, int height)
        {
            const int INNER_OFFSET = 7;

            int hr = radius - height;
            int[] jc = Enumerable.Range(0, height).Select(j => hr + j + INNER_OFFSET).ToArray();

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    double x = sinTable[i] * jc[j] + center.X;
                    double y = cosTable[i] * jc[j] + center.Y;
                    dst.At<byte>(j, i) = Bilinear8Bit(src, x, y, w, h);
                }
            }
        }

        private static byte Bilinear8Bit(Mat img, double x, double y, int w, int h)
        {
            int u = (int)x;
            int v = (int)y;
            if (u < 0 || v < 0 || u >= w - 1 || v >= h - 1)
                return 0;

            double dx = x - u;
            double dy = y - v;

            byte p1 = img.At<byte>(v, u);
            byte p2 = img.At<byte>(v, u + 1);
            byte p3 = img.At<byte>(v + 1, u);
            byte p4 = img.At<byte>(v + 1, u + 1);

            double interpolated = (1 - dx) * (1 - dy) * p1 +
                                  dx * (1 - dy) * p2 +
                                  (1 - dx) * dy * p3 +
                                  dx * dy * p4;

            return (byte)Math.Round(interpolated);
        }

        private static void GetWStatistics(Mat src, float[] dst, int w, int h)
        {
            Array.Clear(dst, 0, dst.Length);
            unsafe
            {
                byte* srcPtr = (byte*)src.DataPointer;
                fixed (float* dstPtr = dst)
                {
                    for (int j = 0; j < h; j++)
                    {
                        byte* rowPtr = srcPtr + j * w;
                        for (int i = 0; i < w; i++)
                            dstPtr[i] += rowPtr[i];
                    }
                }
            }
        }

        public int MyCFFT(float[] v, int n, double threshold)
        {
            if (n <= 0 || v == null || v.Length < n)
                throw new ArgumentException("Некорректные входные данные");

            // 1. Центрируем сигнал (вычитаем среднее)
            double mean = 0;
            for (int i = 0; i < n; i++)
                mean += v[i];
            mean /= n;
            for (int i = 0; i < n; i++)
                v[i] -= (float)mean;

            // 2. Преобразуем сигнал в массив комплексных чисел
            Complex[] spectrum = new Complex[n];
            for (int i = 0; i < n; i++)
                spectrum[i] = new Complex(v[i], 0);

            // 3. Выполняем быстрое преобразование Фурье (FFT)
            Fourier.Forward(spectrum, FourierOptions.NoScaling);

            // 4. Вычисляем модули спектра в нужном диапазоне
            double[] magnitudes = new double[n];
            for (int i = MIN_HARMONIC_INDEX; i < MAX_HARMONIC_INDEX; i++)
                magnitudes[i] = spectrum[i].Magnitude;

            // 5. Поиск максимальной и второй по величине гармоники
            double max_value = magnitudes[MIN_HARMONIC_INDEX];
            int max_index = MIN_HARMONIC_INDEX;

            for (int i = MIN_HARMONIC_INDEX + 1; i < MAX_HARMONIC_INDEX; i++)
            {
                if (magnitudes[i] > max_value)
                {
                    max_value = magnitudes[i];
                    max_index = i;
                }
            }

            double sub_max_value = 0;
            for (int i = MIN_HARMONIC_INDEX; i < MAX_HARMONIC_INDEX; i++)
            {
                if (i != max_index && magnitudes[i] > sub_max_value)
                    sub_max_value = magnitudes[i];
            }

            // 6. Проверка критерия дефекта
            double ratio = max_value / sub_max_value;
            return ratio > threshold ? 1 : 0;
        }

        #endregion

        #region Методы цветовой обработки

        public static void NonlinearBackgroundDecolorization(Mat img, byte nWhite)
        {
            if (img.Empty() || img.Type() != MatType.CV_8UC3)
                throw new ArgumentException("Ожидается 3-канальное 8-битное изображение.");

            int total = img.Rows * img.Cols * 3;

            unsafe
            {
                byte* data = (byte*)img.DataPointer;

                // 1️⃣ Выбеливание, если крышка не зелёная
                if (!isGreenColor)
                {
                    for (int i = 0; i < total; i++)
                    {
                        int val = (255 * data[i]) / nWhite;
                        if (val > 255) val = 255;
                        data[i] = (byte)val;
                    }
                }

                // 2️⃣ Если крышка цветная
                if (isColored)
                {
                    for (int i = 0; i < total; i += 3)
                    {
                        if (!isGreenColor)
                        {
                            // Цветоразностная компонента (для не зелёных)
                            data[i] = (byte)Math.Abs(data[i] - ((data[i + 1] + data[i + 2]) >> 1));
                        }
                        else
                        {
                            // Для зелёных крышек — бинаризация по синему каналу
                            data[i] = (data[i] < GREEN_THRESHOLD) ? (byte)0x00 : (byte)0xFF;
                        }
                    }
                }
            }
        }


        private unsafe void GetHueChannel(Mat img, int[] arrIndex, byte[] hueLUT)
        {
            int totalPixels = img.Cols * img.Rows;
            byte* imgData = (byte*)img.Data.ToPointer();

            for (int i = 0; i < totalPixels; i++)
            {
                int pixelOffset = i * 3;

                byte b = imgData[pixelOffset + arrIndex[2]];
                byte g = imgData[pixelOffset + arrIndex[0]];
                byte r = imgData[pixelOffset + arrIndex[1]];

                // Вычисляем индекс в LUT таблице
                int lutIndex = ((b >> 1) << 14) + ((g >> 1) << 7) + (r >> 1);

                // Записываем Hue значение в первый канал (синий)
                imgData[pixelOffset] = hueLUT[lutIndex];
            }
        }

        private void GetArrIndex(int[] arr, int[] arrIndex)
        {
            // Инициализация индексов по умолчанию (BGR)
            arrIndex[0] = 0; // B
            arrIndex[1] = 1; // G
            arrIndex[2] = 2; // R

            if (arr[1] < arr[0])
            {
                Swap(ref arr[0], ref arr[1]);
                Swap(ref arrIndex[0], ref arrIndex[1]);
            }

            if (arr[2] < arr[1])
            {
                Swap(ref arr[1], ref arr[2]);
                Swap(ref arrIndex[1], ref arrIndex[2]);
                if (arr[1] < arr[0])
                {
                    Swap(ref arr[1], ref arr[0]);
                    Swap(ref arrIndex[1], ref arrIndex[0]);
                }
            }
        }

        private void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        private unsafe void Fast_RGB_pseudo_color(Mat img, int sparse, byte[] hueLut)
        {
            int[] arr = new int[3];
            int[] arrIndex = new int[3];

            // Подсчёт интенсивности в каждом канале
            byte* imgData = (byte*)img.Data.ToPointer();
            int count = 0;

            for (int j = 0; j < img.Rows; j += sparse)
            {
                int rowOffset = j * img.Cols * 3;
                for (int i = 0; i < img.Cols * 3; i += 3 * sparse)
                {
                    int pixelOffset = rowOffset + i;
                    arr[0] += imgData[pixelOffset];     // B
                    arr[1] += imgData[pixelOffset + 1]; // G
                    arr[2] += imgData[pixelOffset + 2]; // R
                    count++;
                }
            }

            // Вычисляем среднюю интенсивность
            arr[0] /= count;
            arr[1] /= count;
            arr[2] /= count;

            // Получаем порядок каналов
            GetArrIndex(arr, arrIndex);

            // Вычисляем Hue канал
            GetHueChannel(img, arrIndex, hueLut);
        }

        #endregion

        #region Дополнительные методы проверки (для полноты)

        private bool CheckUnderfill(Mat gray, Mat imgColor)
        {
            // Размытие
            Mat smoothImage = new Mat();
            Cv2.BoxFilter(imgColor, smoothImage, -1, new Size(4, 4), new Point(-1, -1), true, BorderTypes.Default);

            // Преобразование в HSV
            Mat imgHSV = new Mat();
            Cv2.CvtColor(smoothImage, imgHSV, ColorConversionCodes.BGR2HSV);

            Point[] capContour = GetCapContour(gray, imgColor);

            if (capContour == null || capContour.Length < 5)
            {
                return true;
            }

            // Находим эллипс по контуру
            RotatedRect ellipse = Cv2.FitEllipse(capContour);
            Point center = new Point((int)ellipse.Center.X, (int)ellipse.Center.Y);
            int radius = (int)(Math.Max(ellipse.Size.Width, ellipse.Size.Height) / 2);

            // Рисуем найденный круг
            Cv2.Circle(imgColor, center, radius, new Scalar(0, 255, 0), 2);
            Cv2.Circle(imgColor, center, 5, new Scalar(0, 0, 255), -1);

            // Получаем признак недолива
            int decision = GetDefectFeatureFromRectifiedImage(imgColor, imgHSV, radius, center, 1024, (int)(radius * 0.08), TRESHOLD);

            return decision == 0;
        }

        private void Form1_Load()
        {
            labelCoordinates.Location = new System.Drawing.Point(10, 10);
            labelCoordinates.AutoSize = true;
            Controls.Add(labelCoordinates);
        }

        #endregion

        private void originPb_Click(object sender, EventArgs e)
        {

        }
    }
}