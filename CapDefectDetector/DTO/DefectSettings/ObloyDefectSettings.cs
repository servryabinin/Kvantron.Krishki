using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace CapDefectDetector.DTO.DefectSettings
{
    public class ObloyDefectSettings
    {
        public double CapFlashOffset { get; set; } = 3.0;

        public double NoiseContourArea { get; set; } = 10.0;

        public MorphShapes MorphShape { get; set; } = MorphShapes.Rect;
        public MorphTypes MorphType { get; set; } = MorphTypes.Erode;
        public int KernelSize { get; set; } = 3;
        public int MorphIterations { get; set; } = 1;

        public double MinAreaObloy { get; set; } = 1000.0;
    }
}
