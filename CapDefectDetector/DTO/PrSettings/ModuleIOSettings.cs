using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.DTO.PrSettings
{
    public class ModuleIOSettings
    {
        public string Name { get; set; }
        public int BreakingTime { get; set; } = 55;
        public int CameraOffset { get; set; } = 250;
        public int BreakerOffset { get; set; } = 1300;
        public BreakingSettings BreakerSettings { get; set; }
    }
}
 