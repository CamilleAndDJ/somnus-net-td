using System.Collections;
using SomnusNet.Core;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Visual
{
    public class LuckSparkleEffect : MonoBehaviour
    {
        const float Duration = 0.65f;
        const int SparkleCount = 12;
        const float OrbitRadius = 0.72f;
        const float SparkleSize = 0.38f;
        const float BurstSize = 1.05f;
        const int SortOrder = 25;

        static readonly Color SparkleCore = new(1f, 1f, 0.82f, 1f);
        static readonly Color SparkleEdge = new(0.45f, 1f, 0.55f, 1f);
        static readonly Color BurstColor = new(1f, 0.98f, 0.55f, 0.85f);
        static readonly Color GlowFlashColor = new(0.55f, 1f, 0.62f, 0.75f);

        static Sprite _sparkleSprite;
        static Sprite _burstSprite;

        Transform _root;
        SpriteRenderer _burst;
        SpriteRenderer[] _sparkles;
        SpriteRenderer _glow;
        Vector3 _glowBaseScale;
        Color _glowBaseColor;
        float[] _phaseOffsets;

        public static void Play(DreamBlob blob)
        {
            if (blob == null || !blob.IsAlive)
                return;

            var existing = blob.GetComponent<LuckSparkleEffect>();
            if (existing != null)
                Destroy(existing);

            blob.gameObject.AddComponent<LuckSparkleEffect>();
        }

        void Start() => StartCoroutine(Animate());

        IEnumerator Animate()
        {
            EnsureSprites();
            CacheGlow();

            _phaseOffsets = new float[SparkleCount];
            _sparkles = new SpriteRenderer[SparkleCount];
            _root = new GameObject("LuckSparkleRoot").transform;
            _root.SetParent(transform);
            _root.localPosition = Vector3.zero;

            var burstGo = new GameObject("LuckBurst");
            burstGo.transform.SetParent(_root);
            burstGo.transform.localPosition = Vector3.zero;
            _burst = burstGo.AddComponent<SpriteRenderer>();
            _burst.sprite = _burstSprite;
            _burst.sortingOrder = SortOrder;

            for (var i = 0; i < SparkleCount; i++)
            {
                _phaseOffsets[i] = i * (Mathf.PI * 2f / SparkleCount) + Random.Range(-0.2f, 0.2f);
                var sparkleGo = new GameObject($"Sparkle_{i}");
                sparkleGo.transform.SetParent(_root);
                var sr = sparkleGo.AddComponent<SpriteRenderer>();
                sr.sprite = _sparkleSprite;
                sr.sortingOrder = SortOrder + 1;
                _sparkles[i] = sr;
            }

            var elapsed = 0f;
            while (elapsed < Duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / Duration);
                var holdFade = t < 0.35f ? 1f : 1f - ((t - 0.35f) / 0.65f);
                var pop = t < 0.12f ? t / 0.12f : 1f;
                var expand = 1f + t * 0.55f;

                if (_burst != null)
                {
                    var burstScale = BurstSize * (0.35f + pop * 0.95f) * (1f - t * 0.35f);
                    _burst.transform.localScale = Vector3.one * burstScale;
                    var burstTint = BurstColor;
                    burstTint.a = holdFade * (0.55f + 0.45f * pop);
                    _burst.color = burstTint;
                    _burst.transform.rotation = Quaternion.Euler(0f, 0f, elapsed * 220f);
                }

                for (var i = 0; i < SparkleCount; i++)
                {
                    var angle = _phaseOffsets[i] + t * Mathf.PI * 3f;
                    var radius = OrbitRadius * expand;
                    var sr = _sparkles[i];
                    if (sr == null)
                        continue;

                    sr.transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
                    var twinkle = 0.72f + 0.28f * Mathf.Sin((t * 8f + i * 0.4f) * Mathf.PI);
                    var tint = Color.Lerp(SparkleEdge, SparkleCore, twinkle);
                    tint.a = holdFade * twinkle;
                    sr.color = tint;
                    sr.transform.localScale = Vector3.one * (SparkleSize * (0.85f + twinkle * 0.35f));
                    sr.transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + elapsed * 260f);
                }

                PulseGlow(holdFade, pop);
                yield return null;
            }

            RestoreGlow();
            Cleanup();
            Destroy(this);
        }

        void CacheGlow()
        {
            var glowTransform = transform.Find("Glow");
            if (glowTransform == null)
                return;

            _glow = glowTransform.GetComponent<SpriteRenderer>();
            if (_glow == null)
                return;

            _glowBaseScale = glowTransform.localScale;
            _glowBaseColor = _glow.color;
        }

        void PulseGlow(float fade, float pop)
        {
            if (_glow == null)
                return;

            var glowTransform = _glow.transform;
            glowTransform.localScale = _glowBaseScale * (1f + pop * 0.45f);
            var glowTint = GlowFlashColor;
            glowTint.a = _glowBaseColor.a + fade * 0.55f;
            _glow.color = Color.Lerp(_glowBaseColor, glowTint, fade * 0.85f);
        }

        void RestoreGlow()
        {
            if (_glow == null)
                return;

            _glow.transform.localScale = _glowBaseScale;
            _glow.color = _glowBaseColor;
        }

        void OnDestroy()
        {
            RestoreGlow();
            Cleanup();
        }

        void Cleanup()
        {
            if (_root != null)
            {
                Destroy(_root.gameObject);
                _root = null;
            }
        }

        static void EnsureSprites()
        {
            if (_sparkleSprite == null)
                _sparkleSprite = CreateStarSprite(24, sharp: true);

            if (_burstSprite == null)
                _burstSprite = CreateStarSprite(32, sharp: false);
        }

        static Sprite CreateStarSprite(int size, bool sharp)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.5f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x - center.x) / center.x;
                var dy = (y - center.y) / center.y;
                var dist = Mathf.Sqrt(dx * dx + dy * dy);
                var angle = Mathf.Atan2(dy, dx);
                var points = sharp ? 4 : 8;
                var starRadius = 0.35f + 0.65f * Mathf.Abs(Mathf.Cos(angle * points));
                var alpha = Mathf.Clamp01(1f - dist / starRadius);
                alpha *= alpha;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24f);
        }
    }
}
