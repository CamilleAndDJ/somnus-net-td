namespace SomnusNet.Core
{
    public static class AbsentHistoryTypeRules
    {
        public const string Label = "Absent History";
        public const BlobTypeCategory Category = BlobTypeCategory.General;
        public const int TotalUniqueInGame = 2;
        public const int InnerRangeSize = 5;
        public const int OuterRangeSize = 7;
        public const int DamagePerUniqueBlobOnField = 4;

        public static bool FeatureEnabled => BlobTypeFeatures.Enabled;
    }
}
