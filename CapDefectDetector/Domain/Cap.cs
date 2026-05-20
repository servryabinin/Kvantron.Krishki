namespace CapDefectDetector.Domain
{
    /// <summary>
    /// Доменная сущность «Крышка».
    /// </summary>
    public sealed class Cap
    {
        public string Id { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public CapContourRecipe Recipe { get; init; } = new();

        public bool IsColored => Recipe.Kind == CapKind.Colored;
        public bool IsBlackOrBrown => Recipe.Kind == CapKind.BlackOrBrown;
    }
}