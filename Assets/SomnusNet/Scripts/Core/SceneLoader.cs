using UnityEngine;
using UnityEngine.SceneManagement;

namespace SomnusNet.Core
{
    public static class SceneLoader
    {
        public static void LoadWorldMap()
        {
            Time.timeScale = 1f;
            if (TryLoadScene(SceneNames.WorldMap))
                return;

            QuitToMenu();
        }

        public static void LoadStoryMode1()
        {
            Time.timeScale = 1f;
            if (!TryLoadScene(SceneNames.StoryMode1))
                QuitToMenu();
        }

        public static void LoadStoryMode2()
        {
            Time.timeScale = 1f;
            if (!TryLoadScene(SceneNames.StoryMode2))
                QuitToMenu();
        }

        public static void LoadAllFeatures()
        {
            Time.timeScale = 1f;
            if (!TryLoadScene(SceneNames.AllFeatures))
                QuitToMenu();
        }

        public static void QuitToMenu()
        {
            Time.timeScale = 1f;
            if (!TryLoadScene(SceneNames.MainMenu))
                SceneManager.LoadScene(0);
        }

        static bool TryLoadScene(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
                return true;
            }

            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                if (!path.EndsWith($"/{sceneName}.unity"))
                    continue;

                SceneManager.LoadScene(i);
                return true;
            }

            return false;
        }
    }
}
