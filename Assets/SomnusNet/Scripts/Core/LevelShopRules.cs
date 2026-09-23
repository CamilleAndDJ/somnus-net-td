using SomnusNet.Data;

namespace SomnusNet.Core
{
    public static class LevelShopRules
    {
        public static bool UsesLimitedShop => StoryModeRules.Active;

        public static bool IsBlobUnlocked(BlobKind kind, int round)
        {
            kind = BlobKinds.Normalize(kind);

            if (StoryModeRules.Active)
            {
                foreach (var allowed in StoryModeRules.ShopBlobOrder)
                {
                    if (kind == allowed)
                        return true;
                }

                return false;
            }

            return true;
        }
    }
}
