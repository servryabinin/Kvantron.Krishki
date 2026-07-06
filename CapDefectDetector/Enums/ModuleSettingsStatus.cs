using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.Enums
{
    public enum ModuleSettingsStatus
    {
        [Description("Настройки применены")]
        Applied,

        [Description("Настройки неприменены")]
        NotApplied
    }
}
