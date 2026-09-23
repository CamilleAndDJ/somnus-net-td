using SomnusNet.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class AbsentHistoryTypeHud : MonoBehaviour
    {
        const string AbsentHistoryMechanics =
            "Absent History blobs naturally have Looped Sight — they can target Looped glitches and deal full Cicadian Rhythm damage.\nFar-ring hits (outside 5×5, inside 7×7) gain +4 damage per unique blob on the field.";

        static string PopupTitle =>
            TypeDescriptionText.PopupTitle(AbsentHistoryTypeRules.Label, AbsentHistoryTypeRules.Category);

        static string PopupBody => TypeDescriptionText.GeneralTypeBody(AbsentHistoryMechanics);

        static readonly Vector2 DefaultIconAnchorMin = new(0.02f, 0.835f);
        static readonly Vector2 DefaultIconAnchorMax = new(0.065f, 0.905f);
        static readonly Vector2 DefaultCounterAnchorMin = new(0.07f, 0.835f);
        static readonly Vector2 DefaultCounterAnchorMax = new(0.16f, 0.905f);

        [SerializeField] Button iconButton;
        [SerializeField] Text counterText;
        [SerializeField] RectTransform popupRoot;

        bool _popupOpen;
        float _nextCounterRefresh;

        const float CounterRefreshInterval = 0.2f;

        public static void EnsureExists()
        {
            if (!AbsentHistoryTypeRules.FeatureEnabled) return;
            if (Object.FindFirstObjectByType<AbsentHistoryTypeHud>() != null) return;
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<AbsentHistoryTypeHud>();
        }

        void Awake()
        {
            if (!AbsentHistoryTypeRules.FeatureEnabled) { enabled = false; return; }
        }

        void Start()
        {
            if (!AbsentHistoryTypeRules.FeatureEnabled) return;
            EnsureUiBuilt();
            HidePopup();
            SetRowVisible(false);
            iconButton.onClick.AddListener(TogglePopup);
        }

        void Update()
        {
            if (!AbsentHistoryTypeRules.FeatureEnabled) return;
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
            TypeHudLayout.NotifyPopupOpening(TypeHudLayout.Kind.AbsentHistory);
            TypeHudLayout.RefreshNow();
            _popupOpen = true;
            popupRoot.gameObject.SetActive(true);
        }
        void HidePopup() { _popupOpen = false; if (popupRoot != null) popupRoot.gameObject.SetActive(false); }

        void RefreshCounter()
        {
            var grid = GridManager.Instance;
            var onField = grid != null ? grid.CountUniqueKindsWithType(AbsentHistoryTypeRules.Label) : 0;
            var visible = onField > 0;
            SetRowVisible(visible);
            if (!visible)
            {
                if (_popupOpen) HidePopup();
                TypeHudLayout.SetVisible(TypeHudLayout.Kind.AbsentHistory, false);
                return;
            }

            counterText.text = $"({onField}/{AbsentHistoryTypeRules.TotalUniqueInGame})";
            TypeHudLayout.SetVisible(TypeHudLayout.Kind.AbsentHistory, true);
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

            var iconGo = new GameObject("AbsentHistoryTypeIcon");
            iconGo.transform.SetParent(canvas.transform, false);
            var iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = DefaultIconAnchorMin;
            iconRect.anchorMax = DefaultIconAnchorMax;
            iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
            var iconImage = iconGo.AddComponent<Image>();
            iconImage.sprite = TypeHudSprites.CreateCircleSprite();
            iconImage.color = new Color(0.55f, 0.78f, 0.98f);
            iconButton = iconGo.AddComponent<Button>();
            iconButton.targetGraphic = iconImage;

            counterText = TypeHudSprites.CreateText(canvas.transform, "AbsentHistoryTypeCounter",
                DefaultCounterAnchorMin, DefaultCounterAnchorMax, 16, TextAnchor.MiddleLeft, font);

            TypeHudLayout.Register(TypeHudLayout.Kind.AbsentHistory, iconRect, counterText.rectTransform);

            popupRoot = TypeHudSprites.CreatePopup(canvas.transform, font, "AbsentHistoryTypePopup",
                new Vector2(0.02f, 0.58f), new Vector2(0.42f, 0.82f), PopupTitle, PopupBody);
            TypeHudLayout.RegisterPopup(TypeHudLayout.Kind.AbsentHistory, popupRoot, HidePopup);
        }
    }
}
