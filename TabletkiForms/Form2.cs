using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using GetImageProject;
using Kvantron.Hardware.SmartDio;
using Kvantron.Ruberoid.Hardwares;
using MathNet.Numerics.IntegralTransforms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace KrishkiForms
{
    public partial class Form2 : Form
    {
        private System.Windows.Forms.ToolTip tooltip = new System.Windows.Forms.ToolTip();

        private Label labelCoordinates = new Label();

        HikCamera cam = new HikCamera(LocalSettings.Instance.Cam1SN);

        bool currentDetectorState = false;
        DioModule module = null;
        bool isFirstImageCam1 = false;
        Mat img1 = new Mat(); //camera image
        bool cameraError1 = false; //error message output
        bool isDrawLines = false;

        bool isStreamCam = false;

        private Bitmap originalImage = null; // Храним оригинальное изображение

        private int frameCounter = 0; // Счётчик кадров
        private System.Windows.Forms.Timer frameProcessingTimer;



        private bool isDrawing = false;
        private System.Drawing.Point startPoint;
        private Rectangle selectedROI;

        private bool isROISelected = false; // Флаг, указывающий, что ROI выбран
        private bool isRoiProduce = false;
        private Mat croppedImage = null; // Хранит обрезанное изображение
        private OpenCvSharp.Rect roi;

        private bool isConfirmVisible = false; // Флаг для отображения кнопок
        private Rectangle checkRect, crossRect; // Области кнопок

        // Глобальные переменные
        private int ellipseReject = 0;
        private int sizeReject = 0;
        private int defectReject = 0;



        private CancellationTokenSource cts;
        private Task processingTask;

        // Глобальные переменные для счетчиков дефектов
        private int ovalityCount = 0;
        private int inclusionCount = 0;
        private int paintDefectCount = 0;
        private int obloyDefectCount = 0;
        private int underfillCount = 0;
        private int blowTriggerCount = 0; // счётчик срабатываний обдува


        private bool isProcessing = false;

        private int MIN_DIAMETER_PX = 500;
        private int MAX_DIAMETER_PX = 700;
        private int HOUGH_DETECT_PARAM_1 = 170;
        private int HOUGH_DETECT_PARAM_2 = 85;
        private int STRIPED_IMAGE_HEIGHT = 30;
        private double TRESHOLD = 2.25;
        private double SCALE = 0.5;

        private const int MIN_HARMONIC_INDEX = 12;
        private const int MAX_HARMONIC_INDEX = 30;

        private ModbusTCP modbusClient;
        private bool obduvEnabled = false;

        private byte[] HUE_LUT = new byte[128 * 128 * 128];

        private DateTime? _lastImageReceivedTime = null;
        private readonly string _logFilePath = "SendImageLog.txt";

        // Добавляем в класс формы
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

        private readonly string settingsFilePath;


        private readonly string ovalityDefectPath;
        private string fileNameForOvalityDefect = $"ovality_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bmp";
        private string fullPathForOvalityDefect = "";
        private double ovalityThreshold = 0.7;
        private Point[] largestContourOvality;
        private double majorAxis;
        private double minorAxis;
        private double axisRatio;

        private readonly string paintDefectPath;
        private string fileNameForPaintDefect = $"paint_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bmp";
        private string fullPathForPaintDefect = "";
        private double minInpaintWhiteTgreshold = 150.0;
        private double minAreaInpaintDefect = 500.0;

        private readonly string inclusionDefectPath;
        private string fileNameForInclusionDefect = $"inclusion_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bmp";
        private string fullPathForInclusionDefect = "";
        private double inclusionThreshold = 0.5;
        private double minAreaInclusion = 50.0;
        private double maxAreaInclusion = 500.0;

        private readonly string obloyDefectPath;
        private string fileNameForObloyDefect = $"obloy_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bmp";
        private string fullPathForObloyDefect = "";
        private Point[] largestContourObloy;
        private Point capCenter;
        private Mat CapRadiusMask = new Mat(532, 568, MatType.CV_8UC1);
        private Mat blurChannel_0;
        private Mat blurChannel_1;
        private Mat blurChannel_2;
        private const float STANDARD_CAP_MAX_RADIUS = 156.0f;
        private const float CAP_FLASH_OFFSET = 5.0f;
        private const int MIN_BINARY_PIXELS_FOR_FLASH_DECISION = 15;

        // Параметры обработки
        private int window = 15;
        private int morph_size = 7;
        private int morph_size_2 = 7;

        // Константы для деколоризации фона
        private const byte BLUE_CAPS = 80;
        private const byte YELLOW_CAPS = 160;
        private const byte GOLD_CAPS = 160; //Заменить на настоящие
        private const byte WHITE_CAPS = 160; //Заменить на настоящие
        private static bool isColored = true;

        private Mat element1;
        private Mat element2;
        private Mat elementMask;

        private double lowerSPercentile = 12.75;
        private double upperSPercentile = 242.75;
        private double lowerVPercentile = 12.75;
        private double upperVPercentile = 242.75;
        private static readonly Scalar lowerMain = new Scalar(0, 255 * 0.05, 255 * 0.05); // 5% от 255
        private static readonly Scalar upperMain = new Scalar(180, 255 * 0.95, 255 * 0.95); // 95% от 255

        private volatile bool newFrameAvailable = false;
        private readonly object frameLock = new object();
        private Mat latestFrame = null;

        private readonly int obduvRegister = 16465;

        private bool cameraConnected = false;
        private bool prConnected = false;

        private bool isObduv = true;
        private bool isInclusion = true;
        private bool isInpaint = true;

        private int _writeZeroFailCount = 0;
        private int _writeOneFailCount = 0;
        private bool obduvState = false; // false = выкл (0), true = вкл (1)

        private List<string> imageFiles = new List<string>();
        private int currentImageIndex = 0;
        private bool isProcessingFromFolder = false;
        private object imageListLock = new object();

        private byte capsColor = 0;
        private int delayValue;

        private int igf = 0;


        public Form2()
        {
            InitializeComponent();
            InitializeHueLUT();

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

            Form1_Load();

            settingsFilePath = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\")), "файлы настроек");
            ovalityDefectPath = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\")), "дефектные крышки", "овальность");
            paintDefectPath = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\")), "дефектные крышки", "непрокрас");
            inclusionDefectPath = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\")), "дефектные крышки", "вкрапления");
            obloyDefectPath = Path.Combine(Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\")), "дефектные крышки", "облой");

            try
            {
                modbusClient = new ModbusTCP("10.10.69.38", 502); // IP ПР205
                modbusClient.Connect();

                if (modbusClient.Connected)
                {
                    MessageBox.Show("Modbus подключение к ПР205 установлено.");

                    prStatus.Text = "Подключено";
                    prStatus.ForeColor = Color.Green;

                    button10.Text = "Отключиться от ПР";
                    prConnected = true; // камеру удалось открыть
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к ПР205.");

                    prStatus.Text = "Не подключено";
                    prStatus.ForeColor = Color.Red;

                    button10.Text = "Подключиться к ПР";
                    prConnected = false; // камеру удалось открыть
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к ПР205: {ex.Message}");
                prStatus.Text = "Не подключено";
                prStatus.ForeColor = Color.Red;
            }


            if (cam.Open())
            {
                cam.SendImage += GetImage;

                camStatus.Text = "Подключено";
                camStatus.ForeColor = Color.Green;

                button11.Text = "Отключиться от камеры";
                cameraConnected = true; // камеру удалось открыть
            }
            else
            {
                MessageBox.Show("Камера 1 - ошибка");

                camStatus.Text = "Не подключено";
                camStatus.ForeColor = Color.Red;

                button11.Text = "Подключиться к камере";
                cameraConnected = false; // камера не подключена
                cameraError1 = true;
            }

            // Подключение к модулю
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

            StartStop(false, true);

            LocalSettings.Instance.Save();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (cameraConnected)
            {
                cam.Close();
                cameraConnected = false;

                button11.Text = "Подключиться к камере";
                camStatus.Text = "Не подключено";
                camStatus.ForeColor = Color.Red;
            }
            else
            {
                if (cam.Open())
                {
                    cam.SendImage += GetImage;
                    cameraConnected = true;

                    button11.Text = "Отключиться от камеры";
                    camStatus.Text = "Подключено";
                    camStatus.ForeColor = Color.Green;
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к камере.");
                    camStatus.Text = "Не подключено";
                    camStatus.ForeColor = Color.Red;
                    cameraConnected = false;
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            // Если уже подключено — отключаем
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
                button10.Text = "Подключиться к ПР";
                prStatus.Text = "Не подключено";
                prStatus.ForeColor = Color.Red;
                return;
            }

            // Подключение
            try
            {
                string ip = textBox5.Text.Trim();
                modbusClient = new ModbusTCP(ip, 502);
                modbusClient.Connect();

                if (modbusClient.Connected)
                {
                    prConnected = true;
                    MessageBox.Show("Modbus подключение к ПР205 установлено.");

                    prStatus.Text = "Подключено";
                    prStatus.ForeColor = Color.Green;
                    button10.Text = "Отключиться от ПР";
                }
                else
                {
                    prConnected = false;
                    MessageBox.Show("Не удалось подключиться к ПР205.");

                    prStatus.Text = "Не подключено";
                    prStatus.ForeColor = Color.Red;
                    button10.Text = "Подключиться к ПР";
                }
            }
            catch (Exception ex)
            {
                prConnected = false;
                MessageBox.Show($"Ошибка подключения к ПР205: {ex.Message}");
                prStatus.Text = "Не подключено";
                prStatus.ForeColor = Color.Red;
                button10.Text = "Подключиться к ПР";
            }
        }


        // Метод инициализации LUT
        private void InitializeHueLUT()
        {
            // Используем Parallel.For для ускорения инициализации
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

        private void Form1_Load()
        {
            // Очищаем все существующие столбцы и строки в первой таблице
            /*dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();

            // Очищаем все существующие столбцы и строки во второй таблице
            dataGridView2.Columns.Clear();
            dataGridView2.Rows.Clear();

            // Добавляем столбцы в обе таблицы
            dataGridView1.Columns.Add("Length", "Length");
            dataGridView1.Columns.Add("Brightness", "Brightness");

            dataGridView2.Columns.Add("Length", "Length");
            dataGridView2.Columns.Add("Brightness", "Brightness");

            // Определяем количество строк (например, 10)
            int rowCount = 10;

            // Добавляем несколько пустых строк в обе таблицы
            for (int i = 0; i < rowCount; i++)
            {
                dataGridView1.Rows.Add(); // Добавляем пустую строку в первую таблицу
                dataGridView2.Rows.Add(); // Добавляем пустую строку во вторую таблицу
            }*/

            labelCoordinates.Location = new System.Drawing.Point(10, 10);
            labelCoordinates.AutoSize = true;
            Controls.Add(labelCoordinates);
        }

        private void loadImageButton_Click(object sender, EventArgs e)
        {
            isStreamCam = false;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    lock (imageListLock)
                    {
                        imageFiles = new List<string>(openFileDialog.FileNames);
                        currentImageIndex = 0;
                        isProcessingFromFolder = imageFiles.Count > 0;
                    }

                    if (imageFiles.Count > 0)
                    {
                        LoadAndDisplayCurrentImage();
                    }
                }
            }
        }

        // Новый метод для загрузки и отображения текущего изображения
        private void LoadAndDisplayCurrentImage()
        {
            lock (imageListLock)
            {
                if (imageFiles.Count == 0) return;

                try
                {
                    using (var imageFromFile = new Mat(imageFiles[currentImageIndex]))
                    {
                        // Обновляем latestFrame для обработки
                        lock (frameLock)
                        {
                            latestFrame?.Dispose();
                            latestFrame = imageFromFile.Clone();
                            newFrameAvailable = true;
                        }

                        // Отображаем текущее изображение
                        originPb.Image = MatToBitmap(imageFromFile.Clone());

                        // Обновляем информацию о текущем изображении
                        /*textBoxCurrentImage.Text = $"{currentImageIndex + 1}/{imageFiles.Count}";
                        textBoxCurrentImageName.Text = Path.GetFileName(imageFiles[currentImageIndex]);*/
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            isStreamCam = false;
            originPb.Image = null;
            inclusionPb.Image = null;
            /*chart1.Series.Clear(); // Удаляем все серии данных из графика
            chart1.ChartAreas.Clear(); // Очищаем области графика
            dataGridView1.Rows.Clear(); // Очищаем все строки в таблице
            chart2.Series.Clear(); // Удаляем все серии данных из графика
            chart2.ChartAreas.Clear(); // Очищаем области графика
            dataGridView2.Rows.Clear(); // Очищаем все строки в таблице*/
        }

        private async void recognizeButton_Click(object sender, EventArgs e)  // Добавили async
        {
            if (isProcessing)
            {
                // Остановить обработку (без блокировки UI)
                cts?.Cancel();
                recognizeButton.Text = "Остановка...";
                recognizeButton.Enabled = false;

                try
                {
                    await processingTask;  // Асинхронное ожидание вместо Wait()
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
                    try
                    {
                        if (modbusClient != null && modbusClient.Connected)
                        {
                            modbusClient.WriteSingleRegister(obduvRegister, 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при выключении обдува: {ex.Message}");
                    }
                    isProcessing = false;
                    recognizeButton.Text = "Начать распознавание";
                    recognizeButton.Enabled = true;
                    cts?.Dispose();
                    cts = null;
                }
            }
            else
            {
                ApplyRecognitionParameters();
                if (originPb.Image == null)
                {
                    MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                    return;
                }

                try
                {
                    if (modbusClient != null && modbusClient.Connected)
                    {
                        modbusClient.WriteSingleRegister(obduvRegister, 0);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при включении обдува: {ex.Message}");
                }

                // Запустить обработку
                cts = new CancellationTokenSource();
                try
                {
                    capsColor = GetSelectedCapValue();
                    int.TryParse(delayTb.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out delayValue);
                    processingTask = Task.Run(() => StartContinuousProcessing(cts.Token));
                    isProcessing = true;
                    recognizeButton.Text = "Остановить распознавание";
                }
                catch
                {
                    cts?.Dispose();
                    throw;
                }
            }
        }

        public int GetDefectFeatureFromRectifiedImage(Mat img0, Mat imgHSV, int radius, Point center, int rectW, int rectH, double threshold)
        {
            Stopwatch timer = new Stopwatch();
            string timeReport = "";

            // Извлекаем канал Saturation из HSV-изображения
            timer.Restart();
            Mat[] hsvChannels = Cv2.Split(imgHSV);
            Mat imgSaturation = hsvChannels[1]; // Берём 2-й канал (Saturation)
            timer.Stop();
            timeReport += $"Извлечение Saturation-канала: {timer.Elapsed.TotalMilliseconds:F2} мс\n";

            // Масштабируем изображение (работаем с Saturation)
            timer.Restart();
            Mat imgScaled = new Mat();
            Cv2.Resize(imgSaturation, imgScaled, new Size(), SCALE, SCALE, InterpolationFlags.Linear);
            timer.Stop();
            timeReport += $"Масштабирование: {timer.Elapsed.TotalMilliseconds:F2} мс\n";

            // --- ПОИСК КОНТУРА И ЭЛЛИПСА ---
            timer.Restart();
            Mat gray = new Mat();
            Cv2.CvtColor(img0, gray, ColorConversionCodes.BGR2GRAY); // Преобразуем в grayscale

            timer.Stop();
            timeReport += $"Определение центра и радиуса через эллипс: {timer.Elapsed.TotalMilliseconds:F2} мс\n";

            // --- СОЗДАНИЕ РАЗВЁРНУТОГО ИЗОБРАЖЕНИЯ ---
            timer.Restart();
            Mat imgRect = new Mat(rectH, rectW, MatType.CV_8UC1);
            double[] sinTable = new double[rectW];
            double[] cosTable = new double[rectW];

            InitTrigTables(sinTable, cosTable, rectW);
            GetStripeImg(img0, imgRect, sinTable, cosTable, center, radius, img0.Width, img0.Height, rectW, rectH);
            //Cv2.ImShow("imgRect", imgRect);
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

            //Cv2.ImShow(decision.ToString(), img0); // Для отладки можно вывести картинку
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
            const int INNER_OFFSET = 7; // Добавляем константу

            int hr = radius - height;
            int[] jc = Enumerable.Range(0, height).Select(j => hr + j + INNER_OFFSET).ToArray(); // Добавляем INNER_OFFSET

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
            Fourier.Forward(spectrum, FourierOptions.NoScaling); // Быстрое Фурье-преобразование (O(n log n))

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
            //ratioTb.Text = ratio.ToString();
            return ratio > threshold ? 1 : 0;
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

                // 16465 = 0x4051 — регистр обдува
                int register = 16465;
                int state = obduvEnabled ? 1 : 0;

                modbusClient.WriteSingleRegister(register, state);

                obduvBatton.Text = obduvEnabled ? "Включить обдув" : "Выключить обдув";
                //MessageBox.Show($"Обдув {(obduvEnabled ? "выключён" : "включен")}");
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка отправки сигнала: {ex.Message}");
            }
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
                hue = 0.0f; // серый цвет
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

            return (byte)(0.5f * hue); // из диапазона 0–360 в 0–180
        }

        // Функция получения HUE-канала всего изображения
        // с учётом перестановки порядка каналов B/G/R = 0/1/2 согласно массиву индексов arrIndex
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

        // Функция перестановки номеров каналов для формирования пурпурных изображений
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

        // Вспомогательная функция обмена значений
        private void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        // Быстрое изменение палитры цветов с индексом децимации sparse
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

        private async Task SetObduv(bool newState)
        {
            if (obduvState != newState)
            {
                obduvState = newState;
                modbusClient.WriteSingleRegister(obduvRegister, newState ? 0 : 1);
                await Task.Delay(delayValue);
            }
        }

        private async Task PulseObduv(int delayMs = 25)
        {
            // Сначала выключаем (если было включено)
            if (obduvState)
            {
                obduvState = false;
                modbusClient.WriteSingleRegister(obduvRegister, 1);
            }

            // Делаем одну паузу
            await Task.Delay(delayValue);

            // Теперь включаем снова
            obduvState = true;
            modbusClient.WriteSingleRegister(obduvRegister, 0);
        }



        private async void StartContinuousProcessing(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    try
                    {

                    }
                    catch (Exception ex)
                    {
                        _writeZeroFailCount++;
                        UpdateTextBox(generalTime, _writeZeroFailCount);

                        BeginInvoke((Action)(() =>
                            MessageBox.Show($"Ошибка инициализации обдува: {ex.Message}")));
                        return; // Прерываем если не удалось проверить/включить обдув
                    }

                    Mat frameToProcess = null;

                    if (isStreamCam)
                    {
                        // Оригинальная логика для камеры
                        lock (frameLock)
                        {
                            if (!newFrameAvailable) continue;
                            frameToProcess = latestFrame.Clone();
                            newFrameAvailable = false;
                        }
                    }

                    else if (isProcessingFromFolder)
                    {
                        await Task.Delay(100);
                        // Логика для изображений из папки
                        lock (imageListLock)
                        {
                            if (imageFiles.Count == 0) continue;

                            try
                            {
                                frameToProcess = new Mat(imageFiles[currentImageIndex]);

                                // Переходим к следующему изображению для следующей итерации
                                currentImageIndex = (currentImageIndex + 1) % imageFiles.Count;


                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                                continue;
                            }
                        }


                    }

                    if (frameToProcess == null || frameToProcess.Empty())
                        continue;

                    using (frameToProcess)
                    using (Mat gray = new Mat())
                    {

                        Cv2.CvtColor(frameToProcess, gray, ColorConversionCodes.BGR2GRAY);
                        Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

                        // Копируем изображения только если соответствующий CheckBox активен
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
                            timeoutCts.CancelAfter(1000000);

                            try
                            {
                                // Инициализируем задачи как завершенные с результатом false
                                var ovalityTask = Task.FromResult(false);
                                var inclusionsTask = Task.FromResult(false);
                                var paintTask = Task.FromResult(false);
                                var obloyTask = Task.FromResult(false);
                                // Запускаем только если CheckBox активен
                                if (ovalityCB.Checked)
                                {
                                    ovalityTask = RunCheckWithTimeout(_grayForOvality, _imageForOvality, timeoutCts.Token, RunCheckOvality);
                                }

                                if (inclusionCB.Checked)
                                {
                                    inclusionsTask = RunCheckWithTimeout(_grayForInclusions, _imageForInclusions, timeoutCts.Token, RunCheckForInclusions);
                                }

                                if (inpaintCB.Checked)
                                {
                                    paintTask = RunCheckWithTimeout(_grayForPaintDefects, _imageForPaintDefects, timeoutCts.Token, RunCheckForPaintDefects);
                                }

                                if (obloyCB.Checked)
                                {
                                    obloyTask = RunCheckWithTimeout(_grayForObloyDefects, _imageForObloyDefects, timeoutCts.Token, RunCheckForObloyDefects);
                                }


                                await Task.WhenAll(ovalityTask, inclusionsTask, paintTask);
                                // Проверяем только те задачи, которые были запущены
                                bool anyDefect = (ovalityCB.Checked && ovalityTask.Result) ||
                                               (inclusionCB.Checked && inclusionsTask.Result) ||
                                               (inpaintCB.Checked && paintTask.Result);

                                if (anyDefect)
                                {
                                    BeginInvoke((Action)(() =>
                                    {
                                        blowTriggerCount++;
                                        textBox4.Text = blowTriggerCount.ToString();
                                    }));
                                }
                                else // нет дефекта
                                {
                                    // Запускаем Modbus-последовательность в отдельном таске
                                    _ = Task.Run(() => PulseObduvAsync(delayValue, token));
                                }


                                stopwatch.Stop();
                                UpdateTextBox(generalTime, stopwatch.ElapsedMilliseconds);

                            }
                            catch (OperationCanceledException)
                            {
                                // ОК
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BeginInvoke((Action)(() =>
                    MessageBox.Show($"Ошибка обработки: {ex.Message}")));
            }
        }

        // Метод для работы с Modbus в отдельном потоке
        private async Task PulseObduvAsync(int delayMs, CancellationToken token)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    var stopwatch = Stopwatch.StartNew();

                    // Включаем обдув
                    modbusClient.WriteSingleRegister(obduvRegister, 1);

                    // Ждём в отдельном таске
                    await Task.Delay(delayMs, token);

                    // Выключаем обдув
                    modbusClient.WriteSingleRegister(obduvRegister, 0);

                    stopwatch.Stop();
                    //UpdateTextBox(imageProcDelay, stopwatch.ElapsedMilliseconds);
                }

            }
            catch (TaskCanceledException)
            {
                // отмена - ничего страшного
            }
            catch (Exception ex)
            {

            }
        }


        private async Task<bool> RunCheckWithTimeout(Mat gray, Mat image, CancellationToken token, Func<Mat, Mat, CancellationToken, bool> checkFunc)
        {
            try
            {
                return await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    return checkFunc(gray, image, token);
                }, token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        private bool RunCheckUnderfill(Mat gray, Mat image, CancellationToken token)
        {
            if (token.IsCancellationRequested) return false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool underfillResult = CheckUnderfill(gray, image); // true = дефект

            if (underfillResult)
            {
                underfillCount++;
                UpdateTextBox(underfillDef, underfillCount);
            }

            stopwatch.Stop();
            UpdateTextBox(timeUnderfill, stopwatch.ElapsedMilliseconds);
            UpdatePictureBox(underfillPictureBox, image);

            return underfillResult;
        }



        private bool RunCheckOvality(Mat gray, Mat image, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Stopwatch stopwatch = Stopwatch.StartNew();

            bool isOval = CheckOvality(gray, image, token); // метод с токеном

            if (isOval)
            {
                ovalityCount++;
                UpdateTextBox(ovalityDef, ovalityCount);
                fileNameForOvalityDefect = $"ovality_{ovalityCount}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.bmp";
                fullPathForOvalityDefect = Path.Combine(ovalityDefectPath, fileNameForOvalityDefect);
                image.SaveImage(fullPathForOvalityDefect);
            }

            stopwatch.Stop();
            UpdateTextBox(timeOvality, stopwatch.ElapsedMilliseconds);
            UpdatePictureBox(ovalityPb, image);

            return isOval;
        }



        private bool RunCheckForInclusions(Mat gray, Mat image, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool hasInclusions = CheckForInclusions(gray, image, token); // true = дефект

            if (hasInclusions)
            {
                inclusionCount++;
                UpdateTextBox(inclusionDef, inclusionCount);
                fileNameForInclusionDefect = $"inclusion_{inclusionCount}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.bmp";
                fullPathForInclusionDefect = Path.Combine(inclusionDefectPath, fileNameForInclusionDefect);
                image.SaveImage(fullPathForInclusionDefect);
            }

            stopwatch.Stop();
            UpdateTextBox(conclusionTime, stopwatch.ElapsedMilliseconds);
            UpdatePictureBox(inclusionPb, image);

            return hasInclusions;
        }


        private bool RunCheckForPaintDefects(Mat gray, Mat image, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool hasPaintDefects = CheckForPaintDefects(gray, image, token); // true = дефект

            if (hasPaintDefects)
            {
                paintDefectCount++;
                UpdateTextBox(InpaintDef, paintDefectCount);
                fileNameForPaintDefect = $"paint_{paintDefectCount}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.bmp";
                fullPathForPaintDefect = Path.Combine(paintDefectPath, fileNameForPaintDefect);
                image.SaveImage(fullPathForPaintDefect);
            }

            stopwatch.Stop();
            UpdateTextBox(inpaintTime, stopwatch.ElapsedMilliseconds);
            UpdatePictureBox(inpaintPb, image);

            return hasPaintDefects;
        }

        private bool RunCheckForObloyDefects(Mat gray, Mat image, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool hasObloyDefects = CheckForObloyDefects(gray, image, token); // true = дефект

            if (hasObloyDefects)
            {
                obloyDefectCount++;
                UpdateTextBox(obloyDef, obloyDefectCount);
                fileNameForObloyDefect = $"obloy_{obloyDefectCount}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.bmp";
                fullPathForObloyDefect = Path.Combine(obloyDefectPath, fileNameForObloyDefect);
                image.SaveImage(fullPathForObloyDefect);
            }

            stopwatch.Stop();
            UpdateTextBox(obloyTime, stopwatch.ElapsedMilliseconds);
            UpdatePictureBox(obloyPb, image);

            return hasObloyDefects;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = @"C:\Users\sergey\Desktop\Krishki\золотые крышки — копия\1.bmp";

            // Загружаем изображение в цвете (BGR)
            Mat image = Cv2.ImRead(path, ImreadModes.Color);

            // Создаём ч/б версию
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

            // 1. Получаем контур крышки
            capsColor = GetSelectedCapValue();

            Stopwatch stopwatch = Stopwatch.StartNew();
            largestContourObloy = GetCapContour(gray, image);

            // 2. Считаем центр как среднее всех точек
            double sumX = 0;
            double sumY = 0;
            foreach (var pt in largestContourObloy)
            {
                sumX += pt.X;
                sumY += pt.Y;
            }
            var capCenter = new Point(
                (int)(sumX / largestContourObloy.Length),
                (int)(sumY / largestContourObloy.Length)
            );

            // 3. Считаем радиус как среднее расстояние до центра
            double radius = 0;
            foreach (var pt in largestContourObloy)
            {
                double dx = pt.X - capCenter.X;
                double dy = pt.Y - capCenter.Y;
                radius += Math.Sqrt(dx * dx + dy * dy);
            }
            radius /= largestContourObloy.Length;

            // 4. Подгоняем маску под размер изображения
            if (CapRadiusMask == null || CapRadiusMask.Size() != image.Size())
            {
                CapRadiusMask?.Dispose();
                CapRadiusMask = new Mat(image.Rows, image.Cols, MatType.CV_8UC1);
            }

            // 5. Строим внешнее кольцо для анализа облоя
            GetIdealCapMask(CapRadiusMask, capCenter, (float)(radius + 3.0f), (float)(radius + 3.0f + CAP_FLASH_OFFSET));

            Cv2.BitwiseAnd(CapRadiusMask, blurChannel_2, blurChannel_2);
            // 8. Эрозия 3x3: результат кладём во второй канал (аналог C++ blur_channels[1])
            Cv2.MorphologyEx(blurChannel_2, blurChannel_1, MorphTypes.Erode, elementMask);

            // 9. Подсчёт бинарных пикселей
            int pixCount = Cv2.CountNonZero(blurChannel_1);

            stopwatch.Stop();
            MessageBox.Show($"Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
            UpdateTextBox(pixelCount, pixCount);


        }


        private bool CheckForObloyDefects(Mat gray, Mat image, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            // 1. Получаем контур крышки
            largestContourObloy = GetCapContour(gray, image);

            // 2. Считаем центр как среднее всех точек
            double sumX = 0;
            double sumY = 0;
            foreach (var pt in largestContourObloy)
            {
                sumX += pt.X;
                sumY += pt.Y;
            }
            var capCenter = new Point(
                (int)(sumX / largestContourObloy.Length),
                (int)(sumY / largestContourObloy.Length)
            );

            // 3. Считаем радиус как среднее расстояние до центра
            double radius = 0;
            foreach (var pt in largestContourObloy)
            {
                double dx = pt.X - capCenter.X;
                double dy = pt.Y - capCenter.Y;
                radius += Math.Sqrt(dx * dx + dy * dy);
            }
            radius /= largestContourObloy.Length;

            // 4. Подгоняем маску под размер изображения
            if (CapRadiusMask == null || CapRadiusMask.Size() != image.Size())
            {
                CapRadiusMask?.Dispose();
                CapRadiusMask = new Mat(image.Rows, image.Cols, MatType.CV_8UC1);
            }

            // 5. Строим внешнее кольцо для анализа облоя
            GetIdealCapMask(CapRadiusMask, capCenter, (float)(radius + 3.0f), (float)(radius + 3.0f + CAP_FLASH_OFFSET));

            Cv2.BitwiseAnd(CapRadiusMask, blurChannel_2, blurChannel_2);
            // 8. Эрозия 3x3: результат кладём во второй канал (аналог C++ blur_channels[1])
            Cv2.MorphologyEx(blurChannel_2, blurChannel_1, MorphTypes.Erode, elementMask);

            // 9. Подсчёт бинарных пикселей
            int pixCount = Cv2.CountNonZero(blurChannel_1);

            return pixCount > MIN_BINARY_PIXELS_FOR_FLASH_DECISION;
        }

        private int CountBinaryPixels(Mat img)
        {
            int count = 0;
            for (int row = 0; row < img.Rows; row++)
                for (int col = 0; col < img.Cols; col++)
                    if (img.At<byte>(row, col) == 255)
                        count++;
            return count;
        }

        // Метод для нахождения центра контура
        private Point FindCapCenter(Point[] contour)
        {
            var moments = Cv2.Moments(contour);
            if (moments.M00 != 0)
            {
                int cx = (int)(moments.M10 / moments.M00);
                int cy = (int)(moments.M01 / moments.M00);
                return new Point(cx, cy);
            }
            return new Point(0, 0);
        }


        // Метод для создания кольца крышки (аналог GetIdealCapMask)
        private void GetIdealCapMask(Mat mask, Point center, float innerRadius, float outerRadius)
        {
            mask.SetTo(0);

            using var outer = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.Black);
            using var inner = new Mat(mask.Size(), MatType.CV_8UC1, Scalar.Black);

            // Заполняем круги
            Cv2.Circle(outer, center, (int)outerRadius, Scalar.White, -1);
            Cv2.Circle(inner, center, (int)innerRadius, Scalar.White, -1);

            // Разность = кольцо
            Cv2.Subtract(outer, inner, mask);
        }


        private int CountBinaryPixelsAndFlashDecision(Mat img, int thresholdCount)
        {
            int count = 0;

            // Обход всех пикселей
            for (int row = 0; row < img.Rows; row++)
            {
                for (int col = 0; col < img.Cols; col++)
                {
                    if (img.At<byte>(row, col) != 0)
                        count++;
                }
            }

            // Возвращаем true, если количество пикселей больше порога
            return count;
        }




        // Потокобезопасное обновление PictureBox
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

        // Потокобезопасное обновление текстового поля
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

        private bool CheckUnderfill(Mat gray, Mat imgColor)
        {
            // Размытие
            Mat smoothImage = new Mat();
            Cv2.BoxFilter(imgColor, smoothImage, -1, new Size(4, 4), new Point(-1, -1), true, BorderTypes.Default);

            // Преобразование в HSV
            Mat imgHSV = new Mat();
            Cv2.CvtColor(smoothImage, imgHSV, ColorConversionCodes.BGR2HSV);

            Point[] capContour = GetCapContour(gray, imgColor); // Получаем контур крышки

            if (capContour == null || capContour.Length < 5)
            {
                // Контур не найден или недостаточно точек для FitEllipse
                return true;
            }

            // Находим эллипс по контуру
            RotatedRect ellipse = Cv2.FitEllipse(capContour);
            Point center = new Point((int)ellipse.Center.X, (int)ellipse.Center.Y);
            int radius = (int)(Math.Max(ellipse.Size.Width, ellipse.Size.Height) / 2);
            //radiusTb.Text = radius.ToString();

            // --- РИСУЕМ НАЙДЕННЫЙ КРУГ ---
            Cv2.Circle(imgColor, center, radius, new Scalar(0, 255, 0), 2); // Круг (зелёный, толщина 2)
            Cv2.Circle(imgColor, center, 5, new Scalar(0, 0, 255), -1); // Центр (красная точка)

            // Получаем признак недолива
            int decision = GetDefectFeatureFromRectifiedImage(imgColor, imgHSV, radius, center, 1024, (int)(radius * 0.08), TRESHOLD);

            if (decision == 1)
            {
                return false;
            }
            else if (decision == 0)
            {
                return true;
            }

            return false;
        }



        private bool CheckCrownRemoval(Mat gray, Mat image)
        {
            // 1. Получаем контур крышки
            Point[] largestContour = GetCapContour(gray, image);
            if (largestContour == null || largestContour.Length < 10)
            {
                MessageBox.Show("Контур крышки не найден.");
                return false;
            }

            // 2. Находим минимальную охватывающую окружность для всего контура (основная крышка)
            Point2f mainCenter;
            float mainRadius;
            Cv2.MinEnclosingCircle(largestContour, out mainCenter, out mainRadius);

            // 3. Фильтруем контур, чтобы найти области, которые могут быть оторванной коронкой
            var potentialCrowns = largestContour
                .Where(p => Math.Sqrt(Math.Pow(mainCenter.X - p.X, 2) + Math.Pow(mainCenter.Y - p.Y, 2)) > mainRadius * 0.6)
                .ToArray();

            if (potentialCrowns.Length < 5) // Если недостаточно точек для коронки
            {
                Cv2.Circle(image, (Point)mainCenter, (int)mainRadius, new Scalar(0, 255, 0), 2); // Основная окружность (крышка)
                Cv2.ImShow("Крышка без дефектов", image);
                return false; // Оторванной коронки нет
            }

            // 4. Находим минимальную охватывающую окружность для потенциальной коронки
            Point2f crownCenter;
            float crownRadius;
            Cv2.MinEnclosingCircle(potentialCrowns, out crownCenter, out crownRadius);

            // 5. Визуализация
            Cv2.DrawContours(image, new[] { largestContour }, -1, new Scalar(0, 0, 255), 2); // Синий контур крышки
            Cv2.Circle(image, (Point)mainCenter, (int)mainRadius, new Scalar(0, 255, 0), 2); // Основная окружность (зелёная)
            Cv2.Circle(image, (Point)crownCenter, (int)crownRadius, new Scalar(255, 0, 0), 2); // Оторванная коронка (красная)
            Cv2.ImShow("Найденные окружности", image);

            return true; // Коронка найдена — потенциальный дефект
        }

        public bool CheckForPaintDefects(Mat gray, Mat image, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            // 1. Конвертация в HSV
            using (Mat hsv = new Mat())
            {
                Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
                token.ThrowIfCancellationRequested();

                // 2. Получение контура таблетки
                Point[] capContour = GetCapContour(gray, image);
                token.ThrowIfCancellationRequested();

                if (capContour == null || capContour.Length == 0)
                {
                    return false;
                }

                // Рисование контура на изображении
                Cv2.Polylines(image, new[] { capContour }, true, new Scalar(255, 0, 0), 2);

                // 3. Создание маски для крышки
                using (Mat capMask = Mat.Zeros(image.Size(), MatType.CV_8UC1))
                {
                    Cv2.FillPoly(capMask, new[] { capContour }, new Scalar(255));
                    token.ThrowIfCancellationRequested();

                    // 4. Применение маски к HSV
                    using (Mat maskedHSV = new Mat())
                    {
                        Cv2.BitwiseAnd(hsv, hsv, maskedHSV, capMask);
                        token.ThrowIfCancellationRequested();

                        // 5. Разделение на каналы HSV
                        Mat[] hsvChannels;
                        Cv2.Split(maskedHSV, out hsvChannels);
                        using (Mat sChannel = hsvChannels[1])
                        using (Mat vChannel = hsvChannels[2])
                        {
                            // 6. Определение пороговых значений
                            token.ThrowIfCancellationRequested();

                            // 7. Создание маски для белого цвета
                            using (Mat whiteMask = new Mat())
                            {
                                Cv2.InRange(hsv, lowerMain, upperMain, whiteMask);
                                token.ThrowIfCancellationRequested();

                                // 8. Инвертирование маски для получения дефектов
                                using (Mat defectsMask = new Mat())
                                {
                                    Cv2.BitwiseNot(whiteMask, defectsMask);

                                    // 9. Наложение маски таблетки на дефекты
                                    using (Mat maskedDefects = new Mat())
                                    {
                                        Cv2.BitwiseAnd(defectsMask, capMask, maskedDefects);
                                        token.ThrowIfCancellationRequested();

                                        // 10. Поиск контуров дефектов
                                        Point[][] contours;
                                        HierarchyIndex[] hierarchy;
                                        Cv2.FindContours(maskedDefects, out contours, out hierarchy,
                                                        RetrievalModes.External,
                                                        ContourApproximationModes.ApproxSimple);

                                        var significantContours = contours.Where(c =>
                                            Cv2.ContourArea(c) > minAreaInpaintDefect).ToList();
                                        token.ThrowIfCancellationRequested();

                                        // 11. Фильтрация по цвету
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

                                        // 12. Визуализация результатов
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
        }

        private bool CheckForInclusions(Mat gray, Mat image, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            // 1. Находим контур крышки
            Point[] bestContour = GetCapContour(gray, image);
            token.ThrowIfCancellationRequested();

            // 2. Аппроксимируем эллипсом
            RotatedRect ellipse = Cv2.FitEllipse(bestContour);
            Point2f ellipseCenter = ellipse.Center;
            float ellipseRadius = (float)(0.7 * (ellipse.Size.Width + ellipse.Size.Height) / 4.0);

            // 3. Визуализация (рисуем круг для отладки)
            Cv2.Circle(image, (Point)ellipseCenter, (int)ellipseRadius, new Scalar(0, 255, 0), 2);

            // 4. Создаем маску круга
            using (Mat mask = Mat.Zeros(gray.Size(), MatType.CV_8UC1))
            using (Mat croppedRegion = new Mat())
            {
                // Рисуем белый круг на маске
                Cv2.Circle(mask, (Point)ellipseCenter, (int)ellipseRadius, new Scalar(255), -1);

                // 5. Вырезаем область круга из исходного изображения
                gray.CopyTo(croppedRegion, mask);

                // 6. Бинаризуем ТОЛЬКО вырезанную область
                using (Mat binary = new Mat())
                using (Mat maskedBinary = new Mat())
                using (Mat filteredBinary = new Mat())
                {
                    Cv2.AdaptiveThreshold(croppedRegion, binary, 255,
                                        AdaptiveThresholdTypes.MeanC,
                                        ThresholdTypes.BinaryInv, 11, 2);

                    // 8. Морфологическая обработка для удаления шума
                    var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
                    Cv2.MorphologyEx(binary, filteredBinary, MorphTypes.Open, kernel, iterations: 1);

                    // 9. Поиск контуров
                    Point[][] inclusionContours;
                    HierarchyIndex[] inclusionHierarchy;
                    Cv2.FindContours(filteredBinary, out inclusionContours, out inclusionHierarchy,
                                   RetrievalModes.List, ContourApproximationModes.ApproxSimple);

                    token.ThrowIfCancellationRequested();

                    // 10. Анализ найденных контуров
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


        private bool IsCircularContour(Point[] contour)
        {
            double perimeter = Cv2.ArcLength(contour, true);
            double area = Cv2.ContourArea(contour);
            double circularity = (4 * Math.PI * area) / (perimeter * perimeter);

            // Круглый контур имеет высокий показатель круглоподобности (близкий к 1)
            return circularity > inclusionThreshold; // Настроить порог для определения округлости
        }

        // ✅ Метод проверки на овальность
        private bool CheckOvality(Mat gray, Mat image, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            largestContourOvality = GetCapContour(gray, image);
            token.ThrowIfCancellationRequested();

            Cv2.DrawContours(image, new[] { largestContourOvality }, -1, new Scalar(255, 0, 0), 2);

            RotatedRect ellipse = Cv2.FitEllipse(largestContourOvality);

            majorAxis = Math.Max(ellipse.Size.Width, ellipse.Size.Height);
            minorAxis = Math.Min(ellipse.Size.Width, ellipse.Size.Height);

            axisRatio = minorAxis / majorAxis;

            token.ThrowIfCancellationRequested();

            bool isOval = axisRatio < ovalityThreshold;

            // Визуализация
            using (Mat resultImage = image.Clone())  // Если нужно сохранить оригинал
            {
                Scalar color = isOval ? new Scalar(0, 0, 255) : new Scalar(0, 255, 0);
                Cv2.Ellipse(image, ellipse, color, 2);
                Cv2.PutText(image, $"Ratio: {axisRatio:F5}", new Point(10, 30),
                           HersheyFonts.HersheySimplex, 1, color, 2);
            }
            return isOval;
        }

        public static void NonlinearBackgroundDecolorization(Mat img, byte nWhite)
        {
            if (img.Empty() || img.Type() != MatType.CV_8UC3)
                throw new ArgumentException("Ожидается 3-канальное 8-битное изображение.");

            int total = img.Rows * img.Cols * 3;
            unsafe
            {
                byte* data = (byte*)img.DataPointer;

                if (!isColored)
                {
                    // Простое выбеливание всех каналов
                    for (int i = 0; i < total; i++)
                    {
                        int val = (255 * data[i]) / nWhite;
                        if (val > 255) val = 255;
                        data[i] = (byte)val;
                    }
                }
                else
                {
                    // Выбеливание и цветоразностная компонента
                    for (int i = 0; i < total; i += 3)
                    {
                        int b = (255 * data[i]) / nWhite;
                        int g = (255 * data[i + 1]) / nWhite;
                        int r = (255 * data[i + 2]) / nWhite;

                        if (b > 255) b = 255;
                        if (g > 255) g = 255;
                        if (r > 255) r = 255;

                        // Цветоразностная компонента: новый B = |B - (G+R)/2|
                        data[i] = (byte)Math.Abs(b - ((g + r) >> 1));

                        // G и R остаются выбеленными
                        data[i + 1] = (byte)g;
                        data[i + 2] = (byte)r;
                    }
                }
            }
        }


        private Point[] GetCapContour(Mat gray, Mat image)
        {
            // Проверка входных данных
            if (gray.Empty() || image.Empty())
                return null;

            // Обработка изображения
            Mat processed = image.Clone();
            NonlinearBackgroundDecolorization(processed, capsColor);
            //Fast_RGB_pseudo_color(processed, 64, HUE_LUT);
            // Cv2.ImShow("sac", processed);

            // Разделение каналов
            Mat[] channels;
            Cv2.Split(processed, out channels);

            // Гауссово размытие (σ = 4) вместо Blur
            Cv2.GaussianBlur(channels[0], channels[1], new Size(window, window), 4);

            // Бинаризация без инверсии (Otsu + Binary)
            Cv2.Threshold(channels[1], channels[0], 128, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);

            // Морфологические операции (как в C++)
            Cv2.MorphologyEx(channels[0], channels[1], MorphTypes.Dilate, element1);
            Cv2.MorphologyEx(channels[1], channels[2], MorphTypes.Erode, element2);

            blurChannel_0 = channels[0];
            blurChannel_1 = channels[1];
            blurChannel_2 = channels[2];
            // ВАЖНО: не делаем финальную инверсию! (bitwise_not убран)

            // Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(
                channels[2],
                out contours,
                out hierarchy,
                RetrievalModes.External,
                ContourApproximationModes.ApproxNone
            );

            // Поиск самого длинного контура
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

        private byte GetSelectedCapValue()
        {
            string selected = comboBox1.SelectedItem?.ToString();
            switch (selected)
            {
                case "Желтые":
                    isColored = true;
                    return YELLOW_CAPS;
                case "Синие":
                    isColored = true;
                    return BLUE_CAPS;
                case "Золотые":
                    isColored = true;
                    return GOLD_CAPS;
                case "Белые":
                    isColored = false;
                    return WHITE_CAPS;
                default:
                    isColored = true; // fallback для всех остальных
                    return YELLOW_CAPS;
            }
        }



        // ✅ Метод проверки на облои
        private bool CheckBurrs(Mat gray, Mat image)
        {
            // 3. Находим самый большой контур (предположительно крышка)
            Point[] largestContour = GetCapContour(gray, image);

            // 4. Аппроксимация эллипсом
            RotatedRect ellipse = Cv2.FitEllipse(largestContour);

            // 5. Рассчет среднего радиуса для построения окружности
            int averageRadius = (int)((ellipse.Size.Width + ellipse.Size.Height) / 4);

            // 6. Рассчитываем длину окружности
            double circumference = 2 * Math.PI * averageRadius;

            // 7. Рисуем окружность (а не эллипс)
            Cv2.Circle(image, (int)ellipse.Center.X, (int)ellipse.Center.Y, averageRadius, new Scalar(0, 255, 0), 2);

            // 8. Визуализация центра окружности
            Cv2.Circle(image, (int)ellipse.Center.X, (int)ellipse.Center.Y, 3, new Scalar(0, 0, 255), -1);

            // 9. Группировка точек за пределами окружности и выделение прямоугольниками
            List<List<Point>> burrGroups = new List<List<Point>>(); // Список групп точек

            List<Point> currentGroup = new List<Point>();
            Point lastPoint = new Point(); // Последняя точка в предыдущей группе

            int pointsOutside = 0; // Количество точек за пределами окружности
            bool isBurrExist = false;

            foreach (var point in largestContour)
            {
                // Расстояние от центра окружности до точки контура
                double distance = Math.Sqrt(Math.Pow(ellipse.Center.X - point.X, 2) + Math.Pow(ellipse.Center.Y - point.Y, 2));

                // Если точка находится за пределами окружности (с учётом радиуса)
                if (distance > averageRadius + 10)
                {
                    pointsOutside++; // Увеличиваем количество точек вне окружности

                    if (currentGroup.Count > 0)
                    {
                        // Проверяем расстояние до предыдущей точки
                        double lastDistance = Math.Sqrt(Math.Pow(lastPoint.X - point.X, 2) + Math.Pow(lastPoint.Y - point.Y, 2));

                        // Если расстояние между точками меньше 100, добавляем точку в текущую группу
                        if (lastDistance < 100)
                        {
                            isBurrExist = true;
                            currentGroup.Add(point);
                        }
                        else
                        {
                            // Иначе, сохраняем текущую группу и начинаем новую
                            burrGroups.Add(currentGroup);
                            currentGroup = new List<Point> { point };
                        }
                    }
                    else
                    {
                        currentGroup.Add(point); // Начинаем новую группу
                    }

                    lastPoint = point; // Обновляем последнюю точку
                }
            }


            // Добавляем последнюю группу, если она не пустая
            if (currentGroup.Count > 0)
            {
                burrGroups.Add(currentGroup);
            }

            // Пороговое значение для минимального количества точек в группе
            int minPointsInGroup = 20; // Например, группа из менее 5 точек не будет выделена

            // Рисуем прямоугольники для каждой группы точек
            foreach (var burrGroup in burrGroups)
            {
                // Пропускаем группы с малым количеством точек
                if (burrGroup.Count < minPointsInGroup)
                {
                    continue;
                }

                // Находим минимальные и максимальные значения для координат прямоугольника
                int xMin = burrGroup.Min(p => p.X);
                int yMin = burrGroup.Min(p => p.Y);
                int xMax = burrGroup.Max(p => p.X);
                int yMax = burrGroup.Max(p => p.Y);

                // Рассчитываем отношение P / L
                double ratio = (double)burrGroup.Count / circumference;

                // Определяем размер облоя (большой или малый)
                Scalar burrColor = ratio > 0.01 ? new Scalar(0, 255, 255) : new Scalar(0, 255, 0); // Желтый для большого облоя, зеленый для малого

                // Рисуем прямоугольник вокруг группы точек
                Rect burrRect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
                Cv2.Rectangle(image, burrRect, burrColor, 2); // Рисуем прямоугольник на изображении

                // Помечаем точки внутри прямоугольника как части облоя
                foreach (var point in burrGroup)
                {
                    if (burrRect.Contains(point))
                    {
                        // Рисуем синие точки внутри облоя
                        Scalar pointColor = new Scalar(255, 0, 0); // Синий цвет для точек в облое
                        Cv2.Circle(image, point, 2, pointColor, -1); // Рисуем синие точки
                    }
                }

                // Выводим информацию о количестве точек и соотношении рядом с облоями
                string text = $"Points: {burrGroup.Count}, Ratio: {ratio * 100:F2}%";
                Point textPosition = new Point(xMin, yMin - 10);
                Cv2.PutText(image, text, textPosition, HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 0, 255), 1);
            }

            // Выводим общую длину окружности на изображение
            string circumferenceText = $"Circumference: {circumference:F2} px";
            Point circumferencePosition = new Point(10, 30);
            Cv2.PutText(image, circumferenceText, circumferencePosition, HersheyFonts.HersheySimplex, 0.7, new Scalar(255, 255, 255), 2);

            // Отображаем количество облоев в MessageBox
            int burrCount = burrGroups.Count(g => g.Count >= minPointsInGroup); // Считаем количество облоев, у которых достаточно точек
            MessageBox.Show($"Количество облоев: {burrCount}");



            // Визуализация
            Cv2.ImShow("Detected Burrs", image);
            Cv2.WaitKey(0);

            return isBurrExist; // Пока только определили круг и облои
        }

        private void applySettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!img1.Empty())
                {
                    endStream_Click(null, null);
                    // Считываем параметры камеры
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

                    getImageButton_Click_1(null, null);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public void GetImage(Mat img)
        {
            DateTime now = DateTime.Now;

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

            _lastImageReceivedTime = now;
            if (isStreamCam)
            {


                lock (frameLock)
                {
                    latestFrame?.Dispose();
                    latestFrame = img.Clone();
                    newFrameAvailable = true;
                }

                // Если ROI не выбран, показываем полное изображение с камеры
                if (!LocalSettings.Instance.UseVConcat)
                {
                    img1 = img.Clone();
                    if (isRoiProduce == true && isROISelected == true)
                    {
                        img1 = new Mat(img, roi);
                    }
                    originPb.Image = MatToBitmap(img1);
                    /*recognizePictureBox.Image = MatToBitmap(img1);
                    pictureBox1.Image = MatToBitmap(img1);
                    underfillPictureBox.Image = MatToBitmap(img1);*/

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
                        originPb.Image = MatToBitmap(img1);
                        /*recognizePictureBox.Image = MatToBitmap(img1);
                        pictureBox1.Image = MatToBitmap(img1);
                        underfillPictureBox.Image = MatToBitmap(img1);*/
                        isFirstImageCam1 = true;

                    }
                    else
                    {
                        Cv2.VConcat(img1, img.Clone(), img1);
                        if (isRoiProduce == true && isROISelected == true)
                        {
                            img1 = new Mat(img, roi);
                        }
                        originPb.Image = MatToBitmap(img1);
                        /*recognizePictureBox.Image = MatToBitmap(img1);
                        pictureBox1.Image = MatToBitmap(img1);
                        underfillPictureBox.Image = MatToBitmap(img1);*/

                    }
                }
            }
        }


        public void GetModuleState(int number, bool state) //функция получения состояния модуля
        {
            if (number == LocalSettings.Instance.DINumber && currentDetectorState == true) //если number равно единице и флаг текущего состояния равен 1,
            {
                module.SetOutput(LocalSettings.Instance.DONumber, true); //то module равен 1

                if (!cameraError1) //если флаг ошибки не выставлен,
                {
                    new System.Threading.Thread(new System.Threading.ThreadStart(() => cam.StartStream())).Start(); //то начать стрим        
                }

                currentDetectorState = false; //сбросить состояние
            }

            else if (number == LocalSettings.Instance.DINumber && currentDetectorState == false) //иначе
            {
                System.Threading.Thread th = new System.Threading.Thread(() =>
                {
                    System.Threading.Thread.Sleep(LocalSettings.Instance.EndStreamDelay); //режим ожидания на 1000

                    module.SetOutput(LocalSettings.Instance.DONumber, false); //module сбросить в 0

                    if (!cameraError1) //если флаг ошибки не выставлен,
                    {
                        new System.Threading.Thread(new System.Threading.ThreadStart(() => cam.EndStream())).Start(); //закончить стрим
                    }

                    currentDetectorState = true; //установить состояние
                });
                th.Start(); //войти в режим ожидания                
            }
        }

        private void StartStop(bool isStart, bool isInit = false)
        {
            if (!isStart)
            {
                if (!isInit && !LocalSettings.Instance.UseModule)
                {

                    if (!cameraError1 && cam.Streamed) cam.EndStream();
                }
                return;
            }


            isFirstImageCam1 = false;

            if (!isInit && !LocalSettings.Instance.UseModule)
            {
                if (!cameraError1 && !cam.Streamed) cam.StartStream();
                img1 = new Mat();
            }
        }

        public Bitmap MatToBitmap(Mat mat) //
        {
            try
            {
                if (!mat.Empty()) //если массив не является пустым множеством,
                {
                    return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat); //возвращает преобразованное в тип bmp изображение
                }
                else return new Bitmap(10, 10); //иначе возвращает новый bmp 10х10
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Bitmap(10, 10);
            }
        }

        private void getImageButton_Click_1(object sender, EventArgs e)
        {
            if (originalImage != null)
            {
                originalImage = null;
            }
            isStreamCam = true;
            /*cam.TriggerMode = false;
            cam.SetTriggerMode();
            cam.SetExposureTime();*/

            ApplyRecognitionParameters();

            ovalityCoef.Enabled = false;
            circleCoefTx.Enabled = false;
            minSquareInclusion.Enabled = false;
            maxSquareInclusion.Enabled = false;
            minSquareInpaint.Enabled = false;
            whiteThresoldTx.Enabled = false;

            StartStop(true);
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
                minAreaInclusion = 50; // значение по умолчанию
                minSquareInclusion.Text = minAreaInclusion.ToString(CultureInfo.InvariantCulture);
            }

            if (!double.TryParse(maxSquareInclusion.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out maxAreaInclusion))
            {
                maxAreaInclusion = 500.0; // значение по умолчанию
                maxSquareInclusion.Text = maxAreaInclusion.ToString(CultureInfo.InvariantCulture);
            }

            if (!double.TryParse(minSquareInpaint.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out minAreaInpaintDefect))
            {
                minAreaInpaintDefect = 500; // значение по умолчанию
                minSquareInpaint.Text = minAreaInpaintDefect.ToString(CultureInfo.InvariantCulture);
            }

            if (!double.TryParse(whiteThresoldTx.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out minInpaintWhiteTgreshold))
            {
                minInpaintWhiteTgreshold = 150.0; // значение по умолчанию
                whiteThresoldTx.Text = minInpaintWhiteTgreshold.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void endStream_Click(object sender, EventArgs e)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteSingleRegister(obduvRegister, 1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выключении обдува: {ex.Message}");
            }

            isRoiProduce = false;
            isROISelected = false;
            isStreamCam = false;
            originPb.Image = null;

            ovalityCoef.Enabled = true;
            circleCoefTx.Enabled = true;
            minSquareInclusion.Enabled = true;
            maxSquareInclusion.Enabled = true;
            minSquareInpaint.Enabled = true;
            whiteThresoldTx.Enabled = true;

            StartStop(false);
        }

        private void saveSettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Собираем значения из текстбоксов
                var settings = new
                {
                    Width = widthTb.Text,
                    Height = heightTb.Text,
                    Exposure = exposureTb.Text,
                    Gain = gainTb.Text
                };

                // Сериализуем в JSON
                string json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                // Открываем диалог сохранения
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    saveFileDialog.Title = "Сохранить настройки";
                    saveFileDialog.FileName = "settings.json";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Сохраняем файл
                        File.WriteAllText(saveFileDialog.FileName, json);
                        MessageBox.Show("Настройки успешно сохранены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении настроек: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Чтение содержимого файла
                        string json = File.ReadAllText(openFileDialog.FileName);

                        // Десериализация в словарь
                        var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        if (settings != null)
                        {
                            // Заполнение текстбоксов
                            widthTb.Text = settings.ContainsKey("Width") ? settings["Width"] : "";
                            heightTb.Text = settings.ContainsKey("Height") ? settings["Height"] : "";
                            exposureTb.Text = settings.ContainsKey("Exposure") ? settings["Exposure"] : "";
                            gainTb.Text = settings.ContainsKey("Gain") ? settings["Gain"] : "";

                            MessageBox.Show("Настройки успешно загружены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось прочитать настройки из файла.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке настроек: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void drawRoi_Click(object sender, EventArgs e)
        {
            isDrawing = true;
            isRoiProduce = true;
            isConfirmVisible = false; // Скрываем кнопки
            originPb.MouseDown += OriginPictureBox_MouseDown;
            originPb.MouseMove += OriginPictureBox_MouseMove;
            originPb.MouseUp += OriginPictureBox_MouseUp;
            originPb.Paint += OriginPictureBox_Paint;
            originPb.MouseClick += OriginPictureBox_MouseClick;
        }

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
                originPb.Invalidate(); // Перерисовываем PictureBox
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
                    // Показываем кнопки подтверждения
                    isConfirmVisible = true;

                    // Определяем размеры и отступы
                    int btnSize = 20;   // Размер кнопок
                    int btnOffset = 5;  // Отступ вниз
                    int btnSpacing = 5; // Расстояние между кнопками

                    int buttonY = selectedROI.Bottom + btnOffset; // Новая Y-координата (ниже рамки)
                    int buttonX = selectedROI.Right - (btnSize * 2 + btnSpacing); // Смещаем кнопки в правый угол

                    // Располагаем кнопки в правом нижнем углу под рамкой
                    checkRect = new Rectangle(buttonX, buttonY, btnSize, btnSize);
                    crossRect = new Rectangle(buttonX + btnSize + btnSpacing, buttonY, btnSize, btnSize);

                    originPb.Invalidate(); // Перерисовываем PictureBox
                }
            }
        }

        // Применение или сброс ROI при клике на кнопки
        private void OriginPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (isConfirmVisible)
            {
                if (checkRect.Contains(e.Location))
                {
                    ApplyROI(); // Применяем ROI ✅
                }
                else if (crossRect.Contains(e.Location))
                {
                    ResetROI(); // Сбрасываем ROI ❌
                }
            }
        }

        // Метод для применения ROI
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
                isConfirmVisible = false; // Скрываем кнопки
                selectedROI = Rectangle.Empty;
                originPb.Image = MatToBitmap(img1);
                originPb.Invalidate();
            }
        }

        // Метод для сброса ROI
        private void ResetROI()
        {
            selectedROI = Rectangle.Empty;
            isConfirmVisible = false; // Скрываем кнопки
            originPb.Invalidate();
        }

        // Метод отрисовки ROI и кнопок ✅❌
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
                        // Рисуем фон кнопок
                        e.Graphics.FillRectangle(brush, checkRect);
                        e.Graphics.FillRectangle(brush, crossRect);
                    }

                    using (Pen penYes = new Pen(Color.Green, 2))
                    using (Pen penNot = new Pen(Color.Red, 2))
                    {
                        // Галочка ✅
                        e.Graphics.DrawLine(penYes, checkRect.Left + 3, checkRect.Top + checkRect.Height / 2,
                                            checkRect.Left + checkRect.Width / 3, checkRect.Bottom - 3);
                        e.Graphics.DrawLine(penYes, checkRect.Left + checkRect.Width / 3, checkRect.Bottom - 3,
                                            checkRect.Right - 3, checkRect.Top + 3);

                        // Крестик ❌
                        e.Graphics.DrawLine(penNot, crossRect.Left + 3, crossRect.Top + 3, crossRect.Right - 3, crossRect.Bottom - 3);
                        e.Graphics.DrawLine(penNot, crossRect.Right - 3, crossRect.Top + 3, crossRect.Left + 3, crossRect.Bottom - 3);
                    }
                }
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseProgramButton_Click(null, null);
        }
        private void CloseProgramButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    // Установить обдув в "отключен" при завершении работы
                    modbusClient.WriteSingleRegister(obduvRegister, 1);
                }
            }
            catch (Exception ex)
            {
                // Логируем или игнорируем, чтобы не мешать закрытию программы
                MessageBox.Show($"Ошибка при установке обдува при завершении: {ex.Message}");
            }

            // Завершение потоков камеры
            if (!cameraError1)
            {
                if (cam.Streamed)
                    cam.EndStream();

                cam.Close();
            }

            if (modbusClient != null && modbusClient.Connected)
            {
                modbusClient.Disconnect();
            }

            Application.Exit();
        }


        private void redrawRoi_Click(object sender, EventArgs e)
        {
            isROISelected = false; // Сбрасываем флаг
            croppedImage = null; // Очищаем обрезанное изображение
        }

        private void ovalityCB_CheckedChanged(object sender, EventArgs e)
        {
            if (ovalityCB.Checked == true)
            {
                ovalityCB.BackColor = Color.Lime;
            }
            if (ovalityCB.Checked == false)
            {
                ovalityCB.BackColor = Color.Red;
            }
        }

        private void inclusionCB_CheckedChanged(object sender, EventArgs e)
        {
            if (inclusionCB.Checked == true)
            {
                inclusionCB.BackColor = Color.Lime;
            }
            if (inclusionCB.Checked == false)
            {
                inclusionCB.BackColor = Color.Red;
            }
        }

        private void inpaintCB_CheckedChanged(object sender, EventArgs e)
        {
            if (inpaintCB.Checked == true)
            {
                inpaintCB.BackColor = Color.Lime;
            }
            if (inpaintCB.Checked == false)
            {
                inpaintCB.BackColor = Color.Red;
            }
        }


    }
}
