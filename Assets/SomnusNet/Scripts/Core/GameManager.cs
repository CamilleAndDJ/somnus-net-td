using System;
using System.Collections;
using SomnusNet.UI;
using SomnusNet.Units;
using SomnusNet.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SomnusNet.Core
{
    public enum GamePhase { Playing, RoundIntermission, Paused, Won, Lost }

    public class GameManager : MonoBehaviour
    {
        public const int Level01DefeatGoal = 10;
        public const int ExtraGlitchesPerRound = 2;
        public const int DefaultTotalRounds = 2;
        public const float RoundEndBannerDuration = 5f;
        public const float RoundStartDelay = 5f;
        public const int DevSkipPonderBonus = 200;

        public static GameManager Instance { get; private set; }

        public GamePhase Phase { get; private set; } = GamePhase.Playing;
        public int CurrentRound { get; private set; } = 1;
        public int GlitchesDefeated { get; private set; }
        public int DefeatGoal { get; private set; } = Level01DefeatGoal;
        public bool UpgradesUnlocked { get; private set; }

        public bool CanOpenUpgradeMenu => UpgradesUnlocked;

        public bool CanPurchaseUpgrades => UpgradesUnlocked;

        public bool CanViewModifications => UpgradesUnlocked;

        public bool InfiniteRounds => LevelSettings.Instance != null && LevelSettings.Instance.infiniteRounds;
        public int TotalRounds => LevelSettings.Instance != null
            ? LevelSettings.Instance.totalRounds
            : DefaultTotalRounds;

        public event Action<int, int> OnDefeatCountChanged;
        public event Action<int> OnRoundComplete;
        public event Action<int> OnRoundStarted;
        public event Action OnLevelComplete;
        public event Action OnUpgradesUnlocked;

        Coroutine _completeRoundCoroutine;
        GamePhase _phaseBeforePause;

        public bool IsPaused => Phase == GamePhase.Paused;

        public bool IsHoldingLevelStart { get; private set; }

        public void HoldLevelStart()
        {
            IsHoldingLevelStart = true;
            Time.timeScale = 0f;
        }

        public void ReleaseLevelStart()
        {
            if (!IsHoldingLevelStart)
                return;

            IsHoldingLevelStart = false;
            if (Phase is GamePhase.Playing or GamePhase.RoundIntermission)
                Time.timeScale = 1f;
        }

        void Awake()
        {
            Instance = this;
            DefeatGoal = GetDefeatGoalForRound(CurrentRound);
            CountdownLandmineField.ClearAll();
        }

        void Start()
        {
            if (StoryModeRules.HasDialogs)
            {
                StoryModeIntroController.EnsureExists();
                StoryModeHarmonyTutorial.EnsureExists();
                StoryModeRoundTwoTutorial.EnsureExists();
                StoryModeSkyCompassRewardPopup.EnsureExists();
            }

            if (LevelSettings.Instance != null && LevelSettings.Instance.upgradesUnlockedFromStart)
                EnableUpgrades();

            DevRoundSkip.EnsureExists();
        }

        public void NotifyGlitchReachedCore()
        {
            if (Phase != GamePhase.Playing) return;
            Phase = GamePhase.Lost;
            Time.timeScale = 0f;
        }

        public void NotifyGlitchDefeated()
        {
            if (Phase != GamePhase.Playing) return;

            GlitchesDefeated++;

            if (GlitchesDefeated >= DefeatGoal)
            {
                if (InfiniteRounds || CurrentRound < TotalRounds)
                    _completeRoundCoroutine = StartCoroutine(CompleteRound());
                else if (StoryModeRules.Active)
                {
                    if (StoryModeRules.HasDialogs)
                        BeginStoryModeOutro();
                    else
                        CompleteLevel();
                }
                else
                    CompleteLevel();
            }

            OnDefeatCountChanged?.Invoke(GlitchesDefeated, DefeatGoal);
        }

        void CompleteLevel()
        {
            Phase = GamePhase.Won;
            Time.timeScale = 0f;
            WorldMapProgress.MarkCurrentSceneBeaten();
            OnLevelComplete?.Invoke();
        }

        void BeginStoryModeOutro()
        {
            ClearRemainingGlitches();

            if (StoryModeIntroController.Instance != null)
                StoryModeIntroController.Instance.BeginOutro(CompleteLevel);
            else
                CompleteLevel();
        }

        IEnumerator CompleteRound()
        {
            Phase = GamePhase.RoundIntermission;
            OnRoundComplete?.Invoke(CurrentRound);
            UnlockUpgradesIfNeeded();

            yield return new WaitForSeconds(RoundEndBannerDuration);

            ClearRemainingGlitches();
            DefeatGoal = GetDefeatGoalForRound(CurrentRound + 1);
            GlitchesDefeated = 0;
            OnDefeatCountChanged?.Invoke(GlitchesDefeated, DefeatGoal);

            yield return new WaitForSeconds(RoundStartDelay);

            StartNextRound();
            _completeRoundCoroutine = null;
        }

        void UnlockUpgradesIfNeeded()
        {
            if (UpgradesUnlocked || CurrentRound != 1) return;
            EnableUpgrades();
        }

        public void EnableUpgrades()
        {
            if (UpgradesUnlocked) return;
            UpgradesUnlocked = true;
            BlobUpgradePanel.EnsureExists();
            OnUpgradesUnlocked?.Invoke();
        }

        void StartNextRound()
        {
            CurrentRound++;
            DefeatGoal = GetDefeatGoalForRound(CurrentRound);
            Phase = GamePhase.Playing;
            OnDefeatCountChanged?.Invoke(GlitchesDefeated, DefeatGoal);
            OnRoundStarted?.Invoke(CurrentRound);
        }

        public static int GetExtraGlitchCountForRound(int round) =>
            (round - 1) * ExtraGlitchesPerRound;

        public static int GetDefeatGoalForRound(int round) =>
            Level01DefeatGoal + GetExtraGlitchCountForRound(round);

        /// <summary>Editor / development builds only — skips intermission or finishes the level.</summary>
        public void DevSkipRound()
        {
            if (!DevRoundSkip.IsEnabled) return;
            if (Phase == GamePhase.Won || Phase == GamePhase.Lost) return;

            if (_completeRoundCoroutine != null)
            {
                StopCoroutine(_completeRoundCoroutine);
                _completeRoundCoroutine = null;
            }

            if (InfiniteRounds || Phase == GamePhase.RoundIntermission || CurrentRound < TotalRounds)
            {
                ClearRemainingGlitches();
                UnlockUpgradesIfNeeded();
                if (LevelSettings.Instance != null && LevelSettings.Instance.upgradesUnlockedFromStart)
                    EnableUpgrades();
                GlitchesDefeated = 0;
                StartNextRound();
            }
            else if (CurrentRound >= TotalRounds)
            {
                GlitchesDefeated = DefeatGoal;
                OnDefeatCountChanged?.Invoke(GlitchesDefeated, DefeatGoal);
                CompleteLevel();
            }

            GrantDevSkipPonders();
        }

        static void GrantDevSkipPonders()
        {
            if (PonderEconomy.Instance != null)
                PonderEconomy.Instance.Add(DevSkipPonderBonus);
        }

        static void ClearRemainingGlitches() => GlitchRegistry.ClearRemaining();

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Pause()
        {
            if (Phase is GamePhase.Paused or GamePhase.Won or GamePhase.Lost)
                return;

            _phaseBeforePause = Phase;
            Phase = GamePhase.Paused;
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            if (Phase != GamePhase.Paused)
                return;

            Phase = _phaseBeforePause;
            Time.timeScale = Phase is GamePhase.Won or GamePhase.Lost || IsHoldingLevelStart ? 0f : 1f;
        }

        public void OpenWorldMap() => SceneLoader.LoadWorldMap();

        public void QuitToMenu() => SceneLoader.QuitToMenu();
    }
}
