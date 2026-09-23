using UnityEngine;

namespace SomnusNet.Visual
{
    public static class CheerfulHeartSprites
    {
        static Sprite _heartSprite;
        static Sprite _projectileSprite;

        public static Sprite HeartSprite
        {
            get
            {
                if (_heartSprite == null)
                    _heartSprite = CreateHeartSprite(40);
                return _heartSprite;
            }
        }

        public static Sprite ProjectileSprite
        {
            get
            {
                if (_projectileSprite == null)
                    _projectileSprite = CreateHeartSprite(28);
                return _projectileSprite;
            }
        }

        static Sprite CreateHeartSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var centerX = size * 0.5f;
            var centerY = size * 0.48f;
            var scale = size * 0.19f;

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var u = (x - centerX) / scale;
                var v = (y - centerY) / scale;
                var alpha = SampleHeartAlpha(u, v);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.38f), size);
        }

        static float SampleHeartAlpha(float u, float v)
        {
            var inside = IsInsideHeart(u, v);
            if (!inside)
                return 0f;

            const float outline = 0.08f;
            var edge = Mathf.Min(
                Mathf.Min(DistanceToHeartEdge(u, v), DistanceToHeartEdge(u + outline, v)),
                DistanceToHeartEdge(u, v + outline));
            return Mathf.Clamp01(0.35f + edge * 2.4f);
        }

        static bool IsInsideHeart(float u, float v)
        {
            const float lobeRadius = 0.95f;
            const float lobeY = 0.42f;
            const float lobeX = 0.52f;

            if (Vector2.Distance(new Vector2(u + lobeX, v - lobeY), Vector2.zero) <= lobeRadius)
                return true;
            if (Vector2.Distance(new Vector2(u - lobeX, v - lobeY), Vector2.zero) <= lobeRadius)
                return true;

            if (v > -1.05f && v < lobeY - lobeRadius * 0.55f)
            {
                var tipY = -1.05f;
                var baseY = lobeY - lobeRadius * 0.55f;
                var t = (v - tipY) / (baseY - tipY);
                var halfWidth = Mathf.Lerp(0.04f, 1.05f, t);
                if (Mathf.Abs(u) <= halfWidth)
                    return true;
            }

            return false;
        }

        static float DistanceToHeartEdge(float u, float v)
        {
            if (!IsInsideHeart(u, v))
                return 0f;

            const float step = 0.04f;
            var minDist = 999f;
            for (var dy = -1; dy <= 1; dy++)
            for (var dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                var nu = u + dx * step;
                var nv = v + dy * step;
                if (IsInsideHeart(nu, nv))
                    continue;

                minDist = Mathf.Min(minDist, step);
            }

            return minDist < 999f ? minDist : step;
        }
    }
}
