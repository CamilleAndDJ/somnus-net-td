using System.Collections.Generic;
using SomnusNet.Data;
using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class ArchivistResumeSynergy
    {
        static readonly (int dc, int dr)[] ClockwiseOffsets =
        {
            (0, 1),
            (1, 1),
            (1, 0),
            (1, -1),
            (0, -1),
            (-1, -1),
            (-1, 0),
            (-1, 1)
        };

        public static DreamBlob SelectTarget(DreamBlob archivist)
        {
            if (archivist == null || !archivist.IsAlive || archivist.Kind != BlobKind.Archivist ||
                GridManager.Instance == null)
                return null;

            var adjacent = CollectAdjacent(archivist);
            if (adjacent.Count == 0)
                return null;

            var blazes = FilterKind(adjacent, BlobKind.Blaze);
            if (blazes.Count > 0)
                return PickClockwiseFirst(archivist, blazes);

            var dealers = FilterKind(adjacent, BlobKind.Dealer);
            if (dealers.Count > 0)
                return PickClockwiseFirst(archivist, dealers);

            return PickClockwiseFirst(archivist, adjacent);
        }

        static List<DreamBlob> CollectAdjacent(DreamBlob archivist)
        {
            var list = new List<DreamBlob>();
            GridManager.Instance.ForEachAdjacentBlob(archivist.Column, archivist.Lane, blob =>
            {
                if (blob != null && blob.IsAlive && blob != archivist && blob.Kind != BlobKind.Cheerful)
                    list.Add(blob);
            });
            return list;
        }

        static List<DreamBlob> FilterKind(List<DreamBlob> blobs, BlobKind kind)
        {
            var list = new List<DreamBlob>();
            foreach (var blob in blobs)
            {
                if (blob.Kind == kind)
                    list.Add(blob);
            }

            return list;
        }

        static DreamBlob PickClockwiseFirst(DreamBlob archivist, List<DreamBlob> candidates)
        {
            var grid = GridManager.Instance;
            if (grid == null || candidates == null || candidates.Count == 0)
                return null;

            var col = archivist.Column;
            var lane = archivist.Lane;
            foreach (var offset in ClockwiseOffsets)
            {
                var blob = grid.Get(col + offset.dc, lane + offset.dr);
                if (blob != null && blob.IsAlive && candidates.Contains(blob))
                    return blob;
            }

            return null;
        }
    }
}
