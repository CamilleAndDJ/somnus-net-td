using System.Collections.Generic;
using SomnusNet.Data;
using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class BenchTrioSynergy
    {
        static bool IsRosterComplete() =>
            GridManager.Instance != null &&
            GridManager.Instance.AreAllRosterMembersOnField(BenchTrioTypeRules.MemberKinds);

        public static int GetStrifeLevel(DreamBlob recipient)
        {
            if (!BenchTrioTypeRules.FeatureEnabled || recipient == null ||
                !recipient.HasType(BenchTrioTypeRules.Label))
                return 0;
            if (GridManager.Instance == null)
                return 0;
            if (!IsRosterComplete())
                return 0;

            var strife = 0;
            foreach (var member in BenchTrioTypeRules.MemberRoster)
            {
                if (member.strifeBonusLevels <= 0)
                    continue;
                if (GridManager.Instance.HasAliveBlobKind(member.kind))
                    strife += member.strifeBonusLevels;
            }

            return strife;
        }

        public static float GetLanternFieldIntervalMultiplier(DreamBlob recipient)
        {
            if (!BenchTrioTypeRules.FeatureEnabled || recipient == null ||
                !recipient.HasType(BenchTrioTypeRules.Label))
                return 1f;

            if (!LanternRules.ReceivesPulseBonus(recipient.Kind))
                return 1f;

            if (!IsRosterComplete())
                return 1f;

            return LanternRules.FieldAttackSpeedIntervalMultiplier;
        }

        public static float GetLanternFieldAttackSpeedMultiplier(DreamBlob recipient)
        {
            var interval = GetLanternFieldIntervalMultiplier(recipient);
            return interval >= 1f ? 1f : 1f / interval;
        }
    }
}
