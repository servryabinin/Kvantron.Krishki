using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector
{
    public class CapRecipe
    {
        public string Name { get; set; }
        public byte CapsColor { get; set; }
        public int Window { get; set; }
        public int MorphSize { get; set; }
        public int MorphSize2 { get; set; }
        public int CameraSaturation { get; set; }
        public bool IsGreen { get; set; }
        public bool IsColored { get; set; }
        public bool IsYellow { get; set; }
        public bool IsWhite { get; set; }
    }

}
