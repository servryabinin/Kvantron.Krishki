//#define OLD_FRAME_PROCESSING

using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using CapDefectDetector.Authorization;
using CapDefectDetector.CameraAndModbusClasses;
using CapDefectDetector.DTO;
using CapDefectDetector.Forms;
using CapDefectDetector.FrameProcessing;
using CapDefectDetector.Hardware;
using CapDefectDetector.Logger;
using CapDefectDetector.ResultStateAndProcessingSettings;
using CapDefectDetector.StatisticProcessing;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;
using Timer = System.Windows.Forms.Timer;

namespace CapDefectDetector
{
    public partial class MainWorkForm : Form
    {
        #region Поля и константы
        // UI элементы
        private Color _connectedColor = Color.FromArgb(229, 115, 115); // красный — отключить
        private Color _disconnectedColor = Color.FromArgb(4, 85, 191); // синий — подключить
        private bool _capsAreWhite = false;
        private bool _recipeCapsAreWhite = false;
        private bool _manualDisconnect = false;
        // Выделение лейблов
        private Color _labelNormalColor = Color.Black;
        private Color _labelHoverColor = Color.FromArgb(66, 133, 244);

        private Font _labelNormalFont;
        private Font _labelHoverFont;

        // Регистры ПР205
        private HikCamera _cam;
        private ModbusTCP _modbusClient;
        private int _breakingTimeRegister = 16466;
        private int _cameraOffsetRegister = 16402;
        private int _breakerOffsetRegister = 16404;
        private int _breakerAllowRegister = 16401;
        private int _startRecognizeProcessingRegister = 16400;
        //Значения на регистрах ПР205: время отбраковки, расстояние от датчика до камера, расстояние от датчика до сдува
        private int _breakingTimeValue = 55;
        private int _cameraOffsetValue = 300;
        private int _breakerOffsetValue = 2430;
        //Значения для разрешения на сдув и начало обработки
        private int _breakerAllowTrue = 1;
        private int _breakerAllowFalse = 0;
        private int _recognizeProcessingStart = 1;
        private int _recognizeProcessingFinish = 0;

        // Состояния приложения
        private bool _cameraConnected = false;
        private bool _prConnected = false;
        private bool _isFirstImageCam1 = false;
        private bool _cameraError1 = false;
        private bool _isStreamCam = false;
        private bool _isProcessing = false;
        private bool _isProcessingFromFolder = false;
        private bool _isImageLoaded = false;


        // Получение изображений с камеры
        private Mat _img1 = new Mat();
        private Bitmap _originalImage = null;

        // Таймеры и многопоточность
        private readonly object _stateLock = new object();
        private ResultState _state = new ResultState();
        private volatile ProcessingSettings _settings;
        private System.Windows.Forms.Timer _uiTimer;

        private CancellationTokenSource _processingCts;
        private CancellationTokenSource _applyPrCts;
        private Task _processingTask;

        // Блокировки и синхронизация
#if OLD_FRAME_PROCESSING
        private readonly object frameLock = new object();
        private volatile bool newFrameAvailable = false;
        private Mat latestFrame = null;
#else
        private readonly FrameBuffer _imageQueue = new();
#endif

        // Счетчики дефектов
        private int _ovalityDefectCount = 0;
        private int _inclusionDefectCount = 0;
        private int _paintDefectCount = 0;
        private int _obloyDefectCount = 0;
        private int _underFillDefectCount = 0;
        //Время работы каждого дефекта
        public float _timeOvality = 0;
        public float _timeInclusion = 0;
        public float _timeInpaint = 0;
        public float _timeObloy = 0;
        public float _timeUnderFill = 0;
        // Процент дефекта по каждому виду от общего числа
        private float _percentOvalityCaps = 0;
        private float _percentInclusionCaps = 0;
        private float _percentInpaintCaps = 0;
        private float _percentObloyCaps = 0;
        private float _percentUnderfillCaps = 0;
        // Общая статистика крышек
        private float _generalCapsCount = 0;
        private float _okCapsCount = 0;
        private float _ngCapsCount = 0;
        private float _percentOkCaps = 0;
        private float _percentNgCaps = 0;

        // Параметры обработки изображений по дефекту "Недолив"
        private double[] _sinTable;
        private double[] _cosTable;
        private readonly object _trigTablesLock = new object();
        private const int UNDERFILL_RECT_WIDTH = 1024;

        // Поля рецепта
        private byte _capsColor = 0;
        private const byte GREEN_THRESHOLD = 40;
        private static bool _isGreenColor = false;
        private static bool _isColored = true;
        private static bool _isYellowCap = false;
        private static bool _isBlackOrBrown = false;
        private static bool _recipeIsGreenColor = false;
        private static bool _recipeIsColored = true;
        private static bool _recipeIsYellowCap = false;
        private static bool _recipeIsBlackOrBrown = false;
        private static int _saturationColor = 0;
        private static int _saturationBlackOrBrown = 0;
        private static float _contourCorrectionColor = 1.15f;
        private static float _contourCorrectionBlackOrBrown = 1.15f;
        private static int _medianFilter = 0;
        private static int _cannyThreshold = 0;
        // Морфологические элементы
        private Mat element1;
        private Mat element2;
        private Mat elementMask;
        private int window = 15;
        private int morph_size = 7;
        private int morph_size_2 = 7;
        private const float OUTIER_THRESHOLD = 1.15f;

        //Для работы с файлами рецептов крышек
        private Dictionary<string, CapRecipe> _recipes = new();
        private FileSystemWatcher _recipesWatcher;
        private string _recipesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Рецепты");
        private bool _isApplyingRecipe = false;

        // Параметры дефектов
        private double _ovalityThreshold = 0.7;
        private double _minInpaintWhiteThreshold = 150.0;
        private double _minAreaInpaintDefect = 500.0;
        private double _inclusionThreshold = 0.5;
        private double _minAreaInclusion = 50.0;
        private double _maxAreaInclusion = 500.0;
        private double _coefCapRadiusInclusion = 0.7;
        private double _minAreaObloy = 1000.0;
        private double _corrugationsCountForUnderFill = 10;
        private double _coefCapRadiusUnderFill = 0.8;

        // Параметры для нахождения "Облой"
        private Mat _capRadiusMask = new Mat(532, 568, MatType.CV_8UC1);
        private Mat _blurChannel_0;
        private Mat _blurChannel_1;
        private Mat _blurChannel_2;
        private const float CAP_FLASH_OFFSET = 5.0f;

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
        private Mat _imageOriginReceptParam;
        private Mat _imageForTest;
        private Mat _imageForTestForDisplay;

        // Пути до настроек и прочие поля
        private string _folderParamDefect = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Настройки\\Настройка параметров дефектов");
        private Dictionary<string, DefectSettings> _defectSettings = new();
        private bool _isApplyingDefectSettings = false;
        private FileSystemWatcher _defectSettingsWatcher;

        private string _folderParamPr205 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Настройки\\Настройка аппаратуры\\Настройки ПР205");
        private Dictionary<string, PrSettings> _prSettings = new();
        private bool _isApplyingPrSettings = false;
        private FileSystemWatcher _prSettingsWatcher;

        private string _folderParamCamera = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Настройки\\Настройка аппаратуры\\Настройки камеры");
        private Dictionary<string, CameraSettings> _cameraSettings = new();
        private bool _isApplyingCameraSettings = false;
        private FileSystemWatcher _cameraSettingsWatcher;

        // Работа с изображениями из файла
        private ImmutableList<string> _imageFiles = [];
        private int _currentImageIndex = 0;
        private int _currentFrameNumber = 0;

        //Избежание дубликатов кадров
        private ulong _lastFrameHash = 0;
        private bool _hasLastHash = false;
        private byte[][]? _lastRows;
        private bool _hasLastRows = false;
        private Mat? _lastFrameForDuplicate;

        //Статистика
        private StatisticsManager _statisticsManager;

        //Режим вывода изображения: Все, хорошие, плохие
        private enum OutputMode
        {
            All,
            Good,
            Bad
        }
        private OutputMode _outputMode = OutputMode.All;

        private Timer _roleDisplayTimer = new Timer();

        #endregion

        #region Конструктор и инициализация

        public MainWorkForm(HikCamera camera, ModbusTCP modbus)
        {
            // Проверяем, что камера не null и подключена
            if (camera != null)
            {
                _cam = camera;
                _cameraConnected = true;
            }
            else
            {
                _cam = null;
                _cameraConnected = false;
            }

            if (modbus != null)
            {
                _modbusClient = modbus;
                _prConnected = true;

                _modbusClient.ConnectionStatusChanged += ModbusClient_ConnectionStatusChanged;
                _modbusClient.EnableAutoReconnect();
                _modbusClient.StartPolling();
            }
            else
            {
                _modbusClient = null;
                _prConnected = false;
            }

            InitializeComponent();
            InitializeReceptTabControl();
            InitializeApplication();
            SendStopSignalsToPLC();
        }

        private void InitializeReceptTabControl()
        {
            receptVisualisationTabControl.ItemSize = new System.Drawing.Size(receptVisualisationTabControl.Size.Width / receptVisualisationTabControl.TabPages.Count, receptVisualisationTabControl.ItemSize.Height);
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
                // === подключено ===
                if (cameraOffsetTb != null)
                {
                    cameraOffsetTb.Text = _cameraOffsetValue.ToString();
                    SendCameraOffset(_cameraOffsetValue);
                }

                if (breakerOffsetTb != null)
                {
                    breakerOffsetTb.Text = _breakerOffsetValue.ToString();
                    SendBreakerOffset(_breakerOffsetValue);
                }

                if (breakingTimeTb != null)
                {
                    breakingTimeTb.Text = _breakingTimeValue.ToString();
                    SendBreakingTime(_breakingTimeValue);
                }

                _manualDisconnect = false;

                prStatus.Text = "Подключено";
                prStatus.ForeColor = Color.Green;

                connectPrButton.Text = "Отключиться от ПР";
                connectPrButton.BackColor = _connectedColor;

                _prConnected = true;
                breakingAllowCb.Enabled = true;
                breakingAllowCb.Checked = false;
                applyPrBreakerParamButton.Enabled = true;


                // И сбросить анализ
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    try
                    {
                        _modbusClient.WriteRegister(_startRecognizeProcessingRegister, (ushort)_recognizeProcessingFinish);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "Ошибка при сбросе регистра после переподключения");
                    }
                }
            }
            else
            {
                // === отключено ===
                if (_manualDisconnect)
                {
                    prStatus.Text = "Откл. вручную";
                }
                else
                {
                    prStatus.Text = "Не подключено";
                }

                prStatus.ForeColor = Color.Red;
                connectPrButton.Text = "Подключиться к ПР";
                connectPrButton.BackColor = _disconnectedColor;

                _prConnected = false;
                breakingAllowCb.Enabled = false;
                applyPrBreakerParamButton.Enabled = false;

                //Если потеря связи с ПЛК — останавливаем обработку
                if (_isProcessing && !_manualDisconnect)
                {
                    _isProcessing = false;

                    _processingCts?.Cancel();

                    Task.Run(async () =>
                    {
                        try
                        {
                            if (_processingTask != null)
                                await _processingTask;
                        }
                        catch
                        {
                        }

#if OLD_FRAME_PROCESSING
                        lock (frameLock)
                        {
                            latestFrame?.Dispose();
                            latestFrame = null;
                            newFrameAvailable = false;
                        }
#else

                        _imageQueue.Clear();
#endif

                        recognizeButton.Text = "Начать анализ";
                        recognizeButton.BackColor = Color.FromArgb(4, 85, 191);
                        recognizeButton.Enabled = true;

                        if (!_isImageLoaded)
                            startStreamButton.Enabled = true;
                        if (_isImageLoaded)
                            loadImageButton.Enabled = true;

                        BeginInvoke(() =>
                        {
                            MessageBox.Show(
                                "Потеряно соединение с ПР205. Обработка кадров остановлена. При переподключении сдув будет отключен.",
                                "Ошибка соединения",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        });
                    });
                }
            }
        }

        private void InitializeApplication()
        {
            InitializeSettingsBindings();
            InitializeCoreSystems();
            InitializePR205Status();
            InitializeCameraStatus();
            InitializeCycleSystem();
            InitializeUISelections();
            InitializeAuthorizationSystem();
            InitializeStatistics();
            InitLabelHoverEffects();
        }

        private void InitializeCoreSystems()
        {
            InitializeImageMatrices();
            InitializeRecipeFolderAndRecipeFileWatcher();
            InitializeDefectSettingsFileWatcher();
            InitializePrSettingsFileWatcher();
            InitializeCameraSettingsFileWatcher();
            LoadCameraParam();
            LoadDefectParam();
            InitializeRecepts();
            InitializeDefectSettings();
            InitializePrSettings();
            LoadPrParam();
            InitializeCameraSettings();
            InitializeMorphologicalElements();
            InitializeTrigTables();

            recognizeButton.Enabled = false;

            StartStop(false, true);
            LocalSettings.Instance.Save();
        }

        private void InitializeSettingsBindings()
        {
            ovalityCB.CheckedChanged += AnySettingChanged;
            inclusionCB.CheckedChanged += AnySettingChanged;
            inpaintCB.CheckedChanged += AnySettingChanged;
            obloyCB.CheckedChanged += AnySettingChanged;
            underFillCb.CheckedChanged += AnySettingChanged;

            okCapsSaveCb.CheckedChanged += AnySettingChanged;
            ngCapsSaveCb.CheckedChanged += AnySettingChanged;
        }

        private void InitializePR205Status()
        {
            pr205IpTb.Text = Properties.Settings.Default.Settings_IpAdressPr;

            int savedPort;
            if (int.TryParse(Properties.Settings.Default.Settings_PortPr, out savedPort))
                pr205PortTb.Text = savedPort.ToString();
            else
                pr205PortTb.Text = "502";

            if (_modbusClient != null && _modbusClient.Connected)
            {
                prStatus.Text = "Подключено";
                prStatus.ForeColor = Color.Green;
                connectPrButton.Text = "Отключиться от ПР";
                connectPrButton.BackColor = _connectedColor;
            }
            else
            {
                prStatus.Text = "Не подключено";
                prStatus.ForeColor = Color.Red;
                connectPrButton.Text = "Подключиться к ПР";
                connectPrButton.BackColor = _disconnectedColor;
                breakingAllowCb.Enabled = false;
                applyPrBreakerParamButton.Enabled = false;
            }
        }

        private void InitializeCameraStatus()
        {
            if (_cam != null && _cam.Connected)
            {
                _cam.SendImage += GetImage;
                camStatus.Text = "Подключено";
                camStatus.ForeColor = Color.Green;
                connectCameraButton.Text = "Отключиться от камеры";
                connectCameraButton.BackColor = _connectedColor;
                cameraIpTextBox.Text = _cam.IpAdress;
            }
            else
            {
                camStatus.Text = "Не подключено";
                camStatus.ForeColor = Color.Red;
                connectCameraButton.Text = "Подключиться к камере";
                connectCameraButton.BackColor = _disconnectedColor;
                cameraIpTextBox.Text = "Камера не выбрана на этапе инициализации";
            }
        }

        private void InitializeCycleSystem()
        {
            int cycle = 0;
            int.TryParse(Properties.Settings.Default.Settings_LastCycleTime, out cycle);

            // защита от неправильного значения
            if (cycle < cycleUpDown.Minimum)
            {
                cycle = (int)cycleUpDown.Minimum;
                Properties.Settings.Default.Settings_LastCycleTime = cycle.ToString();
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
            if (outputImageCmB.Items.Count > 0)
                outputImageCmB.SelectedIndex = 0;
        }

        private void InitializeAuthorizationSystem()
        {
            AuthManager.Instance.RoleChanged += OnRoleChanged;

            AuthManager.Instance.SetRole(Role.Operator);
            ApplyRoleRestrictions(AuthManager.Instance.CurrentRole);
            UpdateRoleUI(AuthManager.Instance.CurrentRole);

            _roleDisplayTimer.Interval = 1000;
            _roleDisplayTimer.Tick += (s, e) =>
            {
                if (AuthManager.Instance.CurrentRole == Role.Admin)
                    timeLeftTb.Text = AuthManager.Instance.GetSecondsLeft().ToString();
                else
                    timeLeftTb.Text = "∞";
            };

            _roleDisplayTimer.Start();
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

        private void InitializeTrigTables()
        {
            lock (_trigTablesLock)
            {
                _sinTable = new double[UNDERFILL_RECT_WIDTH];
                _cosTable = new double[UNDERFILL_RECT_WIDTH];

                double dTheta = 2 * Math.PI / UNDERFILL_RECT_WIDTH;
                for (int i = 0; i < UNDERFILL_RECT_WIDTH; i++)
                {
                    _sinTable[i] = Math.Sin(i * dTheta);
                    _cosTable[i] = Math.Cos(i * dTheta);
                }
            }
        }

        private void InitializeDefectSettingsFileWatcher()
        {
            if (!Directory.Exists(_folderParamDefect))
                Directory.CreateDirectory(_folderParamDefect);

            // Настройка FileSystemWatcher
            _defectSettingsWatcher = new FileSystemWatcher
            {
                Path = _folderParamDefect,
                Filter = "*.json",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            _defectSettingsWatcher.Created += OnDefectSettingsFolderChanged;
            _defectSettingsWatcher.Deleted += OnDefectSettingsFolderChanged;
            _defectSettingsWatcher.Renamed += OnDefectSettingsFolderChanged;

            _defectSettingsWatcher.EnableRaisingEvents = true;
        }

        private void OnDefectSettingsFolderChanged(object sender, FileSystemEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ReloadDefectSettings()));
            }
            else
            {
                ReloadDefectSettings();
            }
        }

        // Метод для обновления комбобокса
        private void ReloadDefectSettings()
        {
            LoadDefectSettings();

            paramDefCmB.Items.Clear();
            foreach (var defectSettingName in _defectSettings.Keys)
                paramDefCmB.Items.Add(defectSettingName);
        }

        private void InitializePrSettingsFileWatcher()
        {
            if (!Directory.Exists(_folderParamPr205))
                Directory.CreateDirectory(_folderParamPr205);
            // Настройка FileSystemWatcher
            _prSettingsWatcher = new FileSystemWatcher
            {
                Path = _folderParamPr205,
                Filter = "*.json",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };
            _prSettingsWatcher.Created += OnPrSettingsFolderChanged;
            _prSettingsWatcher.Deleted += OnPrSettingsFolderChanged;
            _prSettingsWatcher.Renamed += OnPrSettingsFolderChanged;
            _prSettingsWatcher.EnableRaisingEvents = true;
        }

        private void OnPrSettingsFolderChanged(object sender, FileSystemEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ReloadPrSettings()));
            }
            else
            {
                ReloadPrSettings();
            }
        }

        // Метод для обновления комбобокса
        private void ReloadPrSettings()
        {
            LoadPrSettings();

            prSettingsCmB.Items.Clear();
            foreach (var prSettingsName in _prSettings.Keys)
                prSettingsCmB.Items.Add(prSettingsName);
        }

        private void InitializeRecipeFolderAndRecipeFileWatcher()
        {
            currentReceptFolderTb.Text = _recipesFolder;

            if (!Directory.Exists(_recipesFolder))
                Directory.CreateDirectory(_recipesFolder);

            // Настройка FileSystemWatcher
            _recipesWatcher = new FileSystemWatcher
            {
                Path = _recipesFolder,
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

        private void InitializeCameraSettingsFileWatcher()
        {
            if (!Directory.Exists(_folderParamCamera))
                Directory.CreateDirectory(_folderParamCamera);
            // Настройка FileSystemWatcher
            _cameraSettingsWatcher = new FileSystemWatcher
            {
                Path = _folderParamCamera,
                Filter = "*.json",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };
            _cameraSettingsWatcher.Created += OnCameraSettingsFolderChanged;
            _cameraSettingsWatcher.Deleted += OnCameraSettingsFolderChanged;
            _cameraSettingsWatcher.Renamed += OnCameraSettingsFolderChanged;
            _cameraSettingsWatcher.EnableRaisingEvents = true;
        }

        private void OnCameraSettingsFolderChanged(object sender, FileSystemEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ReloadCameraSettings()));
            }
            else
            {
                ReloadCameraSettings();
            }
        }

        // Метод для обновления комбобокса
        private void ReloadCameraSettings()
        {
            LoadCameraSettings();

            cameraSettingsCmB.Items.Clear();
            foreach (var cameraSettingsName in _cameraSettings.Keys)
                cameraSettingsCmB.Items.Add(cameraSettingsName);
        }

        private void LoadPrParam()
        {
            // ===== Параметры ПР =====
            if (breakingTimeTb != null)
            {
                _breakingTimeValue = int.Parse(breakingTimeTb.Text);
                SendBreakingTime(_breakingTimeValue);
            }

            if (breakerOffsetTb != null)
            {
                _breakerOffsetValue = int.Parse(breakerOffsetTb.Text);
                SendBreakerOffset(_breakerOffsetValue);
            }

            if (cameraOffsetTb != null)
            {
                _cameraOffsetValue = int.Parse(cameraOffsetTb.Text);
                SendCameraOffset(_cameraOffsetValue);
            }
        }

        private void LoadDefectParam()
        {
            // ===== Параметры ПР =====
            double.TryParse(Properties.Settings.Default.Settings_OvalityThreshold, out _ovalityThreshold);

            if (ovalityCoefNumUpD != null)
            {
                ovalityCoefNumUpD.Text = _ovalityThreshold.ToString();
            }

            double.TryParse(Properties.Settings.Default.Settings_InclusionThreshold, out _inclusionThreshold);

            if (circleCoefNumUpD != null)
            {
                circleCoefNumUpD.Text = _inclusionThreshold.ToString();
            }

            double.TryParse(Properties.Settings.Default.Settings_MinAreaInclusion, out _minAreaInclusion);

            if (minSquareInclusionNumUpD != null)
            {
                minSquareInclusionNumUpD.Text = _minAreaInclusion.ToString();
            }

            double.TryParse(Properties.Settings.Default.Settings_MaxAreaInclusion, out _maxAreaInclusion);

            if (maxSquareInclusionNumUpD != null)
            {
                maxSquareInclusionNumUpD.Text = _maxAreaInclusion.ToString();
            }

            double.TryParse(Properties.Settings.Default.Settings_CoefCapRadiusInclusion, out _coefCapRadiusInclusion);

            if (coefCapRadiusInclusionUpD != null)
            {
                coefCapRadiusInclusionUpD.Text = _coefCapRadiusInclusion.ToString();
            }

            double.TryParse(Properties.Settings.Default.Settings_MinAreaInpaintDefect, out _minAreaInpaintDefect);

            if (minSquareInpaintNumUpD != null)
            {
                minSquareInpaintNumUpD.Text = _minAreaInpaintDefect.ToString();
            }

            double.TryParse(Properties.Settings.Default.Settings_MinInpaintWhiteThreshold, out _minInpaintWhiteThreshold);

            if (whiteThresoldNumUpD != null)
            {
                whiteThresoldNumUpD.Text = _minInpaintWhiteThreshold.ToString();
            }

            double.TryParse(Properties.Settings.Default.Settings_MinAreaObloy, out _minAreaObloy);

            if (obloyPixCountNumUpD != null)
            {
                obloyPixCountNumUpD.Text = _minAreaObloy.ToString();
            }

            double.TryParse(Properties.Settings.Default.Settings_СorrugationsCountForUnderFill, out _corrugationsCountForUnderFill);

            if (countCorrugationsNumUpD != null)
            {
                countCorrugationsNumUpD.Text = _corrugationsCountForUnderFill.ToString();
            }

            double.TryParse(Properties.Settings.Default.Settings_СoefCapRadiusUnderFill, out _coefCapRadiusUnderFill);

            if (coefCapRadiusMaskUnderFillNumUpD != null)
            {
                coefCapRadiusMaskUnderFillNumUpD.Text = _coefCapRadiusUnderFill.ToString();
            }
        }

        private void LoadCameraParam()
        {
            // ===== Параметры камеры (Width, Height, Exposure, Saturation) =====
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Settings_WidthFrame))
                frameWidthNumUpD.Text = Properties.Settings.Default.Settings_WidthFrame;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Settings_HeightFrame))
                frameHeightNumUpD.Text = Properties.Settings.Default.Settings_HeightFrame;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Settings_ExposureFrame))
                frameExposureNumUpD.Text = Properties.Settings.Default.Settings_ExposureFrame;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Settings_SaturationFrame))
                frameSaturationNumUpD.Text = Properties.Settings.Default.Settings_SaturationFrame;
        }

        private void SendStopSignalsToPLC()
        {
            try
            {
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    // Сбрасываем breakerAllowRegister
                    _modbusClient.WriteRegister(_breakerAllowRegister, (ushort)_breakerAllowFalse);

                    // Сбрасываем startRecognizeProcessing
                    if (!_isImageLoaded)
                        _modbusClient.WriteRegister(_startRecognizeProcessingRegister, (ushort)_recognizeProcessingFinish);
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
                var lastRecipeName = Properties.Settings.Default.Settings_LastRecipeFileName;

                if (!string.IsNullOrEmpty(lastRecipeName) && receptCapsCmB.Items.Contains(lastRecipeName))
                {
                    receptCapsCmB.SelectedItem = lastRecipeName;
                }
                else
                {
                    // иначе выбираем первый
                    receptCapsCmB.SelectedIndex = 0;
                }
            }
        }

        private void InitializeDefectSettings()
        {
            LoadDefectSettings();

            paramDefCmB.Items.Clear();
            foreach (var defectSettings in _defectSettings.Keys)
                paramDefCmB.Items.Add(defectSettings);

            if (_defectSettings.Count > 0)
            {
                var lastDefectSettingsName = Properties.Settings.Default.Settings_LastDefectSettingsFileName;

                if (!string.IsNullOrEmpty(lastDefectSettingsName) && paramDefCmB.Items.Contains(lastDefectSettingsName))
                {
                    paramDefCmB.SelectedItem = lastDefectSettingsName;
                }
                else
                {
                    // иначе выбираем первый
                    paramDefCmB.SelectedIndex = 0;
                }
            }
        }

        private void InitializePrSettings()
        {
            LoadPrSettings();

            prSettingsCmB.Items.Clear();
            foreach (var prSettings in _prSettings.Keys)
                prSettingsCmB.Items.Add(prSettings);

            if (_prSettings.Count > 0)
            {
                var lastPrSettingsName = Properties.Settings.Default.Settings_LastPrSettingsFileName;
                if (!string.IsNullOrEmpty(lastPrSettingsName) && prSettingsCmB.Items.Contains(lastPrSettingsName))
                {
                    prSettingsCmB.SelectedItem = lastPrSettingsName;
                }
                else
                {
                    // иначе выбираем первый
                    prSettingsCmB.SelectedIndex = 0;
                }
            }
        }

        private void InitializeCameraSettings()
        {
            LoadCameraSettings();

            cameraSettingsCmB.Items.Clear();
            foreach (var cameraSettings in _cameraSettings.Keys)
                cameraSettingsCmB.Items.Add(cameraSettings);

            if (_cameraSettings.Count > 0)
            {
                var lastCameraSettings = Properties.Settings.Default.Settings_LastCameraSettingsFileName;
                if (!string.IsNullOrEmpty(lastCameraSettings) && cameraSettingsCmB.Items.Contains(lastCameraSettings))
                {
                    cameraSettingsCmB.SelectedItem = lastCameraSettings;
                }
                else
                {
                    // иначе выбираем первый
                    cameraSettingsCmB.SelectedIndex = 0;
                }
            }
        }

        private void LoadRecipes()
        {
            _recipes.Clear();

            if (!Directory.Exists(_recipesFolder))
                Directory.CreateDirectory(_recipesFolder);

            string[] files = Directory.GetFiles(_recipesFolder, "*.json");

            foreach (var file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);

                CapRecipe recipe = JsonConvert.DeserializeObject<CapRecipe>(json);
                if (recipe != null)
                    _recipes[name] = recipe;
            }
        }

        private void LoadDefectSettings()
        {
            _defectSettings.Clear();

            if (!Directory.Exists(_folderParamDefect))
                Directory.CreateDirectory(_folderParamDefect);

            string[] files = Directory.GetFiles(_folderParamDefect, "*.json");

            foreach (var file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);

                DefectSettings defectSettings = JsonConvert.DeserializeObject<DefectSettings>(json);
                if (defectSettings != null)
                    _defectSettings[name] = defectSettings;
            }
        }

        private void LoadPrSettings()
        {
            _prSettings.Clear();

            if (!Directory.Exists(_folderParamPr205))
                Directory.CreateDirectory(_folderParamPr205);

            string[] files = Directory.GetFiles(_folderParamPr205, "*.json");

            foreach (var file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);

                PrSettings prSettings = JsonConvert.DeserializeObject<PrSettings>(json);
                if (prSettings != null)
                    _prSettings[name] = prSettings;
            }
        }

        private void LoadCameraSettings()
        {
            _cameraSettings.Clear();

            if (!Directory.Exists(_folderParamCamera))
                Directory.CreateDirectory(_folderParamCamera);

            string[] files = Directory.GetFiles(_folderParamCamera, "*.json");

            foreach (var file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);

                CameraSettings cameraSettings = JsonConvert.DeserializeObject<CameraSettings>(json);
                if (cameraSettings != null)
                    _cameraSettings[name] = cameraSettings;
            }
        }

        private void InitializeStatistics()
        {
            //_statisticsManager = new StatisticsManager(statisticsDataGridView);
        }

        private void InitLabelHoverEffects()
        {
            _labelNormalFont = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            _labelHoverFont = new Font("Segoe UI", 8.25F, FontStyle.Bold | FontStyle.Underline);

            ApplyHover(ovalityParamLb);
        }

        private void ApplyHover(Label label)
        {
            label.Cursor = Cursors.Hand;
            label.ForeColor = _labelNormalColor;
            label.Font = _labelNormalFont;

            label.MouseEnter += Label_MouseEnter;
            label.MouseLeave += Label_MouseLeave;
        }
        #endregion

        #region Обработчики событий UI

        #region Кнопки управления оборудованием

        private void connectCameraButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_cameraConnected)
                {
                    _cam.SendImage -= GetImage;
                    _cam.Close();

                    _cameraConnected = false;
                    connectCameraButton.Text = "Подключиться к камере";
                    connectCameraButton.BackColor = _disconnectedColor;

                    camStatus.Text = "Не подключено";
                    camStatus.ForeColor = Color.Red;

                    startStreamButton.Enabled = false;

                    ErrorLogger.Log(new Exception("Камера отключена пользователем"), "connectCameraButton_Click");
                }
                else
                {
                    if (_cam.Open())
                    {
                        _cam.SendImage -= GetImage;
                        _cam.SendImage += GetImage;
                        _cameraConnected = true;
                        connectCameraButton.Text = "Отключиться от камеры";
                        connectCameraButton.BackColor = _connectedColor;

                        camStatus.Text = "Подключено";
                        camStatus.ForeColor = Color.Green;

                        startStreamButton.Enabled = true;

                        ErrorLogger.Log(new Exception("Камера успешно подключена"), "connectCameraButton_Click");
                    }
                    else
                    {
                        _cameraConnected = false;
                        connectCameraButton.Text = "Подключиться к камере";
                        connectCameraButton.BackColor = _disconnectedColor;

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
                _cameraConnected = false;
                connectCameraButton.Text = "Подключиться";
                connectCameraButton.BackColor = _disconnectedColor;
                camStatus.Text = "Не подключено";
                camStatus.ForeColor = Color.Red;
                startStreamButton.Enabled = false;
            }
        }

        private void connectPrButton_Click(object sender, EventArgs e)
        {
            if (_prConnected && _modbusClient != null && _modbusClient.Connected)
            {
                try
                {
                    _manualDisconnect = true;

                    _modbusClient.DisableAutoReconnect();
                    _modbusClient.StopPolling();
                    _modbusClient.Disconnect();

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
                string ip = pr205IpTb.Text.Trim();

                if (!int.TryParse(pr205PortTb.Text.Trim(), out int port))
                    throw new Exception("Неверный формат порта ПР205");

                if (_modbusClient != null)
                    _modbusClient.ConnectionStatusChanged -= ModbusClient_ConnectionStatusChanged;

                _modbusClient = new ModbusTCP(ip, port);
                _modbusClient.ConnectionStatusChanged += ModbusClient_ConnectionStatusChanged;
                _modbusClient.EnableAutoReconnect();

                if (_modbusClient.Connect())
                {
                    _modbusClient.StartPolling();

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
                    _manualDisconnect = false;
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
                _manualDisconnect = false;
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
            if (_isImageLoaded)
            {
                _isStreamCam = false;
                originPb.Image?.Dispose();
                originPb.Image = null;

                _imageFiles = [];
                _isProcessingFromFolder = false;

                // Восстанавливаем внешний вид кнопки
                loadImageButton.Text = "Загрузить";
                loadImageButton.BackColor = Color.FromArgb(66, 133, 244); // Синий

                // Разблокируем кнопку запуска потока
                if (_cameraConnected)
                {
                    startStreamButton.Enabled = true;
                }

                _isImageLoaded = false;
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
                        _imageFiles = [.. openFileDialog.FileNames];
                        _currentImageIndex = 0;
                        if (_imageFiles.Count > 0)
                        {
                            _isProcessingFromFolder = true;
                        }

                        if (_imageFiles.Count > 0)
                        {
                            LoadAndDisplayCurrentImage();

                            // Меняем внешний вид кнопки
                            loadImageButton.Text = "Отключить";
                            loadImageButton.BackColor = Color.FromArgb(229, 115, 115); // Красный

                            // Блокируем кнопку старта потока
                            startStreamButton.Enabled = false;

                            _isImageLoaded = true;
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
                if (!_isStreamCam)
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
                if (_originalImage != null)
                {
                    _originalImage = null;
                }

                _isStreamCam = true;

                //ApplyRecognitionParameters();

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
                recognizeButton.Enabled = true;

                loadImageForReceptParamBt.Enabled = false;
                openCurReceptFolderBt.Enabled = false;
                loadImageTestBt.Enabled = false;
                openCurrentFolderBtn.Enabled = false;
                chooseBaseFolderBtn.Enabled = false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в StartStream");

                connectCameraButton.Enabled = true;
                connectPrButton.Enabled = true;
                loadImageButton.Enabled = true;
                _isStreamCam = false;
            }
        }

        private void StopStream()
        {
            _isStreamCam = false;

            originPb.Image?.Dispose();
            originPb.Image = null;

            // Разблокируем кнопки подключения
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
            recognizeButton.Enabled = false;

            loadImageForReceptParamBt.Enabled = true;
            openCurReceptFolderBt.Enabled = true;
            loadImageTestBt.Enabled = true;
            openCurrentFolderBtn.Enabled = true;
            chooseBaseFolderBtn.Enabled = true;

        }


        #endregion

        #region Кнопки обработки и распознавания

        private async void recognizeButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_isProcessing)
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

        #region Кнопки настроек

        #region Настройки камеры
        private void applySettingsButton_Click(object sender, EventArgs e)
        {
            ApplyCameraSettings();
        }

        private void saveCameraSettingsNew_Click(object sender, EventArgs e)
        {
            if (!ValidateCameraSettings())
            {
                return;
            }

            // --- Папка ---
            string folder = _folderParamCamera;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string name = cameraSettingsNameTb.Text.Trim();
            string fileName = name + ".json";
            string fullPath = Path.Combine(folder, fileName);

            bool existedBefore = File.Exists(fullPath);

            var cameraSettings = new CameraSettings
            {
                Name = name,
                Height = int.Parse(frameHeightNumUpD.Text),
                Width = int.Parse(frameWidthNumUpD.Text),
                Exposure = int.Parse(frameExposureNumUpD.Text),
                Saturation = int.Parse(frameSaturationNumUpD.Text)
            };

            // --- JSON с нормальной русской кодировкой ---
            string json = System.Text.Json.JsonSerializer.Serialize(
                cameraSettings,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }
            );

            File.WriteAllText(fullPath, json, Encoding.UTF8);

            // Сохраняем в Settings
            Properties.Settings.Default.Settings_WidthFrame = frameWidthNumUpD.Text;
            Properties.Settings.Default.Settings_HeightFrame = frameHeightNumUpD.Text;
            Properties.Settings.Default.Settings_ExposureFrame = frameExposureNumUpD.Text;
            Properties.Settings.Default.Settings_SaturationFrame = frameSaturationNumUpD.Text;
            Properties.Settings.Default.Save();

            // --- Обновляем список настроек дефектов ---
            LoadCameraSettings();

            cameraSettingsCmB.Items.Clear();
            foreach (var cameraSettingsNamer in _cameraSettings.Keys)
                cameraSettingsCmB.Items.Add(cameraSettingsNamer);
            cameraSettingsCmB.SelectedItem = name;

            // --- Сообщение ---
            if (existedBefore)
                MessageBox.Show($"Файл настроек \"{name}\" редактирован успешно!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show($"Файл настроек \"{name}\" создан успешно!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void cameraSettingsCmB_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = cameraSettingsCmB.SelectedItem?.ToString();
            if (key == null || !_cameraSettings.ContainsKey(key))
                return;

            Properties.Settings.Default.Settings_LastCameraSettingsFileName = key;
            Properties.Settings.Default.Save();

            ApplyCameraSettings(_cameraSettings[key]);
        }

        private void ApplyCameraSettings(CameraSettings r)
        {
            _isApplyingCameraSettings = true;

            frameHeightNumUpD.Text = r.Height.ToString();
            frameWidthNumUpD.Text = r.Width.ToString();
            frameExposureNumUpD.Text = r.Exposure.ToString();
            frameSaturationNumUpD.Text = r.Saturation.ToString();

            _isApplyingCameraSettings = false;
        }

        private bool ValidateCameraSettings()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cameraSettingsNameTb.Text))
                {
                    string msg = "Введите имя файла настроек.";
                    ErrorLogger.Log(new Exception(msg), "ValidateDefectSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cameraSettingsNameTb.Focus();
                    return false;
                }

                if (!int.TryParse(frameWidthNumUpD.Text, out int width) || width <= 0)
                {
                    string msg = "'Ширина' кадра должна быть положительным числом.";
                    ErrorLogger.Log(new Exception(msg), "ValidateCameraSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    frameWidthNumUpD.Focus();
                    return false;
                }

                if (!int.TryParse(frameHeightNumUpD.Text, out int height) || height <= 0)
                {
                    string msg = "'Высота' кадра должна быть положительным числом.";
                    ErrorLogger.Log(new Exception(msg), "ValidateCameraSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    frameHeightNumUpD.Focus();
                    return false;
                }

                if (!int.TryParse(frameExposureNumUpD.Text, out int exposure) || exposure < 0)
                {
                    string msg = "'Экспозиция' должна быть положительным числом.";
                    ErrorLogger.Log(new Exception(msg), "ValidateCameraSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    frameExposureNumUpD.Focus();
                    return false;
                }

                if (!int.TryParse(frameSaturationNumUpD.Text, out int saturation)
                    || saturation < 0 || saturation > 255)
                {
                    string msg = "'Сатурация' должна быть числом от 0 до 255.";
                    ErrorLogger.Log(new Exception(msg), "ValidateCameraSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    frameSaturationNumUpD.Focus();
                    return false;
                }

                return true;

            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "ValidateCameraSettings");
                MessageBox.Show("Ошибка при проверке настроек камеры:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        private void loadSettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    openFileDialog.Title = "Загрузить настройки";
                    openFileDialog.InitialDirectory = _folderParamCamera;

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

        #region Настройки ПР
        private void savePr205SettingsNew_Click(object sender, EventArgs e)
        {
            if (!ValidatePrSettings())
            {
                return;
            }

            // --- Папка ---
            string folder = _folderParamPr205;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string name = prSettingsNameTb.Text.Trim();
            string fileName = name + ".json";
            string fullPath = Path.Combine(folder, fileName);

            bool existedBefore = File.Exists(fullPath);

            var prSettings = new PrSettings
            {
                Name = name,
                IpAddress = pr205IpTb.Text,
                Port = int.Parse(pr205PortTb.Text),
                BreakingTime = int.Parse(breakingTimeTb.Text),
                CameraOffset = int.Parse(cameraOffsetTb.Text),
                BreakerOffset = int.Parse(breakerOffsetTb.Text)
            };

            // --- JSON с нормальной русской кодировкой ---
            string json = System.Text.Json.JsonSerializer.Serialize(
                prSettings,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }
            );

            File.WriteAllText(fullPath, json, Encoding.UTF8);

            // Сохраняем в Settings
            Properties.Settings.Default.Settings_BreakingTime = breakingTimeTb.Text;
            Properties.Settings.Default.Settings_CameraOffset = cameraOffsetTb.Text;
            Properties.Settings.Default.Settings_BreakerOffset = breakerOffsetTb.Text;
            Properties.Settings.Default.Settings_IpAdressPr = pr205IpTb.Text;
            Properties.Settings.Default.Settings_PortPr = pr205PortTb.Text;
            Properties.Settings.Default.Save();

            // --- Обновляем список настроек дефектов ---
            LoadPrSettings();

            prSettingsCmB.Items.Clear();
            foreach (var prSettingsNamer in _prSettings.Keys)
                prSettingsCmB.Items.Add(prSettingsNamer);
            prSettingsCmB.SelectedItem = name;

            // --- Сообщение ---
            if (existedBefore)
                MessageBox.Show($"Файл настроек \"{name}\" редактирован успешно!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show($"Файл настроек \"{name}\" создан успешно!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void prSettingsCmB_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = prSettingsCmB.SelectedItem?.ToString();
            if (key == null || !_prSettings.ContainsKey(key))
                return;

            Properties.Settings.Default.Settings_LastPrSettingsFileName = key;
            Properties.Settings.Default.Save();

            ApplyPrSettings(_prSettings[key]);
        }

        private void ApplyPrSettings(PrSettings r)
        {
            _isApplyingPrSettings = true;

            pr205IpTb.Text = r.IpAddress;
            pr205PortTb.Text = r.Port.ToString();
            breakingTimeTb.Text = r.BreakingTime.ToString();
            cameraOffsetTb.Text = r.CameraOffset.ToString();
            breakerOffsetTb.Text = r.BreakerOffset.ToString();

            _isApplyingPrSettings = false;
        }

        private bool ValidatePrSettings()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(prSettingsNameTb.Text))
                {
                    string msg = "Введите имя файла настроек.";
                    ErrorLogger.Log(new Exception(msg), "ValidatePrSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    prSettingsNameTb.Focus();
                    return false;
                }

                if (!System.Net.IPAddress.TryParse(pr205IpTb.Text, out _))
                {
                    string msg = "''Ip адрес сдува' имеет неправльный формат'.\nПример: 192.168.0.10";
                    ErrorLogger.Log(new Exception(msg), "ValidatePrSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    pr205IpTb.Focus();
                    return false;
                }

                if (!int.TryParse(pr205PortTb.Text, out int port) || port < 1 || port > 65535)
                {
                    string msg = "'Порт ПР205' должен быть числом от 1 до 65535.";
                    ErrorLogger.Log(new Exception(msg), "ValidatePrSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    pr205PortTb.Focus();
                    return false;
                }

                if (!int.TryParse(breakingTimeTb.Text, out int delay) || delay < 0 || delay > 65535)
                {
                    string msg = "'Время сдува, мс' должно быть числом от 0 до 65535.";
                    ErrorLogger.Log(new Exception(msg), "ValidatePrSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    breakingTimeTb.Focus();
                    return false;
                }

                if (!int.TryParse(cameraOffsetTb.Text, out int cameraOffset) || cameraOffset < 0 || cameraOffset > 65535)
                {
                    string msg = "'Расстояние от датчика до камеры, шаги' должно быть числом от 0 до 65535.";
                    ErrorLogger.Log(new Exception(msg), "ValidatePrSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cameraOffsetTb.Focus();
                    return false;
                }

                if (!int.TryParse(breakerOffsetTb.Text, out int breakerOffset) || breakerOffset < 0 || breakerOffset > 65535)
                {
                    string msg = "'Расстояние от датчика до сдува, шаги' должно быть числом от 0 до 65535.";
                    ErrorLogger.Log(new Exception(msg), "ValidatePrSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    breakerOffsetTb.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "ValidatePrSettings — непредвиденная ошибка");

                MessageBox.Show("Непредвиденная ошибка при проверке настроек:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        private async void ApplyPr_Click(object sender, EventArgs e)
        {
            if (_modbusClient == null || !_modbusClient.Connected)
            {
                return;
            }

            _applyPrCts = new CancellationTokenSource();

            try
            {
                if (!int.TryParse(breakingTimeTb.Text.Trim(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out _breakingTimeValue) || _breakingTimeValue <= 0)
                {
                    _breakingTimeValue = 55;
                }

                await Task.Run(() => SendBreakingTime(_breakingTimeValue), _applyPrCts.Token);

                if (!int.TryParse(cameraOffsetTb.Text.Trim(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out _cameraOffsetValue) || _cameraOffsetValue <= 0)
                {
                    _cameraOffsetValue = 300;
                }

                await Task.Run(() => SendCameraOffset(_cameraOffsetValue), _applyPrCts.Token);

                if (!int.TryParse(breakerOffsetTb.Text.Trim(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out _breakerOffsetValue) || _breakerOffsetValue <= 0)
                {
                    _breakerOffsetValue = 2430;
                }

                await Task.Run(() => SendBreakerOffset(_breakerOffsetValue), _applyPrCts.Token);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка при отправке параметров на ПР205 (ApplyPr_Click)");
            }
        }
        #endregion

        #region Настройка параметров обнаржуения дефектов
        private void saveDefectSettingsNew_Click(object sender, EventArgs e)
        {
            if (!ValidateDefectSettings())
            {
                return;
            }

            // --- Папка ---
            string folder = _folderParamDefect;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string name = defectSettingsNameTb.Text.Trim();
            string fileName = name + ".json";
            string fullPath = Path.Combine(folder, fileName);

            bool existedBefore = File.Exists(fullPath);

            var defectSettings = new DefectSettings
            {
                Name = name,
                OvalityThreshold = double.Parse(ovalityCoefNumUpD.Text),
                InclusionThreshold = double.Parse(circleCoefNumUpD.Text),
                MinAreaInclusion = double.Parse(minSquareInclusionNumUpD.Text),
                MaxAreaInclusion = double.Parse(maxSquareInclusionNumUpD.Text),
                CoefCapRadiusInclusion = double.Parse(coefCapRadiusInclusionUpD.Text),
                MinAreaInpaintDefect = double.Parse(minSquareInpaintNumUpD.Text),
                MinInpaintWhiteThreshold = double.Parse(whiteThresoldNumUpD.Text),
                MinAreaObloy = double.Parse(obloyPixCountNumUpD.Text),
                CorrugationsCountForUnderFill = double.Parse(countCorrugationsNumUpD.Text),
                CoefCapRadiusUnderFill = double.Parse(coefCapRadiusMaskUnderFillNumUpD.Text)
            };

            // --- JSON с нормальной русской кодировкой ---
            string json = System.Text.Json.JsonSerializer.Serialize(
                defectSettings,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }
            );

            File.WriteAllText(fullPath, json, Encoding.UTF8);

            // Сохраняем в Settings
            Properties.Settings.Default.Settings_OvalityThreshold = ovalityCoefNumUpD.Text;
            Properties.Settings.Default.Settings_InclusionThreshold = circleCoefNumUpD.Text;
            Properties.Settings.Default.Settings_MinAreaInclusion = minSquareInclusionNumUpD.Text;
            Properties.Settings.Default.Settings_MaxAreaInclusion = maxSquareInclusionNumUpD.Text;
            Properties.Settings.Default.Settings_CoefCapRadiusInclusion = coefCapRadiusInclusionUpD.Text;
            Properties.Settings.Default.Settings_MinAreaInpaintDefect = minSquareInpaintNumUpD.Text;
            Properties.Settings.Default.Settings_MinInpaintWhiteThreshold = whiteThresoldNumUpD.Text;
            Properties.Settings.Default.Settings_MinAreaObloy = obloyPixCountNumUpD.Text;
            Properties.Settings.Default.Settings_СorrugationsCountForUnderFill = countCorrugationsNumUpD.Text;
            Properties.Settings.Default.Settings_СoefCapRadiusUnderFill = coefCapRadiusMaskUnderFillNumUpD.Text;
            Properties.Settings.Default.Save();

            // --- Обновляем список настроек дефектов ---
            LoadDefectSettings();

            paramDefCmB.Items.Clear();
            foreach (var defectSettingsNamer in _defectSettings.Keys)
                paramDefCmB.Items.Add(defectSettingsNamer);
            paramDefCmB.SelectedItem = name;

            // --- Сообщение ---
            if (existedBefore)
                MessageBox.Show($"Файл настроек \"{name}\" редактирован успешно!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show($"Файл настроек \"{name}\" создан успешно!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void paramDefCmB_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = paramDefCmB.SelectedItem?.ToString();
            if (key == null || !_defectSettings.ContainsKey(key))
                return;

            Properties.Settings.Default.Settings_LastDefectSettingsFileName = key;
            Properties.Settings.Default.Save();

            ApplyDefectSettings(_defectSettings[key]);
        }

        private void ApplyDefectSettings(DefectSettings r)
        {
            _isApplyingDefectSettings = true;

            ovalityCoefNumUpD.Text = r.OvalityThreshold.ToString();

            circleCoefNumUpD.Text = r.InclusionThreshold.ToString();
            coefCapRadiusInclusionUpD.Text = r.CoefCapRadiusInclusion.ToString();
            minSquareInclusionNumUpD.Text = r.MinAreaInclusion.ToString();
            maxSquareInclusionNumUpD.Text = r.MaxAreaInclusion.ToString();

            minSquareInpaintNumUpD.Text = r.MinAreaInpaintDefect.ToString();
            whiteThresoldNumUpD.Text = r.MinInpaintWhiteThreshold.ToString();

            obloyPixCountNumUpD.Text = r.MinAreaObloy.ToString();

            countCorrugationsNumUpD.Text = r.CorrugationsCountForUnderFill.ToString();
            coefCapRadiusMaskUnderFillNumUpD.Text = r.CoefCapRadiusUnderFill.ToString();

            _isApplyingDefectSettings = false;
        }

        private bool ValidateDefectSettings()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(defectSettingsNameTb.Text))
                {
                    string msg = "Введите имя файла настроек.";
                    ErrorLogger.Log(new Exception(msg), "ValidateDefectSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    defectSettingsNameTb.Focus();
                    return false;
                }

                if (!double.TryParse(ovalityCoefNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double ovality)
                    || ovality <= 0 || ovality > 1)
                {
                    string msg = "Параметр 'Коэффициент овальности' должен быть числом от 0 до 1.";
                    ErrorLogger.Log(new Exception(msg), "ValidateDefectSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ovalityCoefNumUpD.Focus();
                    return false;
                }

                if (!double.TryParse(circleCoefNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double inclusion)
                    || inclusion <= 0 || inclusion > 1)
                {
                    string msg = "Параметр 'Коэффициент округлости вкраплений' должен быть числом от 0 до 1.";
                    ErrorLogger.Log(new Exception(msg), "ValidateDefectSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    circleCoefNumUpD.Focus();
                    return false;
                }

                if (!double.TryParse(coefCapRadiusInclusionUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double coefCapRadius)
                    || coefCapRadius <= 0 || coefCapRadius > 1)
                {
                    string msg = "Параметр 'Коэффициент от радиуса крышки' должен быть числом от 0 до 1.";
                    ErrorLogger.Log(new Exception(msg), "ValidateDefectSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    coefCapRadiusInclusionUpD.Focus();
                    return false;
                }

                if (!double.TryParse(minSquareInclusionNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double minInclusion)
                    || minInclusion < 0)
                {
                    string msg = "Параметр 'Минимальная площадь вкрапления' должен быть ≥ 0.";
                    ErrorLogger.Log(new Exception(msg), "ValidateDefectSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    minSquareInclusionNumUpD.Focus();
                    return false;
                }

                if (!double.TryParse(maxSquareInclusionNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double maxInclusion)
                    || maxInclusion <= minInclusion)
                {
                    string msg = "Параметр 'Максимальная площадь вкрапления' должен быть больше минимальной.";
                    ErrorLogger.Log(new Exception(msg), "ValidateDefectSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    maxSquareInclusionNumUpD.Focus();
                    return false;
                }

                if (!double.TryParse(minSquareInpaintNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double minInpaint)
                    || minInpaint <= 0)
                {
                    string msg = "Параметр 'Минимальная площадь непрокраса' должен быть > 0.";
                    ErrorLogger.Log(new Exception(msg), "ValidateDefectSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    minSquareInpaintNumUpD.Focus();
                    return false;
                }

                if (!double.TryParse(whiteThresoldNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double whiteThreshold)
                    || whiteThreshold <= 0)
                {
                    string msg = "Параметр 'Близость к белому' должен быть > 0.";
                    ErrorLogger.Log(new Exception(msg), "ValidateDefectSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    whiteThresoldNumUpD.Focus();
                    return false;
                }

                if (!double.TryParse(obloyPixCountNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double obloy)
                    || obloy <= 0)
                {
                    string msg = "Параметр 'Минимальная площадь облоя' должен быть > 0.";
                    ErrorLogger.Log(new Exception(msg), "ValidateDefectSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    obloyPixCountNumUpD.Focus();
                    return false;
                }

                if (!double.TryParse(countCorrugationsNumUpD.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double corrugationsCount)
                    || corrugationsCount <= 0)
                {
                    string msg = "Параметр 'Количество зубцов на коронке' должен быть > 0.";
                    ErrorLogger.Log(new Exception(msg), "ValidateDefectSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    countCorrugationsNumUpD.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "ValidateDefectSettings - непредвиденная ошибка");

                MessageBox.Show("Произошла непредвиденная ошибка при проверке настроек:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

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

            Properties.Settings.Default.Settings_LastRecipeFileName = key;
            Properties.Settings.Default.Save();

            ApplyRecipe(_recipes[key]);
        }

        private void ApplyRecipe(CapRecipe r)
        {
            _isApplyingRecipe = true;

            // ---------- Цветовые флаги ----------
            _isGreenColor = r.IsGreen;
            _isColored = r.IsColored;
            _isYellowCap = r.IsYellow;
            _capsAreWhite = r.IsWhite;
            _isBlackOrBrown = r.IsBlackOrBrown;

            _recipeIsGreenColor = r.IsGreen;
            _recipeIsColored = r.IsColored;
            _recipeIsYellowCap = r.IsYellow;
            _recipeCapsAreWhite = r.IsWhite;
            _recipeIsBlackOrBrown = r.IsBlackOrBrown;

            // ---------- Параметры обработки ----------
            // Для цв/бцв крышек
            window = r.Window;
            morph_size = r.MorphSize;
            morph_size_2 = r.MorphSize2;
            _capsColor = r.CapsColor;
            _saturationColor = r.CameraSaturation;

            _contourCorrectionColor = (float)Clamp(
                (decimal)r.ContourCorrectionColor,
                contourCorrectionColorUpDown.Minimum,
                contourCorrectionColorUpDown.Maximum
            );

            // Для черных/коричневых крышек
            _saturationBlackOrBrown = (int)Clamp(
                r.CameraSaturationBlackOrBrown,
                saturationBlackOrBrownUpDown.Minimum,
                saturationBlackOrBrownUpDown.Maximum
            );

            _medianFilter = (int)Clamp(
                r.MedianFilter,
                medianFilterUpDown.Minimum,
                medianFilterUpDown.Maximum
            );

            _cannyThreshold = (int)Clamp(
                r.CannyThreshold,
                cannyUpDown.Minimum,
                cannyUpDown.Maximum
            );

            _contourCorrectionBlackOrBrown = (float)Clamp(
                (decimal)r.ContourCorrectionBlackOrBrown,
                contourCorrectionBlackOrBrownUpDown.Minimum,
                contourCorrectionBlackOrBrownUpDown.Maximum
            );

            // ---------- Чекбоксы UI (важно ДО камеры) ----------
            isWhiteCb.Checked = _capsAreWhite;
            isBlackOrBrownCb.Checked = _isBlackOrBrown;
            isColorCb.Checked = _isColored;
            isGreenCb.Checked = _isGreenColor;
            isYellowCb.Checked = _isYellowCap;

            UpdateColorModeUI();

            // ---------- Камера ----------
            if (_cam != null)
            {
                uint saturation = isBlackOrBrownCb.Checked
                    ? (uint)r.CameraSaturationBlackOrBrown
                    : (uint)r.CameraSaturation;

                _cam.Saturation = saturation;
                _cam.SetSaturation();
            }

            // ---------- inpaint ----------
            if (_capsAreWhite)
            {
                inpaintCB.Checked = false;
                inpaintCB.Enabled = false;
            }
            else
            {
                inpaintCB.Enabled = true;
                inpaintCB.Checked = true;
            }

            // ---------- Интерфейс рецепта ----------
            // Для цв/бцв крышек
            saturationColorUpDown.Value = Clamp(
                r.CameraSaturation,
                saturationColorUpDown.Minimum,
                saturationColorUpDown.Maximum
            );

            capcolorUpDown.Value = Clamp(
                r.CapsColor,
                capcolorUpDown.Minimum,
                capcolorUpDown.Maximum
            );

            if (windowCb.Items.Contains(r.Window.ToString()))
                windowCb.SelectedItem = r.Window.ToString();

            if (morphCb.Items.Contains(r.MorphSize.ToString()))
                morphCb.SelectedItem = r.MorphSize.ToString();

            contourCorrectionColorUpDown.Value = Clamp(
                (decimal)r.ContourCorrectionColor,
                contourCorrectionColorUpDown.Minimum,
                contourCorrectionColorUpDown.Maximum
            );

            // Для черных/коричневых крышек
            saturationBlackOrBrownUpDown.Value = Clamp(
                r.CameraSaturationBlackOrBrown,
                saturationBlackOrBrownUpDown.Minimum,
                saturationBlackOrBrownUpDown.Maximum
            );

            medianFilterUpDown.Value = Clamp(
                r.MedianFilter,
                medianFilterUpDown.Minimum,
                medianFilterUpDown.Maximum
            );

            cannyUpDown.Value = Clamp(
                r.CannyThreshold,
                cannyUpDown.Minimum,
                cannyUpDown.Maximum
            );

            contourCorrectionBlackOrBrownUpDown.Value = Clamp(
                (decimal)r.ContourCorrectionBlackOrBrown,
                contourCorrectionBlackOrBrownUpDown.Minimum,
                contourCorrectionBlackOrBrownUpDown.Maximum
            );

            // ---------- UI текст ----------
            receptNameTb.Text = r.Name;

            frameSaturationNumUpD.Text = (
                isBlackOrBrownCb.Checked
                    ? r.CameraSaturationBlackOrBrown
                    : r.CameraSaturation
            ).ToString();

            RebuildMorphology();

            _isApplyingRecipe = false;
        }

        private decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
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

        private bool CheckOvality(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                // Если контур пустой или null, сразу возвращаем false
                if (capContour == null || capContour.Length < 5)
                {
                    ErrorLogger.Log(new Exception("Контур для проверки овальности пустой или содержит недостаточно точек"),
                        "CheckOvality - проверка наличия контуров");
                    return false;
                }

                token.ThrowIfCancellationRequested();

                RotatedRect ellipse = Cv2.FitEllipse(capContour);

                var majorAxis = Math.Max(ellipse.Size.Width, ellipse.Size.Height);
                var minorAxis = Math.Min(ellipse.Size.Width, ellipse.Size.Height);
                var axisRatio = minorAxis / majorAxis;

                bool isOval = axisRatio < _ovalityThreshold;

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

        private bool CheckForInclusions(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length < 5)
                {
                    ErrorLogger.Log(new Exception("Контур для проверки включений пустой или содержит недостаточно точек"),
                        "CheckForInclusions - проверка наличия контура");
                    return false;
                }

                token.ThrowIfCancellationRequested();

                RotatedRect ellipse;
                try
                {
                    ellipse = Cv2.FitEllipse(capContour);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForInclusions - ошибка при расчёте эллипса");
                    return false;
                }

                Point2f ellipseCenter = ellipse.Center;
                float ellipseRadius = (float)(_coefCapRadiusInclusion * (ellipse.Size.Width + ellipse.Size.Height) / 4.0);

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
                                if (area > _minAreaInclusion && area < _maxAreaInclusion && IsCircularContour(contour))
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
                                                Cv2.ContourArea(c) > _minAreaInpaintDefect).ToList();
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

                                                            if (Math.Abs(meanColor.Val2 - 255) < _minInpaintWhiteThreshold)
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

                if (_blurChannel_1 == null || _blurChannel_2 == null)
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
                    if (_capRadiusMask == null || _capRadiusMask.Size() != image.Size())
                    {
                        _capRadiusMask?.Dispose();
                        _capRadiusMask = new Mat(image.Rows, image.Cols, MatType.CV_8UC1);
                    }

                    GetIdealCapMask(_capRadiusMask, capCenter,
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
                    Cv2.BitwiseAnd(_capRadiusMask, _blurChannel_2, _blurChannel_2);
                    Cv2.MorphologyEx(_blurChannel_2, _blurChannel_1, MorphTypes.Erode, elementMask);
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
                    Cv2.FindContours(_blurChannel_1, out obloyContours, out hierarchyObloy,
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
                    pixCount = Cv2.CountNonZero(_blurChannel_1);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "CheckForObloyDefects - ошибка при подсчете ненулевых пикселей");
                    return false;
                }

                if (pixCount > _minAreaObloy)
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

                return pixCount > _minAreaObloy;
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

        private bool CheckForUnderFillDefects(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length < 5)
                {
                    ErrorLogger.Log(new Exception("Контур для проверки недолива пустой или содержит недостаточно точек"),
                        "CheckForUnderFillDefects - проверка наличия контура");
                    return false;
                }

                // ===== ПРИМЕНЯЕМ ЦВЕТОКОРРЕКЦИЮ ПРЯМО ЗДЕСЬ =====
                // Создаем копию исходного изображения для цветокоррекции
                Mat colorCorrectedImage = image.Clone();
                Mat colorCorrectedGray = null;

                try
                {
                    // Применяем цветокоррекцию с нужными параметрами
                    // Параметры: img, nWhite=capsColor, isColored=true, isYellowCap=true, isGreenColor=false
                    NonlinearBackgroundDecolorization(colorCorrectedImage, _capsColor, true, true, false);

                    // Создаем grayscale версию
                    colorCorrectedGray = new Mat();
                    Cv2.CvtColor(colorCorrectedImage, colorCorrectedGray, ColorConversionCodes.BGR2GRAY);

                    token.ThrowIfCancellationRequested();

                    // === ЭТАП А: Нахождение коронки ===
                    RotatedRect outerEllipse = Cv2.FitEllipse(capContour);

                    Size2f outerSize = outerEllipse.Size;
                    Size2f innerSize = new Size2f(
                        (float)(outerSize.Width * _coefCapRadiusUnderFill),
                        (float)(outerSize.Height * _coefCapRadiusUnderFill)
                    );

                    RotatedRect innerEllipse = new RotatedRect(
                        outerEllipse.Center,
                        innerSize,
                        outerEllipse.Angle
                    );

                    token.ThrowIfCancellationRequested();

                    // Параметры развертки
                    int rectHeight = (int)(Math.Max(outerSize.Width, outerSize.Height) * 0.15);

                    // === ЭТАП Б: Выделение коронки и разворот ===
                    Mat crownMask = new Mat(colorCorrectedImage.Size(), MatType.CV_8UC1, Scalar.All(0));

                    try
                    {
                        Cv2.Ellipse(crownMask, outerEllipse, Scalar.White, -1);
                        Cv2.Ellipse(crownMask, innerEllipse, Scalar.Black, -1);

                        token.ThrowIfCancellationRequested();

                        // Выделяем область коронки на цветном и grayscale изображениях
                        Mat maskedColor = new Mat();
                        Cv2.BitwiseAnd(colorCorrectedImage, colorCorrectedImage, maskedColor, crownMask);

                        Mat maskedGray = new Mat();
                        Cv2.BitwiseAnd(colorCorrectedGray, colorCorrectedGray, maskedGray, crownMask);

                        token.ThrowIfCancellationRequested();

                        // Разворачиваем коронку в прямоугольник
                        Mat rectifiedCrown = new Mat(rectHeight, UNDERFILL_RECT_WIDTH, MatType.CV_8UC1);

                        float outerRadius = (float)(Math.Max(outerSize.Width, outerSize.Height) / 2);
                        float innerRadius = (float)(Math.Max(innerSize.Width, innerSize.Height) / 2);
                        float meanRadius = (outerRadius + innerRadius) / 2;

                        Point center = new Point((int)outerEllipse.Center.X, (int)outerEllipse.Center.Y);

                        // Используем кэшированные тригонометрические таблицы
                        GetStripeImg(maskedGray, rectifiedCrown, _sinTable, _cosTable,
                                    center, (int)meanRadius,
                                    maskedGray.Width, maskedGray.Height,
                                    UNDERFILL_RECT_WIDTH, rectHeight);

                        token.ThrowIfCancellationRequested();

                        // === ЭТАП В: Анализ через Фурье ===
                        float[] rowStatistics = new float[UNDERFILL_RECT_WIDTH];
                        GetWStatistics(rectifiedCrown, rowStatistics, UNDERFILL_RECT_WIDTH, rectHeight);

                        token.ThrowIfCancellationRequested();

                        bool hasUnderfill = AnalyzeUnderfillFFT(rowStatistics, UNDERFILL_RECT_WIDTH, _corrugationsCountForUnderFill);

                        // Отрисовка результата
                        try
                        {
                            Scalar color = hasUnderfill ? new Scalar(0, 0, 255) : new Scalar(0, 255, 0);
                            Cv2.Ellipse(drawFrame, innerEllipse, color, 2);
                        }
                        catch (Exception drawEx)
                        {
                            ErrorLogger.Log(drawEx, "CheckForUnderFillDefects - ошибка при рисовании эллипса");
                        }

                        return hasUnderfill;
                    }
                    finally
                    {
                        crownMask?.Dispose();
                        // maskedColor и maskedGray будут автоматически освобождены в using, который добавим ниже
                    }
                }
                finally
                {
                    // Освобождаем созданные ресурсы
                    colorCorrectedImage?.Dispose();
                    colorCorrectedGray?.Dispose();
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CheckForUnderFillDefects - ошибка при определении недолива");
                return false;
            }
        }

        /// <summary>
        /// Анализ Фурье для определения недолива
        /// </summary>
        private bool AnalyzeUnderfillFFT(float[] signal, int length, double expectedHarmonic)
        {
            try
            {
                if (signal == null || signal.Length < length)
                    return false;

                // Копируем сигнал для анализа
                float[] data = new float[length];
                Array.Copy(signal, data, length);

                // Центрируем сигнал (вычитаем среднее)
                double mean = 0;
                for (int i = 0; i < length; i++)
                    mean += data[i];
                mean /= length;

                for (int i = 0; i < length; i++)
                    data[i] -= (float)mean;

                // Преобразуем в комплексные числа для FFT
                System.Numerics.Complex[] complexData = new System.Numerics.Complex[length];
                for (int i = 0; i < length; i++)
                    complexData[i] = new System.Numerics.Complex(data[i], 0);

                // Выполняем FFT
                MathNet.Numerics.IntegralTransforms.Fourier.Forward(complexData);

                // Анализируем гармоники в окрестности ожидаемой
                int minHarmonic = (int)Math.Max(1, expectedHarmonic - 2);
                int maxHarmonic = (int)Math.Min(length / 2, expectedHarmonic + 2);

                double maxMagnitude = 0;
                int maxIndex = minHarmonic;

                for (int i = minHarmonic; i <= maxHarmonic; i++)
                {
                    double magnitude = complexData[i].Magnitude;
                    if (magnitude > maxMagnitude)
                    {
                        maxMagnitude = magnitude;
                        maxIndex = i;
                    }
                }

                bool result;

                if (maxIndex >= expectedHarmonic - 1 && maxIndex <= expectedHarmonic + 1)
                {
                    result = false;
                }
                else
                {
                    result = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "AnalyzeUnderfillFFT - ошибка при Фурье-анализе");
                return false;
            }
        }

        /// <summary>
        /// Получение усредненной яркости по строкам развертки
        /// </summary>
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

        /// <summary>
        /// Получение развертки изображения
        /// </summary>
        private static void GetStripeImg(Mat src, Mat dst, double[] sinTable, double[] cosTable,
                                         Point center, int radius, int w, int h, int width, int height)
        {
            const int INNER_OFFSET = 7; // Можно сделать параметром

            int hr = radius - height;

            for (int j = 0; j < height; j++)
            {
                int r = hr + j + INNER_OFFSET;
                for (int i = 0; i < width; i++)
                {
                    double x = sinTable[i] * r + center.X;
                    double y = cosTable[i] * r + center.Y;

                    // Билинейная интерполяция
                    dst.Set<byte>(j, i, Bilinear8Bit(src, x, y, w, h));
                }
            }
        }

        /// <summary>
        /// Билинейная интерполяция для 8-битных изображений
        /// </summary>
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

            Point[] contour;

            if (_isBlackOrBrown)
            {
                contour = GetBlackOrBrownContour(gray, image);
                return CorrectContour(contour, _contourCorrectionBlackOrBrown);
            }  
            else
            {
                contour = GetColorCapContour(gray, image);
                return CorrectContour(contour, _contourCorrectionColor);
            }
        }

        #region Цветные крышки
        private Point[] GetColorCapContour(Mat gray, Mat image)
        {
            if (gray.Empty() || image.Empty())
                return null;

            Mat sat = ApplySaturationStep(image, _saturationColor);

            Mat caps = ApplyCapsColorStep(
                sat,
                _capsColor,
                _isColored,
                _isYellowCap,
                _isGreenColor
            );

            Mat[] channels = ApplyWindowStep(caps, window);

            ApplyMorphologyStep(channels, element1, element2);

            return GetMaxContour(channels[2]);
        }

        private Mat ApplySaturationStep(Mat input, int saturation)
        {
            return SimulateCameraSaturation(input, saturation);
        }

        private Mat ApplyCapsColorStep(Mat input, byte capsColor, bool colored, bool yellow, bool green)
        {
            Mat result = input.Clone();
            NonlinearBackgroundDecolorization(result, capsColor, colored, yellow, green);
            return result;
        }

        private Mat[] ApplyWindowStep(Mat input, int window)
        {
            Mat[] channels;
            Cv2.Split(input, out channels);

            Cv2.GaussianBlur(
                channels[0],
                channels[1],
                new Size(window, window),
                4
            );

            return channels;
        }

        private void ApplyMorphologyStep(Mat[] channels, Mat element1, Mat element2)
        {
            Cv2.Threshold(
                channels[1],
                channels[0],
                128,
                255,
                ThresholdTypes.Otsu | ThresholdTypes.Binary
            );

            Cv2.MorphologyEx(channels[0], channels[1], MorphTypes.Dilate, element1);
            Cv2.MorphologyEx(channels[1], channels[2], MorphTypes.Erode, element2);

            Cv2.MedianBlur(channels[2], channels[2], 5);
        }

        private Point[] GetMaxContour(Mat image)
        {
            Cv2.FindContours(
                image,
                out Point[][] contours,
                out _,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple
            );

            if (contours.Length == 0)
                return null;

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

            return contours[maxInd];
        }
        #endregion

        #region методы для черных крышек
        private Point[] GetBlackOrBrownContour(Mat grayInput, Mat image)
        {
            if (image.Empty())
                return null;

            Mat sat = ApplySaturation(image, _saturationBlackOrBrown);

            Mat gray = ToGray(sat);

            Mat blurred = ApplyMedian(gray, _medianFilter);

            Mat edges = ApplyCanny(blurred, _cannyThreshold);

            return ContourToEllipse(edges);
        }

        private Mat ApplySaturation(Mat image, int saturation)
        {
            return SimulateCameraSaturation(image, saturation);
        }

        private Mat ToGray(Mat image)
        {
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            return gray;
        }

        private Mat ApplyMedian(Mat image, int size)
        {
            Mat result = image.Clone();
            Cv2.MedianBlur(result, result, size);
            return result;
        }

        private Mat ApplyCanny(Mat image, int threshold)
        {
            Mat result = image.Clone();
            Cv2.Canny(result, result, threshold, 255);
            return result;
        }

        private Point[] ContourToEllipse(Mat image)
        {
            Cv2.FindContours(image, out Point[][] contours, out _,
                RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            var allPoints = new List<Point>();
            foreach (var cnt in contours)
                allPoints.AddRange(cnt);

            if (allPoints.Count < 5)
                return null;

            Point[] hull = Cv2.ConvexHull(allPoints.ToArray());

            if (hull.Length < 5)
                return hull;

            RotatedRect ellipse = Cv2.FitEllipse(hull);

            return Cv2.Ellipse2Poly(
                (Point)ellipse.Center,
                new Size((int)(ellipse.Size.Width / 2), (int)(ellipse.Size.Height / 2)),
                (int)ellipse.Angle,
                0,
                360,
                1
            );
        }
        #endregion


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
            return circularity > _inclusionThreshold;
        }

        #endregion

        #region Методы управления приложением

        private async Task StopProcessingAsync()
        {
            try
            {
                _processingCts?.Cancel();

                recognizeButton.Text = "Остановка...";
                recognizeButton.Enabled = false;

                try
                {
                    if (_processingTask != null)
                        await _processingTask;
                }
                catch (OperationCanceledException)
                {
                    // Норма, ничего не делаем
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "Ошибка в StopProcessingAsync во время остановки обработки");
                }

                // Если есть подключение, отправляем стоп-сигнал
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    if (!_isImageLoaded)
                    {
                        try
                        {
                            _modbusClient.WriteRegister(_startRecognizeProcessingRegister, (ushort)_recognizeProcessingFinish);
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.Log(ex, "Ошибка в StopProcessingAsync при отправке стоп-сигнала в ПР");
                        }
                    }
                }
            }
            finally
            {
                _uiTimer?.Stop();

                _isProcessing = false;

                recognizeButton.Text = "Начать анализ";
                recognizeButton.BackColor = Color.FromArgb(4, 85, 191);
                recognizeButton.Enabled = true;

                if (!_isImageLoaded)
                {
                    startStreamButton.Enabled = true;
                }
                if (_isImageLoaded)
                {
                    loadImageButton.Enabled = true;
                }

                _processingCts?.Dispose();
                _processingCts = null;
            }
        }


        private void StartProcessing()
        {
            try
            {
                if (_modbusClient == null || !_modbusClient.Connected)
                {
                    MessageBox.Show(
                        "ПР205 не подключено, отбраковка не будет происходить.",
                        "Предупреждение",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
                else
                {
                    if (!_isImageLoaded)
                    {
                        // Отправляем сигнал на ПР
                        _modbusClient.WriteRegister(_startRecognizeProcessingRegister, (ushort)_recognizeProcessingStart);

                    }
                }

                _settings = ReadSettingsFromUi();

                _processingCts = new CancellationTokenSource();

                _processingTask = Task.Run(() => ProcessingLoop(_processingCts.Token));

                _isProcessing = true;

                recognizeButton.Text = "Остановить анализ";
                recognizeButton.BackColor = Color.FromArgb(229, 115, 115);

                startStreamButton.Enabled = false;

                if (_isImageLoaded)
                {
                    loadImageButton.Enabled = false;
                }

                StartUiLoop();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в StartProcessing");

                _processingCts?.Dispose();
                _processingCts = null;
            }
        }

        private void ApplyCameraSettings()
        {
            try
            {
                if (_cam == null)
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
                if (_isStreamCam == true)
                    StopStream();

                // ---------- 3. Применяем настройки камеры ----------
                _cam.Width = width;
                _cam.Height = height;
                _cam.ExposureTime = exposure;
                _cam.Saturation = saturation;

                _cam.SetWidth();
                _cam.SetHeight();
                _cam.SetExposureTime();
                _cam.SetSaturation();

                // ---------- 4. Возобновляем стрим ----------
                if (_isStreamCam == false)
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
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    try
                    {
                        _modbusClient.WriteRegister(_startRecognizeProcessingRegister, (ushort)_recognizeProcessingFinish);
                        _modbusClient.WriteRegister(_breakerAllowRegister, (ushort)_breakerAllowFalse);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при отправке сигнала завершения в ПР205: {ex.Message}");
                    }
                }

                // --- Закрываем камеру ---
                if (_cam != null && _cameraConnected)
                {
                    try
                    {
                        if (_cam.Streamed)
                            _cam.EndStream();
                        _cam.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при закрытии камеры: {ex.Message}");
                    }
                    finally
                    {
                        _cam = null;
                        _cameraConnected = false;
                    }
                }

                // --- Отключаем modbus ---
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    try
                    {
                        _modbusClient.Disconnect();
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

        #region Вспомогательные методы

        private void ovalityCoefNumUpD_ValueChanged(object sender, EventArgs e)
        {
            _ovalityThreshold = (double)ovalityCoefNumUpD.Value;
        }

        private void circleCoefNumUpD_ValueChanged(object sender, EventArgs e)
        {
            _inclusionThreshold = (double)circleCoefNumUpD.Value;
        }

        private void coefCapRadiusInclusionUpD_ValueChanged(object sender, EventArgs e)
        {
            _coefCapRadiusInclusion = (double)coefCapRadiusInclusionUpD.Value;
        }

        private void minSquareInclusionNumUpD_ValueChanged(object sender, EventArgs e)
        {
            _minAreaInclusion = (double)minSquareInclusionNumUpD.Value;
        }

        private void maxSquareInclusionNumUpD_ValueChanged(object sender, EventArgs e)
        {
            _maxAreaInclusion = (double)maxSquareInclusionNumUpD.Value;
        }

        private void minSquareInpaintNumUpD_ValueChanged(object sender, EventArgs e)
        {
            _minAreaInpaintDefect = (double)minSquareInpaintNumUpD.Value;
        }

        private void whiteThresoldNumUpD_ValueChanged(object sender, EventArgs e)
        {
            _minInpaintWhiteThreshold = (double)whiteThresoldNumUpD.Value;
        }

        private void obloyPixCountNumUpD_ValueChanged(object sender, EventArgs e)
        {
            _minAreaObloy = (double)obloyPixCountNumUpD.Value;
        }

        private void countCorrugationsNumUpD_ValueChanged(object sender, EventArgs e)
        {
            _corrugationsCountForUnderFill = (double)countCorrugationsNumUpD.Value;
        }

        private void coefCapRadiusMaskUnderFillNumUpD_ValueChanged(object sender, EventArgs e)
        {
            _coefCapRadiusUnderFill = (double)coefCapRadiusMaskUnderFillNumUpD.Value;
        }

        private void LoadSettings()
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    openFileDialog.Title = "Загрузить настройки";
                    openFileDialog.InitialDirectory = _folderParamCamera;

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
            if (!_isStreamCam || img == null || img.Empty())
                return;

            try
            {
#if OLD_FRAME_PROCESSING
        try
        {
            lock (frameLock)
            {
                latestFrame?.Dispose();
                latestFrame = img.Clone();
                newFrameAvailable = true;

                if (isProcessing)
                    currentFrameNumber++;
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "Error in OLD_FRAME_PROCESSING block");
        }
#else
                if (_isProcessing)
                {
                    try
                    {
                        _img1 = img.Clone();
                        _imageQueue.Put(img.Clone());
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "Ошибка при добавлении кадра в очередь");
                    }
                }
#endif
                else
                {
                    try
                    {
                        _img1 = img.Clone();
                        UpdatePictureBox(originPb, img);
                        //CycleImageSaver.SaveDuplicate(img, 0);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "Ошибка при отображении кадра");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Общая ошибка в GetImage");
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
                        if (!_cameraError1)
                        {
                            if (_cam != null)
                            {
                                if (_cam.Streamed)
                                {
                                    _cam.EndStream();
                                }
                            }
                        }
                    }
                    return;
                }

                _isFirstImageCam1 = false;

                if (!isInit && !LocalSettings.Instance.UseModule)
                {
                    if (!_cameraError1)
                    {
                        if (_cam != null)
                        {
                            if (!_cam.Streamed)
                            {
                                _cam.StartStream();
                            }
                        }
                    }
                    _img1 = new Mat();
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
                                    CycleImageSaver.SaveDuplicate(first, _generalCapsCount);
                                    CycleImageSaver.SaveDuplicate(second, _generalCapsCount);
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
                        if (_isStreamCam)
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
                        else if (_isProcessingFromFolder)
                        {
                            await Task.Delay(100);

                            if (_imageFiles.Count == 0) continue;

                            try
                            {
                                frameToProcess = new Mat(_imageFiles[_currentImageIndex]);
                                _currentImageIndex = (_currentImageIndex + 1) % _imageFiles.Count;
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

                            if (IsDuplicateFrameByRows(frameToProcess))
                                continue;

                            try
                            {
                                // === PRE-PROCESS ===
                                Cv2.CvtColor(frameToProcess, gray, ColorConversionCodes.BGR2GRAY);
                                Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

                                frameToProcess.CopyTo(_frameToDisplay);

                                Point[] capContour = GetCapContour(gray, frameToProcess);

                                _generalCapsCount++;
                                UpdateTextBox(generalCapsCountTb, _generalCapsCount, 0);

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

                                if (underFillCb.Checked)
                                {
                                    frameToProcess.CopyTo(_imageForUnderfill);
                                    gray.CopyTo(_grayForUnderfill);
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
                                        var underFillTask = Task.FromResult(false);

                                        if (ovalityCB.Checked)
                                            ovalityTask = RunCheckWithTimeout(_grayForOvality, _imageForOvality, _frameToDisplay, timeoutCts.Token, RunCheckOvality, capContour);

                                        if (inclusionCB.Checked)
                                            inclusionsTask = RunCheckWithTimeout(_grayForInclusions, _imageForInclusions, _frameToDisplay, timeoutCts.Token, RunCheckForInclusions, capContour);

                                        if (inpaintCB.Checked)
                                            paintTask = RunCheckWithTimeout(_grayForPaintDefects, _imageForPaintDefects, _frameToDisplay, timeoutCts.Token, RunCheckForPaintDefects, capContour);

                                        if (obloyCB.Checked)
                                            obloyTask = RunCheckWithTimeout(_grayForObloyDefects, _imageForObloyDefects, _frameToDisplay, timeoutCts.Token, RunCheckForObloyDefects, capContour);

                                        if (underFillCb.Checked)
                                            underFillTask = RunCheckWithTimeout(_grayForUnderfill, _imageForUnderfill, _frameToDisplay, timeoutCts.Token, RunCheckForUnderFillDefects, capContour);


                                        await Task.WhenAll(ovalityTask, inclusionsTask, paintTask, obloyTask);

                                        bool anyDefect =
                                            (ovalityCB.Checked && ovalityTask.Result) ||
                                            (inclusionCB.Checked && inclusionsTask.Result) ||
                                            (inpaintCB.Checked && paintTask.Result) ||
                                            (obloyCB.Checked && obloyTask.Result) ||
                                            (underFillCb.Checked && underFillTask.Result);

                                        // === сбор типов дефектов ===
                                        List<string> defects = new();

                                        if (ovalityCB.Checked && ovalityTask.Result) defects.Add("Овальность");
                                        if (inclusionCB.Checked && inclusionsTask.Result) defects.Add("Вкрапление");
                                        if (inpaintCB.Checked && paintTask.Result) defects.Add("Непрокрас");
                                        if (obloyCB.Checked && obloyTask.Result) defects.Add("Облой");
                                        if (underFillCb.Checked && underFillTask.Result) defects.Add("Недолив");

                                        string defectText = defects.Count > 0 ? string.Join(", ", defects) : "-";

                                        // === счётчики ===
                                        if (anyDefect)
                                        {
                                            _ngCapsCount++;
                                            _percentNgCaps = _generalCapsCount > 0 ? _ngCapsCount / _generalCapsCount * 100 : 0;
                                            UpdateTextBox(ngCapsCountTb, _ngCapsCount, 0);
                                            UpdateTextBox(percentNgCapsTb, _percentNgCaps);
                                        }
                                        else
                                        {
                                            _okCapsCount++;
                                            _percentOkCaps = _generalCapsCount > 0 ? _okCapsCount / _generalCapsCount * 100 : 0;
                                            _percentNgCaps = _generalCapsCount > 0 ? _ngCapsCount / _generalCapsCount * 100 : 0;

                                            UpdateTextBox(okCapsCountTb, _okCapsCount, 0);
                                            UpdateTextBox(percentOkCapsTb, _percentOkCaps);
                                            UpdateTextBox(percentNgCapsTb, _percentNgCaps);
                                        }

                                        // === имя файла как в CycleImageSaver ===
                                        string fileName =
                                            $"{(anyDefect ? "NG" : "OK")}_{DateTime.Now:dd.MM.yyyy_HH-mm-ss_fff}_{_generalCapsCount}.jpg";

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
                                                    generalCount: _generalCapsCount,
                                                    fileName
                                                );
                                            }
                                            catch (Exception ex)
                                            {
                                                ErrorLogger.Log(ex, "Ошибка при сохранении изображения в CycleImageSaver");
                                            }
                                            finally { copy.Dispose(); }
                                        });

                                        string folder = Path.Combine(CycleImageSaver.CurrentCycleFolder);

                                        // === запись статистики ===
                                        try
                                        {
                                            _statisticsManager.Add(
                                                new CapStatistics
                                                {
                                                    Number = _generalCapsCount,
                                                    IsNg = anyDefect,
                                                    Defects = defectText,
                                                    SaveFolder = folder,
                                                    ImageName = fileName
                                                },
                                                okCapsSaveCb.Checked, // передаём состояние чекбокса для OK
                                                ngCapsSaveCb.Checked  // передаём состояние чекбокса для NG
                                            );
                                        }
                                        catch (Exception ex)
                                        {
                                            ErrorLogger.Log(ex, "Ошибка при добавлении записи в статистику");
                                        }

                                        BeginInvoke(() => currentFolderTb.Text = CycleImageSaver.CurrentCycleFolder);

                                        // === Передача результата в ПЛК ===
                                        PLCData.QualityStatus st = anyDefect
                                            ? PLCData.QualityStatus.Bad
                                            : PLCData.QualityStatus.Good;

                                        if (!_isProcessingFromFolder)
                                        {
                                            _ = Task.Run(() =>
                                            {
                                                try { SendQualityStatus(st); }
                                                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка при отправке статуса качества в ПЛК"); }
                                            });
                                        }

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
                                                    UpdatePictureBox(originColorReceptParamSmallPb, _imageOriginReceptParam);
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

        private async Task ProcessingLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Mat frameToProcess = null;

                    try
                    {
                        // ===== Получение кадра =====
                        if (_isStreamCam)
                        {
#if OLD_FRAME_PROCESSING
                    lock (frameLock)
                    {
                        if (!newFrameAvailable)
                            continue;

                        frameToProcess = latestFrame.Clone();
                        newFrameAvailable = false;
                    }
#else
                            frameToProcess = _imageQueue.Get(token);
#endif
                        }
                        else if (_isProcessingFromFolder)
                        {
                            await Task.Delay(100, token);

                            if (_imageFiles.Count == 0)
                                continue;

                            try
                            {
                                frameToProcess = new Mat(_imageFiles[_currentImageIndex]);
                                _currentImageIndex = (_currentImageIndex + 1) % _imageFiles.Count;
                            }
                            catch (Exception ex)
                            {
                                ErrorLogger.Log(ex, "Ошибка загрузки изображения");
                                continue;
                            }
                        }

                        if (frameToProcess == null || frameToProcess.Empty())
                            continue;

                        using (frameToProcess)
                        using (Mat gray = new Mat())
                        {
                            try
                            {
                                var stopwatch = Stopwatch.StartNew();

                                if (IsDuplicateFrameByRows(frameToProcess))
                                    continue;

                                Cv2.CvtColor(frameToProcess, gray, ColorConversionCodes.BGR2GRAY);

                                frameToProcess.CopyTo(_frameToDisplay);

                                Point[] capContour = GetCapContour(gray, frameToProcess);

                                bool anyDefect = false;
                                List<string> defects = new();

                                var settings = _settings;

                                // === Подготовка изображений по категориям ===
                                if (settings.Ovality)
                                {
                                    frameToProcess.CopyTo(_imageForOvality);
                                    gray.CopyTo(_grayForOvality);
                                }

                                if (settings.Inclusion)
                                {
                                    frameToProcess.CopyTo(_imageForInclusions);
                                    gray.CopyTo(_grayForInclusions);
                                }

                                if (settings.Inpaint)
                                {
                                    frameToProcess.CopyTo(_imageForPaintDefects);
                                    gray.CopyTo(_grayForPaintDefects);
                                }

                                if (settings.Obloy)
                                {
                                    frameToProcess.CopyTo(_imageForObloyDefects);
                                    gray.CopyTo(_grayForObloyDefects);
                                }

                                if (settings.UnderFill)
                                {
                                    frameToProcess.CopyTo(_imageForUnderfill);
                                    gray.CopyTo(_grayForUnderfill);
                                }

                                using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token))
                                {
                                    timeoutCts.CancelAfter(60);

                                    var ovalityTask = Task.FromResult(false);
                                    var inclusionsTask = Task.FromResult(false);
                                    var paintTask = Task.FromResult(false);
                                    var obloyTask = Task.FromResult(false);
                                    var underFillTask = Task.FromResult(false);

                                    if (settings.Ovality)
                                        ovalityTask = RunCheckWithTimeout(_grayForOvality, _imageForOvality, _frameToDisplay, timeoutCts.Token, RunCheckOvality, capContour);

                                    if (settings.Inclusion)
                                        inclusionsTask = RunCheckWithTimeout(_grayForInclusions, _imageForInclusions, _frameToDisplay, timeoutCts.Token, RunCheckForInclusions, capContour);

                                    if (settings.Inpaint)
                                        paintTask = RunCheckWithTimeout(_grayForPaintDefects, _imageForPaintDefects, _frameToDisplay, timeoutCts.Token, RunCheckForPaintDefects, capContour);

                                    if (settings.Obloy)
                                        obloyTask = RunCheckWithTimeout(_grayForObloyDefects, _imageForObloyDefects, _frameToDisplay, timeoutCts.Token, RunCheckForObloyDefects, capContour);

                                    if (settings.UnderFill)
                                        underFillTask = RunCheckWithTimeout(_grayForUnderfill, _imageForUnderfill, _frameToDisplay, timeoutCts.Token, RunCheckForUnderFillDefects, capContour);

                                    await Task.WhenAll(ovalityTask, inclusionsTask, paintTask, obloyTask, underFillTask);

                                    if (settings.Ovality && ovalityTask.Result) { anyDefect = true; defects.Add("Овальность"); }
                                    if (settings.Inclusion && inclusionsTask.Result) { anyDefect = true; defects.Add("Вкрапление"); }
                                    if (settings.Inpaint && paintTask.Result) { anyDefect = true; defects.Add("Непрокрас"); }
                                    if (settings.Obloy && obloyTask.Result) { anyDefect = true; defects.Add("Облой"); }
                                    if (settings.UnderFill && underFillTask.Result) { anyDefect = true; defects.Add("Недолив"); }
                                }

                                string defectText = defects.Count > 0 ? string.Join(", ", defects) : "-";

                                // === обновление состояния (НЕ UI) ===
                                long t = stopwatch.ElapsedMilliseconds;
                                lock (_stateLock)
                                {
                                    _state.Frame?.Dispose();
                                    _state.Frame = _frameToDisplay.Clone();

                                    _generalCapsCount++;
                                    if (anyDefect)
                                    {
                                        _ngCapsCount++;
                                    }
                                    else
                                    {
                                        _okCapsCount++;
                                    }

                                    _percentNgCaps = _generalCapsCount > 0 ? _ngCapsCount / _generalCapsCount * 100 : 0;
                                    _percentOkCaps = _generalCapsCount > 0 ? _okCapsCount / _generalCapsCount * 100 : 0;

                                    _state.GeneralCapsCount = _generalCapsCount;
                                    _state.Ok = _okCapsCount;
                                    _state.Ng = _ngCapsCount;
                                    _state.PercentNG = _percentNgCaps;
                                    _state.PercentOK = _percentOkCaps;

                                    _state.OvalityDefectCount = _ovalityDefectCount;
                                    _state.InclusionDefectCount = _inclusionDefectCount;
                                    _state.PaintDefectCount = _paintDefectCount;
                                    _state.ObloyDefectCount = _obloyDefectCount;
                                    _state.UnderFillDefectCount = _underFillDefectCount;

                                    _state.PercentOvality = _percentOvalityCaps;
                                    _state.PercentPaint = _percentInpaintCaps;
                                    _state.PercentInclusion = _percentInclusionCaps;
                                    _state.PercentObloy = _percentObloyCaps;
                                    _state.PercentUnderFill = _percentUnderfillCaps;

                                    _state.TimeOvality = _timeOvality;
                                    _state.TimeInclusion = _timeInclusion;
                                    _state.TimePaint = _timeInpaint;
                                    _state.TimeObloy = _timeObloy;
                                    _state.TimeUnderFill = _timeUnderFill;

                                    _state.Time = t;
                                    _state.DefectText = defectText;
                                    _state.IsNg = anyDefect;
                                }

                                string defectSuffix = defects.Count > 0
                                    ? "_" + string.Join(", ", defects)
                                    : "";

                                string fileName = $"{(anyDefect ? "NG" : "OK")}_{DateTime.Now:dd.MM.yyyy_HH-mm-ss_fff}{defectSuffix}.png";

                                // === сохранение (без UI) ===
                                if (settings.SaveOk || settings.SaveNg)
                                {
                                    Mat copy = frameToProcess.Clone();

                                    _ = Task.Run(() =>
                                    {
                                        try
                                        {
                                            CycleImageSaver.Save(
                                                copy,
                                                anyDefect,
                                                settings.SaveOk,
                                                settings.SaveNg,
                                                _state.Ok + _state.Ng,
                                                fileName
                                            );
                                        }
                                        catch (Exception ex)
                                        {
                                            ErrorLogger.Log(ex, "Ошибка сохранения");
                                        }
                                        finally { copy.Dispose(); }
                                    });
                                }

                                // === PLC ===
                                if (!_isProcessingFromFolder)
                                {
                                    var st = anyDefect
                                        ? PLCData.QualityStatus.Bad
                                        : PLCData.QualityStatus.Good;

                                    _ = Task.Run(() =>
                                    {
                                        try { SendQualityStatus(st); }
                                        catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка PLC"); }
                                    });
                                }

                                stopwatch.Stop();
                            }
                            catch (Exception ex)
                            {
                                ErrorLogger.Log(ex, "Ошибка обработки кадра");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Log(ex, "Ошибка цикла обработки");
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Фатальная ошибка ProcessingLoop");
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
                var stopwatch = Stopwatch.StartNew();

                bool isOval = false;
                try
                {
                    isOval = CheckOvality(gray, image, drawFrame, token, capContour);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "Ошибка в CheckOvality");
                }

                if (isOval)
                {
                    _ovalityDefectCount++;
                }

                stopwatch.Stop();
                _timeOvality = stopwatch.ElapsedMilliseconds;

                _percentOvalityCaps = _generalCapsCount > 0 ? _ovalityDefectCount / _generalCapsCount * 100 : 0;

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
                var stopwatch = Stopwatch.StartNew();

                bool isInclusion = false;
                try
                {
                    isInclusion = CheckForInclusions(gray, image, drawFrame, token, capContour);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "Ошибка в CheckForInclusions");
                }

                if (isInclusion)
                {
                    _inclusionDefectCount++;
                }

                stopwatch.Stop();
                _timeInclusion = stopwatch.ElapsedMilliseconds;

                _percentInclusionCaps = _generalCapsCount > 0 ? _inclusionDefectCount / _generalCapsCount * 100 : 0;

                return isInclusion;
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
                var stopwatch = Stopwatch.StartNew();

                bool isInpaint = false;
                try
                {
                    isInpaint = CheckForPaintDefects(gray, image, drawFrame, token, capContour);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "Ошибка в CheckForPaintDefects");
                }

                if (isInpaint)
                {
                    _paintDefectCount++;
                }

                stopwatch.Stop();
                _timeInpaint = stopwatch.ElapsedMilliseconds;

                _percentInpaintCaps = _generalCapsCount > 0 ? _paintDefectCount / _generalCapsCount * 100 : 0;

                return isInpaint;
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
                var stopwatch = Stopwatch.StartNew();

                bool isObloy = false;
                try
                {
                    isObloy = CheckForObloyDefects(gray, image, drawFrame, token, capContour);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "Ошибка в CheckForObloyDefects");
                }

                if (isObloy)
                {
                    _obloyDefectCount++;
                }

                stopwatch.Stop();
                _timeObloy = stopwatch.ElapsedMilliseconds;

                _percentObloyCaps = _generalCapsCount > 0 ? _obloyDefectCount / _generalCapsCount * 100 : 0;

                return isObloy;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в RunCheckForObloyDefects");
                return false;
            }
        }

        private bool RunCheckForUnderFillDefects(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Point[] capContour)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var stopwatch = Stopwatch.StartNew();

                bool isUnderFill = false;
                try
                {
                    isUnderFill = CheckForUnderFillDefects(gray, image, drawFrame, token, capContour);
                }
                catch (Exception ex)
                {
                    ErrorLogger.Log(ex, "Ошибка в CheckForUnderFillDefects");
                }

                if (isUnderFill)
                {
                    _underFillDefectCount++;
                }

                stopwatch.Stop();
                _timeUnderFill = stopwatch.ElapsedMilliseconds;

                _percentUnderfillCaps = _generalCapsCount > 0 ? _underFillDefectCount / _generalCapsCount * 100 : 0;

                return isUnderFill;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка в RunCheckForUnderFillDefects");
                return false;
            }
        }
        #endregion

        #region Методы работы с Modbus
        private void SendQualityStatus(PLCData.QualityStatus qualityStatus)
        {
            try
            {
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    _modbusClient.WriteRegister(PLCData.QualityRegisterModbus, (ushort)qualityStatus);
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
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    _modbusClient.WriteRegister(_breakingTimeRegister, (ushort)breakingTime);
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
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    _modbusClient.WriteRegister(_cameraOffsetRegister, (ushort)cameraOffset);
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
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    _modbusClient.WriteRegister(_breakerOffsetRegister, (ushort)breakerOffset);
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
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    int valueToSend = breakingAllowCb.Checked
                        ? (int)_breakerAllowTrue
                        : (int)_breakerAllowFalse;

                    _modbusClient.WriteRegister(_breakerAllowRegister, (ushort)valueToSend);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка при отправке сигнала на ПР205");
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
            if (_imageFiles.Count == 0) return;

            try
            {
                // Отображаем текущее изображение
                using Mat imageFromFile = new(_imageFiles[_currentImageIndex]);
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
                    pictureBox.BeginInvoke(new Action(() =>
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
                textBox.BeginInvoke(new Action(() =>
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
                textBox.BeginInvoke(new Action(() =>
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

        #region Методы цветовой обработки

        public static void NonlinearBackgroundDecolorization(Mat img, byte nWhite, bool isColored, bool isYellowCap, bool isGreenColor)
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
            if (_img1 == null || _img1.Empty())
            {
                MessageBox.Show($"Нет изображения от камеры. Нажмите '{startStreamButton.Text}', чтобы загрузить изображения для тестирования параметров дефектов.");
                return;
            }

            _imageForTest?.Dispose();
            _imageForTest = _img1.Clone();

            _img1 = null;

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
                //ApplyRecognitionParameters();

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
                Mat frameO = null, frameI = null, frameP = null, frameOb = null, frameUf = null;
                Mat grayO = null, grayI = null, grayP = null, grayOb = null, grayUf = null;

                if (ovalityCB.Checked) { frameO = frameBase.Clone(); grayO = grayBase.Clone(); }
                if (inclusionCB.Checked) { frameI = frameBase.Clone(); grayI = grayBase.Clone(); }
                if (inpaintCB.Checked) { frameP = frameBase.Clone(); grayP = grayBase.Clone(); }
                if (obloyCB.Checked) { frameOb = frameBase.Clone(); grayOb = grayBase.Clone(); }
                if (underFillCb.Checked) { frameUf = frameBase.Clone(); grayUf = grayBase.Clone(); }

                // Запуск проверок
                CancellationToken fake = CancellationToken.None;
                bool oval = false, incl = false, paint = false, obloy = false, underFill = false;

                try { if (ovalityCB.Checked) oval = CheckOvality(grayO, frameO, finalFrame, fake, contour); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки овальности"); }

                try { if (inclusionCB.Checked) incl = CheckForInclusions(grayI, frameI, finalFrame, fake, contour); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки включений"); }

                try { if (inpaintCB.Checked) paint = CheckForPaintDefects(grayP, frameP, finalFrame, fake, contour); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки дефектов краски"); }

                try { if (obloyCB.Checked) obloy = CheckForObloyDefects(grayOb, frameOb, finalFrame, fake, contour); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки облоя"); }

                try { if (underFillCb.Checked) underFill = CheckForUnderFillDefects(grayUf, frameUf, finalFrame, fake, contour); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки недолива"); }

                using (Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(finalFrame))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        Font font = new Font("Arial", 18, FontStyle.Bold);
                        Brush brush = Brushes.Red;

                        int margin = 10;
                        int y = 10;

                        void DrawRight(string text)
                        {
                            SizeF size = g.MeasureString(text, font);
                            float x = bmp.Width - size.Width - margin;
                            g.DrawString(text, font, brush, x, y);
                            y += 40;
                        }

                        if (oval) DrawRight("ОВАЛЬНОСТЬ");
                        if (incl) DrawRight("ВКРАПЛЕНИЕ");
                        if (paint) DrawRight("НЕПРОКРАС");
                        if (obloy) DrawRight("ОБЛОЙ");
                        if (underFill) DrawRight("НЕДОЛИВ");
                    }

                    testingResultPb.Image?.Dispose();
                    testingResultPb.Image = (Bitmap)bmp.Clone();
                }

                // Очистка
                frameO?.Dispose();
                frameI?.Dispose();
                frameP?.Dispose();
                frameOb?.Dispose();
                frameUf?.Dispose();
                grayO?.Dispose();
                grayI?.Dispose();
                grayP?.Dispose();
                grayOb?.Dispose();
                grayUf?.Dispose();
                frameBase.Dispose();
                grayBase.Dispose();
                finalFrame.Dispose();
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
            Properties.Settings.Default.Settings_LastCycleTime = hours.ToString();
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

            Point[] contour;

            if (_recipeIsBlackOrBrown)
            {
                contour = ProcessBlackOrBrown();

                contour = CorrectContour(contour,(float)contourCorrectionBlackOrBrownUpDown.Value);

                Mat result = DrawContour(contour);

                contourCorrectionBlackOrBrownReceptParamSmallPb.Image = BitmapConverter.ToBitmap(result);
                resultContourBlackOrBrownSmallPb.Image = BitmapConverter.ToBitmap(result);

                ShowInGeneralPreview(resultContourBlackOrBrownSmallPb, generalReceptParamPb);
            }
            else
            {
                contour = ProcessColor();

                contour = CorrectContour(
                    contour,
                    (float)contourCorrectionColorUpDown.Value
                );

                Mat result = DrawContour(contour);

                contourCorrectionColorReceptParamSmallPb.Image = BitmapConverter.ToBitmap(result);
                resultContourColorSmallPb.Image = BitmapConverter.ToBitmap(result);

                ShowInGeneralPreview(resultContourColorSmallPb, generalReceptParamPb);
            }
        }

        private Point[] ProcessBlackOrBrown()
        {
            Mat satImg = ApplySaturation(_imageOriginReceptParam,(int)saturationBlackOrBrownUpDown.Value);

            saturationBlackOrBrownReceptParamSmallPb.Image = BitmapConverter.ToBitmap(satImg);

            Mat gray = ToGray(satImg);

            Mat blurred = ApplyMedian(gray, (int)medianFilterUpDown.Value);
            medianFilterBlackOrBrownReceptParamSmallPb.Image = BitmapConverter.ToBitmap(blurred);

            Mat edges = ApplyCanny(blurred, (int)cannyUpDown.Value);
            cannyBlackOrBrownReceptParamSmallPb.Image = BitmapConverter.ToBitmap(edges);

            return ContourToEllipse(edges);
        }


        private Point[] ProcessColor()
        {
            if (_imageOriginReceptParam == null || _imageOriginReceptParam.Empty())
                return null;

            // ---------------- 1. Saturation --------------------
            Mat satImg = ApplySaturationStep(
                _imageOriginReceptParam,
                (int)saturationColorUpDown.Value
            );

            saturationColorReceptParamSmallPb.Image =
                BitmapConverter.ToBitmap(satImg);

            // ---------------- 2. CapsColor ----------------------
            Mat capsImg = ApplyCapsColorStep(
                satImg,
                (byte)capcolorUpDown.Value,
                _recipeIsColored,
                _recipeIsYellowCap,
                _recipeIsGreenColor
            );

            capscolorColorReceptParamSmallPb.Image =
                BitmapConverter.ToBitmap(capsImg);

            // ---------------- 3. Window -------------------------
            Mat[] channels = ApplyWindowStep(
                capsImg,
                int.Parse(windowCb.Text)
            );

            windowColorReceptParamSmallPb.Image =
                BitmapConverter.ToBitmap(channels[1]);

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

            ApplyMorphologyStep(channels, element1, element2);

            Mat morphImg = channels[2];

            morphColorReceptParamSmallPb.Image =
                BitmapConverter.ToBitmap(morphImg);

            // ---------------- 5. Contour -------------------------
            return GetMaxContour(morphImg);
        }

        private Point[] CorrectContour(Point[] contour, float threshold)
        {
            if (contour == null || contour.Length < 5)
                return contour;

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

                if (norm > threshold)
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

        private Mat DrawContour(Point[] contour)
        {
            Mat img = _imageOriginReceptParam.Clone();

            if (contour != null)
            {
                Cv2.Polylines(img, new[] { contour }, true, Scalar.Red, 2);
            }

            return img;
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

        private void contourCorrectionColorUpDown_ValueChanged(object sender, EventArgs e)
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
                    originColorReceptParamSmallPb.Image = BitmapConverter.ToBitmap(_imageOriginReceptParam);
                    originBlackOrBrownReceptParamSmallPb.Image = BitmapConverter.ToBitmap(_imageOriginReceptParam);
                    RecomputeAll();
                }
            }
        }

        private void findContourCreateReceptBt_Click(object sender, EventArgs e)
        {
            RecomputeAll();
        }

        private void originColorReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(originColorReceptParamSmallPb, generalReceptParamPb);
        }

        private void saturationColorReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(saturationColorReceptParamSmallPb, generalReceptParamPb);
        }

        private void capscolorReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(capscolorColorReceptParamSmallPb, generalReceptParamPb);
        }

        private void windowReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(windowColorReceptParamSmallPb, generalReceptParamPb);
        }

        private void morphReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(morphColorReceptParamSmallPb, generalReceptParamPb);
        }

        private void contourCorrectionColorReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(contourCorrectionColorReceptParamSmallPb, generalReceptParamPb);
        }

        private void resultContourSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(resultContourColorSmallPb, generalReceptParamPb);
        }

        // Вспомогательный метод
        private void ShowInGeneralPreview(PictureBox smallPb, PictureBox generalPb)
        {
            if (smallPb.Image != null)
            {
                generalPb.Image?.Dispose(); // освобождаем предыдущий Image
                generalPb.Image = (System.Drawing.Image)smallPb.Image.Clone();
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
            string folder = _recipesFolder;
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
                CameraSaturation = (int)saturationColorUpDown.Value,
                ContourCorrectionColor = (float)contourCorrectionColorUpDown.Value,
                CameraSaturationBlackOrBrown = (int)saturationBlackOrBrownUpDown.Value,
                ContourCorrectionBlackOrBrown = (float)contourCorrectionBlackOrBrownUpDown.Value,
                MedianFilter = (int)medianFilterUpDown.Value,
                CannyThreshold = (int)cannyUpDown.Value,

                IsGreen = _recipeIsGreenColor,
                IsColored = _recipeIsColored,
                IsYellow = _recipeIsYellowCap,
                IsWhite = _recipeCapsAreWhite,
                IsBlackOrBrown = _recipeIsBlackOrBrown,
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

            if (contourCorrectionColorUpDown.Value < 0 || contourCorrectionColorUpDown.Value > 2)
            {
                error = "Корр. контура должна быть в диапазоне 1–2.";
                return false;
            }

            if (_isBlackOrBrown)
            {
                if (medianFilterUpDown.Value <= 0)
                {
                    error = "Медианный фильтр должен быть больше 0.";
                    return false;
                }

                if (cannyUpDown.Value < 0)
                {
                    error = "Порог Canny должен быть больше 0.";
                    return false;
                }

                if (contourCorrectionBlackOrBrownUpDown.Value < 0 || contourCorrectionBlackOrBrownUpDown.Value > 2)
                {
                    error = "Коррекция контура (черные) должна быть 0–2.";
                    return false;
                }
            }

            // Все проверки прошли
            return true;
        }


        private void openCurReceptFolderBt_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(_recipesFolder))
                Directory.CreateDirectory(_recipesFolder);

            System.Diagnostics.Process.Start("explorer.exe", _recipesFolder);
        }

        private void isGreenCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            _recipeIsGreenColor = isGreenCb.Checked;
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void isColorCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            _recipeIsColored = isColorCb.Checked;
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void isYellowCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            _recipeIsYellowCap = isYellowCb.Checked;
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void isWhiteCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            _recipeCapsAreWhite = isWhiteCb.Checked;
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void isBlackOrBrownCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            _recipeIsBlackOrBrown = isBlackOrBrownCb.Checked;
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void UpdateColorModeUI()
        {
            // ---------- WHITE ----------
            if (isWhiteCb.Checked)
            {
                isWhiteCb.Enabled = true;

                isColorCb.Checked = false;
                isGreenCb.Checked = false;
                isYellowCb.Checked = false;
                isBlackOrBrownCb.Checked = false;

                isColorCb.Enabled = false;
                isGreenCb.Enabled = false;
                isYellowCb.Enabled = false;
                isBlackOrBrownCb.Enabled = false;

                SelectTab(colorCapTabPage);

                // --- блок black параметров ---
                saturationBlackOrBrownUpDown.Enabled = false;
                medianFilterUpDown.Enabled = false;
                cannyUpDown.Enabled = false;
                contourCorrectionBlackOrBrownUpDown.Enabled = false;

                // --- включаем цветные ---
                saturationColorUpDown.Enabled = true;
                capcolorUpDown.Enabled = true;
                windowCb.Enabled = true;
                morphCb.Enabled = true;
                contourCorrectionColorUpDown.Enabled = true;

                return;
            }
            else
            {
                isColorCb.Enabled = true;
                isGreenCb.Enabled = true;
                isBlackOrBrownCb.Enabled = true;
            }

            // ---------- BLACK / BROWN ----------
            if (isBlackOrBrownCb.Checked)
            {
                isWhiteCb.Checked = false;
                isColorCb.Checked = false;
                isGreenCb.Checked = false;
                isYellowCb.Checked = false;

                isWhiteCb.Enabled = false;
                isColorCb.Enabled = false;
                isGreenCb.Enabled = false;
                isYellowCb.Enabled = false;

                SelectTab(blackOrBrownTabPage);

                // --- блок цветных параметров ---
                saturationColorUpDown.Enabled = false;
                capcolorUpDown.Enabled = false;
                windowCb.Enabled = false;
                morphCb.Enabled = false;
                contourCorrectionColorUpDown.Enabled = false;

                // --- включаем black параметры ---
                saturationBlackOrBrownUpDown.Enabled = true;
                medianFilterUpDown.Enabled = true;
                cannyUpDown.Enabled = true;
                contourCorrectionBlackOrBrownUpDown.Enabled = true;

                return;
            }
            else
            {
                // если не black → включаем цветные параметры
                saturationColorUpDown.Enabled = true;
                capcolorUpDown.Enabled = true;
                windowCb.Enabled = true;
                morphCb.Enabled = true;
                contourCorrectionColorUpDown.Enabled = true;
            }

            // ---------- COLOR ----------
            if (isColorCb.Checked)
            {
                isWhiteCb.Enabled = false;
                isBlackOrBrownCb.Enabled = false;

                isGreenCb.Enabled = true;

                SelectTab(colorCapTabPage);

                // --- блок black параметров ---
                saturationBlackOrBrownUpDown.Enabled = false;
                medianFilterUpDown.Enabled = false;
                cannyUpDown.Enabled = false;
                contourCorrectionBlackOrBrownUpDown.Enabled = false;
            }
            else
            {
                isGreenCb.Checked = false;
                isYellowCb.Checked = false;

                isGreenCb.Enabled = false;
                isWhiteCb.Enabled = true;
                isBlackOrBrownCb.Enabled = true;

                // если color выключен → black параметры можно
                saturationBlackOrBrownUpDown.Enabled = true;
                medianFilterUpDown.Enabled = true;
                cannyUpDown.Enabled = true;
                contourCorrectionBlackOrBrownUpDown.Enabled = true;
            }

            // ---------- GREEN ----------
            if (isGreenCb.Checked)
            {
                if (!isColorCb.Checked)
                    isColorCb.Checked = true;

                isYellowCb.Checked = false;
            }

            // ---------- YELLOW ----------
            if (isYellowCb.Checked)
            {
                isGreenCb.Checked = false;
                isGreenCb.Enabled = false;
            }
            else
            {
                if (isColorCb.Checked)
                    isGreenCb.Enabled = true;
            }

            // ---------- ДОСТУПНОСТЬ YELLOW (фикс бага) ----------
            isYellowCb.Enabled = isColorCb.Checked && !isGreenCb.Checked;

            // ---------- CAP COLOR LOCK ----------
            capcolorUpDown.Enabled = !isGreenCb.Checked;
        }

        private void SelectTab(TabPage page)
        {
            receptVisualisationTabControl.SelectedTab = page;
        }

        private void loadImageForCreateReceptFromCameraBt_Click(object sender, EventArgs e)
        {
            if (_img1 == null || _img1.Empty())
            {
                MessageBox.Show($"Нет изображения от камеры. Нажмите '{startStreamButton.Text}', чтобы загрузить изображения для создания рецепта.");
                return;
            }

            _imageOriginReceptParam?.Dispose();
            _imageOriginReceptParam = _img1.Clone();

            _img1 = null;

            originColorReceptParamSmallPb.Image?.Dispose();
            originColorReceptParamSmallPb.Image = BitmapConverter.ToBitmap(_imageOriginReceptParam);

            originBlackOrBrownReceptParamSmallPb.Image?.Dispose();
            originBlackOrBrownReceptParamSmallPb.Image = BitmapConverter.ToBitmap(_imageOriginReceptParam);
            RecomputeAll();
        }

        private void saturationBlackOrBrownUpDown_ValueChanged(object sender, EventArgs e)
        {
            RecomputeAll();
        }

        private void medianFilterUpDown_ValueChanged(object sender, EventArgs e)
        {
            RecomputeAll();
        }

        private void cannyUpDown_ValueChanged(object sender, EventArgs e)
        {
            RecomputeAll();
        }

        private void contourCorrectionBlackOrBrownUpDown_ValueChanged(object sender, EventArgs e)
        {
            RecomputeAll();
        }

        private void originBlackOrBrownReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(originBlackOrBrownReceptParamSmallPb, generalReceptParamPb);
        }

        private void medianFilterBlackOrBrownReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(medianFilterBlackOrBrownReceptParamSmallPb, generalReceptParamPb);
        }

        private void cannyBlackOrBrownReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(cannyBlackOrBrownReceptParamSmallPb, generalReceptParamPb);
        }

        private void contourCorrectionBlackOrBrownReceptParamSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(contourCorrectionBlackOrBrownReceptParamSmallPb, generalReceptParamPb);
        }

        private void resultContourBlackOrBrownSmallPb_Click(object sender, EventArgs e)
        {
            ShowInGeneralPreview(resultContourBlackOrBrownSmallPb, generalReceptParamPb);
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
            bool allowConnectButtons = admin && !_isStreamCam && !_isProcessing;

            pr205PortTb.Enabled = admin;
            pr205IpTb.Enabled = admin;
            connectPrButton.Enabled = allowConnectButtons;

            cameraIpTextBox.Enabled = admin;
            connectCameraButton.Enabled = allowConnectButtons;

            frameHeightNumUpD.Enabled = admin;
            frameWidthNumUpD.Enabled = admin;
            frameExposureNumUpD.Enabled = admin;
            frameSaturationNumUpD.Enabled = admin;

            applySettingsButton.Enabled = admin;
            cameraSettingsCmB.Enabled = admin;
            cameraSettingsNameTb.Enabled = admin;
            saveCameraSettingsNew.Enabled = admin;
        }
        #endregion

        #region Статистика
        /*private void statisticsDataGridView_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, что кликнули на валидную строку и на столбец с именем файла
            if (e.RowIndex < 0 || e.ColumnIndex != statisticsDataGridView.Columns["dg_nameCapImage"].Index)
                return;

            string fileName = statisticsDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();

            if (string.IsNullOrEmpty(fileName) || fileName == "Сохранение отключено")
                return;

            // Получаем путь к папке из столбца SaveFolder
            string folder = statisticsDataGridView.Rows[e.RowIndex].Cells["dg_saveFolder"].Value?.ToString();

            if (string.IsNullOrEmpty(folder) || folder == "Сохранение отключено")
                return;

            // Воссоздаем полный путь
            string fullPath = Path.Combine(CycleImageSaver.BaseFolder, folder, fileName);

            if (!File.Exists(fullPath))
            {
                MessageBox.Show($"Файл не найден:\n{fullPath}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Открываем файл с помощью стандартного просмотрщика
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть файл:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }*/
        #endregion

        #region Многопоточность
        private ProcessingSettings ReadSettingsFromUi()
        {
            return new ProcessingSettings
            {
                Ovality = ovalityCB.Checked,
                Inclusion = inclusionCB.Checked,
                Inpaint = inpaintCB.Checked,
                Obloy = obloyCB.Checked,
                UnderFill = underFillCb.Checked,
                SaveOk = okCapsSaveCb.Checked,
                SaveNg = ngCapsSaveCb.Checked
            };
        }

        private void AnySettingChanged(object sender, EventArgs e)
        {
            _settings = ReadSettingsFromUi();
        }

        private void StartUiLoop()
        {
            _uiTimer = new System.Windows.Forms.Timer();
            _uiTimer.Interval = 100;

            _uiTimer.Tick += UiTimer_Tick;

            _uiTimer.Start();
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            ResultState snapshot;

            lock (_stateLock)
            {
                snapshot = new ResultState
                {
                    Frame = _state.Frame?.Clone(),

                    GeneralCapsCount = _state.GeneralCapsCount,
                    Ok = _state.Ok,
                    Ng = _state.Ng,
                    PercentOK = _state.PercentOK,
                    PercentNG = _state.PercentNG,

                    OvalityDefectCount = _state.OvalityDefectCount,
                    InclusionDefectCount = _state.InclusionDefectCount,
                    PaintDefectCount = _state.PaintDefectCount,
                    ObloyDefectCount = _state.ObloyDefectCount,
                    UnderFillDefectCount = _state.UnderFillDefectCount,

                    PercentOvality = _state.PercentOvality,
                    PercentInclusion = _state.PercentInclusion,
                    PercentPaint = _state.PercentPaint,
                    PercentObloy = _state.PercentObloy,
                    PercentUnderFill = _state.PercentUnderFill,

                    TimeOvality = _state.TimeOvality,
                    TimeInclusion = _state.TimeInclusion,
                    TimeUnderFill = _state.TimeUnderFill,
                    TimeObloy = _state.TimeObloy,
                    TimePaint = _state.TimePaint,

                    Time = _state.Time,
                    IsNg = _state.IsNg,

                    DefectText = _state.DefectText,

                };
            }

            try
            {
                generalCapsCountTb.Text = snapshot.GeneralCapsCount.ToString();
                okCapsCountTb.Text = snapshot.Ok.ToString();
                ngCapsCountTb.Text = snapshot.Ng.ToString();
                percentOkCapsTb.Text = snapshot.PercentOK.ToString("F1");
                percentNgCapsTb.Text = snapshot.PercentNG.ToString("F1");

                ovalityDef.Text = snapshot.OvalityDefectCount.ToString();
                inclusionDef.Text = snapshot.InclusionDefectCount.ToString();
                inpaintDef.Text = snapshot.PaintDefectCount.ToString();
                obloyDef.Text = snapshot.ObloyDefectCount.ToString();
                underFillDef.Text = snapshot.UnderFillDefectCount.ToString();

                percentOvalityCapsTb.Text = snapshot.PercentOvality.ToString("F1");
                percentInclusionCapsTb.Text = snapshot.PercentInclusion.ToString("F1");
                percentInpaintCapsTb.Text = snapshot.PercentPaint.ToString("F1");
                percentObloyCapsTb.Text = snapshot.PercentObloy.ToString("F1");
                percentUnderFillCapsTb.Text = snapshot.PercentUnderFill.ToString("F1");

                ovalityTime.Text = snapshot.TimeOvality.ToString();
                inclusionTime.Text = snapshot.TimeInclusion.ToString();
                inpaintTime.Text = snapshot.TimePaint.ToString();
                obloyTime.Text = snapshot.TimeObloy.ToString();
                underFillTime.Text = snapshot.TimeUnderFill.ToString();

                generalTimeTb.Text = snapshot.Time.ToString();

                bool show =
                    _outputMode == OutputMode.All ||
                    (_outputMode == OutputMode.Good && !snapshot.IsNg) ||
                    (_outputMode == OutputMode.Bad && snapshot.IsNg);

                if (snapshot.Frame != null)
                {
                    if (show)
                    {
                        UpdatePictureBox(originPb, snapshot.Frame);
                    }

                    snapshot.Frame.Dispose();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка обновления UI");
            }
        }
        #endregion

        #region Выделение лейблов
        private void Label_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Label lbl)
            {
                lbl.ForeColor = _labelHoverColor;
                lbl.Font = _labelHoverFont;
            }
        }

        private void Label_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Label lbl)
            {
                lbl.ForeColor = _labelNormalColor;
                lbl.Font = _labelNormalFont;
            }
        }
        #endregion
    }
}