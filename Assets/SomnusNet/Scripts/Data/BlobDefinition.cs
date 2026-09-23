using UnityEngine;

namespace SomnusNet.Data
{
    [CreateAssetMenu(fileName = "BlobDefinition", menuName = "Somnus Net/Blob Definition")]
    public class BlobDefinition : ScriptableObject
    {
        public BlobKind kind;
        public string displayName;
        public string typeLabel = "None";
        public string[] typeLabels = System.Array.Empty<string>();
        [TextArea] public string verbDescription;
        public int ponderCost;
        public Sprite sprite;

        [Header("Combat")]
        public int maxHealth;
        public int attackDamage;
        public float attackInterval = 1.1f;
        public int rangeSize = 5;

        [Header("Shooter")]
        public int projectileDamage = 20;
        public float fireInterval = 1.2f;
        public float projectileSpeed = 8f;
        public int shotsPerVolley = 1;
        public float slowDuration;
        public float slowMultiplier = 1f;

        [Header("Placement")]
        public bool canPlaceOnWater;
        public int novaDamage = 999;
        public float novaRadius = 1.8f;
        public float novaArmDelay = 0.4f;

        public bool HasType(string label)
        {
            if (typeLabels != null && typeLabels.Length > 0)
            {
                foreach (var entry in typeLabels)
                {
                    if (entry == label) return true;
                }

                return false;
            }

            return typeLabel == label;
        }

        public string TypeDisplay
        {
            get
            {
                if (typeLabels != null && typeLabels.Length > 0)
                    return string.Join(", ", typeLabels);
                return typeLabel;
            }
        }
    }
}
