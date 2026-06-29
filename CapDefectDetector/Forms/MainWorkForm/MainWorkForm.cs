//#define OLD_FRAME_PROCESSING

using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using CapDefectDetector.Authorization;
using CapDefectDetector.CameraAndModbusClasses;
using CapDefectDetector.DTO.CameraSettings;
using CapDefectDetector.DTO.CapRecipe;
using CapDefectDetector.DTO.DefectSettings;
using CapDefectDetector.DTO.PrSettings;
using CapDefectDetector.Forms;
using CapDefectDetector.Forms.DefectParamSettingsForms.InclusionSettingsForm;
using CapDefectDetector.Forms.DefectParamSettingsForms.InpaintSettingsForm;
using CapDefectDetector.Forms.DefectParamSettingsForms.ObloySettingsForm;
using CapDefectDetector.Forms.DefectParamSettingsForms.OvalitySettingsForm;
using CapDefectDetector.Forms.DefectParamSettingsForms.UnderfillSettingsForm;
using CapDefectDetector.FrameProcessing;
using CapDefectDetector.Hardware;
using CapDefectDetector.ImageProcessing.Utils;
using CapDefectDetector.ImageProcessing.Utils.ContourProcessor;
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
        private bool _manualDisconnect = false;
        // Выделение лейблов
        private Color _labelNormalColor = Color.Black;
        private Color _labelHoverColor = Color.FromArgb(66, 133, 244);
        private Color _labelDisabledHoverColor = Color.Gray;
        private Font _labelNormalFont;
        private Font _labelHoverFont;

        // Регистры ПР205
        private HikCamera _cam;
        private ModuleIO _modbusClient;

        // Состояния приложения
        private bool _cameraConnected = false;
        private bool _prConnected = false;
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

        private CapContourUtils _capContourUtils;
        private CapContourUtils _editableCapContourUtils;

        //Для работы с файлами рецептов крышек
        private Dictionary<string, CapRecipe> _recipes = new();
        private FileSystemWatcher _recipesWatcher;
        private string _recipesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Рецепты");
        private bool _isApplyingRecipe = false;

        //Объекты классов утилит для дефектов
        private CapOvalityDefectUtils _ovalityUtils;
        private CapInclusionDefectUtils _inclusionUtils;
        private CapPaintDefectUtils _paintUtils;
        private CapObloyDefectUtils _obloyUtils;
        private CapUnderfillDefectUtils _underfillUtils;
        //Флаг для проверки измененных парамтеров дефектов
        private bool _defectSettingsSaved = true;

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
        private Dictionary<string, ModuleIOSettings> _prSettings = new();
        private bool _isApplyingPrSettings = false;
        private FileSystemWatcher _prSettingsWatcher;

        private string _folderParamCamera = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Настройки\\Настройка аппаратуры\\Настройки камеры");
        private Dictionary<string, HikCameraSettings> _cameraSettings = new();
        private bool _isApplyingCameraSettings = false;
        private FileSystemWatcher _cameraSettingsWatcher;

        // Работа с изображениями из файла
        private ImmutableList<string> _imageFiles = [];
        private int _currentImageIndex = 0;

        //Избежание дубликатов кадров
        private ulong _lastFrameHash = 0;
        private bool _hasLastHash = false;
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

        private Timer _roleDisplayTimer = new Timer();

        #endregion

        #region Конструктор и инициализация

        public MainWorkForm(HikCamera camera, ModuleIO modbus)
        {
            InitializeConnections(camera, modbus);
            InitializeComponent();
            InitializeReceptTabControl();
            InitializeApplication();
            InitializeModuleInitSettings();
        }

        private void InitializeConnections(HikCamera camera, ModuleIO modbus)
        {
            _cam = camera;
            _cameraConnected = camera != null;
            if (_cam != null)
                _cam.SendImage += GetImage;

            _modbusClient = modbus;
            _prConnected = modbus != null;

            if (_modbusClient != null)
            {
                _modbusClient.ConnectionStatusChanged += ModbusClient_ConnectionStatusChanged;
                _modbusClient.EnableAutoReconnect();
                _modbusClient.StartPolling();
            }
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
                _modbusClient.ApplySettings();
                _manualDisconnect = false;
                _prConnected = true;

                UpdatePRConnectionUI(true);

                try { _modbusClient.StopRecognizeProcessing(); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка при сбросе регистра после переподключения"); }
            }
            else
            {
                _prConnected = false;
                UpdatePRConnectionUI(false);
                if (_isProcessing && !_manualDisconnect)
                {
                    _ = StopProcessingAsync();
                    MessageBox.Show("Потеряно соединение с ПР205. Обработка кадров остановлена. При переподключении сдув будет отключен.", "Ошибка соединения", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void UpdatePRConnectionUI(bool connected)
        {
            if (connected)
            {
                prStatus.Text = "Подключено";
                pr205IpTb.Text = _modbusClient.ipAddressModule;
                pr205PortTb.Text = _modbusClient.portModule.ToString();
                prStatus.ForeColor = Color.Green;
                connectPrButton.Text = "Отключиться от ПР";
                connectPrButton.BackColor = _connectedColor;
                breakingAllowCb.Enabled = true;
                soundSignalAllowCb.Enabled = true;
                applyPrBreakerParamButton.Enabled = true;
            }
            else
            {
                prStatus.Text = _manualDisconnect ? "Откл. вручную" : "Не подключено";
                prStatus.ForeColor = Color.Red;
                pr205IpTb.Text = _modbusClient.ipAddressModule;
                pr205PortTb.Text = _modbusClient.portModule.ToString();
                connectPrButton.Text = "Подключиться к ПР";
                connectPrButton.BackColor = _disconnectedColor;
                breakingAllowCb.Enabled = false;
                soundSignalAllowCb.Enabled = false;
                applyPrBreakerParamButton.Enabled = false;
            }
        }

        private void UpdateCameraConnectionUI()
        {
            bool connected = _cam?.Connected == true;

            if (connected)
            {
                cameraIpTextBox.Text = _cam.IpAdress;
                camStatus.Text = "Подключено";
                camStatus.ForeColor = Color.Green;
                connectCameraButton.Text = "Отключиться от камеры";
                connectCameraButton.BackColor = _connectedColor;
            }
            else
            {
                cameraIpTextBox.Text = "Камера не выбрана на этапе инициализации";
                camStatus.Text = "Не подключено";
                camStatus.ForeColor = Color.Red;
                connectCameraButton.Text = "Подключиться к камере";
                connectCameraButton.BackColor = _disconnectedColor;
                startStreamButton.Enabled = false;
                applySettingsButton.Enabled = false;
            }
        }

        private void InitializeApplication()
        {
            InitializeSettingsBindings();
            InitializeImageMatrices();
            InitializeAllFileWatchers();
            InitializeAllSettings();
            UpdateConnectionStatuses();
            InitializeCycleSystem();
            InitializeUISelections();
            InitializeAuthorizationSystem();
            InitLabelHoverEffects();

            recognizeButton.Enabled = false;
            StartStop(false, true);
            LocalSettings.Instance.Save();
        }

        private void InitializeSettingsBindings()
        {
            var checkboxes = new[] { ovalityCB, inclusionCB, inpaintCB, obloyCB, underFillCb, okCapsSaveCb, ngCapsSaveCb };
            foreach (var cb in checkboxes)
                cb.CheckedChanged += AnySettingChanged;
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

        private void InitializeAllFileWatchers()
        {
            InitializeFileWatcher(ref _recipesWatcher, _recipesFolder, OnRecipesFolderChanged);
            InitializeFileWatcher(ref _defectSettingsWatcher, _folderParamDefect, OnDefectSettingsFolderChanged);
            InitializeFileWatcher(ref _prSettingsWatcher, _folderParamPr205, OnPrSettingsFolderChanged);
            InitializeFileWatcher(ref _cameraSettingsWatcher, _folderParamCamera, OnCameraSettingsFolderChanged);
        }

        private void InitializeFileWatcher(ref FileSystemWatcher watcher, string folder, FileSystemEventHandler handler)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            watcher = new FileSystemWatcher
            {
                Path = folder,
                Filter = "*.json",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            watcher.Created += handler;
            watcher.Deleted += handler;
            watcher.Renamed += new RenamedEventHandler(handler);
            watcher.EnableRaisingEvents = true;
        }

        // Универсальный метод для настройки ComboBox
        private void SetupComboBox<T>(ComboBox cb, Dictionary<string, T> dict, string lastSelectedKey)
        {
            cb.Items.Clear();
            foreach (var key in dict.Keys)
                cb.Items.Add(key);

            if (dict.Count > 0)
            {
                if (!string.IsNullOrEmpty(lastSelectedKey) && cb.Items.Contains(lastSelectedKey))
                    cb.SelectedItem = lastSelectedKey;
                else
                    cb.SelectedIndex = 0;
            }
        }

        private void InitializeAllSettings()
        {
            // Рецепты
            LoadRecipes();
            SetupComboBox(receptCapsCmB, _recipes, Properties.Settings.Default.Settings_LastRecipeFileName);

            // Настройки дефектов
            LoadDefectSettings();
            SetupComboBox(paramDefCmB, _defectSettings, Properties.Settings.Default.Settings_LastDefectSettingsFileName);

            // Настройки ПР205
            LoadPrSettings();
            SetupComboBox(prSettingsCmB, _prSettings, Properties.Settings.Default.Settings_LastPrSettingsFileName);

            // Настройки камеры
            LoadCameraSettings();
            SetupComboBox(cameraSettingsCmB, _cameraSettings, Properties.Settings.Default.Settings_LastCameraSettingsFileName);
        }

        private void UpdateConnectionStatuses()
        {
            UpdatePRConnectionUI(_modbusClient?.Connected == true);
            UpdateCameraConnectionUI();
        }

        private void InitializeCycleSystem()
        {
            int cycle = 0;
            int.TryParse(Properties.Settings.Default.Settings_LastCycleTime, out cycle);

            if (cycle < cycleUpDown.Minimum)
            {
                cycle = (int)cycleUpDown.Minimum;
                Properties.Settings.Default.Settings_LastCycleTime = cycle.ToString();
                Properties.Settings.Default.Save();
            }

            CycleImageSaver.SetCycleHours(cycle);
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
                timeLeftTb.Text = AuthManager.Instance.CurrentRole == Role.Admin
                    ? AuthManager.Instance.GetSecondsLeft().ToString()
                    : "∞";
            };
            _roleDisplayTimer.Start();
        }

        #region Обработчики изменения папок (Watchers)

        private void OnRecipesFolderChanged(object sender, FileSystemEventArgs e)
        {
            if (InvokeRequired) { Invoke(new Action(() => ReloadRecipes())); }
            else { ReloadRecipes(); }
        }

        private void OnDefectSettingsFolderChanged(object sender, FileSystemEventArgs e)
        {
            if (InvokeRequired) { Invoke(new Action(() => ReloadDefectSettings())); }
            else { ReloadDefectSettings(); }
        }

        private void OnPrSettingsFolderChanged(object sender, FileSystemEventArgs e)
        {
            if (InvokeRequired) { Invoke(new Action(() => ReloadPrSettings())); }
            else { ReloadPrSettings(); }
        }

        private void OnCameraSettingsFolderChanged(object sender, FileSystemEventArgs e)
        {
            if (InvokeRequired) { Invoke(new Action(() => ReloadCameraSettings())); }
            else { ReloadCameraSettings(); }
        }

        #endregion

        #region Методы перезагрузки настроек

        private void ReloadRecipes()
        {
            LoadRecipes();
            receptCapsCmB.Items.Clear();
            foreach (var name in _recipes.Keys)
                receptCapsCmB.Items.Add(name);
        }

        private void ReloadDefectSettings()
        {
            LoadDefectSettings();
            paramDefCmB.Items.Clear();
            foreach (var name in _defectSettings.Keys)
                paramDefCmB.Items.Add(name);
        }

        private void ReloadPrSettings()
        {
            LoadPrSettings();
            prSettingsCmB.Items.Clear();
            foreach (var name in _prSettings.Keys)
                prSettingsCmB.Items.Add(name);
        }

        private void ReloadCameraSettings()
        {
            LoadCameraSettings();
            cameraSettingsCmB.Items.Clear();
            foreach (var name in _cameraSettings.Keys)
                cameraSettingsCmB.Items.Add(name);
        }

        #endregion

        #region Загрузка настроек из файлов

        private void LoadRecipes()
        {
            _recipes.Clear();
            LoadSettingsFromFolder(_recipesFolder, _recipes, (json) => JsonConvert.DeserializeObject<CapRecipe>(json));
        }

        private void LoadDefectSettings()
        {
            _defectSettings.Clear();
            LoadSettingsFromFolder(_folderParamDefect, _defectSettings, (json) => JsonConvert.DeserializeObject<DefectSettings>(json));
        }

        private void LoadPrSettings()
        {
            _prSettings.Clear();
            LoadSettingsFromFolder(_folderParamPr205, _prSettings, (json) => JsonConvert.DeserializeObject<ModuleIOSettings>(json));
        }

        private void LoadCameraSettings()
        {
            _cameraSettings.Clear();
            LoadSettingsFromFolder(_folderParamCamera, _cameraSettings, (json) => JsonConvert.DeserializeObject<HikCameraSettings>(json));
        }

        private void LoadSettingsFromFolder<T>(string folder, Dictionary<string, T> dict, Func<string, T> deserialize)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (string file in Directory.GetFiles(folder, "*.json"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);
                T item = deserialize(json);
                if (item != null)
                    dict[name] = item;
            }
        }

        #endregion

        private void InitLabelHoverEffects()
        {
            _labelNormalFont = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            _labelHoverFont = new Font("Segoe UI", 8.25F, FontStyle.Bold | FontStyle.Underline);
            ApplyHover(ovalityParamLb, inclusionParamLb, inpaintParamLb, obloyParamLb, underfillParamLb);
        }

        private void ApplyHover(params Label[] labels)
        {
            foreach (var label in labels)
            {
                label.Cursor = Cursors.Hand;
                label.ForeColor = _labelNormalColor;
                label.Font = _labelNormalFont;
                label.MouseEnter += Label_MouseEnter;
                label.MouseLeave += Label_MouseLeave;
            }
        }

        private void InitializeModuleInitSettings()
        {
            try
            {
                if (_modbusClient?.Connected == true)
                {
                    _modbusClient.ApplyInitialSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка отправки в PLC: " + ex.Message);
            }
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
                string ip = pr205IpTb.Text.Trim();

                if (!int.TryParse(pr205PortTb.Text.Trim(), out int port))
                    throw new Exception("Неверный формат порта ПР205");

                if (_modbusClient != null)
                    _modbusClient.ConnectionStatusChanged -= ModbusClient_ConnectionStatusChanged;

                _modbusClient = new ModuleIO(ip, port);
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

            var cameraSettings = new HikCameraSettings
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

            // --- Обновляем список настроек дефектов ---
            LoadCameraSettings();

            cameraSettingsCmB.Items.Clear();
            foreach (string cameraSettingsNamer in _cameraSettings.Keys)
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

        private void ApplyCameraSettings(HikCameraSettings r)
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

            string folder = _folderParamPr205;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string name = prSettingsNameTb.Text.Trim();
            string fileName = name + ".json";
            string fullPath = Path.Combine(folder, fileName);

            bool existedBefore = File.Exists(fullPath);

            var prSettings = new ModuleIOSettings
            {
                Name = name,
                BreakingTime = _modbusClient.GetBreakingTime(),
                CameraOffset = _modbusClient.GetCameraOffset(),
                BreakerOffset = _modbusClient.GetBreakerOffset()
            };

            string json = System.Text.Json.JsonSerializer.Serialize(
                prSettings,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }
            );

            File.WriteAllText(fullPath, json, Encoding.UTF8);

            Properties.Settings.Default.Settings_IpAdressPr = pr205IpTb.Text;
            Properties.Settings.Default.Settings_PortPr = pr205PortTb.Text;
            Properties.Settings.Default.Save();

            LoadPrSettings();

            prSettingsCmB.Items.Clear();
            foreach (string prSettingsNamer in _prSettings.Keys)
                prSettingsCmB.Items.Add(prSettingsNamer);
            prSettingsCmB.SelectedItem = name;

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

        private void ApplyPrSettings(ModuleIOSettings r)
        {
            _isApplyingPrSettings = true;

            try
            {
                _modbusClient?.ApplyLocalSettings(r);
                breakingTimeUd.Text = r.BreakingTime.ToString();
                cameraOffsetUd.Text = r.CameraOffset.ToString();
                breakerOffsetUd.Text = r.BreakerOffset.ToString();
            }
            finally
            {
                _isApplyingPrSettings = false;
            }
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

                if (!int.TryParse(breakingTimeUd.Text, out int delay) || delay < 0 || delay > 65535)
                {
                    string msg = "'Время сдува, мс' должно быть числом от 0 до 65535.";
                    ErrorLogger.Log(new Exception(msg), "ValidatePrSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    breakingTimeUd.Focus();
                    return false;
                }

                if (!int.TryParse(cameraOffsetUd.Text, out int cameraOffset) || cameraOffset < 0 || cameraOffset > 65535)
                {
                    string msg = "'Расстояние от датчика до камеры, шаги' должно быть числом от 0 до 65535.";
                    ErrorLogger.Log(new Exception(msg), "ValidatePrSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cameraOffsetUd.Focus();
                    return false;
                }

                if (!int.TryParse(breakerOffsetUd.Text, out int breakerOffset) || breakerOffset < 0 || breakerOffset > 65535)
                {
                    string msg = "'Расстояние от датчика до сдува, шаги' должно быть числом от 0 до 65535.";
                    ErrorLogger.Log(new Exception(msg), "ValidatePrSettings");
                    MessageBox.Show(msg, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    breakerOffsetUd.Focus();
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
                return;

            _applyPrCts = new CancellationTokenSource();

            try
            {
                await Task.Run(() => _modbusClient.ApplySettings(), _applyPrCts.Token);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка при отправке параметров на ПР205 (ApplyPr_Click)");
            }
        }

        private void breakingTimeUd_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingPrSettings) return;

            _modbusClient?.SetBreakingTime((int)breakingTimeUd.Value);
        }

        private void cameraOffsetUd_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingPrSettings) return;

            _modbusClient?.SetCameraOffset((int)cameraOffsetUd.Value);
        }

        private void breakerOffsetUd_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingPrSettings) return;

            _modbusClient?.SetBreakerOffset((int)breakerOffsetUd.Value);
        }
        #endregion

        #region Настройка параметров обнаржуения дефектов
        private void saveDefectSettingsNew_Click(object sender, EventArgs e)
        {
            if (!ValidateDefectSettings())
                return;

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

                Ovality = _ovalityUtils.GetSettings(),
                Inclusion = _inclusionUtils.GetSettings(),
                Paint = _paintUtils.GetSettings(),
                Obloy = _obloyUtils.GetSettings(),
                Underfill = _underfillUtils.GetSettings()
            };

            string json = System.Text.Json.JsonSerializer.Serialize(
                        defectSettings,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            Converters =
                            {
                                new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase, allowIntegerValues: true)
                            }
                        });

            File.WriteAllText(fullPath, json, Encoding.UTF8);

            LoadDefectSettings();

            paramDefCmB.Items.Clear();
            foreach (string key in _defectSettings.Keys)
                paramDefCmB.Items.Add(key);

            paramDefCmB.SelectedItem = name;
            _defectSettingsSaved = true;

            MessageBox.Show(existedBefore ? $"Файл настроек \"{name}\" обновлён!" : $"Файл настроек \"{name}\" создан!");
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

            try
            {
                _ovalityUtils = new CapOvalityDefectUtils(r.Ovality);
                ovalityCoefNumUpD.Value = (decimal)r.Ovality.OvalityThreshold;

                _inclusionUtils = new CapInclusionDefectUtils(r.Inclusion);
                coefCapRadiusInclusionUpD.Value = (decimal)r.Inclusion.CoefCapRadiusInclusion;
                minSquareInclusionNumUpD.Value = (decimal)r.Inclusion.MinAreaInclusion;
                maxSquareInclusionNumUpD.Value = (decimal)r.Inclusion.MaxAreaInclusion;
                circleCoefNumUpD.Value = (decimal)r.Inclusion.InclusionThreshold;

                _paintUtils = new CapPaintDefectUtils(r.Paint);
                minSquareInpaintNumUpD.Value = (decimal)r.Paint.MinAreaInpaintDefect;
                whiteThresoldNumUpD.Value = (decimal)r.Paint.MinInpaintWhiteThreshold;

                _obloyUtils = new CapObloyDefectUtils(r.Obloy);
                capFlashOffsetNumUpD.Value = (decimal)r.Obloy.CapFlashOffset;
                obloyPixCountNumUpD.Value = (decimal)r.Obloy.MinAreaObloy;

                _underfillUtils = new CapUnderfillDefectUtils(r.Underfill);
                coefCapRadiusMaskUnderFillNumUpD.Value = (decimal)r.Underfill.CoefCapRadiusUnderFill;
                countCorrugationsNumUpD.Value = (decimal)r.Underfill.CorrugationsCountForUnderFill;
            }
            finally
            {
                _isApplyingDefectSettings = false;
            }
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
        private void MainWorkForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.ApplicationExitCall)
                return;

            if (!ConfirmApplicationExit())
            {
                e.Cancel = true;
                return;
            }

            if (!CheckDefectSettingsBeforeExit())
            {
                e.Cancel = true;
                return;
            }

            SendShutdownSignalToPlc();
            CloseCamera();
            DisconnectModbus();

#if OLD_FRAME_PROCESSING
#else
            DisposeImageQueue();
#endif
            this.FormClosing -= MainWorkForm_FormClosing;
            Application.Exit();
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

            try
            {
                _capContourUtils = new CapContourUtils(r);
                _editableCapContourUtils = new CapContourUtils(r);

                #region Чекбоксы
                isWhiteCb.Checked = r.IsWhite;
                isBlackOrBrownCb.Checked = r.IsBlackOrBrown;
                isColorCb.Checked = r.IsColored;
                isGreenCb.Checked = r.IsGreen;
                isYellowCb.Checked = r.IsYellow;
                UpdateColorModeUI();
                #endregion

                #region Настройка сатурации камеры
                if (_cam != null)
                {
                    uint saturation = r.IsBlackOrBrown ? (uint)r.CameraSaturationBlackOrBrown : (uint)r.CameraSaturation;
                    _cam.Saturation = saturation;
                    _cam.SetSaturation();
                }
                #endregion

                #region Блокировка непрокраса если крышка белая
                if (r.IsWhite)
                {
                    inpaintCB.Checked = false;
                    inpaintCB.Enabled = false;
                }
                else
                {
                    inpaintCB.Enabled = true;
                    inpaintCB.Checked = true;
                }
                #endregion

                #region Интерфейс рецепта

                // Интерфейс настрорйки цветных крышек (начало)
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
                // Интерфейс настрорйки цветных крышек (Конец)

                // Интерфейс настрорйки черных/коричневых крышек (начало)
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
                // Интерфейс настрорйки черных/коричневых крышек (конец)
                #endregion

                receptNameTb.Text = r.Name;
                currentReceptFolderTb.Text = _recipesFolder;
                frameSaturationNumUpD.Text = (r.IsBlackOrBrown ? r.CameraSaturationBlackOrBrown : r.CameraSaturation).ToString();
            }
            finally
            {
                _isApplyingRecipe = false;
            }
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
                ResetState();
                _isProcessing = false;
                _imageQueue.Clear();

                // Если есть подключение, отправляем стоп-сигнал
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    if (!_isImageLoaded)
                    {
                        try
                        {
                            _modbusClient.StopRecognizeProcessing();
                            _modbusClient.TurnOffGreenSemaphore();
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
                    MessageBox.Show("ПР205 не подключено, отбраковка не будет происходить.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    if (!_isImageLoaded)
                    {
                        // Отправляем сигнал на ПР
                        _modbusClient.StartRecognizeProcessing();
                        _modbusClient.TurnOnGreenSemaphore();
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

        private bool CheckDefectSettingsBeforeExit()
        {
            if (_defectSettingsSaved)
                return true;

            DialogResult result = MessageBox.Show("Не сохранены настройки дефектов, выйти (Да) или сохранить (Нет).", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            switch (result)
            {
                case DialogResult.Yes:
                    return true;

                case DialogResult.No:
                    return false;

                default:
                    return false;
            }
        }

        private bool ConfirmApplicationExit()
        {
            DialogResult result = MessageBox.Show("Завершить работу приложения?", "Выход", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        private void SendShutdownSignalToPlc()
        {
            if (_modbusClient == null || !_modbusClient.Connected)
                return;

            try
            {
                _modbusClient.StopRecognizeProcessing();
                _modbusClient.DenyBreaker();
                _modbusClient.DenySoundSignal();
                _modbusClient.TurnOffOrangeSemaphore();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Ошибка при отправке сигнала завершения в ПР205: {ex.Message}");
            }
        }

        private void CloseCamera()
        {
            if (_cam == null || !_cameraConnected)
                return;

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

        private void DisconnectModbus()
        {
            if (_modbusClient == null || !_modbusClient.Connected)
                return;

            try
            {
                _modbusClient.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отключении от ПР205: {ex.Message}");
            }
        }

        private void DisposeImageQueue()
        {
            _imageQueue?.Dispose();
        }

        #endregion

        #region Вспомогательные методы

        private void ovalityCoefNumUpD_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingDefectSettings) return;

            _ovalityUtils.SetThreshold((double)ovalityCoefNumUpD.Value);
            _defectSettingsSaved = false;
        }

        private void circleCoefNumUpD_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingDefectSettings) return;

            _inclusionUtils.SetInclusionThreshold((double)circleCoefNumUpD.Value);
            _defectSettingsSaved = false;
        }

        private void coefCapRadiusInclusionUpD_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingDefectSettings) return;

            _inclusionUtils.SetCoefCapRadiusInclusion((double)coefCapRadiusInclusionUpD.Value);
            _defectSettingsSaved = false;
        }

        private void minSquareInclusionNumUpD_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingDefectSettings) return;
            _inclusionUtils.SetMinAreaInclusion((double)minSquareInclusionNumUpD.Value);
        }

        private void maxSquareInclusionNumUpD_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingDefectSettings) return;

            _inclusionUtils.SetMaxAreaInclusion((double)maxSquareInclusionNumUpD.Value);
            _defectSettingsSaved = false;
        }

        private void minSquareInpaintNumUpD_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingDefectSettings) return;

            _paintUtils.SetMinAreaInpaintDefect((double)minSquareInpaintNumUpD.Value);
            _defectSettingsSaved = false;
        }

        private void whiteThresoldNumUpD_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingDefectSettings) return;

            _paintUtils.SetMinInpaintWhiteThreshold((double)whiteThresoldNumUpD.Value);
            _defectSettingsSaved = false;
        }

        private void capFlashOffsetNumUpD_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingDefectSettings) return;

            _obloyUtils.SetCapFlashOffset((double)capFlashOffsetNumUpD.Value);
            _defectSettingsSaved = false;
        }

        private void obloyPixCountNumUpD_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingDefectSettings) return;

            _obloyUtils.SetMinAreaObloy((double)obloyPixCountNumUpD.Value);
            _defectSettingsSaved = false;
        }

        private void countCorrugationsNumUpD_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingDefectSettings) return;

            _underfillUtils.SetCorrugationsCountForUnderFill((double)countCorrugationsNumUpD.Value);
            _defectSettingsSaved = false;
        }

        private void coefCapRadiusMaskUnderFillNumUpD_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingDefectSettings) return;

            _underfillUtils.SetCoefCapRadiusUnderFill((double)coefCapRadiusMaskUnderFillNumUpD.Value);
            _defectSettingsSaved = false;
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
                        _img1?.Dispose();
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
                        _img1?.Dispose();
                        _img1 = img.Clone();

                        UpdatePictureBox(originPb, _img1);
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

                byte[][] currentRows = new byte[rowsY.Length][];

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
                            try
                            {
                                frameToProcess = _imageQueue.Get(token);
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }
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

                                CapContourResult capResult = _capContourUtils.GetCapContour(gray, frameToProcess);

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
                                        ovalityTask = RunCheckWithTimeout(_grayForOvality, _imageForOvality, _frameToDisplay, timeoutCts.Token, RunCheckOvality, capResult);

                                    if (settings.Inclusion)
                                        inclusionsTask = RunCheckWithTimeout(_grayForInclusions, _imageForInclusions, _frameToDisplay, timeoutCts.Token, RunCheckForInclusions, capResult);

                                    if (settings.Inpaint)
                                        paintTask = RunCheckWithTimeout(_grayForPaintDefects, _imageForPaintDefects, _frameToDisplay, timeoutCts.Token, RunCheckForPaintDefects, capResult);

                                    if (settings.Obloy)
                                        obloyTask = RunCheckWithTimeout(_grayForObloyDefects, _imageForObloyDefects, _frameToDisplay, timeoutCts.Token, RunCheckForObloyDefects, capResult);

                                    if (settings.UnderFill)
                                        underFillTask = RunCheckWithTimeout(_grayForUnderfill, _imageForUnderfill, _frameToDisplay, timeoutCts.Token, RunCheckForUnderFillDefects, capResult);

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
                                        try { _modbusClient.SendQualityStatus(st); }
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

        private async Task<bool> RunCheckWithTimeout(Mat gray, Mat image, Mat drawFrame, CancellationToken token, Func<Mat, Mat, Mat, CancellationToken, CapContourResult, bool> checkFunc, CapContourResult capResult)
        {
            try
            {
                return await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    return checkFunc(gray, image, drawFrame, token, capResult);
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

        private bool RunCheckOvality(Mat gray, Mat image, Mat drawFrame, CancellationToken token, CapContourResult capResult)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var stopwatch = Stopwatch.StartNew();

                bool isOval = false;
                try
                {
                    isOval = _ovalityUtils.CheckOvality(gray, image, drawFrame, token, capResult.Contour);
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

        private bool RunCheckForInclusions(Mat gray, Mat image, Mat drawFrame, CancellationToken token, CapContourResult capResult)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var stopwatch = Stopwatch.StartNew();

                bool isInclusion = false;
                try
                {
                    isInclusion = _inclusionUtils.CheckForInclusions(gray, image, drawFrame, token, capResult.Contour);
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

        private bool RunCheckForPaintDefects(Mat gray, Mat image, Mat drawFrame, CancellationToken token, CapContourResult capResult)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var stopwatch = Stopwatch.StartNew();

                bool isInpaint = false;
                try
                {
                    isInpaint = _paintUtils.CheckForPaintDefects(gray, image, drawFrame, token, capResult.Contour);
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

        private bool RunCheckForObloyDefects(Mat gray, Mat image, Mat drawFrame, CancellationToken token, CapContourResult capResult)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var stopwatch = Stopwatch.StartNew();

                bool isObloy = false;
                try
                {
                    isObloy = _obloyUtils.CheckForObloyDefects(gray, image, drawFrame, token, capResult.Contour, capResult.Blur1, capResult.Blur2, capResult.Mask);
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

        private bool RunCheckForUnderFillDefects(Mat gray, Mat image, Mat drawFrame, CancellationToken token, CapContourResult capResult)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var stopwatch = Stopwatch.StartNew();

                bool isUnderFill = false;
                try
                {
                    isUnderFill = _underfillUtils.CheckForUnderFillDefects(gray, image, drawFrame, token, capResult.Contour);
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
        private void breakingAllowCb_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    bool valueToSend = breakingAllowCb.Checked;

                    _modbusClient.SetBreakerAllow(valueToSend);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "Ошибка при отправке сигнала на ПР205");
            }
        }

        private void soundSignalAllowCb_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_modbusClient != null && _modbusClient.Connected)
                {
                    bool valueToSend = soundSignalAllowCb.Checked;

                    _modbusClient.SetSoundSignalAllow(valueToSend);
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
                CapContourResult capResult = _capContourUtils.GetCapContour(grayBase, frameBase);
                Point[] contour = capResult?.Contour;
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

                try { if (ovalityCB.Checked) oval = _ovalityUtils.CheckOvality(grayO, frameO, finalFrame, fake, contour); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки овальности"); }

                try { if (inclusionCB.Checked) incl = _inclusionUtils.CheckForInclusions(grayI, frameI, finalFrame, fake, contour); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки включений"); }

                try { if (inpaintCB.Checked) paint = _paintUtils.CheckForPaintDefects(grayP, frameP, finalFrame, fake, contour); }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки дефектов краски"); }

                try { if (obloyCB.Checked) obloy = _obloyUtils.CheckForObloyDefects(grayOb, frameOb, finalFrame, fake, capResult?.Contour, capResult?.Blur1, capResult?.Blur2, capResult?.Mask); ; }
                catch (Exception ex) { ErrorLogger.Log(ex, "Ошибка проверки облоя"); }

                try { if (underFillCb.Checked) underFill = _underfillUtils.CheckForUnderFillDefects(grayUf, frameUf, finalFrame, fake, contour); }
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

            if (_editableCapContourUtils.GetIsBlackOrBrown())
            {
                contour = ProcessBlackOrBrown();
                contour = _editableCapContourUtils.CorrectContour(contour, _editableCapContourUtils.GetContourCorrectionBlackOrBrown());
                Mat result = DrawContour(contour);
                contourCorrectionBlackOrBrownReceptParamSmallPb.Image = BitmapConverter.ToBitmap(result);
                resultContourBlackOrBrownSmallPb.Image = BitmapConverter.ToBitmap(result);
                ShowInGeneralPreview(resultContourBlackOrBrownSmallPb, generalReceptParamPb);
            }
            else
            {
                contour = ProcessColor();
                contour = _editableCapContourUtils.CorrectContour(contour, _editableCapContourUtils.GetContourCorrectionColor());
                Mat result = DrawContour(contour);
                contourCorrectionColorReceptParamSmallPb.Image = BitmapConverter.ToBitmap(result);
                resultContourColorSmallPb.Image = BitmapConverter.ToBitmap(result);
                ShowInGeneralPreview(resultContourColorSmallPb, generalReceptParamPb);
            }
        }

        private Point[] ProcessBlackOrBrown()
        {
            Mat satImg = _editableCapContourUtils.ApplySaturationStep(_imageOriginReceptParam, _editableCapContourUtils.GetCameraSaturationBlackOrBrown());
            saturationBlackOrBrownReceptParamSmallPb.Image = BitmapConverter.ToBitmap(satImg);

            Mat gray = _editableCapContourUtils.ApplyGrayStep(satImg);

            Mat blurred = _editableCapContourUtils.ApplyMedianStep(gray, _editableCapContourUtils.GetMedianFilter());
            medianFilterBlackOrBrownReceptParamSmallPb.Image = BitmapConverter.ToBitmap(blurred);

            Mat edges = _editableCapContourUtils.ApplyCannyStep(blurred, _editableCapContourUtils.GetCannyThreshold());
            cannyBlackOrBrownReceptParamSmallPb.Image = BitmapConverter.ToBitmap(edges);

            return _editableCapContourUtils.ApplyEllipseStep(edges);
        }


        private Point[] ProcessColor()
        {
            if (_imageOriginReceptParam == null || _imageOriginReceptParam.Empty())
                return null;

            Mat satImg = _editableCapContourUtils.ApplySaturationStep(_imageOriginReceptParam, _editableCapContourUtils.GetCameraSaturation());
            saturationColorReceptParamSmallPb.Image = BitmapConverter.ToBitmap(satImg);

            Mat capsImg = _editableCapContourUtils.ApplyCapsColorStep(satImg, _editableCapContourUtils.GetCapsColor(), _editableCapContourUtils.GetIsColored(), _editableCapContourUtils.GetIsYellow(), _editableCapContourUtils.GetIsGreen());
            capscolorColorReceptParamSmallPb.Image = BitmapConverter.ToBitmap(capsImg);

            Mat[] channels = _editableCapContourUtils.ApplyWindowStep(capsImg, _editableCapContourUtils.GetWindow());
            windowColorReceptParamSmallPb.Image = BitmapConverter.ToBitmap(channels[1]);

            _editableCapContourUtils.ApplyMorphologyStep(channels, _editableCapContourUtils.GetElement1(), _editableCapContourUtils.GetElement2());
            Mat morphImg = channels[2];

            morphColorReceptParamSmallPb.Image = BitmapConverter.ToBitmap(morphImg);

            return _editableCapContourUtils.GetMaxContour(morphImg);
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
            if (_isApplyingRecipe) return;

            _editableCapContourUtils.SetCameraSaturation((int)saturationColorUpDown.Value);
            RecomputeAll();
        }

        private void capcolorUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;

            _editableCapContourUtils.SetCapsColor((byte)capcolorUpDown.Value);
            RecomputeAll();
        }

        private void windowCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;

            if (int.TryParse(windowCb.Text, out int value))
                _editableCapContourUtils.SetWindow(value);
            RecomputeAll();
        }

        private void morphCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;

            if (int.TryParse(morphCb.Text, out int value))
                _editableCapContourUtils.SetMorphSize(value);
            RecomputeAll();
        }

        private void contourCorrectionColorUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;

            _editableCapContourUtils.SetContourCorrectionColor((float)contourCorrectionColorUpDown.Value);
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

        private void ShowInGeneralPreview(PictureBox smallPb, PictureBox generalPb)
        {
            if (smallPb.Image != null)
            {
                generalPb.Image?.Dispose();
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

            string folder = _recipesFolder;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string name = receptNameTb.Text.Trim();
            string fileName = name + ".json";
            string fullPath = Path.Combine(folder, fileName);

            bool existedBefore = File.Exists(fullPath);

            CapRecipe recipe = _editableCapContourUtils.GetSettings();

            recipe.Name = name;

            string json = System.Text.Json.JsonSerializer.Serialize(
                recipe,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }
            );

            File.WriteAllText(fullPath, json, Encoding.UTF8);

            LoadRecipes();

            receptCapsCmB.Items.Clear();
            foreach (string recipeName in _recipes.Keys)
                receptCapsCmB.Items.Add(recipeName);

            receptCapsCmB.SelectedItem = name;

            MessageBox.Show(existedBefore ? $"Рецепт \"{name}\" редактирован успешно!" : $"Рецепт \"{name}\" создан успешно!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            _editableCapContourUtils.SetIsGreen(isGreenCb.Checked);
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void isColorCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            _editableCapContourUtils.SetIsColored(isColorCb.Checked);
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void isYellowCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            _editableCapContourUtils.SetIsYellow(isYellowCb.Checked);
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void isWhiteCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            _editableCapContourUtils.SetIsWhite(isWhiteCb.Checked);
            UpdateColorModeUI();
            RecomputeAll();
        }

        private void isBlackOrBrownCb_CheckedChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;
            _editableCapContourUtils.SetIsBlackOrBrown(isBlackOrBrownCb.Checked);
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
            if (_isApplyingRecipe) return;

            _editableCapContourUtils.SetCameraSaturationBlackOrBrown((int)saturationBlackOrBrownUpDown.Value);
            RecomputeAll();
        }

        private void medianFilterUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;

            _editableCapContourUtils.SetMedianFilter((int)medianFilterUpDown.Value);
            RecomputeAll();
        }

        private void cannyUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;

            _editableCapContourUtils.SetCannyThreshold((int)cannyUpDown.Value);
            RecomputeAll();
        }

        private void contourCorrectionBlackOrBrownUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (_isApplyingRecipe) return;

            _editableCapContourUtils.SetContourCorrectionBlackOrBrown((float)contourCorrectionBlackOrBrownUpDown.Value);
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

        private void ResetState()
        {
            lock (_stateLock)
            {
                _state.Frame?.Dispose();
                _state.Frame = null;

                _state.GeneralCapsCount = 0;
                _state.Ok = 0;
                _state.Ng = 0;
                _state.PercentOK = 0;
                _state.PercentNG = 0;

                _state.OvalityDefectCount = 0;
                _state.InclusionDefectCount = 0;
                _state.PaintDefectCount = 0;
                _state.ObloyDefectCount = 0;
                _state.UnderFillDefectCount = 0;

                _state.PercentOvality = 0;
                _state.PercentInclusion = 0;
                _state.PercentPaint = 0;
                _state.PercentObloy = 0;
                _state.PercentUnderFill = 0;

                _state.TimeOvality = 0;
                _state.TimeInclusion = 0;
                _state.TimePaint = 0;
                _state.TimeObloy = 0;
                _state.TimeUnderFill = 0;

                _state.Time = 0;
                _state.DefectText = string.Empty;
                _state.IsNg = false;
            }
        }
        #endregion

        #region Выделение лейблов и работа с кнопками
        private void Label_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Label lbl)
            {
                lbl.Font = _labelHoverFont;

                if (_isProcessing || _isProcessingFromFolder)
                {
                    lbl.ForeColor = _labelDisabledHoverColor;
                }
                else
                {
                    lbl.ForeColor = _labelHoverColor;
                }
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

        private void ovalityParamLb_Click(object sender, EventArgs e)
        {
            if (_isProcessing || _isProcessingFromFolder)
                return;

            var form = new OvalitySettingsForm(_imageForTest, _ovalityUtils, _capContourUtils);

            if (form.ShowDialog() == DialogResult.OK)
            {
                var settings = _ovalityUtils.GetSettings();
                ovalityCoefNumUpD.Value = (decimal)settings.OvalityThreshold;

                _defectSettingsSaved = false;
            }
        }

        private void inclusionParamLb_Click(object sender, EventArgs e)
        {
            if (_isProcessing || _isProcessingFromFolder)
                return;

            var form = new InclusionSettingsForm(_imageForTest, _inclusionUtils, _capContourUtils);

            if (form.ShowDialog() == DialogResult.OK)
            {
                var settings = _inclusionUtils.GetSettings();
                coefCapRadiusInclusionUpD.Value = (decimal)settings.CoefCapRadiusInclusion;
                minSquareInclusionNumUpD.Value = (decimal)settings.MinAreaInclusion;
                maxSquareInclusionNumUpD.Value = (decimal)settings.MaxAreaInclusion;
                circleCoefNumUpD.Value = (decimal)settings.InclusionThreshold;

                _defectSettingsSaved = false;
            }
        }

        private void inpaintParamLb_Click(object sender, EventArgs e)
        {
            if (_isProcessing || _isProcessingFromFolder)
                return;

            var form = new InpaintSettingsForm(_imageForTest, _paintUtils, _capContourUtils);

            if (form.ShowDialog() == DialogResult.OK)
            {
                var settings = _paintUtils.GetSettings();
                minSquareInpaintNumUpD.Value = (decimal)settings.MinAreaInpaintDefect;
                whiteThresoldNumUpD.Value = (decimal)settings.MinInpaintWhiteThreshold;

                _defectSettingsSaved = false;
            }
        }

        private void obloyParamLb_Click(object sender, EventArgs e)
        {
            if (_isProcessing || _isProcessingFromFolder)
                return;

            var form = new ObloySettingsForm(_imageForTest, _obloyUtils, _capContourUtils);

            if (form.ShowDialog() == DialogResult.OK)
            {
                var settings = _obloyUtils.GetSettings();
                capFlashOffsetNumUpD.Value = (decimal)settings.CapFlashOffset;
                obloyPixCountNumUpD.Value = (decimal)settings.MinAreaObloy;

                _defectSettingsSaved = false;
            }
        }

        private void underfillParamLb_Click(object sender, EventArgs e)
        {
            if (_isProcessing || _isProcessingFromFolder)
                return;

            var form = new UnderfillSettingsForm(_imageForTest, _underfillUtils, _capContourUtils);

            if (form.ShowDialog() == DialogResult.OK)
            {
                var settings = _underfillUtils.GetSettings();
                coefCapRadiusMaskUnderFillNumUpD.Value = (decimal)settings.CoefCapRadiusUnderFill;
                countCorrugationsNumUpD.Value = (decimal)settings.CorrugationsCountForUnderFill;

                _defectSettingsSaved = false;
            }
        }
        #endregion
    }
}