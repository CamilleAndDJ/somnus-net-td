using SomnusNet.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class HarmonyTypeHud : MonoBehaviour
    {
        const string HarmonyMechanics =
            "3×3 bands link through Harmony blobs. +4 damage per mate — Harmony only.";

        static string PopupTitle =>
            TypeDescriptionText.PopupTitle(HarmonyTypeRules.HarmonyLabel, HarmonyTypeRules.Category);

        static string PopupBody => TypeDescriptionText.GeneralTypeBody(HarmonyMechanics);

        static readonly Vector2 DefaultIconAnchorMin = new(0.02f, 0.835f);
        static readonly Vector2 DefaultIconAnchorMax = new(0.065f, 0.905f);
        static readonly Vector2 DefaultCounterAnchorMin = new(0.07f, 0.835f);
        static readonly Vector2 DefaultCounterAnchorMax = new(0.16f, 0.905f);

        [SerializeField] Button iconButton;
        [SerializeField] Image iconImage;
        [SerializeField] Text counterText;
        [SerializeField] RectTransform popupRoot;
        [SerializeField] Text popupTitleText;
        [SerializeField] Text popupBodyText;

        bool _popupOpen;
        float _nextCounterRefresh;

        const float CounterRefreshInterval = 0.2f;

        public static HarmonyTypeHud Instance { get; private set; }

        public static void EnsureExists()
        {
            if (!HarmonyTypeRules.FeatureEnabled) return;
            if (Instance != null) return;

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<HarmonyTypeHud>();
        }

        void Awake()
        {
            if (!HarmonyTypeRules.FeatureEnabled)
            {
                enabled = false;
                return;
            }

            Instance = this;
        }

        void Start()
        {
            if (!HarmonyTypeRules.FeatureEnabled) return;

            EnsureUiBuilt();
            HidePopup();
            RefreshCounter();
            if (iconButton != null)
                iconButton.gameObject.SetActive(false);
            if (counterText != null)
                counterText.gameObject.SetActive(false);

            if (iconButton != null)
                iconButton.onClick.AddListener(TogglePopup);
        }

        void Update()
        {
            if (!HarmonyTypeRules.FeatureEnabled) return;

            if (Time.unscaledTime >= _nextCounterRefresh)
            {
                _nextCounterRefresh = Time.unscaledTime + CounterRefreshInterval;
                RefreshCounter();
            }

            if (!_popupOpen || !Input.GetMouseButtonDown(0)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            HidePopup();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            if (iconButton != null)
                iconButton.onClick.RemoveListener(TogglePopup);
        }

        void TogglePopup()
        {
            if (_popupOpen)
                HidePopup();
            else
                ShowPopup();
        }

        void ShowPopup()
        {
            TypeHudLayout.NotifyPopupOpening(TypeHudLayout.Kind.Harmony);
            TypeHudLayout.RefreshNow();
            _popupOpen = true;
            if (popupRoot != null)
                popupRoot.gameObject.SetActive(true);
        }

        void HidePopup()
        {
            _popupOpen = false;
            if (popupRoot != null)
                popupRoot.gameObject.SetActive(false);
        }

        public bool TryGetIconRect(out RectTransform rect)
        {
            EnsureUiBuilt();
            rect = iconButton != null ? iconButton.GetComponent<RectTransform>() : null;
            return rect != null;
        }

        public bool TryGetCounterRect(out RectTransform rect)
        {
            EnsureUiBuilt();
            rect = counterText != null ? counterText.rectTransform : null;
            return rect != null;
        }

        void RefreshCounter()
        {
            var grid = GridManager.Instance;
            var onField = grid != null ? grid.CountUniqueHarmonyTypesOnField() : 0;
            var forceVisible = StoryModeHarmonyTutorial.Instance != null
                               && StoryModeHarmonyTutorial.Instance.IsTutorialActive;
            var visible = onField > 0 || forceVisible;

            if (iconButton != null)
                iconButton.gameObject.SetActive(visible);
            if (counterText != null)
            {
                counterText.gameObject.SetActive(visible);
                counterText.text = $"({onField}/{HarmonyTypeRules.TotalUniqueHarmonyTypes})";
            }

            if (!visible && _popupOpen)
                HidePopup();

            TypeHudLayout.SetVisible(TypeHudLayout.Kind.Harmony, visible);
        }

        void EnsureUiBuilt()
        {
            if (iconButton != null) return;

            var canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var iconSprite = HarmonySymbolSprites.Icon ?? TypeHudSprites.CreateCircleSprite();

            var iconGo = new GameObject("HarmonyTypeIcon");
            iconGo.transform.SetParent(canvas.transform, false);
            var iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = DefaultIconAnchorMin;
            iconRect.anchorMax = DefaultIconAnchorMax;
            iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;

            iconImage = iconGo.AddComponent<Image>();
            iconImage.sprite = iconSprite;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;

            iconButton = iconGo.AddComponent<Button>();
            iconButton.targetGraphic = iconImage;

            counterText = CreateText(canvas.transform, "HarmonyTypeCounter",
                DefaultCounterAnchorMin, DefaultCounterAnchorMax, 16, TextAnchor.MiddleLeft, font);
            counterText.color = new Color(0.92f, 0.92f, 0.96f);

            TypeHudLayout.Register(TypeHudLayout.Kind.Harmony, iconRect, counterText.rectTransform);

            popupRoot = CreatePopup(canvas.transform, font);
            TypeHudLayout.RegisterPopup(TypeHudLayout.Kind.Harmony, popupRoot, HidePopup);
        }

        RectTransform CreatePopup(Transform parent, Font font)
        {
            var popupGo = new GameObject("HarmonyTypePopup");
            popupGo.transform.SetParent(parent, false);
            var popupRect = popupGo.AddComponent<RectTransform>();
            popupRect.anchorMin = new Vector2(0.02f, 0.58f);
            popupRect.anchorMax = new Vector2(0.42f, 0.82f);
            popupRect.offsetMin = popupRect.offsetMax = Vector2.zero;

            var bg = popupGo.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.14f, 0.28f, 0.96f);

            popupTitleText = CreateText(popupGo.transform, "Title",
                new Vector2(0.06f, 0.72f), new Vector2(0.94f, 0.94f), 20, TextAnchor.UpperCenter, font);
            popupTitleText.fontStyle = FontStyle.Bold;
            popupTitleText.text = PopupTitle;

            popupBodyText = CreateText(popupGo.transform, "Body",
                new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.7f), 13, TextAnchor.UpperLeft, font);
            popupBodyText.text = PopupBody;
            popupBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            popupBodyText.verticalOverflow = VerticalWrapMode.Overflow;

            return popupRect;
        }

        static Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            int fontSize, TextAnchor align, Font font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = new Color(0.92f, 0.9f, 1f);
            return text;
        }

    }
}
