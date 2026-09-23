using SomnusNet.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class SpeedTypeHud : MonoBehaviour
    {
        const string SpeedMechanics =
            "All Speed-type blobs attack faster based on how many Speed types are on the field.";

        static string PopupTitle =>
            TypeDescriptionText.PopupTitle(SpeedTypeRules.Label, SpeedTypeRules.Category);

        static string PopupBody => TypeDescriptionText.GeneralTypeBody(SpeedMechanics);

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
            if (!BlobTypeFeatures.Enabled) return;
            if (Object.FindFirstObjectByType<SpeedTypeHud>() != null) return;
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<SpeedTypeHud>();
        }

        void Awake()
        {
            if (!BlobTypeFeatures.Enabled) { enabled = false; return; }
        }

        void Start()
        {
            if (!BlobTypeFeatures.Enabled) return;
            EnsureUiBuilt();
            HidePopup();
            SetRowVisible(false);
            iconButton.onClick.AddListener(TogglePopup);
        }

        void Update()
        {
            if (!BlobTypeFeatures.Enabled) return;
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
            TypeHudLayout.NotifyPopupOpening(TypeHudLayout.Kind.Speed);
            TypeHudLayout.RefreshNow();
            _popupOpen = true;
            popupRoot.gameObject.SetActive(true);
        }
        void HidePopup() { _popupOpen = false; if (popupRoot != null) popupRoot.gameObject.SetActive(false); }

        void RefreshCounter()
        {
            var grid = GridManager.Instance;
            var onField = grid != null ? grid.CountUniqueKindsWithType(SpeedTypeRules.Label) : 0;
            var visible = onField > 0;
            SetRowVisible(visible);
            if (!visible)
            {
                if (_popupOpen) HidePopup();
                TypeHudLayout.SetVisible(TypeHudLayout.Kind.Speed, false);
                return;
            }

            counterText.text = $"({onField}/{SpeedTypeRules.TotalUniqueInGame})";
            TypeHudLayout.SetVisible(TypeHudLayout.Kind.Speed, true);
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

            var iconGo = new GameObject("SpeedTypeIcon");
            iconGo.transform.SetParent(canvas.transform, false);
            var iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = DefaultIconAnchorMin;
            iconRect.anchorMax = DefaultIconAnchorMax;
            iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
            var iconImage = iconGo.AddComponent<Image>();
            iconImage.sprite = SpeedSymbolSprites.Icon ?? TypeHudSprites.CreateCircleSprite();
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
            iconButton = iconGo.AddComponent<Button>();
            iconButton.targetGraphic = iconImage;

            counterText = TypeHudSprites.CreateText(canvas.transform, "SpeedTypeCounter",
                DefaultCounterAnchorMin, DefaultCounterAnchorMax, 16, TextAnchor.MiddleLeft, font);

            TypeHudLayout.Register(TypeHudLayout.Kind.Speed, iconRect, counterText.rectTransform);

            popupRoot = TypeHudSprites.CreatePopup(canvas.transform, font, "SpeedTypePopup",
                new Vector2(0.02f, 0.58f), new Vector2(0.42f, 0.82f), PopupTitle, PopupBody);
            TypeHudLayout.RegisterPopup(TypeHudLayout.Kind.Speed, popupRoot, HidePopup);
        }
    }
}
