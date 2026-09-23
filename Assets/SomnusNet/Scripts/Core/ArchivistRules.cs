using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Core
{
    public static class ArchivistRules
    {
        public const float FreezeChance = 0.35f;
        public const float FreezeDuration = 1f;
        public const float RewindProjectileSpeedMultiplier = 0.48f;
        public const float RewindFireIntervalMultiplier = 1.85f;
        public const float PushBackwardDistance = 0.42f;
        public const float ResumeIntervalMultiplier = 0.82f;
        public const int ResumeShotsBeforeBuff = 2;
        public const int ResumeBuffDamage = 4;
        public const float ResumeBuffDuration = 3.5f;
        public const int BrokenRingsCloseRangeSize = 3;
        public const int BrokenRingsCloseRangeBonusDamage = 5;
    }
}
