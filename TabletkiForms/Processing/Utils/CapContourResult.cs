using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.Processing.Utils
{
    public class CapContourResult
    {
        public Point[] Contour { get; set; }
        public Mat BlurChannel1 { get; set; }
        public Mat BlurChannel2 { get; set; }
    }
}
