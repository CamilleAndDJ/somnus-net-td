using SomnusNet.Data;

namespace SomnusNet.Core
{
    public static class BrokenRingsTypeRules
    {
        public const string Label = "Broken Rings";
        public const BlobTypeCategory Category = BlobTypeCategory.Group;
        public const int TotalUniqueInGame = 3;

        public struct MemberDefinition
        {
            public BlobKind kind;
            public string effectLine;
            public int bonusDamage;
            public float attackSpeedIntervalMultiplier;
            public int closeRangeBonusDamage;
            public int closeRangeSize;
        }

        public static readonly MemberDefinition[] MemberRoster =
        {
            new()
            {
                kind = BlobKind.Blaze,
                effectLine = "Blaze — Attack Damage Inc",
                bonusDamage = 5,
                attackSpeedIntervalMultiplier = 1f
            },
            new()
            {
                kind = BlobKind.Dealer,
                effectLine = "Dealer — Attack Speed Inc",
                bonusDamage = 0,
                attackSpeedIntervalMultiplier = 0.88f
            },
            new()
            {
                kind = BlobKind.Archivist,
                effectLine = "Archivist — Close Range Damage Inc",
                bonusDamage = 0,
                attackSpeedIntervalMultiplier = 1f,
                closeRangeBonusDamage = ArchivistRules.BrokenRingsCloseRangeBonusDamage,
                closeRangeSize = ArchivistRules.BrokenRingsCloseRangeSize
            }
        };

        public static readonly BlobKind[] MemberKinds =
        {
            BlobKind.Blaze,
            BlobKind.Dealer,
            BlobKind.Archivist
        };

        public static bool FeatureEnabled => BlobTypeFeatures.Enabled;
    }
}
