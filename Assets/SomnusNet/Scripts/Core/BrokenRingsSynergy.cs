using System.Collections.Generic;
using SomnusNet.Data;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Core
{
    public static class BrokenRingsSynergy
    {
        static bool IsRosterComplete() =>
            GridManager.Instance != null &&
            GridManager.Instance.AreAllRosterMembersOnField(BrokenRingsTypeRules.MemberKinds);

        public static int GetBonusDamage(DreamBlob recipient)
        {
            if (!BrokenRingsTypeRules.FeatureEnabled || recipient == null ||
                !recipient.HasType(BrokenRingsTypeRules.Label))
                return 0;
            if (GridManager.Instance == null)
                return 0;
            if (!IsRosterComplete())
                return 0;

            var bonus = 0;
            foreach (var member in BrokenRingsTypeRules.MemberRoster)
            {
                if (member.bonusDamage <= 0)
                    continue;
                if (GridManager.Instance.HasAliveBlobKind(member.kind))
                    bonus += member.bonusDamage;
            }

            return DreamTeamSynergy.ScaleBondBonus(bonus, recipient);
        }

        public static int GetCloseRangeBonusDamage(DreamBlob recipient, int targetCol, int targetRow)
        {
            if (!BrokenRingsTypeRules.FeatureEnabled || recipient == null ||
                !recipient.HasType(BrokenRingsTypeRules.Label))
                return 0;
            if (GridManager.Instance == null)
                return 0;
            if (!IsRosterComplete())
                return 0;

            var bonus = 0;
            foreach (var member in BrokenRingsTypeRules.MemberRoster)
            {
                if (member.closeRangeBonusDamage <= 0 || member.closeRangeSize <= 0)
                    continue;
                if (!GridManager.Instance.HasAliveBlobKind(member.kind))
                    continue;
                if (!GridManager.Instance.IsInBlobRange(recipient.Column, recipient.Lane, targetCol, targetRow,
                        member.closeRangeSize))
                    continue;

                bonus += member.closeRangeBonusDamage;
            }

            return DreamTeamSynergy.ScaleBondBonus(bonus, recipient);
        }

        public static float GetAttackSpeedIntervalMultiplier(DreamBlob recipient)
        {
            if (!BrokenRingsTypeRules.FeatureEnabled || recipient == null ||
                !recipient.HasType(BrokenRingsTypeRules.Label))
                return 1f;
            if (GridManager.Instance == null)
                return 1f;
            if (!IsRosterComplete())
                return 1f;

            var interval = 1f;
            foreach (var member in BrokenRingsTypeRules.MemberRoster)
            {
                if (member.attackSpeedIntervalMultiplier >= 0.999f || member.attackSpeedIntervalMultiplier <= 0f)
                    continue;
                if (!GridManager.Instance.HasAliveBlobKind(member.kind))
                    continue;
                interval *= member.attackSpeedIntervalMultiplier;
            }

            return interval;
        }

        public static void AppendBuffLines(DreamBlob recipient, List<string> lines)
        {
            if (!BrokenRingsTypeRules.FeatureEnabled || recipient == null ||
                !recipient.HasType(BrokenRingsTypeRules.Label))
                return;
            if (GridManager.Instance == null)
                return;
            if (!IsRosterComplete())
                return;

            foreach (var member in BrokenRingsTypeRules.MemberRoster)
            {
                if (!GridManager.Instance.HasAliveBlobKind(member.kind))
                    continue;

                if (member.bonusDamage > 0)
                    lines.Add($"Broken Rings {member.effectLine} +{member.bonusDamage} dmg");

                if (member.attackSpeedIntervalMultiplier < 0.999f && member.attackSpeedIntervalMultiplier > 0f)
                {
                    var mult = 1f / member.attackSpeedIntervalMultiplier;
                    lines.Add($"Broken Rings {member.effectLine} x{mult:F2}");
                }

                if (member.closeRangeBonusDamage > 0)
                    lines.Add(
                        $"Broken Rings {member.effectLine} +{member.closeRangeBonusDamage} dmg in {member.closeRangeSize}×{member.closeRangeSize}");
            }
        }
    }
}
