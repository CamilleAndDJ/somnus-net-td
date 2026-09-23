using SomnusNet.Data;

namespace SomnusNet.Core
{
    public static class SlimySupportTypeRules
    {
        public const string Label = "Slimy Support";
        public const BlobTypeCategory Category = BlobTypeCategory.Group;
        public const int TotalUniqueInGame = 2;
        public const int HeartEaterLuckBonus = 2;

        public struct MemberDefinition
        {
            public BlobKind kind;
            public string effectLine;
            public int luckBonusLevels;
            public int bonusDamage;
        }

        public static readonly MemberDefinition[] MemberRoster =
        {
            new()
            {
                kind = BlobKind.Dealer,
                effectLine = "Dealer — Luck +1",
                luckBonusLevels = 1,
                bonusDamage = 0
            },
            new()
            {
                kind = BlobKind.Cheerful,
                effectLine = "Cheerful — Attack Damage +2",
                luckBonusLevels = 0,
                bonusDamage = 2
            }
        };

        public static readonly BlobKind[] MemberKinds =
        {
            BlobKind.Dealer,
            BlobKind.Cheerful
        };

        public static bool FeatureEnabled => BlobTypeFeatures.Enabled;
    }
}
