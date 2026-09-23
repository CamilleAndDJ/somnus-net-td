namespace SomnusNet.Core
{
    /// <summary>
    /// Shared ponder economy — story mode differs only via <see cref="StoryModeRules.StartingPonders"/>.
    /// </summary>
    public static class PonderEconomyRules
    {
        public const int KillReward = 5;
        public const int StandardStartingPonders = 100;
        public const int PassiveIncomeAmount = 1;
        public const float PassiveIncomeIntervalSeconds = 1f;
    }
}
