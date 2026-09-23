using UnityEngine;

namespace SomnusNet.Visual
{
    public static class LoopedVisual
    {
        static readonly Color[] RectangleColors =
        {
            Color.black,
            Color.white,
            new(0.55f, 0.55f, 0.55f)
        };

        static Sprite _rectangleSprite;

        public static void Apply(Transform parent)
        {
            if (parent == null)
                return;

            EnsureRectangleSprite();

            var root = new GameObject("LoopedOverlay");
            root.transform.SetParent(parent, false);

            const int count = 5;
            for (var i = 0; i < count; i++)
            {
                var rectGo = new GameObject($"LoopedRect_{i}");
                rectGo.transform.SetParent(root.transform, false);

                var sr = rectGo.AddComponent<SpriteRenderer>();
                sr.sprite = _rectangleSprite;
                sr.color = RectangleColors[i % RectangleColors.Length];
                sr.sortingOrder = 12;

                var y = Random.value < 0.85f
                    ? Random.Range(0.18f, 0.48f)
                    : Random.Range(0.02f, 0.18f);
                rectGo.transform.localPosition = new Vector3(
                    Random.Range(-0.34f, 0.34f),
                    y,
                    0f);
                rectGo.transform.localScale = new Vector3(
                    Random.Range(0.26f, 0.48f),
                    Random.Range(0.08f, 0.16f),
                    1f);
                rectGo.transform.localRotation = Quaternion.identity;
            }
        }

        static void EnsureRectangleSprite()
        {
            if (_rectangleSprite != null)
                return;

            const int width = 8;
            const int height = 8;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                tex.SetPixel(x, y, Color.white);

            tex.Apply();
            _rectangleSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 8f);
        }
    }
}
