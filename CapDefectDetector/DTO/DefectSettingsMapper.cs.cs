using CapDefectDetector.Domain;

namespace CapDefectDetector.DTO
{
    internal static class DefectSettingsMapper
    {
        public static DefectSettingsModel ToDomain(this DefectSettings dto)
        {
            return new DefectSettingsModel
            {
                Ovality = new OvalitySettings 
                { 
                    OvalityThreshold = dto.OvalityThreshold 
                },

                Inclusion = new InclusionSettings
                {
                    InclusionThreshold = dto.InclusionThreshold,
                    MinAreaInclusion = dto.MinAreaInclusion,
                    MaxAreaInclusion = dto.MaxAreaInclusion,
                    CoefCapRadiusInclusion = dto.CoefCapRadiusInclusion
                },

                Inpaint = new InpaintSettings
                {
                    MinAreaInpaintDefect = dto.MinAreaInpaintDefect,
                    MinInpaintWhiteThreshold = dto.MinInpaintWhiteThreshold
                },

                Obloy = new ObloySettings 
                { 
                    MinAreaObloy = dto.MinAreaObloy 
                },

                Underfill = new UnderfillSettings
                {
                    CorrugationsCountForUnderFill = dto.CorrugationsCountForUnderFill,
                    CoefCapRadiusUnderFill = dto.CoefCapRadiusUnderFill
                }
            };
        }
    }
}