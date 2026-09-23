using System.Collections.Generic;
using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class BurdenedCrownSynergy
    {
        public static int GetMemberCount()
        {
            if (!BurdenedCrownTypeRules.FeatureEnabled || GridManager.Instance == null)
                return 0;

            return GridManager.Instance.CountUniqueKindsWithType(BurdenedCrownTypeRules.Label);
        }

        public static int GetCloseRangeBonusDamage(DreamBlob attacker, int targetCol, int targetRow)
        {
            if (!BurdenedCrownTypeRules.FeatureEnabled || attacker == null ||
                !attacker.HasType(BurdenedCrownTypeRules.Label))
                return 0;
            if (GridManager.Instance == null)
                return 0;

            if (!GridManager.Instance.IsInBlobRange(attacker.Column, attacker.Lane, targetCol, targetRow,
                    BurdenedCrownTypeRules.CloseRangeSize))
                return 0;

            var members = GetMemberCount();
            if (members <= 0)
                return 0;

            return members * BurdenedCrownTypeRules.CloseRangeDamagePerMember;
        }

        public static void AppendBuffLines(DreamBlob attacker, List<string> lines)
        {
            if (!BurdenedCrownTypeRules.FeatureEnabled || attacker == null ||
                !attacker.HasType(BurdenedCrownTypeRules.Label))
                return;

            var members = GetMemberCount();
            if (members <= 0)
                return;

            var bonus = members * BurdenedCrownTypeRules.CloseRangeDamagePerMember;
            lines.Add(
                $"Burdened Crown +{bonus} close-range dmg ({members} member{(members == 1 ? "" : "s")}, {BurdenedCrownTypeRules.CloseRangeSize}×{BurdenedCrownTypeRules.CloseRangeSize})");
        }
    }
}
