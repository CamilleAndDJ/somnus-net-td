using System.Collections.Generic;
using SomnusNet.Data;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Core
{
    public static class DreamTeamSynergy
    {
        static bool IsRosterComplete() =>
            GridManager.Instance != null &&
            GridManager.Instance.AreAllRosterMembersOnField(DreamTeamTypeRules.MemberKinds);

        public static bool IsCreatorMemberOnField()
        {
            if (!BlobTypeFeatures.Enabled || GridManager.Instance == null)
                return false;
            return GridManager.Instance.HasAliveBlobKind(BlobKind.Creator);
        }

        public static float GetBondBuffMultiplier(DreamBlob recipient)
        {
            if (!BlobTypeFeatures.Enabled || recipient == null || !recipient.HasType(DreamTeamTypeRules.Label))
                return 1f;
            if (GridManager.Instance == null)
                return 1f;
            if (!IsRosterComplete())
                return 1f;

            var mult = 1f;
            foreach (var member in DreamTeamTypeRules.MemberRoster)
            {
                if (member.bondBuffMultiplier <= 1.001f)
                    continue;
                if (GridManager.Instance.HasAliveBlobKind(member.kind))
                    mult *= member.bondBuffMultiplier;
            }

            return mult;
        }

        public static float ScaleBondMultiplier(float multiplier, DreamBlob recipient)
        {
            if (multiplier <= 1.001f)
                return multiplier;

            var bond = GetBondBuffMultiplier(recipient);
            if (bond <= 1.001f)
                return multiplier;

            return 1f + (multiplier - 1f) * bond;
        }

        public static int ScaleBondBonus(int bonus, DreamBlob recipient)
        {
            if (bonus <= 0)
                return 0;

            var bond = GetBondBuffMultiplier(recipient);
            if (bond <= 1.001f)
                return bonus;

            return Mathf.Max(0, Mathf.RoundToInt(bonus * bond));
        }

        public static float GetAttackSpeedIntervalMultiplier(DreamBlob recipient)
        {
            if (!BlobTypeFeatures.Enabled || recipient == null || !recipient.HasType(DreamTeamTypeRules.Label))
                return 1f;
            if (!IsRosterComplete())
                return 1f;

            var interval = DreamTeamTypeRules.CreatorMemberAttackSpeedIntervalMultiplier;
            var bond = GetBondBuffMultiplier(recipient);
            if (bond <= 1.001f)
                return interval;
            return 1f - (1f - interval) * bond;
        }

        public static float GetAttackSpeedMultiplier(DreamBlob recipient)
        {
            var interval = GetAttackSpeedIntervalMultiplier(recipient);
            return interval >= 1f ? 1f : 1f / interval;
        }

        public static int GetCloseRangeBonusDamage(DreamBlob recipient, int targetCol, int targetRow)
        {
            if (!BlobTypeFeatures.Enabled || recipient == null || !recipient.HasType(DreamTeamTypeRules.Label))
                return 0;
            if (GridManager.Instance == null)
                return 0;
            if (!IsRosterComplete())
                return 0;

            var bonus = 0;
            foreach (var member in DreamTeamTypeRules.MemberRoster)
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

            return ScaleBondBonus(bonus, recipient);
        }

        public static int GetBonusDamage(DreamBlob recipient)
        {
            if (!BlobTypeFeatures.Enabled || recipient == null || !recipient.HasType(DreamTeamTypeRules.Label))
                return 0;
            if (GridManager.Instance == null)
                return 0;
            if (!IsRosterComplete())
                return 0;

            var bonus = 0;
            foreach (var member in DreamTeamTypeRules.MemberRoster)
            {
                if (member.kind == BlobKind.Creator || member.bonusDamage <= 0)
                    continue;
                if (GridManager.Instance.HasAliveBlobKind(member.kind))
                    bonus += member.bonusDamage;
            }

            return bonus;
        }

        public static float GetBonusDamageMultiplier(DreamBlob recipient)
        {
            if (!BlobTypeFeatures.Enabled || recipient == null || !recipient.HasType(DreamTeamTypeRules.Label))
                return 1f;

            var bonus = GetBonusDamage(recipient);
            if (bonus <= 0) return 1f;

            var baseDamage = recipient.DefinitionBaseDamage + recipient.UpgradeBonusDamage;
            if (baseDamage <= 0) return 1f;
            return (baseDamage + bonus) / (float)baseDamage;
        }

        public static void AppendUpgradeBuffLines(DreamBlob recipient, List<string> lines)
        {
            if (!BlobTypeFeatures.Enabled || recipient == null || !recipient.HasType(DreamTeamTypeRules.Label))
                return;
            if (GridManager.Instance == null)
                return;
            if (!IsRosterComplete())
                return;

            foreach (var member in DreamTeamTypeRules.MemberRoster)
            {
                if (!GridManager.Instance.HasAliveBlobKind(member.kind))
                    continue;

                if (member.attackSpeedIntervalMultiplier < 0.999f)
                {
                    var mult = 1f / member.attackSpeedIntervalMultiplier;
                    lines.Add($"Dream Team {member.speedBuffLabel} x{mult:F2}");
                }

                if (member.bonusDamage > 0)
                {
                    var mult = recipient.DefinitionBaseDamage + recipient.UpgradeBonusDamage > 0
                        ? (recipient.DefinitionBaseDamage + recipient.UpgradeBonusDamage + member.bonusDamage)
                          / (float)(recipient.DefinitionBaseDamage + recipient.UpgradeBonusDamage)
                        : 1f;
                    lines.Add($"Dream Team {member.damageBuffLabel} +{member.bonusDamage} dmg (x{mult:F2})");
                }
                if (member.bondBuffMultiplier > 1.001f)
                    lines.Add($"Dream Team {member.damageBuffLabel} x{member.bondBuffMultiplier:F2}");

                if (member.closeRangeBonusDamage > 0)
                    lines.Add($"Dream Team {member.damageBuffLabel} +{member.closeRangeBonusDamage} dmg in {member.closeRangeSize}×{member.closeRangeSize}");
            }
        }
    }
}
