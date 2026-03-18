using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace CapDefectDetector.ResultStateAndProcessingSettings
{
    class ResultState
    {
        public Mat Frame;

        public float GeneralCapsCount;
        public float Ok;
        public float Ng;
        public float PercentOK;
        public float PercentNG;

        public int OvalityDefectCount;
        public int InclusionDefectCount;
        public int PaintDefectCount;
        public int ObloyDefectCount;
        public int UnderFillDefectCount;

        public float PercentOvality;
        public float PercentInclusion;
        public float PercentPaint;
        public float PercentObloy;
        public float PercentUnderFill;

        public float TimeOvality;
        public float TimeInclusion;
        public float TimePaint;
        public float TimeObloy;
        public float TimeUnderFill;

        public float Time;
        public bool IsNg;

        public string DefectText;
        public string SaveFolder;
        public string FileName;
    }

    class ProcessingSettings
    {
        public bool Ovality;
        public bool Inclusion;
        public bool Inpaint;
        public bool Obloy;
        public bool UnderFill;

        public bool SaveOk;
        public bool SaveNg;
    }
}
