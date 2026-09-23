using SomnusNet.Data;
using UnityEngine;

namespace SomnusNet.Core
{
    public static class GlitchRuntimeDefaults
    {
        public static GlitchDefinition TryCreate(GlitchKind kind)
        {
            if (kind != GlitchKind.CicadianRhythm)
                return null;

            var def = ScriptableObject.CreateInstance<GlitchDefinition>();
            def.kind = GlitchKind.CicadianRhythm;
            def.displayName = "Cicadian Rhythm";
            def.fantasyNote = "Round boss — looped rhythm that warps damage and surges forward";
            def.maxHealth = 880;
            def.moveSpeed = 0.3f;
            def.visualScaleMultiplier = CicadianRhythmRules.DefaultVisualScaleMultiplier;
            def.ponderReward = PonderEconomyRules.KillReward;
            def.biteDamage = 12;
            def.biteInterval = 0.9f;
            return def;
        }
    }
}
