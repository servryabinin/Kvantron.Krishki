using CapDefectDetector.Processing;

namespace CapDefectDetector.Domain
{
    public static class CapFactory
    {
        public static Cap Create(string id, string displayName, CapContourRecipe recipe)
        {
            return new Cap
            {
                Id = id,
                DisplayName = displayName,
                Recipe = recipe
            };
        }

        public static ProcessingParameters BuildProcessingParameters(Cap cap)
        {
            var recipe = cap.Recipe;

            var param = new ProcessingParameters
            {
                CapsColor = recipe.CapsColor,
                IsGreenColor = recipe.IsGreen,
                IsYellowCap = recipe.IsYellow,
                IsColored = recipe.Kind == CapKind.Colored,
                Window = recipe.Window,
                MorphSize = recipe.MorphSize,
                MorphSize2 = recipe.MorphSize2,
                Saturation = recipe.Kind == CapKind.Colored ? recipe.CameraSaturation : recipe.CameraSaturationBlackOrBrown,
                OutlierThreshold = recipe.Kind == CapKind.Colored ? recipe.ContourCorrectionColor : recipe.ContourCorrectionBlackOrBrown
            };

            param.BuildMorphology();
            return param;
        }
    }
}