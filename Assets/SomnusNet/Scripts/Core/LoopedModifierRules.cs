using SomnusNet.Data;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Core
{
    public static class LoopedModifierRules
    {
        public const int UnlockRound = 10;
        public const float SpawnChance = 0.12f;
        public const string Label = "Looped";

        public static bool IsUnlocked(int round) => round >= UnlockRound;

        public static bool ShouldApplyToSpawn(int round, GlitchKind kind)
        {
            if (!BlobTypeFeatures.Enabled || !IsUnlocked(round) || MiniBossRules.IsMiniBoss(kind)
                || CicadianRhythmRules.IsBoss(kind))
                return false;

            return Random.value < SpawnChance;
        }

        public static bool CanBlobTarget(Glitch glitch, DreamBlob attacker)
        {
            if (glitch == null || !glitch.IsTargetable)
                return false;

            if (attacker != null && !LanternRules.AttacksGlitches(attacker))
                return false;

            if (CicadianRhythmRules.IsBoss(glitch))
                return true;

            if (!glitch.IsLooped)
                return true;

            if (!BlobTypeFeatures.Enabled)
                return false;

            return attacker != null && attacker.HasLoopedSight;
        }

        /// <summary>Split shards, magma defense, and other splash/AoE respect Looped Sight.</summary>
        public static bool CanAreaEffectDamage(Glitch glitch, DreamBlob attacker)
        {
            if (glitch == null || !glitch.IsTargetable)
                return false;

            if (attacker != null && !LanternRules.AttacksGlitches(attacker))
                return false;

            if (CicadianRhythmRules.IsBoss(glitch))
                return true;

            if (glitch.IsLooped)
                return attacker != null && attacker.HasLoopedSight;

            return CanBlobTarget(glitch, attacker);
        }

        /// <summary>Looped glitches pass over landmines without setting them off.</summary>
        public static bool CanTriggerLandmines(Glitch glitch)
        {
            if (glitch == null || !glitch.IsTargetable)
                return false;

            if (CicadianRhythmRules.IsBoss(glitch))
                return true;

            return !glitch.IsLooped;
        }
    }
}
