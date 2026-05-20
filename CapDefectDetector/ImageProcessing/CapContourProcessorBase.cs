using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace CapDefectDetector.ImageProcessing.CapContours
{
    public abstract class CapContourProcessorBase
    {
        public abstract Point[] GetContour(Mat gray, Mat image);

        protected abstract Mat SimulateCameraSaturation(Mat img, int saturation);

        protected abstract Point[] CorrectContour(Point[] contour, float threshold);
    }
}