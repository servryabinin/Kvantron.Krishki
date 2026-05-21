using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.DTO.CameraSettings
{
    class CameraSettings
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Exposure { get; set; }
        public int  Saturation { get; set; }
    }
}
