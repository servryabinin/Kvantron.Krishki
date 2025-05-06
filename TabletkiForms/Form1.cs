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

namespace TabletkiForms
{
    public partial class Form1 : Form
    {
        private ToolTip tooltip = new ToolTip();

        private Label labelCoordinates = new Label();

        HikCamera cam = new HikCamera(LocalSettings.Instance.Cam1SN);

        bool currentDetectorState = false;
        DioModule module = null;
        bool isFirstImageCam1 = false;
        Mat img1 = new Mat(); //camera image
        bool cameraError1 = false; //error message output

        int leftColumn = 90;
        int rightColumn = 210;

        public Form1()
        {
            InitializeComponent();
            chart1.MouseMove += Chart1_MouseMove; // Добавляем обработчик событий
            chart1.MouseClick += Chart1_MouseClick;
            chart2.MouseClick += Chart2_MouseClick;
            originPictureBox.MouseMove += originPictureBox_MouseMove;


            Form1_Load();

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

        private void loadImageButton_Click(object sender, EventArgs e)
        {
            // Создаем экземпляр OpenFileDialog
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Устанавливаем фильтр для файлов изображений
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                // Проверяем, выбрал ли пользователь файл и нажал "ОК"
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
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

        private void Form1_Load()
        {
            // Очищаем все существующие столбцы и строки в первой таблице
            dataGridView1.Columns.Clear();
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
            }

            labelCoordinates.Location = new System.Drawing.Point(10, 10);
            labelCoordinates.AutoSize = true;
            Controls.Add(labelCoordinates);
        }



        private void originPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            // Выводим координаты в label
            labelCoordinates.Text = $"X: {e.X}, Y: {e.Y}";
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




        /*private void recognizeButton_Click(object sender, EventArgs e)
        {
            // Проверяем, загружено ли изображение в originPictureBox
            if (originPictureBox.Image == null)
            {
                MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                return;
            }

            // Сохраняем изображение из PictureBox во временный файл
            string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
            originPictureBox.Image.Save(tempImagePath);

            // Загружаем изображение в OpenCV
            Mat image = Cv2.ImRead(tempImagePath, ImreadModes.Color);

            // Преобразуем в оттенки серого
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

            // Применяем размытие для уменьшения шума
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

            // Находим центральный столбец
            int middleColumn = gray.Cols / 2;

            // Порог изменения яркости для обнаружения границ
            int brightnessChangeThreshold = 10;

            // Проходим по центральному столбцу сверху вниз
            for (int y = 1; y < gray.Rows; y++)
            {
                // Получаем яркость текущего и предыдущего пикселя
                byte currentBrightness = gray.At<byte>(y, middleColumn);
                byte previousBrightness = gray.At<byte>(y - 1, middleColumn);

                // Вычисляем изменение яркости
                int brightnessChange = Math.Abs(currentBrightness - previousBrightness);

                // Если изменение яркости превышает порог, отмечаем точку
                if (brightnessChange > brightnessChangeThreshold)
                {
                    // Рисуем зеленую точку на изображении
                    Cv2.Circle(image, new OpenCvSharp.Point(middleColumn, y), 3, Scalar.Green, -1);
                }
            }

            // Сохраняем результат
            string resultImagePath = System.IO.Path.GetTempFileName() + "_result.jpg";
            Cv2.ImWrite(resultImagePath, image);

            // Показываем результат в PictureBox
            recognizePictureBox.Image = new Bitmap(resultImagePath);

            // Освобождаем ресурсы
            image.Dispose();
            gray.Dispose();
        }*/

        private void recognizeButton_Click(object sender, EventArgs e)
        {
            if (originPictureBox.Image == null)
            {
                MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                return;
            }

            string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
            originPictureBox.Image.Save(tempImagePath);

            Mat image = Cv2.ImRead(tempImagePath, ImreadModes.Color);
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

            /*// Применение бинаризации (не передаём в HoughCircles)
            Mat binary = new Mat();
            Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.Otsu);

            // Детектор границ Canny (помогает HoughCircles)
            Mat edges = new Mat();
            Cv2.Canny(gray, edges, 50, 150);

            // Вывод бинарного и Canny изображений для отладки
            Cv2.ImShow("Бинаризация", binary);
            Cv2.ImShow("Canny", edges);*/

            // Поиск окружностей (используем `gray`, а не `binary`)
            CircleSegment[] circles = Cv2.HoughCircles(
                gray, HoughModes.Gradient, 1, 20, 40, 20, 20, 60
            );

            if (circles.Length == 0)
            {
                MessageBox.Show("Окружности не найдены.");
                return;
            }

            List<double> brightnessValues = new List<double>();
            double totalBrightness = 0;

            foreach (var circle in circles)
            {
                int x = (int)circle.Center.X;
                int y = (int)circle.Center.Y;
                double brightness = gray.At<byte>(y, x);
                brightnessValues.Add(brightness);
                totalBrightness += brightness;
            }

            double averageBrightness = brightnessValues.Count > 0 ? totalBrightness / brightnessValues.Count : 0;

            int tabletCount = 0;
            int emptyCellCount = 0;
            foreach (var circle in circles)
            {
                int x = (int)circle.Center.X;
                int y = (int)circle.Center.Y;
                int radius = (int)circle.Radius;
                double brightness = gray.At<byte>(y, x);

                Scalar color = (brightness >= averageBrightness*0.7) ? Scalar.Green : Scalar.Red;
                Cv2.Circle(image, new OpenCvSharp.Point(x, y), radius, color, 2);

                if (brightness >= averageBrightness*0.7)
                    tabletCount++;
                else
                    emptyCellCount++;
            }

            string resultImagePath = System.IO.Path.GetTempFileName() + "_result.jpg";
            Cv2.ImWrite(resultImagePath, image);
            recognizePictureBox.Image = new Bitmap(resultImagePath);

            tabletkiTextBox.Text = tabletCount.ToString();
            voidTextBox.Text = emptyCellCount.ToString();

            MessageBox.Show($"Таблеток: {tabletCount}\nПустых ячеек: {emptyCellCount}\nСредняя яркость: {averageBrightness:F2}");

            image.Dispose();
            gray.Dispose();
            /*binary.Dispose();
            edges.Dispose();*/
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (originPictureBox.Image == null)
            {
                MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                return;
            }

            // Считываем значения из гистограммы
            int pillBrightness = int.Parse(textBoxPill.Text); // Яркость таблетки
            int emptyCellBrightness = int.Parse(textBoxEmptyCell.Text); // Яркость пустой ячейки
            int backgroundBrightness = int.Parse(textBoxBackground.Text); // Яркость фона
            float minPillWidth = int.Parse(textBoxPillWidth.Text); // Минимальная ширина таблетки
            int maxEmptyDistance = int.Parse(maxEmptyDistanceTextBox.Text); // Порог пустых пикселей

            string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
            originPictureBox.Image.Save(tempImagePath);

            Mat image = Cv2.ImRead(tempImagePath, ImreadModes.Color);
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

           

            // Преобразуем список в массив и выводим
            int[] columns = { leftColumn, rightColumn};

          

            bool pillDetected = false;
            bool missingPillDetected = false;

            float totalExist = 0;
            float totalVoid = 0;
            Mat highlightedImage = image.Clone(); // Копия изображения для разметки

            foreach (int column in columns)
            {
                Cv2.Line(highlightedImage, new OpenCvSharp.Point(column, 0), new OpenCvSharp.Point(column, gray.Rows - 1), Scalar.Red, 2);
            
                int consecutivePillPixels = 0;
                int emptyPixelsCount = 0;

                float countExist = 0;
                float countVoid = 0;

                List<int> yCoordinates = new List<int>();
                List<int> brightnessValues = new List<int>();

                for (int y = 0; y < gray.Rows; y++)
                {
                    byte currentBrightness = gray.At<byte>(y, column);
                    yCoordinates.Add(y);
                    brightnessValues.Add(currentBrightness);

                    if (Math.Abs(currentBrightness - pillBrightness) < 15)
                    {
                        // Найдена таблетка
                        consecutivePillPixels++;
                        countExist++;
                        emptyPixelsCount = 0;
                        Cv2.Circle(highlightedImage, new OpenCvSharp.Point(column, y), 1, Scalar.Green, -1);

                        if (consecutivePillPixels >= minPillWidth / 2)
                        {
                            pillDetected = true;
                        }
                    }
                    else if (Math.Abs(currentBrightness - emptyCellBrightness) < 15)
                    {
                        // Найдена пустая ячейка
                        consecutivePillPixels = 0;
                        emptyPixelsCount++;
                        countVoid++;

                        if (emptyPixelsCount >= minPillWidth / 2)
                        {
                            missingPillDetected = true;
                        }

                        Cv2.Circle(highlightedImage, new OpenCvSharp.Point(column, y), 1, Scalar.Red, -1);
                    }
                }

                totalExist += countExist;
                totalVoid += countVoid;
            }

            string resultMessage = (pillDetected && !missingPillDetected) ? "Таблетка присутствует!" : "Таблетка отсутствует!";
            MessageBox.Show(resultMessage);

            double countTabl = Math.Round(totalExist / minPillWidth);
            double countEmpty = Math.Round(totalVoid / minPillWidth);

            MessageBox.Show($"Количество таблеток: {countTabl}\nКоличество пустых ячеек: {countEmpty}");

            tabletkiTextBox.Text = countTabl.ToString();
            voidTextBox.Text = countEmpty.ToString();

            // Отображаем разметку
            Bitmap outputBitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(highlightedImage);
            recognizePictureBox.Image = outputBitmap;
        }



        // Метод для вычисления средней яркости на участке


        /*private void recognizeButton_Click(object sender, EventArgs e)
        {
            if (originPictureBox.Image == null)
            {
                MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                return;
            }

            // Сохранение временного изображения
            string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
            originPictureBox.Image.Save(tempImagePath);

            // Загружаем изображение в OpenCV
            Mat image = Cv2.ImRead(tempImagePath, ImreadModes.Color);
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

            int height = gray.Rows;
            int width = gray.Cols;
            int centerX = width / 2; // Анализируем центральный столбец

            List<int> brightnessValues = new List<int>();

            // Читаем яркость по центру изображения (по вертикали)
            for (int y = 0; y < height; y++)
            {
                brightnessValues.Add(gray.At<byte>(y, centerX));
            }

            int threshold = 10; // Порог резкого изменения яркости (граница)
            int? upperEdge = null;
            bool isInsideCell = false;

            for (int y = 1; y < brightnessValues.Count; y++)
            {
                int prevBrightness = brightnessValues[y - 1];
                int currBrightness = brightnessValues[y];

                // Определяем резкий скачок яркости (границу)
                if (Math.Abs(currBrightness - prevBrightness) > threshold)
                {
                    if (!isInsideCell) // Верхняя граница
                    {
                        upperEdge = y;
                        isInsideCell = true;
                        Cv2.Circle(image, new OpenCvSharp.Point(centerX, y), 10, Scalar.Blue, -1);
                        y = y + 2;
                    }
                    else // Нижняя граница
                    {
                        Cv2.Circle(image, new OpenCvSharp.Point(centerX, y), 10, Scalar.Red, -1);
                        isInsideCell = false;
                        y = y + 2;
                    }
                }
            }

            // Сохранение результата
            string resultImagePath = System.IO.Path.GetTempFileName() + "_result.jpg";
            Cv2.ImWrite(resultImagePath, image);
            recognizePictureBox.Image = new Bitmap(resultImagePath);

            image.Dispose();
            gray.Dispose();
        }*/

        /*private void recognizeButton_Click(object sender, EventArgs e)
        {
            // Проверяем, загружено ли изображение в originPictureBox
            if (originPictureBox.Image == null)
            {
                MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                return;
            }

            // Сохраняем изображение из PictureBox во временный файл
            string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
            originPictureBox.Image.Save(tempImagePath);

            // Загружаем изображение в OpenCV
            Mat image = Cv2.ImRead(tempImagePath, ImreadModes.Color);

            // Преобразуем в оттенки серого
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

            // Применяем размытие
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

            // Находим круги с помощью HoughCircles
            CircleSegment[] circles = Cv2.HoughCircles(
                gray,
                HoughModes.Gradient, // Используем HoughModes.Gradient
                1, // dp: разрешение аккумулятора
                20, // minDist: минимальное расстояние между центрами кругов
                param1: 100, // Верхний порог для Canny edge detector
                param2: 30, // Порог для обнаружения центров кругов
                minRadius: 10, // Минимальный радиус круга
                maxRadius: 50 // Максимальный радиус круга
            );

            // Счетчики для таблеток и пустых ячеек
            int tabletCount = 0;
            int emptyCellCount = 0;
            int circleCount = 0;

            // Суммы яркости для таблеток и пустых ячеек
            double totalBrightnessTabletki = 0;
            double totalBrightnessVoid = 0;

            // Анализируем каждый круг
            foreach (var circle in circles)
            {
                // Получаем область круга
                Rect roi = new Rect(
                    (int)(circle.Center.X - circle.Radius),
                    (int)(circle.Center.Y - circle.Radius),
                    (int)(2 * circle.Radius),
                    (int)(2 * circle.Radius)
                );

                // Вырезаем область круга
                Mat circleRegion = new Mat(image, roi);

                // Преобразуем в HSV для анализа цвета
                Mat hsv = new Mat();
                Cv2.CvtColor(circleRegion, hsv, ColorConversionCodes.BGR2HSV);

                // Вычисляем средний цвет в области
                Scalar meanColor = Cv2.Mean(hsv);
                circleCount++;

                // Если цвет близок к белому (например, высокая яркость)
                if (meanColor.Val2 > 210) // Val2 — это яркость в HSV
                {
                    // Это таблетка
                    Cv2.Circle(image, (int)circle.Center.X, (int)circle.Center.Y, 5, Scalar.Green, -1); // Зеленая точка
                    tabletCount++;
                    totalBrightnessTabletki += meanColor.Val2; // Накопление яркости для таблеток
                }
                else
                {
                    // Это пустая ячейка
                    Cv2.Circle(image, (int)circle.Center.X, (int)circle.Center.Y, 5, Scalar.Red, -1); // Красная точка
                    emptyCellCount++;
                    totalBrightnessVoid += meanColor.Val2; // Накопление яркости для пустых ячеек
                }
            }

            // Вычисляем среднюю яркость для таблеток и пустых ячеек
            double averageBrightnessTabletki = tabletCount > 0 ? totalBrightnessTabletki / tabletCount : 0;
            double averageBrightnessVoid = emptyCellCount > 0 ? totalBrightnessVoid / emptyCellCount : 0;

            // Сохраняем результат
            string resultImagePath = System.IO.Path.GetTempFileName() + "_result.jpg";
            Cv2.ImWrite(resultImagePath, image);

            // Показываем результат в PictureBox
            recognizePictureBox.Image = new Bitmap(resultImagePath);

            // Выводим количество таблеток и пустых ячеек
            tabletkiTextBox.Text = tabletCount.ToString();
            voidTextBox.Text = emptyCellCount.ToString();

            // Выводим среднюю яркость для таблеток и пустых ячеек
            brightnessTabletki.Text = averageBrightnessTabletki.ToString("F2"); // Форматируем до 2 знаков после запятой
            brightnessVoid.Text = averageBrightnessVoid.ToString("F2"); // Форматируем до 2 знаков после запятой

            // Выводим сообщение с результатами
            MessageBox.Show($"Таблеток обнаружено: {tabletCount}\nПустых ячеек: {emptyCellCount}\n" +
                            $"Окружностей обнаружено: {circleCount}\n" +
                            $"Средняя яркость таблеток: {averageBrightnessTabletki:F2}\n" +
                            $"Средняя яркость пустых ячеек: {averageBrightnessVoid:F2}");

            // Освобождаем ресурсы
            image.Dispose();
            gray.Dispose();
        }*/

        private void button1_Click(object sender, EventArgs e)
        {
            if (originPictureBox.Image == null)
            {
                MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                return;
            }

            string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
            originPictureBox.Image.Save(tempImagePath);

            Mat image = Cv2.ImRead(tempImagePath, ImreadModes.Color);
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

            

            // Создаём список данных для разных столбцов
            var columnsData = new List<(int column, List<int> yCoords, List<int> brightness)>
            {
                (leftColumn, new List<int>(), new List<int>()),
                (rightColumn, new List<int>(), new List<int>())
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
            chart1.Series.Clear();
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
            }
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
            HitTestResult result = chart1.HitTest(e.X, e.Y);

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
            }
        }

        private void Chart2_MouseClick(object sender, MouseEventArgs e)
        {
            HitTestResult result = chart2.HitTest(e.X, e.Y);

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
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            originPictureBox.Image = null;
            recognizePictureBox.Image = null;
            chart1.Series.Clear(); // Удаляем все серии данных из графика
            chart1.ChartAreas.Clear(); // Очищаем области графика
            dataGridView1.Rows.Clear(); // Очищаем все строки в таблице
            chart2.Series.Clear(); // Удаляем все серии данных из графика
            chart2.ChartAreas.Clear(); // Очищаем области графика
            dataGridView2.Rows.Clear(); // Очищаем все строки в таблице
        }

        private void getImageButton_Click(object sender, EventArgs e)
        {
            cam.TriggerMode = false;
            cam.SetTriggerMode();
            cam.SetExposureTime();
            StartStop(true);
        }

        public void GetImage(Mat img) //функция для получения изображения
        {
            if (!LocalSettings.Instance.UseVConcat) //если не UseVConcat (по умолчанию 0),
            {
                img1 = img.Clone(); //то создать полную копию изображения в переменной img1,
                originPictureBox.Image = MatToBitmap(img); //записать в pictureBox1.Image изображения типа bmp,
            }
            else //иначе
            {
                if (!isFirstImageCam1) //если это не первое изображение (по умолчанию isFirstImageCam1 = false),
                {
                    img1 = img.Clone(); //то создать полную копию изображения в переменной img1,
                    //Invoke( new Action(() => pictureBox1.Image = MatToBitmap(img1))); //записать в pictureBox1.Image изображение img1 типа bmp
                    originPictureBox.Image = MatToBitmap(img1); //записать в pictureBox1.Image изображение img1 типа bmp
                    isFirstImageCam1 = true; //установить флаг первого изображения,
                    //img1.SaveImage(@"E:\Camera\dataset\Image.bmp");
                }
                else //иначе
                {
                    Cv2.VConcat(img1, img.Clone(), img1); //склеить img1, img.Clone(), img1 друг за другом
                    //Invoke(new Action(() => pictureBox1.Image = MatToBitmap(img1))); //записать в pictureBox1.Image изображение img1 типа bmp
                    originPictureBox.Image = MatToBitmap(img1); //записать в pictureBox1.Image изображение img1 типа bmp
                    //img1.SaveImage(@"E:\Camera\dataset\Image.bmp");
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

        private void endStream_Click(object sender, EventArgs e)
        {
            cameraPictureBox.Image = null;
            StartStop(false);
        }
    }
}






//ГИСТОГРАМА
/*
 private void button1_Click(object sender, EventArgs e)
{
    if (originPictureBox.Image == null)
    {
        MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
        return;
    }

    string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
    originPictureBox.Image.Save(tempImagePath);

    Mat image = Cv2.ImRead(tempImagePath, ImreadModes.Color);
    Mat gray = new Mat();
    Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
    Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

    // Массив для подсчета пикселей по значениям яркости
    int[] histogram = new int[256];

    // Проходим по всем пикселям изображения и считаем яркость
    for (int y = 0; y < gray.Rows; y++)
    {
        for (int x = 0; x < gray.Cols; x++)
        {
            byte brightness = gray.At<byte>(y, x);
            histogram[brightness]++;
        }
    }

    // Теперь у нас есть гистограмма яркостей
    BuildHistogramChart(histogram);

    // Очистка ресурсов
    image.Dispose();
    gray.Dispose();
}

private void BuildHistogramChart(int[] histogram)
{
    chart1.Series.Clear();
    if (chart1.ChartAreas.Count == 0)
    {
        chart1.ChartAreas.Add(new ChartArea());
    }
    chart1.ChartAreas[0].AxisX.Title = "Brightness";
    chart1.ChartAreas[0].AxisY.Title = "Pixel Count";

    var series = new Series
    {
        Name = "Histogram",
        ChartType = SeriesChartType.Column,  // Используем столбчатую диаграмму для гистограммы
        Color = Color.Blue
    };

    for (int i = 0; i < histogram.Length; i++)
    {
        series.Points.AddXY(i, histogram[i]);
    }

    chart1.Series.Add(series);
    chart1.Invalidate();
}
*/


/*
 private void button2_Click(object sender, EventArgs e)
        {
            if (originPictureBox.Image == null)
            {
                MessageBox.Show("Пожалуйста, загрузите изображение перед распознаванием.");
                return;
            }

            // Считываем значения из гистограммы
            int pillBrightness = int.Parse(textBoxPill.Text); // Яркость таблетки
            int emptyCellBrightness = int.Parse(textBoxEmptyCell.Text); // Яркость пустой ячейки
            int backgroundBrightness = int.Parse(textBoxBackground.Text); // Яркость фона
            float minPillWidth = int.Parse(textBoxPillWidth.Text); // Минимальная ширина таблетки
            int maxEmptyDistance = int.Parse(maxEmptyDistanceTextBox.Text); // Порог пустых пикселей

            string tempImagePath = System.IO.Path.GetTempFileName() + ".jpg";
            originPictureBox.Image.Save(tempImagePath);

            Mat image = Cv2.ImRead(tempImagePath, ImreadModes.Color);
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

            // Применяем пороговую обработку для выделения столбцов
            Mat binary = new Mat();
Cv2.Threshold(gray, binary, backgroundBrightness, 255, ThresholdTypes.Binary);

// Ищем контуры
OpenCvSharp.Point[][] contours;
HierarchyIndex[] hierarchy;
Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

// Сортируем контуры по x-координатам, чтобы найти вертикальные линии (столбцы)
List<int> columnCenters = new List<int>();
foreach (var contour in contours)
{
    // Ограничение по размеру контуров, чтобы исключить слишком маленькие или большие области
    if (Cv2.ContourArea(contour) > minPillWidth) // Порог по площади
    {
        // Находим прямоугольник, охватывающий контур
        Rect boundingBox = Cv2.BoundingRect(contour);

        // Добавляем центр прямоугольника как возможный центр столбца
        int columnCenter = boundingBox.X + boundingBox.Width / 2;
        columnCenters.Add(columnCenter);
    }
}

// Сортируем найденные центры столбцов
columnCenters.Sort();

// Группировка центров столбцов, если они близки друг к другу
List<int> filteredColumns = new List<int>();
int minColumnDistance = 150; // Минимальное расстояние между центрами столбцов

foreach (var column in columnCenters)
{
    if (filteredColumns.Count == 0 || column - filteredColumns[filteredColumns.Count - 1] > minColumnDistance)
    {
        filteredColumns.Add(column);
    }
}


// Преобразуем список в массив и выводим
int[] columns = { 125, 280 };

// Помечаем центры столбцов на изображении
foreach (int columnCenter in columns)
{
    // Рисуем круг вокруг каждого центра столбца (центр + радиус)
    Cv2.Circle(image, new OpenCvSharp.Point(columnCenter, image.Rows / 2), 5, new Scalar(0, 0, 255), 2); // Красный цвет, радиус 5
}

// Сохраняем изображение с помеченными центрами
string markedImagePath = System.IO.Path.GetTempFileName() + "_marked.jpg";
Cv2.ImWrite(markedImagePath, image);

// Отображаем изображение с помеченными центрами в PictureBox
originPictureBox.Image = Image.FromFile(markedImagePath);

bool pillDetected = false;
bool missingPillDetected = false;

float totalExist = 0;
float totalVoid = 0;
Mat highlightedImage = image.Clone(); // Копия изображения для разметки

foreach (int column in columns)
{

    int consecutivePillPixels = 0;
    int emptyPixelsCount = 0;

    float countExist = 0;
    float countVoid = 0;

    List<int> yCoordinates = new List<int>();
    List<int> brightnessValues = new List<int>();

    for (int y = 0; y < gray.Rows; y++)
    {
        byte currentBrightness = gray.At<byte>(y, column);
        yCoordinates.Add(y);
        brightnessValues.Add(currentBrightness);

        if (Math.Abs(currentBrightness - pillBrightness) < 5)
        {
            // Найдена таблетка
            consecutivePillPixels++;
            countExist++;
            emptyPixelsCount = 0;
            Cv2.Circle(highlightedImage, new OpenCvSharp.Point(column, y), 1, Scalar.Green, -1);

            if (consecutivePillPixels >= minPillWidth / 2)
            {
                pillDetected = true;
            }
        }
        else if (Math.Abs(currentBrightness - emptyCellBrightness) < 5)
        {
            // Найдена пустая ячейка
            consecutivePillPixels = 0;
            emptyPixelsCount++;
            countVoid++;

            if (emptyPixelsCount >= minPillWidth / 2)
            {
                missingPillDetected = true;
            }

            Cv2.Circle(highlightedImage, new OpenCvSharp.Point(column, y), 1, Scalar.Red, -1);
        }
    }

    totalExist += countExist;
    totalVoid += countVoid;
}

string resultMessage = (pillDetected && !missingPillDetected) ? "Таблетка присутствует!" : "Таблетка отсутствует!";
MessageBox.Show(resultMessage);

double countTabl = Math.Round(totalExist / minPillWidth);
double countEmpty = Math.Round(totalVoid / minPillWidth);

MessageBox.Show($"Количество таблеток: {countTabl}\nКоличество пустых ячеек: {countEmpty}");

tabletkiTextBox.Text = countTabl.ToString();
voidTextBox.Text = countEmpty.ToString();

// Отображаем разметку
Bitmap outputBitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(highlightedImage);
recognizePictureBox.Image = outputBitmap;
        }
 */