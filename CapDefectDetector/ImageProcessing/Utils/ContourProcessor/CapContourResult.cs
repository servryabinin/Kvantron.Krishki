using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.ImageProcessing.Utils.ContourProcessor
{
    public class CapContourResult
    {
        public Point[] Contour { get; set; }
        public Mat Blur1 { get; set; }
        public Mat Blur2 { get; set; }
        public Mat Mask { get; set; }
    }
}
