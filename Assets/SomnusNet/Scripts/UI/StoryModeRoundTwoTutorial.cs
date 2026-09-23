using SomnusNet.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class StoryModeRoundTwoTutorial : MonoBehaviour
    {
        const int OverlaySortOrder = 237;
        const string HintMessage = "Click on a Blob to view upgrades";
        const string SpeechMessage = "The stars and moon are with me!";

        RectTransform _overlayRoot;
        Text _hintText;
        bool _active;
        bool _completed;

        public static StoryModeRoundTwoTutorial Instance { get; private set; }

        public static void EnsureExists()
        {
            if (!StoryModeRules.HasDialogs || Instance != null)
                return;

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<StoryModeRoundTwoTutorial>();
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
            HideHint();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRoundStarted += HandleRoundStarted;
                if (GameManager.Instance.CurrentRound >= 2)
                    TryBegin();
            }
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnRoundStarted -= HandleRoundStarted;

            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (!_active || _completed)
                return;

            if (BlobUpgradePanel.Instance != null && BlobUpgradePanel.Instance.IsOpen)
                Complete();
        }

        void HandleRoundStarted(int round)
        {
            if (round == 2)
                TryBegin();
        }

        void TryBegin()
        {
            if (_active || _completed)
                return;

            if (StoryModeIntroController.Instance != null && !StoryModeIntroController.Instance.IsIntroComplete)
                return;

            _active = true;
            ShowHint();
            StoryModeStargazerSpeech.ShowMessage(SpeechMessage);
        }

        void Complete()
        {
            _completed = true;
            _active = false;
            HideHint();
        }

        void ShowHint()
        {
            if (_hintText != null)
                _hintText.gameObject.SetActive(true);
        }

        void HideHint()
        {
            if (_hintText != null)
                _hintText.gameObject.SetActive(false);
        }

        void EnsureUiBuilt()
        {
            if (_hintText != null)
                return;

            var canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            _overlayRoot = UiPanelFactory.EnsureOverlayCanvas(canvas.transform, "StoryRoundTwoTutorialOverlay", OverlaySortOrder)
                .GetComponent<RectTransform>();

            _hintText = UiPanelFactory.CreateText(_overlayRoot, "UpgradeViewHint",
                new Vector2(0.18f, 0.16f), new Vector2(0.82f, 0.22f), 17, TextAnchor.MiddleCenter, HintMessage);
            _hintText.fontStyle = FontStyle.Bold;
            _hintText.color = Color.white;
            _hintText.gameObject.SetActive(false);
        }
    }
}
