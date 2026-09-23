using SomnusNet.Core;
using SomnusNet.Units;
using SomnusNet.UI;
using UnityEngine;

namespace SomnusNet.Visual
{
    public static class BondVisualFeedback
    {
        static bool _initialized;
        static int _harmonyCount;
        static int _speedCount;
        static int _dreamTeamCount;
        static int _absentHistoryCount;
        static int _brokenRingsCount;
        static int _slimySupportCount;
        static int _entertainerCount;
        static int _benchTrioCount;
        static int _burdenedCrownCount;

        public static void ResetBaseline()
        {
            RefreshStoredCounts();
            _initialized = true;
        }

        /// <summary>Call after a blob leaves the field so placement can re-trigger bond VFX.</summary>
        public static void SyncCountsFromField()
        {
            if (!_initialized)
                return;

            RefreshStoredCounts();
        }

        public static void NotifyBlobPlaced(DreamBlob blob)
        {
            if (!BlobTypeFeatures.Enabled || blob == null)
                return;

            if (GridManager.Instance == null)
                return;

            if (!_initialized)
                ResetBaseline();

            var runner = BondVisualFeedbackRunner.Ensure();
            if (runner == null)
                return;

            CaptureCounts(out var harmony, out var speed, out var dreamTeam, out var absentHistory,
                out var brokenRings, out var slimySupport, out var entertainer, out var benchTrio,
                out var burdenedCrown);

            if (blob.HasType(HarmonyTypeRules.HarmonyLabel))
            {
                MarkVisible(TypeHudLayout.Kind.Harmony, harmony);
                HandleGeneralBond(harmony, _harmonyCount, runner, TypeHudLayout.Kind.Harmony);
            }

            if (blob.HasType(SpeedTypeRules.Label))
            {
                MarkVisible(TypeHudLayout.Kind.Speed, speed);
                HandleGeneralBond(speed, _speedCount, runner, TypeHudLayout.Kind.Speed);
            }

            if (blob.HasType(AbsentHistoryTypeRules.Label))
            {
                MarkVisible(TypeHudLayout.Kind.AbsentHistory, absentHistory);
                HandleGeneralBond(absentHistory, _absentHistoryCount, runner, TypeHudLayout.Kind.AbsentHistory);
            }

            if (blob.HasType(EntertainerTypeRules.Label))
            {
                MarkVisible(TypeHudLayout.Kind.Entertainer, entertainer);
                HandleGeneralBond(entertainer, _entertainerCount, runner, TypeHudLayout.Kind.Entertainer);
            }

            if (blob.HasType(DreamTeamTypeRules.Label))
            {
                MarkVisible(TypeHudLayout.Kind.DreamTeam, dreamTeam);
                HandleGroupBond(dreamTeam, _dreamTeamCount, runner, TypeHudLayout.Kind.DreamTeam,
                    DreamTeamTypeRules.MemberKinds.Length);
            }

            if (blob.HasType(BrokenRingsTypeRules.Label))
            {
                MarkVisible(TypeHudLayout.Kind.BrokenRings, brokenRings);
                HandleGroupBond(brokenRings, _brokenRingsCount, runner, TypeHudLayout.Kind.BrokenRings,
                    BrokenRingsTypeRules.MemberKinds.Length);
            }

            if (blob.HasType(SlimySupportTypeRules.Label))
            {
                MarkVisible(TypeHudLayout.Kind.SlimySupport, slimySupport);
                HandleGroupBond(slimySupport, _slimySupportCount, runner, TypeHudLayout.Kind.SlimySupport,
                    SlimySupportTypeRules.MemberKinds.Length);
            }

            if (blob.HasType(BenchTrioTypeRules.Label))
            {
                MarkVisible(TypeHudLayout.Kind.BenchTrio, benchTrio);
                HandleGroupBond(benchTrio, _benchTrioCount, runner, TypeHudLayout.Kind.BenchTrio,
                    BenchTrioTypeRules.MemberKinds.Length);
            }

            if (blob.HasType(BurdenedCrownTypeRules.Label))
            {
                MarkVisible(TypeHudLayout.Kind.BurdenedCrown, burdenedCrown);
                HandleGeneralBond(burdenedCrown, _burdenedCrownCount, runner, TypeHudLayout.Kind.BurdenedCrown);
            }

            _harmonyCount = harmony;
            _speedCount = speed;
            _dreamTeamCount = dreamTeam;
            _absentHistoryCount = absentHistory;
            _brokenRingsCount = brokenRings;
            _slimySupportCount = slimySupport;
            _entertainerCount = entertainer;
            _benchTrioCount = benchTrio;
            _burdenedCrownCount = burdenedCrown;
        }

        static void MarkVisible(TypeHudLayout.Kind kind, int count)
        {
            if (count > 0)
                TypeHudLayout.SetVisible(kind, true);
        }

        static void RefreshStoredCounts() =>
            CaptureCounts(out _harmonyCount, out _speedCount, out _dreamTeamCount, out _absentHistoryCount,
                out _brokenRingsCount, out _slimySupportCount, out _entertainerCount, out _benchTrioCount,
                out _burdenedCrownCount);

        static void CaptureCounts(out int harmony, out int speed, out int dreamTeam, out int absentHistory,
            out int brokenRings, out int slimySupport, out int entertainer, out int benchTrio,
            out int burdenedCrown)
        {
            var grid = GridManager.Instance;
            if (grid == null)
            {
                harmony = speed = dreamTeam = absentHistory = brokenRings = slimySupport = entertainer = 0;
                benchTrio = burdenedCrown = 0;
                return;
            }

            harmony = grid.CountUniqueHarmonyTypesOnField();
            speed = grid.CountUniqueKindsWithType(SpeedTypeRules.Label);
            dreamTeam = grid.CountRosterMembersOnField(DreamTeamTypeRules.MemberKinds);
            absentHistory = grid.CountUniqueKindsWithType(AbsentHistoryTypeRules.Label);
            brokenRings = grid.CountRosterMembersOnField(BrokenRingsTypeRules.MemberKinds);
            slimySupport = grid.CountRosterMembersOnField(SlimySupportTypeRules.MemberKinds);
            entertainer = grid.CountUniqueKindsWithType(EntertainerTypeRules.Label);
            benchTrio = grid.CountRosterMembersOnField(BenchTrioTypeRules.MemberKinds);
            burdenedCrown = grid.CountUniqueKindsWithType(BurdenedCrownTypeRules.Label);
        }

        static void HandleGeneralBond(int next, int prev, BondVisualFeedbackRunner runner, TypeHudLayout.Kind kind)
        {
            if (next <= prev)
                return;

            runner.QueueSwirl(kind);
        }

        static void HandleGroupBond(int next, int prev, BondVisualFeedbackRunner runner, TypeHudLayout.Kind kind,
            int rosterSize)
        {
            if (next <= prev)
                return;

            if (next >= rosterSize && prev < rosterSize)
                runner.QueueComplete(kind);
            else
                runner.QueueSwirl(kind);
        }
    }
}
