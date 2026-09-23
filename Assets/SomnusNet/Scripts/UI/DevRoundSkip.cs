using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.Core
{
    /// <summary>Developer-only control to skip round waits during testing.</summary>
    public class DevRoundSkip : MonoBehaviour
    {
        static readonly Vector2 ButtonAnchorMin = new(0.84f, 0.02f);
        static readonly Vector2 ButtonAnchorMax = new(0.98f, 0.09f);

        public static bool IsEnabled
        {
            get
            {
                if (LevelSettings.Instance == null || !LevelSettings.Instance.enableDevSkip)
                    return false;

#if UNITY_EDITOR
                return true;
#else
                return Debug.isDebugBuild;
#endif
            }
        }

        public static void EnsureExists()
        {
            if (!IsEnabled) return;
            if (Object.FindFirstObjectByType<DevRoundSkip>() != null) return;
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
                canvas.gameObject.AddComponent<DevRoundSkip>();
        }

        public static DevRoundSkip Instance { get; private set; }

        [SerializeField] Button skipButton;

        void Awake() => Instance = this;

        void Start()
        {
            if (!IsEnabled)
            {
                if (skipButton != null)
                    skipButton.gameObject.SetActive(false);
                enabled = false;
                return;
            }

            EnsureButton();
            ApplyButtonLayout();
            if (skipButton != null)
                skipButton.onClick.AddListener(OnSkipClicked);
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            if (skipButton != null)
                skipButton.onClick.RemoveListener(OnSkipClicked);
        }

        void OnSkipClicked()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.DevSkipRound();
        }

        void EnsureButton()
        {
            if (skipButton != null) return;

            var canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var go = new GameObject("DevSkipRound");
            go.transform.SetParent(canvas.transform, false);
            go.AddComponent<RectTransform>();

            var img = go.AddComponent<Image>();
            img.color = new Color(0.35f, 0.18f, 0.18f, 0.88f);

            skipButton = go.AddComponent<Button>();

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var lrt = labelGo.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;

            var txt = labelGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 9;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(1f, 0.85f, 0.85f);
            txt.text = "Dev: Skip";
        }

        void ApplyButtonLayout()
        {
            if (skipButton == null) return;

            var rt = skipButton.GetComponent<RectTransform>();
            rt.anchorMin = ButtonAnchorMin;
            rt.anchorMax = ButtonAnchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var label = skipButton.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.fontSize = 9;
                label.text = "Dev: Skip";
            }
        }
    }
}
