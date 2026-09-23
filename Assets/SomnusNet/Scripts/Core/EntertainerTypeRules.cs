namespace SomnusNet.Core
{
    public static class EntertainerTypeRules
    {
        public const string Label = "Entertainer";
        public const BlobTypeCategory Category = BlobTypeCategory.General;
        public const int TotalUniqueInGame = 1;
        public const int DamagePerAdjacentEntertainer = 3;

        public static bool FeatureEnabled => BlobTypeFeatures.Enabled;
    }
}
