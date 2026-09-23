using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class MainMenu : MonoBehaviour
    {
        const string StoryModeScene = "StoryMode1";
        const string AllFeaturesScene = "AllFeaturesLevel";

        void Start() => EnsureUiBuilt();

        void EnsureUiBuilt()
        {
            if (transform.Find("MenuOverlay") != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var overlay = UiPanelFactory.CreateRect(canvasGo.transform, "MenuOverlay", Vector2.zero, Vector2.one);
            var bg = overlay.gameObject.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.07f, 0.14f, 1f);

            var title = UiPanelFactory.CreateText(overlay, "Title",
                new Vector2(0.15f, 0.66f), new Vector2(0.85f, 0.86f), 42, TextAnchor.MiddleCenter, "Somnus Net");
            title.fontStyle = FontStyle.Bold;

            var subtitle = UiPanelFactory.CreateText(overlay, "Subtitle",
                new Vector2(0.2f, 0.58f), new Vector2(0.8f, 0.66f), 18, TextAnchor.MiddleCenter,
                "Defend the Dream Core");

            var storyMode = UiPanelFactory.CreateButton(overlay, "PlayStoryMode1",
                new Vector2(0.32f, 0.40f), new Vector2(0.68f, 0.48f), "Story Mode 1", 18);
            storyMode.onClick.AddListener(() => LoadScene(StoryModeScene));

            var allFeatures = UiPanelFactory.CreateButton(overlay, "PlayAllFeatures",
                new Vector2(0.32f, 0.28f), new Vector2(0.68f, 0.36f), "Play All Features", 18);
            allFeatures.onClick.AddListener(() => LoadScene(AllFeaturesScene));

            var quit = UiPanelFactory.CreateButton(overlay, "Quit",
                new Vector2(0.32f, 0.14f), new Vector2(0.68f, 0.22f), "Quit", 16);
            quit.onClick.AddListener(() => Application.Quit());
        }

        static void LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            if (Application.CanStreamedLevelBeLoaded(sceneName))
                SceneManager.LoadScene(sceneName);
            else
                SceneManager.LoadScene(1);
        }
    }
}
