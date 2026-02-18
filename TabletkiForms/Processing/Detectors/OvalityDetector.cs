using OpenCvSharp;
using System;
using System.Threading;
using KrishkiForms.Processing;
using Point = OpenCvSharp.Point; // для ProcessingParameters

namespace KrishkiForms.Processing.Detectors
{
    internal class OvalityDetector : ICapDetector
    {
        /// <summary>
        /// Проверка овальности крышки по контуру
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

                // Контур пустой или слишком маленький — дефект не определяем
                if (contour == null || contour.Length < 5)
                    return false;

                token.ThrowIfCancellationRequested();

                // Подгоняем эллипс под контур
                RotatedRect ellipse = Cv2.FitEllipse(contour);

                double major = Math.Max(ellipse.Size.Width, ellipse.Size.Height);
                double minor = Math.Min(ellipse.Size.Width, ellipse.Size.Height);

                double ratio = minor / major;

                token.ThrowIfCancellationRequested();

                // Проверка на дефект по порогу из параметров рецепта
                bool defect = ratio < param.OvalityThreshold;

                // Цвет для отрисовки
                Scalar color = defect ? Scalar.Red : Scalar.Green;

                if (drawFrame != null && !drawFrame.Empty())
                {
                    try
                    {
                        Cv2.Ellipse(drawFrame, ellipse, color, 2);
                        Cv2.PutText(drawFrame, $"Ratio:{ratio:F5}",
                            new Point(10, 30),
                            HersheyFonts.HersheySimplex,
                            1,
                            color,
                            2);
                    }
                    catch
                    {
                        // Игнорируем ошибки рисования
                    }
                }

                return defect;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}