using SomnusNet.Data;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Core
{
    public static class CicadianRhythmRules
    {
        public const int SpawnRound = 10;
        public const float DefaultVisualScaleMultiplier = 1.32f;
        public const float ReducedDamageMultiplier = 0.5f;
        public const float DashInterval = 4.8f;
        public const float DashDuration = 0.42f;
        public const float DashSpeedMultiplier = 3.6f;
        public const float InitialDashDelay = 2.2f;

        public static bool IsBoss(GlitchKind kind) => kind == GlitchKind.CicadianRhythm;

        public static bool IsBoss(Glitch glitch) => glitch != null && IsBoss(glitch.Kind);

        public static bool CanDealFullLoopedDamage(DreamBlob attacker)
        {
            if (!BlobTypeFeatures.Enabled || attacker == null)
                return false;

            return attacker.HasLoopedSight;
        }

        public static int ScaleIncomingDamage(int amount, DreamBlob attacker)
        {
            if (amount <= 0)
                return 0;

            var multiplier = CanDealFullLoopedDamage(attacker) ? 1f : ReducedDamageMultiplier;
            return Mathf.Max(1, Mathf.RoundToInt(amount * multiplier));
        }
    }
}
