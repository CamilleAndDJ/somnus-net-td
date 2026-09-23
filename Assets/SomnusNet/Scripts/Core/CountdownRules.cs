using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class CountdownRules
    {
        public const int BlastRangeSize = 3;
        public const int DangerBlastRangeSize = 5;
        public const int MaxMinesPerTile = 3;
        public const int DangerMaxMinesPerTile = 5;

        public static int GetMaxMinesPerTile(DreamBlob owner) =>
            owner != null && owner.IsAlive && owner.HasDanger ? DangerMaxMinesPerTile : MaxMinesPerTile;

        public static int GetBlastRangeSize(DreamBlob owner) =>
            owner != null && owner.IsAlive && owner.HasDanger ? DangerBlastRangeSize : BlastRangeSize;
    }
}
