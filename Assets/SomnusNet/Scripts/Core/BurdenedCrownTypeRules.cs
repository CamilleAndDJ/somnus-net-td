namespace SomnusNet.Core
{
    public static class BurdenedCrownTypeRules
    {
        public const string Label = "Burdened Crown";
        public const BlobTypeCategory Category = BlobTypeCategory.General;
        public const int TotalUniqueInGame = 1;
        public const int CloseRangeSize = 3;
        public const int CloseRangeDamagePerMember = 3;

        public static bool FeatureEnabled => BlobTypeFeatures.Enabled;
    }
}
