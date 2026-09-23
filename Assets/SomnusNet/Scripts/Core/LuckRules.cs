using System.Collections.Generic;
using SomnusNet.Data;
using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class LuckRules
    {
        public const float ExtraAttackChancePerLevel = 0.10f;

        public static void AppendModifierLine(DreamBlob blob, List<string> lines)
        {
            if (blob == null || lines == null)
                return;

            var level = blob.LuckLevel;
            if (level <= 0)
                return;

            lines.Add(
                blob.Kind switch
                {
                    BlobKind.Countdown => $"Luck Lv.{level} ({level * ExtraAttackChancePerLevel:P0} extra mine)",
                    BlobKind.Cheerful => $"Luck Lv.{level} ({level * ExtraAttackChancePerLevel:P0} extra cheer)",
                    BlobKind.Lantern => $"Luck Lv.{level} ({level * ExtraAttackChancePerLevel:P0} extra pulse)",
                    _ => $"Luck Lv.{level} ({level * ExtraAttackChancePerLevel:P0} extra attack)"
                });
        }

        public static string FormatLevels(int levels) => $"Luck Lv.{levels}";
    }
}
