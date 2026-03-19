using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.DTO
{
    class DefectSettings
    {
        public string Name { get; set; }
        public double OvalityThreshold { get; set; }
        public double InclusionThreshold { get; set; }
        public double MinAreaInclusion { get; set; }
        public double MaxAreaInclusion { get; set; }
        public double CoefCapRadiusInclusion { get; set; }
        public double MinAreaInpaintDefect { get; set; }
        public double MinInpaintWhiteThreshold { get; set; }
        public double MinAreaObloy { get; set; }
        public double CorrugationsCountForUnderFill { get; set; }
        public double CoefCapRadiusUnderFill { get; set; }
    }
}
