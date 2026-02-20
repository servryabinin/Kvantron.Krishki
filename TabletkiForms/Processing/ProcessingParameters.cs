using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.Processing
{
    using OpenCvSharp;

    public class ProcessingParameters
    {
        // Все парамтеры, относящиеся к нахождению контура крышки (они являются рецептом)
        public byte CapsColor = 0;
        public bool IsColored = true;
        public bool IsYellowCap = false;
        public bool IsGreenColor = false;
        public int Saturation = 0;
        public int Window = 15;
        public int MorphSize = 7;
        public int MorphSize2 = 7;
        public Mat Element1;                                //нужно для рассчета морфологии
        public Mat Element2;                                //нужно для рассчета морфологии
        public Mat ElementMask;                             //нужно для при обработке облоя
        public float OutlierThreshold = 1.15f;              //постоянное значения для ограничения контура при сильном шуме
        public byte GreenThreshold = 40;                    //постоянное значение нужно если крышка зеленая

        // Параметры дефектов
        public double OvalityThreshold = 0.7;               // 1.1. коэффициент овальности - овальность

        public double MinInpaintWhiteThreshold = 150;       // 2.1. близость к белому - непрокрас
        public double MinAreaInpaintDefect = 500;           // 2.2. минимальная площадь непрокраса - непрокрас

        public double InclusionThreshold = 0.5;             // 3.1. коэффициент округлости вкраплений - вкрапления
        public double MinAreaInclusion = 50;                // 3.2. минимальная площадь вкрапления - вкрапления
        public double MaxAreaInclusion = 500;               // 3.3. макимальная площадь вкрапления - вкрапления
        public double CoefCapRadiusInclusion = 0.7;         // 3.4. коэффициент (часть) от радиуса крышки внутри которого будет искаться вкрапление - вкрапления

        public double MinAreaObloy = 1000;                  // 4.1. минимальная площадь облоя - облой

        public double CorrugationsCountForUnderFill = 10;   // 5.1. количество зубцов на коронке - облой
        public double CoefCapRadiusUnderFill = 0.8;         // 5.2. коэффициент (часть) от радиуса крышки который будет создавать маску для поиска облоя - облой

        /// <summary>
        /// Метод для рассчета морфологических элементов
        /// </summary>
        public void BuildMorphology()
        {
            Element1 = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new Size(2 * MorphSize + 1, 2 * MorphSize + 1));

            Element2 = Cv2.GetStructuringElement(
                MorphShapes.Cross,
                new Size(2 * MorphSize2 + 1, 2 * MorphSize2 + 1));

            ElementMask = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new Size(3, 3));
        }
    }
}
