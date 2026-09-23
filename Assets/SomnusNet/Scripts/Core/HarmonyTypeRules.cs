namespace SomnusNet.Core
{
    public static class HarmonyTypeRules
    {
        public const string HarmonyLabel = "Harmony";
        public const BlobTypeCategory Category = BlobTypeCategory.General;
        public const int TotalUniqueHarmonyTypes = 1;
        public const int BandRangeSize = 3;
        public const int DamagePerNearbyBlob = 4;

        public static bool FeatureEnabled => BlobTypeFeatures.Enabled;
    }
}
