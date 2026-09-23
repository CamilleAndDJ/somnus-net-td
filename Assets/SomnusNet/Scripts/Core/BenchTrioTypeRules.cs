using SomnusNet.Data;

namespace SomnusNet.Core
{
    public static class BenchTrioTypeRules
    {
        public const string Label = "Bench Trio";
        public const BlobTypeCategory Category = BlobTypeCategory.Group;
        public const int TotalUniqueInGame = 3;

        public struct MemberDefinition
        {
            public BlobKind kind;
            public string effectLine;
            public int strifeBonusLevels;
        }

        public static readonly MemberDefinition[] MemberRoster =
        {
            new()
            {
                kind = BlobKind.Countdown,
                effectLine = "Countdown — Strife +1",
                strifeBonusLevels = 1
            },
            new()
            {
                kind = BlobKind.Lantern,
                effectLine = "Lantern — Bench Trio atk speed on field; pulse dmg buff",
                strifeBonusLevels = 0
            },
            new()
            {
                kind = BlobKind.Troublemaker,
                effectLine = "Troublemaker — (coming soon)",
                strifeBonusLevels = 0
            }
        };

        public static readonly BlobKind[] MemberKinds =
        {
            BlobKind.Countdown,
            BlobKind.Lantern,
            BlobKind.Troublemaker
        };

        public static bool FeatureEnabled => BlobTypeFeatures.Enabled;
    }
}
