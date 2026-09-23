using UnityEngine;

namespace SomnusNet.Units
{
    public static class FourOhFourShotSprites
    {
        const float PixelsPerUnit = 32f;
        static readonly Vector2 Pivot = new(0.5f, 0.5f);
        static readonly Color StandardCore = new(0.72f, 0.9f, 1f);
        static readonly Color StandardEdge = new(0.42f, 0.68f, 0.92f);
        static readonly Color SporesCore = new(0.86f, 0.97f, 1f);
        static readonly Color SporesEdge = new(0.52f, 0.78f, 0.98f);

        static Sprite _standardSprite;
        static Sprite _sporesSprite;

        public static Sprite GetSprite(ProjectileBehavior behavior) =>
            behavior == ProjectileBehavior.Spores ? SporesSprite : StandardSprite;

        public static Sprite StandardSprite
        {
            get
            {
                if (_standardSprite != null)
                    return _standardSprite;

                _standardSprite = CreateSoftShotSprite(StandardCore, StandardEdge, 0.36f);
                return _standardSprite;
            }
        }

        public static Sprite SporesSprite
        {
            get
            {
                if (_sporesSprite != null)
                    return _sporesSprite;

                _sporesSprite = CreateSoftShotSprite(SporesCore, SporesEdge, 0.42f);
                return _sporesSprite;
            }
        }

        static Sprite CreateSoftShotSprite(Color core, Color edge, float radiusScale)
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.5f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var d = Vector2.Distance(new Vector2(x, y), center) / (size * radiusScale);
                var t = Mathf.Clamp01(1f - d);
                t = t * t * (3f - 2f * t);
                var c = Color.Lerp(edge, core, t);
                c.a = t > 0.04f ? 1f : 0f;
                tex.SetPixel(x, y, c);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), Pivot, PixelsPerUnit);
        }
    }
}
