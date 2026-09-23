namespace SomnusNet.Core
{
    public enum BlobTypeCategory
    {
        General,
        Group
    }

    public static class BlobTypeCategoryLabels
    {
        public const string General = "General Type";
        public const string Group = "Group Type";

        public static string DisplayName(BlobTypeCategory category) =>
            category == BlobTypeCategory.Group ? Group : General;
    }
}
