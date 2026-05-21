using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.DTO.DefectSettings
{
    public class PaintDefectSettings
    {
        public double SMin { get; set; } = 0.05;
        public double SMax { get; set; } = 0.95;
        public double VMin { get; set; } = 0.05;
        public double VMax { get; set; } = 0.95;

        public double MinInpaintWhiteThreshold { get; set; } = 150.0;
        public double MinAreaInpaintDefect { get; set; } = 500.0;
    }
}
