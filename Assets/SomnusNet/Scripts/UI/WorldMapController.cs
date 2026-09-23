using System.Collections;
using System.Collections.Generic;
using SomnusNet.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class WorldMapController : MonoBehaviour
    {
        public const string SceneName = SceneNames.WorldMap;

        const string MapResource = "MindSphereMap";
        const string FullMapTitle = "The Mind Sphere";
        const string ZoomedMapTitle = "Dream Isles";
        const string ContinentLabelText = "Phantasia";
        const string LevelLabelText = "1: A Call For Help";

        const int MapTitleFontSize = 46;
        const int ContinentLabelFontSize = 22;
        const float ContinentLabelWidth = 160f;
        const float ContinentLabelHeight = 32f;
        const float ContinentLabelGap = 10f;
        const float LockedPopupDuration = 1f;
        const float LockedContinentAnchorTopX = 0.52f;
        const float LockedContinentAnchorTopY = 0.76f;
        const float LockedContinentAnchorRightX = 0.82f;
        const float LockedContinentAnchorRightY = 0.54f;
        const float LockedContinentAnchorBottomX = 0.50f;
        const float LockedContinentAnchorBottomY = 0.30f;
        const float LockedContinentAnchorBottomRightX = 0.76f;
        const float LockedContinentAnchorBottomRightY = 0.26f;
        static readonly Color LockedButtonFill = new(0.62f, 0.64f, 0.68f, 1f);
        static readonly Color LockedButtonRing = new(0.48f, 0.50f, 0.54f, 1f);
        static readonly Color TransitionBackgroundColor = new(0.78f, 0.89f, 0.98f, 1f);
        static readonly Color MapBackgroundColor = Color.white;
        static readonly Color CloudOutlineColor = new(0.78f, 0.80f, 0.84f, 1f);
        const float CloudOutlineScale = 1.1f;

        const string StargazerZoomedMapTitle = "Cosmos";
        const string StargazerLevelLabelText = "0: Firewall Breach";
        const string StargazerRecallDialogText = "Stargazer: Oh? Do you want to recall the begining?";

        const float LevelAnchorX = 0.215f;
        const float LevelAnchorY = 0.52f;
        const float ContinentButtonAnchorX = 0.265f;
        const float ContinentButtonAnchorY = 0.57f;
        const float StargazerContinentButtonAnchorX = 0.16f;
        const float StargazerContinentButtonAnchorY = 0.10f;
        const float StargazerLevelAnchorX = 0.09f;
        const float StargazerLevelAnchorY = 0.145f;
        const float StargazerRecallDialogWidth = 268f;
        const float StargazerRecallDialogHeight = 54f;
        const float StargazerRecallDialogGap = 10f;
        const int StargazerRecallDialogFontSize = 13;
        const float ContinentButtonSize = 52f;
        const float LevelButtonSize = 34f;
        const float LevelLabelWidth = 220f;
        const float LevelLabelHeight = 28f;
        const float LevelLabelGap = 8f;
        const float ZoomScale = 2.4f;
        const float ZoomDuration = 0.8f;
        const float ButtonAppearDelay = 0.45f;
        const float ButtonFadeDuration = 0.35f;
        const float CloudTransitionDuration = 4f;
        const float CloudOverlayFadeDuration = 0.4f;
        const float MapRevealAfterCloudsDuration = 0.5f;
        const int CloudTransitionRowCount = 5;
        const int CloudTransitionSecondRowCount = 5;
        const float CloudTransitionSecondRowYOffset = 100f;
        const float CloudTransitionSecondRowXOffset = 90f;
        const float CloudTransitionLowY = -520f;
        const float CloudTransitionHighY = 480f;
        const float CloudTransitionOffscreenX = 1550f;
        const float CloudTransitionScreenHalfWidth = 960f;
        const float CloudTransitionInwardPortion = 0.55f;
        const float LevelCrackStepDelay = 0.28f;
        const float LevelBreakDuration = 0.35f;
        const float LevelHoleCoverDuration = 0.4f;
        const float LevelCrackZoomDuration = 1.1f;
        const float LevelCrackZoomScale = 7.5f;
        const float LevelBlackFadeDuration = 0.35f;
        const float CompletionCheckSize = 64f;
        const float CompletionCheckPopDuration = 0.35f;
        const float CompletionCheckHoldDuration = 2.2f;
        const float CompletionCheckFadeDuration = 0.4f;
        const float UnlockStarSize = 28f;
        const float UnlockStarFlightDuration = 1.2f;
        const float UnlockStarArcHeight = 90f;
        const float UnlockStarSpinDegrees = 540f;
        const float UnlockStarBurstDuration = 0.25f;
        const float AlertBadgeSize = 26f;
        const float AlertBadgeBorder = 3f;
        const float AlertBadgePopDuration = 0.3f;
        const float AlertPulsePeriod = 0.9f;
        const float AlertPulseScale = 1.3f;
        static readonly Color AlertRed = new(0.88f, 0.12f, 0.12f, 1f);
        static readonly Color UnlockStarColor = new(1f, 0.88f, 0.32f, 1f);
        static readonly Color CheckOutlineColor = new(0.14f, 0.42f, 0.16f, 1f);

        struct TransitionCloud
        {
            public RectTransform Rect;
            public float StartX;
            public float EndX;
            public float Y;
            public float Delay;
            public bool FromLeft;
        }

        enum MapRegion
        {
            None,
            DreamIsles,
            Stargazer
        }

        RectTransform _viewport;
        RectTransform _mapRoot;
        RectTransform _continentMarkerRoot;
        RectTransform _continentButtonRect;
        Button _continentButton;
        Image _continentButtonFill;
        Image _continentButtonRing;
        CanvasGroup _continentButtonGroup;
        Text _continentLabelText;
        RectTransform _stargazerContinentMarkerRoot;
        RectTransform _stargazerContinentButtonRect;
        Button _stargazerContinentButton;
        Image _stargazerContinentButtonFill;
        Image _stargazerContinentButtonRing;
        CanvasGroup _stargazerContinentButtonGroup;
        Image _stargazerCompleteCheck;
        RectTransform _phantasiaAlertRoot;
        Coroutine _phantasiaAlertRoutine;
        RectTransform _lockedContinentsRoot;
        CanvasGroup _lockedContinentsGroup;
        RectTransform _lockedPopupRoot;
        Coroutine _lockedPopupRoutine;
        readonly List<RectTransform> _lockedContinentMarkers = new();
        RectTransform _levelMarkerRoot;
        RectTransform _levelButtonRect;
        Button _levelButton;
        Image _levelButtonFill;
        Image _levelButtonRing;
        Text _levelLabelText;
        RectTransform _stargazerRecallDialogRoot;
        RectTransform _levelCrackRoot;
        readonly Image[] _levelCrackImages = new Image[3];
        RectTransform _levelHoleRect;
        Image _levelHoleImage;
        Image _levelEnterBlackout;
        readonly List<RectTransform> _levelBreakShards = new();
        Text _mapTitleText;
        CanvasGroup _mapContentGroup;
        CanvasGroup _mapTitleGroup;
        RectTransform _cloudTransitionRoot;
        CanvasGroup _cloudTransitionGroup;
        RectTransform _cloudLayer;
        readonly List<TransitionCloud> _transitionClouds = new();
        Image _oceanBackground;
        Image _cloudSkyBackground;
        RectTransform _leftRevealRect;
        RectTransform _rightRevealRect;
        Coroutine _pulseRoutine;
        float _mapAspect = 1024f / 729f;
        bool _zoomed;
        bool _busy;
        bool _levelEnterTransition;
        MapRegion _zoomedRegion;

        float ActiveLevelAnchorX =>
            _zoomedRegion == MapRegion.Stargazer ? StargazerLevelAnchorX : LevelAnchorX;

        float ActiveLevelAnchorY =>
            _zoomedRegion == MapRegion.Stargazer ? StargazerLevelAnchorY : LevelAnchorY;

        void Start()
        {
            Time.timeScale = 1f;
            EnsureCamera();
            EnsureUiBuilt();
            StartCoroutine(Boot());
        }

        void Update()
        {
            if (!_zoomed || _busy || _levelEnterTransition || !Input.GetMouseButtonDown(0))
                return;

            if (IsPointerOverLevelButton() || IsPointerOverContinentButton() || IsPointerOverStargazerContinentButton())
                return;

            StartCoroutine(ZoomOut());
        }

        static void EnsureCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                camGo.tag = "MainCamera";
                camGo.AddComponent<AudioListener>();
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = TransitionBackgroundColor;
        }

        void EnsureUiBuilt()
        {
            if (transform.Find("MapCanvas") != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            var canvasGo = new GameObject("MapCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _oceanBackground = UiPanelFactory.CreateImage(canvasGo.transform, "OceanBg",
                Vector2.zero, Vector2.one, TransitionBackgroundColor);
            _oceanBackground.raycastTarget = false;

            _viewport = UiPanelFactory.CreateRect(canvasGo.transform, "MapViewport", Vector2.zero, Vector2.one);
            _viewport.gameObject.AddComponent<RectMask2D>();
            _mapContentGroup = _viewport.gameObject.AddComponent<CanvasGroup>();
            _mapContentGroup.alpha = 0f;
            _mapContentGroup.interactable = false;
            _mapContentGroup.blocksRaycasts = false;

            _mapRoot = UiPanelFactory.CreateRect(_viewport, "MapRoot", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _mapRoot.pivot = new Vector2(0.5f, 0.5f);
            _mapRoot.anchoredPosition = Vector2.zero;

            var mapImage = _mapRoot.gameObject.AddComponent<Image>();
            mapImage.preserveAspect = false;
            mapImage.raycastTarget = false;
            var mapSprite = LoadMapSprite();
            if (mapSprite != null)
            {
                mapImage.sprite = mapSprite;
                mapImage.color = Color.white;
                _mapAspect = mapSprite.rect.width / mapSprite.rect.height;
            }
            else
            {
                mapImage.color = new Color(0.22f, 0.48f, 0.78f, 1f);
            }

            BuildContinentButton();
            BuildStargazerContinentButton();
            BuildStargazerCompleteCheck();
            BuildPhantasiaAlert();
            BuildLockedContinentButtons();
            BuildLevelMarker();
            BuildLockedPopup(canvasGo.transform);
            BuildCloudTransition(canvasGo.transform);
            BuildLevelEnterBlackout(canvasGo.transform);

            _mapTitleText = UiPanelFactory.CreateText(canvasGo.transform, "MapTitle",
                new Vector2(0.08f, 0.88f), new Vector2(0.92f, 0.99f),
                MapTitleFontSize, TextAnchor.MiddleCenter, FullMapTitle);
            StyleWhiteMapText(_mapTitleText);
            _mapTitleText.raycastTarget = false;
            _mapTitleGroup = _mapTitleText.gameObject.AddComponent<CanvasGroup>();
            _mapTitleGroup.alpha = 0f;
            _mapTitleGroup.interactable = false;
            _mapTitleGroup.blocksRaycasts = false;
            _mapTitleText.transform.SetAsLastSibling();

            UpdateMapTitle();
            RefreshOverviewButtonColors();
        }

        void BuildContinentButton()
        {
            _continentMarkerRoot = UiPanelFactory.CreateRect(_mapRoot, "ContinentMarker",
                new Vector2(ContinentButtonAnchorX, ContinentButtonAnchorY),
                new Vector2(ContinentButtonAnchorX, ContinentButtonAnchorY));
            _continentMarkerRoot.pivot = new Vector2(0.5f, 0.5f);
            _continentMarkerRoot.sizeDelta = Vector2.zero;

            _continentButtonRect = UiPanelFactory.CreateRect(_continentMarkerRoot, "ContinentButton",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _continentButtonRect.pivot = new Vector2(0.5f, 0.5f);
            _continentButtonRect.anchoredPosition = Vector2.zero;
            _continentButtonRect.sizeDelta = new Vector2(ContinentButtonSize, ContinentButtonSize);

            var circle = CreateCircleSprite();
            _continentButtonFill = _continentButtonRect.gameObject.AddComponent<Image>();
            _continentButtonFill.sprite = circle;
            _continentButtonFill.preserveAspect = true;
            _continentButtonFill.raycastTarget = true;

            _continentButtonRing = UiPanelFactory.CreateImage(_continentButtonRect, "Ring", Vector2.zero, Vector2.one,
                Color.white);
            _continentButtonRing.sprite = circle;
            _continentButtonRing.preserveAspect = true;
            _continentButtonRing.raycastTarget = false;
            var ringRect = _continentButtonRing.rectTransform;
            ringRect.offsetMin = new Vector2(-8f, -8f);
            ringRect.offsetMax = new Vector2(8f, 8f);
            _continentButtonRing.transform.SetAsFirstSibling();

            _continentButton = _continentButtonRect.gameObject.AddComponent<Button>();
            _continentButton.targetGraphic = _continentButtonFill;
            _continentButton.onClick.AddListener(HandleContinentButtonClicked);

            _continentButtonGroup = _continentMarkerRoot.gameObject.AddComponent<CanvasGroup>();
            _continentButtonGroup.alpha = 0f;
            _continentButtonGroup.interactable = false;
            _continentButtonGroup.blocksRaycasts = false;

            var labelTop = ContinentButtonSize * 0.5f + ContinentLabelGap + ContinentLabelHeight;
            _continentLabelText = UiPanelFactory.CreateText(_continentMarkerRoot, "ContinentLabel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                ContinentLabelFontSize, TextAnchor.MiddleCenter, ContinentLabelText);
            var labelRect = _continentLabelText.rectTransform;
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(ContinentLabelWidth, ContinentLabelHeight);
            labelRect.anchoredPosition = new Vector2(0f, labelTop - ContinentLabelHeight);
            StyleWhiteMapText(_continentLabelText);
            _continentLabelText.raycastTarget = false;
        }

        void BuildStargazerContinentButton()
        {
            _stargazerContinentMarkerRoot = UiPanelFactory.CreateRect(_mapRoot, "StargazerContinentMarker",
                new Vector2(StargazerContinentButtonAnchorX, StargazerContinentButtonAnchorY),
                new Vector2(StargazerContinentButtonAnchorX, StargazerContinentButtonAnchorY));
            _stargazerContinentMarkerRoot.pivot = new Vector2(0.5f, 0.5f);
            _stargazerContinentMarkerRoot.sizeDelta = Vector2.zero;

            _stargazerContinentButtonRect = UiPanelFactory.CreateRect(_stargazerContinentMarkerRoot, "StargazerContinentButton",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _stargazerContinentButtonRect.pivot = new Vector2(0.5f, 0.5f);
            _stargazerContinentButtonRect.anchoredPosition = Vector2.zero;
            _stargazerContinentButtonRect.sizeDelta = new Vector2(ContinentButtonSize, ContinentButtonSize);

            var circle = CreateCircleSprite();
            _stargazerContinentButtonFill = _stargazerContinentButtonRect.gameObject.AddComponent<Image>();
            _stargazerContinentButtonFill.sprite = circle;
            _stargazerContinentButtonFill.preserveAspect = true;
            _stargazerContinentButtonFill.raycastTarget = true;

            _stargazerContinentButtonRing = UiPanelFactory.CreateImage(_stargazerContinentButtonRect, "Ring", Vector2.zero, Vector2.one,
                Color.white);
            _stargazerContinentButtonRing.sprite = circle;
            _stargazerContinentButtonRing.preserveAspect = true;
            _stargazerContinentButtonRing.raycastTarget = false;
            var ringRect = _stargazerContinentButtonRing.rectTransform;
            ringRect.offsetMin = new Vector2(-8f, -8f);
            ringRect.offsetMax = new Vector2(8f, 8f);
            _stargazerContinentButtonRing.transform.SetAsFirstSibling();

            _stargazerContinentButton = _stargazerContinentButtonRect.gameObject.AddComponent<Button>();
            _stargazerContinentButton.targetGraphic = _stargazerContinentButtonFill;
            _stargazerContinentButton.onClick.AddListener(HandleStargazerContinentButtonClicked);

            _stargazerContinentButtonGroup = _stargazerContinentMarkerRoot.gameObject.AddComponent<CanvasGroup>();
            _stargazerContinentButtonGroup.alpha = 0f;
            _stargazerContinentButtonGroup.interactable = false;
            _stargazerContinentButtonGroup.blocksRaycasts = false;
        }

        void BuildStargazerCompleteCheck()
        {
            _stargazerCompleteCheck = UiPanelFactory.CreateImage(_stargazerContinentMarkerRoot, "CompleteCheck",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Color.white);
            var rect = _stargazerCompleteCheck.rectTransform;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(CompletionCheckSize, CompletionCheckSize);
            _stargazerCompleteCheck.sprite = CreateCheckmarkSprite();
            _stargazerCompleteCheck.preserveAspect = true;
            _stargazerCompleteCheck.raycastTarget = false;
            _stargazerCompleteCheck.gameObject.SetActive(false);
        }

        void BuildPhantasiaAlert()
        {
            var circle = CreateCircleSprite();
            _phantasiaAlertRoot = UiPanelFactory.CreateRect(_continentMarkerRoot, "PhantasiaAlert",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _phantasiaAlertRoot.pivot = new Vector2(0.5f, 0.5f);
            _phantasiaAlertRoot.anchoredPosition = new Vector2(ContinentButtonSize * 0.4f, ContinentButtonSize * 0.4f);
            _phantasiaAlertRoot.sizeDelta = new Vector2(AlertBadgeSize, AlertBadgeSize);

            var border = _phantasiaAlertRoot.gameObject.AddComponent<Image>();
            border.sprite = circle;
            border.color = AlertRed;
            border.preserveAspect = true;
            border.raycastTarget = false;

            var inner = UiPanelFactory.CreateImage(_phantasiaAlertRoot, "Inner", Vector2.zero, Vector2.one, Color.white);
            inner.sprite = circle;
            inner.preserveAspect = true;
            inner.raycastTarget = false;
            inner.rectTransform.offsetMin = new Vector2(AlertBadgeBorder, AlertBadgeBorder);
            inner.rectTransform.offsetMax = new Vector2(-AlertBadgeBorder, -AlertBadgeBorder);

            var mark = UiPanelFactory.CreateText(_phantasiaAlertRoot, "Mark", Vector2.zero, Vector2.one,
                20, TextAnchor.MiddleCenter, "!");
            mark.fontStyle = FontStyle.Bold;
            mark.color = AlertRed;
            mark.horizontalOverflow = HorizontalWrapMode.Overflow;
            mark.verticalOverflow = VerticalWrapMode.Overflow;
            mark.raycastTarget = false;

            _phantasiaAlertRoot.gameObject.SetActive(false);
        }

        void BuildLockedContinentButtons()
        {
            _lockedContinentsRoot = UiPanelFactory.CreateRect(_mapRoot, "LockedContinents",
                Vector2.zero, Vector2.one);
            _lockedContinentsRoot.gameObject.SetActive(false);
            _lockedContinentsGroup = _lockedContinentsRoot.gameObject.AddComponent<CanvasGroup>();
            _lockedContinentsGroup.alpha = 0f;
            _lockedContinentsGroup.interactable = false;
            _lockedContinentsGroup.blocksRaycasts = false;

            CreateLockedContinentButton(LockedContinentAnchorTopX, LockedContinentAnchorTopY, "Tactilis");
            CreateLockedContinentButton(LockedContinentAnchorRightX, LockedContinentAnchorRightY, "Volition");
            CreateLockedContinentButton(LockedContinentAnchorBottomX, LockedContinentAnchorBottomY, "Remoria");
            CreateLockedContinentButton(LockedContinentAnchorBottomRightX, LockedContinentAnchorBottomRightY, "Wayrest");
        }

        void CreateLockedContinentButton(float anchorX, float anchorY, string labelText)
        {
            var marker = UiPanelFactory.CreateRect(_lockedContinentsRoot, "LockedContinent",
                new Vector2(anchorX, anchorY), new Vector2(anchorX, anchorY));
            marker.pivot = new Vector2(0.5f, 0.5f);
            marker.sizeDelta = Vector2.zero;
            _lockedContinentMarkers.Add(marker);

            var buttonRect = UiPanelFactory.CreateRect(marker, "Button",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonRect.sizeDelta = new Vector2(ContinentButtonSize, ContinentButtonSize);

            var circle = CreateCircleSprite();
            var fill = buttonRect.gameObject.AddComponent<Image>();
            fill.sprite = circle;
            fill.color = LockedButtonFill;
            fill.preserveAspect = true;
            fill.raycastTarget = true;

            var ring = UiPanelFactory.CreateImage(buttonRect, "Ring", Vector2.zero, Vector2.one, LockedButtonRing);
            ring.sprite = circle;
            ring.preserveAspect = true;
            ring.raycastTarget = false;
            var ringRect = ring.rectTransform;
            ringRect.offsetMin = new Vector2(-8f, -8f);
            ringRect.offsetMax = new Vector2(8f, 8f);
            ring.transform.SetAsFirstSibling();

            var button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = fill;
            button.onClick.AddListener(ShowLockedPopup);

            var labelTop = ContinentButtonSize * 0.5f + ContinentLabelGap + ContinentLabelHeight;
            var label = UiPanelFactory.CreateText(marker, "ContinentLabel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                ContinentLabelFontSize, TextAnchor.MiddleCenter, labelText);
            var labelRect = label.rectTransform;
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(ContinentLabelWidth, ContinentLabelHeight);
            labelRect.anchoredPosition = new Vector2(0f, labelTop - ContinentLabelHeight);
            StyleWhiteMapText(label);
            label.raycastTarget = false;
        }

        void BuildLockedPopup(Transform canvasTransform)
        {
            _lockedPopupRoot = UiPanelFactory.EnsureOverlayCanvas(canvasTransform, "LockedPopupOverlay", 250)
                .GetComponent<RectTransform>();

            var dim = UiPanelFactory.CreateRect(_lockedPopupRoot, "LockedPopupDim", Vector2.zero, Vector2.one);
            var dimImg = dim.gameObject.AddComponent<Image>();
            dimImg.color = UiPanelFactory.DimOverlay;
            dimImg.raycastTarget = true;

            var panel = UiPanelFactory.CreateRect(_lockedPopupRoot, "LockedPopupPanel",
                new Vector2(0.34f, 0.44f), new Vector2(0.66f, 0.56f));
            var panelBg = panel.gameObject.AddComponent<Image>();
            panelBg.color = UiPanelFactory.PanelBg;

            var message = UiPanelFactory.CreateText(panel, "LockedMessage",
                new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.82f),
                28, TextAnchor.MiddleCenter, "Locked");
            message.fontStyle = FontStyle.Bold;
            message.color = Color.white;

            _lockedPopupRoot.gameObject.SetActive(false);
        }

        void BuildCloudTransition(Transform canvasTransform)
        {
            var canvas = UiPanelFactory.EnsureOverlayCanvas(canvasTransform, "CloudTransitionOverlay", 300);
            _cloudTransitionRoot = canvas.GetComponent<RectTransform>();
            _cloudTransitionGroup = _cloudTransitionRoot.gameObject.AddComponent<CanvasGroup>();
            _cloudTransitionGroup.alpha = 1f;
            _cloudTransitionGroup.interactable = true;
            _cloudTransitionGroup.blocksRaycasts = true;

            var sky = UiPanelFactory.CreateImage(_cloudTransitionRoot, "Sky", Vector2.zero, Vector2.one,
                TransitionBackgroundColor);
            sky.raycastTarget = true;
            _cloudSkyBackground = sky;

            _leftRevealRect = UiPanelFactory.CreateRect(_cloudTransitionRoot, "LeftReveal",
                new Vector2(0f, 0f), new Vector2(0f, 1f));
            _leftRevealRect.pivot = new Vector2(0f, 0.5f);
            _leftRevealRect.anchoredPosition = Vector2.zero;
            _leftRevealRect.sizeDelta = Vector2.zero;
            var leftRevealImage = _leftRevealRect.gameObject.AddComponent<Image>();
            leftRevealImage.color = MapBackgroundColor;
            leftRevealImage.raycastTarget = false;

            _rightRevealRect = UiPanelFactory.CreateRect(_cloudTransitionRoot, "RightReveal",
                new Vector2(1f, 0f), new Vector2(1f, 1f));
            _rightRevealRect.pivot = new Vector2(1f, 0.5f);
            _rightRevealRect.anchoredPosition = Vector2.zero;
            _rightRevealRect.sizeDelta = Vector2.zero;
            var rightRevealImage = _rightRevealRect.gameObject.AddComponent<Image>();
            rightRevealImage.color = MapBackgroundColor;
            rightRevealImage.raycastTarget = false;

            var layer = UiPanelFactory.CreateRect(_cloudTransitionRoot, "CloudLayer", Vector2.zero, Vector2.one);
            _cloudLayer = layer;
            var cloudCanvas = layer.gameObject.AddComponent<Canvas>();
            cloudCanvas.overrideSorting = true;
            cloudCanvas.sortingOrder = 301;
            layer.gameObject.AddComponent<GraphicRaycaster>();

            var cloudSprite = CreateCloudSprite();

            for (var i = 0; i < CloudTransitionRowCount; i++)
            {
                var y = Mathf.Lerp(CloudTransitionLowY, CloudTransitionHighY, (i + 0.5f) / CloudTransitionRowCount);
                var width = 520f + (i % 3) * 70f;
                // Inward first: start off-screen, end on the opposite side (midpoint = center meet).
                CreateTransitionCloud(layer, cloudSprite, $"CloudLeft{i}", y, width,
                    -CloudTransitionOffscreenX, CloudTransitionOffscreenX, true);
                CreateTransitionCloud(layer, cloudSprite, $"CloudRight{i}", y, width,
                    CloudTransitionOffscreenX, -CloudTransitionOffscreenX, false);
            }

            for (var i = 0; i < CloudTransitionSecondRowCount; i++)
            {
                var y = Mathf.Lerp(CloudTransitionLowY, CloudTransitionHighY, (i + 0.5f) / CloudTransitionSecondRowCount)
                        + CloudTransitionSecondRowYOffset;
                var width = 520f + (i % 3) * 70f;
                var xOffset = CloudTransitionSecondRowXOffset;
                CreateTransitionCloud(layer, cloudSprite, $"CloudLeftB{i}", y, width,
                    -CloudTransitionOffscreenX + xOffset, CloudTransitionOffscreenX + xOffset, true);
                CreateTransitionCloud(layer, cloudSprite, $"CloudRightB{i}", y, width,
                    CloudTransitionOffscreenX - xOffset, -CloudTransitionOffscreenX - xOffset, false);
            }

            _cloudLayer.SetAsLastSibling();
            _cloudTransitionRoot.gameObject.SetActive(false);
        }

        void CreateTransitionCloud(Transform layer, Sprite cloudSprite, string name, float y, float width,
            float startX, float endX, bool fromLeft)
        {
            var cloudHeight = width * 0.48f;
            var cloudRoot = UiPanelFactory.CreateRect(layer, name,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            cloudRoot.pivot = new Vector2(0.5f, 0.5f);
            cloudRoot.sizeDelta = Vector2.zero;
            cloudRoot.anchoredPosition = new Vector2(startX, y);

            var outlineRect = UiPanelFactory.CreateRect(cloudRoot, "Outline",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            outlineRect.pivot = new Vector2(0.5f, 0.5f);
            outlineRect.anchoredPosition = Vector2.zero;
            outlineRect.sizeDelta = new Vector2(width * CloudOutlineScale, cloudHeight * CloudOutlineScale);
            var outlineImg = outlineRect.gameObject.AddComponent<Image>();
            outlineImg.sprite = cloudSprite;
            outlineImg.color = CloudOutlineColor;
            outlineImg.preserveAspect = true;
            outlineImg.raycastTarget = false;
            outlineRect.SetAsFirstSibling();

            var fillRect = UiPanelFactory.CreateRect(cloudRoot, "Fill",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            fillRect.pivot = new Vector2(0.5f, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = new Vector2(width, cloudHeight);
            var fillImg = fillRect.gameObject.AddComponent<Image>();
            fillImg.sprite = cloudSprite;
            fillImg.color = Color.white;
            fillImg.preserveAspect = true;
            fillImg.raycastTarget = false;

            _transitionClouds.Add(new TransitionCloud
            {
                Rect = cloudRoot,
                StartX = startX,
                EndX = endX,
                Y = y,
                Delay = 0f,
                FromLeft = fromLeft
            });
        }

        void UpdateCloudReveal(float leftCloudX, float rightCloudX)
        {
            if (_leftRevealRect != null)
            {
                var leftWidth = Mathf.Clamp(leftCloudX + CloudTransitionScreenHalfWidth, 0f,
                    CloudTransitionScreenHalfWidth * 2f);
                _leftRevealRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, leftWidth);
            }

            if (_rightRevealRect != null)
            {
                var rightWidth = Mathf.Clamp(CloudTransitionScreenHalfWidth - rightCloudX, 0f,
                    CloudTransitionScreenHalfWidth * 2f);
                _rightRevealRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rightWidth);
            }
        }

        static float EvaluateCloudX(float startX, float endX, float t)
        {
            var midX = (startX + endX) * 0.5f;
            if (t <= CloudTransitionInwardPortion)
            {
                var u = Mathf.SmoothStep(0f, 1f, t / CloudTransitionInwardPortion);
                return Mathf.Lerp(startX, midX, u);
            }

            var outwardT = (t - CloudTransitionInwardPortion) / (1f - CloudTransitionInwardPortion);
            var v = Mathf.SmoothStep(0f, 1f, outwardT);
            return Mathf.Lerp(midX, endX, v);
        }

        void ShowLockedPopup()
        {
            if (_lockedPopupRoot == null)
                return;

            if (_lockedPopupRoutine != null)
            {
                StopCoroutine(_lockedPopupRoutine);
                _lockedPopupRoutine = null;
            }

            _lockedPopupRoot.gameObject.SetActive(true);
            _lockedPopupRoot.SetAsLastSibling();
            _lockedPopupRoutine = StartCoroutine(HideLockedPopupAfterDelay());
        }

        IEnumerator HideLockedPopupAfterDelay()
        {
            yield return new WaitForSecondsRealtime(LockedPopupDuration);
            if (_lockedPopupRoot != null)
                _lockedPopupRoot.gameObject.SetActive(false);
            _lockedPopupRoutine = null;
        }

        void BuildLevelMarker()
        {
            _levelMarkerRoot = UiPanelFactory.CreateRect(_mapRoot, "LevelMarker",
                new Vector2(LevelAnchorX, LevelAnchorY),
                new Vector2(LevelAnchorX, LevelAnchorY));
            _levelMarkerRoot.pivot = new Vector2(0.5f, 0.5f);
            _levelMarkerRoot.sizeDelta = Vector2.zero;
            _levelMarkerRoot.gameObject.SetActive(false);

            _levelButtonRect = UiPanelFactory.CreateRect(_levelMarkerRoot, "LevelButton",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _levelButtonRect.pivot = new Vector2(0.5f, 0.5f);
            _levelButtonRect.anchoredPosition = Vector2.zero;
            _levelButtonRect.sizeDelta = new Vector2(LevelButtonSize, LevelButtonSize);

            var circle = CreateCircleSprite();
            _levelButtonFill = _levelButtonRect.gameObject.AddComponent<Image>();
            _levelButtonFill.sprite = circle;
            _levelButtonFill.color = Color.white;
            _levelButtonFill.preserveAspect = true;
            _levelButtonFill.raycastTarget = true;

            _levelButtonRing = UiPanelFactory.CreateImage(_levelButtonRect, "Ring", Vector2.zero, Vector2.one, Color.white);
            _levelButtonRing.sprite = circle;
            _levelButtonRing.preserveAspect = true;
            _levelButtonRing.raycastTarget = false;
            var ringRect = _levelButtonRing.rectTransform;
            ringRect.offsetMin = new Vector2(-5f, -5f);
            ringRect.offsetMax = new Vector2(5f, 5f);
            _levelButtonRing.transform.SetAsFirstSibling();

            _levelButton = _levelButtonRect.gameObject.AddComponent<Button>();
            _levelButton.targetGraphic = _levelButtonFill;
            _levelButton.onClick.AddListener(HandleLevelButtonClicked);

            BuildLevelCrackVisuals();

            var labelTop = LevelButtonSize * 0.5f + LevelLabelGap + LevelLabelHeight;
            _levelLabelText = UiPanelFactory.CreateText(_levelMarkerRoot, "LevelLabel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                15, TextAnchor.MiddleCenter, LevelLabelText);
            var labelRect = _levelLabelText.rectTransform;
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(LevelLabelWidth, LevelLabelHeight);
            labelRect.anchoredPosition = new Vector2(0f, labelTop - LevelLabelHeight);
            _levelLabelText.fontStyle = FontStyle.Bold;
            StyleWhiteMapText(_levelLabelText);
            _levelLabelText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _levelLabelText.verticalOverflow = VerticalWrapMode.Overflow;
            _levelLabelText.raycastTarget = false;

            BuildStargazerRecallDialog();
        }

        void BuildStargazerRecallDialog()
        {
            var dialogTop = -(LevelButtonSize * 0.5f + StargazerRecallDialogGap);
            _stargazerRecallDialogRoot = UiPanelFactory.CreateRect(_levelMarkerRoot, "StargazerRecallDialog",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _stargazerRecallDialogRoot.pivot = new Vector2(0.5f, 1f);
            _stargazerRecallDialogRoot.anchoredPosition = new Vector2(0f, dialogTop);
            _stargazerRecallDialogRoot.sizeDelta = new Vector2(StargazerRecallDialogWidth, StargazerRecallDialogHeight);

            var panelBg = _stargazerRecallDialogRoot.gameObject.AddComponent<Image>();
            panelBg.color = UiPanelFactory.PanelBg;
            panelBg.raycastTarget = false;

            var message = UiPanelFactory.CreateText(_stargazerRecallDialogRoot, "StargazerRecallMessage",
                new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.88f),
                StargazerRecallDialogFontSize, TextAnchor.MiddleCenter, StargazerRecallDialogText);
            message.fontStyle = FontStyle.Italic;
            message.color = Color.white;
            message.horizontalOverflow = HorizontalWrapMode.Wrap;
            message.verticalOverflow = VerticalWrapMode.Overflow;
            message.raycastTarget = false;

            _stargazerRecallDialogRoot.gameObject.SetActive(false);
        }

        void BuildLevelCrackVisuals()
        {
            _levelCrackRoot = UiPanelFactory.CreateRect(_levelButtonRect, "Cracks", Vector2.zero, Vector2.one);
            _levelCrackRoot.gameObject.SetActive(false);

            for (var i = 0; i < _levelCrackImages.Length; i++)
            {
                var crack = UiPanelFactory.CreateImage(_levelCrackRoot, $"Crack{i}", Vector2.zero, Vector2.one,
                    new Color(0.12f, 0.12f, 0.14f, 1f));
                crack.sprite = CreateCrackSprite(i);
                crack.preserveAspect = true;
                crack.raycastTarget = false;
                crack.enabled = false;
                _levelCrackImages[i] = crack;
            }

            _levelHoleRect = UiPanelFactory.CreateRect(_levelMarkerRoot, "CrackHole",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _levelHoleRect.pivot = new Vector2(0.5f, 0.5f);
            _levelHoleRect.anchoredPosition = Vector2.zero;
            _levelHoleRect.sizeDelta = Vector2.zero;
            _levelHoleImage = _levelHoleRect.gameObject.AddComponent<Image>();
            _levelHoleImage.sprite = CreateCrackedHoleSprite();
            _levelHoleImage.color = Color.black;
            _levelHoleImage.preserveAspect = true;
            _levelHoleImage.raycastTarget = false;
            _levelHoleRect.gameObject.SetActive(false);

            var shardAngles = new[] { 20f, 140f, 250f };
            var shardOffsets = new[]
            {
                new Vector2(4f, 6f),
                new Vector2(-7f, 2f),
                new Vector2(1f, -7f)
            };
            for (var i = 0; i < shardAngles.Length; i++)
            {
                var shard = UiPanelFactory.CreateRect(_levelMarkerRoot, $"BreakShard{i}",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                shard.pivot = new Vector2(0.5f, 0.5f);
                shard.anchoredPosition = shardOffsets[i];
                shard.sizeDelta = new Vector2(LevelButtonSize * 0.42f, LevelButtonSize * 0.42f);
                shard.localRotation = Quaternion.Euler(0f, 0f, shardAngles[i]);
                var shardImg = shard.gameObject.AddComponent<Image>();
                shardImg.sprite = CreateCircleSprite();
                shardImg.color = Color.white;
                shardImg.preserveAspect = true;
                shardImg.raycastTarget = false;
                shard.gameObject.SetActive(false);
                _levelBreakShards.Add(shard);
            }
        }

        void BuildLevelEnterBlackout(Transform canvasTransform)
        {
            _levelEnterBlackout = UiPanelFactory.CreateImage(canvasTransform, "LevelEnterBlackout",
                Vector2.zero, Vector2.one, Color.black);
            _levelEnterBlackout.raycastTarget = true;
            var group = _levelEnterBlackout.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            _levelEnterBlackout.gameObject.SetActive(false);
        }

        IEnumerator Boot()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            FitMapToViewport();
            SetMapContentAlpha(0f);
            yield return PlayCloudTransition();
            SetMapContentAlpha(1f);
            yield return RevealContinentButton();

            if (WorldMapProgress.ConsumeStargazerCelebration())
                yield return PlayStargazerCompletionSequence();
            else
                RefreshPhantasiaAlert();
        }

        IEnumerator PlayStargazerCompletionSequence()
        {
            _busy = true;
            yield return ShowStargazerCompleteCheck();

            if (!WorldMapProgress.IsContinentComplete(WorldMapProgress.Continent.DreamIsles))
            {
                yield return FlyUnlockStar();
                WorldMapProgress.EnablePhantasiaAlert();
                RefreshPhantasiaAlert(true);
            }

            _busy = false;
        }

        IEnumerator ShowStargazerCompleteCheck()
        {
            if (_stargazerCompleteCheck == null)
                yield break;

            var rect = _stargazerCompleteCheck.rectTransform;
            _stargazerCompleteCheck.gameObject.SetActive(true);
            rect.SetAsLastSibling();
            _stargazerCompleteCheck.color = Color.white;

            var t = 0f;
            while (t < CompletionCheckPopDuration)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.Clamp01(t / CompletionCheckPopDuration);
                rect.localScale = Vector3.one * EvaluatePopScale(u);
                yield return null;
            }

            rect.localScale = Vector3.one;
            yield return new WaitForSecondsRealtime(CompletionCheckHoldDuration);

            t = 0f;
            while (t < CompletionCheckFadeDuration)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.Clamp01(t / CompletionCheckFadeDuration);
                _stargazerCompleteCheck.color = new Color(1f, 1f, 1f, 1f - u);
                yield return null;
            }

            _stargazerCompleteCheck.gameObject.SetActive(false);
        }

        IEnumerator FlyUnlockStar()
        {
            if (_mapRoot == null)
                yield break;

            var from = new Vector2(StargazerContinentButtonAnchorX, StargazerContinentButtonAnchorY);
            var to = new Vector2(ContinentButtonAnchorX, ContinentButtonAnchorY);

            var star = UiPanelFactory.CreateRect(_mapRoot, "UnlockStar", from, from);
            star.pivot = new Vector2(0.5f, 0.5f);
            star.anchoredPosition = Vector2.zero;
            star.sizeDelta = new Vector2(UnlockStarSize, UnlockStarSize);
            star.SetAsLastSibling();
            var starImage = star.gameObject.AddComponent<Image>();
            starImage.sprite = CreateStarSprite();
            starImage.color = UnlockStarColor;
            starImage.preserveAspect = true;
            starImage.raycastTarget = false;

            var t = 0f;
            while (t < UnlockStarFlightDuration)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.Clamp01(t / UnlockStarFlightDuration);
                var eased = Mathf.SmoothStep(0f, 1f, u);
                var anchor = Vector2.Lerp(from, to, eased);
                star.anchorMin = anchor;
                star.anchorMax = anchor;
                star.anchoredPosition = new Vector2(0f, Mathf.Sin(eased * Mathf.PI) * UnlockStarArcHeight);
                star.localRotation = Quaternion.Euler(0f, 0f, -UnlockStarSpinDegrees * eased);
                star.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, Mathf.Sin(u * Mathf.PI * 0.5f));
                yield return null;
            }

            star.anchorMin = to;
            star.anchorMax = to;
            star.anchoredPosition = Vector2.zero;

            t = 0f;
            while (t < UnlockStarBurstDuration)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.Clamp01(t / UnlockStarBurstDuration);
                star.localScale = Vector3.one * Mathf.Lerp(1f, 2f, u);
                starImage.color = new Color(UnlockStarColor.r, UnlockStarColor.g, UnlockStarColor.b, 1f - u);
                yield return null;
            }

            Destroy(star.gameObject);
        }

        void RefreshPhantasiaAlert(bool popIn = false)
        {
            if (_phantasiaAlertRoot == null)
                return;

            var show = WorldMapProgress.ShouldShowPhantasiaAlert;
            _phantasiaAlertRoot.gameObject.SetActive(show);
            if (!show)
            {
                if (_phantasiaAlertRoutine != null)
                {
                    StopCoroutine(_phantasiaAlertRoutine);
                    _phantasiaAlertRoutine = null;
                }

                return;
            }

            _phantasiaAlertRoot.SetAsLastSibling();
            if (_phantasiaAlertRoutine == null || popIn)
            {
                if (_phantasiaAlertRoutine != null)
                    StopCoroutine(_phantasiaAlertRoutine);
                _phantasiaAlertRoutine = StartCoroutine(PulsePhantasiaAlert(popIn));
            }
        }

        IEnumerator PulsePhantasiaAlert(bool popIn)
        {
            if (popIn)
            {
                var p = 0f;
                while (p < AlertBadgePopDuration && _phantasiaAlertRoot != null)
                {
                    p += Time.unscaledDeltaTime;
                    _phantasiaAlertRoot.localScale =
                        Vector3.one * EvaluatePopScale(Mathf.Clamp01(p / AlertBadgePopDuration));
                    yield return null;
                }
            }

            var t = 0f;
            while (_phantasiaAlertRoot != null)
            {
                t += Time.unscaledDeltaTime;
                var u = (Mathf.Sin(t / AlertPulsePeriod * Mathf.PI * 2f - Mathf.PI * 0.5f) + 1f) * 0.5f;
                _phantasiaAlertRoot.localScale = Vector3.one * Mathf.Lerp(1f, AlertPulseScale, u);
                yield return null;
            }
        }

        static float EvaluatePopScale(float u) =>
            u < 0.7f
                ? Mathf.Lerp(0f, 1.2f, Mathf.SmoothStep(0f, 1f, u / 0.7f))
                : Mathf.Lerp(1.2f, 1f, (u - 0.7f) / 0.3f);

        IEnumerator PlayCloudTransition()
        {
            if (_cloudTransitionRoot == null || _transitionClouds.Count == 0)
                yield break;

            _cloudTransitionRoot.gameObject.SetActive(true);
            _cloudTransitionRoot.SetAsLastSibling();
            if (_cloudTransitionGroup != null)
            {
                _cloudTransitionGroup.alpha = 1f;
                _cloudTransitionGroup.blocksRaycasts = true;
            }

            foreach (var cloud in _transitionClouds)
            {
                if (cloud.Rect != null)
                    cloud.Rect.anchoredPosition = new Vector2(cloud.StartX, cloud.Y);
            }

            UpdateCloudReveal(-CloudTransitionOffscreenX, CloudTransitionOffscreenX);

            var totalTime = CloudTransitionDuration;
            var elapsed = 0f;
            var leftCloudX = -CloudTransitionOffscreenX;
            var rightCloudX = CloudTransitionOffscreenX;

            while (elapsed < totalTime)
            {
                elapsed += Time.unscaledDeltaTime;
                leftCloudX = -CloudTransitionOffscreenX;
                rightCloudX = CloudTransitionOffscreenX;

                foreach (var cloud in _transitionClouds)
                {
                    if (cloud.Rect == null)
                        continue;

                    var localTime = elapsed - cloud.Delay;
                    float x;
                    if (localTime <= 0f)
                    {
                        x = cloud.StartX;
                    }
                    else
                    {
                        // Spend the first portion moving inward to center, then continue outward
                        // off the opposite edge. SmoothStep on each half kept the start off-screen
                        // too long, so clouds looked like they spawned at center and only exited.
                        var t = Mathf.Clamp01(localTime / CloudTransitionDuration);
                        x = EvaluateCloudX(cloud.StartX, cloud.EndX, t);
                    }

                    cloud.Rect.anchoredPosition = new Vector2(x, cloud.Y);

                    if (cloud.FromLeft)
                        leftCloudX = Mathf.Max(leftCloudX, x);
                    else
                        rightCloudX = Mathf.Min(rightCloudX, x);
                }

                UpdateCloudReveal(leftCloudX, rightCloudX);
                if (_cloudLayer != null)
                    _cloudLayer.SetAsLastSibling();
                SetMapContentAlpha(0f);

                yield return null;
            }

            UpdateCloudReveal(CloudTransitionOffscreenX, -CloudTransitionOffscreenX);
            ApplyBackgroundColor(MapBackgroundColor);

            var fadeT = 0f;
            while (fadeT < CloudOverlayFadeDuration)
            {
                fadeT += Time.unscaledDeltaTime;
                if (_cloudTransitionGroup != null)
                    _cloudTransitionGroup.alpha = 1f - Mathf.Clamp01(fadeT / CloudOverlayFadeDuration);
                yield return null;
            }

            _cloudTransitionRoot.gameObject.SetActive(false);

            fadeT = 0f;
            while (fadeT < MapRevealAfterCloudsDuration)
            {
                fadeT += Time.unscaledDeltaTime;
                SetMapContentAlpha(Mathf.Clamp01(fadeT / MapRevealAfterCloudsDuration));
                yield return null;
            }

            SetMapContentAlpha(1f);
        }

        void SetMapContentAlpha(float alpha)
        {
            if (_mapContentGroup != null)
            {
                _mapContentGroup.alpha = alpha;
                var visible = alpha > 0.01f;
                _mapContentGroup.interactable = visible;
                _mapContentGroup.blocksRaycasts = visible;
            }

            if (_mapTitleGroup != null)
            {
                _mapTitleGroup.alpha = alpha;
                var visible = alpha > 0.01f;
                _mapTitleGroup.interactable = visible;
                _mapTitleGroup.blocksRaycasts = visible;
            }

            ApplyBackgroundColor(Color.Lerp(TransitionBackgroundColor, MapBackgroundColor, alpha));
        }

        void ApplyBackgroundColor(Color color)
        {
            if (_oceanBackground != null)
                _oceanBackground.color = color;

            if (_cloudSkyBackground != null)
                _cloudSkyBackground.color = color;

            var cam = Camera.main;
            if (cam != null)
                cam.backgroundColor = color;
        }

        IEnumerator RevealContinentButton()
        {
            yield return new WaitForSecondsRealtime(ButtonAppearDelay);
            if (_continentButtonGroup == null || _continentMarkerRoot == null)
                yield break;

            _continentMarkerRoot.gameObject.SetActive(true);
            if (_stargazerContinentMarkerRoot != null)
                _stargazerContinentMarkerRoot.gameObject.SetActive(true);
            if (_lockedContinentsRoot != null)
                _lockedContinentsRoot.gameObject.SetActive(true);

            _continentButtonGroup.alpha = 0f;
            _continentButtonGroup.interactable = false;
            _continentButtonGroup.blocksRaycasts = false;
            if (_stargazerContinentButtonGroup != null)
            {
                _stargazerContinentButtonGroup.alpha = 0f;
                _stargazerContinentButtonGroup.interactable = false;
                _stargazerContinentButtonGroup.blocksRaycasts = false;
            }
            if (_lockedContinentsGroup != null)
            {
                _lockedContinentsGroup.alpha = 0f;
                _lockedContinentsGroup.interactable = false;
                _lockedContinentsGroup.blocksRaycasts = false;
            }

            var t = 0f;
            while (t < ButtonFadeDuration)
            {
                t += Time.unscaledDeltaTime;
                var alpha = Mathf.Clamp01(t / ButtonFadeDuration);
                _continentButtonGroup.alpha = alpha;
                if (_stargazerContinentButtonGroup != null)
                    _stargazerContinentButtonGroup.alpha = alpha;
                if (_lockedContinentsGroup != null)
                    _lockedContinentsGroup.alpha = alpha;
                yield return null;
            }

            SetOverviewMarkersVisible(true);
            RefreshOverviewButtonColors();
            StartContinentPulse();
        }

        void RefreshOverviewButtonColors()
        {
            ApplyMapButtonColors(
                _continentButtonFill,
                _continentButtonRing,
                WorldMapProgress.GetContinentButtonFill(WorldMapProgress.Continent.DreamIsles),
                WorldMapProgress.GetContinentButtonRing(WorldMapProgress.Continent.DreamIsles));
            ApplyMapButtonColors(
                _stargazerContinentButtonFill,
                _stargazerContinentButtonRing,
                WorldMapProgress.GetContinentButtonFill(WorldMapProgress.Continent.Stargazer),
                WorldMapProgress.GetContinentButtonRing(WorldMapProgress.Continent.Stargazer));
        }

        void RefreshActiveLevelButtonColors()
        {
            var levelId = WorldMapProgress.GetLevelForRegion(_zoomedRegion == MapRegion.Stargazer);
            ApplyMapButtonColors(
                _levelButtonFill,
                _levelButtonRing,
                WorldMapProgress.GetLevelButtonFill(levelId),
                WorldMapProgress.GetLevelButtonRing(levelId));
        }

        static void ApplyMapButtonColors(Image fill, Image ring, Color fillColor, Color ringColor)
        {
            if (fill != null)
                fill.color = fillColor;

            if (ring != null)
                ring.color = ringColor;
        }

        void StartContinentPulse()
        {
            StopPulse();

            if (WorldMapProgress.IsContinentComplete(WorldMapProgress.Continent.DreamIsles))
                return;

            _pulseRoutine = StartCoroutine(PulseOverviewContinentButtons());
        }

        IEnumerator PulseOverviewContinentButtons()
        {
            while (!_zoomed)
            {
                var t = 0f;
                const float period = 1.1f;
                while (t < period && !_zoomed)
                {
                    t += Time.unscaledDeltaTime;
                    var u = (Mathf.Sin(t / period * Mathf.PI * 2f) + 1f) * 0.5f;
                    var scale = Mathf.Lerp(1f, 1.12f, u);
                    ApplyPulseScale(_continentButtonRect, scale);
                    yield return null;
                }
            }

            ApplyPulseScale(_continentButtonRect, 1f);
            ApplyPulseScale(_stargazerContinentButtonRect, 1f);
        }

        static void ApplyPulseScale(RectTransform buttonRect, float scale)
        {
            if (buttonRect == null)
                return;

            buttonRect.localScale = new Vector3(scale, scale, 1f);
        }

        void StartLevelPulse()
        {
            StopPulse();
            if (_levelButtonRect != null)
                _levelButtonRect.localScale = Vector3.one;

            if (_zoomedRegion == MapRegion.Stargazer)
                return;

            var levelId = WorldMapProgress.GetLevelForRegion(false);
            if (WorldMapProgress.IsLevelBeaten(levelId))
                return;

            _pulseRoutine = StartCoroutine(PulseButton(_levelButtonRect, () => _zoomed && !_busy));
        }

        void StopPulse()
        {
            if (_pulseRoutine == null)
                return;

            StopCoroutine(_pulseRoutine);
            _pulseRoutine = null;
        }

        static IEnumerator PulseButton(RectTransform buttonRect, System.Func<bool> keepPulsing)
        {
            while (keepPulsing() && buttonRect != null)
            {
                var t = 0f;
                const float period = 1.1f;
                while (t < period && keepPulsing())
                {
                    t += Time.unscaledDeltaTime;
                    var u = (Mathf.Sin(t / period * Mathf.PI * 2f) + 1f) * 0.5f;
                    var scale = Mathf.Lerp(1f, 1.12f, u);
                    buttonRect.localScale = new Vector3(scale, scale, 1f);
                    yield return null;
                }
            }

            if (buttonRect != null)
                buttonRect.localScale = Vector3.one;
        }

        void HandleContinentButtonClicked()
        {
            if (_busy || _zoomed || _continentMarkerRoot == null || !_continentMarkerRoot.gameObject.activeInHierarchy)
                return;

            _zoomedRegion = MapRegion.DreamIsles;
            SetLockedContinentsVisible(false);
            StartCoroutine(ZoomIn());
        }

        void HandleStargazerContinentButtonClicked()
        {
            if (_busy || _zoomed || _stargazerContinentMarkerRoot == null
                || !_stargazerContinentMarkerRoot.gameObject.activeInHierarchy)
                return;

            _zoomedRegion = MapRegion.Stargazer;
            SetLockedContinentsVisible(false);
            StartCoroutine(ZoomIn());
        }

        void HandleLevelButtonClicked()
        {
            if (_busy || _levelEnterTransition || !_zoomed)
                return;

            StartCoroutine(PlayLevelEnterTransition());
        }

        IEnumerator PlayLevelEnterTransition()
        {
            _levelEnterTransition = true;
            if (_levelButton != null)
                _levelButton.interactable = false;

            yield return null;

            _busy = true;
            _levelEnterTransition = false;
            StopPulse();
            if (_levelButtonRect != null)
                _levelButtonRect.localScale = Vector3.one;

            if (_levelLabelText != null)
                _levelLabelText.gameObject.SetActive(false);

            SetStargazerRecallDialogVisible(false);

            if (_levelCrackRoot != null)
                _levelCrackRoot.gameObject.SetActive(true);

            for (var i = 0; i < _levelCrackImages.Length; i++)
            {
                if (_levelCrackImages[i] != null)
                    _levelCrackImages[i].enabled = true;
                yield return new WaitForSecondsRealtime(LevelCrackStepDelay);
            }

            yield return PlayLevelButtonBreak();
            yield return CoverButtonWithCrackHole();
            yield return ZoomIntoCrackHole();
            yield return FadeToBlackThenLoad();
        }

        IEnumerator PlayLevelButtonBreak()
        {
            var startPositions = new List<Vector2>(_levelBreakShards.Count);
            var endPositions = new List<Vector2>(_levelBreakShards.Count);
            for (var i = 0; i < _levelBreakShards.Count; i++)
            {
                var shard = _levelBreakShards[i];
                shard.gameObject.SetActive(true);
                startPositions.Add(shard.anchoredPosition);
                endPositions.Add(shard.anchoredPosition * 3.2f);
                shard.localScale = Vector3.one * 0.85f;
            }

            if (_levelButtonFill != null)
                _levelButtonFill.enabled = false;
            if (_levelButtonRing != null)
                _levelButtonRing.enabled = false;
            if (_levelCrackRoot != null)
                _levelCrackRoot.gameObject.SetActive(false);

            var t = 0f;
            while (t < LevelBreakDuration)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / LevelBreakDuration));
                for (var i = 0; i < _levelBreakShards.Count; i++)
                {
                    var shard = _levelBreakShards[i];
                    shard.anchoredPosition = Vector2.LerpUnclamped(startPositions[i], endPositions[i], u);
                    shard.localScale = Vector3.one * Mathf.Lerp(0.85f, 0.35f, u);
                    var img = shard.GetComponent<Image>();
                    if (img != null)
                    {
                        var c = img.color;
                        c.a = 1f - u;
                        img.color = c;
                    }
                }

                yield return null;
            }

            foreach (var shard in _levelBreakShards)
                shard.gameObject.SetActive(false);

            if (_levelButtonRect != null)
                _levelButtonRect.gameObject.SetActive(false);
        }

        IEnumerator CoverButtonWithCrackHole()
        {
            if (_levelHoleRect == null || _levelHoleImage == null)
                yield break;

            _levelHoleRect.gameObject.SetActive(true);
            _levelHoleRect.SetAsLastSibling();
            _levelHoleRect.sizeDelta = Vector2.zero;
            _levelHoleImage.color = Color.black;

            var endSize = LevelButtonSize * 1.35f;
            var t = 0f;
            while (t < LevelHoleCoverDuration)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / LevelHoleCoverDuration));
                var size = Mathf.Lerp(LevelButtonSize * 0.2f, endSize, u);
                _levelHoleRect.sizeDelta = new Vector2(size, size);
                yield return null;
            }

            _levelHoleRect.sizeDelta = new Vector2(endSize, endSize);
            yield return new WaitForSecondsRealtime(0.12f);
        }

        IEnumerator ZoomIntoCrackHole()
        {
            if (_mapRoot == null || _levelHoleRect == null)
                yield break;

            Canvas.ForceUpdateCanvases();
            FitMapToViewport();

            var mapSize = _mapRoot.rect.size;
            var buttonLocal = new Vector2(
                (ActiveLevelAnchorX - 0.5f) * mapSize.x,
                (ActiveLevelAnchorY - 0.5f) * mapSize.y);
            var startPos = _mapRoot.anchoredPosition;
            var endPos = -buttonLocal * LevelCrackZoomScale;
            var startScale = _mapRoot.localScale;
            var endScale = Vector3.one * LevelCrackZoomScale;
            var startHoleSize = _levelHoleRect.sizeDelta.x;
            var endHoleSize = LevelButtonSize * 18f;

            var t = 0f;
            while (t < LevelCrackZoomDuration)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / LevelCrackZoomDuration));
                _mapRoot.localScale = Vector3.LerpUnclamped(startScale, endScale, u);
                _mapRoot.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, u);
                var holeSize = Mathf.Lerp(startHoleSize, endHoleSize, u);
                _levelHoleRect.sizeDelta = new Vector2(holeSize, holeSize);
                yield return null;
            }

            _mapRoot.localScale = endScale;
            _mapRoot.anchoredPosition = endPos;
            _levelHoleRect.sizeDelta = new Vector2(endHoleSize, endHoleSize);
        }

        IEnumerator FadeToBlackThenLoad()
        {
            if (_levelEnterBlackout == null)
            {
                LoadSelectedLevel();
                yield break;
            }

            _levelEnterBlackout.gameObject.SetActive(true);
            _levelEnterBlackout.transform.SetAsLastSibling();
            var group = _levelEnterBlackout.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = true;
            }

            var t = 0f;
            while (t < LevelBlackFadeDuration)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.Clamp01(t / LevelBlackFadeDuration);
                if (group != null)
                    group.alpha = u;
                yield return null;
            }

            if (group != null)
                group.alpha = 1f;

            yield return new WaitForSecondsRealtime(0.15f);
            LoadSelectedLevel();
        }

        void LoadSelectedLevel()
        {
            if (_zoomedRegion == MapRegion.Stargazer)
                LoadStoryMode1();
            else
                LoadStoryMode2();
        }

        static void LoadStoryMode2() => SceneLoader.LoadStoryMode2();

        bool IsPointerOverLevelButton()
        {
            if (_levelButtonRect == null || EventSystem.current == null)
                return false;

            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject == null)
                    continue;

                if (result.gameObject == _levelButtonRect.gameObject
                    || result.gameObject.transform.IsChildOf(_levelButtonRect))
                    return true;
            }

            return false;
        }

        bool IsPointerOverContinentButton()
        {
            if (_continentButtonRect == null || !_continentButtonRect.gameObject.activeInHierarchy
                || EventSystem.current == null)
                return false;

            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject == null)
                    continue;

                if (result.gameObject == _continentButtonRect.gameObject
                    || result.gameObject.transform.IsChildOf(_continentButtonRect))
                    return true;
            }

            return false;
        }

        bool IsPointerOverStargazerContinentButton()
        {
            if (_stargazerContinentButtonRect == null || !_stargazerContinentButtonRect.gameObject.activeInHierarchy
                || EventSystem.current == null)
                return false;

            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject == null)
                    continue;

                if (result.gameObject == _stargazerContinentButtonRect.gameObject
                    || result.gameObject.transform.IsChildOf(_stargazerContinentButtonRect))
                    return true;
            }

            return false;
        }

        IEnumerator ZoomIn()
        {
            _busy = true;
            if (_continentButton != null)
                _continentButton.interactable = false;
            if (_stargazerContinentButton != null)
                _stargazerContinentButton.interactable = false;

            StopPulse();

            if (_continentButtonRect != null)
                _continentButtonRect.localScale = Vector3.one;
            if (_stargazerContinentButtonRect != null)
                _stargazerContinentButtonRect.localScale = Vector3.one;

            Canvas.ForceUpdateCanvases();
            FitMapToViewport();

            var mapSize = _mapRoot.rect.size;
            var buttonLocal = new Vector2(
                (ActiveLevelAnchorX - 0.5f) * mapSize.x,
                (ActiveLevelAnchorY - 0.5f) * mapSize.y);
            var startPos = _mapRoot.anchoredPosition;
            var endPos = -buttonLocal * ZoomScale;
            var startScale = _mapRoot.localScale;
            var endScale = Vector3.one * ZoomScale;

            var t = 0f;
            while (t < ZoomDuration)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / ZoomDuration));
                _mapRoot.localScale = Vector3.LerpUnclamped(startScale, endScale, u);
                _mapRoot.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, u);
                yield return null;
            }

            _mapRoot.localScale = endScale;
            _mapRoot.anchoredPosition = endPos;
            _zoomed = true;
            _busy = false;
            ShowLevelMarker();
        }

        void RepositionLevelMarker()
        {
            if (_levelMarkerRoot == null)
                return;

            var anchor = new Vector2(ActiveLevelAnchorX, ActiveLevelAnchorY);
            _levelMarkerRoot.anchorMin = anchor;
            _levelMarkerRoot.anchorMax = anchor;
            _levelMarkerRoot.anchoredPosition = Vector2.zero;
        }

        IEnumerator ZoomOut()
        {
            _busy = true;
            if (_levelButton != null)
                _levelButton.interactable = false;

            HideLevelMarker();

            var startPos = _mapRoot.anchoredPosition;
            var endPos = Vector2.zero;
            var startScale = _mapRoot.localScale;
            var endScale = Vector3.one;

            var t = 0f;
            while (t < ZoomDuration)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / ZoomDuration));
                _mapRoot.localScale = Vector3.LerpUnclamped(startScale, endScale, u);
                _mapRoot.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, u);
                yield return null;
            }

            _mapRoot.localScale = endScale;
            _mapRoot.anchoredPosition = endPos;
            _zoomed = false;
            _zoomedRegion = MapRegion.None;
            _busy = false;
            ShowContinentButton();
        }

        void ShowLevelMarker()
        {
            SetOverviewMarkersVisible(false);
            RepositionLevelMarker();

            if (_levelMarkerRoot != null)
                _levelMarkerRoot.gameObject.SetActive(true);

            if (_levelLabelText != null)
            {
                _levelLabelText.text = _zoomedRegion == MapRegion.Stargazer
                    ? StargazerLevelLabelText
                    : LevelLabelText;
                _levelLabelText.gameObject.SetActive(true);
            }

            if (_levelButton != null)
                _levelButton.interactable = true;

            if (_levelButtonRect != null)
                _levelButtonRect.gameObject.SetActive(true);

            RefreshActiveLevelButtonColors();
            SetStargazerRecallDialogVisible(_zoomedRegion == MapRegion.Stargazer);
            UpdateMapTitle();
            StartLevelPulse();
        }

        void SetStargazerRecallDialogVisible(bool visible)
        {
            if (_stargazerRecallDialogRoot != null)
                _stargazerRecallDialogRoot.gameObject.SetActive(visible);
        }

        void HideLevelMarker()
        {
            StopPulse();
            SetStargazerRecallDialogVisible(false);

            if (_levelButtonRect != null)
                _levelButtonRect.localScale = Vector3.one;

            if (_levelMarkerRoot != null)
                _levelMarkerRoot.gameObject.SetActive(false);
        }

        void ShowContinentButton()
        {
            SetOverviewMarkersVisible(true);
            RefreshOverviewButtonColors();
            UpdateMapTitle();
            StartContinentPulse();
            RefreshPhantasiaAlert();
        }

        void SetOverviewMarkersVisible(bool visible)
        {
            SetContinentButtonVisible(visible);
            SetStargazerContinentButtonVisible(visible);
            SetLockedContinentsVisible(visible);
        }

        void SetLockedContinentsVisible(bool visible)
        {
            if (_lockedContinentsRoot == null)
                return;

            _lockedContinentsRoot.gameObject.SetActive(visible);
            if (!visible || _lockedContinentsGroup == null)
                return;

            _lockedContinentsGroup.alpha = 1f;
            _lockedContinentsGroup.interactable = true;
            _lockedContinentsGroup.blocksRaycasts = !_busy && !_zoomed;
        }

        void UpdateMapTitle()
        {
            if (_mapTitleText == null)
                return;

            _mapTitleText.text = _zoomedRegion switch
            {
                MapRegion.DreamIsles => ZoomedMapTitle,
                MapRegion.Stargazer => StargazerZoomedMapTitle,
                _ => FullMapTitle
            };
        }

        void SetStargazerContinentButtonVisible(bool visible)
        {
            if (_stargazerContinentMarkerRoot == null)
                return;

            _stargazerContinentMarkerRoot.gameObject.SetActive(visible);
            if (!visible)
                return;

            if (_stargazerContinentButtonRect != null)
            {
                _stargazerContinentButtonRect.localScale = Vector3.one;
                _stargazerContinentButtonRect.SetAsLastSibling();
            }

            if (_stargazerContinentButtonGroup != null)
            {
                _stargazerContinentButtonGroup.alpha = 1f;
                _stargazerContinentButtonGroup.interactable = true;
                _stargazerContinentButtonGroup.blocksRaycasts = true;
            }

            if (_stargazerContinentButton != null)
                _stargazerContinentButton.interactable = !_busy;
        }

        void SetContinentButtonVisible(bool visible)
        {
            if (_continentMarkerRoot == null)
                return;

            _continentMarkerRoot.gameObject.SetActive(visible);
            if (!visible)
                return;

            if (_continentButtonRect != null)
            {
                _continentButtonRect.localScale = Vector3.one;
                _continentButtonRect.SetAsLastSibling();
            }

            if (_continentButtonGroup != null)
            {
                _continentButtonGroup.alpha = 1f;
                _continentButtonGroup.interactable = true;
                _continentButtonGroup.blocksRaycasts = true;
            }

            if (_continentButton != null)
                _continentButton.interactable = !_busy;
        }

        static void StyleWhiteMapText(Text text)
        {
            if (text == null)
                return;

            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;

            if (text.GetComponent<Outline>() == null)
            {
                var outline = text.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
                outline.effectDistance = new Vector2(1.2f, -1.2f);
            }
        }

        void FitMapToViewport()
        {
            if (_viewport == null || _mapRoot == null || _mapAspect <= 0f)
                return;

            var size = _viewport.rect.size;
            if (size.x <= 1f || size.y <= 1f)
                return;

            var viewportAspect = size.x / size.y;
            float width;
            float height;
            if (viewportAspect > _mapAspect)
            {
                height = size.y;
                width = height * _mapAspect;
            }
            else
            {
                width = size.x;
                height = width / _mapAspect;
            }

            _mapRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _mapRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        static Sprite LoadMapSprite()
        {
            var tex = Resources.Load<Texture2D>(MapResource);
            if (tex == null)
                return null;

            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f,
                0, SpriteMeshType.FullRect);
        }

        static Sprite CreateCircleSprite()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var radius = size * 0.46f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dist = Vector2.Distance(new Vector2(x, y), center);
                    var a = Mathf.Clamp01(radius - dist + 0.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static Sprite CreateCheckmarkSprite()
        {
            const int size = 64;
            const float outerThickness = 7f;
            const float innerThickness = 4.2f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            var a = new Vector2(0.18f, 0.52f) * size;
            var b = new Vector2(0.42f, 0.28f) * size;
            var c = new Vector2(0.84f, 0.74f) * size;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var p = new Vector2(x, y);
                    var dist = Mathf.Min(DistanceToSegment(p, a, b), DistanceToSegment(p, b, c));
                    var alpha = Mathf.Clamp01(outerThickness - dist + 0.5f);
                    var whiteness = Mathf.Clamp01(innerThickness - dist + 0.5f);
                    var color = Color.Lerp(CheckOutlineColor, Color.white, whiteness);
                    color.a = alpha;
                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }

        static Sprite CreateStarSprite()
        {
            const int size = 64;
            const int samples = 4;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            var center = new Vector2(size * 0.5f, size * 0.5f);
            var outerRadius = size * 0.48f;
            var innerRadius = outerRadius * 0.45f;
            var points = new Vector2[10];
            for (var i = 0; i < points.Length; i++)
            {
                var angle = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                var radius = i % 2 == 0 ? outerRadius : innerRadius;
                points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var hits = 0;
                    for (var sy = 0; sy < samples; sy++)
                    for (var sx = 0; sx < samples; sx++)
                    {
                        var p = new Vector2(x + (sx + 0.5f) / samples, y + (sy + 0.5f) / samples);
                        if (IsInsidePolygon(p, points))
                            hits++;
                    }

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, hits / (float)(samples * samples)));
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static bool IsInsidePolygon(Vector2 p, Vector2[] polygon)
        {
            var inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];
                if ((pi.y > p.y) != (pj.y > p.y)
                    && p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x)
                    inside = !inside;
            }

            return inside;
        }

        static Sprite CreateCrackSprite(int variant)
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var clear = new Color(0f, 0f, 0f, 0f);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                    tex.SetPixel(x, y, clear);
            }

            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            Vector2[][] paths =
            {
                new[]
                {
                    new Vector2(0.50f, 0.82f), new Vector2(0.46f, 0.62f), new Vector2(0.52f, 0.42f),
                    new Vector2(0.48f, 0.22f)
                },
                new[]
                {
                    new Vector2(0.22f, 0.68f), new Vector2(0.38f, 0.56f), new Vector2(0.55f, 0.48f),
                    new Vector2(0.74f, 0.34f)
                },
                new[]
                {
                    new Vector2(0.78f, 0.72f), new Vector2(0.62f, 0.58f), new Vector2(0.44f, 0.50f),
                    new Vector2(0.28f, 0.30f), new Vector2(0.34f, 0.18f)
                }
            };

            var path = paths[Mathf.Clamp(variant, 0, paths.Length - 1)];
            for (var i = 0; i < path.Length - 1; i++)
            {
                var a = center + (path[i] - new Vector2(0.5f, 0.5f)) * size;
                var b = center + (path[i + 1] - new Vector2(0.5f, 0.5f)) * size;
                DrawCrackSegment(tex, a, b, 1.15f + variant * 0.15f);
                if (i == path.Length / 2)
                {
                    var branch = b + new Vector2((variant % 2 == 0 ? 1f : -1f) * 8f, -6f);
                    DrawCrackSegment(tex, b, branch, 0.9f);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static void DrawCrackSegment(Texture2D tex, Vector2 from, Vector2 to, float thickness)
        {
            var steps = Mathf.CeilToInt(Vector2.Distance(from, to) * 2f);
            for (var i = 0; i <= steps; i++)
            {
                var p = Vector2.Lerp(from, to, i / (float)steps);
                var jag = Mathf.Sin(i * 0.7f) * 0.8f;
                var px = Mathf.RoundToInt(p.x + jag);
                var py = Mathf.RoundToInt(p.y);
                var radius = Mathf.Max(1, Mathf.CeilToInt(thickness));
                for (var oy = -radius; oy <= radius; oy++)
                {
                    for (var ox = -radius; ox <= radius; ox++)
                    {
                        if (ox * ox + oy * oy > thickness * thickness)
                            continue;
                        var x = px + ox;
                        var y = py + oy;
                        if (x < 0 || y < 0 || x >= tex.width || y >= tex.height)
                            continue;
                        var a = Mathf.Clamp01(thickness - Mathf.Sqrt(ox * ox + oy * oy) + 0.35f);
                        var existing = tex.GetPixel(x, y);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(existing.a, a)));
                    }
                }
            }
        }

        static Sprite CreateCrackedHoleSprite()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var baseRadius = size * 0.34f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var pos = new Vector2(x, y);
                    var dir = pos - center;
                    var angle = Mathf.Atan2(dir.y, dir.x);
                    var jagged = 1f
                                 + 0.12f * Mathf.Sin(angle * 5f)
                                 + 0.08f * Mathf.Sin(angle * 11f + 1.2f)
                                 + 0.05f * Mathf.Sin(angle * 17f + 0.4f);
                    var radius = baseRadius * jagged;
                    var dist = dir.magnitude;
                    var a = Mathf.Clamp01(radius - dist + 1.2f);
                    if (a <= 0.01f)
                    {
                        tex.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                        continue;
                    }

                    var edgeCrack = 0f;
                    if (dist > radius * 0.72f)
                    {
                        edgeCrack = Mathf.Max(0f, Mathf.Sin(angle * 7f) * 0.35f)
                                    + Mathf.Max(0f, Mathf.Sin(angle * 13f + 0.8f) * 0.25f);
                    }

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(a + edgeCrack)));
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static Sprite CreateCloudSprite()
        {
            const int width = 160;
            const int height = 80;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            var centers = new[]
            {
                new Vector2(34f, 38f),
                new Vector2(62f, 48f),
                new Vector2(92f, 40f),
                new Vector2(118f, 44f)
            };
            var radii = new[] { 26f, 30f, 24f, 20f };

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var alpha = 0f;
                    for (var i = 0; i < centers.Length; i++)
                    {
                        var dist = Vector2.Distance(new Vector2(x, y), centers[i]);
                        alpha = Mathf.Max(alpha, Mathf.Clamp01(radii[i] - dist + 0.5f));
                    }

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        static void LoadAllFeatures() => SceneLoader.LoadAllFeatures();
        static void LoadStoryMode1() => SceneLoader.LoadStoryMode1();
    }
}
