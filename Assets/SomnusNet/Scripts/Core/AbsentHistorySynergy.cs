using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class AbsentHistorySynergy
    {
        public static bool IsTargetInFarRing(DreamBlob attacker, int targetCol, int targetRow)
        {
            if (!AbsentHistoryTypeRules.FeatureEnabled || attacker == null || !attacker.HasType(AbsentHistoryTypeRules.Label))
                return false;

            var grid = GridManager.Instance;
            if (grid == null)
                return false;

            if (!grid.IsInBlobRange(attacker.Column, attacker.Lane, targetCol, targetRow,
                    attacker.RangeSize))
                return false;

            return !grid.IsInBlobRange(attacker.Column, attacker.Lane, targetCol, targetRow,
                AbsentHistoryTypeRules.InnerRangeSize);
        }

        public static int GetBonusDamage(DreamBlob attacker, int targetCol, int targetRow)
        {
            if (!IsTargetInFarRing(attacker, targetCol, targetRow) || GridManager.Instance == null)
                return 0;

            var unique = GridManager.Instance.CountUniqueBlobKindsOnField();
            if (unique <= 0)
                return 0;

            return unique * AbsentHistoryTypeRules.DamagePerUniqueBlobOnField;
        }
    }
}
