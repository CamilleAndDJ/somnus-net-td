using UnityEngine;
using UnityEngine.SceneManagement;

namespace SomnusNet.Core
{
    public static class WorldMapProgress
    {
        const string PrefsPrefix = "WorldMapLevel_";
        const string PhantasiaAlertKey = "WorldMapPhantasiaAlert";

        static bool _pendingStargazerCelebration;

        public enum Continent
        {
            DreamIsles,
            Stargazer
        }

        public enum LevelId
        {
            Stargazer_0,
            DreamIsles_1
        }

        public static readonly Color UnlockedFill = new(0.92f, 0.32f, 0.32f, 1f);
        public static readonly Color UnlockedRing = new(0.72f, 0.22f, 0.22f, 1f);
        public static readonly Color LevelBeatenFill = new(0.55f, 0.85f, 1f, 1f);
        public static readonly Color LevelBeatenRing = new(0.35f, 0.65f, 0.92f, 1f);
        public static readonly Color ContinentCompleteFill = new(0.62f, 0.95f, 0.58f, 1f);
        public static readonly Color ContinentCompleteRing = new(0.42f, 0.78f, 0.38f, 1f);

        static readonly LevelId[] DreamIslesLevels = { LevelId.DreamIsles_1 };
        static readonly LevelId[] StargazerLevels = { LevelId.Stargazer_0 };

        public static bool IsLevelBeaten(LevelId levelId)
        {
            if (levelId == LevelId.Stargazer_0)
                return true;

            return PlayerPrefs.GetInt(PrefsPrefix + levelId, 0) == 1;
        }

        public static bool IsContinentComplete(Continent continent)
        {
            if (continent == Continent.Stargazer)
                return true;

            foreach (var levelId in GetLevelsForContinent(continent))
            {
                if (!IsLevelBeaten(levelId))
                    return false;
            }

            return true;
        }

        public static Color GetContinentButtonFill(Continent continent) =>
            IsContinentComplete(continent) ? ContinentCompleteFill : UnlockedFill;

        public static Color GetContinentButtonRing(Continent continent) =>
            IsContinentComplete(continent) ? ContinentCompleteRing : UnlockedRing;

        public static Color GetLevelButtonFill(LevelId levelId) =>
            IsLevelBeaten(levelId) ? LevelBeatenFill : UnlockedFill;

        public static Color GetLevelButtonRing(LevelId levelId) =>
            IsLevelBeaten(levelId) ? LevelBeatenRing : UnlockedRing;

        public static LevelId GetLevelForRegion(bool stargazer) =>
            stargazer ? LevelId.Stargazer_0 : LevelId.DreamIsles_1;

        public static void MarkCurrentSceneBeaten()
        {
            if (!TryGetLevelFromScene(SceneManager.GetActiveScene().name, out var levelId))
                return;

            MarkLevelBeaten(levelId);
        }

        public static void MarkLevelBeaten(LevelId levelId)
        {
            PlayerPrefs.SetInt(PrefsPrefix + levelId, 1);
            PlayerPrefs.Save();

            if (levelId == LevelId.Stargazer_0)
                _pendingStargazerCelebration = true;
        }

        public static bool ConsumeStargazerCelebration()
        {
            var pending = _pendingStargazerCelebration;
            _pendingStargazerCelebration = false;
            return pending || !IsContinentComplete(Continent.DreamIsles);
        }

        public static bool ShouldShowPhantasiaAlert =>
            PlayerPrefs.GetInt(PhantasiaAlertKey, 0) == 1 && !IsContinentComplete(Continent.DreamIsles);

        public static void EnablePhantasiaAlert()
        {
            PlayerPrefs.SetInt(PhantasiaAlertKey, 1);
            PlayerPrefs.Save();
        }

        public static bool TryGetLevelFromScene(string sceneName, out LevelId levelId)
        {
            switch (sceneName)
            {
                case SceneNames.StoryMode2:
                    levelId = LevelId.DreamIsles_1;
                    return true;
                case SceneNames.StoryMode1:
                    levelId = LevelId.Stargazer_0;
                    return true;
                case SceneNames.AllFeatures:
                    levelId = LevelId.DreamIsles_1;
                    return true;
                default:
                    levelId = default;
                    return false;
            }
        }

        static LevelId[] GetLevelsForContinent(Continent continent) =>
            continent switch
            {
                Continent.Stargazer => StargazerLevels,
                _ => DreamIslesLevels
            };
    }
}
