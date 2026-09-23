using SomnusNet.Data;

namespace SomnusNet.Core
{
    public static class BlobTypeFeatures
    {
        public static bool Enabled =>
            LevelSettings.Instance != null && LevelSettings.Instance.enableHarmonyTypes;
    }

    public static class SpeedTypeRules
    {
        public const string Label = "Speed";
        public const BlobTypeCategory Category = BlobTypeCategory.General;
        public const int TotalUniqueInGame = 1;
        public const float AttackSpeedPerUniqueOnField = 0.08f;
    }

    public static class DreamTeamTypeRules
    {
        public const string Label = "Dream Team";
        public const BlobTypeCategory Category = BlobTypeCategory.Group;
        public const int TotalUniqueInGame = 3;
        public const float CreatorMemberAttackSpeedIntervalMultiplier = 0.88f;

        public struct MemberDefinition
        {
            public BlobKind kind;
            public string effectLine;
            public string speedBuffLabel;
            public string damageBuffLabel;
            public float attackSpeedIntervalMultiplier;
            public int bonusDamage;
            public float bondBuffMultiplier;
            public int closeRangeBonusDamage;
            public int closeRangeSize;
        }

        public static readonly MemberDefinition[] MemberRoster =
        {
            new()
            {
                kind = BlobKind.Creator,
                effectLine = "Creator — Attack Speed Inc",
                speedBuffLabel = "Atk Speed",
                damageBuffLabel = "Atk Damage",
                attackSpeedIntervalMultiplier = CreatorMemberAttackSpeedIntervalMultiplier,
                bonusDamage = 0,
                bondBuffMultiplier = 1f,
                closeRangeBonusDamage = 0,
                closeRangeSize = 0
            },
            new()
            {
                kind = BlobKind.FourOhFour,
                effectLine = "404 — Bond Buff Amp",
                speedBuffLabel = "Bond Buffs",
                damageBuffLabel = "Bond Buffs",
                attackSpeedIntervalMultiplier = 1f,
                bonusDamage = 0,
                bondBuffMultiplier = 1.08f,
                closeRangeBonusDamage = 0,
                closeRangeSize = 0
            },
            new()
            {
                kind = BlobKind.Blaze,
                effectLine = "Blaze — Close Range Dmg",
                speedBuffLabel = "Close Range",
                damageBuffLabel = "Close Range",
                attackSpeedIntervalMultiplier = 1f,
                bonusDamage = 0,
                bondBuffMultiplier = 1f,
                closeRangeBonusDamage = 6,
                closeRangeSize = 3
            }
        };

        public static readonly BlobKind[] MemberKinds =
        {
            BlobKind.Creator,
            BlobKind.FourOhFour,
            BlobKind.Blaze
        };
    }
}
