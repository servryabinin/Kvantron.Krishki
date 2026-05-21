using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.DTO.DefectSettings
{
    public class DefectSettings
    {
        public string Name { get; set; }

        public OvalityDefectSettings Ovality { get; set; } = new();

        public InclusionDefectSettings Inclusion { get; set; } = new();

        public PaintDefectSettings Paint { get; set; } = new();

        public ObloyDefectSettings Obloy { get; set; } = new();

        public UnderfillDefectSettings Underfill { get; set; } = new();
    }
}
