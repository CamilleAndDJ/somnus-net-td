using UnityEngine;

namespace SomnusNet.Core
{
    [DefaultExecutionOrder(-100)]
    public class LevelSettings : MonoBehaviour
    {
        public string levelTitle = "Level 01 — First Breach";
        public bool infiniteRounds;
        public bool storyMode;
        public int storyModeVariant = 1;
        public bool enableStoryDialogs = true;
        public bool upgradesUnlockedFromStart;
        public bool enableHarmonyTypes;
        public bool enableDevSkip;
        public int totalRounds = 2;
        public string levelCompleteBanner = "End of Level 1";

        public static LevelSettings Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            if (storyMode)
                LevelLayoutSession.ApplyForStoryMode(storyModeVariant);
            else
                LevelLayoutSession.BeginAllFeaturesLoad();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
