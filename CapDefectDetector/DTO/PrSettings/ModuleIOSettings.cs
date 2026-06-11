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
        public int BreakingTime { get; set; }
        public int CameraOffset { get; set; }
        public int BreakerOffset { get; set; }
    }
}
 