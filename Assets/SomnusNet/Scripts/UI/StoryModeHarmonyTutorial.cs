using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.Units;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class StoryModeHarmonyTutorial : MonoBehaviour
    {
        const int OverlaySortOrder = 236;
        const float ShopArrowClearance = 6f;
        const float HarmonyArrowClearance = 8f;
        const float HarmonyHintWidth = 0.32f;

        enum Phase
        {
            Idle,
            Active,
            Complete
        }

        RectTransform _overlayRoot;
        RectTransform _shopArrow;
        RectTransform _harmonyArrow;
        Text _shopHintText;
        Text _harmonyHintText;

        Phase _phase = Phase.Idle;
        bool _introComplete;
        bool _hadBelowStarting;

        public static StoryModeHarmonyTutorial Instance { get; private set; }

        public bool IsTutorialActive => _phase == Phase.Active;

        public static void EnsureExists()
        {
            if (!StoryModeRules.HasDialogs || Instance != null)
                return;

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<StoryModeHarmonyTutorial>();
        }

        public static void NotifyBlobPlaced(DreamBlob blob)
        {
            if (blob == null)
                return;

            Instance?.OnBlobPlaced();
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
            HideTutorialUi();

            if (PonderEconomy.Instance != null)
            {
                PonderEconomy.Instance.OnPondersChanged += HandlePondersChanged;
                if (PonderEconomy.Instance.Current < StoryModeRules.StartingPonders)
                    _hadBelowStarting = true;
            }
        }

        void OnDestroy()
        {
            if (PonderEconomy.Instance != null)
                PonderEconomy.Instance.OnPondersChanged -= HandlePondersChanged;

            if (Instance == this)
                Instance = null;
        }

        void LateUpdate()
        {
            if (_phase != Phase.Active)
                return;

            UpdateShopArrowPosition();
            UpdateHarmonyArrowPosition();

            if (HasHarmonyBandLink())
                CompleteTutorial();
        }

        public void NotifyIntroComplete()
        {
            _introComplete = true;

            if (PonderEconomy.Instance != null && PonderEconomy.Instance.Current < StoryModeRules.StartingPonders)
                _hadBelowStarting = true;

            TryBeginTutorial(PonderEconomy.Instance != null ? PonderEconomy.Instance.Current : 0);
        }

        void HandlePondersChanged(int amount)
        {
            if (_phase != Phase.Idle || !_introComplete)
                return;

            if (amount < StoryModeRules.StartingPonders)
                _hadBelowStarting = true;

            TryBeginTutorial(amount);
        }

        void TryBeginTutorial(int ponders)
        {
            if (_phase != Phase.Idle || !_introComplete || !_hadBelowStarting)
                return;

            if (ponders < StoryModeRules.StartingPonders)
                return;

            BeginTutorial();
        }

        void BeginTutorial()
        {
            _phase = Phase.Active;
            ShowTutorialUi();
            StoryModeStargazerSpeech.ShowMessage("I may need some help");
        }

        void OnBlobPlaced()
        {
            if (_phase != Phase.Active)
                return;

            if (HasHarmonyBandLink())
                CompleteTutorial();
        }

        void CompleteTutorial()
        {
            if (_phase != Phase.Active)
                return;

            _phase = Phase.Complete;
            HideTutorialUi();
            StoryModeStargazerSpeech.ShowMessage("Thanks!");
        }

        static bool HasHarmonyBandLink()
        {
            var grid = GridManager.Instance;
            var stargazer = StargazerRegistry.Current;
            if (grid == null || stargazer == null || !stargazer.IsAlive)
                return false;

            return grid.GetHarmonyClusterSize(stargazer) >= 2;
        }

        void ShowTutorialUi()
        {
            if (_shopArrow != null)
                _shopArrow.gameObject.SetActive(true);
            if (_harmonyArrow != null)
                _harmonyArrow.gameObject.SetActive(true);
            if (_shopHintText != null)
                _shopHintText.gameObject.SetActive(true);
            if (_harmonyHintText != null)
                _harmonyHintText.gameObject.SetActive(true);

            UpdateShopArrowPosition();
            UpdateHarmonyArrowPosition();
        }

        void HideTutorialUi()
        {
            if (_shopArrow != null)
                _shopArrow.gameObject.SetActive(false);
            if (_harmonyArrow != null)
                _harmonyArrow.gameObject.SetActive(false);
            if (_shopHintText != null)
                _shopHintText.gameObject.SetActive(false);
            if (_harmonyHintText != null)
                _harmonyHintText.gameObject.SetActive(false);
        }

        void UpdateShopArrowPosition()
        {
            if (_shopArrow == null)
                return;

            var hud = Object.FindFirstObjectByType<GameHud>();
            if (hud == null || !hud.TryGetShopButtonRect(BlobKind.Gatekeeper, out var shopRect))
                return;

            var canvasRect = shopRect.parent as RectTransform;
            if (canvasRect == null)
                return;

            if (_shopArrow.parent != canvasRect)
            {
                _shopArrow.SetParent(canvasRect, false);
                EnsureArrowRendersOnTop(_shopArrow);
            }

            var centerX = (shopRect.anchorMin.x + shopRect.anchorMax.x) * 0.5f;
            var topY = shopRect.anchorMax.y;

            _shopArrow.anchorMin = _shopArrow.anchorMax = new Vector2(centerX, topY);
            _shopArrow.pivot = new Vector2(0.5f, 0f);
            _shopArrow.anchoredPosition = new Vector2(0f, ShopArrowClearance);
            _shopArrow.localEulerAngles = new Vector3(0f, 0f, -90f);
            _shopArrow.SetAsLastSibling();
        }

        void UpdateHarmonyArrowPosition()
        {
            if (_harmonyArrow == null)
                return;

            if (HarmonyTypeHud.Instance == null || !HarmonyTypeHud.Instance.TryGetCounterRect(out var counterRect))
                return;

            var canvasRect = counterRect.parent as RectTransform;
            if (canvasRect == null)
                return;

            if (_harmonyArrow.parent != canvasRect)
            {
                _harmonyArrow.SetParent(canvasRect, false);
                EnsureArrowRendersOnTop(_harmonyArrow);
            }

            var counterRightX = counterRect.anchorMax.x;
            var centerY = (counterRect.anchorMin.y + counterRect.anchorMax.y) * 0.5f;

            _harmonyArrow.anchorMin = _harmonyArrow.anchorMax = new Vector2(counterRightX, centerY);
            _harmonyArrow.pivot = new Vector2(0f, 0.5f);
            _harmonyArrow.anchoredPosition = new Vector2(HarmonyArrowClearance, 0f);
            _harmonyArrow.localEulerAngles = new Vector3(0f, 0f, 180f);
            _harmonyArrow.SetAsLastSibling();

            if (_harmonyHintText != null)
            {
                var hintRect = _harmonyHintText.rectTransform;
                var hintLeft = counterRightX + 0.028f;
                hintRect.anchorMin = new Vector2(hintLeft, counterRect.anchorMin.y);
                hintRect.anchorMax = new Vector2(hintLeft + HarmonyHintWidth, counterRect.anchorMax.y);
                hintRect.offsetMin = hintRect.offsetMax = Vector2.zero;
            }
        }

        static void EnsureArrowRendersOnTop(RectTransform arrow)
        {
            if (arrow == null)
                return;

            var arrowCanvas = arrow.GetComponent<Canvas>();
            if (arrowCanvas == null)
                arrowCanvas = arrow.gameObject.AddComponent<Canvas>();

            var parentCanvas = arrow.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                arrowCanvas.renderMode = parentCanvas.renderMode;
                arrowCanvas.worldCamera = parentCanvas.worldCamera;
                arrowCanvas.planeDistance = parentCanvas.planeDistance;
            }

            arrowCanvas.overrideSorting = true;
            arrowCanvas.sortingOrder = OverlaySortOrder + 5;
        }

        void EnsureUiBuilt()
        {
            if (_overlayRoot != null)
                return;

            var canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            _overlayRoot = UiPanelFactory.EnsureOverlayCanvas(canvas.transform, "StoryHarmonyTutorialOverlay", OverlaySortOrder)
                .GetComponent<RectTransform>();

            _shopArrow = CreateTutorialArrow("HarmonyTutorialShopArrow");
            _harmonyArrow = CreateTutorialArrow("HarmonyTutorialHarmonyArrow");

            _shopHintText = UiPanelFactory.CreateText(_overlayRoot, "HarmonyTutorialShopHint",
                new Vector2(0.18f, 0.16f), new Vector2(0.82f, 0.22f), 17, TextAnchor.MiddleCenter,
                "Place another blob next to the first");
            _shopHintText.fontStyle = FontStyle.Bold;
            _shopHintText.color = Color.white;

            _harmonyHintText = UiPanelFactory.CreateText(_overlayRoot, "HarmonyTutorialHarmonyHint",
                new Vector2(0.19f, 0.835f), new Vector2(0.51f, 0.905f), 12,
                TextAnchor.MiddleLeft, "Harmony Type blobs get stronger together.");
            _harmonyHintText.color = Color.white;
            _harmonyHintText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _harmonyHintText.verticalOverflow = VerticalWrapMode.Overflow;

            HideTutorialUi();
        }

        static RectTransform CreateTutorialArrow(string name)
        {
            var arrowGo = new GameObject(name);
            var arrow = arrowGo.AddComponent<RectTransform>();
            arrow.sizeDelta = new Vector2(44f, 44f);

            var arrowText = arrowGo.AddComponent<Text>();
            arrowText.font = UiPanelFactory.DefaultFont;
            arrowText.text = ">";
            arrowText.fontSize = 34;
            arrowText.fontStyle = FontStyle.Bold;
            arrowText.alignment = TextAnchor.MiddleCenter;
            arrowText.color = new Color(1f, 0.92f, 0.45f);
            arrowText.raycastTarget = false;
            return arrow;
        }
    }
}
