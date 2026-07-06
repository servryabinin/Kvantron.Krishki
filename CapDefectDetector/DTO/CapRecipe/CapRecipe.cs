using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.DTO.CapRecipe
{
    public class CapRecipe
    {
        public string Name { get; set; }
        public byte CapsColor { get; set; } = 100;
        public int Window { get; set; } = 5;
        public int MorphSize { get; set; } = 3;
        public int MorphSize2 { get; set; } = 3;
        public int CameraSaturation { get; set; } = 128;
        public float ContourCorrectionColor { get; set; } = 1.15f;
        public bool IsGreen { get; set; } = false;
        public bool IsColored { get; set; } = true;
        public bool IsYellow { get; set; } = false;
        public bool IsWhite { get; set; } = false;

        //------Для черных/коричневых крышек------
        public int CameraSaturationBlackOrBrown { get; set; } = 128;
        public int MedianFilter { get; set; } = 3;
        public int CannyThreshold { get; set; } = 3;
        public float ContourCorrectionBlackOrBrown { get; set; } = 1.15f;
        public bool IsBlackOrBrown { get; set; } = false;
    }

}
