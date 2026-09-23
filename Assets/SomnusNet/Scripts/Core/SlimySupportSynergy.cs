using System.Collections.Generic;
using SomnusNet.Data;
using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class SlimySupportSynergy
    {
        static bool IsRosterComplete() =>
            GridManager.Instance != null &&
            GridManager.Instance.AreAllRosterMembersOnField(SlimySupportTypeRules.MemberKinds);

        public static int GetLuckLevel(DreamBlob recipient)
        {
            if (!SlimySupportTypeRules.FeatureEnabled || recipient == null ||
                !recipient.HasType(SlimySupportTypeRules.Label))
                return 0;

            var luck = GetUpgradeLuckLevel(recipient);
            if (GridManager.Instance == null || !IsRosterComplete())
                return luck;

            foreach (var member in SlimySupportTypeRules.MemberRoster)
            {
                if (member.luckBonusLevels <= 0)
                    continue;
                if (GridManager.Instance.HasAliveBlobKind(member.kind))
                    luck += member.luckBonusLevels;
            }

            return luck;
        }

        static int GetUpgradeLuckLevel(DreamBlob recipient)
        {
            if (recipient == null)
                return 0;

            if (recipient.Kind == BlobKind.Dealer && recipient.HasHeartEater)
                return SlimySupportTypeRules.HeartEaterLuckBonus;

            return 0;
        }

        public static int GetBonusDamage(DreamBlob recipient)
        {
            if (!SlimySupportTypeRules.FeatureEnabled || recipient == null ||
                !recipient.HasType(SlimySupportTypeRules.Label))
                return 0;
            if (GridManager.Instance == null)
                return 0;
            if (!IsRosterComplete())
                return 0;

            var bonus = 0;
            foreach (var member in SlimySupportTypeRules.MemberRoster)
            {
                if (member.bonusDamage <= 0)
                    continue;
                if (GridManager.Instance.HasAliveBlobKind(member.kind))
                    bonus += member.bonusDamage;
            }

            return bonus;
        }

        public static void AppendBuffLines(DreamBlob recipient, List<string> lines)
        {
            if (!SlimySupportTypeRules.FeatureEnabled || recipient == null ||
                !recipient.HasType(SlimySupportTypeRules.Label))
                return;
            if (GridManager.Instance == null)
                return;

            var damage = GetBonusDamage(recipient);
            if (damage > 0)
                lines.Add($"Slimy Support +{damage} attack damage");
        }
    }
}
