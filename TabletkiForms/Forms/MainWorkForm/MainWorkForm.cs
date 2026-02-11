//#define OLD_FRAME_PROCESSING

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using KrishkiForms.Authorization;
using KrishkiForms.CameraAndModbusClasses;
using KrishkiForms.Forms;
using KrishkiForms.FrameProcessing;
using KrishkiForms.Hardware;
using KrishkiForms.Logger;
using Kvantron.Hardware.SmartDio;
using Kvantron.UI.Controls.Utils;
using MathNet.Numerics.IntegralTransforms;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;
using Timer = System.Windows.Forms.Timer;

namespace KrishkiForms
{
    public partial class MainWorkForm : Form
    {
        #region Поля и константы

        // UI элементы
        private System.Windows.Forms.ToolTip tooltip = new System.Windows.Forms.ToolTip();
        private Label labelCoordinates = new Label();
        private Color connectedColor = Color.FromArgb(229, 115, 115); // красный — отключить
        private Color disconnectedColor = Color.FromArgb(4, 85, 191); // синий — подключить
        private bool capsAreWhite = false;
        private bool manualDisconnect = false;

        // Оборудование
        private DioModule module = null;
        private HikCamera cam;
        private ModbusTCP modbusClient;
        private int breakingTimeRegister = 16466;
        private int cameraOffsetRegister = 16402;
        private int breakerOffsetRegister = 16404;
        private int breakerAllowRegister = 16401;
        private int startRecognizeProcessing = 16400;
        private int startCountingImagesAfterReceivngImages = 16700;
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
        private float percentOvalityCaps = 0;
        private float percentInclusionCaps = 0;
        private float percentInpaintCaps = 0;
        private float percentObloyCaps = 0;
        private float generalCapsCount = 0;
        private float okCapsCount = 0;
        private float ngCapsCount = 0;
        private float percentOkCaps = 0;
        private float percentNgCaps = 0;
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
        private const byte GREEN_THRESHOLD = 40;
        private const byte ORANGE_CAPS = 80;
        private static bool isGreenColor = false;
        private static bool isColored = true;
        private static bool isYellowCap = false;
        private static int saturation = 0;

        //Для работы с файлами цветов крышек
        private Dictionary<string, CapRecipe> _recipes = new();
        private string RecipesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Рецепты");
        private bool _isApplyingRecipe = false;

        // Морфологические элементы
        private Mat element1;
        private Mat element2;
        private Mat elementMask;
        private int window = 15;
        private int morph_size = 7;
        private int morph_size_2 = 7;
        const float outlierThreshold = 1.15f;

        // Параметры дефектов
        private double ovalityThreshold = 0.7;
        private double minInpaintWhiteThreshold = 150.0;
        private double minAreaInpaintDefect = 500.0;
        private double inclusionThreshold = 0.5;
        private double minAreaInclusion = 50.0;
        private double maxAreaInclusion = 500.0;
        private double coefCapRadiusInclusion = 0.7;
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
        private Mat _frameToDisplay;
        private Mat _frameToSave;
        private Mat _imageOriginReceptParam;

        //Изображение для тестирования параметров
        private Mat _imageForTest;
        private Mat _imageForTestForDisplay;

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
        private FileSystemWatcher _recipesWatcher;
        private string folderParamDefect = AppDomain.CurrentDomain.BaseDirectory + @"Настройки\Настройка параметров дефектов";
        private string folderParamCamera= AppDomain.CurrentDomain.BaseDirectory + @"Настройки\Настройка аппаратуры\Настройки камеры";
        private string folderParamPr205 = AppDomain.CurrentDomain.BaseDirectory + @"Настройки\Настройка аппаратуры\Настройки ПР205";

        // Логирование
        private readonly ErrorLogger logger = new ErrorLogger();
        private DateTime? _lastImageReceivedTime = null;
        private readonly string _logFilePath = "SendImageLog.txt";
        private readonly string _processTimeLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProcessTime.txt");
        private ImmutableList<string> imageFiles = [];
        private int currentImageIndex = 0;
        private int _writeZeroFailCount = 0;
        private int _writeOneFailCount = 0;
        private int currentFrameNumber = 0;

        //Избежание дубликатов кадров
        private ulong _lastFrameHash = 0;
        private bool _hasLastHash = false;
        private long _skippedDuplicateFrames = 0;
        private byte[][]? _lastRows;
        private bool _hasLastRows = false;
        private Mat? _lastFrameForDuplicate;

        //Режим вывода изображения: Все, хорошие, плохие
        private enum OutputMode
        {
            All,
            Good,
            Bad
        }
        private OutputMode _outputMode = OutputMode.All;

        private Timer roleDisplayTimer = new Timer();

        #endregion

        #region Конструктор и инициализация

        public MainWorkForm(HikCamera camera, ModbusTCP modbus)
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

                modbusClient.ConnectionStatusChanged += ModbusClient_ConnectionStatusChanged;
                modbusClient.EnableAutoReconnect();
                modbusClient.StartPolling();
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

        private void ModbusClient_ConnectionStatusChanged(bool isConnected)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ModbusClient_ConnectionStatusChanged(isConnected)));
                return;
            }

            if (isConnected)
            {

                if (cameraOffsetTb != null)
                {
                    cameraOffsetTb.Text = CameraOffset.ToString();
                    SendCameraOffset(CameraOffset);
                }

                if (breakerOffsetTb != null)
                {
                    breakerOffsetTb.Text = BreakerOffset.ToString();
                    SendBreakerOffset(BreakerOffset);
                }

                if (breakingTimeTb != null)
                {
                    breakingTimeTb.Text = BreakingTime.ToString();
                    SendBreakingTime(BreakingTime);
                }

                manualDisconnect = false;

                prStatus.Text = "Подключено";
                prStatus.ForeColor = Color.Green;

                connectPrButton.Text = "Отключиться от ПР";
                connectPrButton.BackColor = connectedColor;

                prConnected = true;
                breakingAllowCb.Enabled = true;
                applyPrBreakerParamButton.Enabled = true;
            }
            else
            {
                if (manualDisconnect)
                {
                    prStatus.Text = "Откл. вручную";
                    prStatus.ForeColor = Color.Red;
                }
                else
                {
                    prStatus.Text = "Не подключено";
                    prStatus.ForeColor = Color.Red;
                }

                connectPrButton.Text = "Подключиться к ПР";
                connectPrButton.BackColor = disconnectedColor;

                prConnected = false;
                breakingAllowCb.Enabled = false;
                applyPrBreakerParamButton.Enabled = false;
            }
        }

        private void InitializeApplication()
        {
            InitializeCoreSystems();
            InitializePR205Status();
            InitializeCameraStatus();
            InitializeCycleSystem();
            InitializeUISelections();
            InitializeAuthorizationSystem();
        }

        private void InitializeCoreSystems()
        {
            InitializeHueLUT();
            InitializeImageMatrices();
            InitializePaths();
            LoadDefectAndCameraParam();
            InitializeRecepts();
            InitializeMorphologicalElements();
            Form1_Load();

            recognizeButton.Enabled = false;

            StartStop(false, true);
            LocalSettings.Instance.Save();
        }

        private void InitializePR205Status()
        {
            prIpTextBox.Text = Properties.Settings.Default.IpAdressPr;

            int savedPort;
            if (int.TryParse(Properties.Settings.Default.PortPr, out savedPort))
                pr205PortTb.Text = savedPort.ToString();
            else
                pr205PortTb.Text = "502";

            if (modbusClient != null && modbusClient.Connected)
            {
                prStatus.Text = "Подключено";
                prStatus.ForeColor = Color.Green;
                connectPrButton.Text = "Отключиться от ПР";
                connectPrButton.BackColor = connectedColor;
            }
            else
            {
                prStatus.Text = "Не подключено";
                prStatus.ForeColor = Color.Red;
                connectPrButton.Text = "Подключиться к ПР";
                connectPrButton.BackColor = disconnectedColor;
            }
        }

        private void InitializeCameraStatus()
        {
            if (cam != null && cam.Connected)
            {
                cam.SendImage += GetImage;
                camStatus.Text = "Подключено";
                camStatus.ForeColor = Color.Green;
                connectCameraButton.Text = "Отключиться от камеры";
                connectCameraButton.BackColor = connectedColor;
                cameraIpTextBox.Text = cam.IpAdress;
            }
            else
            {
                camStatus.Text = "Не подключено";
                camStatus.ForeColor = Color.Red;
                connectCameraButton.Text = "Подключиться к камере";
                connectCameraButton.BackColor = disconnectedColor;
                cameraIpTextBox.Text = "Камера не выбрана на этапе инициализации";
            }
        }

        private void InitializeCycleSystem()
        {
            int cycle = 0;
            int.TryParse(Properties.Settings.Default.LastCycleTime, out cycle);

            // защита от неправильного значения
            if (cycle < cycleUpDown.Minimum)
            {
                cycle = (int)cycleUpDown.Minimum;
                Properties.Settings.Default.LastCycleTime = cycle.ToString();
                Properties.Settings.Default.Save();
            }

            CycleImageSaver.SetCycleHours(cycle);

            if (cycleUpDown != null)
                cycleUpDown.Value = cycle;

            CycleImageSaver.Init();
            currentFolderTb.Text = CycleImageSaver.CurrentCycleFolder;
        }

        private void InitializeUISelections()
        {
            if (windowCb.Items.Count > 0)
                windowCb.SelectedIndex = 0;

            if (morphCb.Items.Count > 0)
                morphCb.SelectedIndex = 0;
        }

        private void InitializeAuthorizationSystem()
        {
            AuthManager.Instance.RoleChanged += OnRoleChanged;

            AuthManager.Instance.SetRole(Role.Operator);
            ApplyRoleRestrictions(AuthManager.Instance.CurrentRole);
            UpdateRoleUI(AuthManager.Instance.CurrentRole);

            roleDisplayTimer.Interval = 1000;
            roleDisplayTimer.Tick += (s, e) =>
            {
                if (AuthManager.Instance.CurrentRole == Role.Admin)
                    timeLeftTb.Text = AuthManager.Instance.GetSecondsLeft().ToString();
                else
                    timeLeftTb.Text = "∞";
            };

            roleDisplayTimer.Start();
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
            _frameToDisplay = new Mat();
            _imageForTest = new Mat();
            _imageForTestForDisplay = new Mat();
            _frameToSave = new Mat();
            _imageOriginReceptParam = new Mat();
        }

        private void InitializeMorphologicalElements()
        {
            RebuildMorphology();
        }

        private void RebuildMorphology()
        {
            element1 = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new Size(2 * morph_size + 1, 2 * morph_size + 1),
                new Point(morph_size, morph_size)
            );

            element2 = Cv2.GetStructuringElement(
                MorphShapes.Cross,
                new Size(2 * morph_size_2 + 1, 2 * morph_size_2 + 1),
                new Point(morph_size_2, morph_size_2)
            );

            elementMask = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new Size(3, 3),
                new Point(1, 1)
            );
        }

        private void InitializePaths()
        {
            currentReceptFolderTb.Text = RecipesFolder;

            if (!Directory.Exists(RecipesFolder))
                Directory.CreateDirectory(RecipesFolder);

            // Настройка FileSystemWatcher
            _recipesWatcher = new FileSystemWatcher
            {
                Path = RecipesFolder,
                Filter = "*.json",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            _recipesWatcher.Created += OnRecipesFolderChanged;
            _recipesWatcher.Deleted += OnRecipesFolderChanged;
            _recipesWatcher.Renamed += OnRecipesFolderChanged;

            _recipesWatcher.EnableRaisingEvents = true;
        }

        private void OnRecipesFolderChanged(object sender, FileSystemEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ReloadRecipes()));
            }
            else
            {
                ReloadRecipes();
            }
        }

        // Метод для обновления комбобокса
        private void ReloadRecipes()
        {
            LoadRecipes();

            receptCapsCmB.Items.Clear();
            foreach (var recipeName in _recipes.Keys)
                receptCapsCmB.Items.Add(recipeName);
        }

        private void LoadDefectAndCameraParam()
        {
            // ===== Параметры ПР =====
            int.TryParse(Properties.Settings.Default.BreakingTime, out BreakingTime);
            int.TryParse(Properties.Settings.Default.BreakerOffset, out BreakerOffset);
            int.TryParse(Properties.Settings.Default.CameraOffset, out CameraOffset);

            if (cameraOffsetTb != null)
            {
                cameraOffsetTb.Text = CameraOffset.ToString();
                SendCameraOffset(CameraOffset);
            }

            if (breakerOffsetTb != null)
            {
                breakerOffsetTb.Text = BreakerOffset.ToString();
                SendBreakerOffset(BreakerOffset);
            }

            if (breakingTimeTb != null)
            {
                breakingTimeTb.Text = BreakingTime.ToString();
                SendBreakingTime(BreakingTime);
            }

            // ===== Параметры дефектов (автоподгрузка при запуске) =====
            if (!string.IsNullOrEmpty(Properties.Settings.Default.OvalityThreshold))
                ovalityCoefNumUpD.Text = Properties.Settings.Default.OvalityThreshold;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.InclusionThreshold))
                circleCoefNumUpD.Text = Properties.Settings.Default.InclusionThreshold;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.MinAreaInclusion))
                minSquareInclusionNumUpD.Text = Properties.Settings.Default.MinAreaInclusion;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.MaxAreaInclusion))
                maxSquareInclusionNumUpD.Text = Properties.Settings.Default.MaxAreaInclusion;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.CoefCapRadiusInclusion))
                coefCapRadiusInclusionUpD.Text = Properties.Settings.Default.CoefCapRadiusInclusion;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.MinAreaInpaintDefect))
                minSquareInpaintNumUpD.Text = Properties.Settings.Default.MinAreaInpaintDefect;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.MinInpaintWhiteThreshold))
                whiteThresoldNumUpD.Text = Properties.Settings.Default.MinInpaintWhiteThreshold;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.MinAreaObloy))
                obloyPixCountNumUpD.Text = Properties.Settings.Default.MinAreaObloy;

            // ===== Параметры камеры (Width, Height, Exposure, Saturation) =====
            if (!string.IsNullOrEmpty(Properties.Settings.Default.WidthFrame))
                frameWidthNumUpD.Text = Properties.Settings.Default.WidthFrame;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.HeightFrame))
                frameHeightNumUpD.Text = Properties.Settings.Default.HeightFrame;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.ExposureFrame))
                frameExposureNumUpD.Text = Properties.Settings.Default.ExposureFrame;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.SaturationFrame))
                frameSaturationNumUpD.Text = Properties.Settings.Default.SaturationFrame;
        }

        private void SendStopSignalsToPLC()
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    // Сбрасываем breakerAllowRegister
                    modbusClient.WriteRegister(breakerAllowRegister, 0);

                    // Сбрасываем startRecognizeProcessing
                    if (!isImageLoaded)
                        modbusClient.WriteRegister(startRecognizeProcessing, 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка отправки в PLC: " + ex.Message);
            }
        }

        private void InitializeRecepts()
        {
            LoadRecipes();

            receptCapsCmB.Items.Clear();
            foreach (var recipe in _recipes.Keys)
                receptCapsCmB.Items.Add(recipe);

            if (_recipes.Count > 0)
            {
                receptCapsCmB.SelectedIndex = 0;
                var first = _recipes.Values.First();
                ApplyRecipe(first);
            }
        }

        private void LoadRecipes()
        {
            _recipes.Clear();

            if (!Directory.Exists(RecipesFolder))
                Directory.CreateDirectory(RecipesFolder);

            string[] files = Directory.GetFiles(RecipesFolder, "*.json");

            foreach (var file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);

                CapRecipe recipe = JsonConvert.DeserializeObject<CapRecipe>(json);
                if (recipe != null)
                    _recipes[name] = recipe;
            }
        }

        #endregion

        #region Обработчики событий UI

        #region Кнопки управления оборудованием

        private void connectCameraButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (cameraConnected)
                {
                    cam.SendImage -= GetImage;
                    cam.Close();

                    cameraConnected = false;
                    connectCameraButton.Text = "Подключиться к камере";
                    connectCameraButton.BackColor = disconnectedColor;

                    camStatus.Text = "Не подключено";
                    camStatus.ForeColor = Color.Red;

                    startStreamButton.Enabled = false;

                    ErrorLogger.Log(new Exception("Камера отключена пользователем"), "connectCameraButton_Click");
                }
                else
                {
                    if (cam.Open())
                    {
                        cam.SendImage -= GetImage;
                        cam.SendImage += GetImage;
                        cameraConnected = true;
                        connectCameraButton.Text = "Отключиться от камеры";
                        connectCameraButton.BackColor = connectedColor;

                        camStatus.Text = "Подключено";
                        camStatus.ForeColor = Color.Green;

                        startStreamButton.Enabled = true;

                        ErrorLogger.Log(new Exception("Камера успешно подключена"), "connectCameraButton_Click");
                    }
                    else
                    {
                        cameraConnected = false;
                        connectCameraButton.Text = "Подключиться к камере";
                        connectCameraButton.BackColor = disconnectedColor;

                        camStatus.Text = "Не подключено";
                        camStatus.ForeColor = Color.Red;

                        startStreamButton.Enabled = false;

                        ErrorLogger.Log(new Exception("Не удалось подключиться к камере"), "connectCameraButton_Click");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в connectCameraButton_Click");
                cameraConnected = false;
                connectCameraButton.Text = "Подключиться";
                connectCameraButton.BackColor = disconnectedColor;
                camStatus.Text = "Не подключено";
                camStatus.ForeColor = Color.Red;
                startStreamButton.Enabled = false;
            }
        }

        private void connectPrButton_Click(object sender, EventArgs e)
        {
            if (prConnected && modbusClient != null && modbusClient.Connected)
            {
                try
                {
                    manualDisconnect = true;

                    modbusClient.DisableAutoReconnect();
                    modbusClient.StopPolling();
                    modbusClient.Disconnect();

                    ModbusClient_ConnectionStatusChanged(false);

                    ErrorLogger.Log(
                        new Exception("Отключение от ПР205 выполнено успешно"),
                        "connectPrButton_Click"
                    );

                    MessageBox.Show(
                        "Соединение с ПР205 разорвано.",
                        "Отключение",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "Ошибка при отключении от ПР205");

                    MessageBox.Show(
                        $"Ошибка при отключении от ПР205:\n{ex.Message}",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }

                return;
            }

            try
            {
                string ip = prIpTextBox.Text.Trim();

                if (!int.TryParse(pr205PortTb.Text.Trim(), out int port))
                    throw new Exception("Неверный формат порта ПР205");

                if (modbusClient != null)
                    modbusClient.ConnectionStatusChanged -= ModbusClient_ConnectionStatusChanged;

                modbusClient = new ModbusTCP(ip, port);
                modbusClient.ConnectionStatusChanged += ModbusClient_ConnectionStatusChanged;
                modbusClient.EnableAutoReconnect();

                if (modbusClient.Connect())
                {
                    modbusClient.StartPolling();

                    ModbusClient_ConnectionStatusChanged(true);

                    ErrorLogger.Log(
                        new Exception("Подключение к ПР205 установлено успешно"),
                        "connectPrButton_Click"
                    );

                    MessageBox.Show(
                        "Соединение с ПР205 успешно установлено!",
                        "Успех",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    manualDisconnect = false;
                    ModbusClient_ConnectionStatusChanged(false);

                    ErrorLogger.Log(
                        new Exception("Не удалось подключиться к ПР205"),
                        "connectPrButton_Click"
                    );

                    MessageBox.Show(
                        "Не удалось подключиться к ПР205.\nПроверьте IP, порт и кабель.",
                        "Ошибка подключения",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                manualDisconnect = false;
                ModbusClient_ConnectionStatusChanged(false);

                ErrorLogger.Log(ex, "Ошибка подключения к ПР205");

                MessageBox.Show(
                    $"Ошибка подключения к ПР205:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
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

                imageFiles = [];
                isProcessingFromFolder = false;

                // Восстанавливаем внешний вид кнопки
                loadImageButton.Text = "Загрузить";
                loadImageButton.BackColor = Color.FromArgb(66, 133, 244); // Синий

                // Разблокируем кнопку запуска потока
                if (cameraConnected)
                {
                    startStreamButton.Enabled = true;
                }

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
                        if (imageFiles.Count > 0)
                        {
                            isProcessingFromFolder = true;
                        }

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
            try
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
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в startStreamButton_Click");
            }
        }


        private void StartStream()
        {
            try
            {
                if (originalImage != null)
                {
                    originalImage = null;
                }

                isStreamCam = true;

                ApplyRecognitionParameters();

                if (AuthManager.Instance.CurrentRole == Role.Operator)
                {
                    loadImageButton.Enabled = false;
                }

                if (AuthManager.Instance.CurrentRole == Role.Admin)
                {
                    loadImageButton.Enabled = false;
                    connectCameraButton.Enabled = false;
                    connectPrButton.Enabled = false;
                }

                StartStop(true);

                startStreamButton.Text = "Остановить";
                startStreamButton.BackColor = Color.FromArgb(229, 115, 115);
                cameraStatusLabel.Text = "Запущен";
                cameraStatusLabel.ForeColor = Color.Green;
                isStreamRunning = true;
                recognizeButton.Enabled = true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в StartStream");

                connectCameraButton.Enabled = true;
                connectPrButton.Enabled = true;
                loadImageButton.Enabled = true;
                isStreamRunning = false;
            }
        }

        private void StopStream()
        {
            isRoiProduce = false;
            isROISelected = false;
            isStreamCam = false;

            originPb.Image?.Dispose();
            originPb.Image = null;

            // 🔓 Разблокируем кнопки подключения
            if (AuthManager.Instance.CurrentRole == Role.Admin)
            {
                connectCameraButton.Enabled = true;
                connectPrButton.Enabled = true;
            }
            loadImageButton.Enabled = true;

            StartStop(false);

            startStreamButton.Text = "Изображение с камеры";
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
            try
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
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в recognizeButton_Click");
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
                    saveFileDialog.InitialDirectory = folderParamPr205;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveFileDialog.FileName, json);

                        Properties.Settings.Default.BreakingTime = breakingTimeTb.Text;
                        Properties.Settings.Default.CameraOffset = cameraOffsetTb.Text;
                        Properties.Settings.Default.BreakerOffset = breakerOffsetTb.Text;
                        Properties.Settings.Default.IpAdressPr = prIpTextBox.Text;
                        Properties.Settings.Default.PortPr = pr205PortTb.Text;
                        Properties.Settings.Default.Save();

                        MessageBox.Show("Настройки ПР205 успешно сохранены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка при сохранении настроек ПР205 (savePrSettings_Click)");
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
                    openFileDialog.InitialDirectory = folderParamPr205;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string json = File.ReadAllText(openFileDialog.FileName);
                        var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        if (settings != null)
                        {
                            prIpTextBox.Text = settings.ContainsKey("IPAddress") ? settings["IPAddress"] : "10.10.69.38";
                            pr205PortTb.Text = settings.ContainsKey("Port") ? settings["Port"] : "502";
                            breakingTimeTb.Text = settings.ContainsKey("BreakingTime") ? settings["BreakingTime"] : "55";
                            cameraOffsetTb.Text = settings.ContainsKey("CameraOffset") ? settings["CameraOffset"] : "300";
                            breakerOffsetTb.Text = settings.ContainsKey("BreakerOffset") ? settings["BreakerOffset"] : "2430";

                            MessageBox.Show("Настройки ПР205 успешно загружены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            ErrorLogger.Log(new Exception("Не удалось прочитать настройки из файла"),
                                "loadPrSettings_Click");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка при загрузке настроек ПР205 (loadPrSettings_Click)");
            }
        }

        private bool ValidatePrSettings()
        {
            try
            {
                if (!System.Net.IPAddress.TryParse(prIpTextBox.Text, out _))
                    throw new Exception("Неверный формат IP адреса");

                if (!int.TryParse(pr205PortTb.Text, out int port) || port < 1 || port > 65535)
                    throw new Exception("Порт должен быть числом от 1 до 65535");

                if (!int.TryParse(breakingTimeTb.Text, out int delay) || delay < 0)
                    throw new Exception("Задержка должна быть положительным числом");

                if (!int.TryParse(cameraOffsetTb.Text, out int cameraOffset) || cameraOffset < 0)
                    throw new Exception("Расстояние от датчика до камеры должно быть положительным числом");

                if (!int.TryParse(breakerOffsetTb.Text, out int breakerOffset) || breakerOffset < 0)
                    throw new Exception("Расстояние от датчика до отбраковщика должно быть положительным числом");

                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка валидации настроек ПР205 (ValidatePrSettings)");
                return false;
            }
        }


        private async void ApplyPr_Click(object sender, EventArgs e)
        {
            if (modbusClient == null || !modbusClient.Connected)
            {
                // Централизованное обновление интерфейса при отсутствии соединения
                ModbusClient_ConnectionStatusChanged(false);
                return;
            }

            cts = new CancellationTokenSource();

            try
            {
                if (!int.TryParse(breakingTimeTb.Text.Trim(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out BreakingTime) || BreakingTime <= 0)
                {
                    BreakingTime = 55;
                }

                await Task.Run(() => SendBreakingTime(BreakingTime), cts.Token);

                if (!int.TryParse(cameraOffsetTb.Text.Trim(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out CameraOffset) || CameraOffset <= 0)
                {
                    CameraOffset = 300;
                }

                await Task.Run(() => SendCameraOffset(CameraOffset), cts.Token);

                if (!int.TryParse(breakerOffsetTb.Text.Trim(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out BreakerOffset) || BreakerOffset <= 0)
                {
                    BreakerOffset = 2430;
                }

                await Task.Run(() => SendBreakerOffset(BreakerOffset), cts.Token);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка при отправке параметров на ПР205 (ApplyPr_Click)");
                ModbusClient_ConnectionStatusChanged(false); // тоже блокируем элементы при ошибке
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
                    OvalityThreshold = ovalityCoefNumUpD.Text,
                    InclusionThreshold = circleCoefNumUpD.Text,
                    MinAreaInclusion = minSquareInclusionNumUpD.Text,
                    MaxAreaInclusion = maxSquareInclusionNumUpD.Text,
                    CoefCapRadiusInclusion = coefCapRadiusInclusionUpD.Text,
                    MinAreaInpaintDefect = minSquareInpaintNumUpD.Text,
                    MinInpaintWhiteThreshold = whiteThresoldNumUpD.Text,
                    MinAreaObloy = obloyPixCountNumUpD.Text
                };

                string json = System.Text.Json.JsonSerializer.Serialize(settings,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                if (!Directory.Exists(folderParamDefect))
                    Directory.CreateDirectory(folderParamDefect);

                using (SaveFileDialog dlg = new SaveFileDialog())
                {
                    dlg.InitialDirectory = folderParamDefect;
                    dlg.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    dlg.Title = "Сохранить настройки дефектов";
                    dlg.FileName = "defect_settings.json";

                    if (dlg.ShowDialog() == DialogResult.OK)
                        File.WriteAllText(dlg.FileName, json);
                }

                // Сохраняем в Settings
                Properties.Settings.Default.OvalityThreshold = ovalityCoefNumUpD.Text;
                Properties.Settings.Default.InclusionThreshold = circleCoefNumUpD.Text;
                Properties.Settings.Default.MinAreaInclusion = minSquareInclusionNumUpD.Text;
                Properties.Settings.Default.MaxAreaInclusion = maxSquareInclusionNumUpD.Text;
                Properties.Settings.Default.CoefCapRadiusInclusion = coefCapRadiusInclusionUpD.Text;
                Properties.Settings.Default.MinAreaInpaintDefect = minSquareInpaintNumUpD.Text;
                Properties.Settings.Default.MinInpaintWhiteThreshold = whiteThresoldNumUpD.Text;
                Properties.Settings.Default.MinAreaObloy = obloyPixCountNumUpD.Text;

                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка при сохранении настроек дефектов");
            }
        }

        private void loadDefectSettings_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(folderParamDefect))
                    Directory.CreateDirectory(folderParamDefect);

                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.InitialDirectory = folderParamDefect;
                    dlg.Title = "Загрузка параметров дефектов";
                    dlg.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        string json = File.ReadAllText(dlg.FileName);

                        var settings =
                            System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        if (settings == null) return;

                        ovalityCoefNumUpD.Text = settings.GetValueOrDefault("OvalityThreshold", "0.7");
                        circleCoefNumUpD.Text = settings.GetValueOrDefault("InclusionThreshold", "0.5");
                        minSquareInclusionNumUpD.Text = settings.GetValueOrDefault("MinAreaInclusion", "50");
                        maxSquareInclusionNumUpD.Text = settings.GetValueOrDefault("MaxAreaInclusion", "500");
                        coefCapRadiusInclusionUpD.Text = settings.GetValueOrDefault("CoefCapRadiusInclusion", "0,7");
                        minSquareInpaintNumUpD.Text = settings.GetValueOrDefault("MinAreaInpaintDefect", "500");
                        whiteThresoldNumUpD.Text = settings.GetValueOrDefault("MinInpaintWhiteThreshold", "150");
                        obloyPixCountNumUpD.Text = settings.GetValueOrDefault("MinAreaObloy", "1000");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка при загрузке настроек дефектов");
            }
        }


        private bool ValidateDefectSettings()
        {
            try
            {
                if (!double.TryParse(ovalityCoefNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double ovality) || ovality <= 0 || ovality > 1)
                {
                    ErrorLogger.Log(new Exception("Параметр 'OvalityThreshold' некорректен"), "ValidateDefectSettings");
                    return false;
                }

                if (!double.TryParse(circleCoefNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double inclusion) || inclusion <= 0 || inclusion > 1)
                {
                    ErrorLogger.Log(new Exception("Параметр 'InclusionThreshold' некорректен"), "ValidateDefectSettings");
                    return false;
                }

                if (!double.TryParse(coefCapRadiusInclusionUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double coefCapRadius) || coefCapRadius <= 0 || coefCapRadius > 1)
                {
                    ErrorLogger.Log(new Exception("Параметр 'coefCapRadius' некорректен"), "ValidateDefectSettings");
                    return false;
                }

                if (!double.TryParse(minSquareInclusionNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double minInclusion) || minInclusion < 0)
                {
                    ErrorLogger.Log(new Exception("Параметр 'MinAreaInclusion' некорректен"), "ValidateDefectSettings");
                    return false;
                }

                if (!double.TryParse(maxSquareInclusionNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double maxInclusion) || maxInclusion <= minInclusion)
                {
                    ErrorLogger.Log(new Exception("Параметр 'MaxAreaInclusion' должен быть больше MinAreaInclusion"), "ValidateDefectSettings");
                    return false;
                }

                if (!double.TryParse(minSquareInpaintNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double minInpaint) || minInpaint <= 0)
                {
                    ErrorLogger.Log(new Exception("Параметр 'MinAreaInpaintDefect' некорректен"), "ValidateDefectSettings");
                    return false;
                }

                if (!double.TryParse(whiteThresoldNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double whiteThreshold) || whiteThreshold <= 0)
                {
                    ErrorLogger.Log(new Exception("Параметр 'MinInpaintWhiteThreshold' некорректен"), "ValidateDefectSettings");
                    return false;
                }

                if (!double.TryParse(obloyPixCountNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double obloy) || obloy <= 0)
                {
                    ErrorLogger.Log(new Exception("Параметр 'MinAreaObloy' некорректен"), "ValidateDefectSettings");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "ValidateDefectSettings - непредвиденная ошибка");
                return false;
            }
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

        private void receptCapsCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = receptCapsCmB.SelectedItem?.ToString();
            if (key == null || !_recipes.ContainsKey(key))
                return;

            ApplyRecipe(_recipes[key]);
        }

        private void ApplyRecipe(CapRecipe r)
        {
            _isApplyingRecipe = true;

            // ---------- Цветовые флаги ----------
            isGreenColor = r.IsGreen;
            isColored = r.IsColored;
            isYellowCap = r.IsYellow;
            capsAreWhite = r.IsWhite;

            // ---------- Параметры обработки ----------
            window = r.Window;
            morph_size = r.MorphSize;
            morph_size_2 = r.MorphSize2;
            capsColor = r.CapsColor;
            saturation = r.CameraSaturation;

            // ---------- Камера ----------
            if (cam != null)
            {
                cam.Saturation = (uint)r.CameraSaturation;
                cam.SetSaturation();
            }

            // ---------- inpaint ----------
            if (capsAreWhite)
            {
                inpaintCB.Checked = false;
                inpaintCB.Enabled = false;
            }
            else
            {
                inpaintCB.Enabled = true;
            }

            // ---------- Чекбоксы UI (ВАЖЕН ПОРЯДОК) ----------
            isWhiteCb.Checked = capsAreWhite;
            isColorCb.Checked = isColored;
            isGreenCb.Checked = isGreenColor;
            isYellowCb.Checked = isYellowCap;

            // Один раз применяем логику UI
            UpdateColorModeUI();

            _isApplyingRecipe = false;

            // ---------- Numeric / Combo ----------
            capcolorUpDown.Value = r.CapsColor;
            saturationUpDown.Value = r.CameraSaturation;

            if (windowCb.Items.Contains(r.Window.ToString()))
                windowCb.SelectedItem = r.Window.ToString();

            if (morphCb.Items.Contains(r.MorphSize.ToString()))
                morphCb.SelectedItem = r.MorphSize.ToString();

            receptNameTb.Text = r.Name;
            frameSaturationNumUpD.Text = r.CameraSaturation.ToString();

            RebuildMorphology();
        }



        private void outputImageCmB_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (outputImageCmB.SelectedIndex)
            {
                case 0:
                    _outputMode = OutputMode.All;   // Все
                    break;
                case 1:
                    _outputMode = OutputMode.Good;  // Хорошие
                    break;
                case 2:
                    _outputMode = OutputMode.Bad;   // Плохие
                    break;
                default:
                    _outputMode = OutputMode.All;
                    break;
            }
        }

        #endregion

        #endregion

        #region Методы обработки изображений

        private bool CheckOvality(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] largestContourOvality)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                // Если контур пустой или null, сразу возвращаем false
                if (largestContourOvality == null || largestContourOvality.Length < 5)
                {
                    ErrorLogger.Log(new Exception("Контур для проверки овальности пустой или содержит недостаточно точек"),
                        "CheckOvality - проверка наличия контуров");
                    return false;
                }

                token.ThrowIfCancellationRequested();

                RotatedRect ellipse = Cv2.FitEllipse(largestContourOvality);
                majorAxis = Math.Max(ellipse.Size.Width, ellipse.Size.Height);
                minorAxis = Math.Min(ellipse.Size.Width, ellipse.Size.Height);
                axisRatio = minorAxis / majorAxis;

                token.ThrowIfCancellationRequested();
                bool isOval = axisRatio < ovalityThreshold;

                try
                {
                    Scalar color = isOval ? new Scalar(0, 0, 255) : new Scalar(0, 255, 0);
                    Cv2.Ellipse(drawFrame, ellipse, color, 2);
                    Cv2.PutText(drawFrame, $"Ratio: {axisRatio:F5}", new Point(10, 30),
                                   HersheyFonts.HersheySimplex, 1, color, 2);
                }
                catch (Exception drawEx)
                {
                    ErrorLogger.Log(drawEx, "CheckOvality - ошибка при рисовании эллипса или текста");
                }

                return isOval;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CheckOvality - ошибка при расчёте овальности крышки");
                return false;
            }
        }

        private bool CheckForInclusions(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] bestContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (bestContour == null || bestContour.Length < 5)
                {
                    ErrorLogger.Log(new Exception("Контур для проверки включений пустой или содержит недостаточно точек"),
                        "CheckForInclusions - проверка наличия контура");
                    return false;
                }

                token.ThrowIfCancellationRequested();

                RotatedRect ellipse;
                try
                {
                    ellipse = Cv2.FitEllipse(bestContour);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForInclusions - ошибка при расчёте эллипса");
                    return false;
                }

                Point2f ellipseCenter = ellipse.Center;
                float ellipseRadius = (float)(coefCapRadiusInclusion * (ellipse.Size.Width + ellipse.Size.Height) / 4.0);

                try
                {
                    Cv2.Circle(drawFrame, (Point)ellipseCenter, (int)ellipseRadius, new Scalar(255, 0, 0), 2);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForInclusions - ошибка при рисовании эллипса");
                }

                try
                {
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
                                    try
                                    {
                                        Rect bbox = Cv2.BoundingRect(contour);
                                        Cv2.Rectangle(drawFrame, bbox.TopLeft, bbox.BottomRight, new Scalar(0, 0, 255), 2);
                                        inclusionsFound = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorLogger.Log(ex, "CheckForInclusions - ошибка при рисовании прямоугольника вокруг включения");
                                    }
                                }
                            }

                            return inclusionsFound;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForInclusions - ошибка при обработке изображения для поиска включений");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CheckForInclusions - непредвиденная ошибка");
                return false;
            }
        }
        private bool CheckForPaintDefects(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length < 5)
                {
                    ErrorLogger.Log(new Exception("Контур крышки пустой или содержит недостаточно точек"),
                        "CheckForPaintDefects - проверка контура");
                    return false;
                }

                using (Mat hsv = new Mat())
                {
                    try
                    {
                        Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "CheckForPaintDefects - ошибка при преобразовании в HSV");
                        return false;
                    }

                    token.ThrowIfCancellationRequested();

                    using (Mat capMask = Mat.Zeros(image.Size(), MatType.CV_8UC1))
                    {
                        try
                        {
                            Cv2.FillPoly(capMask, new[] { capContour }, new Scalar(255));
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.Log(ex, "CheckForPaintDefects - ошибка при создании маски крышки");
                            return false;
                        }

                        token.ThrowIfCancellationRequested();

                        using (Mat maskedHSV = new Mat())
                        {
                            try
                            {
                                Cv2.BitwiseAnd(hsv, hsv, maskedHSV, capMask);
                            }
                            catch (Exception ex)
                            {
                                ErrorLogger.Log(ex, "CheckForPaintDefects - ошибка при применении маски к HSV");
                                return false;
                            }

                            token.ThrowIfCancellationRequested();

                            Mat[] hsvChannels;
                            try
                            {
                                Cv2.Split(maskedHSV, out hsvChannels);
                            }
                            catch (Exception ex)
                            {
                                ErrorLogger.Log(ex, "CheckForPaintDefects - ошибка при разделении HSV каналов");
                                return false;
                            }

                            using (Mat sChannel = hsvChannels[1])
                            using (Mat vChannel = hsvChannels[2])
                            using (Mat whiteMask = new Mat())
                            {
                                try
                                {
                                    Cv2.InRange(hsv, new Scalar(0, 255 * 0.05, 255 * 0.05),
                                                  new Scalar(180, 255 * 0.95, 255 * 0.95), whiteMask);
                                }
                                catch (Exception ex)
                                {
                                    ErrorLogger.Log(ex, "CheckForPaintDefects - ошибка при создании белой маски");
                                    return false;
                                }

                                token.ThrowIfCancellationRequested();

                                using (Mat defectsMask = new Mat())
                                {
                                    try
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
                                                try
                                                {
                                                    using (Mat contourMask = Mat.Zeros(image.Size(), MatType.CV_8UC1))
                                                    {
                                                        Cv2.FillPoly(contourMask, new[] { contour }, new Scalar(255));
                                                        using (Mat maskedImage = new Mat())
                                                        {
                                                            Cv2.BitwiseAnd(image, image, maskedImage, contourMask);
                                                            Scalar meanColor = Cv2.Mean(maskedImage, contourMask);

                                                            if (Math.Abs(meanColor.Val2 - 255) < minInpaintWhiteThreshold)
                                                            {
                                                                whiteDefects.Add(contour);
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    ErrorLogger.Log(ex, "CheckForPaintDefects - ошибка при обработке контура дефекта");
                                                }
                                            }

                                            if (whiteDefects.Count > 0)
                                            {
                                                try
                                                {
                                                    Cv2.DrawContours(drawFrame, whiteDefects, -1, new Scalar(0, 0, 255), 2);
                                                    foreach (var contour in whiteDefects)
                                                    {
                                                        Rect boundingBox = Cv2.BoundingRect(contour);
                                                        Cv2.Rectangle(drawFrame, boundingBox, new Scalar(0, 255, 255), 2);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    ErrorLogger.Log(ex, "CheckForPaintDefects - ошибка при рисовании контуров дефектов");
                                                }
                                            }

                                            return whiteDefects.Count > 0;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorLogger.Log(ex, "CheckForPaintDefects - ошибка при создании маски дефектов");
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CheckForPaintDefects - непредвиденная ошибка");
                return false;
            }
        }

        private bool CheckForObloyDefects(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length < 5)
                {
                    ErrorLogger.Log(new Exception("Контур крышки пустой или содержит слишком мало точек"),
                        "CheckForObloyDefects - проверка контура");
                    return false;
                }

                if (blurChannel_1 == null || blurChannel_2 == null)
                {
                    ErrorLogger.Log(new Exception("Каналы blurChannel_1 или blurChannel_2 не инициализированы"),
                        "CheckForObloyDefects - проверка инициализации каналов");
                    return false;
                }

                // Вычисляем центр крышки
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

                try
                {
                    if (CapRadiusMask == null || CapRadiusMask.Size() != image.Size())
                    {
                        CapRadiusMask?.Dispose();
                        CapRadiusMask = new Mat(image.Rows, image.Cols, MatType.CV_8UC1);
                    }

                    GetIdealCapMask(CapRadiusMask, capCenter,
                                    (float)(radius + 3.0f),
                                    (float)(radius + 3.0f + CAP_FLASH_OFFSET));
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForObloyDefects - ошибка при создании маски крышки");
                    return false;
                }

                try
                {
                    Cv2.BitwiseAnd(CapRadiusMask, blurChannel_2, blurChannel_2);
                    Cv2.MorphologyEx(blurChannel_2, blurChannel_1, MorphTypes.Erode, elementMask);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForObloyDefects - ошибка при применении морфологии");
                    return false;
                }

                Point[][] obloyContours;
                HierarchyIndex[] hierarchyObloy;
                try
                {
                    Cv2.FindContours(blurChannel_1, out obloyContours, out hierarchyObloy,
                                     RetrievalModes.External, ContourApproximationModes.ApproxNone);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForObloyDefects - ошибка при поиске контуров");
                    return false;
                }

                int pixCount;
                try
                {
                    pixCount = Cv2.CountNonZero(blurChannel_1);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForObloyDefects - ошибка при подсчете ненулевых пикселей");
                    return false;
                }

                if (pixCount > minAreaObloy)
                {
                    try
                    {
                        Cv2.DrawContours(drawFrame, obloyContours, -1, new Scalar(0, 0, 255), 2);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "CheckForObloyDefects - ошибка при рисовании контуров дефектов");
                    }
                }

                return pixCount > minAreaObloy;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CheckForObloyDefects - непредвиденная ошибка");
                return false;
            }
        }
        #endregion

        #region Вспомогательные методы обработки

        /*private Point[] GetCapContour(Mat gray, Mat image)
        {
            if (gray.Empty() || image.Empty())
                return null;

            //Цветокоррекция (Начало)
            Mat processed = image.Clone();
            processed = SimulateCameraSaturation(processed, saturation);
            NonlinearBackgroundDecolorization(processed, capsColor);
            //Цветокоррекция (Конец)

            Mat[] channels;
            Cv2.Split(processed, out channels);

            //Окно фильтра (Начало)
            Cv2.GaussianBlur(channels[0], channels[1], new Size(window, window), 4);
            //Окно фильтра (Конец)

            //Морфологический фильтр (Начало)
            Cv2.Threshold(channels[1], channels[0], 128, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);
            Cv2.MorphologyEx(channels[0], channels[1], MorphTypes.Dilate, element1);
            Cv2.MorphologyEx(channels[1], channels[2], MorphTypes.Erode, element2);
            //Морфологический фильтр (Конец)

            blurChannel_0 = channels[0];
            blurChannel_1 = channels[1];
            blurChannel_2 = channels[2];

            //Контур (начало)
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
            //Контур (Конец)

            return contours.Length > 0 ? contours[maxInd] : null;
        }*/

        private Point[] GetCapContour(Mat gray, Mat image)
        {
            if (gray.Empty() || image.Empty())
                return null;

            // ===== Цветокоррекция =====
            Mat processed = image.Clone();
            processed = SimulateCameraSaturation(processed, saturation);
            NonlinearBackgroundDecolorization(processed, capsColor);

            Mat[] channels;
            Cv2.Split(processed, out channels);

            // ===== Blur =====
            Cv2.GaussianBlur(channels[0], channels[1], new Size(window, window), 4);

            // ===== Threshold + Morphology =====
            Cv2.Threshold(channels[1], channels[0], 128, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);

            Cv2.MorphologyEx(channels[0], channels[1], MorphTypes.Dilate, element1);
            Cv2.MorphologyEx(channels[1], channels[2], MorphTypes.Erode, element2);

            // лёгкий антишум перед контурами
            Cv2.MedianBlur(channels[2], channels[2], 5);

            blurChannel_0 = channels[0];
            blurChannel_1 = channels[1];
            blurChannel_2 = channels[2];

            // ===== Поиск контуров =====
            Point[][] contours;
            HierarchyIndex[] hierarchy;

            Cv2.FindContours(
                channels[2],
                out contours,
                out hierarchy,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple
            );

            if (contours.Length == 0)
                return null;

            // ===== Берём самый большой =====
            int maxInd = 0;
            double maxArea = 0;

            for (int i = 0; i < contours.Length; i++)
            {
                double area = Cv2.ContourArea(contours[i]);
                if (area > maxArea)
                {
                    maxArea = area;
                    maxInd = i;
                }
            }

            var contour = contours[maxInd];

            // защита: FitEllipse требует минимум 5 точек
            if (contour.Length < 5)
                return contour;

            // ===== Fit ellipse =====
            RotatedRect ellipse = Cv2.FitEllipse(contour);

            Point2f center = ellipse.Center;

            float a = ellipse.Size.Width / 2f;
            float b = ellipse.Size.Height / 2f;

            List<Point> fixedContour = new(contour.Length); HttpStyleUriParser://cpskj.oss-cn-shanghai.aliyuncs.com/CPS-Digital.zip


            foreach (var p in contour)
            {
                float dx = p.X - center.X;
                float dy = p.Y - center.Y;

                float norm = (dx * dx) / (a * a) + (dy * dy) / (b * b);

                // выброс → проекция обратно на эллипс
                if (norm > outlierThreshold)
                {
                    float scale = 1.0f / (float)Math.Sqrt(norm);

                    int nx = (int)(center.X + dx * scale);
                    int ny = (int)(center.Y + dy * scale);

                    fixedContour.Add(new Point(nx, ny));
                }
                else
                {
                    fixedContour.Add(p);
                }
            }

            return fixedContour.ToArray();
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
            try
            {
                cts?.Cancel();

                recognizeButton.Text = "Остановка...";
                recognizeButton.Enabled = false;

                // Если есть подключение, отправляем стоп-сигнал
                if (modbusClient != null && modbusClient.Connected)
                {
                    try
                    {
                        modbusClient.WriteRegister(startRecognizeProcessing, 0);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "Ошибка в StopProcessingAsync при отправке стоп-сигнала в ПР");
                    }
                }
                else
                {
                    // Централизованное обновление интерфейса при отсутствии соединения
                    ModbusClient_ConnectionStatusChanged(false);
                }

                currentFrameNumber = 0;

                try
                {
                    if (processingTask != null)
                        await processingTask;
                }
                catch (OperationCanceledException)
                {
                    // Норма, ничего не делаем
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "Ошибка в StopProcessingAsync во время остановки обработки");
                }
            }
            finally
            {
                isProcessing = false;

                recognizeButton.Text = "Начать анализ";
                recognizeButton.BackColor = Color.FromArgb(4, 85, 191);
                recognizeButton.Enabled = true;

                if (!isImageLoaded)
                {
                    startStreamButton.Enabled = true;
                }
                if (isImageLoaded)
                {
                    loadImageButton.Enabled = true;
                }

                cts?.Dispose();
                cts = null;
            }
        }


        private void StartProcessing()
        {
            try
            {
                ApplyRecognitionParameters();

                if (modbusClient == null || !modbusClient.Connected)
                {
                    ModbusClient_ConnectionStatusChanged(false);
                }
                else
                {
                    if (!isImageLoaded)
                    {
                        // Отправляем сигнал на ПР
                        modbusClient.WriteRegister(startRecognizeProcessing, 1);
                  
                    }
                }

                cts = new CancellationTokenSource();
                processingTask = Task.Run(() => StartContinuousProcessing(cts.Token));
                isProcessing = true;

                recognizeButton.Text = "Остановить анализ";
                recognizeButton.BackColor = Color.FromArgb(229, 115, 115);

                startStreamButton.Enabled = false;
                if (isImageLoaded)
                {
                    loadImageButton.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в StartProcessing");

                cts?.Dispose();
                cts = null;
            }
        }



        private void SetUiDuringRecognition(bool isLocked)
        {
            try
            {
                applySettingsButton.Enabled = !isLocked;
                loadSettingsButton.Enabled = !isLocked;
                saveSettingsButton.Enabled = !isLocked;
                loadDefectSettings.Enabled = !isLocked;
                saveDefectSettings.Enabled = !isLocked;
                applyPrBreakerParamButton.Enabled = !isLocked;
                loadPrSettings.Enabled = !isLocked;
                savePrSettings.Enabled = !isLocked;
                receptCapsCmB.Enabled = !isLocked;

                if (!isImageLoaded)
                {
                    startStreamButton.Enabled = !isLocked;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в SetUiDuringRecognition");
            }
        }


        private void ApplyCameraSettings()
        {
            try
            {
                if (cam == null)
                {
                    MessageBox.Show("Камера не инициализирована.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ---------- 1. Валидация параметров ----------
                if (!uint.TryParse(frameWidthNumUpD.Text, out uint width) || width < 100)
                {
                    MessageBox.Show("Некорректная ширина кадра.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!uint.TryParse(frameHeightNumUpD.Text, out uint height) || height < 100)
                {
                    MessageBox.Show("Некорректная высота кадра.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!uint.TryParse(frameExposureNumUpD.Text, out uint exposure))
                {
                    MessageBox.Show("Некорректная выдержка.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!uint.TryParse(frameSaturationNumUpD.Text, out uint saturation))
                {
                    MessageBox.Show("Некорректная насыщенность.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ---------- 2. Останавливаем поток только если идёт стрим ----------
                if (isStreamCam == true)
                    StopStream();

                // ---------- 3. Применяем настройки камеры ----------
                cam.Width = width;
                cam.Height = height;
                cam.ExposureTime = exposure;
                cam.Saturation = saturation;

                cam.SetWidth();
                cam.SetHeight();
                cam.SetExposureTime();
                cam.SetSaturation();

                // ---------- 4. Возобновляем стрим ----------
                if (isStreamCam == false)
                    StartStream();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении настроек камеры:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(ex, "Ошибка при применении настроек камеры в ApplyCameraSettings");
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
                        modbusClient.WriteRegister(startRecognizeProcessing, (ushort)RecognizeProcessingAndBreakerAllowFinish);
                        modbusClient.WriteRegister(breakerAllowRegister, (ushort)RecognizeProcessingAndBreakerAllowFinish);
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

        private void ApplyRecognitionParameters()
        {
            try
            {
                if (!double.TryParse(ovalityCoefNumUpD.Text.Replace(',', '.'),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out ovalityThreshold))
                {
                    ovalityThreshold = 0.7;
                    ovalityCoefNumUpD.Text = ovalityThreshold.ToString();
                }

                if (!double.TryParse(circleCoefNumUpD.Text.Replace(',', '.'),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out inclusionThreshold))
                {
                    inclusionThreshold = 0.5;
                    circleCoefNumUpD.Text = inclusionThreshold.ToString(CultureInfo.InvariantCulture);
                }

                if (!double.TryParse(coefCapRadiusInclusionUpD.Text.Replace(',', '.'),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out coefCapRadiusInclusion))
                {
                    coefCapRadiusInclusion = 0.7;
                    coefCapRadiusInclusionUpD.Text = coefCapRadiusInclusion.ToString(CultureInfo.InvariantCulture);
                }

                if (!double.TryParse(minSquareInclusionNumUpD.Text.Replace(',', '.'),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out minAreaInclusion))
                {
                    minAreaInclusion = 50;
                    minSquareInclusionNumUpD.Text = minAreaInclusion.ToString(CultureInfo.InvariantCulture);
                }

                if (!double.TryParse(maxSquareInclusionNumUpD.Text.Replace(',', '.'),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out maxAreaInclusion))
                {
                    maxAreaInclusion = 500.0;
                    maxSquareInclusionNumUpD.Text = maxAreaInclusion.ToString(CultureInfo.InvariantCulture);
                }

                if (!double.TryParse(minSquareInpaintNumUpD.Text.Replace(',', '.'),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out minAreaInpaintDefect))
                {
                    minAreaInpaintDefect = 500;
                    minSquareInpaintNumUpD.Text = minAreaInpaintDefect.ToString(CultureInfo.InvariantCulture);
                }

                if (!double.TryParse(whiteThresoldNumUpD.Text.Replace(',', '.'),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out minInpaintWhiteThreshold))
                {
                    minInpaintWhiteThreshold = 150.0;
                    whiteThresoldNumUpD.Text = minInpaintWhiteThreshold.ToString(CultureInfo.InvariantCulture);
                }

                if (!double.TryParse(obloyPixCountNumUpD.Text.Replace(',', '.'),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out minAreaObloy))
                {
                    minAreaObloy = 1000;
                    obloyPixCountNumUpD.Text = minAreaObloy.ToString(CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в ApplyRecognitionParameters");
            }
        }


        private void ovalityCoefNumUpD_ValueChanged(object sender, EventArgs e)
        {
            ovalityThreshold = (double)ovalityCoefNumUpD.Value;
        }

        private void circleCoefNumUpD_ValueChanged(object sender, EventArgs e)
        {
            inclusionThreshold = (double)circleCoefNumUpD.Value;
        }

        private void coefCapRadiusInclusionUpD_ValueChanged(object sender, EventArgs e)
        {
            coefCapRadiusInclusion = (double)coefCapRadiusInclusionUpD.Value;
        }

        private void minSquareInclusionNumUpD_ValueChanged(object sender, EventArgs e)
        {
            minAreaInclusion = (double)minSquareInclusionNumUpD.Value;
        }

        private void maxSquareInclusionNumUpD_ValueChanged(object sender, EventArgs e)
        {
            maxAreaInclusion = (double)maxSquareInclusionNumUpD.Value;
        }

        private void minSquareInpaintNumUpD_ValueChanged(object sender, EventArgs e)
        {
            minAreaInpaintDefect = (double)minSquareInpaintNumUpD.Value;
        }

        private void whiteThresoldNumUpD_ValueChanged(object sender, EventArgs e)
        {
            minInpaintWhiteThreshold = (double)whiteThresoldNumUpD.Value;
        }

        private void obloyPixCountNumUpD_ValueChanged(object sender, EventArgs e)
        {
            minAreaObloy = (double)obloyPixCountNumUpD.Value;
        }

        private void SaveSettings()
        {
            try
            {
                // Простая валидация
                if (string.IsNullOrWhiteSpace(frameWidthNumUpD.Text) ||
                    string.IsNullOrWhiteSpace(frameHeightNumUpD.Text) ||
                    string.IsNullOrWhiteSpace(frameExposureNumUpD.Text) ||
                    string.IsNullOrWhiteSpace(frameSaturationNumUpD.Text))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля перед сохранением.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var settings = new
                {
                    Width = frameWidthNumUpD.Text,
                    Height = frameHeightNumUpD.Text,
                    Exposure = frameExposureNumUpD.Text,
                    Saturation = frameSaturationNumUpD.Text
                };

                string json = System.Text.Json.JsonSerializer.Serialize(settings,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    saveFileDialog.Title = "Сохранить настройки";
                    saveFileDialog.FileName = "settings.json";
                    saveFileDialog.InitialDirectory = folderParamCamera;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveFileDialog.FileName, json);

                        // ✅ Сохраняем также в Settings
                        Properties.Settings.Default.WidthFrame = frameWidthNumUpD.Text;
                        Properties.Settings.Default.HeightFrame = frameHeightNumUpD.Text;
                        Properties.Settings.Default.ExposureFrame = frameExposureNumUpD.Text;
                        Properties.Settings.Default.SaturationFrame = frameSaturationNumUpD.Text;
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
                ErrorLogger.Log(ex, "Ошибка при сохранении настроек камеры в SaveSettings");
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
                    openFileDialog.InitialDirectory = folderParamCamera;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string json = File.ReadAllText(openFileDialog.FileName);
                        var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        if (settings != null)
                        {
                            frameWidthNumUpD.Text = settings.ContainsKey("Width") ? settings["Width"] : "496";
                            frameHeightNumUpD.Text = settings.ContainsKey("Height") ? settings["Height"] : "532";
                            frameExposureNumUpD.Text = settings.ContainsKey("Exposure") ? settings["Exposure"] : "450";
                            frameSaturationNumUpD.Text = settings.ContainsKey("Saturation") ? settings["Saturation"] : "128";

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
                ErrorLogger.Log(ex, "Ошибка при загрузке настроек камеры в LoadSettings");
            }
        }


        #endregion

        #region Методы получения данных от оборудования

        public void GetImage(Mat img)
        {
            try
            {
                if (isStreamCam)
                {

            #if OLD_FRAME_PROCESSING
            try
            {
                lock (frameLock)
                {
                    latestFrame?.Dispose();
                    latestFrame = img.Clone();
                    newFrameAvailable = true;
                    if (isProcessing == true)
                    {
                        currentFrameNumber++;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Error in OLD_FRAME_PROCESSING block");
            }
            #else
                    try
                    {
                        if (isProcessing)
                        {
                            _imageQueue.Put(img.Clone());
                            currentFrameNumber++;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "Ошибка в GetImage, в _imageQueue.Put");
                    }
#endif

                    try
                    {
                        // вывод в PictureBox
                        if (!isProcessing)
                        {
                            if (!LocalSettings.Instance.UseVConcat)
                            {
                                img1 = img.Clone();
                                if (isRoiProduce && isROISelected)
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
                                    if (isRoiProduce && isROISelected)
                                    {
                                        img1 = new Mat(img, roi);
                                    }
                                    UpdatePictureBox(originPb, img1);
                                    isFirstImageCam1 = true;
                                }
                                else
                                {
                                    Cv2.VConcat(img1, img.Clone(), img1);
                                    if (isRoiProduce && isROISelected)
                                    {
                                        img1 = new Mat(img, roi);
                                    }
                                    UpdatePictureBox(originPb, img1);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "Ошибка в GetImage, в выводе изображения в originPb");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в GetImage, общая");
            }
        }


        #endregion

        #region Методы управления потоком обработки

        private void StartStop(bool isStart, bool isInit = false)
        {
            try
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
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в StartStop");
            }
        }

        #region Избежание дубликатов кадров
        private bool IsDuplicateFrameByHash(Mat frame)
        {
            try
            {
                ulong currentHash = ComputeAverageHash(frame);

                if (_hasLastHash && currentHash == _lastFrameHash)
                {
                    return true; // ДУБЛЬ
                }

                _lastFrameHash = currentHash;
                _hasLastHash = true;
                return false; // УНИКАЛЬНЫЙ КАДР
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка вычисления hash изображения");
                return false; // лучше обработать кадр, чем потерять
            }
        }

        private static ulong ComputeAverageHash(Mat src)
        {
            using var gray = new Mat();
            using var small = new Mat();

            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.Resize(gray, small, new Size(8, 8));

            double avg = Cv2.Mean(small).Val0;

            ulong hash = 0;
            int bit = 0;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++, bit++)
                {
                    if (small.At<byte>(y, x) > avg)
                        hash |= 1UL << bit;
                }
            }

            return hash;
        }

        private bool IsDuplicateFrameByRows(Mat frame)
        {
            try
            {
                using var gray = new Mat();
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

                int width = gray.Cols;
                int height = gray.Rows;

                int[] rowsY =
                {
                    height / 2 - 10,
                    height / 2,
                    height / 2 + 10
                };

                var currentRows = new byte[rowsY.Length][];

                for (int i = 0; i < rowsY.Length; i++)
                {
                    int y = rowsY[i];
                    if (y < 0 || y >= height)
                        return false;

                    currentRows[i] = new byte[width];
                    Marshal.Copy(gray.Ptr(y), currentRows[i], 0, width);
                }

                // ===== сравнение =====
                if (_hasLastRows)
                {
                    bool isDuplicate = true;

                    for (int i = 0; i < currentRows.Length; i++)
                    {
                        if (!currentRows[i].SequenceEqual(_lastRows![i]))
                        {
                            isDuplicate = false;
                            break;
                        }
                    }

                    if (isDuplicate)
                    {
                        if (_lastFrameForDuplicate != null)
                        {
                            Mat first = _lastFrameForDuplicate.Clone();
                            Mat second = frame.Clone();

                            Task.Run(() =>
                            {
                                try
                                {
                                    CycleImageSaver.SaveDuplicate(first, generalCapsCount);
                                    CycleImageSaver.SaveDuplicate(second, generalCapsCount);
                                }
                                finally
                                {
                                    first.Dispose();
                                    second.Dispose();
                                }
                            });
                        }

                        return true; // ДУБЛЬ
                    }
                }

                // ===== НЕ дубль → обновляем эталон =====
                _lastRows = currentRows;
                _hasLastRows = true;

                _lastFrameForDuplicate?.Dispose();
                _lastFrameForDuplicate = frame.Clone();

                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка сравнения изображения по строкам");
                return false;
            }
        }
        #endregion

        private async void StartContinuousProcessing(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Mat? frameToProcess = null;

                    try
                    {
                        // ===== Получение кадра =====
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
                                currentImageIndex = (currentImageIndex + 1) % imageFiles.Count;
                            }
                            catch (Exception ex)
                            {
                                ErrorLogger.Log(ex, "Ошибка загрузки изображения из папки");
                                continue;
                            }
                        }

                        if (frameToProcess == null || frameToProcess.Empty())
                            continue;

                        using (frameToProcess)
                        using (Mat gray = new Mat())
                        {
                            Stopwatch stopwatch = Stopwatch.StartNew();

                            /*if (IsDuplicateFrameByHash(frameToProcess))
                                continue;*/

                            if (IsDuplicateFrameByRows(frameToProcess))
                                continue;

                            try
                            {
                                // === PRE-PROCESS ===
                                Cv2.CvtColor(frameToProcess, gray, ColorConversionCodes.BGR2GRAY);
                                Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

                                frameToProcess.CopyTo(_frameToDisplay);

                                Point[] capContour = GetCapContour(gray, frameToProcess);

                                generalCapsCount++;
                                UpdateTextBox(generalCapsCountTb, generalCapsCount, 0);

                                if (capContour != null && capContour.Length > 0)
                                {
                                    Cv2.DrawContours(_frameToDisplay, new[] { capContour }, -1, new Scalar(255, 0, 0), 2);
                                }

                                // === Подготовка изображений по категориям ===
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

                                // === Запуск проверок ===
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
                                            ovalityTask = RunCheckWithTimeout(_grayForOvality, _imageForOvality, _frameToDisplay, timeoutCts.Token, RunCheckOvality, capContour);

                                        if (inclusionCB.Checked)
                                            inclusionsTask = RunCheckWithTimeout(_grayForInclusions, _imageForInclusions, _frameToDisplay, timeoutCts.Token, RunCheckForInclusions, capContour);

                                        if (inpaintCB.Checked)
                                            paintTask = RunCheckWithTimeout(_grayForPaintDefects, _imageForPaintDefects, _frameToDisplay, timeoutCts.Token, RunCheckForPaintDefects, capContour);

                                        if (obloyCB.Checked)
                                            obloyTask = RunCheckWithTimeout(_grayForObloyDefects, _imageForObloyDefects, _frameToDisplay, timeoutCts.Token, RunCheckForObloyDefects, capContour);

                                        await Task.WhenAll(ovalityTask, inclusionsTask, paintTask, obloyTask);

                                        bool anyDefect =
                                            (ovalityCB.Checked && ovalityTask.Result) ||
                                            (inclusionCB.Checked && inclusionsTask.Result) ||
                                            (inpaintCB.Checked && paintTask.Result) ||
                                            (obloyCB.Checked && obloyTask.Result);

                                        // === счётчики ===
                                        if (anyDefect)
                                        {
                                            ngCapsCount++;
                                            percentNgCaps = generalCapsCount > 0 ? ngCapsCount / generalCapsCount * 100 : 0;
                                            UpdateTextBox(ngCapsCountTb, ngCapsCount, 0);
                                            UpdateTextBox(percentNgCapsTb, percentNgCaps);
                                        }
                                        else
                                        {
                                            okCapsCount++;
                                            percentOkCaps = generalCapsCount > 0 ? okCapsCount / generalCapsCount * 100 : 0;
                                            percentNgCaps = generalCapsCount > 0 ? ngCapsCount / generalCapsCount * 100 : 0;

                                            UpdateTextBox(okCapsCountTb, okCapsCount, 0);
                                            UpdateTextBox(percentOkCapsTb, percentOkCaps);
                                            UpdateTextBox(percentNgCapsTb, percentNgCaps);
                                        }

                                        // === Сохранение изображения ===
                                        Mat copy = frameToProcess.Clone();
                                        _ = Task.Run(() =>
                                        {
                                            try
                                            {
                                                CycleImageSaver.Save(
                                                    copy,
                                                    isNG: anyDefect,
                                                    allowOk: okCapsSaveCb.Checked,
                                                    allowNg: ngCapsSaveCb.Checked,
                                                    generalCount: generalCapsCount
                                                );
                                            }
                                            catch (Exception ex)
                                            {
                                                ErrorLogger.Log(ex, "Ошибка при сохранении изображения в CycleImageSaver");
                                            }
                                            finally { copy.Dispose(); }
                                        });

                                        BeginInvoke(() => currentFolderTb.Text = CycleImageSaver.CurrentCycleFolder);

                                        // === Передача результата в ПЛК ===
                                        PLCData.QualityStatus st = anyDefect
                                            ? PLCData.QualityStatus.Bad
                                            : PLCData.QualityStatus.Good;

                                        _ = Task.Run(() =>
                                        {
                                            try { SendQualityStatus(st); }
                                            catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка при отправке статуса качества в ПЛК"); }
                                        });

                                        bool show =
                                            _outputMode == OutputMode.All ||
                                            (_outputMode == OutputMode.Good && !anyDefect) ||
                                            (_outputMode == OutputMode.Bad && anyDefect);

                                        if (show)
                                        {
                                            BeginInvoke(() =>
                                            {
                                                UpdatePictureBox(originPb, _frameToDisplay);

                                                try
                                                {
                                                    _imageOriginReceptParam?.Dispose();
                                                    _imageOriginReceptParam = frameToProcess.Clone();
                                                    _imageForTest?.Dispose();
                                                    _imageForTest = frameToProcess.Clone();

                                                    UpdatePictureBox(generalReceptParamPb, _imageOriginReceptParam);
                                                    UpdatePictureBox(originReceptParamSmallPb, _imageOriginReceptParam);
                                                    UpdatePictureBox(testingPb, _imageForTest);
                                                }
                                                catch (Exception ex)
                                                {
                                                    ErrorLogger.Log(ex, "Ошибка при обновлении рецептурных изображений");
                                                }
                                            });
                                        }


                                        stopwatch.Stop();
                                        UpdateTextBox(generalTimeTb, stopwatch.ElapsedMilliseconds, 0);
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        // Норма
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorLogger.Log(ex, "Ошибка внутри обработки одного кадра");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ErrorLogger.Log(ex, "Ошибка обработки кадра перед проверками");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "Ошибка цикла StartContinuousProcessing");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                ErrorLogger.Log(new Exception("StartContinuousProcessing отменён"), "TaskCanceledException");
            }
            catch (OperationCanceledException)
            {
                ErrorLogger.Log(new Exception("StartContinuousProcessing прерван"), "OperationCanceledException");
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Фатальная ошибка в StartContinuousProcessing");
            }
        }

        private async Task<bool> RunCheckWithTimeout(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Func<Mat, Mat, Mat, CancellationToken, Point[], bool> checkFunc, Point[] capContour)
        {
            try
            {
                return await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    return checkFunc(gray, image, drawFrame, token, capContour);
                }, token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в RunCheckWithTimeout при запуске проверки дефекта");
                return false;
            }
        }

        #endregion

        #region Методы проверки дефектов

        private bool RunCheckOvality(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                Stopwatch stopwatch = Stopwatch.StartNew();

                bool isOval = CheckOvality(gray, image, drawFrame, token, capContour);

                if (isOval)
                {
                    ovalityCount++;
                    percentOvalityCaps = generalCapsCount > 0 ? ovalityCount / generalCapsCount * 100 : 0;
                    UpdateTextBox(ovalityDef, ovalityCount);
                    UpdateTextBox(percentOvalityCapsTb, percentOvalityCaps);
                }

                stopwatch.Stop();
                percentOvalityCaps = generalCapsCount > 0 ? ovalityCount / generalCapsCount * 100 : 0;
                UpdateTextBox(timeOvality, stopwatch.ElapsedMilliseconds, 0);
                UpdateTextBox(percentOvalityCapsTb, percentOvalityCaps);

                return isOval;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в RunCheckOvality");
                return false;
            }
        }

        private bool RunCheckForInclusions(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                Stopwatch stopwatch = Stopwatch.StartNew();

                bool hasInclusions = CheckForInclusions(gray, image, drawFrame, token, capContour);

                if (hasInclusions)
                {
                    inclusionCount++;
                    percentInclusionCaps = generalCapsCount > 0 ? inclusionCount / generalCapsCount * 100 : 0;
                    UpdateTextBox(inclusionDef, inclusionCount);
                    UpdateTextBox(percentInclusionCapsTb, percentInclusionCaps);
                }

                stopwatch.Stop();
                percentInclusionCaps = generalCapsCount > 0 ? inclusionCount / generalCapsCount * 100 : 0;
                UpdateTextBox(inclusionTime, stopwatch.ElapsedMilliseconds, 0);
                UpdateTextBox(percentInclusionCapsTb, percentInclusionCaps);

                return hasInclusions;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в RunCheckForInclusions");
                return false;
            }
        }

        private bool RunCheckForPaintDefects(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                Stopwatch stopwatch = Stopwatch.StartNew();

                bool hasPaintDefects = CheckForPaintDefects(gray, image, drawFrame, token, capContour);

                if (hasPaintDefects)
                {
                    paintDefectCount++;
                    percentInpaintCaps = generalCapsCount > 0 ? paintDefectCount / generalCapsCount * 100 : 0;
                    UpdateTextBox(InpaintDef, paintDefectCount);
                    UpdateTextBox(percentInpaintCapsTb, percentInpaintCaps);
                }

                stopwatch.Stop();
                percentInpaintCaps = generalCapsCount > 0 ? paintDefectCount / generalCapsCount * 100 : 0;
                UpdateTextBox(inpaintTime, stopwatch.ElapsedMilliseconds, 0);
                UpdateTextBox(percentInpaintCapsTb, percentInpaintCaps);

                return hasPaintDefects;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в RunCheckForPaintDefects");
                return false;
            }
        }

        private bool RunCheckForObloyDefects(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                Stopwatch stopwatch = Stopwatch.StartNew();

                bool hasObloyDefects = CheckForObloyDefects(gray, image, drawFrame, token, capContour);

                if (hasObloyDefects)
                {
                    obloyDefectCount++;
                    percentObloyCaps = generalCapsCount > 0 ? obloyDefectCount / generalCapsCount * 100 : 0;
                    UpdateTextBox(obloyDef, obloyDefectCount);
                    UpdateTextBox(percentObloyCapsTb, percentObloyCaps);
                }

                stopwatch.Stop();
                percentObloyCaps = generalCapsCount > 0 ? obloyDefectCount / generalCapsCount * 100 : 0;
                UpdateTextBox(obloyTime, stopwatch.ElapsedMilliseconds, 0);
                UpdateTextBox(percentObloyCapsTb, percentObloyCaps);

                return hasObloyDefects;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в RunCheckForObloyDefects");
                return false;
            }
        }

        #endregion

        #region Методы работы с Modbus
        private void SendQualityStatus(PLCData.QualityStatus qualityStatus)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteRegister(PLCData.QualityRegisterModbus, (ushort)qualityStatus);
                }
                else
                {
                    // Централизованное обновление UI при разрыве соединения
                    ModbusClient_ConnectionStatusChanged(false);
                }
            }
            catch (TaskCanceledException)
            {
                // Норма, ничего не делаем
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"Ошибка отправки статуса качества {qualityStatus} на ПЛК");
            }
        }

        private void SendBreakingTime(int breakingTime)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteRegister(breakingTimeRegister, (ushort)breakingTime);
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"Ошибка отправки времени обдува ({breakingTime}) на ПЛК");
            }
        }

        private void SendCameraOffset(int cameraOffset)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteRegister(cameraOffsetRegister, (ushort)cameraOffset);
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"Ошибка отправки смещения камеры ({cameraOffset}) на ПЛК");
            }
        }

        private void SendBreakerOffset(int breakerOffset)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteRegister(breakerOffsetRegister, (ushort)breakerOffset);
                }
            }
            catch (TaskCanceledException)
            {
                // отмена - нормально
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"Ошибка отправки смещения отбраковщика ({breakerOffset}) на ПЛК");
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

                    modbusClient.WriteRegister(breakerAllowRegister, (ushort)valueToSend);
                }
                else
                {
                    // Централизованное обновление UI при разрыве соединения
                    ModbusClient_ConnectionStatusChanged(false);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка при отправке сигнала на ПР205");
                ModbusClient_ConnectionStatusChanged(false);
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
            try
            {
                if (pictureBox.InvokeRequired)
                {
                    pictureBox.Invoke(new Action(() =>
                    {
                        try
                        {
                            pictureBox.Image?.Dispose();
                            pictureBox.Image = BitmapConverter.ToBitmap(image);
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.Log(ex, "Ошибка в UpdatePictureBox, при обновлении {pictureBox}");
                        }
                    }));
                }
                else
                {
                    try
                    {
                        pictureBox.Image?.Dispose();
                        pictureBox.Image = BitmapConverter.ToBitmap(image);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "Ошибка в UpdatePictureBox, при прямом обновлении изображения для PictureBox: {pictureBox}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в UpdatePictureBox, (внешний уровень) для PictureBox: {pictureBox}");
            }
        }


        private void UpdateTextBox(System.Windows.Forms.TextBox textBox, float value, int decimals = 1)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() =>
                {
                    textBox.Text = value.ToString($"F{decimals}");
                }));
            }
            else
            {
                textBox.Text = value.ToString($"F{decimals}");
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
                else
                {
                    // 2️⃣ Ветка для зелёных крышек — как в C++-коде
                    for (int i = 0; i < total; i += 3)
                    {
                        data[i + 1] = (byte)Math.Abs(
                            data[i + 1] - ((data[i] + data[i + 2]) >> 1)
                        );
                    }
                }

                // 2️⃣ Если крышка цветная
                if (isColored)
                {
                    for (int i = 0; i < total; i += 3)
                    {
                        if (!isGreenColor)
                        {
                            if (isYellowCap)
                            {
                                data[i] = (byte)((3 * data[i + 2] + data[i + 1]) >> 2);
                            }
                            else
                            {
                                data[i] = (byte)Math.Abs(data[i] - ((data[i + 1] + data[i + 2]) >> 1));
                            }
                        }
                        else
                        {
                            // Для зелёных крышек — бинаризация по синему каналу
                            if (data[i + 1] < GREEN_THRESHOLD)
                            {
                                data[i] = (byte)0x00;
                            }
                            else
                            {
                                data[i] = (byte)0xFF;
                            }
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

        #region Тестирование нахождения брака
        private void loadImageTestTb_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
                ofd.Title = "Выберите изображение";
                ofd.InitialDirectory = CycleImageSaver.CurrentCycleFolder;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Загружаем Mat
                        _imageForTest = Cv2.ImRead(ofd.FileName, ImreadModes.Color);

                        if (_imageForTest.Empty())
                        {
                            ErrorLogger.Log(new Exception("Не удалось загрузить изображение"), "loadImageTestTb_Click");
                            return;
                        }

                        // Показываем в PictureBox
                        using (var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(_imageForTest))
                        {
                            testingPb.Image?.Dispose();  // чистим старое изображение
                            testingPb.Image = (Bitmap)bitmap.Clone();
                        }

                        TestDefectParams();
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "Ошибка при загрузке тестового изображения");
                    }
                }
            }
        }

        private void loadImageForTestDefectFromCameraBt_Click(object sender, EventArgs e)
        {
            if (img1 == null || img1.Empty())
            {
                MessageBox.Show("Нет изображения от камеры");
                return;
            }

            _imageForTest?.Dispose();
            _imageForTest = img1.Clone();

            testingPb.Image?.Dispose();
            testingPb.Image = BitmapConverter.ToBitmap(_imageForTest);

            TestDefectParams();
        }

        private void testDefectParamBt_Click(object sender, EventArgs e)
        {
            TestDefectParams();
        }

        public void TestDefectParams()
        {
            try
            {
                ApplyRecognitionParameters();

                if (_imageForTest == null || _imageForTest.Empty())
                {
                    ErrorLogger.Log(new Exception("Тестовое изображение не загружено"), "testDefectParamBt_Click");
                    return;
                }

                // Исходные копии
                Mat frameBase = _imageForTest.Clone();
                Mat grayBase = new Mat();

                // Базовая обработка
                Cv2.CvtColor(frameBase, grayBase, ColorConversionCodes.BGR2GRAY);
                Cv2.GaussianBlur(grayBase, grayBase, new OpenCvSharp.Size(5, 5), 0);

                // Контур крышки
                Point[] contour = GetCapContour(grayBase, frameBase);
                if (contour == null || contour.Length == 0)
                {
                    ErrorLogger.Log(new Exception("Контур крышки не найден"), "testDefectParamBt_Click");
                    frameBase.Dispose();
                    grayBase.Dispose();
                    return;
                }

                // Для отображения на экране
                using Mat finalFrame = frameBase.Clone();
                Cv2.DrawContours(finalFrame, new[] { contour }, -1, new Scalar(0, 255, 0), 2);

                // Независимые копии для каждого дефекта
                Mat frameO = null, frameI = null, frameP = null, frameOb = null;
                Mat grayO = null, grayI = null, grayP = null, grayOb = null;

                if (ovalityCB.Checked) { frameO = frameBase.Clone(); grayO = grayBase.Clone(); }
                if (inclusionCB.Checked) { frameI = frameBase.Clone(); grayI = grayBase.Clone(); }
                if (inpaintCB.Checked) { frameP = frameBase.Clone(); grayP = grayBase.Clone(); }
                if (obloyCB.Checked) { frameOb = frameBase.Clone(); grayOb = grayBase.Clone(); }

                // Запуск проверок
                CancellationToken fake = CancellationToken.None;
                bool oval = false, incl = false, paint = false, obloy = false;

                try { if (ovalityCB.Checked) oval = CheckOvality(grayO, frameO, finalFrame, fake, contour); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки овальности"); }

                try { if (inclusionCB.Checked) incl = CheckForInclusions(grayI, frameI, finalFrame, fake, contour); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки включений"); }

                try { if (inpaintCB.Checked) paint = CheckForPaintDefects(grayP, frameP, finalFrame, fake, contour); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки дефектов краски"); }

                try { if (obloyCB.Checked) obloy = CheckForObloyDefects(grayOb, frameOb, finalFrame, fake, contour); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки облоя"); }

                // Отметка на финальном изображении
                if (oval) Cv2.PutText(finalFrame, "OVALITY", new Point(20, 40), HersheyFonts.HersheySimplex, 1.0, new Scalar(0, 0, 255), 2);
                if (incl) Cv2.PutText(finalFrame, "INCLUSIONS", new Point(20, 80), HersheyFonts.HersheySimplex, 1.0, new Scalar(0, 0, 255), 2);
                if (paint) Cv2.PutText(finalFrame, "PAINT DEFECT", new Point(20, 120), HersheyFonts.HersheySimplex, 1.0, new Scalar(0, 0, 255), 2);
                if (obloy) Cv2.PutText(finalFrame, "OBLOY", new Point(20, 160), HersheyFonts.HersheySimplex, 1.0, new Scalar(0, 0, 255), 2);

                // Показ результата
                using (var bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(finalFrame))
                {
                    testingResultPb.Image?.Dispose();
                    testingResultPb.Image = (Bitmap)bmp.Clone();
                }

                // Очистка
                frameO?.Dispose(); frameI?.Dispose(); frameP?.Dispose(); frameOb?.Dispose();
                grayO?.Dispose(); grayI?.Dispose(); grayP?.Dispose(); grayOb?.Dispose();
                frameBase.Dispose(); grayBase.Dispose(); finalFrame.Dispose();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка во время тестирования параметров дефектов");
            }
        }
        #endregion

        #region Сохранение изображений и обработчики
        private void cycleUpDown_ValueChanged(object sender, EventArgs e)
        {
            int hours = (int)cycleUpDown.Value;
            CycleImageSaver.SetCycleHours(hours);
            currentFolderTb.Text = CycleImageSaver.CurrentCycleFolder;

            // Сохраняем в настройки
            Properties.Settings.Default.LastCycleTime = hours.ToString();
            Properties.Settings.Default.Save();
        }

        private void chooseBaseFolderBtn_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "Выберите папку для хранения крышек";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                CycleImageSaver.BaseFolder = dialog.SelectedPath;
                CycleImageSaver.EnsureCycleFolder();
                currentFolderTb.Text = CycleImageSaver.CurrentCycleFolder;
            }
        }

        private void openCurrentFolderBtn_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(CycleImageSaver.CurrentCycleFolder))
                System.Diagnostics.Process.Start("explorer", CycleImageSaver.CurrentCycleFolder);
        }
        #endregion

        #region Создание рецепта
        private Mat SimulateCameraSaturation(Mat img, int saturation)
        {
            if (img.Empty()) return null;

            float koeff = saturation / 128.0f;

            Mat imgHSV = new Mat();
            Cv2.CvtColor(img, imgHSV, ColorConversionCodes.BGR2HSV);

            Mat[] hsv = Cv2.Split(imgHSV);
            Mat h = hsv[0];
            Mat s = hsv[1];
            Mat v = hsv[2];

            unsafe
            {
                byte* satPtr = (byte*)s.DataPointer;
                int total = s.Rows * s.Cols;

                for (int i = 0; i < total; i++)
                {
                    float corrected = koeff * satPtr[i];
                    if (corrected > 255f) corrected = 255f;
                    satPtr[i] = (byte)corrected;
                }
            }

            Cv2.Merge(new Mat[] { h, s, v }, imgHSV);

            Mat imgSat = new Mat();
            Cv2.CvtColor(imgHSV, imgSat, ColorConversionCodes.HSV2BGR);

            return imgSat;
        }

        private void RecomputeAll()
        {
            if (_imageOriginReceptParam == null || _imageOriginReceptParam.Empty())
                return;

            // ---------------- 1. Saturation --------------------
            Mat satImg = SimulateCameraSaturation(_imageOriginReceptParam, (int)saturationUpDown.Value);
            saturationReceptParamSmallPb.Image = BitmapConverter.ToBitmap(satImg);

            // ---------------- 2. CapsColor ----------------------
            Mat capsImg = satImg.Clone();
            NonlinearBackgroundDecolorization(capsImg, (byte)capcolorUpDown.Value);
            capscolorReceptParamSmallPb.Image = BitmapConverter.ToBitmap(capsImg);

            // ---------------- 3. Window filtering ----------------
            Mat[] channels;
            Cv2.Split(capsImg, out channels);

            int window = int.Parse(windowCb.Text);
            Cv2.GaussianBlur(channels[0], channels[1], new Size(window, window), 4);
            Mat windowImg = channels[1];
            windowReceptParamSmallPb.Image = BitmapConverter.ToBitmap(windowImg);

            // ---------------- 4. Morphology ---------------------
            int morph = int.Parse(morphCb.Text);
            Mat element1 = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new Size(2 * morph + 1, 2 * morph + 1),
                new Point(morph, morph)
            );

            Mat element2 = Cv2.GetStructuringElement(
                MorphShapes.Cross,
                new Size(2 * morph + 1, 2 * morph + 1),
                new Point(morph, morph)
            );

            Cv2.Threshold(channels[1], channels[0], 128, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);
            Cv2.MorphologyEx(channels[0], channels[1], MorphTypes.Dilate, element1);
            Cv2.MorphologyEx(channels[1], channels[2], MorphTypes.Erode, element2);

            // лёгкий антишум перед контурами
            Cv2.MedianBlur(channels[2], channels[2], 5);

            Mat morphImg = channels[2];
            morphReceptParamSmallPb.Image = BitmapConverter.ToBitmap(morphImg);

            // ---------------- 5. Contour -------------------------
            Point[][] contours;
            HierarchyIndex[] hierarchy;

            Cv2.FindContours(morphImg, out contours, out hierarchy,
                RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
                return;

            int maxInd = 0;
            int maxLength = 0;
            for (int i = 0; i < contours.Length; i++)
                if (contours[i].Length > maxLength)
                {
                    maxLength = contours[i].Length;
                    maxInd = i;
                }

            // ---------------- УСТРАНЕНИЕ ПРИЦЕПКИ ФОНА ----------------
            var contour = contours[maxInd];

            if (contour.Length >= 5)
            {
                RotatedRect ellipse = Cv2.FitEllipse(contour);

                Point2f center = ellipse.Center;
                float a = ellipse.Size.Width / 2f;
                float b = ellipse.Size.Height / 2f;

                List<Point> fixedContour = new(contour.Length);

                foreach (var p in contour)
                {
                    float dx = p.X - center.X;
                    float dy = p.Y - center.Y;

                    float norm = (dx * dx) / (a * a) + (dy * dy) / (b * b);

                    if (norm > outlierThreshold)
                    {
                        float scale = 1.0f / (float)Math.Sqrt(norm);

                        int nx = (int)(center.X + dx * scale);
                        int ny = (int)(center.Y + dy * scale);

                        fixedContour.Add(new Point(nx, ny));
                    }
                    else
                    {
                        fixedContour.Add(p);
                    }
                }

                contour = fixedContour.ToArray();
            }

            // ---------------- Отрисовать контур -------------------
            Mat contourDraw = _imageOriginReceptParam.Clone();

            Cv2.DrawContours(contourDraw,
                new[] { contour },
                -1,
                Scalar.Red,
                2);

            resultContourSmallPb.Image = BitmapConverter.ToBitmap(contourDraw);

            ShowInGeneralPreview(resultContourSmallPb);

        }

        private void saturationUpDown_ValueChanged(object sender, EventArgs e)
        {
            RecomputeAll();
        }

        private void capcolorUpDown_ValueChanged(object sender, EventArgs e)
        {
            RecomputeAll();
        }

        private void windowCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            RecomputeAll();
        }

        private void morphCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            RecomputeAll();
        }


        private void loadImageForReceptParamBt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp";
                ofd.Title = "Выберите изображение";
                ofd.InitialDirectory = CycleImageSaver.CurrentCycleFolder;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _imageOriginReceptParam = new Mat(ofd.FileName);
                    originReceptParamSmallPb.Image = BitmapConverter.ToBitmap(_imageOriginReceptParam);
                    RecomputeAll();
                }
            }
        }

        private void findContourCreateReceptBt_Click(object sender, EventArgs e)
        {
            RecomputeAll();
        }

        private void originReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(originReceptParamSmallPb);
        }

        private void saturationReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(saturationReceptParamSmallPb);
        }

        private void capscolorReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(capscolorReceptParamSmallPb);
        }

        private void windowReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(windowReceptParamSmallPb);
        }

        private void morphReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(morphReceptParamSmallPb);
        }

        private void resultContourSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(resultContourSmallPb);
        }

        // Вспомогательный метод
        private void ShowInGeneralPreview(PictureBox smallPb)
        {
            if (smallPb.Image != null)
            {
                generalReceptParamPb.Image?.Dispose(); // освобождаем предыдущий Image
                generalReceptParamPb.Image = (System.Drawing.Image)smallPb.Image.Clone();
            }
        }

        private void saveReceptBt_Click(object sender, EventArgs e)
        {
            if (!ValidateRecept(out string error))
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // --- Папка ---
            string folder = RecipesFolder;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string name = receptNameTb.Text.Trim();
            string fileName = name + ".json";
            string fullPath = Path.Combine(folder, fileName);

            bool existedBefore = File.Exists(fullPath);

            // --- Window ---
            int windowValue = 0;
            if (windowCb.SelectedItem != null)
                int.TryParse(windowCb.SelectedItem.ToString(), out windowValue);

            // --- MorphSize ---
            int morphSize = 1;
            if (morphCb.SelectedItem != null)
                int.TryParse(morphCb.SelectedItem.ToString(), out morphSize);

            int morphSize2 = morphSize;

            // --- Рецепт ---
            var recipe = new CapRecipe
            {
                Name = name,
                CapsColor = (byte)capcolorUpDown.Value,
                Window = windowValue,
                MorphSize = morphSize,
                MorphSize2 = morphSize2,
                CameraSaturation = (int)saturationUpDown.Value,

                IsGreen = isGreenColor,
                IsColored = isColored,
                IsYellow = isYellowCap,
                IsWhite = capsAreWhite
            };

            // --- JSON с нормальной русской кодировкой ---
            string json = System.Text.Json.JsonSerializer.Serialize(
                recipe,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }
            );

            File.WriteAllText(fullPath, json, Encoding.UTF8);

            // --- Обновляем список рецептов ---
            LoadRecipes();

            receptCapsCmB.Items.Clear();
            foreach (var recipeName in _recipes.Keys)
                receptCapsCmB.Items.Add(recipeName);
            receptCapsCmB.SelectedItem = name;

            // --- Сообщение ---
            if (existedBefore)
                MessageBox.Show($"Рецепт \"{name}\" редактирован успешно!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show($"Рецепт \"{name}\" создан успешно!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool ValidateRecept(out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(receptNameTb.Text))
            {
                error = "Введите имя рецепта.";
                return false;
            }

            if (capcolorUpDown.Value < 0 || capcolorUpDown.Value > 255)
            {
                error = "Цв. крышки должен быть в диапазоне 0–255.";
                return false;
            }

            if (windowCb.SelectedIndex < 0)
            {
                error = "Выберите значение Ок.фильтр.";
                return false;
            }

            if (morphCb.SelectedIndex < 0)
            {
                error = "Выберите Мф.фильтр.";
                return false;
            }

            // Все проверки прошли
            return true;
        }


        private void openCurReceptFolderBt_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(RecipesFolder))
                Directory.CreateDirectory(RecipesFolder);

            System.Diagnostics.Process.Start("explorer.exe", RecipesFolder);
        }

        private void isGreenCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            isGreenColor = isGreenCb.Checked;
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void isColorCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            isColored = isColorCb.Checked;
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void isYellowCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            isYellowCap = isYellowCb.Checked;
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void isWhiteCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            capsAreWhite = isWhiteCb.Checked;
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void UpdateColorModeUI()
        {
            // --- WHITE режим ---
            if (isWhiteCb.Checked)
            {
                isWhiteCb.Enabled = true;   // ← ВОТ ЭТО КРИТИЧНО

                isColorCb.Checked = false;
                isGreenCb.Checked = false;
                isYellowCb.Checked = false;

                isColorCb.Enabled = false;
                isGreenCb.Enabled = false;
                isYellowCb.Enabled = false;

                return;
            }
            else
            {
                isColorCb.Enabled = true;
                isGreenCb.Enabled = true;
                isWhiteCb.Enabled = true;  // ← и тут тоже
            }

            // --- COLOR режим ---
            if (isColorCb.Checked)
            {
                isYellowCb.Enabled = true;
                isGreenCb.Enabled = true;
                isWhiteCb.Enabled = false;
            }
            else
            {
                isGreenCb.Checked = false;
                isYellowCb.Checked = false;

                isGreenCb.Enabled = false;
                isYellowCb.Enabled = false;
                isWhiteCb.Enabled = true;
            }

            // --- GREEN ---
            if (isGreenCb.Checked)
            {
                if (!isColorCb.Checked)
                    isColorCb.Checked = true;

                isYellowCb.Checked = false;
                isYellowCb.Enabled = false;
            }

            // --- YELLOW ---
            if (isYellowCb.Checked)
            {
                isGreenCb.Checked = false;
            }
        }

        private void loadImageForCreateReceptFromCameraBt_Click(object sender, EventArgs e)
        {
            if (img1 == null || img1.Empty())
            {
                MessageBox.Show("Нет изображения от камеры");
                return;
            }

            _imageOriginReceptParam?.Dispose();
            _imageOriginReceptParam = img1.Clone();

            originReceptParamSmallPb.Image?.Dispose();
            originReceptParamSmallPb.Image = BitmapConverter.ToBitmap(_imageOriginReceptParam);

            generalReceptParamPb.Image?.Dispose();
            generalReceptParamPb.Image = BitmapConverter.ToBitmap(_imageOriginReceptParam);

            RecomputeAll();
        }

        #endregion

        #region Авторизация

        private void changeProfileBt_Click(object sender, EventArgs e)
        {
            using (ProfileForm pf = new ProfileForm())
            {
                if (pf.ShowDialog() == DialogResult.OK)
                {
                }
            }
        }

        private void OnRoleChanged(Role newRole)
        {
            UpdateRoleUI(newRole);
            ApplyRoleRestrictions(newRole);
        }


        private void UpdateRoleUI(Role role)
        {
            if (role == Role.Admin)
            {
                profileStatusTb.Text = "Администратор";
                timeLeftTb.Text = AuthManager.Instance.GetSecondsLeft().ToString();
            }
            if (role == Role.Operator)
            {
                profileStatusTb.Text = "Оператор";
                timeLeftTb.Text = "∞";
            }
        }

        private void ApplyRoleRestrictions(Role role)
        {
            bool admin = role == Role.Admin;

            // --- Кнопки доступны ТОЛЬКО админу и ТОЛЬКО когда система не работает ---
            bool allowConnectButtons = admin && !isStreamRunning && !isProcessing;

            pr205PortTb.Enabled = admin;
            prIpTextBox.Enabled = admin;
            connectPrButton.Enabled = allowConnectButtons;

            cameraIpTextBox.Enabled = admin;
            connectCameraButton.Enabled = allowConnectButtons;

            frameHeightNumUpD.Enabled = admin;
            frameWidthNumUpD.Enabled = admin;
            frameExposureNumUpD.Enabled = admin;
            frameSaturationNumUpD.Enabled = admin;

            applySettingsButton.Enabled = admin;
            loadSettingsButton.Enabled = admin;
            saveSettingsButton.Enabled = admin;
        }


        #endregion
    }
}