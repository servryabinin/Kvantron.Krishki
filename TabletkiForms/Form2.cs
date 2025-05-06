using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using GetImageProject;
using Kvantron.Hardware.SmartDio;
using Kvantron.Ruberoid.Hardwares;
using OpenCvSharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System;
using OpenCvSharp;
using System;
using System.Windows.Forms;
using ScottPlot.Statistics;
using System.Web.Helpers;

using System.Windows.Forms.DataVisualization.Charting;
using Chart = System.Windows.Forms.DataVisualization.Charting.Chart;
using Series = System.Windows.Forms.DataVisualization.Charting.Series;
using GetImageProject;
using Kvantron.Ruberoid.Hardwares;
using Kvantron.Hardware.SmartDio;
using ScottPlot.MultiplotLayouts;

using System.Windows.Forms;
using OpenCvSharp.Extensions;
using static Guna.UI2.Native.WinApi;
using ScottPlot.PlotStyles;
using Point = OpenCvSharp.Point;
using System.Diagnostics;
using ScottPlot.AxisLimitManagers;
using Size = OpenCvSharp.Size;
using OpenCvSharp.Internal.Vectors;
using System.Numerics;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using ScottPlot.Colormaps;
using System.Runtime.Intrinsics.X86;

using EasyModbus;

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




        public Form2()
        {
            InitializeComponent();

            InitializeHueLUT();

            /*chart1.MouseMove += Chart1_MouseMove; // Добавляем обработчик событий
            chart2.MouseMove += Chart1_MouseMove;
            chart1.MouseClick += Chart1_MouseClick;
            chart2.MouseClick += Chart2_MouseClick;*/

            Form1_Load();

            try
            {
                modbusClient = new ModbusTCP("192.168.1.99", 502); // IP ПР205
                modbusClient.Connect();

                if (modbusClient.Connected)
                {
                    MessageBox.Show("Modbus подключение к ПР205 установлено.");
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к ПР205.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к ПР205: {ex.Message}");
            }


            if (cam.Open() == false) //если открытие камеры не удалось,
            {
                MessageBox.Show("камера 1 - ошибка"); //вывести сообщение об ошибке
                cameraError1 = true; //выставить флаг ошибки,
            }
            else //иначе
            {
                cam.SendImage += GetImage; //???
            }

            if (LocalSettings.Instance.UseModule) //если UseModule истинно (по умолчанию истинно),
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

        private void LoadImage(string path)
        {
            if (originalImage != null)
            {
                originalImage.Dispose();
            }
            originalImage = new Bitmap(path);
            originPictureBox.Image = (Bitmap)originalImage.Clone();
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
            // Создаем экземпляр OpenFileDialog
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Устанавливаем фильтр для файлов изображений
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                // Проверяем, выбрал ли пользователь файл и нажал "ОК"
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadImage(openFileDialog.FileName);
                    try
                    {
                        // Загружаем выбранное изображение в PictureBox
                        originPictureBox.Image = new Bitmap(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        // Обрабатываем возможные ошибки (например, если файл не является изображением)
                        MessageBox.Show("Не удалось загрузить изображение: " + ex.Message);
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            isStreamCam = false;
            originPictureBox.Image = null;
            recognizePictureBox.Image = null;
            /*chart1.Series.Clear(); // Удаляем все серии данных из графика
            chart1.ChartAreas.Clear(); // Очищаем области графика
            dataGridView1.Rows.Clear(); // Очищаем все строки в таблице
            chart2.Series.Clear(); // Удаляем все серии данных из графика
            chart2.ChartAreas.Clear(); // Очищаем области графика
            dataGridView2.Rows.Clear(); // Очищаем все строки в таблице*/
        }

        private void recognizeButton_Click(object sender, EventArgs e)
        {
            if (isProcessing)
            {
                // Остановить обработку
                cts?.Cancel();
                processingTask?.Wait();
                isProcessing = false;
                recognizeButton.Text = "Начать распознавание";
            }
            else
            {
                if (originPictureBox.Image == null)
                {
                    MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                    return;
                }

                // Запустить обработку
                cts = new CancellationTokenSource();
                var token = cts.Token;

                processingTask = Task.Run(() => StartContinuousProcessing(token));
                isProcessing = true;
                recognizeButton.Text = "Остановить распознавание";
            }
        }

        private void imagesMinus_Click(object sender, EventArgs e)
        {
            string filePath = "C:/Users/sergey/Desktop/Krishki/TabletkiForms/норм_2.bmp";

            // Загружаем цветное изображение
            Mat imgColor = Cv2.ImRead(filePath, ImreadModes.Color);

            if (imgColor.Empty())
            {
                MessageBox.Show("Ошибка загрузки изображения!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Засекаем время выполнения
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Размытие цветного изображения окном 4x4
            Mat smoothImage = new Mat();
            Cv2.BoxFilter(imgColor, smoothImage, -1, new Size(4, 4), new Point(-1, -1), true, BorderTypes.Default);

            // Преобразование в HSV
            Mat imgHSV = new Mat();
            Cv2.CvtColor(smoothImage, imgHSV, ColorConversionCodes.BGR2HSV);

            // Разделение каналов (берём канал Saturation)
            Mat[] channels;
            Cv2.Split(imgHSV, out channels);
            Mat imgSaturation = channels[1];

            /*// Основная функция по определению дефекта, передаём канал S вместо img0
            int decision = GetDefectFeatureFromRectifiedImage(
                imgColor,           // Исходное grayscale-изображение
                imgHSV,         // HSV-изображение (используется только для Saturation)
                1024,           // rect_w (можно оставить фиксированное значение или вычислять)
                STRIPED_IMAGE_HEIGHT, // rect_h
                TRESHOLD        // threshold
            );


            stopwatch.Stop();
            double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

            // Вывод результата
            //MessageBox.Show($"Результат: {decision}\nВремя выполнения: {elapsedMs:F2} мс", "Результаты анализа");

            textBox8.Text = decision.ToString();*/
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Mat imgColor = CaptureImage();
            // Размытие
            Mat smoothImage = new Mat();
            Cv2.BoxFilter(imgColor, smoothImage, -1, new Size(4, 4), new Point(-1, -1), true, BorderTypes.Default);

            // Преобразование в HSV
            Mat imgHSV = new Mat();
            Cv2.CvtColor(smoothImage, imgHSV, ColorConversionCodes.BGR2HSV);

            Mat gray = new Mat();
            Cv2.CvtColor(imgColor, gray, ColorConversionCodes.BGR2GRAY); // Преобразуем в grayscale

            Point[] capContour = GetCapContour(gray, imgColor); // Получаем контур крышки

            if (capContour == null || capContour.Length < 5)
            {
            }

            // Находим эллипс по контуру
            RotatedRect ellipse = Cv2.FitEllipse(capContour);
            Point center = new Point((int)ellipse.Center.X, (int)ellipse.Center.Y);
            int radius = (int)(Math.Max(ellipse.Size.Width, ellipse.Size.Height) / 2);
            radiusTb.Text = radius.ToString();

            // --- РИСУЕМ НАЙДЕННЫЙ КРУГ ---
            Cv2.Circle(imgColor, center, radius, new Scalar(0, 255, 0), 2); // Круг (зелёный, толщина 2)
            Cv2.Circle(imgColor, center, 5, new Scalar(0, 0, 255), -1); // Центр (красная точка)

            // Получаем признак недолива
            int decision = GetDefectFeatureFromRectifiedImage(imgColor, imgHSV, radius, center, 1024, (int)(radius * 0.08), TRESHOLD);
            Cv2.ImShow("csac", imgColor);

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



        private void button8_Click(object sender, EventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Mat processedImage = CaptureImage();
            Mat originalImage = processedImage.Clone();
            Mat image = processedImage.Clone();
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            OpenCvSharp.Size size = new OpenCvSharp.Size(5, 5);

            double GetContrast(Mat img)
            {
                Scalar mean, stdDev;
                Cv2.MeanStdDev(img, out mean, out stdDev);
                return stdDev.Val0; // Стандартное отклонение как мера контраста
            }

            // Улучшение контраста при необходимости
            if (GetContrast(gray) < 30)
            {
                var clahe = Cv2.CreateCLAHE(clipLimit: 4.0, tileGridSize: new OpenCvSharp.Size(16, 16));
                clahe.Apply(gray, gray);

                Mat sharpened = new Mat();
                Mat sharpenKernel = new Mat(new OpenCvSharp.Size(3, 3), MatType.CV_32F);
                sharpenKernel.SetArray(new float[]
                {
            -1, -1, -1,
            -1,  9, -1,
            -1, -1, -1
                });

                Cv2.Filter2D(gray, sharpened, gray.Depth(), sharpenKernel);
                gray = sharpened;
            }

            // Обработка краев (усиление границ)
            Mat edges = new Mat();
            Cv2.GaussianBlur(gray, gray, size, 1.5);
            Cv2.Canny(gray, edges, 50, 150);

            // Морфологическая обработка для удаления шумов
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, size);
            Cv2.MorphologyEx(edges, edges, MorphTypes.Close, kernel);

            // Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
            {
                edges.Dispose();
            }

            // Выбор самого большого контура
            Point[] bestContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).FirstOrDefault();
            Cv2.DrawContours(originalImage, new[] { bestContour }, -1, Scalar.LimeGreen, 2);

            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;
            MessageBox.Show($"Время выполнения до ImShow: {elapsedMs} мс", "Время");

            Cv2.ImShow("Контур", originalImage);
            edges.Dispose();
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
                MessageBox.Show($"Обдув {(obduvEnabled ? "выключён" : "включен")}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки сигнала: {ex.Message}");
            }
        }



        private void button7_Click(object sender, EventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Mat processedImage = CaptureImage();
            Mat originalImage = processedImage.Clone();
            Mat image = processedImage.Clone();

            image = ProcessImage(processedImage);

            OpenCvSharp.Size size = new OpenCvSharp.Size(5, 5);
            Mat edges = new Mat();
            Cv2.GaussianBlur(image, image, size, 1.5);

            double sigma = 0.33; // Коэффициент регулировки
            double median = (double)Cv2.Mean(image)[0];
            int lower = (int)Math.Max(0, (1.0 - sigma) * median);
            int upper = (int)Math.Min(255, (1.0 + sigma) * median);

            Cv2.Canny(image, edges, lower, upper);

            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, size);
            Cv2.MorphologyEx(edges, edges, MorphTypes.Close, kernel);

            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length > 0)
            {
                Point[] bestContour = contours.OrderByDescending(c => Cv2.ArcLength(c, true)).FirstOrDefault();

                double scaleX = (double)originalImage.Width / image.Width;
                double scaleY = (double)originalImage.Height / image.Height;

                Point[] scaledContour = bestContour.Select(p => new Point((int)(p.X * scaleX), (int)(p.Y * scaleY))).ToArray();
                Cv2.DrawContours(originalImage, new[] { scaledContour }, -1, Scalar.LimeGreen, 2); // Контур
            }

            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;
            MessageBox.Show($"Время выполнения до ImShow: {elapsedMs} мс", "Время");

            Cv2.ImShow("Контур", originalImage);
            edges.Dispose();
        }


        private void button6_Click(object sender, EventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Mat image = CaptureImage(); // захват изображения
            Mat imageCopy = image.Clone();

            // Преобразование в HSV
            Mat imgHSV = new Mat();
            Cv2.CvtColor(image, imgHSV, ColorConversionCodes.BGR2HSV);
            Mat[] hsvChannels = Cv2.Split(imgHSV);

            int window = 31;
            int morphSize = 11;
            int morphSize2 = 11;

            // Структурные элементы для морфологии
            Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(2 * morphSize + 1, 2 * morphSize + 1), new Point(morphSize, morphSize));
            Mat element2 = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(2 * morphSize2 + 1, 2 * morphSize2 + 1), new Point(morphSize2, morphSize2));

            // Гауссов фильтр по каналу H (или можно по S)
            Cv2.GaussianBlur(hsvChannels[0], hsvChannels[1], new Size(window, window), 4);

            // Бинаризация по Отцу
            Cv2.Threshold(hsvChannels[1], hsvChannels[0], 30, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);

            // Морфологические операции: дилатация, эрозия, инверсия
            Cv2.MorphologyEx(hsvChannels[0], hsvChannels[1], MorphTypes.Dilate, element);
            Cv2.MorphologyEx(hsvChannels[1], hsvChannels[2], MorphTypes.Erode, element2);
            Cv2.BitwiseNot(hsvChannels[2], hsvChannels[2]);

            // Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(hsvChannels[2], out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);

            if (contours.Length > 0)
            {
                Point[] bestContour = contours.OrderByDescending(c => Cv2.ArcLength(c, true)).FirstOrDefault();

                if (bestContour.Length >= 5)
                {
                    RotatedRect ellipse = Cv2.FitEllipse(bestContour);
                    Cv2.Ellipse(image, ellipse, Scalar.Red, 2); // Эллипс
                }

                Cv2.DrawContours(image, new[] { bestContour }, -1, Scalar.LimeGreen, 2); // Контур
            }

            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;
            MessageBox.Show($"Время обработки: {elapsedMs} мс", "Замер времени");

            // Отображение результата
            Cv2.ImShow("Контур", image);
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

        // Пример использования в button9_Click
        private void button9_Click(object sender, EventArgs e)
        {
            // Захват изображения
            Mat imgColor = CaptureImage();
            if (imgColor.Empty())
            {
                MessageBox.Show("Изображение не захвачено.");
                return;
            }

            // Конвертация в grayscale
            Mat imgGray = new Mat();
            Cv2.CvtColor(imgColor, imgGray, ColorConversionCodes.BGR2GRAY);

            // Измерение времени выполнения
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Получение контура
            Point[] capContour = GetCapContour(imgGray, imgColor);

            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;

            // Вывод времени выполнения
            string timeInfo = $"Время обработки: {elapsedTime.TotalMilliseconds} мс";
            Console.WriteLine(timeInfo);  // В консоль
            MessageBox.Show(timeInfo);    // В сообщении

            if (capContour != null)
            {
                // Отрисовка контура
                Cv2.DrawContours(imgColor, new Point[][] { capContour }, 0, new Scalar(0, 255, 0), 2);

                // Добавление текста с временем на изображение
                Cv2.PutText(imgColor, timeInfo, new Point(10, 30),
                           HersheyFonts.HersheySimplex, 0.7, new Scalar(0, 0, 255), 2);

                Cv2.ImShow("Result", imgColor);
            }
            else
            {
                MessageBox.Show("Контур не найден");
            }
        }




        private async void StartContinuousProcessing(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Mat image = CaptureImage();
                if (image.Empty()) break;

                Mat gray = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

                // Копии изображений для параллельных потоков
                Mat imageForOvality = image.Clone();
                Mat grayForOvality = gray.Clone();

                Mat imageForInclusions = image.Clone();
                Mat grayForInclusions = gray.Clone();

                Mat imageForPaintDefects = image.Clone();
                Mat grayForPaintDefects = gray.Clone();

                Mat imageForUnderfill = image.Clone();
                Mat grayForUnderfill = gray.Clone();

                // Запуск каждой проверки в своём потоке
                var ovalityTask = Task.Run(() => RunCheckOvality(grayForOvality, imageForOvality, token));
                var inclusionsTask = Task.Run(() => RunCheckForInclusions(grayForInclusions, imageForInclusions, token));
                var paintTask = Task.Run(() => RunCheckForPaintDefects(grayForPaintDefects, imageForPaintDefects, token));
                var underfillTask = Task.Run(() => RunCheckUnderfill(grayForUnderfill, imageForUnderfill, token));

                // Ожидаем завершения всех
                bool[] results = await Task.WhenAll(ovalityTask, inclusionsTask, paintTask, underfillTask);

                bool defectOvality = results[0];
                bool defectInclusion = results[1];
                bool defectPaint = results[2];
                bool defectUnderfill = results[3];

                bool anyDefect = defectOvality || defectInclusion || defectPaint || defectUnderfill;

                try
                {
                    //modbusClient.WriteSingleRegister(16465, anyDefect ? 1 : 0);

                    if (anyDefect)
                    {
                        blowTriggerCount++;
                        UpdateTextBox(textBox4, blowTriggerCount);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при отправке команды на ПЛК: " + ex.Message);
                }

                //await Task.Delay(5);
            }
        }



        private async Task<bool> RunWithTimeout(Func<bool> checkFunc, int timeoutMs)
        {
            var task = Task.Run(checkFunc);
            var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
            if (completed == task)
            {
                return task.Result; // вернёт true = дефект, false = нет дефекта
            }
            else
            {
                return false; // Превышен лимит — считаем, что дефекта нет
            }
        }


        // Гистограмма по прореженному через каждые 16 строк и столбцов изображению
        // Важно: ширина и высота должны быть кратны 16, но обычно так оно и есть
        private void BuildHistFast16(Mat src, int[] hist)
        {
            int w = src.Cols;
            int h = src.Rows;
            hist.Initialize(); // Заполняем массив нулями

            for (int j = 0; j < h; j += 16)
            {
                for (int i = 0; i < w; i += 16)
                {
                    byte intensity = src.At<byte>(j, i);
                    hist[intensity]++;
                }
            }
        }

        // Средняя яркость из прореженной гистограммы
        private byte GetMeanIntensity(Mat src)
        {
            int[] hist = new int[256];
            BuildHistFast16(src, hist);

            int m = 0;
            for (int i = 0; i < 256; i++)
            {
                m += i * hist[i];
            }

            int totalPixels = (src.Cols * src.Rows) >> 8; // Уменьшаем счетчик для прореженной гистограммы
            if (totalPixels > 0)
            {
                m /= totalPixels;
            }

            return (byte)m;
        }

        // Повышение контрастности изображения
        private void GetHighContrastImg(Mat src)
        {
            int w = src.Cols;
            int h = src.Rows;
            int meanIntensity = GetMeanIntensity(src);
            int low = meanIntensity - 5;

            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    int value = 25 * (src.At<byte>(j, i) - low);
                    src.Set<byte>(j, i, (byte)Math.Clamp(value, 0, 255));
                }
            }
        }


        private void button2_Click_1(object sender, EventArgs e)
        {
            const double SCALE = 0.5;
            const int MIN_DIAMETER_PX = 550;
            const int MAX_DIAMETER_PX = 850;
            const double HOUGH_DETECT_PARAM_1 = 170;
            const double HOUGH_DETECT_PARAM_2 = 85;

            // Читаем изображение
            Mat image = CaptureImage();

            // Преобразуем в оттенки серого
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

            // Улучшение контраста
            GetHighContrastImg(gray);
            Cv2.ImShow("GetHighContrastImg", gray);
            //Cv2.ImShow("gray", gray);
            // Масштабируем изображение
            Mat imgShow = new Mat();
            Cv2.Resize(gray, imgShow, new OpenCvSharp.Size(), SCALE, SCALE, InterpolationFlags.Linear);
            Cv2.ImShow("xcbxcb", imgShow);
            // Поиск окружностей методом Хафа
            CircleSegment[] circles = Cv2.HoughCircles(
                imgShow,
                HoughModes.Gradient,
                dp: 1,
                minDist: imgShow.Rows / 256,
                param1: HOUGH_DETECT_PARAM_1,
                param2: HOUGH_DETECT_PARAM_2,
                minRadius: (int)(SCALE * (MIN_DIAMETER_PX / 2)),
                maxRadius: (int)(SCALE * (MAX_DIAMETER_PX / 2))
            );

            // Выбираем самую большую окружность
            Point topCenter = new Point(0, 0);
            int topRadius = 0;

            foreach (var c in circles)
            {
                Point center = new Point((int)c.Center.X, (int)c.Center.Y);
                int radius = (int)c.Radius;

                if (radius > topRadius)
                {
                    topRadius = radius;
                    topCenter = center;
                }
            }

            // Создаём маску бинаризации
            Mat imgBin = new Mat(imgShow.Rows, imgShow.Cols, MatType.CV_8UC1, Scalar.All(0));

            int R2 = topRadius * topRadius;
            for (int j = 0; j < imgShow.Rows; j++)
            {
                for (int k = 0; k < imgShow.Cols; k++)
                {
                    int x2 = topCenter.X - k;
                    int y2 = topCenter.Y - j;
                    int r2 = x2 * x2 + y2 * y2;
                    imgBin.Set<byte>(j, k, (byte)(r2 <= R2 ? 255 : 0));
                }
            }

            // Применяем маску
            Cv2.BitwiseAnd(imgShow, imgBin, imgShow);

            // Откат масштаба
            Point centerScaled = new Point(topCenter.X / SCALE, topCenter.Y / SCALE);
            int radiusScaled = (int)(topRadius / SCALE);

            // Вывод изображения
            Cv2.ImShow("Detected Circle", imgShow);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Mat image = CaptureImage();
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            Mat hsv = new Mat();
            Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);


            // Время для этапа 2 (Получение контура таблетки)
            Stopwatch step2 = new Stopwatch();
            step2.Start();
            // Получение контура таблетки
            Point[] capContour = GetCapContour(gray, image);
            if (capContour == null || capContour.Length == 0)
            {
                MessageBox.Show("Контур таблетки не найден.");
            }
            step2.Stop();

            // Рисование контура на изображении
            Cv2.Polylines(image, new[] { capContour }, true, new Scalar(255, 0, 0), 2); // Зеленый цвет, толщина 2

            // Время для этапа 3 (Маска для крышки)
            Stopwatch step3 = new Stopwatch();
            step3.Start();
            Mat capMask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
            Cv2.FillPoly(capMask, new[] { capContour }, new Scalar(255));
            step3.Stop();
            //Cv2.ImShow("capMask", capMask);
            // Отображение изображения с контуром
            //Cv2.ImShow("Контур крышки", image);


            // Время для этапа 4 (Применение маски к HSV)
            Stopwatch step4 = new Stopwatch();
            step4.Start();
            Mat maskedHSV = new Mat();
            Cv2.BitwiseAnd(hsv, hsv, maskedHSV, capMask);
            step4.Stop();
            //Cv2.ImShow("maskedHSV", maskedHSV);

            // Время для этапа 5 (Разделение на каналы HSV)
            Stopwatch step5 = new Stopwatch();
            step5.Start();
            Mat[] hsvChannels;
            Cv2.Split(maskedHSV, out hsvChannels);
            Mat hChannel = hsvChannels[0];
            Mat sChannel = hsvChannels[1];
            Mat vChannel = hsvChannels[2];
            step5.Stop();

            // Время для этапа 6 (Получение значений для перцентилей)
            Stopwatch step6 = new Stopwatch();
            step6.Start();

            // Статические перцентильные значения для S и V
            int lowerMainBorder = 5;
            int upperMainBorder = 95;

            // Для каналов S и V берем заранее известные минимальные и максимальные значения
            // Эти значения могут быть вычислены на основе стандартных изображений, например, из базы данных
            // или в случае постоянных характеристик изображения они остаются фиксированными.
            double minS = 0;
            double maxS = 255;
            double minV = 0;
            double maxV = 255;

            // Рассчитываем перцентильные значения для S и V
            double lowerSPercentile = minS + (maxS - minS) * (lowerMainBorder / 100.0);
            double upperSPercentile = minS + (maxS - minS) * (upperMainBorder / 100.0);

            double lowerVPercentile = minV + (maxV - minV) * (lowerMainBorder / 100.0);
            double upperVPercentile = minV + (maxV - minV) * (upperMainBorder / 100.0);

            // Результат
            Scalar lowerMain = new Scalar(0, lowerSPercentile, lowerVPercentile);
            Scalar upperMain = new Scalar(180, upperSPercentile, upperVPercentile);

            step6.Stop();

            // Время для этапа 7 (Вычисление перцентилей)
            Stopwatch step7 = new Stopwatch();
            step7.Start();

            // Результат для верхних и нижних перцентилей

            step7.Stop();



            // Время для этапа 8 (Маска для белого цвета)
            Stopwatch step8 = new Stopwatch();
            step8.Start();
            Mat whiteMask = new Mat();
            Cv2.InRange(hsv, lowerMain, upperMain, whiteMask);
            step8.Stop();
            // Cv2.ImShow("whiteMask", whiteMask);

            // Время для этапа 9 (Инвертирование маски)
            Stopwatch step9 = new Stopwatch();
            step9.Start();
            Mat defectsMask = new Mat();
            Cv2.BitwiseNot(whiteMask, defectsMask);
            step9.Stop();
            //Cv2.ImShow("defectsMask", defectsMask);

            // Время для этапа 10 (Наложение маски таблетки на дефекты)
            Stopwatch step10 = new Stopwatch();
            step10.Start();
            Mat maskedDefects = new Mat();
            Cv2.BitwiseAnd(defectsMask, capMask, maskedDefects);
            step10.Stop();
            //Cv2.ImShow("maskedDefects", maskedDefects);

            // Время для этапа 11 (Поиск и фильтрация контуров дефектов)
            Stopwatch step11 = new Stopwatch();
            step11.Start();
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(maskedDefects, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            double minDefectArea;
            double.TryParse(minSquareInpaint.Text, out minDefectArea);
            var significantContours = contours.Where(c => Cv2.ContourArea(c) > minDefectArea).ToList();
            step11.Stop();

            double whiteThresold;
            double.TryParse(whiteThresoldTx.Text, out whiteThresold);

            Stopwatch step12 = new Stopwatch();
            step12.Start();
            var whiteDefects = new List<Point[]>();
            foreach (var contour in significantContours)
            {
                // Создаем маску для текущего контура
                Mat contourMask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
                Cv2.FillPoly(contourMask, new[] { contour }, new Scalar(255));

                // Вычисляем среднее значение яркости пикселей в области контуров
                Mat maskedImage = new Mat();
                Cv2.BitwiseAnd(image, image, maskedImage, contourMask);  // Применяем маску

                // Вычисляем среднее значение всех каналов в маске
                Scalar meanColor = Cv2.Mean(maskedImage, contourMask); // Возвращает среднее значение по каналам (BGR)

                // Среднее значение яркости (значение V в HSV)
                double averageBrightness = meanColor.Val2; // Среднее значение канала V

                // Проверяем, насколько близка средняя яркость к белому (255)
                if (Math.Abs(averageBrightness - 255) < whiteThresold)  // Порог близости к белому
                {
                    whiteDefects.Add(contour);
                }
            }
            step12.Stop();


            // Время для этапа 13 (Отображение найденных дефектов)
            Stopwatch step13 = new Stopwatch();
            step13.Start();
            if (whiteDefects.Count > 0)
            {
                // Рисуем красные контуры для белых дефектов
                Cv2.DrawContours(image, whiteDefects, -1, new Scalar(0, 0, 255), 2);

                // Рисуем желтые рамки вокруг дефектов
                foreach (var contour in whiteDefects)
                {
                    // Находим ограничивающий прямоугольник для каждого контура
                    Rect boundingBox = Cv2.BoundingRect(contour);
                    // Рисуем желтую рамку вокруг контуров
                    Cv2.Rectangle(image, boundingBox, new Scalar(0, 255, 255), 2); // Желтый цвет (BGR)
                }
            }
            step13.Stop();


            // Вывод времени для каждого этапа
            /*MessageBox.Show($"Общее время выполнения: {overallStopwatch.ElapsedMilliseconds} миллисекунд\n" +
                $"1. Конвертация в HSV: {step1.ElapsedMilliseconds} миллисекунд\n" +
                $"2. Получение контура таблетки: {step2.ElapsedMilliseconds} миллисекунд\n" +
                $"3. Маска для крышки: {step3.ElapsedMilliseconds} миллисекунд\n" +
                $"4. Применение маски к HSV: {step4.ElapsedMilliseconds} миллисекунд\n" +
                $"5. Разделение на каналы HSV: {step5.ElapsedMilliseconds} миллисекунд\n" +
                $"6. Получение значений для перцентилей: {step6.ElapsedMilliseconds} миллисекунд\n" +
                $"7. Вычисление перцентилей: {step7.ElapsedMilliseconds} миллисекунд\n" +
                $"8. Маска для белого цвета: {step8.ElapsedMilliseconds} миллисекунд\n" +
                $"9. Инвертирование маски: {step9.ElapsedMilliseconds} миллисекунд\n" +
                $"10. Наложение маски таблетки на дефекты: {step10.ElapsedMilliseconds} миллисекунд\n" +
                $"11. Поиск и фильтрация контуров дефектов: {step11.ElapsedMilliseconds} миллисекунд\n" +
                $"12. Проверка на приближенность к белому цвету: {step12.ElapsedMilliseconds} миллисекунд\n" +
                $"13. Отображение найденных дефектов: {step13.ElapsedMilliseconds} миллисекунд");
*/
            Cv2.ImShow("Обнаруженные дефекты", image);
        }

        private Mat ProcessImage(Mat image)
        {
            const double SCALE = 0.5;
            const int MIN_DIAMETER_PX = 400;
            const int MAX_DIAMETER_PX = 1000;
            const double HOUGH_DETECT_PARAM_1 = 170;
            const double HOUGH_DETECT_PARAM_2 = 85;

            // Преобразуем в оттенки серого и улучшаем контраст
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            EnhanceContrastFast(gray);

            // Масштабируем изображение
            Mat imgShow = new Mat();
            Cv2.Resize(gray, imgShow, new OpenCvSharp.Size(), SCALE, SCALE, InterpolationFlags.Linear);
            //Cv2.ImShow("sac", imgShow);
            // Поиск окружностей методом Хафа
            CircleSegment[] circles = Cv2.HoughCircles(
                imgShow, HoughModes.Gradient, 1, imgShow.Rows / 256,
                HOUGH_DETECT_PARAM_1, HOUGH_DETECT_PARAM_2,
                (int)(SCALE * (MIN_DIAMETER_PX / 2)),
                (int)(SCALE * (MAX_DIAMETER_PX / 2))
            );

            // Выбираем самую большую окружность
            var largestCircle = circles.MaxBy(c => c.Radius);
            if (largestCircle == null)
                return imgShow;  // Если не найдено окружностей, возвращаем изображение как есть

            Point topCenter = new Point((int)largestCircle.Center.X, (int)largestCircle.Center.Y);
            int topRadius = (int)largestCircle.Radius;

            // Создаем бинарную маску
            Mat imgBin = new Mat(imgShow.Size(), MatType.CV_8UC1, new Scalar(0));
            Cv2.Circle(imgBin, topCenter, topRadius, new Scalar(255), -1); // Быстрее, чем Set<byte>

            // Применяем маску
            Cv2.BitwiseAnd(imgShow, imgBin, imgShow);

            // Возвращаем обработанное изображение
            return imgShow;
        }


        /*private Mat ProcessImage(Mat image)
        {
            *//*const double SCALE = 0.5;
            const int MIN_DIAMETER_PX = 400;
            const int MAX_DIAMETER_PX = 1000;
            const double HOUGH_DETECT_PARAM_1 = 170;
            const double HOUGH_DETECT_PARAM_2 = 85;*//*

            Mat smoothImage = new Mat();
            Cv2.BoxFilter(image, smoothImage, -1, new Size(4, 4), new Point(-1, -1), true, BorderTypes.Default);

            // Преобразование в HSV
            Mat imgHSV = new Mat();
            Cv2.CvtColor(smoothImage, imgHSV, ColorConversionCodes.BGR2HSV);

            Mat[] hsvChannels = Cv2.Split(imgHSV);
            Mat imgSaturation = hsvChannels[1]; // Берём 2-й канал (Saturation)

            Mat imgScaled = new Mat();
            Cv2.Resize(imgSaturation, imgScaled, new Size(), SCALE, SCALE, InterpolationFlags.Linear);

            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

            //Cv2.ImShow("gray", gray);
            // Масштабируем изображение
            Mat imgShow = new Mat();
            Cv2.Resize(gray, imgShow, new OpenCvSharp.Size(), SCALE, SCALE, InterpolationFlags.Linear);

            //Cv2.ImShow("sac", imgShow);
            // Поиск окружностей методом Хафа
            CircleSegment[] circles = Cv2.HoughCircles(imgScaled, HoughModes.Gradient, 1, imgScaled.Rows / 16,
                                               HOUGH_DETECT_PARAM_1, HOUGH_DETECT_PARAM_2,
                                               (int)(SCALE * (MIN_DIAMETER_PX / 2)),
                                               (int)(SCALE * (MAX_DIAMETER_PX / 2)));

            // Выбираем самую большую окружность
            var largestCircle = circles.MaxBy(c => c.Radius);
            if (largestCircle == null)
                return imgShow;  // Если не найдено окружностей, возвращаем изображение как есть

            Point topCenter = new Point((int)largestCircle.Center.X, (int)largestCircle.Center.Y);
            int topRadius = (int)largestCircle.Radius;

            // Создаем бинарную маску
            Mat imgBin = new Mat(imgShow.Size(), MatType.CV_8UC1, new Scalar(0));
            Cv2.Circle(imgBin, topCenter, topRadius, new Scalar(255), -1); // Быстрее, чем Set<byte>

            // Применяем маску
            Cv2.BitwiseAnd(imgShow, imgBin, imgShow);
            Cv2.ImShow("imgShow", imgShow);
            // Возвращаем обработанное изображение
            return imgShow;
        }*/

        // Улучшенный метод контрастности через Normalize (без циклов)
        private void EnhanceContrastFast(Mat src)
        {
            int meanIntensity = GetMeanIntensity(src);
            int low = meanIntensity - 5;

            // Используем Normalize вместо ручного вычисления
            Cv2.Normalize(src, src, alpha: 0, beta: 255, normType: NormTypes.MinMax);
        }




        // Параллельная обработка контраста
        private void EnhanceContrastParallel(Mat src)
        {
            int w = src.Cols;
            int h = src.Rows;
            int meanIntensity = GetMeanIntensity(src);
            int low = meanIntensity - 5;

            // Параллельное улучшение контраста
            Parallel.For(0, h, j =>
            {
                for (int i = 0; i < w; i++)
                {
                    int value = 25 * (src.At<byte>(j, i) - low);
                    src.Set<byte>(j, i, (byte)Math.Clamp(value, 0, 255));
                }
            });
        }

        // Создание бинарной маски
        private Mat CreateBinaryMask(Mat imgShow, Point topCenter, int topRadius)
        {
            Mat imgBin = new Mat(imgShow.Rows, imgShow.Cols, MatType.CV_8UC1, Scalar.All(0));

            int R2 = topRadius * topRadius;
            for (int j = 0; j < imgShow.Rows; j++)
            {
                for (int k = 0; k < imgShow.Cols; k++)
                {
                    int x2 = topCenter.X - k;
                    int y2 = topCenter.Y - j;
                    int r2 = x2 * x2 + y2 * y2;
                    imgBin.Set<byte>(j, k, (byte)(r2 <= R2 ? 255 : 0));
                }
            }

            return imgBin;
        }




        private void button1_Click_1(object sender, EventArgs e)
        {
            Mat processedImage = CaptureImage();
            Mat originImage = processedImage.Clone();

            //Cv2.ImShow("originImage", originImage);

            Mat image = ProcessImage(processedImage);
            Cv2.ImShow("4", image);

            OpenCvSharp.Size size = new OpenCvSharp.Size(5, 5);
            Mat gray = new Mat();

            Mat edges = new Mat();
            Cv2.GaussianBlur(image, image, size, 1.5);
            double sigma = 0.33; // Коэффициент регулировки
            double median = (double)Cv2.Mean(image);
            int lower = (int)Math.Max(0, (1.0 - sigma) * median);
            int upper = (int)Math.Min(255, (1.0 + sigma) * median);

            Cv2.Canny(image, edges, lower, upper);
            //Cv2.ImShow("edges", edges);

            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, size);
            Cv2.MorphologyEx(edges, edges, MorphTypes.Close, kernel);

            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            Cv2.ImShow("edges", edges);
            if (contours.Length == 0)
            {
                edges.Dispose();
                return;  // Возвращаем, если контуры не найдены
            }

            Point[] bestContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).FirstOrDefault();

            Scalar color = new Scalar(0, 255, 0);

            // Получаем эллипс для самого подходящего контура
            RotatedRect ellipse = Cv2.FitEllipse(bestContour);

            double scaleX = (double)originImage.Width / image.Width;
            double scaleY = (double)originImage.Height / image.Height;

            ellipse.Center = new Point2f((float)(ellipse.Center.X * scaleX), (float)(ellipse.Center.Y * scaleY));
            ellipse.Size = new Size2f((float)(ellipse.Size.Width * scaleX), (float)(ellipse.Size.Height * scaleY));
            ellipse.Angle = ellipse.Angle;  // Угловое вращение не требует масштабирования

            // Масштабируем контуры
            Point[] scaledContour = bestContour.Select(p => new Point((int)(p.X * scaleX), (int)(p.Y * scaleY))).ToArray();

            // Отображаем эллипс на оригинальном изображении
            Cv2.Ellipse(originImage, ellipse, color, 2);

            // Отображаем контуры (если нужно)
            //Cv2.DrawContours(originImage, new[] { scaledContour }, -1, color, 2);

            Cv2.ImShow("dsv", originImage);
            edges.Dispose();


        }





        /*private void button1_Click_1(object sender, EventArgs e)
        {
            // Загружаем изображение
            Mat image = CaptureImage();

            // 1️⃣ === ПРЕОБРАЗОВАНИЕ В HSV ===
            Mat hsv = new Mat();
            Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);

            // Разделяем каналы
            Mat[] hsvChannels = Cv2.Split(hsv);
            Mat vChannel = hsvChannels[2]; // Канал яркости (Value)

            // 2️⃣ === ПОДАВЛЕНИЕ БЛИКОВ ===
            Mat blurredV = new Mat();
            Cv2.GaussianBlur(vChannel, blurredV, new OpenCvSharp.Size(15, 15), 0); // Сглаживание ярких областей
            Cv2.Min(vChannel, blurredV, vChannel);  // Подавляем блики
            //Cv2.ImShow("blurredV", blurredV);

            // Собираем HSV обратно
            hsvChannels[2] = vChannel;
            Cv2.Merge(hsvChannels, hsv);

            // 3️⃣ === ОБРАТНО В BGR ===
            Mat filteredImage = new Mat();
            Cv2.CvtColor(hsv, filteredImage, ColorConversionCodes.HSV2BGR);

            // 4️⃣ === КОНВЕРТАЦИЯ В LAB ДЛЯ УСИЛЕНИЯ КОНТРАСТА ===
            Mat lab = new Mat();
            Cv2.CvtColor(filteredImage, lab, ColorConversionCodes.BGR2Lab);

            // Улучшаем контраст через CLAHE
            Mat[] labChannels = Cv2.Split(lab);
            Mat lChannel = labChannels[0];

            var clahe = Cv2.CreateCLAHE(clipLimit: 4.0, tileGridSize: new OpenCvSharp.Size(16, 16));
            clahe.Apply(lChannel, lChannel);

            labChannels[0] = lChannel;
            Cv2.Merge(labChannels, lab);

            // 5️⃣ === ОБРАТНО В BGR ===
            Mat enhancedImage = new Mat();
            Cv2.CvtColor(lab, enhancedImage, ColorConversionCodes.Lab2BGR);

            // 6️⃣ === КОНВЕРТАЦИЯ В ЧЕРНО-БЕЛЫЙ + УМЕНЬШЕНИЕ ШУМОВ ===
            Mat gray = new Mat();
            Cv2.CvtColor(enhancedImage, gray, ColorConversionCodes.BGR2GRAY);

            Mat smoothed = new Mat();
            Cv2.BilateralFilter(gray, smoothed, 9, 100, 100); // Используем отдельную переменную
            gray = smoothed; // Обновляем изображение
            Cv2.MedianBlur(gray, gray, 5);

            // 7️⃣ === ПОРОГОВАЯ БИНАРИЗАЦИЯ (Otsu) ===
            Mat binary = new Mat();
            Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.Otsu);

            // 8️⃣ === ПОИСК И ФИЛЬТРАЦИЯ КОНТУРОВ ===
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // Находим самый длинный контур (по периметру)
            Point[] maxContour = contours.OrderByDescending(c => Cv2.ArcLength(c, true)).FirstOrDefault();

            if (maxContour != null && maxContour.Length > 0)
            {
                // 1️⃣ Найти минимальную охватывающую окружность
                Cv2.MinEnclosingCircle(maxContour, out Point2f center, out float radius);

                // 2️⃣ Нарисовать окружность
                Cv2.Circle(image, (Point)center, (int)radius, new Scalar(0, 255, 0), 2);
            }

            // Создаем маску для крышки
            Mat mask = Mat.Zeros(binary.Size(), MatType.CV_8UC1);
            if (maxContour != null)
            {
                Cv2.DrawContours(mask, new[] { maxContour }, -1, Scalar.White, -1);
            }

            // 9️⃣ === ОТРИСОВКА КОНТУРОВ НА ОРИГИНАЛЕ ===
            Random rand = new Random();
            Scalar color = new Scalar(rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256));
            Cv2.DrawContours(image, new[] { maxContour }, -1, color, 2);

            //  🔥 ВЫВОД
            Cv2.ImShow("Filtered Image", image);
            Cv2.ImShow("Final Edges", binary);
            Cv2.WaitKey(0);
        }*/

        /*
         
            Mat smoothed = new Mat();
            Cv2.BilateralFilter(gray, smoothed, 9, 100, 100); // Используем отдельную переменную
            gray = smoothed; // Обновляем изображение
         */




        private Mat CaptureImage()
        {
            // Этот метод должен быть адаптирован для захвата изображения с камеры или другого источника
            string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
            if (isStreamCam)
            {
                img1.SaveImage(tempImagePath);
            }
            else
            {
                originalImage.Save(tempImagePath);
            }
            return Cv2.ImRead(tempImagePath, ImreadModes.Color);
        }

        private bool isUnderfillSaved = false; // Флаг для сохранения одного изображения


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
            if (token.IsCancellationRequested) return false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool isOval = CheckOvality(gray, image); // true = дефект

            if (isOval)
            {
                ovalityCount++;
                UpdateTextBox(ovalityDef, ovalityCount);
            }

            stopwatch.Stop();
            UpdateTextBox(timeOvality, stopwatch.ElapsedMilliseconds);
            UpdatePictureBox(originPictureBox, image);

            return isOval;
        }


        private bool RunCheckForInclusions(Mat gray, Mat image, CancellationToken token)
        {
            if (token.IsCancellationRequested) return false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool hasInclusions = CheckForInclusions(gray, image); // true = дефект

            if (hasInclusions)
            {
                inclusionCount++;
                UpdateTextBox(inclusionDef, inclusionCount);
            }

            stopwatch.Stop();
            UpdateTextBox(conclusionTime, stopwatch.ElapsedMilliseconds);
            UpdatePictureBox(recognizePictureBox, image);

            return hasInclusions;
        }


        private bool RunCheckForPaintDefects(Mat gray, Mat image, CancellationToken token)
        {
            if (token.IsCancellationRequested) return false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool hasPaintDefects = CheckForPaintDefects(gray, image); // true = дефект

            if (hasPaintDefects)
            {
                paintDefectCount++;
                UpdateTextBox(InpaintDef, paintDefectCount);
            }

            stopwatch.Stop();
            UpdateTextBox(inpaintTime, stopwatch.ElapsedMilliseconds);
            UpdatePictureBox(pictureBox1, image);

            return hasPaintDefects;
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

        // Остановка обработки
        private void stopButton_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
            processingTask?.Wait(); // Дожидаемся завершения задачи
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


        // Метод для нахождения оптимальной точки разрыва (для разделения контура на две половины)
        private int FindBestSplitIndex(Point[] contour)
        {
            double maxDistance = 0;
            int bestIndex = 0;

            Point startPoint = contour[0];

            for (int i = 1; i < contour.Length; i++)
            {
                double distance = Math.Sqrt(
                    Math.Pow(startPoint.X - contour[i].X, 2) +
                    Math.Pow(startPoint.Y - contour[i].Y, 2));

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    bestIndex = i; // Самая удалённая точка — потенциальное место разрыва
                }
            }
            return bestIndex;
        }

        // Метод для аппроксимации окружностей по двум половинкам контура
        private bool TryFitCircles(Point[] points1, Point[] points2,
            out Point2f center1, out float radius1,
            out Point2f center2, out float radius2)
        {
            try
            {
                // Аппроксимация окружностей
                Cv2.MinEnclosingCircle(points1, out center1, out radius1);
                Cv2.MinEnclosingCircle(points2, out center2, out radius2);

                // Отбрасываем шум (слишком маленькие окружности)
                if (radius1 < 5 || radius2 < 5)
                    return false;

                return true;
            }
            catch
            {
                center1 = new Point2f();
                radius1 = 0;
                center2 = new Point2f();
                radius2 = 0;
                return false;
            }
        }

        /*private Point[] RefineCapContour(Point[] capContour, Mat image)
        {
            // Аппроксимация окружности вокруг контура
            Point2f center;
            float radius;
            Cv2.MinEnclosingCircle(capContour, out center, out radius);

            // Ищем точки контура, близкие к окружности
            List<Point> arcPoints = new List<Point>();
            foreach (var point in capContour)
            {
                double distance = Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2));
                if (Math.Abs(distance - radius) < 100) // Допуск для отсечения облоев
                {
                    arcPoints.Add(point);
                }
            }

            if (arcPoints.Count < 5)
            {
                MessageBox.Show("Не удалось найти достаточное количество точек для корректной дуги.");
                return capContour; // Возвращаем исходный контур, если дуга не найдена
            }

            // Ищем самую длинную дугу
            List<Point> longestArc = FindLongestArc(arcPoints);

            // Достраиваем окружность по найденной дуге
            Cv2.Polylines(image, new[] { longestArc.ToArray() }, false, new Scalar(0, 255, 0), 2);

            return longestArc.ToArray();
        }

        private List<Point> FindLongestArc(List<Point> arcPoints)
        {
            List<Point> longestArc = new List<Point>();
            List<Point> currentArc = new List<Point>();

            for (int i = 0; i < arcPoints.Count; i++)
            {
                if (i == 0 || Distance(arcPoints[i], arcPoints[i - 1]) < 0) // Если точки рядом — продолжаем дугу
                {
                    currentArc.Add(arcPoints[i]);
                }
                else
                {
                    // Проверка: текущая дуга длиннее предыдущей?
                    if (currentArc.Count > longestArc.Count)
                    {
                        longestArc = new List<Point>(currentArc);
                    }
                    currentArc.Clear();
                    currentArc.Add(arcPoints[i]);
                }
            }

            // Проверка последней дуги
            if (currentArc.Count > longestArc.Count)
            {
                longestArc = currentArc;
            }

            return longestArc;
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }*/


        private static bool capMaskSaved = false;
        public bool CheckForPaintDefects(Mat gray, Mat image)
        {
            Stopwatch overallStopwatch = new Stopwatch();
            overallStopwatch.Start();

            // Время для этапа 1 (Конвертация в HSV)
            Stopwatch step1 = new Stopwatch();
            step1.Start();
            Mat hsv = new Mat();
            Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
            step1.Stop();

            // Время для этапа 2 (Получение контура таблетки)
            Stopwatch step2 = new Stopwatch();
            step2.Start();
            // Получение контура таблетки
            Point[] capContour = GetCapContour(gray, image);
            if (capContour == null || capContour.Length == 0)
            {
                MessageBox.Show("Контур таблетки не найден.");
                return false;
            }
            step2.Stop();

            // Рисование контура на изображении
            Cv2.Polylines(image, new[] { capContour }, true, new Scalar(255, 0, 0), 2); // Зеленый цвет, толщина 2

            // Время для этапа 3 (Маска для крышки)
            Stopwatch step3 = new Stopwatch();
            step3.Start();
            Mat capMask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
            Cv2.FillPoly(capMask, new[] { capContour }, new Scalar(255));
            step3.Stop();
            //Cv2.ImShow("capMask", capMask);
            // Отображение изображения с контуром
            //Cv2.ImShow("Контур крышки", image);


            // Время для этапа 4 (Применение маски к HSV)
            Stopwatch step4 = new Stopwatch();
            step4.Start();
            Mat maskedHSV = new Mat();
            Cv2.BitwiseAnd(hsv, hsv, maskedHSV, capMask);
            step4.Stop();
            //Cv2.ImShow("maskedHSV", maskedHSV);

            // Время для этапа 5 (Разделение на каналы HSV)
            Stopwatch step5 = new Stopwatch();
            step5.Start();
            Mat[] hsvChannels;
            Cv2.Split(maskedHSV, out hsvChannels);
            Mat hChannel = hsvChannels[0];
            Mat sChannel = hsvChannels[1];
            Mat vChannel = hsvChannels[2];
            step5.Stop();

            // Время для этапа 6 (Получение значений для перцентилей)
            Stopwatch step6 = new Stopwatch();
            step6.Start();

            // Статические перцентильные значения для S и V
            int lowerMainBorder = 5;
            int upperMainBorder = 95;

            // Для каналов S и V берем заранее известные минимальные и максимальные значения
            // Эти значения могут быть вычислены на основе стандартных изображений, например, из базы данных
            // или в случае постоянных характеристик изображения они остаются фиксированными.
            double minS = 0;
            double maxS = 255;
            double minV = 0;
            double maxV = 255;

            // Рассчитываем перцентильные значения для S и V
            double lowerSPercentile = minS + (maxS - minS) * (lowerMainBorder / 100.0);
            double upperSPercentile = minS + (maxS - minS) * (upperMainBorder / 100.0);

            double lowerVPercentile = minV + (maxV - minV) * (lowerMainBorder / 100.0);
            double upperVPercentile = minV + (maxV - minV) * (upperMainBorder / 100.0);

            // Результат
            Scalar lowerMain = new Scalar(0, lowerSPercentile, lowerVPercentile);
            Scalar upperMain = new Scalar(180, upperSPercentile, upperVPercentile);

            step6.Stop();

            // Время для этапа 7 (Вычисление перцентилей)
            Stopwatch step7 = new Stopwatch();
            step7.Start();

            // Результат для верхних и нижних перцентилей

            step7.Stop();



            // Время для этапа 8 (Маска для белого цвета)
            Stopwatch step8 = new Stopwatch();
            step8.Start();
            Mat whiteMask = new Mat();
            Cv2.InRange(hsv, lowerMain, upperMain, whiteMask);
            step8.Stop();
            // Cv2.ImShow("whiteMask", whiteMask);

            // Время для этапа 9 (Инвертирование маски)
            Stopwatch step9 = new Stopwatch();
            step9.Start();
            Mat defectsMask = new Mat();
            Cv2.BitwiseNot(whiteMask, defectsMask);
            step9.Stop();
            //Cv2.ImShow("defectsMask", defectsMask);

            // Время для этапа 10 (Наложение маски таблетки на дефекты)
            Stopwatch step10 = new Stopwatch();
            step10.Start();
            Mat maskedDefects = new Mat();
            Cv2.BitwiseAnd(defectsMask, capMask, maskedDefects);
            step10.Stop();
            //Cv2.ImShow("maskedDefects", maskedDefects);

            // Время для этапа 11 (Поиск и фильтрация контуров дефектов)
            Stopwatch step11 = new Stopwatch();
            step11.Start();
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(maskedDefects, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            double minDefectArea;
            double.TryParse(minSquareInpaint.Text, out minDefectArea);
            var significantContours = contours.Where(c => Cv2.ContourArea(c) > minDefectArea).ToList();
            step11.Stop();

            double whiteThresold;
            double.TryParse(whiteThresoldTx.Text, out whiteThresold);

            Stopwatch step12 = new Stopwatch();
            step12.Start();
            var whiteDefects = new List<Point[]>();
            foreach (var contour in significantContours)
            {
                // Создаем маску для текущего контура
                Mat contourMask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
                Cv2.FillPoly(contourMask, new[] { contour }, new Scalar(255));

                // Вычисляем среднее значение яркости пикселей в области контуров
                Mat maskedImage = new Mat();
                Cv2.BitwiseAnd(image, image, maskedImage, contourMask);  // Применяем маску

                // Вычисляем среднее значение всех каналов в маске
                Scalar meanColor = Cv2.Mean(maskedImage, contourMask); // Возвращает среднее значение по каналам (BGR)

                // Среднее значение яркости (значение V в HSV)
                double averageBrightness = meanColor.Val2; // Среднее значение канала V

                // Проверяем, насколько близка средняя яркость к белому (255)
                if (Math.Abs(averageBrightness - 255) < whiteThresold)  // Порог близости к белому
                {
                    whiteDefects.Add(contour);
                }
            }
            step12.Stop();


            // Время для этапа 13 (Отображение найденных дефектов)
            Stopwatch step13 = new Stopwatch();
            step13.Start();
            if (whiteDefects.Count > 0)
            {
                // Рисуем красные контуры для белых дефектов
                Cv2.DrawContours(image, whiteDefects, -1, new Scalar(0, 0, 255), 2);

                // Рисуем желтые рамки вокруг дефектов
                foreach (var contour in whiteDefects)
                {
                    // Находим ограничивающий прямоугольник для каждого контура
                    Rect boundingBox = Cv2.BoundingRect(contour);
                    // Рисуем желтую рамку вокруг контуров
                    Cv2.Rectangle(image, boundingBox, new Scalar(0, 255, 255), 2); // Желтый цвет (BGR)
                }
            }
            step13.Stop();

            overallStopwatch.Stop();

            // Вывод времени для каждого этапа
            /*MessageBox.Show($"Общее время выполнения: {overallStopwatch.ElapsedMilliseconds} миллисекунд\n" +
                $"1. Конвертация в HSV: {step1.ElapsedMilliseconds} миллисекунд\n" +
                $"2. Получение контура таблетки: {step2.ElapsedMilliseconds} миллисекунд\n" +
                $"3. Маска для крышки: {step3.ElapsedMilliseconds} миллисекунд\n" +
                $"4. Применение маски к HSV: {step4.ElapsedMilliseconds} миллисекунд\n" +
                $"5. Разделение на каналы HSV: {step5.ElapsedMilliseconds} миллисекунд\n" +
                $"6. Получение значений для перцентилей: {step6.ElapsedMilliseconds} миллисекунд\n" +
                $"7. Вычисление перцентилей: {step7.ElapsedMilliseconds} миллисекунд\n" +
                $"8. Маска для белого цвета: {step8.ElapsedMilliseconds} миллисекунд\n" +
                $"9. Инвертирование маски: {step9.ElapsedMilliseconds} миллисекунд\n" +
                $"10. Наложение маски таблетки на дефекты: {step10.ElapsedMilliseconds} миллисекунд\n" +
                $"11. Поиск и фильтрация контуров дефектов: {step11.ElapsedMilliseconds} миллисекунд\n" +
                $"12. Проверка на приближенность к белому цвету: {step12.ElapsedMilliseconds} миллисекунд\n" +
                $"13. Отображение найденных дефектов: {step13.ElapsedMilliseconds} миллисекунд");
*/
            //Cv2.ImShow("Обнаруженные дефекты", image);

            return whiteDefects.Count > 0;
        }


        // Функция для проверки на приближенность к белому цвету
        private bool IsApproximatelyWhite(Vec3b pixel)
        {
            int r = pixel.Item0;
            int g = pixel.Item1;
            int b = pixel.Item2;

            // Проверка, чтобы значения всех каналов близки к 255
            return (Math.Abs(r - 255) < 150) && (Math.Abs(g - 255) < 150) && (Math.Abs(b - 255) < 150);
        }

        // Функция для вычисления перцентилей
        private static double Percentile(List<byte> sequence, double percentile)
        {
            if (sequence.Count == 0) return 0;
            sequence.Sort();
            int index = (int)Math.Floor((percentile / 100.0) * sequence.Count);
            return sequence[Math.Clamp(index, 0, sequence.Count - 1)];
        }

        /*
         public bool CheckForPaintDefects(Mat gray, Mat image)
        {
            Stopwatch overallStopwatch = new Stopwatch();
            overallStopwatch.Start();

            // Время для этапа 1 (Конвертация в HSV)
            Stopwatch step1 = new Stopwatch();
            step1.Start();
            Mat hsv = new Mat();
            Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
            step1.Stop();

            // Время для этапа 2 (Получение контура таблетки)
            Stopwatch step2 = new Stopwatch();
            step2.Start();
            Point[] capContour = GetCapContour(gray, image);
            if (capContour == null || capContour.Length == 0)
            {
                MessageBox.Show("Контур таблетки не найден.");
                return false;
            }
            step2.Stop();

            // Время для этапа 3 (Маска для крышки)
            Stopwatch step3 = new Stopwatch();
            step3.Start();
            Mat capMask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
            Cv2.FillPoly(capMask, new[] { capContour }, new Scalar(255));
            step3.Stop();

            // Время для этапа 4 (Применение маски к HSV)
            Stopwatch step4 = new Stopwatch();
            step4.Start();
            Mat maskedHSV = new Mat();
            Cv2.BitwiseAnd(hsv, hsv, maskedHSV, capMask);
            step4.Stop();

            // Время для этапа 5 (Разделение на каналы HSV)
            Stopwatch step5 = new Stopwatch();
            step5.Start();
            Mat[] hsvChannels;
            Cv2.Split(maskedHSV, out hsvChannels);
            Mat hChannel = hsvChannels[0];
            Mat sChannel = hsvChannels[1];
            Mat vChannel = hsvChannels[2];
            step5.Stop();

            // Время для этапа 6 (Получение значений для перцентилей)
            Stopwatch step6 = new Stopwatch();
            step6.Start();

            // Статические перцентильные значения для S и V
            int lowerMainBorder = 5;
            int upperMainBorder = 95;

            // Для каналов S и V берем заранее известные минимальные и максимальные значения
            // Эти значения могут быть вычислены на основе стандартных изображений, например, из базы данных
            // или в случае постоянных характеристик изображения они остаются фиксированными.
            double minS = 0;
            double maxS = 255;
            double minV = 0;
            double maxV = 255;

            // Рассчитываем перцентильные значения для S и V
            double lowerSPercentile = minS + (maxS - minS) * (lowerMainBorder / 100.0);
            double upperSPercentile = minS + (maxS - minS) * (upperMainBorder / 100.0);

            double lowerVPercentile = minV + (maxV - minV) * (lowerMainBorder / 100.0);
            double upperVPercentile = minV + (maxV - minV) * (upperMainBorder / 100.0);

            // Результат
            Scalar lowerMain = new Scalar(0, lowerSPercentile, lowerVPercentile);
            Scalar upperMain = new Scalar(180, upperSPercentile, upperVPercentile);

            step6.Stop();

            // Время для этапа 7 (Вычисление перцентилей)
            Stopwatch step7 = new Stopwatch();
            step7.Start();

            // Результат для верхних и нижних перцентилей
            
            step7.Stop();



            // Время для этапа 8 (Маска для белого цвета)
            Stopwatch step8 = new Stopwatch();
            step8.Start();
            Mat whiteMask = new Mat();
            Cv2.InRange(hsv, lowerMain, upperMain, whiteMask);
            step8.Stop();
            Cv2.ImShow("whiteMask", whiteMask);

            // Время для этапа 9 (Инвертирование маски)
            Stopwatch step9 = new Stopwatch();
            step9.Start();
            Mat defectsMask = new Mat();
            Cv2.BitwiseNot(whiteMask, defectsMask);
            step9.Stop();
            Cv2.ImShow("defectsMask", defectsMask);

            // Время для этапа 10 (Наложение маски таблетки на дефекты)
            Stopwatch step10 = new Stopwatch();
            step10.Start();
            Mat maskedDefects = new Mat();
            Cv2.BitwiseAnd(defectsMask, capMask, maskedDefects);
            step10.Stop();
            Cv2.ImShow("maskedDefects", maskedDefects);

            // Время для этапа 11 (Поиск и фильтрация контуров дефектов)
            Stopwatch step11 = new Stopwatch();
            step11.Start();
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(maskedDefects, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            double minDefectArea = 50; // Порог по площади
            var significantContours = contours.Where(c => Cv2.ContourArea(c) > minDefectArea).ToList();
            step11.Stop();

            Stopwatch step12 = new Stopwatch();
            step12.Start();
            var whiteDefects = new List<Point[]>();
            foreach (var contour in significantContours)
            {
                // Создаем маску для текущего контура
                Mat contourMask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
                Cv2.FillPoly(contourMask, new[] { contour }, new Scalar(255));

                // Вычисляем среднее значение яркости пикселей в области контуров
                Mat maskedImage = new Mat();
                Cv2.BitwiseAnd(image, image, maskedImage, contourMask);  // Применяем маску

                // Вычисляем среднее значение всех каналов в маске
                Scalar meanColor = Cv2.Mean(maskedImage, contourMask); // Возвращает среднее значение по каналам (BGR)

                // Среднее значение яркости (значение V в HSV)
                double averageBrightness = meanColor.Val2; // Среднее значение канала V

                // Проверяем, насколько близка средняя яркость к белому (255)
                if (Math.Abs(averageBrightness - 255) < 150)  // Порог близости к белому
                {
                    whiteDefects.Add(contour);
                }
            }
            step12.Stop();


            // Время для этапа 13 (Отображение найденных дефектов)
            Stopwatch step13 = new Stopwatch();
            step13.Start();
            if (whiteDefects.Count > 0)
            {
                // Рисуем красные контуры для белых дефектов
                Cv2.DrawContours(image, whiteDefects, -1, new Scalar(0, 0, 255), 2);

                // Рисуем желтые рамки вокруг дефектов
                foreach (var contour in whiteDefects)
                {
                    // Находим ограничивающий прямоугольник для каждого контура
                    Rect boundingBox = Cv2.BoundingRect(contour);
                    // Рисуем желтую рамку вокруг контуров
                    Cv2.Rectangle(image, boundingBox, new Scalar(0, 255, 255), 2); // Желтый цвет (BGR)
                }
            }
            step13.Stop();

            overallStopwatch.Stop();

            // Вывод времени для каждого этапа
            MessageBox.Show($"Общее время выполнения: {overallStopwatch.ElapsedMilliseconds} миллисекунд\n" +
                $"1. Конвертация в HSV: {step1.ElapsedMilliseconds} миллисекунд\n" +
                $"2. Получение контура таблетки: {step2.ElapsedMilliseconds} миллисекунд\n" +
                $"3. Маска для крышки: {step3.ElapsedMilliseconds} миллисекунд\n" +
                $"4. Применение маски к HSV: {step4.ElapsedMilliseconds} миллисекунд\n" +
                $"5. Разделение на каналы HSV: {step5.ElapsedMilliseconds} миллисекунд\n" +
                $"6. Получение значений для перцентилей: {step6.ElapsedMilliseconds} миллисекунд\n" +
                $"7. Вычисление перцентилей: {step7.ElapsedMilliseconds} миллисекунд\n" +
                $"8. Маска для белого цвета: {step8.ElapsedMilliseconds} миллисекунд\n" +
                $"9. Инвертирование маски: {step9.ElapsedMilliseconds} миллисекунд\n" +
                $"10. Наложение маски таблетки на дефекты: {step10.ElapsedMilliseconds} миллисекунд\n" +
                $"11. Поиск и фильтрация контуров дефектов: {step11.ElapsedMilliseconds} миллисекунд\n" +
                $"12. Проверка на приближенность к белому цвету: {step12.ElapsedMilliseconds} миллисекунд\n" +
                $"13. Отображение найденных дефектов: {step13.ElapsedMilliseconds} миллисекунд");

            Cv2.ImShow("Обнаруженные дефекты", image);

            return whiteDefects.Count > 0;
        }*/





        private bool CheckForInclusions(Mat gray, Mat image)
        {
            Point[] bestContour = GetCapContour(gray, image);

            // Аппроксимация эллипсом методом наименьших квадратов
            RotatedRect ellipse = Cv2.FitEllipse(bestContour);
            Point2f ellipseCenter = ellipse.Center;
            float ellipseRadius = (float)(0.7 * (ellipse.Size.Width + ellipse.Size.Height) / 4.0); // Радиус окружности

            // Рисуем окружность внутри эллипса (гладкая сторона крышки)
            Cv2.Circle(image, (Point)ellipseCenter, (int)ellipseRadius, new Scalar(0, 255, 0), 2);

            // Создаем маску для поиска вкраплений внутри окружности
            Mat mask = Mat.Zeros(gray.Size(), MatType.CV_8UC1);
            Cv2.Circle(mask, (Point)ellipseCenter, (int)(ellipseRadius), new Scalar(255), -1); // Маска внутри окружности

            // Бинаризация исходного изображения для поиска темных вкраплений
            Mat binary = new Mat();
            Cv2.AdaptiveThreshold(gray, binary, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 11, 2);
            //Cv2.ImShow("sfsd", binary);

            // Применяем маску для поиска только внутри окружности
            Mat maskedBinary = new Mat();
            binary.CopyTo(maskedBinary, mask);

            // Дополнительная фильтрация (поиск маленьких объектов)
            Mat filteredBinary = new Mat();
            Cv2.Erode(maskedBinary, filteredBinary, new Mat(), iterations: 2); // Уменьшаем шум
            //Cv2.ImShow("sfsd", filteredBinary);
            // Поиск контуров (предположительно вкраплений) внутри окружности
            Point[][] inclusionContours;
            HierarchyIndex[] inclusionHierarchy;
            Cv2.FindContours(filteredBinary, out inclusionContours, out inclusionHierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            bool inclusionsFound = false;
            double minArea;
            double.TryParse(minSquareInclusion.Text, out minArea);
            double maxArea;
            double.TryParse(maxSquareInclusion.Text, out maxArea);
            // Фильтрация контуров по площади — предполагаем, что вкрапления — маленькие черные точки
            foreach (var contour in inclusionContours)
            {
                double area = Cv2.ContourArea(contour);

                // Фильтрация по размеру области
                if (area > minArea && area < maxArea) // Настроить под размер вкраплений
                {
                    // Проверка на форму: чем ближе объект к окружности, тем вероятнее это вкрапление
                    if (IsCircularContour(contour))
                    {
                        // Находим bounding box вокруг вкрапления
                        Rect bbox = Cv2.BoundingRect(contour);

                        // Рисуем красные рамки на исходном изображении
                        Cv2.Rectangle(image,
                            new Point(bbox.X, bbox.Y),
                            new Point(bbox.X + bbox.Width, bbox.Y + bbox.Height),
                            new Scalar(0, 0, 255), 2); // Красный цвет для рамок

                        inclusionsFound = true;
                    }
                }
            }

            // Показываем результаты
            //Cv2.ImShow("Final Image with Inclusions", image);
            //Cv2.WaitKey(0);

            binary.Dispose();
            mask.Dispose();
            filteredBinary.Dispose();

            return inclusionsFound;
        }

        private bool IsCircularContour(Point[] contour)
        {
            double circleCoef;
            double.TryParse(circleCoefTx.Text, out circleCoef);
            double perimeter = Cv2.ArcLength(contour, true);
            double area = Cv2.ContourArea(contour);
            double circularity = (4 * Math.PI * area) / (perimeter * perimeter);

            // Круглый контур имеет высокий показатель круглоподобности (близкий к 1)
            return circularity > circleCoef; // Настроить порог для определения округлости
        }


        /*private bool CheckForPaintDefects(Mat gray, Mat image)
        {
            // Обнаружение границ с помощью оператора Канни
            Mat edges = new Mat();
            if (gray.Empty())
            {
                MessageBox.Show("Ошибка: серое изображение пустое или не загружено!");
                return false;
            }

            Cv2.Canny(gray, edges, 100, 200);

            // Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
            {
                edges.Dispose();
                return false; // Контуры не найдены — крышка не обнаружена
            }

            // Поиск самого большого контура (предположительно крышки)
            Point[] largestContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).FirstOrDefault();

            if (largestContour == null || largestContour.Length < 5)
            {
                edges.Dispose();
                return false; // Нормальный контур не найден — крышка не обнаружена
            }

            // Аппроксимация эллипсом методом наименьших квадратов
            RotatedRect ellipse = Cv2.FitEllipse(largestContour);

            // Визуализация эллипса (зелёная рамка)
            Cv2.Ellipse(image, ellipse, new Scalar(0, 255, 0), 2);

            // Создаём маску эллипса
            Mat mask = Mat.Zeros(gray.Size(), MatType.CV_8UC1);
            Cv2.Ellipse(mask, ellipse, new Scalar(255), -1); // Белый эллипс на черном фоне

            // Накладываем маску на изображение
            Mat maskedGray = new Mat();
            gray.CopyTo(maskedGray, mask);

            // Порог для ярких (непрокрашенных) областей
            Mat brightSpots = new Mat();
            Cv2.Threshold(maskedGray, brightSpots, 200, 255, ThresholdTypes.Binary); // 200 — порог яркости (настраиваемый)

            // Поиск контуров ярких областей (предположительно — непрокрасы)
            Point[][] brightContours;
            HierarchyIndex[] brightHierarchy;
            Cv2.FindContours(brightSpots, out brightContours, out brightHierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            bool paintDefectFound = false;

            // Фильтрация по размеру области (настраиваем диапазон)
            foreach (var contour in brightContours)
            {
                double area = Cv2.ContourArea(contour);

                if (area > 50 && area < 1000) // Подобрать минимальный и максимальный размеры дефектов
                {
                    Rect bbox = Cv2.BoundingRect(contour);
                    Cv2.Rectangle(image, bbox, new Scalar(0, 0, 255), 2); // Красная рамка вокруг дефекта
                    paintDefectFound = true;
                }
            }

            // Визуализация
            Cv2.ImShow("Paint Defects Detection", image);
            Cv2.WaitKey(0);

            // Освобождение ресурсов
            edges.Dispose();
            mask.Dispose();
            maskedGray.Dispose();
            brightSpots.Dispose();

            return paintDefectFound;
        }*/


        // ✅ Метод проверки на овальность
        private bool CheckOvality(Mat gray, Mat image)
        {

            Point[] largestContour = GetCapContour(gray, image);
            Cv2.DrawContours(image, new[] { largestContour }, -1, new Scalar(255, 0, 0), 2); // Красный контур толщиной 2px
            // Аппроксимация эллипсом методом наименьших квадратов
            RotatedRect ellipse = Cv2.FitEllipse(largestContour);

            // Вычисление длин осей эллипса
            double majorAxis = Math.Max(ellipse.Size.Width, ellipse.Size.Height); // большая ось
            double minorAxis = Math.Min(ellipse.Size.Width, ellipse.Size.Height); // малая ось

            // Соотношение осей (чем ближе к 1, тем круглее)
            double axisRatio = minorAxis / majorAxis;

            // Порог овальности (если меньше 0.96 — крышка дефектная)
            double ovalityThreshold;
            if (double.TryParse(ovalityCoef.Text, out ovalityThreshold))
            {
                // Значение успешно преобразовано в double, теперь его можно использовать
                double ovalityCoef = ovalityThreshold; // Теперь переменная ovalityThreshold будет содержать значение из TextBox
            }
            else
            {
                // Если значение не удалось преобразовать, можно вывести ошибку
                MessageBox.Show("Неверный формат числа в поле Threshold!");
            }
            bool isOval = axisRatio < ovalityThreshold;
            // Визуализация: красный — дефект, зеленый — норма
            Scalar color = isOval ? new Scalar(0, 0, 255) : new Scalar(0, 255, 0);
            Cv2.Ellipse(image, ellipse, color, 2);

            Cv2.PutText(image, $"Ratio: {axisRatio:F2}", new Point(10, 30), HersheyFonts.HersheySimplex, 1, color, 2);
            //Cv2.ImShow("image", image);
            return isOval;
        }

        private Point[] GetCapContour(Mat gray, Mat image)
        {
            // Проверка входных данных
            if (gray.Empty() || image.Empty())
                return null;

            // Параметры обработки
            int window = 13;
            int morph_size = 9;
            int morph_size_2 = 9;

            // Создание структурных элементов
            Mat element = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new Size(2 * morph_size + 1, 2 * morph_size + 1),
                new Point(morph_size, morph_size));

            Mat element2 = Cv2.GetStructuringElement(
                MorphShapes.Cross,
                new Size(2 * morph_size_2 + 1, 2 * morph_size_2 + 1),
                new Point(morph_size_2, morph_size_2));

            // Обработка изображения
            Mat processed = image.Clone();
            Fast_RGB_pseudo_color(processed, 64, HUE_LUT);

            // Разделение каналов
            Mat[] channels;
            Cv2.Split(processed, out channels);

            // Размытие и бинаризация
            Cv2.Blur(channels[0], channels[1], new Size(window, window));
            Cv2.Threshold(channels[1], channels[0], 128, 255, ThresholdTypes.Otsu | ThresholdTypes.BinaryInv);

            // Морфологические операции
            Cv2.MorphologyEx(channels[0], channels[1], MorphTypes.Dilate, element);
            Cv2.MorphologyEx(channels[1], channels[2], MorphTypes.Erode, element2);
            Cv2.BitwiseNot(channels[2], channels[2]);

            // Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(channels[2], out contours, out hierarchy,
                            RetrievalModes.External, ContourApproximationModes.ApproxNone);

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

        /*private Point[] GetCapContour(Mat gray, Mat image)
        {
            // Преобразование в HSV
            Mat imgHSV = new Mat();
            Cv2.CvtColor(image, imgHSV, ColorConversionCodes.BGR2HSV);
            Mat[] hsvChannels = Cv2.Split(imgHSV);

            int window = 31;
            int morphSize = 11;
            int morphSize2 = 11;

            // Структурные элементы для морфологии
            Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(2 * morphSize + 1, 2 * morphSize + 1), new Point(morphSize, morphSize));
            Mat element2 = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(2 * morphSize2 + 1, 2 * morphSize2 + 1), new Point(morphSize2, morphSize2));

            // Гауссов фильтр по каналу H (или можно по S)
            Cv2.GaussianBlur(hsvChannels[0], hsvChannels[1], new Size(window, window), 4);

            // Бинаризация по Отцу
            Cv2.Threshold(hsvChannels[1], hsvChannels[0], 30, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);

            // Морфологические операции: дилатация, эрозия, инверсия
            Cv2.MorphologyEx(hsvChannels[0], hsvChannels[1], MorphTypes.Dilate, element);
            Cv2.MorphologyEx(hsvChannels[1], hsvChannels[2], MorphTypes.Erode, element2);
            Cv2.BitwiseNot(hsvChannels[2], hsvChannels[2]);

            // Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(hsvChannels[2], out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);

            if (contours.Length == 0)
                return Array.Empty<Point>();

            // Лучший контур по длине
            Point[] bestContour = contours.OrderByDescending(c => Cv2.ArcLength(c, true)).FirstOrDefault();

            return bestContour;
        }*/


        /*private Point[] GetCapContour(Mat gray, Mat image)
        {
            Mat processedImage = image.Clone();
            Mat originImage = processedImage.Clone();

            //Cv2.ImShow("originImage", originImage);

            image = ProcessImage(processedImage);
            //Cv2.ImShow("image", image);

            OpenCvSharp.Size size = new OpenCvSharp.Size(5, 5);

            Mat edges = new Mat();
            Cv2.GaussianBlur(image, image, size, 1.5);
            double sigma = 0.33; // Коэффициент регулировки
            double median = (double)Cv2.Mean(image);
            int lower = (int)Math.Max(0, (1.0 - sigma) * median);
            int upper = (int)Math.Min(255, (1.0 + sigma) * median);

            Cv2.Canny(image, edges, lower, upper);

            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, size);
            Cv2.MorphologyEx(edges, edges, MorphTypes.Close, kernel);

            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
            {
                edges.Dispose();

            }

            Point[] bestContour = contours.OrderByDescending(c => Cv2.ArcLength(c, true)).FirstOrDefault();

            Scalar color = new Scalar(0, 255, 0);

            double scaleX = (double)originImage.Width / image.Width;
            double scaleY = (double)originImage.Height / image.Height;

            // Масштабируем контуры
            Point[] scaledContour = bestContour.Select(p => new Point((int)(p.X * scaleX), (int)(p.Y * scaleY))).ToArray();

            edges.Dispose();


            return scaledContour;
        }*/


        /*private Point[] GetCapContour(Mat gray, Mat image)
        {
            OpenCvSharp.Size size = new OpenCvSharp.Size(5, 5);

            double GetContrast(Mat img)
            {
                Scalar mean, stdDev;
                Cv2.MeanStdDev(img, out mean, out stdDev);
                return stdDev.Val0; // Стандартное отклонение как мера контраста
            }

            // Улучшение контраста при необходимости
            if (GetContrast(gray) < 30) // Если контраст низкий, усиливаем
            {
                var clahe = Cv2.CreateCLAHE(clipLimit: 4.0, tileGridSize: new OpenCvSharp.Size(16, 16));
                clahe.Apply(gray, gray);

                Mat sharpened = new Mat();
                Mat sharpenKernel = new Mat(new OpenCvSharp.Size(3, 3), MatType.CV_32F);
                sharpenKernel.SetArray(new float[]
                {
                    -1, -1, -1,
                    -1,  9, -1,
                    -1, -1, -1
                });

                Cv2.Filter2D(gray, sharpened, gray.Depth(), sharpenKernel);
                gray = sharpened;
            }

            // Обработка краев (усиление границ)
            Mat edges = new Mat();
            Cv2.GaussianBlur(gray, gray, size, 1.5);
            Cv2.Canny(gray, edges, 50, 150);

            // Морфологическая обработка для удаления шумов
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, size);
            Cv2.MorphologyEx(edges, edges, MorphTypes.Close, kernel);

            // Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
            {
                edges.Dispose();
                return null; // Контуры не найдены — дефектов нет
            }

            // Выбор самого "круглого" контура
            Point[] bestContour = null;
            double bestCircularity = 0;

            bestContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).FirstOrDefault();
            edges.Dispose();

            return bestContour;
        }*/

        private bool CheckOvality2(Mat gray, Mat image)
        {
            OpenCvSharp.Size size = new OpenCvSharp.Size(5, 5);

            double GetContrast(Mat img)
            {
                Scalar mean, stdDev;
                Cv2.MeanStdDev(img, out mean, out stdDev);
                return stdDev.Val0; // Стандартное отклонение как мера контраста
            }

            // Улучшение контраста при необходимости
            if (GetContrast(gray) < 30) // Если контраст низкий, усиливаем
            {
                // Используем CLAHE для адаптивного выравнивания гистограммы
                var clahe = Cv2.CreateCLAHE(clipLimit: 4.0, tileGridSize: new OpenCvSharp.Size(8, 8));
                clahe.Apply(gray, gray);

                // Усиление резкости
                Mat sharpened = new Mat();
                Mat sharpenKernel = new Mat(new OpenCvSharp.Size(3, 3), MatType.CV_32F);
                sharpenKernel.SetArray(new float[]
                {
                    -1, -1, -1,
                    -1,  9, -1,
                    -1, -1, -1
                });

                Cv2.Filter2D(gray, sharpened, gray.Depth(), sharpenKernel);
                gray = sharpened;
            }

            // Обработка краев (усиление границ)
            Mat edges = new Mat();

            Cv2.GaussianBlur(gray, gray, size, 1.5);
            Cv2.Canny(gray, edges, 50, 150);

            // Морфологическая обработка для удаления шумов
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, size);
            Cv2.MorphologyEx(edges, edges, MorphTypes.Close, kernel);

            // Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
            {
                edges.Dispose();
                return true; // Контуры не найдены — дефектов нет
            }

            // Выбор самого "круглого" контура
            Point[] bestContour = null;
            double bestCircularity = 0;

            Point[] largestContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).FirstOrDefault();
            Cv2.DrawContours(image, new[] { largestContour }, -1, new Scalar(255, 0, 0), 1);
            Cv2.ImShow("Detected Burrs", image);

            // Аппроксимация эллипсом
            RotatedRect ellipse = Cv2.FitEllipse(largestContour);

            Scalar color = new Scalar(0, 255, 0);
            Cv2.Ellipse(image, ellipse, color, 2);

            Cv2.ImShow("Detected Burrs", image);

            edges.Dispose();
            return false;
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



        /*
         private bool CheckBurrs(Mat gray, Mat image)
        {
            // 1. Обнаружение границ с помощью оператора Канни
            Mat edges = new Mat();
            Cv2.Canny(gray, edges, 100, 200);

            // 2. Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
            {
                edges.Dispose();
                return true; // Контуры не найдены — дефектов нет
            }

            // 3. Находим самый большой контур (предположительно крышка)
            Point[] largestContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).FirstOrDefault();

            if (largestContour == null || largestContour.Length < 5)
            {
                edges.Dispose();
                return true; // Контур слишком мал
            }

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

            foreach (var contour in contours)
            {
                foreach (var point in contour)
                {
                    // Расстояние от центра окружности до точки контура
                    double distance = Math.Sqrt(Math.Pow(ellipse.Center.X - point.X, 2) + Math.Pow(ellipse.Center.Y - point.Y, 2));

                    // Если точка находится за пределами окружности (с учётом радиуса)
                    if (distance > averageRadius)
                    {
                        pointsOutside++; // Увеличиваем количество точек вне окружности

                        if (currentGroup.Count > 0)
                        {
                            // Проверяем расстояние до предыдущей точки
                            double lastDistance = Math.Sqrt(Math.Pow(lastPoint.X - point.X, 2) + Math.Pow(lastPoint.Y - point.Y, 2));

                            // Если расстояние между точками меньше 100, добавляем точку в текущую группу
                            if (lastDistance < 100)
                            {
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
            }

            // Добавляем последнюю группу, если она не пустая
            if (currentGroup.Count > 0)
            {
                burrGroups.Add(currentGroup);
            }

            // Пороговое значение для минимального количества точек в группе
            int minPointsInGroup = 5; // Например, группа из менее 5 точек не будет выделена

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
                double ratio = (double)pointsOutside / circumference;

                // Определяем размер облоя (большой или малый)
                Scalar burrColor = ratio > 0.1 ? new Scalar(0, 255, 255) : new Scalar(0, 255, 0); // Желтый для большого облоя, зеленый для малого

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
            }

            // Освобождение ресурсов
            edges.Dispose();

            // Визуализация
            Cv2.ImShow("Detected Burrs", image);
            Cv2.WaitKey(0);

            return false; // Пока только определили круг и облои
        }*/








        /*
         private bool CheckBurrs(Mat gray, Mat image)
        {
            // Бинаризация изображения
            Mat binary = new Mat();
            Cv2.Threshold(gray, binary, 127, 255, ThresholdTypes.BinaryInv);

            // Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
            {
                binary.Dispose();
                return true; // Контуры не найдены — дефектов нет
            }

            // Аппроксимация контура
            

            // Поиск выпуклой оболочки
            Point[] hull = Cv2.ConvexHull(contours[0]);
            double contourLength = Cv2.ArcLength(contours[0], true); // Длина аппроксимированного контура
            double hullLength = Cv2.ArcLength(hull, true); // Длина выпуклой оболочки

            // Визуализация контура и выпуклой оболочки
            Cv2.DrawContours(image, new Point[][] { contours[0] }, -1, new Scalar(0, 255, 0), 2); // Контур крышки — зеленым
            Cv2.DrawContours(image, new Point[][] { hull }, -1, new Scalar(255, 0, 0), 2); // Выпуклая оболочка — синим

            int burrPointsCount = 0;
            foreach (var point in hull)
            {
                double h = Cv2.PointPolygonTest(contours[0], point, false);
                // Проверка, входит ли точка выпуклой оболочки в контур
                if (h < 0) // Точка вне контура (за пределами)
                {
                    burrPointsCount++; // Увеличиваем счётчик
                    Cv2.Circle(image, point, 3, new Scalar(0, 0, 255), -1); // Помечаем точку красным
                }
            }

        // Расчёт разницы между контуром и выпуклой оболочкой
        double hullDiff = Cv2.MatchShapes(contours[0], hull, ShapeMatchModes.I2, 0);

        bool hasBurrs = hullDiff > 0.00001; // Пороговое значение

        binary.Dispose();
            return !hasBurrs; // true — облои не найдены, false — найдены
        }
    */






        /*
         private bool CheckBurrs(Mat gray, Mat image)
        {
            // Бинаризация изображения
            Mat binary = new Mat();
            Cv2.Threshold(gray, binary, 127, 255, ThresholdTypes.BinaryInv);

            // Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
            {
                binary.Dispose();
                return true; // Контуры не найдены — дефектов нет
            }

            // Аппроксимация контура
            double epsilon = 0.01 * Cv2.ArcLength(contours[0], true);
            Point[] approx = Cv2.ApproxPolyDP(contours[0], epsilon, true);

            // Поиск выпуклой оболочки
            Point[] hull = Cv2.ConvexHull(contours[0]);

            // Визуализация контура и выпуклой оболочки
            Cv2.DrawContours(image, new Point[][] { contours[0] }, -1, new Scalar(0, 255, 0), 2); // Контур крышки — зеленым

            // Расчёт разницы между контуром и выпуклой оболочкой
            double hullDiff = Cv2.MatchShapes(contours[0], hull, ShapeMatchModes.I2, 0);

            bool hasBurrs = hullDiff > 0.00001; // Пороговое значение

            

            binary.Dispose();
            return !hasBurrs; // true — облои не найдены, false — найдены
        }*/



        // ✅ Метод для отображения результатов
        private void ShowResult(Mat image)
        {
            string resultImagePath = System.IO.Path.GetTempFileName() + "_result.jpg";
            Cv2.ImWrite(resultImagePath, image);
            recognizePictureBox.Image = new Bitmap(resultImagePath);
        }

        // ✅ Очистка ресурсов
        private void Cleanup(Mat image, Mat gray)
        {
            image.Dispose();
            gray.Dispose();
        }



        //ОБЛОИ
        /*
         private void recognizeButton_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (originPictureBox.Image == null)
            {
                MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                return;
            }

            string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
            if (isStreamCam)
            {
                img1.SaveImage(tempImagePath);
            }
            else
            {
                originalImage.Save(tempImagePath);
            }

            // Загрузка изображения
            Mat image = Cv2.ImRead(tempImagePath, ImreadModes.Color);

            // Преобразование в grayscale
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

            // Размытие для уменьшения шума
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

            // Бинаризация
            Mat binary = new Mat();
            Cv2.Threshold(gray, binary, 127, 255, ThresholdTypes.BinaryInv);

            // Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // Проверка, найдены ли контуры
            if (contours.Length == 0)
            {
                MessageBox.Show("Контуры не найдены.");
                return;
            }

            // Аппроксимация контура
            double epsilon = 0.01 * Cv2.ArcLength(contours[0], true);
            Point[] approx = Cv2.ApproxPolyDP(contours[0], epsilon, true);

            // Поиск выпуклой оболочки
            Point[] hull = Cv2.ConvexHull(contours[0]);

            // Визуализация
            Mat result = new Mat();
            Cv2.CvtColor(gray, result, ColorConversionCodes.GRAY2BGR);
            Cv2.DrawContours(result, new Point[][] { contours[0] }, -1, new Scalar(0, 255, 0), 2); // Контур крышки

            // Обработка и пометка облоев (красным)
            for (int i = 0; i < contours[0].Length; i++)
            {
                Point contourPoint = contours[0][i];

                // Проверка, если точка снаружи выпуклой оболочки
                bool isOutside = Cv2.PointPolygonTest(hull, contourPoint, false) < 0;

                if (isOutside)
                {
                    // Рисуем точку красным, если она за пределами выпуклой оболочки (облой)
                    Cv2.Circle(result, contourPoint, 3, new Scalar(0, 0, 255), -1); // Круглый маркер красного цвета
                }
            }

            stopwatch.Stop();
            // Сохранение и отображение результата
            string resultImagePath = System.IO.Path.GetTempFileName() + "_result.jpg";
            Cv2.ImWrite(resultImagePath, result);
            recognizePictureBox.Image = new Bitmap(resultImagePath);
            

            // Анализ отклонений
            double hullDiff = Cv2.MatchShapes(contours[0], hull, ShapeMatchModes.I2, 0);
            if (hullDiff > 0.00001) // Пороговое значение
            {
                MessageBox.Show("Обнаружены облои!");
            }
            else
            {
                MessageBox.Show("Облои не обнаружены.");
            }

            // Освобождение ресурсов
            image.Dispose();
            gray.Dispose();
            binary.Dispose();
            result.Dispose();

            MessageBox.Show($"Время выполнения: {stopwatch.ElapsedMilliseconds} миллисекунд");
        }
         */
        //ОВАЛЬНОСТЬ
        /*
         private void recognizeButton_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch(); // Инициализация таймера
            stopwatch.Start(); // Запуск таймера

            if (originPictureBox.Image == null)
            {
                MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                return;
            }

            string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
            if (isStreamCam)
            {
                img1.SaveImage(tempImagePath);
            }
            else
            {
                originalImage.Save(tempImagePath);
            }
            Mat image = Cv2.ImRead(tempImagePath, ImreadModes.Color);

            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

            // Поиск контуров
            Mat cannyOutput = new Mat();
            Cv2.Canny(gray, cannyOutput, 100, 200);
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(cannyOutput, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            // Поиск эллипсов среди контуров
            foreach (var contour in contours)
            {
                // Фильтрация слишком маленьких контуров
                if (contour.Length < 5)
                    continue; // Для нахождения эллипса необходимо минимум 5 точек в контуре

                // Находим эллипс для текущего контура
                RotatedRect minEllipse = Cv2.FitEllipse(contour);

                // Получаем параметры эллипса
                Point2f center = minEllipse.Center;
                Size2f size = minEllipse.Size;
                float angle = minEllipse.Angle;

                // Вычисляем соотношение осей (ширина / высота)
                float aspectRatio = Math.Min(size.Width, size.Height) / Math.Max(size.Width, size.Height);

                // Если соотношение близко к 1, то это круг (выделяем зеленым)
                if (aspectRatio > 0.85)
                {
                    Cv2.Ellipse(image, (Point)center, new OpenCvSharp.Size((int)size.Width / 2, (int)size.Height / 2), angle, 0, 360, Scalar.Green, 2);
                }
                // Если соотношение значительно меньше 1, то это эллипс (выделяем красным)
                else
                {
                    Cv2.Ellipse(image, (Point)center, new OpenCvSharp.Size((int)size.Width / 2, (int)size.Height / 2), angle, 0, 360, Scalar.Red, 2);
                    ellipseReject++; // Увеличиваем значение ellipseReject
                }
            }

            // Обновляем текстбокс с значением ellipseReject
            ellipseRejectTx.Text = ellipseReject.ToString();

            // Сохранение и отображение результата
            string resultImagePath = System.IO.Path.GetTempFileName() + "_result.jpg";
            Cv2.ImWrite(resultImagePath, image);
            recognizePictureBox.Image = new Bitmap(resultImagePath);

            image.Dispose();
            gray.Dispose();
            cannyOutput.Dispose();

            stopwatch.Stop(); // Остановка таймера
            MessageBox.Show($"Время выполнения: {stopwatch.ElapsedMilliseconds} миллисекунд");
        }*/





        private void button1_Click(object sender, EventArgs e)
        {
            if (originPictureBox.Image == null)
            {
                MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                return;
            }

            string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
            if (isStreamCam)
            {
                img1.SaveImage(tempImagePath);
            }
            else
            {
                originalImage.Save(tempImagePath);
            }
            Mat image = Cv2.ImRead(tempImagePath, ImreadModes.Color);

            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

            /*int leftColumn = int.Parse(leftCol.Text);
            int rightColumn = int.Parse(rightCol.Text);*/

            // Создаём список данных для разных столбцов
            var columnsData = new List<(int column, List<int> yCoords, List<int> brightness)>
            {
                /*(leftColumn, new List<int>(), new List<int>()),
                (rightColumn, new List<int>(), new List<int>())*/
            };

            foreach (var (column, yCoordinates, brightnessValues) in columnsData)
            {
                for (int y = 0; y < gray.Rows; y++)
                {
                    byte currentBrightness = gray.At<byte>(y, column);
                    yCoordinates.Add(y);
                    brightnessValues.Add(currentBrightness);
                }
            }

            image.Dispose();
            gray.Dispose();

            BuildBrightnessChart(columnsData);
        }


        private void BuildBrightnessChart(List<(int column, List<int> yCoordinates, List<int> brightnessValues)> columnsData)
        {
            /* chart1.Series.Clear();
             chart1.ChartAreas.Clear();
             chart2.Series.Clear();
             chart2.ChartAreas.Clear();
             dataGridView1.Rows.Clear();
             dataGridView2.Rows.Clear();

             Chart[] charts = { chart1, chart2 }; // Два графика
             DataGridView[] dataGrids = { dataGridView1, dataGridView2 }; // Две таблицы

             for (int i = 0; i < columnsData.Count; i++)
             {
                 var (column, yCoordinates, brightnessValues) = columnsData[i];
                 Chart currentChart = charts[i];
                 DataGridView currentDataGrid = dataGrids[i]; // Выбираем соответствующую таблицу

                 // Создаём новую область
                 ChartArea chartArea = new ChartArea($"ChartArea_{i}")
                 {
                     AxisX = { Title = "Y Coordinate" },
                     AxisY = { Title = "Brightness" },
                     BackColor = Color.Gray
                 };

                 currentChart.ChartAreas.Add(chartArea);

                 var series = new Series($"Column {column}")
                 {
                     ChartType = SeriesChartType.Line,
                     Color = i == 0 ? Color.Blue : Color.Red,
                     ChartArea = $"ChartArea_{i}"
                 };

                 for (int j = 0; j < yCoordinates.Count; j++)
                 {
                     series.Points.AddXY(yCoordinates[j], brightnessValues[j]);
                 }

                 currentChart.Series.Add(series);
                 currentChart.Legends.Clear();

                 // Анализируем горизонтальные участки яркости и заполняем соответствующую таблицу
                 AnalyzeHorizontalSections(yCoordinates, brightnessValues, currentChart, currentDataGrid);
             }*/
        }

        private void AnalyzeHorizontalSections(List<int> yCoordinates, List<int> brightnessValues, Chart currentChart, DataGridView currentDataGrid)
        {
            int brightnessThreshold = 5;
            int startY = 0, endY = 0;
            int currentBrightness = brightnessValues[0];

            var horizontalSections = new List<(int start, int end, int brightness)>();

            for (int i = 1; i < brightnessValues.Count; i++)
            {
                if (Math.Abs(brightnessValues[i] - currentBrightness) <= brightnessThreshold)
                {
                    endY = yCoordinates[i];
                }
                else
                {
                    if (endY - startY > 0)
                    {
                        horizontalSections.Add((startY, endY, currentBrightness));
                    }
                    startY = yCoordinates[i];
                    endY = yCoordinates[i];
                    currentBrightness = brightnessValues[i];
                }
            }

            if (endY - startY > 0)
            {
                horizontalSections.Add((startY, endY, currentBrightness));
            }

            DisplayInDataGridView(horizontalSections, currentDataGrid);
            DrawSectionsOnChart(horizontalSections, currentChart);
        }

        private void DrawSectionsOnChart(List<(int start, int end, int brightness)> horizontalSections, Chart currentChart)
        {
            List<Color> distinctColors = GenerateDistinctColors(horizontalSections.Count);
            int previousEndY = -1;
            int previousBrightness = -1;

            for (int i = 0; i < horizontalSections.Count; i++)
            {
                var section = horizontalSections[i];
                var color = distinctColors[i];

                var series = new Series
                {
                    ChartType = SeriesChartType.Line,
                    Color = color,
                    BorderWidth = 3,
                    ChartArea = currentChart.ChartAreas[0].Name
                };

                if (previousEndY != -1)
                {
                    series.Points.AddXY(previousEndY, previousBrightness);
                    series.Points.AddXY(section.start, section.brightness);
                }

                for (int y = section.start; y <= section.end; y++)
                {
                    series.Points.AddXY(y, section.brightness);
                }

                currentChart.Series.Add(series);

                previousEndY = section.end;
                previousBrightness = section.brightness;
            }
        }


        // Генерация списка различных цветов (HSV -> RGB)
        private List<Color> GenerateDistinctColors(int count)
        {
            List<Color> colors = new List<Color>();

            for (int i = 0; i < count; i++)
            {
                float hue = (360f / count) * i; // Распределяем цвета равномерно
                colors.Add(ColorFromHSV(hue, 1.0, 1.0)); // Полная насыщенность и яркость
            }

            return colors;
        }

        // Конвертация HSV -> RGB для ярких цветов
        private Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = (int)value;
            int p = (int)(value * (1 - saturation));
            int q = (int)(value * (1 - f * saturation));
            int t = (int)(value * (1 - (1 - f) * saturation));

            return hi switch
            {
                0 => Color.FromArgb(255, v, t, p),
                1 => Color.FromArgb(255, q, v, p),
                2 => Color.FromArgb(255, p, v, t),
                3 => Color.FromArgb(255, p, q, v),
                4 => Color.FromArgb(255, t, p, v),
                _ => Color.FromArgb(255, v, p, q),
            };
        }




        private void DisplayInDataGridView(List<(int start, int end, int brightness)> horizontalSections, DataGridView dataGrid)
        {
            dataGrid.Rows.Clear();
            dataGrid.Columns.Clear();

            // Добавляем скрытые столбцы для startY и endY
            dataGrid.Columns.Add("StartY", "StartY");
            dataGrid.Columns["StartY"].Visible = false; // Скрываем столбец StartY

            dataGrid.Columns.Add("EndY", "EndY");
            dataGrid.Columns["EndY"].Visible = false; // Скрываем столбец EndY

            dataGrid.Columns.Add("Length", "Length");
            dataGrid.Columns.Add("Brightness", "Brightness");

            foreach (var section in horizontalSections)
            {
                // Добавляем значения в скрытые столбцы
                dataGrid.Rows.Add(section.start, section.end, section.end - section.start, section.brightness);
            }
        }



        private void Chart1_MouseMove(object sender, MouseEventArgs e)
        {
            Chart chart = sender as Chart;
            if (chart == null) return;

            HitTestResult result = chart.HitTest(e.X, e.Y);

            if (result.ChartElementType == ChartElementType.DataPoint)
            {
                try
                {
                    double yCoord = chart.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                    double brightness = chart.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);

                    tooltip.Show($"Y: {yCoord:F1}, Brightness: {brightness:F1}", chart, e.X + 10, e.Y - 20);
                }
                catch { }
            }
            else
            {
                tooltip.Hide(chart);
            }
        }

        private void Chart1_MouseClick(object sender, MouseEventArgs e)
        {
            /*HitTestResult result = chart1.HitTest(e.X, e.Y);

            // Проверяем, был ли клик на DataPoint (точке данных) и есть ли связанная серия
            if (result.ChartElementType == ChartElementType.DataPoint && result.Series != null)
            {
                // Получаем координату X в данных (не пиксели, а значение)
                double clickedY = chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);

                // Проходим по всем строкам в DataGridView
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    // Извлекаем значения StartY и EndY для текущей строки таблицы
                    if (row.Cells["StartY"].Value != DBNull.Value && row.Cells["EndY"].Value != DBNull.Value)
                    {
                        int startY = Convert.ToInt32(row.Cells["StartY"].Value);
                        int endY = Convert.ToInt32(row.Cells["EndY"].Value);

                        // Проверяем, находится ли значение clickedY в пределах этих значений
                        if (clickedY >= startY && clickedY <= endY)
                        {
                            // Очищаем предыдущие выделения и выделяем текущую строку
                            dataGridView1.ClearSelection();
                            row.Selected = true;

                            // Прокручиваем таблицу так, чтобы выбранная строка была видна
                            dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;
                            break;
                        }
                    }
                }
            }*/
        }

        private void Chart2_MouseClick(object sender, MouseEventArgs e)
        {
            /*HitTestResult result = chart2.HitTest(e.X, e.Y);

            // Проверяем, был ли клик на DataPoint (точке данных) и есть ли связанная серия
            if (result.ChartElementType == ChartElementType.DataPoint && result.Series != null)
            {
                // Получаем координату X в данных (не пиксели, а значение)
                double clickedY = chart2.ChartAreas[0].AxisX.PixelPositionToValue(e.X);

                // Проходим по всем строкам в DataGridView2
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    // Извлекаем значения StartY и EndY для текущей строки таблицы
                    if (row.Cells["StartY"].Value != DBNull.Value && row.Cells["EndY"].Value != DBNull.Value)
                    {
                        int startY = Convert.ToInt32(row.Cells["StartY"].Value);
                        int endY = Convert.ToInt32(row.Cells["EndY"].Value);

                        // Проверяем, находится ли значение clickedY в пределах этих значений
                        if (clickedY >= startY && clickedY <= endY)
                        {
                            // Очищаем предыдущие выделения и выделяем текущую строку
                            dataGridView2.ClearSelection();
                            row.Selected = true;

                            // Прокручиваем таблицу так, чтобы выбранная строка была видна
                            dataGridView2.FirstDisplayedScrollingRowIndex = row.Index;
                            break;
                        }
                    }
                }
            }*/
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (originPictureBox.Image == null)
            {
                MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                return;
            }

            if (isStreamCam == true)
            {
                // Инициализация таймера для периодической обработки кадров
                if (frameProcessingTimer == null)
                {
                    frameProcessingTimer = new System.Windows.Forms.Timer();
                    frameProcessingTimer.Interval = 1; // Интервал 100 мс (10 кадров в секунду)
                    frameProcessingTimer.Tick += FrameProcessingTimer_Tick;
                }
                frameProcessingTimer.Start(); // Запуск таймера для обработки кадров
            }
            else
            {
                // Однократная обработка изображения
                ProcessImage();
            }
        }

        private void FrameProcessingTimer_Tick(object sender, EventArgs e)
        {
            if (originPictureBox.Image == null)
            {
                frameProcessingTimer.Stop(); // Останавливаем таймер, если изображение отсутствует
                return;
            }

            // Если isStreamCam == true, то обрабатываем каждый 60-й кадр
            frameCounter++;
            if (frameCounter % 1 != 0) return; // Пропускаем кадры, если они не делятся на 60

            ProcessImage();
        }

        private int DeterminePillBrightness(Mat gray, int[] columns)
        {
            int maxBrightness = 0;
            foreach (int column in columns)
            {
                for (int y = 0; y < gray.Rows; y++)
                {
                    byte currentBrightness = gray.At<byte>(y, column);
                    if (currentBrightness > maxBrightness)
                        maxBrightness = currentBrightness;
                }
            }
            return maxBrightness;
        }

        private int DeterminePillWidth(Mat gray, int[] columns, int brightness)
        {
            int maxWidth = 0;
            foreach (int column in columns)
            {
                int currentWidth = 0;
                for (int y = 0; y < gray.Rows; y++)
                {
                    byte currentBrightness = gray.At<byte>(y, column);
                    if (Math.Abs(currentBrightness - brightness) < 15)
                        currentWidth++;
                    else
                    {
                        if (currentWidth > maxWidth)
                            maxWidth = currentWidth;
                        currentWidth = 0;
                    }
                }
                if (currentWidth > maxWidth)
                    maxWidth = currentWidth;
            }
            return maxWidth;
        }

        private void ProcessImage()
        {
            //int maxEmptyDistance = int.Parse(maxEmptyDistanceTextBox.Text);
            string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
            if (isStreamCam && img1 != null) img1.SaveImage(tempImagePath);
            else originalImage.Save(tempImagePath);

            Mat image = Cv2.ImRead(tempImagePath, ImreadModes.Color);
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

            int leftColumn /*= int.Parse(leftCol.Text)*/ = 0;
            int rightColumn /*= int.Parse(rightCol.Text)*/ = 0;
            int[] columns = { leftColumn, rightColumn };

            int pillBrightness = 0; // Яркость таблетки
            float minPillWidth = 0; // Минимальная ширина таблетки
            /*if (*//*autoDetermCb.Checked == true*//*)
            {
                pillBrightness = DeterminePillBrightness(gray, columns);
                minPillWidth = DeterminePillWidth(gray, columns, pillBrightness);

                //textBoxPill.Text = pillBrightness.ToString();
                //textBoxPillWidth.Text = minPillWidth.ToString();
            }
            else if (*//*autoDetermCb.Checked == false*//*)
            {
                //pillBrightness = int.Parse(textBoxPill.Text); // Яркость таблетки
                //minPillWidth = int.Parse(textBoxPillWidth.Text);
            }*/

            bool pillDetected = false, missingPillDetected = false;
            int emptyCellsCount = 0, pillCount = 0;
            Mat highlightedImage = image.Clone();
            List<int> pillDiameters = new List<int>();

            foreach (int column in columns)
            {
                int consecutivePillPixels = 0, emptyPixelsStreak = 0;
                bool isInsideEmptyCell = false, isInsidePill = false;

                for (int y = 0; y < gray.Rows; y++)
                {
                    byte currentBrightness = gray.At<byte>(y, column);

                    if (Math.Abs(currentBrightness - pillBrightness) < 15)
                    {
                        consecutivePillPixels++;
                        emptyPixelsStreak = 0;
                        Cv2.Circle(highlightedImage, new OpenCvSharp.Point(column, y), 1, Scalar.Green, -1);

                        if (!isInsidePill && consecutivePillPixels >= minPillWidth / 2)
                        {
                            pillCount++;
                            pillDetected = true;
                            isInsidePill = true;
                            pillDiameters.Add(consecutivePillPixels);
                        }
                        isInsideEmptyCell = false;
                    }
                    else
                    {
                        consecutivePillPixels = 0;
                        emptyPixelsStreak++;
                        if (/*emptyPixelsStreak >= maxEmptyDistance && */!isInsideEmptyCell)
                        {
                            emptyCellsCount++;
                            missingPillDetected = true;
                            isInsideEmptyCell = true;

                            if (pillDiameters.Count > 0)
                            {
                                int avgDiameter = (int)pillDiameters.Average();
                                Cv2.Circle(highlightedImage, new OpenCvSharp.Point(column, y + (int)minPillWidth / 2), (int)minPillWidth / 2, Scalar.Blue, 2);
                            }
                        }
                        Cv2.Circle(highlightedImage, new OpenCvSharp.Point(column, y), 1, Scalar.Red, -1);
                        isInsidePill = false;
                    }
                }
            }

            //tabletkiTextBox.Text = pillCount.ToString();
            //ellipseRejectTx.Text = emptyCellsCount.ToString();

            Bitmap outputBitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(highlightedImage);
            recognizePictureBox.Image = outputBitmap;
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

        private void DrawLines()
        {
            // Проверяем наличие изображения
            if (originPictureBox.Image == null && img1.Empty())
            {
                MessageBox.Show("Изображение не загружено.");
                return;
            }

            // Создаем копию изображения в зависимости от источника
            Bitmap bitmap = null;
            Mat img = null;


            // Если изображение от камеры (если оно доступно)
            if (!img1.Empty() && originalImage == null)
            {
                img = img1.Clone(); // Используем изображение от камеры
            }
            else
            {
                bitmap = new Bitmap(originalImage); // Используем изображение из PictureBox
                img = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
            }

            Mat imgCopy = img.Clone();

            // Читаем значения столбцов
            /*if (int.TryParse(leftCol.Text, out int leftX) && leftX >= 0 && leftX < img.Cols)
            {
                Cv2.Line(imgCopy, new OpenCvSharp.Point(leftX, 0), new OpenCvSharp.Point(leftX, img.Rows - 1), new Scalar(0, 0, 255), 2);
            }

            if (int.TryParse(rightCol.Text, out int rightX) && rightX >= 0 && rightX < img.Cols)
            {
                Cv2.Line(imgCopy, new OpenCvSharp.Point(rightX, 0), new OpenCvSharp.Point(rightX, img.Rows - 1), new Scalar(0, 0, 255), 2);
            }*/

            // Обновляем изображение в PictureBox
            originPictureBox.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imgCopy);

            // Освобождаем ресурсы
            img.Dispose();
            imgCopy.Dispose();
        }


        public void GetImage(Mat img)
        {
            // Если ROI не выбран, показываем полное изображение с камеры
            if (!LocalSettings.Instance.UseVConcat)
            {
                img1 = img.Clone();
                if (isRoiProduce == true && isROISelected == true)
                {
                    img1 = new Mat(img, roi);
                }
                originPictureBox.Image = MatToBitmap(img1);
                recognizePictureBox.Image = MatToBitmap(img1);
                pictureBox1.Image = MatToBitmap(img1);
                underfillPictureBox.Image = MatToBitmap(img1);

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
                    originPictureBox.Image = MatToBitmap(img1);
                    recognizePictureBox.Image = MatToBitmap(img1);
                    pictureBox1.Image = MatToBitmap(img1);
                    underfillPictureBox.Image = MatToBitmap(img1);
                    isFirstImageCam1 = true;

                }
                else
                {
                    Cv2.VConcat(img1, img.Clone(), img1);
                    if (isRoiProduce == true && isROISelected == true)
                    {
                        img1 = new Mat(img, roi);
                    }
                    originPictureBox.Image = MatToBitmap(img1);
                    recognizePictureBox.Image = MatToBitmap(img1);
                    pictureBox1.Image = MatToBitmap(img1);
                    underfillPictureBox.Image = MatToBitmap(img1);

                }
            }


            // После обновления изображения, рисуем линии, если нужно
            /*if (drawLinesCb.Checked)
            {
                DrawLines();
            }*/
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
            cam.TriggerMode = false;
            cam.SetTriggerMode();
            cam.SetExposureTime();

            ovalityCoef.Enabled = false;
            circleCoefTx.Enabled = false;
            minSquareInclusion.Enabled = false;
            maxSquareInclusion.Enabled = false;
            minSquareInpaint.Enabled = false;
            whiteThresoldTx.Enabled = false;

            StartStop(true);
        }

        private void endStream_Click(object sender, EventArgs e)
        {
            isRoiProduce = false;
            isROISelected = false;
            isStreamCam = false;
            originPictureBox.Image = null;

            ovalityCoef.Enabled = true;
            circleCoefTx.Enabled = true;
            minSquareInclusion.Enabled = true;
            maxSquareInclusion.Enabled = true;
            minSquareInpaint.Enabled = true;
            whiteThresoldTx.Enabled = true;

            StartStop(false);
        }

        private void saveImageButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверяем, какое изображение нужно сохранить
                Bitmap imageToSave = originalImage ?? img1.ToBitmap(); // Если originalImage пустое, используем img1

                if (imageToSave == null)
                {
                    MessageBox.Show("Нет изображения для сохранения.");
                    return;
                }

                // Папка для сохранения изображений
                string saveFolder = Path.Combine(Application.StartupPath, "SaveImages");

                // Проверяем, существует ли папка, если нет, создаем
                if (!Directory.Exists(saveFolder))
                {
                    Directory.CreateDirectory(saveFolder);
                }

                // Генерируем имя файла с текущей датой и временем, чтобы избежать перезаписи
                string fileName = $"image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string filePath = Path.Combine(saveFolder, fileName);

                // Сохраняем изображение
                imageToSave.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                // Выводим сообщение об успешном сохранении
                MessageBox.Show($"Изображение сохранено в: {filePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изображения: {ex.Message}");
            }
        }


        private void buttonResetImage_Click(object sender, EventArgs e)
        {
            if (originalImage != null)
            {
                originPictureBox.Image = (Bitmap)originalImage.Clone();
            }
        }

        private void drawLinesCb_CheckedChanged(object sender, EventArgs e)
        {
            /*if (drawLinesCb.Checked == true)
            {
                if (originalImage != null)
                {
                    DrawLines();
                }
            }
            if (drawLinesCb.Checked == false)
            {
                if (originalImage != null)
                {
                    originPictureBox.Image = (Bitmap)originalImage.Clone();
                }
            }*/

        }

        private void leftCol_TextChanged(object sender, EventArgs e)
        {
            /*if (drawLinesCb.Checked == true)
            {
                if (originalImage != null)
                {
                    DrawLines();
                }
            }
            if (drawLinesCb.Checked == false)
            {
                if (originalImage != null)
                {
                    originPictureBox.Image = (Bitmap)originalImage.Clone();
                }
            }*/
        }

        private void rightCol_TextChanged(object sender, EventArgs e)
        {
            /*if (drawLinesCb.Checked == true)
            {
                if (originalImage != null)
                {
                    DrawLines();
                }
            }
            if (drawLinesCb.Checked == false)
            {
                if (originalImage != null)
                {
                    originPictureBox.Image = (Bitmap)originalImage.Clone();
                }
            }*/
        }



        private void drawRoi_Click(object sender, EventArgs e)
        {
            isDrawing = true;
            isRoiProduce = true;
            isConfirmVisible = false; // Скрываем кнопки
            originPictureBox.MouseDown += OriginPictureBox_MouseDown;
            originPictureBox.MouseMove += OriginPictureBox_MouseMove;
            originPictureBox.MouseUp += OriginPictureBox_MouseUp;
            originPictureBox.Paint += OriginPictureBox_Paint;
            originPictureBox.MouseClick += OriginPictureBox_MouseClick;
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
                originPictureBox.Invalidate(); // Перерисовываем PictureBox
            }
        }

        private void OriginPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDrawing && e.Button == MouseButtons.Left)
            {
                isDrawing = false;
                originPictureBox.MouseDown -= OriginPictureBox_MouseDown;
                originPictureBox.MouseMove -= OriginPictureBox_MouseMove;
                originPictureBox.MouseUp -= OriginPictureBox_MouseUp;

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

                    originPictureBox.Invalidate(); // Перерисовываем PictureBox
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
                int pbWidth = originPictureBox.Width;
                int pbHeight = originPictureBox.Height;

                float scaleX, scaleY;
                int offsetX = 0, offsetY = 0;

                if (originPictureBox.SizeMode == PictureBoxSizeMode.Zoom)
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
                originPictureBox.Image = MatToBitmap(img1);
                originPictureBox.Invalidate();
            }
        }

        // Метод для сброса ROI
        private void ResetROI()
        {
            selectedROI = Rectangle.Empty;
            isConfirmVisible = false; // Скрываем кнопки
            originPictureBox.Invalidate();
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
            if (!cameraError1)
            {
                if (cam.Streamed)
                    cam.EndStream();

                cam.Close();
            }

            Application.Exit();
        }

        private void redrawRoi_Click(object sender, EventArgs e)
        {
            isROISelected = false; // Сбрасываем флаг
            croppedImage = null; // Очищаем обрезанное изображение
        }

        private void obduvBatton_Click(object sender, EventArgs e)
        {

        }

        









        /*private void DrawRedLines()
        {
            if (originPictureBox.Image == null)
            {
                MessageBox.Show("Изображение не загружено.");
                return;
            }

            // Конвертация Image в Mat
            Bitmap bitmap = new Bitmap(originPictureBox.Image);
            Mat img = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
            Mat imgCopy = img.Clone();

            // Парсим значения из текстбоксов
            if (int.TryParse(leftCol.Text, out int leftX) && leftX >= 0 && leftX < img.Cols)
            {
                Cv2.Line(imgCopy, new OpenCvSharp.Point(leftX, 0), new OpenCvSharp.Point(leftX, img.Rows - 1), new Scalar(0, 0, 255), 2);
            }

            if (int.TryParse(rightCol.Text, out int rightX) && rightX >= 0 && rightX < img.Cols)
            {
                Cv2.Line(imgCopy, new OpenCvSharp.Point(rightX, 0), new OpenCvSharp.Point(rightX, img.Rows - 1), new Scalar(0, 0, 255), 2);
            }

            // Обновляем originPictureBox
            originPictureBox.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imgCopy);

            img.Dispose();
            imgCopy.Dispose();
        }

        // Вызываем отрисовку линий при изменении текстбоксов
        private void leftCol_TextChanged(object sender, EventArgs e)
        {
            DrawRedLines();
        }

        private void rightCol_TextChanged(object sender, EventArgs e)
        {
            DrawRedLines();
        }
        
         /// <summary>
        /// Определяет среднюю яркость таблеток без фиксированного порога
        /// </summary>
        private int DetectPillBrightness(Mat gray, int[] columns)
        {
            Dictionary<int, int> brightnessHistogram = new Dictionary<int, int>();

            foreach (int column in columns)
            {
                int consecutivePixels = 0;
                int lastBrightness = -1;

                for (int y = 0; y < gray.Rows; y++)
                {
                    byte currentBrightness = gray.At<byte>(y, column);

                    if (lastBrightness == -1 || Math.Abs(currentBrightness - lastBrightness) < 15)
                    {
                        consecutivePixels++;
                    }
                    else
                    {
                        if (consecutivePixels > 5) // Считаем только крупные сегменты
                        {
                            if (!brightnessHistogram.ContainsKey(lastBrightness))
                                brightnessHistogram[lastBrightness] = 0;
                            brightnessHistogram[lastBrightness] += consecutivePixels;
                        }
                        consecutivePixels = 1;
                    }

                    lastBrightness = currentBrightness;
                }
            }

            return brightnessHistogram.Count > 0
                ? brightnessHistogram.OrderByDescending(kv => kv.Value).First().Key
                : 150;
        }

        /// <summary>
        /// Определяет минимальную ширину таблетки
        /// </summary>
        private int DetectMinPillWidth(Mat gray, int[] columns, int pillBrightness)
        {
            List<int> pillWidths = new List<int>();

            foreach (int column in columns)
            {
                int consecutivePillPixels = 0;
                bool isInsidePill = false;

                for (int y = 0; y < gray.Rows; y++)
                {
                    byte currentBrightness = gray.At<byte>(y, column);

                    if (Math.Abs(currentBrightness - pillBrightness) < 10) // Используем динамически найденный порог
                    {
                        consecutivePillPixels++;
                        if (!isInsidePill) isInsidePill = true;
                    }
                    else
                    {
                        if (isInsidePill)
                        {
                            pillWidths.Add(consecutivePillPixels);
                            consecutivePillPixels = 0;
                            isInsidePill = false;
                        }
                    }
                }
            }

            return pillWidths.Count > 0 ? (int)pillWidths.Average() : 10;
        }

        /// <summary>
        /// Определяет максимальное расстояние пустых ячеек
        /// </summary>
        private int DetectMaxEmptyDistance(Mat gray, int[] columns, int pillBrightness)
        {
            List<int> emptyWidths = new List<int>();

            foreach (int column in columns)
            {
                int emptyPixelsStreak = 0;
                bool isInsideEmptyCell = false;

                for (int y = 0; y < gray.Rows; y++)
                {
                    byte currentBrightness = gray.At<byte>(y, column);

                    if (Math.Abs(currentBrightness - pillBrightness) < 10)
                    {
                        if (isInsideEmptyCell)
                        {
                            emptyWidths.Add(emptyPixelsStreak);
                            emptyPixelsStreak = 0;
                            isInsideEmptyCell = false;
                        }
                    }
                    else
                    {
                        emptyPixelsStreak++;
                        if (!isInsideEmptyCell) isInsideEmptyCell = true;
                    }
                }
            }

            return emptyWidths.Count > 0 ? (int)emptyWidths.Average() : 5;



        private void OriginPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDrawing && e.Button == MouseButtons.Left)
            {
                isDrawing = false;

                // Отписываем события после выделения ROI
                originPictureBox.MouseDown -= OriginPictureBox_MouseDown;
                originPictureBox.MouseMove -= OriginPictureBox_MouseMove;
                originPictureBox.MouseUp -= OriginPictureBox_MouseUp;

                if (selectedROI.Width > 0 && selectedROI.Height > 0)
                {
                    int pictureBoxWidth = originPictureBox.Width;
                    int pictureBoxHeight = originPictureBox.Height;

                    // Рассчитываем масштаб изображения относительно PictureBox
                    float scaleX = (float)img1.Width / pictureBoxWidth;
                    float scaleY = (float)img1.Height / pictureBoxHeight;

                    // Преобразуем координаты выделенной области ROI
                    uint roiX = (uint)(selectedROI.X * scaleX);
                    uint roiY = (uint)(selectedROI.Y * scaleY);
                    uint roiWidth = (uint)(selectedROI.Width * scaleX);
                    uint roiHeight = (uint)(selectedROI.Height * scaleY);

                    // Рассчитываем центр ROI
                    uint roiCenterX = roiX - roiWidth / 2;
                    uint roiCenterY = roiY - roiHeight / 2;

                    endStream_Click(null, null);
                    // Устанавливаем параметры ROI в камеру
                    cam.AOIWidth = roiWidth;
                    cam.AOIHeight = roiHeight;
                    cam.AOIOffsetX = roiCenterX;
                    cam.AOIOffsetY = roiCenterY;

                    // Применяем ROI через методы камеры
                    bool setAIO = cam.SetAOIWidth();
                    bool setheightAIO = cam.SetAOIHeight();
                    bool setoffsetX = cam.SetAOIOffsetX();
                    bool setoffsetY = cam.SetAOIOffsetY();
                    getImageButton_Click_1(null, null);

                    isROISelected = true; // Флаг, что ROI выбран
                }
            }
        }
        }*/


    }
}
