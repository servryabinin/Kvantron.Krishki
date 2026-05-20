namespace CapDefectDetector.Domain
{
    public sealed class DefectSettingsModel
    {
        public OvalitySettings Ovality { get; init; } = new();
        public InclusionSettings Inclusion { get; init; } = new();
        public InpaintSettings Inpaint { get; init; } = new();
        public ObloySettings Obloy { get; init; } = new();
        public UnderfillSettings Underfill { get; init; } = new();
    }

    public sealed class OvalitySettings
    {
        public double OvalityThreshold { get; init; }
    }

    public sealed class InclusionSettings
    {
        public double InclusionThreshold { get; init; }
        public double MinAreaInclusion { get; init; }
        public double MaxAreaInclusion { get; init; }
        public double CoefCapRadiusInclusion { get; init; }
    }

    public sealed class InpaintSettings
    {
        public double MinAreaInpaintDefect { get; init; }
        public double MinInpaintWhiteThreshold { get; init; }
    }

    public sealed class ObloySettings
    {
        public double MinAreaObloy { get; init; }
    }

    public sealed class UnderfillSettings
    {
        public double CorrugationsCountForUnderFill { get; init; }
        public double CoefCapRadiusUnderFill { get; init; }
    }
}