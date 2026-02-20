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
        // ===== Cap contour =====

        public byte CapsColor = 0;
        public bool IsColored = true;
        public bool IsYellowCap = false;
        public bool IsGreenColor = false;

        public int Saturation = 0;

        public int Window = 15;
        public int MorphSize = 7;
        public int MorphSize2 = 7;

        public float OutlierThreshold = 1.15f;

        public byte GreenThreshold = 40;

        // morphology elements

        public Mat Element1;
        public Mat Element2;
        public Mat ElementMask;

        // ===== Defects =====

        public double OvalityThreshold = 0.7;

        public double MinInpaintWhiteThreshold = 150;
        public double MinAreaInpaintDefect = 500;

        public double InclusionThreshold = 0.5;
        public double MinAreaInclusion = 50;
        public double MaxAreaInclusion = 500;
        public double CoefCapRadiusInclusion = 0.7;

        public double MinAreaObloy = 1000;

        public double CorrugationsCountForUnderFill = 10;
        public double CoefCapRadiusUnderFill = 0.8;


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
