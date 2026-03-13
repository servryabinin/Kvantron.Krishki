using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.Processing.Detectors
{
    public interface ICapDetector
    {

        /// <summary>
        /// Метод проверки крышек на дефект
        /// </summary>
        /// <param name="gray">Грейскейл изображение</param>
        /// <param name="image">Исходное изображение</param>
        /// <param name="drawFrame">Кадр для отрисовки результата</param>
        /// <param name="capContour">Контур крышки</param>
        /// <param name="blurChannel1">Один из каналов изображения крышек hsv</param>
        /// <param name="blurChannel2">Один из каналов изображения крышек hsv</param>
        /// <param name="param">Параметры рецепта</param>
        /// <param name="token">Токен отмены</param>
        /// <returns>true если дефект найден</returns>
        bool Detect(

            Mat gray,

            Mat image,

            Mat drawFrame,

            Point[] capContour,

            Mat blurChannel1, 

            Mat blurChannel2,

            ProcessingParameters param,

            CancellationToken token

        );

    }
}
