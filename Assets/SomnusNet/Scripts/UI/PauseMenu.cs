using SomnusNet.Core;
using SomnusNet.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class PauseMenu : MonoBehaviour
    {
        const int OverlaySortOrder = 220;

        [SerializeField] Button pauseButton;
        [SerializeField] RectTransform dimOverlay;
        [SerializeField] RectTransform pausePanel;
        [SerializeField] Button resumeButton;
        [SerializeField] Button infoButton;
        [SerializeField] Button quitButton;

        public static PauseMenu Instance { get; private set; }

        public static void EnsureExists()
        {
            if (Instance != null) return;
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<PauseMenu>();
            BlobGlitchInfoPanel.EnsureExists();
        }

        void Awake() => Instance = this;

        void Start()
        {
            EnsureUiBuilt();
            HidePauseUi();
            BlobGlitchInfoPanel.EnsureExists();

            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
            if (infoButton != null)
                infoButton.onClick.AddListener(OnInfoClicked);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (pauseButton == null) return;
            var gm = GameManager.Instance;
            if (gm == null) return;

            var canPause = gm.Phase is GamePhase.Playing or GamePhase.RoundIntermission;
            pauseButton.gameObject.SetActive(canPause || gm.Phase == GamePhase.Paused);
            pauseButton.interactable = canPause;
        }

        void OnPauseClicked()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.Pause();
            ShowPauseUi();
        }

        void OnResumeClicked()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.Resume();
            BlobGlitchInfoPanel.Instance?.Hide();
            HidePauseUi();
        }

        void OnInfoClicked()
        {
            if (pausePanel != null)
                pausePanel.gameObject.SetActive(false);
            BlobGlitchInfoPanel.Instance?.Show();
        }

        void OnQuitClicked()
        {
            BlobGlitchInfoPanel.Instance?.Hide();
            GameManager.Instance?.QuitToMenu();
        }

        public void ShowPausePanelFromInfo()
        {
            if (dimOverlay != null)
                dimOverlay.gameObject.SetActive(true);
            if (pausePanel != null)
                pausePanel.gameObject.SetActive(true);
        }

        void ShowPauseUi()
        {
            if (dimOverlay != null)
                dimOverlay.gameObject.SetActive(true);
            if (pausePanel != null)
                pausePanel.gameObject.SetActive(true);
        }

        void HidePauseUi()
        {
            if (dimOverlay != null)
                dimOverlay.gameObject.SetActive(false);
            if (pausePanel != null)
                pausePanel.gameObject.SetActive(false);
        }

        void EnsureUiBuilt()
        {
            if (pauseButton != null) return;

            var canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var overlayRoot = UiPanelFactory.EnsureOverlayCanvas(canvas.transform, "PauseMenuOverlay", OverlaySortOrder)
                .transform;

            pauseButton = UiPanelFactory.CreateButton(overlayRoot, "PauseButton",
                new Vector2(0.91f, 0.82f), new Vector2(0.985f, 0.895f), "||", 18);

            dimOverlay = UiPanelFactory.CreateRect(overlayRoot, "PauseDim",
                Vector2.zero, Vector2.one);
            var dimImg = dimOverlay.gameObject.AddComponent<Image>();
            dimImg.color = UiPanelFactory.DimOverlay;
            dimImg.raycastTarget = true;

            pausePanel = UiPanelFactory.CreateRect(overlayRoot, "PausePanel",
                new Vector2(0.32f, 0.34f), new Vector2(0.68f, 0.66f));
            var panelBg = pausePanel.gameObject.AddComponent<Image>();
            panelBg.color = UiPanelFactory.PanelBg;

            var title = UiPanelFactory.CreateText(pausePanel, "Title",
                new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.94f), 24, TextAnchor.MiddleCenter, "Paused");
            title.fontStyle = FontStyle.Bold;

            resumeButton = UiPanelFactory.CreateButton(pausePanel, "ResumeButton",
                new Vector2(0.12f, 0.58f), new Vector2(0.88f, 0.74f), "Resume", 18);
            infoButton = UiPanelFactory.CreateButton(pausePanel, "InfoButton",
                new Vector2(0.12f, 0.36f), new Vector2(0.88f, 0.52f), "More Blob & Glitch Info", 15);
            quitButton = UiPanelFactory.CreateButton(pausePanel, "QuitButton",
                new Vector2(0.12f, 0.14f), new Vector2(0.88f, 0.30f), "Quit to Menu", 16);
        }
    }
}
