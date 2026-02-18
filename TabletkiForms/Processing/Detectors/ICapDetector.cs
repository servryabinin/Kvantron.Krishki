using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace KrishkiForms.Processing.Detectors
{
    public interface ICapDetector
    {

        bool Detect(

            Mat gray,

            Mat image,

            Mat draw,

            Point[] contour,

            ProcessingParameters param,

            CancellationToken token

        );

    }
}
