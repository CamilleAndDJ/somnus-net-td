using UnityEngine;

namespace SomnusNet.Editor
{
    public static class SpriteFactory
    {
        public const float DefaultPpu = 40f;

        public static Texture2D SoftBlobTexture(Color core, Color edge, int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.5f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var d = Vector2.Distance(new Vector2(x, y), center) / (size * 0.42f);
                var t = Mathf.Clamp01(1f - d);
                t = t * t * (3f - 2f * t);
                var c = Color.Lerp(edge, core, t);
                c.a = t > 0.05f ? 1f : 0f;
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Solid light-purple glitch — same fill style as blobs so textures persist correctly.</summary>
        public static Texture2D GlitchTexture(Color core, Color edge, int size = 80, bool angular = false)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.42f);

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                float t;
                if (angular)
                {
                    var dx = Mathf.Abs(x - center.x) / (size * 0.36f);
                    var dy = Mathf.Abs(y - center.y) / (size * 0.34f);
                    t = 1f - (dx + dy);
                }
                else
                {
                    var d = Vector2.Distance(new Vector2(x, y), center) / (size * 0.38f);
                    t = 1f - d;
                }

                t = Mathf.Clamp01(t);
                t = t * t * (3f - 2f * t);
                if (t < 0.08f)
                {
                    tex.SetPixel(x, y, Color.clear);
                    continue;
                }

                var c = Color.Lerp(edge, core, t);
                c.a = 1f;
                // White fringe so glitches pop on the dark Somnus backdrop
                if (t < 0.22f)
                    c = Color.Lerp(new Color(1f, 0.95f, 1f, 1f), c, t / 0.22f);
                tex.SetPixel(x, y, c);
            }

            tex.Apply();
            return tex;
        }

        public static Texture2D ShellOverlayTexture(int size = 80)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.42f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var d = Vector2.Distance(new Vector2(x, y), center) / (size * 0.44f);
                var ring = Mathf.Abs(d - 0.88f) < 0.11f;
                tex.SetPixel(x, y, ring ? new Color(1f, 0.85f, 1f, 0.9f) : Color.clear);
            }
            tex.Apply();
            return tex;
        }
    }
}
