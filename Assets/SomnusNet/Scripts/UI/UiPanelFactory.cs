using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    static class UiPanelFactory
    {
        public static readonly Color PanelBg = new(0.16f, 0.14f, 0.28f, 0.96f);
        public static readonly Color DimOverlay = new(0f, 0f, 0f, 0.55f);
        public static readonly Color ButtonBg = new(0.25f, 0.2f, 0.45f, 0.92f);
        public static readonly Color ButtonSelected = new(0.38f, 0.32f, 0.62f, 0.98f);
        public static readonly Color TextColor = new(0.92f, 0.92f, 0.96f);

        public static Font DefaultFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static RectTransform CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var rt = CreateRect(parent, name, anchorMin, anchorMax);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            int fontSize, TextAnchor align, string content = "")
        {
            var rt = CreateRect(parent, name, anchorMin, anchorMax);
            var text = rt.gameObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = TextColor;
            text.text = content;
            return text;
        }

        public static Button CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            string label, int fontSize = 16)
        {
            var img = CreateImage(parent, name, anchorMin, anchorMax, ButtonBg);
            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;

            CreateText(img.transform, "Label", Vector2.zero, Vector2.one, fontSize, TextAnchor.MiddleCenter, label);
            return button;
        }

        public static (ScrollRect scroll, RectTransform content) CreateScrollList(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var viewport = CreateRect(parent, name, anchorMin, anchorMax);
            var viewportImg = viewport.gameObject.AddComponent<Image>();
            viewportImg.color = new Color(0.1f, 0.09f, 0.18f, 0.85f);
            var mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            var content = CreateRect(viewport, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f));
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 4f;
            layout.padding = new RectOffset(4, 4, 4, 4);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollGo = viewport.gameObject;
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            return (scroll, content);
        }

        public static Canvas EnsureOverlayCanvas(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var canvas = go.AddComponent<Canvas>();
            var parentCanvas = parent.GetComponent<Canvas>() ?? parent.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                canvas.renderMode = parentCanvas.renderMode;
                canvas.worldCamera = parentCanvas.worldCamera;
                canvas.planeDistance = parentCanvas.planeDistance;
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }
    }
}
