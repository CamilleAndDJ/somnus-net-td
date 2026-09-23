using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.Units
{
    public static class CountdownBlobSprites
    {
        static readonly Color BodyColor = new(0.98f, 0.52f, 0.22f);
        static readonly Color EdgeColor = new(0.72f, 0.18f, 0.12f);
        static readonly Vector2 Pivot = new(0.5f, 0.4f);

        static Sprite _shopSprite;

        public static Sprite ShopSprite
        {
            get
            {
                EnsureLoaded();
                return _shopSprite;
            }
        }

        static void EnsureLoaded()
        {
            if (_shopSprite != null)
                return;

            _shopSprite = SpritesheetLoader.LoadSpriteTrimmed(
                "CountdownBlob",
                GridManager.BlobSpriteWorldSize * 1.08f,
                Pivot);

            if (_shopSprite == null)
                _shopSprite = CreateProceduralSprite();
        }

        static Sprite CreateProceduralSprite()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.42f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x - center.x) / (size * 0.34f);
                var dy = (y - center.y) / (size * 0.36f);
                var dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > 1.15f)
                {
                    tex.SetPixel(x, y, Color.clear);
                    continue;
                }

                var edge = Mathf.Clamp01((dist - 0.72f) / 0.28f);
                var color = Color.Lerp(BodyColor, EdgeColor, edge);
                var alpha = Mathf.Clamp01(1.05f - dist);
                color.a = alpha;
                tex.SetPixel(x, y, color);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), Pivot, 40f);
        }
    }
}
