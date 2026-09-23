using SomnusNet.Data;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Core
{
    public static class DealerSecondSynergy
    {
        public const float SelfSpeedIntervalMultiplier = 0.85f;

        static readonly (int dc, int dr)[] AdjacentOffsets =
        {
            (-1, 0), (1, 0), (0, -1), (0, 1),
            (-1, -1), (1, -1), (-1, 1), (1, 1)
        };

        public static float GetSecondIntervalMultiplier(DreamBlob dealer, float dealerBaseInterval)
        {
            if (dealer == null || !dealer.HasSecond || dealerBaseInterval <= 0f)
                return 1f;

            if (!TryFindFastestAdjacent(dealer, dealerBaseInterval, out var fastestInterval, out _))
                return SelfSpeedIntervalMultiplier;

            return fastestInterval / dealerBaseInterval;
        }

        public static DreamBlob GetCopiedBlob(DreamBlob dealer)
        {
            if (dealer == null || !dealer.HasSecond)
                return null;

            var dealerInterval = dealer.ComputeFireIntervalExcludingSecond();
            TryFindFastestAdjacent(dealer, dealerInterval, out _, out var copied);
            return copied;
        }

        public static bool IsSelfBuffing(DreamBlob dealer) =>
            dealer != null && dealer.HasSecond && GetCopiedBlob(dealer) == null;

        static bool TryFindFastestAdjacent(DreamBlob dealer, float dealerInterval, out float fastestInterval,
            out DreamBlob copied)
        {
            fastestInterval = float.MaxValue;
            copied = null;

            var grid = GridManager.Instance;
            if (grid == null)
                return false;

            foreach (var (dc, dr) in AdjacentOffsets)
            {
                var neighbor = grid.Get(dealer.Column + dc, dealer.Lane + dr);
                if (neighbor == null || !neighbor.IsAlive || neighbor == dealer)
                    continue;

                var neighborInterval = neighbor.ComputeFireIntervalExcludingSecond();
                if (neighborInterval >= dealerInterval || neighborInterval >= fastestInterval)
                    continue;

                fastestInterval = neighborInterval;
                copied = neighbor;
            }

            return copied != null;
        }
    }
}
