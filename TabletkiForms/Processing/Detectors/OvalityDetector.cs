using KrishkiForms.Logger;
using KrishkiForms.Processing;
using OpenCvSharp;
using System;
using System.Threading;
using Point = OpenCvSharp.Point;

namespace KrishkiForms.Processing.Detectors
{
    internal class OvalityDetector : ICapDetector
    {
        private readonly double _ovalityThreshold;

        public OvalityDetector(ProcessingParameters param)
        {
            _ovalityThreshold = param.OvalityThreshold;
        }

        /// <summary>
        /// Проверка овальности крышки по контуру через аппроксимацию эллипсом. 
        /// Если отношение меньшей оси к большей меньше заданного порога, крышка считается дефектной. 
        /// Результат отрисовывается на кадре, если он предоставлен. 
        /// В случае ошибок при расчёте или отрисовке, они логируются, а метод возвращает false.
        /// </summary>
        /// <param name="gray">Грейскейл изображение</param>
        /// <param name="image">Исходное изображение</param>
        /// <param name="drawFrame">Кадр для отрисовки результата</param>
        /// <param name="contour">Контур крышки</param>
        /// <param name="param">Параметры рецепта</param>
        /// <param name="token">Токен отмены</param>
        /// <returns>true если дефект найден</returns>
        public bool Detect(
            Mat gray,
            Mat image,
            Mat drawFrame,
            Point[] contour,
            ProcessingParameters param,
            CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (contour == null || contour.Length < 5)
                {
                    ErrorLogger.Log(new Exception("Контур для проверки овальности пустой или содержит недостаточно точек"),
                        "OvalityDetector.Detect - проверка наличия контура");
                    return false;
                }

                token.ThrowIfCancellationRequested();

                RotatedRect ellipse = Cv2.FitEllipse(contour);

                double majorAxis = Math.Max(ellipse.Size.Width, ellipse.Size.Height);
                double minorAxis = Math.Min(ellipse.Size.Width, ellipse.Size.Height);
                double axisRatio = minorAxis / majorAxis;

                token.ThrowIfCancellationRequested();

                bool isOval = axisRatio < _ovalityThreshold;

                if (drawFrame != null && !drawFrame.Empty())
                {
                    try
                    {
                        Scalar color = isOval ? new Scalar(0, 0, 255) : new Scalar(0, 255, 0);
                        Cv2.Ellipse(drawFrame, ellipse, color, 2);
                        Cv2.PutText(drawFrame, $"Ratio: {axisRatio:F5}", new Point(10, 30),
                                    HersheyFonts.HersheySimplex, 1, color, 2);
                    }
                    catch (Exception drawEx)
                    {
                        ErrorLogger.Log(drawEx, "OvalityDetector.Detect - ошибка при рисовании эллипса или текста");
                    }
                }

                return isOval;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "OvalityDetector.Detect - ошибка при расчёте овальности крышки");
                return false;
            }
        }
    }
}