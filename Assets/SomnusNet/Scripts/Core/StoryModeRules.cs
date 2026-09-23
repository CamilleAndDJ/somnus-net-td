using SomnusNet.Data;

namespace SomnusNet.Core
{
    public static class StoryModeRules
    {
        public const int StoryMode1TotalRounds = 2;
        public const int StoryMode2TotalRounds = 4;
        public const int StartingPonders = 50;

        public static readonly BlobKind[] StoryMode1ShopBlobOrder = { BlobKind.Gatekeeper };
        public static readonly BlobKind[] StoryMode2ShopBlobOrder =
        {
            BlobKind.Gatekeeper,
            BlobKind.Creator,
            BlobKind.FourOhFour,
            BlobKind.Blaze
        };

        public static bool Active =>
            LevelSettings.Instance != null && LevelSettings.Instance.storyMode;

        public static bool HasDialogs =>
            Active && LevelSettings.Instance.enableStoryDialogs;

        public static int Variant =>
            Active ? LevelSettings.Instance.storyModeVariant : 0;

        public static int TotalRounds => Variant switch
        {
            2 => StoryMode2TotalRounds,
            _ => StoryMode1TotalRounds
        };

        public static BlobKind[] ShopBlobOrder => Variant switch
        {
            2 => StoryMode2ShopBlobOrder,
            _ => StoryMode1ShopBlobOrder
        };
    }
}
