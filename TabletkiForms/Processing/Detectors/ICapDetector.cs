using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace KrishkiForms.Processing.Detectors
{
    public interface ICapDetector
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
        bool Detect(

            Mat gray,

            Mat image,

            Mat draw,

            Point[] contour,

            ProcessingParameters param,

            CancellationToken token

        );

    }
}
