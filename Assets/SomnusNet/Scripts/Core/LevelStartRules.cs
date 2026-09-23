namespace SomnusNet.Core
{
    public static class LevelStartRules
    {
        public static bool ShouldWaitForFirstBlobPlacement()
        {
            if (!StoryModeRules.HasDialogs)
                return false;

            var round = GameManager.Instance != null ? GameManager.Instance.CurrentRound : 1;
            return round == 1;
        }

        public static bool ShouldPausePassivePonderIncome()
        {
            if (!ShouldWaitForFirstBlobPlacement())
                return false;

            return GridManager.Instance == null || !GridManager.Instance.HasAnyBlobOnField();
        }
    }
}
