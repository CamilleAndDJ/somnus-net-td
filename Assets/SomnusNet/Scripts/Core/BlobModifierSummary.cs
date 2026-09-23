using System.Collections.Generic;
using SomnusNet.Data;
using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class BlobModifierSummary
    {
        public static int GetPermanentLuckLevel(DreamBlob blob) =>
            blob == null ? 0 : blob.LuckLevel - blob.CheerfulHugLuckBonus;

        public static int GetPermanentFlatDamageBonus(DreamBlob blob)
        {
            if (blob == null)
                return 0;

            return blob.HarmonyBonusDamage + blob.DreamTeamSynergyBonusDamage + blob.BrokenRingsBonusDamage +
                   blob.SlimySupportBonusDamage + blob.EntertainerBonusDamage +
                   BlazePandasSynergy.GetNeighborDamageBonus(blob);
        }

        public static float GetPermanentAttackSpeedMultiplier(DreamBlob blob) =>
            blob == null ? 1f : blob.PermanentAttackSpeedMultiplier;

        public static float GetPermanentAttackDamageMultiplier(DreamBlob blob)
        {
            if (blob == null || blob.DefinitionBaseDamage <= 0)
                return 1f;

            var baseDmg = blob.DefinitionBaseDamage;
            var total = baseDmg + blob.UpgradeBonusDamage + GetPermanentFlatDamageBonus(blob);
            if (total <= baseDmg)
                return 1f;

            return total / (float)baseDmg;
        }

        public static void AppendSummaryLines(DreamBlob blob, List<string> lines)
        {
            if (blob == null || lines == null)
                return;

            var luck = GetPermanentLuckLevel(blob);
            if (luck > 0)
                lines.Add(LuckRules.FormatLevels(luck));

            var strife = blob.StrifeLevel;
            if (strife > 0)
                lines.Add(StrifeRules.FormatLevels(strife));

            var speed = GetPermanentAttackSpeedMultiplier(blob);
            if (speed > 1.001f)
                lines.Add($"Atk Speed x{speed:F2}");

            var damageMult = GetPermanentAttackDamageMultiplier(blob);
            if (damageMult > 1.001f)
                lines.Add($"Atk Damage x{damageMult:F2}");
        }

        public static void AppendTemporaryBuffLines(DreamBlob blob, List<string> lines)
        {
            if (blob == null || lines == null)
                return;

            if (blob.CheerfulFriendBoostDamage > 0)
                lines.Add($"Cheerful cheer +{blob.CheerfulFriendBoostDamage} dmg");

            if (blob.LanternPulseBonusDamage > 0)
                lines.Add($"Lantern pulse +{blob.LanternPulseBonusDamage} dmg");

            if (blob.LanternPulseStrifeBonus > 0)
                lines.Add($"Lantern pulse {StrifeRules.FormatLevels(blob.LanternPulseStrifeBonus)}");

            if (blob.ResumeBoostDamage > 0)
                lines.Add($"Resume +{blob.ResumeBoostDamage} dmg");

            if (blob.HasLoopedSight && !blob.HasType(AbsentHistoryTypeRules.Label))
                lines.Add(LoopedSightRules.StatusLabel);

            if (blob.Kind == BlobKind.Creator && blob.CreatorRampBonusDamage > 0)
                lines.Add($"Focus +{blob.CreatorRampBonusDamage} dmg");
        }
    }
}
