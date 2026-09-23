using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    static class TypeHudSprites
    {
        static Sprite _circleSprite;

        public static Sprite CreateCircleSprite()
        {
            if (_circleSprite != null) return _circleSprite;
            const int size = 32;
            var tex = new Texture2D(size, size);
            var center = new Vector2(size / 2f, size / 2f);
            var radius = size * 0.42f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dist = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
            }

            tex.Apply();
            _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _circleSprite;
        }

        public static Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
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
            text.color = new Color(0.92f, 0.92f, 0.96f);
            return text;
        }

        public static RectTransform CreatePopup(Transform parent, Font font, string name,
            Vector2 anchorMin, Vector2 anchorMax, string title, string body)
        {
            var popupGo = new GameObject(name);
            popupGo.transform.SetParent(parent, false);
            var popupRect = popupGo.AddComponent<RectTransform>();
            popupRect.anchorMin = anchorMin;
            popupRect.anchorMax = anchorMax;
            popupRect.offsetMin = popupRect.offsetMax = Vector2.zero;

            var bg = popupGo.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.14f, 0.28f, 0.96f);

            var titleText = CreateText(popupGo.transform, "Title",
                new Vector2(0.06f, 0.72f), new Vector2(0.94f, 0.94f), 20, TextAnchor.UpperCenter, font);
            titleText.fontStyle = FontStyle.Bold;
            titleText.text = title;

            var bodyText = CreateText(popupGo.transform, "Body",
                new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.7f), 13, TextAnchor.UpperLeft, font);
            bodyText.text = body;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;

            popupGo.SetActive(false);
            return popupRect;
        }
    }
}
