using OpenCvSharp;

namespace CapDefectDetector.Domain
{
    public sealed class DefectSettingsFormContext
    {
        public required Cap Cap { get; init; }
        public required CapContourRecipe Recipe { get; init; }
        public required Mat ImageForTest { get; init; }
    }
}