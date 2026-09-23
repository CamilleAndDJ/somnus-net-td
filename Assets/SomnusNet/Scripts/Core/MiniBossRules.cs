using SomnusNet.Data;
using UnityEngine;

namespace SomnusNet.Core
{
    public static class MiniBossRules
    {
        public const float DefaultVisualScaleMultiplier = 1.22f;
        public const float FazeDodgeChance = 0.35f;

        public static bool IsMiniBoss(GlitchKind kind) =>
            kind is GlitchKind.Overload or GlitchKind.Faze or GlitchKind.Sink;

        public static GlitchKind PickRandom()
        {
            var roll = Random.Range(0, 3);
            return roll switch
            {
                0 => GlitchKind.Overload,
                1 => GlitchKind.Faze,
                _ => GlitchKind.Sink
            };
        }
    }
}
