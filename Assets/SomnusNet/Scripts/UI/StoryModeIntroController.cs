using System.Collections;
using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.Gameplay;
using SomnusNet.Units;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class StoryModeIntroController : MonoBehaviour
    {
        const int OverlaySortOrder = 235;

        const int DialogFontSize = 24;
        const int PortraitTitleFontSize = 18;
        const float BasePanelWidth = 0.58f;
        const float MinPanelHeight = 0.26f;
        const float MaxPanelHeight = 0.46f;
        const float PortraitWidth = 0.28f;
        const float PortraitHeight = 0.40f;
        const float PortraitPanelGap = 0.03f;
        const float PortraitTitleBand = 0.05f;
        const float DialogGroupCenterY = 0.5f;
        const float TextSidePadding = 0.06f;
        const float TextTopPadding = 0.10f;
        const float TextBottomPadding = 0.24f;
        const float TypewriterLetterDelay = 0.035f;
        const float ShopArrowClearance = 6f;
        const float PonderArrowClearance = 6f;
        const float PonderHintWidth = 0.42f;
        const float PonderHintBottom = 0.788f;
        const float PonderHintTop = 0.882f;

        static readonly TextGenerator TextMeasure = new();

        static readonly string[] DialogLines =
        {
            "Welcome to The Mind Sphere!",
            "As The Gatekeeper, I would normally show you around...\nbut there seems to be an issue with the barrier recently.",
            "What was that?",
            "No! the glitch swarm is trying to break in!"
        };

        static readonly string OutroLine = "There's too many! We have to retreat!";

        enum Phase
        {
            Dialog,
            AwaitShopSelect,
            AwaitPlacement,
            Complete,
            Outro
        }

        RectTransform _overlayRoot;
        RectTransform _dimOverlay;
        RectTransform _portrait;
        Image _portraitImage;
        Text _portraitTitleText;
        Canvas _rootCanvas;
        RectTransform _panel;
        RectTransform _shopArrow;
        RectTransform _ponderArrow;
        Text _dialogText;
        Text _edgeHintText;
        Text _ponderHintText;
        Button _nextButton;
        int _dialogIndex;
        Phase _phase = Phase.Dialog;
        System.Action _outroCompleteCallback;
        string _pendingDialogText;
        Coroutine _typewriterRoutine;
        bool _typewriterComplete = true;

        public static StoryModeIntroController Instance { get; private set; }

        public bool CanPlaceBlobs => !StoryModeRules.Active || _phase == Phase.AwaitPlacement || _phase == Phase.Complete;

        public bool IsIntroComplete => _phase == Phase.Complete;

        public static void EnsureExists()
        {
            if (!StoryModeRules.HasDialogs || Instance != null)
                return;

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<StoryModeIntroController>();
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
            HideHintUi();
            ShowDialogUi();
            SetDialogLine(0);

            if (_nextButton != null)
                _nextButton.onClick.AddListener(AdvanceDialog);

            var hud = Object.FindFirstObjectByType<GameHud>();
            if (hud != null)
                hud.ShopBlobSelected += HandleShopBlobSelected;

            var placement = Object.FindFirstObjectByType<PlacementController>();
            placement?.ClearSelectionForStoryIntro();

            PauseGame();
        }

        void OnDestroy()
        {
            StopTypewriter();

            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(AdvanceDialog);

            var hud = Object.FindFirstObjectByType<GameHud>();
            if (hud != null)
                hud.ShopBlobSelected -= HandleShopBlobSelected;

            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if ((_phase == Phase.Dialog || _phase == Phase.Outro) && Input.GetKeyDown(KeyCode.Return))
                AdvanceDialog();
        }

        void LateUpdate()
        {
            if (_phase != Phase.AwaitShopSelect)
                return;

            UpdateShopArrowPosition();
            UpdatePonderArrowPosition();
        }

        void AdvanceDialog()
        {
            if (_phase == Phase.Outro)
            {
                if (TryCompleteTypewriter())
                    return;

                DismissOutro();
                return;
            }

            if (_phase != Phase.Dialog)
                return;

            if (TryCompleteTypewriter())
                return;

            if (_dialogIndex == 1)
                StoryModeSfx.PlayCrack(transform);

            if (_dialogIndex >= DialogLines.Length - 1)
            {
                BeginShopSelectPhase();
                return;
            }

            _dialogIndex++;
            SetDialogLine(_dialogIndex);
        }

        void SetDialogLine(int index)
        {
            _dialogText.alignment = index == 1
                ? TextAnchor.UpperCenter
                : TextAnchor.MiddleCenter;
            SetPortraitExpression(StargazerDialogSprites.ForIntroLine(index));
            BeginTypewriter(DialogLines[index]);
        }

        void BeginTypewriter(string fullText)
        {
            StopTypewriter();
            _pendingDialogText = fullText;
            RefreshDialogLayout();
            _typewriterComplete = false;
            _typewriterRoutine = StartCoroutine(TypewriterReveal(fullText));
        }

        IEnumerator TypewriterReveal(string fullText)
        {
            if (_dialogText == null)
            {
                _typewriterComplete = true;
                yield break;
            }

            _dialogText.text = string.Empty;

            for (var i = 0; i < fullText.Length; i++)
            {
                _dialogText.text += fullText[i];
                yield return new WaitForSecondsRealtime(TypewriterLetterDelay);
            }

            _typewriterComplete = true;
            _typewriterRoutine = null;
        }

        bool TryCompleteTypewriter()
        {
            if (_typewriterComplete)
                return false;

            StopTypewriter();
            if (_dialogText != null)
                _dialogText.text = _pendingDialogText;
            _typewriterComplete = true;
            return true;
        }

        void StopTypewriter()
        {
            if (_typewriterRoutine == null)
                return;

            StopCoroutine(_typewriterRoutine);
            _typewriterRoutine = null;
        }

        void SetPortraitExpression(StargazerDialogExpression expression)
        {
            if (_portraitImage == null)
                return;

            var sprite = StargazerDialogSprites.Get(expression);
            if (sprite != null)
                _portraitImage.sprite = sprite;
        }

        void BeginShopSelectPhase()
        {
            _phase = Phase.AwaitShopSelect;
            HideDialogUi();
            ShowHintUi();
            SetEdgeHint("Click to select blob");
            UpdateShopArrowPosition();
            UpdatePonderArrowPosition();
            StartCoroutine(RefreshShopArrowAfterLayout());
            ResumeGame();
        }

        IEnumerator RefreshShopArrowAfterLayout()
        {
            yield return null;
            UpdateShopArrowPosition();
            UpdatePonderArrowPosition();
            yield return null;
            UpdateShopArrowPosition();
            UpdatePonderArrowPosition();
        }

        void HandleShopBlobSelected(BlobKind kind)
        {
            if (_phase != Phase.AwaitShopSelect || kind != BlobKind.Gatekeeper)
                return;

            BeginPlacementPhase();
        }

        void BeginPlacementPhase()
        {
            _phase = Phase.AwaitPlacement;
            HideTutorialArrows();
            SetEdgeHint("Click on a tile to place the blob down");
        }

        public void NotifyFirstBlobPlaced(DreamBlob blob)
        {
            if (_phase != Phase.AwaitPlacement)
                return;

            _phase = Phase.Complete;
            HideHintUi();
            StargazerRegistry.AssignInitialStargazer(blob);
            StoryModeHarmonyTutorial.EnsureExists();
            StoryModeHarmonyTutorial.Instance?.NotifyIntroComplete();
        }

        public void BeginOutro(System.Action onComplete)
        {
            _outroCompleteCallback = onComplete;
            _phase = Phase.Outro;
            EnsureUiBuilt();
            HideHintUi();
            ShowDialogUi();
            _dialogText.alignment = TextAnchor.MiddleCenter;
            SetPortraitExpression(StargazerDialogExpression.Worried);
            BeginTypewriter(OutroLine);
            if (_nextButton != null)
                _nextButton.gameObject.SetActive(true);
            PauseGame();
        }

        void DismissOutro()
        {
            HideDialogUi();
            _phase = Phase.Complete;
            var callback = _outroCompleteCallback;
            _outroCompleteCallback = null;
            callback?.Invoke();
        }

        void PauseGame()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.Phase == GamePhase.Playing)
                gm.Pause();
        }

        void ResumeGame()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.Phase == GamePhase.Paused)
                gm.Resume();
        }

        void ShowDialogUi()
        {
            if (_dimOverlay != null)
                _dimOverlay.gameObject.SetActive(true);
            if (_panel != null)
                _panel.gameObject.SetActive(true);
            if (_portrait != null)
                _portrait.gameObject.SetActive(true);
            if (_portraitTitleText != null)
            {
                StyleStargazerLabel(_portraitTitleText);
                _portraitTitleText.gameObject.SetActive(true);
                _portraitTitleText.transform.SetAsLastSibling();
            }

            if (_portrait != null)
                _portrait.SetAsLastSibling();
        }

        void HideDialogUi()
        {
            StopTypewriter();

            if (_dimOverlay != null)
                _dimOverlay.gameObject.SetActive(false);
            if (_portrait != null)
                _portrait.gameObject.SetActive(false);
            if (_portraitTitleText != null)
                _portraitTitleText.gameObject.SetActive(false);
            if (_panel != null)
                _panel.gameObject.SetActive(false);
        }

        void ShowHintUi()
        {
            if (_edgeHintText != null)
                _edgeHintText.gameObject.SetActive(true);
            if (_shopArrow != null)
                _shopArrow.gameObject.SetActive(true);
            if (_ponderArrow != null)
                _ponderArrow.gameObject.SetActive(true);
            if (_ponderHintText != null)
                _ponderHintText.gameObject.SetActive(true);
        }

        void HideHintUi()
        {
            HideTutorialArrows();
            if (_edgeHintText != null)
                _edgeHintText.gameObject.SetActive(false);
        }

        void HideTutorialArrows()
        {
            if (_shopArrow != null)
                _shopArrow.gameObject.SetActive(false);
            if (_ponderArrow != null)
                _ponderArrow.gameObject.SetActive(false);
            if (_ponderHintText != null)
                _ponderHintText.gameObject.SetActive(false);
        }

        void HideShopArrow()
        {
            HideTutorialArrows();
        }

        void SetEdgeHint(string message)
        {
            if (_edgeHintText != null)
                _edgeHintText.text = message;
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
                EnsureShopArrowRendersOnTop();
            }

            var centerX = (shopRect.anchorMin.x + shopRect.anchorMax.x) * 0.5f;
            var topY = shopRect.anchorMax.y;

            _shopArrow.anchorMin = _shopArrow.anchorMax = new Vector2(centerX, topY);
            _shopArrow.pivot = new Vector2(0.5f, 0f);
            _shopArrow.anchoredPosition = new Vector2(0f, ShopArrowClearance);
            _shopArrow.localEulerAngles = new Vector3(0f, 0f, -90f);
            _shopArrow.SetAsLastSibling();
        }

        void UpdatePonderArrowPosition()
        {
            if (_ponderArrow == null)
                return;

            var hud = Object.FindFirstObjectByType<GameHud>();
            if (hud == null || !hud.TryGetPondersTextRect(out var pondersRect))
                return;

            var canvasRect = pondersRect.parent as RectTransform;
            if (canvasRect == null)
                return;

            if (_ponderArrow.parent != canvasRect)
            {
                _ponderArrow.SetParent(canvasRect, false);
                EnsureArrowRendersOnTop(_ponderArrow);
            }

            var leftX = pondersRect.anchorMin.x + 0.015f;
            var bottomY = pondersRect.anchorMin.y;

            _ponderArrow.anchorMin = _ponderArrow.anchorMax = new Vector2(leftX, bottomY);
            _ponderArrow.pivot = new Vector2(0.5f, 1f);
            _ponderArrow.anchoredPosition = new Vector2(0f, -PonderArrowClearance);
            _ponderArrow.localEulerAngles = new Vector3(0f, 0f, 90f);
            _ponderArrow.SetAsLastSibling();

            if (_ponderHintText != null)
            {
                var hintRect = _ponderHintText.rectTransform;
                var hintLeft = pondersRect.anchorMin.x + 0.01f;
                hintRect.anchorMin = new Vector2(hintLeft, PonderHintBottom);
                hintRect.anchorMax = new Vector2(hintLeft + PonderHintWidth, PonderHintTop);
                hintRect.offsetMin = hintRect.offsetMax = Vector2.zero;
            }
        }

        void EnsureShopArrowRendersOnTop()
        {
            EnsureArrowRendersOnTop(_shopArrow);
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
            var canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            _rootCanvas = canvas;

            if (TryBindExistingOverlay(canvas))
            {
                EnsurePortraitTitle();
                RefreshDialogLayout();
                return;
            }

            ClearStaleOverlays(canvas.transform);
            BuildOverlayUi(canvas);
        }

        void ClearStaleOverlays(Transform canvasTransform)
        {
            for (var i = canvasTransform.childCount - 1; i >= 0; i--)
            {
                var child = canvasTransform.GetChild(i);
                if (child.name == "StoryIntroOverlay")
                    Destroy(child.gameObject);
            }
        }

        bool TryBindExistingOverlay(Canvas canvas)
        {
            var existing = canvas.transform.Find("StoryIntroOverlay") as RectTransform;
            if (existing == null)
                return false;

            _overlayRoot = existing;
            _dimOverlay = existing.Find("StoryIntroDim") as RectTransform;
            _panel = existing.Find("StoryIntroPanel") as RectTransform;
            _portrait = existing.Find("Portrait") as RectTransform;
            _portraitImage = _portrait != null ? _portrait.GetComponent<Image>() : null;
            _dialogText = _panel != null ? _panel.Find("DialogText")?.GetComponent<Text>() : null;
            _nextButton = _panel != null ? _panel.Find("NextButton")?.GetComponent<Button>() : null;
            _edgeHintText = existing.Find("EdgeHint")?.GetComponent<Text>();
            _ponderHintText = existing.Find("PonderHint")?.GetComponent<Text>();
            _shopArrow = existing.Find("ShopArrow") as RectTransform;
            _ponderArrow = existing.Find("PonderArrow") as RectTransform;
            _portraitTitleText = existing.Find("PortraitStargazerLabel")?.GetComponent<Text>();
            if (_portraitTitleText == null && _portrait != null)
                _portraitTitleText = _portrait.Find("PortraitTitle")?.GetComponent<Text>();
            if (_portraitTitleText != null)
                StyleStargazerLabel(_portraitTitleText);

            var staleFieldLabel = existing.Find("FieldStargazerLabel");
            if (staleFieldLabel != null)
                Destroy(staleFieldLabel.gameObject);

            return _panel != null && _portrait != null && _dialogText != null;
        }

        void BuildOverlayUi(Canvas canvas)
        {
            _overlayRoot = UiPanelFactory.EnsureOverlayCanvas(canvas.transform, "StoryIntroOverlay", OverlaySortOrder)
                .GetComponent<RectTransform>();

            _dimOverlay = UiPanelFactory.CreateRect(_overlayRoot, "StoryIntroDim", Vector2.zero, Vector2.one);
            var dimImg = _dimOverlay.gameObject.AddComponent<Image>();
            dimImg.color = UiPanelFactory.DimOverlay;
            dimImg.raycastTarget = true;

            _panel = UiPanelFactory.CreateRect(_overlayRoot, "StoryIntroPanel", Vector2.zero, Vector2.one);
            var panelBg = _panel.gameObject.AddComponent<Image>();
            panelBg.color = UiPanelFactory.PanelBg;

            _dialogText = UiPanelFactory.CreateText(_panel, "DialogText",
                new Vector2(TextSidePadding, TextBottomPadding),
                new Vector2(0.74f, 1f - TextTopPadding),
                DialogFontSize, TextAnchor.MiddleCenter);
            _dialogText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _dialogText.verticalOverflow = VerticalWrapMode.Overflow;

            _nextButton = CreateNextButton(_panel);
            _nextButton.transform.SetAsLastSibling();

            _portrait = UiPanelFactory.CreateRect(_overlayRoot, "Portrait", Vector2.zero, Vector2.one);
            _portraitImage = _portrait.gameObject.AddComponent<Image>();
            _portraitImage.sprite = StargazerDialogSprites.Get(StargazerDialogExpression.Happy);
            _portraitImage.preserveAspect = true;
            _portraitImage.raycastTarget = false;

            _portraitTitleText = UiPanelFactory.CreateText(_overlayRoot, "PortraitStargazerLabel",
                Vector2.zero, Vector2.one,
                PortraitTitleFontSize, TextAnchor.MiddleCenter, StargazerRegistry.DesignationLabel);
            StyleStargazerLabel(_portraitTitleText);
            _portraitTitleText.gameObject.SetActive(false);

            _portrait.gameObject.SetActive(false);
            ApplyDialogGroupLayout(MinPanelHeight);

            _edgeHintText = UiPanelFactory.CreateText(_overlayRoot, "EdgeHint",
                new Vector2(0.18f, 0.16f), new Vector2(0.82f, 0.22f), 17, TextAnchor.MiddleCenter);
            _edgeHintText.fontStyle = FontStyle.Bold;
            _edgeHintText.gameObject.SetActive(false);

            _shopArrow = CreateTutorialArrow("ShopArrow");
            _shopArrow.SetParent(_overlayRoot, false);
            _shopArrow.gameObject.SetActive(false);

            _ponderArrow = CreateTutorialArrow("PonderArrow");
            _ponderArrow.SetParent(_overlayRoot, false);
            _ponderArrow.gameObject.SetActive(false);

            _ponderHintText = UiPanelFactory.CreateText(_overlayRoot, "PonderHint",
                new Vector2(0.02f, PonderHintBottom), new Vector2(0.44f, PonderHintTop), 12, TextAnchor.UpperLeft,
                "This is the currency needed to summon blobs");
            _ponderHintText.color = Color.white;
            _ponderHintText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _ponderHintText.verticalOverflow = VerticalWrapMode.Overflow;
            _ponderHintText.gameObject.SetActive(false);
        }

        void EnsurePortraitTitle()
        {
            if (_portraitTitleText != null || _overlayRoot == null)
                return;

            _portraitTitleText = UiPanelFactory.CreateText(_overlayRoot, "PortraitStargazerLabel",
                Vector2.zero, Vector2.one,
                PortraitTitleFontSize, TextAnchor.MiddleCenter, StargazerRegistry.DesignationLabel);
            StyleStargazerLabel(_portraitTitleText);
            _portraitTitleText.gameObject.SetActive(false);
        }

        void RefreshDialogLayout()
        {
            if (_panel == null || _dialogText == null || _overlayRoot == null)
                return;

            _dialogText.fontSize = DialogFontSize;
            ApplyDialogGroupLayout(CalculatePanelHeightForCurrentText());
            Canvas.ForceUpdateCanvases();
        }

        float CalculatePanelHeightForCurrentText()
        {
            var layoutText = !string.IsNullOrEmpty(_pendingDialogText)
                ? _pendingDialogText
                : _dialogText != null ? _dialogText.text : string.Empty;

            if (_dialogText == null || _overlayRoot == null || string.IsNullOrEmpty(layoutText))
                return MinPanelHeight;

            var overlayHeight = _overlayRoot.rect.height;
            if (overlayHeight <= 0f)
                return MinPanelHeight;

            var textWidthNorm = BasePanelWidth * (0.74f - TextSidePadding);
            var textWidthPx = _overlayRoot.rect.width * textWidthNorm;
            var settings = _dialogText.GetGenerationSettings(new Vector2(textWidthPx, 0f));
            settings.fontSize = DialogFontSize;
            settings.font = _dialogText.font;
            settings.fontStyle = _dialogText.fontStyle;
            settings.lineSpacing = _dialogText.lineSpacing;
            settings.scaleFactor = _rootCanvas != null ? _rootCanvas.scaleFactor : 1f;

            var textHeightPx = TextMeasure.GetPreferredHeight(layoutText, settings);
            var panelHeight = textHeightPx / overlayHeight + TextTopPadding + TextBottomPadding;
            return Mathf.Clamp(panelHeight, MinPanelHeight, MaxPanelHeight);
        }

        void ApplyDialogGroupLayout(float panelHeight)
        {
            var groupWidth = PortraitWidth + PortraitPanelGap + BasePanelWidth;
            var groupLeft = (1f - groupWidth) * 0.5f;
            var panelLeft = groupLeft + PortraitWidth + PortraitPanelGap;
            var panelBottom = DialogGroupCenterY - panelHeight * 0.5f;
            var panelTop = panelBottom + panelHeight;
            var portraitBottom = DialogGroupCenterY - PortraitHeight * 0.5f;
            var portraitTop = portraitBottom + PortraitHeight;
            var titleBottom = portraitBottom - PortraitTitleBand;

            SetAnchors(_panel, new Vector2(panelLeft, panelBottom), new Vector2(panelLeft + BasePanelWidth, panelTop));

            if (_portrait != null)
                SetAnchors(_portrait, new Vector2(groupLeft, portraitBottom), new Vector2(groupLeft + PortraitWidth, portraitTop));

            if (_portraitTitleText != null)
            {
                var titleRect = _portraitTitleText.rectTransform;
                SetAnchors(titleRect, new Vector2(groupLeft, titleBottom), new Vector2(groupLeft + PortraitWidth, portraitBottom));
            }
        }

        static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            if (rect == null)
                return;

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        static void StyleStargazerLabel(Text text)
        {
            if (text == null)
                return;

            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.raycastTarget = false;

            if (text.GetComponent<Outline>() == null)
            {
                var outline = text.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
                outline.effectDistance = new Vector2(1.2f, -1.2f);
            }
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

        static Button CreateNextButton(RectTransform panel)
        {
            var button = UiPanelFactory.CreateButton(panel, "NextButton",
                new Vector2(0.76f, 0.04f), new Vector2(0.98f, 0.26f), ">", 26);

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.fontStyle = FontStyle.Bold;
                label.color = Color.white;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 18;
                label.resizeTextMaxSize = 28;
            }

            return button;
        }
    }
}
