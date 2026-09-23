using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Core
{
    public static class ArchivistResumeRules
    {
        public const float BuffDurationSeconds = ArchivistRules.ResumeBuffDuration;
        public const int BaseBuffPerHit = ArchivistRules.ResumeBuffDamage;

        public static int ComputeBuffAmount(DreamBlob archivist)
        {
            if (archivist == null)
                return BaseBuffPerHit;

            var amount = BaseBuffPerHit * archivist.AttackDamageBuffMultiplier;
            return Mathf.Max(1, Mathf.RoundToInt(amount));
        }
    }
}
