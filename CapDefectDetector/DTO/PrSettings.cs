using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.DTO
{
    class PrSettings
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }  
        public int BreakingTime { get; set; }
        public int CameraOffset { get; set; }
        public int BreakerOffset { get; set; }
    }
}
