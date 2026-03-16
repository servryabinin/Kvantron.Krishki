using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.StatisticProcessing
{
    public class CapStatistics
    {
        public float Number { get; set; }
        public bool IsNg { get; set; }
        public string Status => IsNg ? "NG" : "OK";
        public string Defects { get; set; }
        public string SaveFolder { get; set; }
        public string ImageName { get; set; }
    }
}
