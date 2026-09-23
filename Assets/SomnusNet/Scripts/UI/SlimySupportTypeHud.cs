using SomnusNet.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class SlimySupportTypeHud : MonoBehaviour
    {
        static string PopupTitle =>
            TypeDescriptionText.PopupTitle(SlimySupportTypeRules.Label, SlimySupportTypeRules.Category);

        static string PopupBody =>
            TypeDescriptionText.GroupTypeBodyFromLines(
                System.Array.ConvertAll(SlimySupportTypeRules.MemberRoster, m => m.effectLine));

        static readonly Vector2 DefaultIconAnchorMin = new(0.02f, 0.835f);
        static readonly Vector2 DefaultIconAnchorMax = new(0.065f, 0.905f);
        static readonly Vector2 DefaultCounterAnchorMin = new(0.07f, 0.835f);
        static readonly Vector2 DefaultCounterAnchorMax = new(0.16f, 0.905f);

        [SerializeField] Button iconButton;
        [SerializeField] Image iconImage;
        [SerializeField] Text counterText;
        [SerializeField] RectTransform popupRoot;

        bool _popupOpen;
        float _nextCounterRefresh;

        const float CounterRefreshInterval = 0.2f;

        public static void EnsureExists()
        {
            if (!SlimySupportTypeRules.FeatureEnabled) return;
            if (Object.FindFirstObjectByType<SlimySupportTypeHud>() != null) return;
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<SlimySupportTypeHud>();
        }

        void Awake()
        {
            if (!SlimySupportTypeRules.FeatureEnabled) { enabled = false; return; }
        }

        void Start()
        {
            if (!SlimySupportTypeRules.FeatureEnabled) return;
            EnsureUiBuilt();
            ApplyIconAppearance(GridManager.Instance);
            HidePopup();
            SetRowVisible(false);
            iconButton.onClick.AddListener(TogglePopup);
        }

        void Update()
        {
            if (!SlimySupportTypeRules.FeatureEnabled) return;
            if (Time.unscaledTime >= _nextCounterRefresh)
            {
                _nextCounterRefresh = Time.unscaledTime + CounterRefreshInterval;
                RefreshCounter();
            }
            if (!_popupOpen || !Input.GetMouseButtonDown(0)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            HidePopup();
        }

        void OnDestroy()
        {
            if (iconButton != null)
                iconButton.onClick.RemoveListener(TogglePopup);
        }

        void TogglePopup() { if (_popupOpen) HidePopup(); else ShowPopup(); }
        void ShowPopup()
        {
            TypeHudLayout.NotifyPopupOpening(TypeHudLayout.Kind.SlimySupport);
            TypeHudLayout.RefreshNow();
            _popupOpen = true;
            popupRoot.gameObject.SetActive(true);
        }
        void HidePopup() { _popupOpen = false; if (popupRoot != null) popupRoot.gameObject.SetActive(false); }

        void RefreshCounter()
        {
            var grid = GridManager.Instance;
            var onField = grid != null
                ? grid.CountRosterMembersOnField(SlimySupportTypeRules.MemberKinds)
                : 0;
            var visible = onField > 0;
            SetRowVisible(visible);
            if (!visible)
            {
                if (_popupOpen) HidePopup();
                TypeHudLayout.SetVisible(TypeHudLayout.Kind.SlimySupport, false);
                return;
            }

            counterText.text = $"({onField}/{SlimySupportTypeRules.TotalUniqueInGame})";
            ApplyIconAppearance(grid);
            TypeHudLayout.SetVisible(TypeHudLayout.Kind.SlimySupport, true);
        }

        void ApplyIconAppearance(GridManager grid)
        {
            if (iconImage == null) return;
            iconImage.color = SlimySupportSymbolSprites.IconColor;
            iconImage.sprite = SlimySupportSymbolSprites.GetHudIcon(grid);
        }

        void SetRowVisible(bool visible)
        {
            if (iconButton != null) iconButton.gameObject.SetActive(visible);
            if (counterText != null) counterText.gameObject.SetActive(visible);
        }

        void EnsureUiBuilt()
        {
            if (iconButton != null) return;
            var canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            if (canvas == null) return;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var iconGo = new GameObject("SlimySupportTypeIcon");
            iconGo.transform.SetParent(canvas.transform, false);
            var iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = DefaultIconAnchorMin;
            iconRect.anchorMax = DefaultIconAnchorMax;
            iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
            iconImage = iconGo.AddComponent<Image>();
            iconImage.sprite = SlimySupportSymbolSprites.GetHudIcon(GridManager.Instance);
            iconImage.color = SlimySupportSymbolSprites.IconColor;
            iconImage.preserveAspect = true;
            iconButton = iconGo.AddComponent<Button>();
            iconButton.targetGraphic = iconImage;

            counterText = TypeHudSprites.CreateText(canvas.transform, "SlimySupportTypeCounter",
                DefaultCounterAnchorMin, DefaultCounterAnchorMax, 16, TextAnchor.MiddleLeft, font);

            TypeHudLayout.Register(TypeHudLayout.Kind.SlimySupport, iconRect, counterText.rectTransform);

            popupRoot = TypeHudSprites.CreatePopup(canvas.transform, font, "SlimySupportTypePopup",
                new Vector2(0.02f, 0.58f), new Vector2(0.42f, 0.82f), PopupTitle, PopupBody);
            TypeHudLayout.RegisterPopup(TypeHudLayout.Kind.SlimySupport, popupRoot, HidePopup);
        }
    }
}
