using SomnusNet.Data;

using SomnusNet.Units;



namespace SomnusNet.Core

{

    public static class LanternRules

    {

        public const int PulseRangeSize = 3;

        public const float LoopedSightDurationSeconds = 4f;

        public const float StrifeDurationBonusSeconds = 2f;

        public const float FieldAttackSpeedIntervalMultiplier = 0.9f;

        public const int PulseBenchTrioBonusDamage = 4;

        public const int HeartGeneralPulseDamage = 2;

        public const int HeartExtraBenchTrioDamage = 3;

        public const int MindStrifeBonusLevels = 1;



        public static int GetPulseDamageForBlob(DreamBlob lantern, DreamBlob recipient)

        {

            if (lantern == null || recipient == null || recipient.Kind == BlobKind.Lantern)

                return 0;



            var damage = lantern.UpgradeBonusDamage;

            var benchTrio = ReceivesPulseBonus(recipient.Kind);



            if (lantern.HasHeart)

                damage += HeartGeneralPulseDamage;

            else if (!benchTrio)

                return 0;



            if (benchTrio)

            {

                if (lantern.HasHeart)

                    damage += HeartExtraBenchTrioDamage;



                if (GridManager.Instance != null &&

                    GridManager.Instance.AreAllRosterMembersOnField(BenchTrioTypeRules.MemberKinds))

                    damage += PulseBenchTrioBonusDamage;

            }



            return damage;

        }



        public static float GetFieldAttackSpeedMultiplier() =>

            1f / FieldAttackSpeedIntervalMultiplier;



        public static bool ReceivesPulseBonus(BlobKind kind) =>

            kind == BlobKind.Countdown || kind == BlobKind.Troublemaker;



        public static bool AttacksGlitches(DreamBlob blob) =>

            blob != null && blob.Kind != BlobKind.Lantern;



        public static bool CanReceivePulseBuff(DreamBlob lantern, DreamBlob recipient)

        {

            if (recipient == null || !recipient.IsAlive)

                return false;



            return recipient == lantern || recipient.Kind != BlobKind.Lantern;

        }

    }

}


