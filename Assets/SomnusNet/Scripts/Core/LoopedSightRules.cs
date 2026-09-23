using SomnusNet.Data;
using SomnusNet.Units;

namespace SomnusNet.Core
{
    public static class LoopedSightRules
    {
        public const string StatusLabel = "Looped Sight";

        public static bool HasNaturalLoopedSight(DreamBlob blob)
        {
            if (blob == null)
                return false;

            if (blob.Kind == BlobKind.Lantern)
                return true;

            if (!BlobTypeFeatures.Enabled)
                return false;

            return blob.HasType(AbsentHistoryTypeRules.Label);
        }
    }
}
