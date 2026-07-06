using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.Enums
{
    public enum CameraConnectionStatus
    {
        [Description("Подключено")]
        Connected,

        [Description("Не подключено")]
        Disconnected
    }
}
