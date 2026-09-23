using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Core
{
    public static class CheerfulFriendBoostRules
    {
        public const float BuffDurationSeconds = 3.5f;
        public const int BaseBuffPerHit = 3;
        public const float ViscousBounceChance = 0.45f;
        public const int HugsLuckLevelsPerHit = 1;

        public static int ComputeBuffAmount(DreamBlob slime)
        {
            if (slime == null)
                return BaseBuffPerHit;

            var amount = BaseBuffPerHit * slime.AttackDamageBuffMultiplier * slime.AttackSpeedBuffMultiplier;
            return Mathf.Max(1, Mathf.RoundToInt(amount));
        }
    }
}
