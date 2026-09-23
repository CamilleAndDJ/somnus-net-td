using UnityEngine;

namespace SomnusNet.Visual
{
    public static class GlitchVisual
    {
        static Sprite _fallback;

        public static Sprite FallbackSprite
        {
            get
            {
                if (_fallback != null) return _fallback;
                const int size = 64;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var center = new Vector2(size * 0.5f, size * 0.42f);
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var d = Vector2.Distance(new Vector2(x, y), center) / (size * 0.38f);
                    var t = Mathf.Clamp01(1f - d);
                    var c = Color.Lerp(new Color(0.65f, 0.4f, 0.95f), new Color(0.92f, 0.78f, 1f), t);
                    c.a = t > 0.05f ? 1f : 0f;
                    tex.SetPixel(x, y, c);
                }
                tex.Apply();
                _fallback = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.35f), 32f);
                return _fallback;
            }
        }
    }
}
