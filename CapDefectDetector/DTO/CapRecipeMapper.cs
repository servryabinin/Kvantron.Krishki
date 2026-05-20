using CapDefectDetector.Domain;

namespace CapDefectDetector
{
    public static class CapRecipeMapper
    {
        public static CapContourRecipe ToDomain(this CapRecipe recipe)
        {
            return new CapContourRecipe
            {
                Name = recipe.Name,
                Kind = recipe.IsBlackOrBrown ? CapKind.BlackOrBrown : CapKind.Colored,
                CapsColor = recipe.CapsColor,
                IsGreen = recipe.IsGreen,
                IsColored = recipe.IsColored,
                IsYellow = recipe.IsYellow,
                IsWhite = recipe.IsWhite,
                Window = recipe.Window,
                MorphSize = recipe.MorphSize,
                MorphSize2 = recipe.MorphSize2,
                CameraSaturation = recipe.CameraSaturation,
                ContourCorrectionColor = recipe.ContourCorrectionColor,
                CameraSaturationBlackOrBrown = recipe.CameraSaturationBlackOrBrown,
                MedianFilter = recipe.MedianFilter,
                CannyThreshold = recipe.CannyThreshold,
                ContourCorrectionBlackOrBrown = recipe.ContourCorrectionBlackOrBrown
            };
        }

        public static CapRecipe ToDto(this CapContourRecipe recipe)
        {
            return new CapRecipe
            {
                Name = recipe.Name,
                CapsColor = recipe.CapsColor,
                Window = recipe.Window,
                MorphSize = recipe.MorphSize,
                MorphSize2 = recipe.MorphSize2,
                CameraSaturation = recipe.CameraSaturation,
                ContourCorrectionColor = recipe.ContourCorrectionColor,
                IsGreen = recipe.IsGreen,
                IsColored = recipe.IsColored,
                IsYellow = recipe.IsYellow,
                IsWhite = recipe.IsWhite,
                CameraSaturationBlackOrBrown = recipe.CameraSaturationBlackOrBrown,
                MedianFilter = recipe.MedianFilter,
                CannyThreshold = recipe.CannyThreshold,
                ContourCorrectionBlackOrBrown = recipe.ContourCorrectionBlackOrBrown,
                IsBlackOrBrown = recipe.Kind == CapKind.BlackOrBrown
            };
        }
    }
}