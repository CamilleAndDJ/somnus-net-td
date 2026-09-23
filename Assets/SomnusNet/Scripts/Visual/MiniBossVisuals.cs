using SomnusNet.Data;
using UnityEngine;

namespace SomnusNet.Visual
{
    public static class MiniBossVisuals
    {
        static Sprite _overload;
        static Sprite _faze;
        static Sprite _sink;
        static Sprite _cicadianRhythm;

        public static Sprite GetSprite(GlitchKind kind)
        {
            return kind switch
            {
                GlitchKind.Overload => OverloadSprite,
                GlitchKind.Faze => FazeSprite,
                GlitchKind.Sink => SinkSprite,
                GlitchKind.CicadianRhythm => CicadianRhythmSprite,
                _ => null
            };
        }

        static Sprite OverloadSprite => _overload ??= CreateSprite(
            new Color(1f, 0.58f, 0.82f), new Color(0.78f, 0.24f, 0.62f));

        static Sprite FazeSprite => _faze ??= CreateSprite(
            new Color(0.78f, 0.96f, 1f), new Color(0.4f, 0.7f, 0.98f));

        static Sprite SinkSprite => _sink ??= CreateSprite(
            new Color(0.84f, 0.68f, 1f), new Color(0.5f, 0.3f, 0.92f));

        static Sprite CicadianRhythmSprite => _cicadianRhythm ??= CreateCicadianSprite();

        static Sprite CreateCicadianSprite()
        {
            const int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.42f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var d = Vector2.Distance(new Vector2(x, y), center) / (size * 0.38f);
                var t = Mathf.Clamp01(1f - d);
                t = t * t * (3f - 2f * t);
                if (t < 0.08f)
                {
                    tex.SetPixel(x, y, Color.clear);
                    continue;
                }

                var ring = Mathf.Sin((new Vector2(x, y) - center).magnitude * 0.55f) * 0.5f + 0.5f;
                var core = new Color(0.18f, 0.14f, 0.24f);
                var edge = new Color(0.42f, 0.34f, 0.58f);
                var c = Color.Lerp(edge, core, t);
                c = Color.Lerp(c, new Color(0.92f, 0.88f, 0.72f), ring * 0.55f * t);
                c.a = 1f;
                tex.SetPixel(x, y, c);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.35f), 40f);
        }

        static Sprite CreateSprite(Color core, Color edge)
        {
            const int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.42f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var d = Vector2.Distance(new Vector2(x, y), center) / (size * 0.38f);
                var t = Mathf.Clamp01(1f - d);
                t = t * t * (3f - 2f * t);
                if (t < 0.08f)
                {
                    tex.SetPixel(x, y, Color.clear);
                    continue;
                }

                var c = Color.Lerp(edge, core, t);
                c.a = 1f;
                if (t < 0.22f)
                    c = Color.Lerp(new Color(1f, 0.95f, 1f, 1f), c, t / 0.22f);
                tex.SetPixel(x, y, c);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.35f), 40f);
        }
    }
}
