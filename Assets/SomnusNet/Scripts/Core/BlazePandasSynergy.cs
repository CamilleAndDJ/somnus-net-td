using SomnusNet.Data;
using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class BlazePandasSynergy
    {
        public const int NeighborBandSize = 3;
        public const int DamageBonusPerBlaze = 4;

        public static int GetNeighborDamageBonus(DreamBlob attacker)
        {
            if (!BlobTypeFeatures.Enabled || attacker == null || GridManager.Instance == null)
                return 0;

            var bonus = 0;
            GridManager.Instance.ForEachBlob(blob =>
            {
                if (blob == null || !blob.IsAlive || blob.Kind != BlobKind.Blaze || !blob.HasPandas)
                    return;
                if (GridManager.Instance.IsInBlobRange(blob.Column, blob.Lane, attacker.Column, attacker.Lane,
                        NeighborBandSize))
                    bonus += DamageBonusPerBlaze;
            });

            return bonus;
        }
    }
}
