using System.Collections.Generic;
using SomnusNet.Data;
using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class StrifeRules
    {
        public const float ExtraAttackChancePerLevel = 0.10f;
        public const float FragilityDurationSeconds = 4f;
        public const float DamageMultiplierWhileFragile = 2f;

        public static void AppendModifierLine(DreamBlob blob, List<string> lines)
        {
            if (blob == null || lines == null)
                return;

            if (BenchTrioTypeRules.FeatureEnabled && blob.HasType(BenchTrioTypeRules.Label))
                return;

            var level = blob.StrifeLevel;
            if (level <= 0)
                return;

            lines.Add(DescribeStrifeEffect(blob.Kind, level));
        }

        public static string DescribeStrifeEffect(BlobKind kind, int level)
        {
            var chance = level * ExtraAttackChancePerLevel;
            return kind switch
            {
                BlobKind.Cheerful => $"Strife Lv.{level} ({chance:P0} extra Best Friend buff)",
                BlobKind.Countdown => $"Strife Lv.{level} ({chance:P0} fragility shot)",
                BlobKind.Lantern => $"Strife Lv.{level} ({chance:P0} longer next pulse)",
                _ => $"Strife Lv.{level} ({chance:P0} fragility shot)"
            };
        }

        public static string FormatLevels(int levels) => $"Strife Lv.{levels}";
    }
}
