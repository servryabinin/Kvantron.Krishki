using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.DTO.DefectSettings
{
    public class UnderfillDefectSettings
    {
        public byte CapsColor { get; set; } = 140;
        public bool DecolorizeBackground { get; set; } = true;
        public bool NormalizeBrightness { get; set; } = true;
        public bool PreserveDetails { get; set; } = false;

        public double CoefCapRadiusUnderFill { get; set; } = 0.8;

        public double RectHeightCoef { get; set; } = 0.15;
        public int InnerOffset { get; set; } = 7;

        public int UnderfillRectWidth { get; set; } = 1024;
        public double CorrugationsCountForUnderFill { get; set; } = 10;
    }
}
