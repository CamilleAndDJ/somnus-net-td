namespace SomnusNet.Core
{
    public static class LevelLayoutSession
    {
        public static LevelLayoutId Selected { get; private set; } = LevelLayoutId.Begining;
        public static bool IsReady { get; private set; }

        public static bool RequiresSelection =>
            LevelSettings.Instance != null && !LevelSettings.Instance.storyMode;

        public static bool IsLayoutMaker => IsReady && Selected == LevelLayoutId.LayoutMaker;

        public static void ApplyForStoryMode(int variant = 1)
        {
            Selected = variant == 2 ? LevelLayoutId.OuterRing : LevelLayoutId.Begining;
            IsReady = true;
        }

        public static void BeginAllFeaturesLoad()
        {
            IsReady = false;
        }

        public static void Select(LevelLayoutId layout)
        {
            Selected = layout;
            IsReady = true;
        }
    }
}
