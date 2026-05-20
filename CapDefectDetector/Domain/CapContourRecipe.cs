namespace CapDefectDetector.Domain
{
    /// <summary>
    /// Параметры, относящиеся только к поиску контура.
    /// </summary>
    public sealed class CapContourRecipe
    {
        public string Name { get; init; } = string.Empty;
        public CapKind Kind { get; init; }

        public byte CapsColor { get; init; }
        public bool IsGreen { get; init; }
        public bool IsColored { get; init; }
        public bool IsYellow { get; init; }
        public bool IsWhite { get; init; }

        public int Window { get; init; }
        public int MorphSize { get; init; }
        public int MorphSize2 { get; init; }
        public int CameraSaturation { get; init; }
        public float ContourCorrectionColor { get; init; }

        public int CameraSaturationBlackOrBrown { get; init; }
        public int MedianFilter { get; init; }
        public int CannyThreshold { get; init; }
        public float ContourCorrectionBlackOrBrown { get; init; }
    }
}