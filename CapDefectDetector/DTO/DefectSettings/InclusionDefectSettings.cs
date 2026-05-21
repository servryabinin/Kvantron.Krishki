using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace CapDefectDetector.DTO.DefectSettings
{
    public class InclusionDefectSettings
    {
        public double CoefCapRadiusInclusion { get; set; } = 0.7;

        public AdaptiveThresholdTypes AdaptiveType { get; set; } = AdaptiveThresholdTypes.MeanC;
        public ThresholdTypes ThresholdType { get; set; } = ThresholdTypes.Binary;
        public int BlockSize { get; set; } = 11;
        public double C { get; set; } = 2;

        public MorphShapes MorphShape { get; set; } = MorphShapes.Ellipse;
        public MorphTypes MorphType { get; set; } = MorphTypes.Open;
        public int KernelSize { get; set; } = 3;
        public int MorphIterations { get; set; } = 1;

        public double MinAreaInclusion { get; set; } = 50.0;
        public double MaxAreaInclusion { get; set; } = 500.0;
        public double InclusionThreshold { get; set; } = 0.5;
    }
}
