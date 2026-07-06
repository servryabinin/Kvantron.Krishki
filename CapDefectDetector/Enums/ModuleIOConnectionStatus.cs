using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.Enums
{
    public enum ModuleIOConnectionStatus
    {
        [Description("Подключено")]
        Connected,

        [Description("Не подключено")]
        Disconnected,

        [Description("Откл. вручную")]
        ManualDisconnected
    }
}
