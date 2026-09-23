using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.Data
{
    [CreateAssetMenu(fileName = "GlitchDefinition", menuName = "Somnus Net/Glitch Definition")]
    public class GlitchDefinition : ScriptableObject
    {
        public GlitchKind kind;
        public string displayName;
        [TextArea] public string fantasyNote;
        public Sprite sprite;
        public Sprite shellOverlaySprite;

        public int maxHealth = 100;
        public int shellHealth;
        public float moveSpeed = 0.35f;
        public int biteDamage = 12;
        public float biteInterval = 0.9f;
        public int ponderReward = PonderEconomyRules.KillReward;
        public float visualScaleMultiplier = 1f;
    }
}
