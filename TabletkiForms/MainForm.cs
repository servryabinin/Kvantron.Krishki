/*using Kvantron.Hardware.SmartDio;
using Kvantron.Ruberoid.Hardwares;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GetImageProject
{
	public partial class MainForm : Form
    {

        #region Инициализация
        //int height = 0;
        int i = 0;
        string FileName = null;
        HikCamera cam = new HikCamera(LocalSettings.Instance.Cam1SN);
        bool currentDetectorState = false;
        DioModule module = null;
        bool isFirstImageCam1 = false;
        Mat img1 = new Mat(); //camera image

        //Cv2.ImShow("Display Window", image);
        //Cv2.WaitKey(0);

        bool cameraError1 = false; //error message output

        public MainForm()
        {
            InitializeComponent();

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

        #endregion

        #region Старт-стоп основной работы программы 

        private void StartStop(bool isStart, bool isInit = false)
        {
            if (!isStart)
            {
                StartButton.Visible = true;
                StopButton.Visible = false;
                if (!isInit && !LocalSettings.Instance.UseModule)
                {
                    if(!cameraError1 && cam.Streamed) cam.EndStream();
				}
                return;
            }

			StartButton.Visible = false;
			StopButton.Visible = true;

            isFirstImageCam1 = false;

			if (!isInit && !LocalSettings.Instance.UseModule)
            {
				if (!cameraError1 && !cam.Streamed) cam.StartStream();
                img1 = new Mat();
			}
        }

		#endregion

		#region Получение сигналов с модуля, изображений с камер

		public void GetModuleState(int number, bool state) //функция получения состояния модуля
        {
            if(number == LocalSettings.Instance.DINumber && currentDetectorState == true) //если number равно единице и флаг текущего состояния равен 1,
            {
                module.SetOutput(LocalSettings.Instance.DONumber, true); //то module равен 1

                if (!cameraError1) //если флаг ошибки не выставлен,
                {
                    new System.Threading.Thread(new System.Threading.ThreadStart(() => cam.StartStream())).Start(); //то начать стрим        
                }
                
                currentDetectorState = false; //сбросить состояние
            }

            else if(number == LocalSettings.Instance.DINumber && currentDetectorState == false) //иначе
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

        public void GetImage(Mat img) //функция для получения изображения
        {
            if (!LocalSettings.Instance.UseVConcat) //если не UseVConcat (по умолчанию 0),
            {
				img1 = img.Clone(); //то создать полную копию изображения в переменной img1,
				pictureBox1.Image = MatToBitmap(img); //записать в pictureBox1.Image изображения типа bmp,
            }
            else //иначе
            {
                if (!isFirstImageCam1) //если это не первое изображение (по умолчанию isFirstImageCam1 = false),
                {
					img1 = img.Clone(); //то создать полную копию изображения в переменной img1,
                    //Invoke( new Action(() => pictureBox1.Image = MatToBitmap(img1))); //записать в pictureBox1.Image изображение img1 типа bmp
                    pictureBox1.Image = MatToBitmap(img1); //записать в pictureBox1.Image изображение img1 типа bmp
                    isFirstImageCam1 = true; //установить флаг первого изображения,
                    //img1.SaveImage(@"E:\Camera\dataset\Image.bmp");
                }
                else //иначе
                {
                    Cv2.VConcat(img1, img.Clone(), img1); //склеить img1, img.Clone(), img1 друг за другом
                    //Invoke(new Action(() => pictureBox1.Image = MatToBitmap(img1))); //записать в pictureBox1.Image изображение img1 типа bmp
                    pictureBox1.Image = MatToBitmap(img1); //записать в pictureBox1.Image изображение img1 типа bmp
                    //img1.SaveImage(@"E:\Camera\dataset\Image.bmp");
                }
            }
            FileName = string.Concat("E:\\Camera\\dataset\\Image", i.ToString(), ".bmp");
            img.SaveImage(@FileName);
            i += 1;
        }
		#endregion

		private void CloseProgramButton_Click(object sender, EventArgs e)
        {
            if (!cameraError1)
            {
                if(cam.Streamed)
                    cam.EndStream();

				cam.Close();
			}

            Application.Exit();
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

		private void StartButton_Click(object sender, EventArgs e)
		{
            cam.TriggerMode = false;
            cam.SetTriggerMode();
            cam.SetExposureTime();
            StartStop(true);
        }

		private void StopButton_Click(object sender, EventArgs e)
		{
			StartStop(false);
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
            CloseProgramButton_Click(null, null);
		}

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private void applySettings_Click(object sender, EventArgs e)
        {
            uint width = uint.Parse(widthTextBox.Text.ToString());
            uint height = uint.Parse(heightTextBox.Text.ToString());
            cam.Width = width;
            cam.Height = height;
            cam.SetHeight();
            cam.SetWidth();
            pictureBox1.Width = 840;
            pictureBox1.Height = (int)(840 * height / width);
        }

        private void ExposureTextBox_TextChanged(object sender, EventArgs e)
        {
            cam.ExposureTime = uint.Parse(ExposureTextBox.Text.ToString());
            cam.SetExposureTime();
        }

        private void GainTextBox_TextChanged(object sender, EventArgs e)
        {
            cam.Gain = uint.Parse(GainTextBox.Text.ToString());
            cam.SetGain();
        }
    }
}
*/