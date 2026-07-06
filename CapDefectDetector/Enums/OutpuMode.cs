using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.Enums
{
    public enum OutputMode
    {
        [Description("Все")]
        All,

        [Description("Хорошие")]
        Good,

        [Description("Плохие")]
        Bad
    }
}
