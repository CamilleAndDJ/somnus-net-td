using UnityEngine;

namespace SomnusNet.Units
{
    public static class ArchivistBlobSprites
    {
        const float PixelsPerUnit = 40f;
        static readonly Vector2 Pivot = new(0.5f, 0.4f);
        static readonly Color Core = new(0.88f, 0.78f, 0.52f);
        static readonly Color Edge = new(0.52f, 0.42f, 0.28f);

        static Sprite _bodySprite;

        public static Sprite BodySprite
        {
            get
            {
                if (_bodySprite != null)
                    return _bodySprite;

                var resource = Resources.Load<Sprite>("ArchivistBlob");
                if (resource != null)
                {
                    _bodySprite = resource;
                    return _bodySprite;
                }

                _bodySprite = CreateSoftBlobSprite();
                return _bodySprite;
            }
        }

        public static Sprite ShopSprite => BodySprite;

        static Sprite CreateSoftBlobSprite()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.5f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var d = Vector2.Distance(new Vector2(x, y), center) / (size * 0.42f);
                var t = Mathf.Clamp01(1f - d);
                t = t * t * (3f - 2f * t);
                var ring = Mathf.Abs(Mathf.Sin((new Vector2(x, y) - center).magnitude * 0.45f));
                var c = Color.Lerp(Edge, Core, t);
                c = Color.Lerp(c, Color.white, ring * 0.22f * t);
                c.a = t > 0.05f ? 1f : 0f;
                tex.SetPixel(x, y, c);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), Pivot, PixelsPerUnit);
        }
    }
}
