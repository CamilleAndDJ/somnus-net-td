using SomnusNet.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class StoryModeSkyCompassRewardPopup : MonoBehaviour
    {
        const int OverlaySortOrder = 240;
        const float PanelWidth = 0.56f;
        const float PanelHeight = 0.18f;

        static readonly string RewardMessage =
            $"{StargazerRegistry.DesignationLabel} has given you the Sky Compass.";

        enum Phase
        {
            Idle,
            AwaitingCompleteEnter,
            RewardVisible
        }

        RectTransform _overlayRoot;
        RectTransform _dimOverlay;
        RectTransform _panel;
        Text _messageText;
        Phase _phase = Phase.Idle;

        public static StoryModeSkyCompassRewardPopup Instance { get; private set; }

        public static void EnsureExists()
        {
            if (!StoryModeRules.HasDialogs || Instance != null)
                return;

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<StoryModeSkyCompassRewardPopup>();
        }

        void Awake() => Instance = this;

        void Start()
        {
            if (!StoryModeRules.Active)
            {
                enabled = false;
                return;
            }

            EnsureUiBuilt();
            HideReward();

            if (GameManager.Instance != null)
                GameManager.Instance.OnLevelComplete += HandleLevelComplete;
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnLevelComplete -= HandleLevelComplete;

            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (_phase == Phase.Idle || !Input.GetKeyDown(KeyCode.Return))
                return;

            if (_phase == Phase.AwaitingCompleteEnter)
            {
                ShowReward();
                return;
            }

            if (_phase == Phase.RewardVisible)
            {
                HideReward();
                SceneLoader.LoadWorldMap();
            }
        }

        void HandleLevelComplete()
        {
            _phase = Phase.AwaitingCompleteEnter;
        }

        void ShowReward()
        {
            _phase = Phase.RewardVisible;
            Object.FindFirstObjectByType<GameHud>()?.HideLevelCompleteBanner();

            if (_dimOverlay != null)
                _dimOverlay.gameObject.SetActive(true);
            if (_panel != null)
                _panel.gameObject.SetActive(true);
        }

        void HideReward()
        {
            _phase = Phase.Idle;

            if (_dimOverlay != null)
                _dimOverlay.gameObject.SetActive(false);
            if (_panel != null)
                _panel.gameObject.SetActive(false);
        }

        void EnsureUiBuilt()
        {
            if (_panel != null)
                return;

            var canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            _overlayRoot = UiPanelFactory.EnsureOverlayCanvas(canvas.transform, "StorySkyCompassOverlay", OverlaySortOrder)
                .GetComponent<RectTransform>();

            _dimOverlay = UiPanelFactory.CreateRect(_overlayRoot, "SkyCompassDim", Vector2.zero, Vector2.one);
            var dimImg = _dimOverlay.gameObject.AddComponent<Image>();
            dimImg.color = UiPanelFactory.DimOverlay;
            dimImg.raycastTarget = true;

            var panelLeft = (1f - PanelWidth) * 0.5f;
            var panelBottom = (1f - PanelHeight) * 0.5f;
            _panel = UiPanelFactory.CreateRect(_overlayRoot, "SkyCompassPanel",
                new Vector2(panelLeft, panelBottom),
                new Vector2(panelLeft + PanelWidth, panelBottom + PanelHeight));
            var panelBg = _panel.gameObject.AddComponent<Image>();
            panelBg.color = UiPanelFactory.PanelBg;

            _messageText = UiPanelFactory.CreateText(_panel, "SkyCompassMessage",
                new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.88f), 18, TextAnchor.MiddleCenter, RewardMessage);
            _messageText.fontStyle = FontStyle.Bold;
            _messageText.color = Color.white;
            _messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _messageText.verticalOverflow = VerticalWrapMode.Overflow;

            HideReward();
        }
    }
}
