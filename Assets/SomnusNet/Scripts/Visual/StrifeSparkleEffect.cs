using System.Collections;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Visual
{
    /// <summary>
    /// Strife proc VFX — vivid red crack burst with shockwave (distinct from Luck's orbiting sparkles).
    /// </summary>
    public class StrifeSparkleEffect : MonoBehaviour
    {
        const float Duration = 0.72f;
        const int ShardCount = 8;
        const float CrackHeight = 1.25f;
        const float ShardTravel = 1.15f;
        const int SortOrder = 25;

        static readonly Color CrackCore = new(1f, 0.12f, 0.06f, 1f);
        static readonly Color CrackEdge = new(0.62f, 0.02f, 0.02f, 1f);
        static readonly Color ShardColor = new(1f, 0.18f, 0.08f, 1f);
        static readonly Color ShockwaveColor = new(1f, 0.08f, 0.04f, 0.9f);
        static readonly Color GlowFlashColor = new(1f, 0.15f, 0.08f, 0.95f);
        static readonly Color BodyFlashColor = new(1f, 0.35f, 0.28f, 1f);

        static Sprite _crackSprite;
        static Sprite _shardSprite;
        static Sprite _ringSprite;

        Transform _root;
        SpriteRenderer _crack;
        SpriteRenderer _shockwave;
        SpriteRenderer[] _shards;
        float[] _shardAngles;
        SpriteRenderer _glow;
        SpriteRenderer _body;
        Vector3 _glowBaseScale;
        Color _glowBaseColor;
        Color _bodyBaseColor;

        public static void Play(DreamBlob blob)
        {
            if (blob == null || !blob.IsAlive)
                return;

            var existing = blob.GetComponent<StrifeSparkleEffect>();
            if (existing != null)
            {
                existing.StopAndCleanup();
                Destroy(existing);
            }

            blob.gameObject.AddComponent<StrifeSparkleEffect>();
        }

        void Start() => StartCoroutine(Animate());

        IEnumerator Animate()
        {
            EnsureSprites();
            CacheGlow();
            CacheBody();

            _shardAngles = new float[ShardCount];
            _shards = new SpriteRenderer[ShardCount];
            _root = new GameObject("StrifeCrackRoot").transform;
            _root.SetParent(transform);
            _root.localPosition = Vector3.zero;

            var shockGo = new GameObject("StrifeShockwave");
            shockGo.transform.SetParent(_root);
            shockGo.transform.localPosition = Vector3.zero;
            _shockwave = shockGo.AddComponent<SpriteRenderer>();
            _shockwave.sprite = _ringSprite;
            _shockwave.sortingOrder = SortOrder - 1;
            _shockwave.transform.localScale = Vector3.zero;

            var crackGo = new GameObject("StrifeCrack");
            crackGo.transform.SetParent(_root);
            crackGo.transform.localPosition = Vector3.zero;
            _crack = crackGo.AddComponent<SpriteRenderer>();
            _crack.sprite = _crackSprite;
            _crack.sortingOrder = SortOrder + 1;
            _crack.transform.localScale = new Vector3(0.22f, 0.05f, 1f);

            for (var i = 0; i < ShardCount; i++)
            {
                _shardAngles[i] = -75f + i * (150f / (ShardCount - 1));
                var shardGo = new GameObject($"Shard_{i}");
                shardGo.transform.SetParent(_root);
                var sr = shardGo.AddComponent<SpriteRenderer>();
                sr.sprite = _shardSprite;
                sr.color = ShardColor;
                sr.sortingOrder = SortOrder + 2;
                sr.transform.localScale = Vector3.one * 0.34f;
                sr.transform.rotation = Quaternion.Euler(0f, 0f, _shardAngles[i]);
                _shards[i] = sr;
            }

            var elapsed = 0f;
            while (elapsed < Duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / Duration);
                var fade = t < 0.3f ? 1f : 1f - ((t - 0.3f) / 0.7f);
                var rise = EaseOutBack(Mathf.Clamp01(t / 0.18f));
                var settle = 1f - Mathf.Clamp01((t - 0.15f) / 0.38f) * 0.3f;

                if (_shockwave != null)
                {
                    var waveT = Mathf.Clamp01(t / 0.42f);
                    var waveScale = Mathf.Lerp(0.35f, 1.85f, EaseOutQuad(waveT));
                    _shockwave.transform.localScale = Vector3.one * waveScale;
                    var waveTint = ShockwaveColor;
                    waveTint.a = fade * (1f - waveT) * 0.85f;
                    _shockwave.color = waveTint;
                }

                if (_crack != null)
                {
                    var height = CrackHeight * rise * settle;
                    _crack.transform.localScale = new Vector3(0.3f + rise * 0.16f, Mathf.Max(0.05f, height), 1f);
                    var crackTint = Color.Lerp(CrackEdge, CrackCore, rise);
                    crackTint.a = fade * (0.75f + rise * 0.25f);
                    _crack.color = crackTint;
                    _crack.transform.localPosition = new Vector3(0f, height * 0.24f, 0f);
                }

                for (var i = 0; i < ShardCount; i++)
                {
                    var sr = _shards[i];
                    if (sr == null)
                        continue;

                    var shardT = Mathf.Clamp01((t - 0.03f - i * 0.012f) / 0.4f);
                    var dist = ShardTravel * EaseOutQuad(shardT);
                    var angleRad = _shardAngles[i] * Mathf.Deg2Rad;
                    sr.transform.localPosition = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * dist;
                    var shardTint = ShardColor;
                    shardTint.a = fade * (1f - shardT * 0.75f);
                    sr.color = shardTint;
                    sr.transform.localScale = Vector3.one * (0.3f + (1f - shardT) * 0.16f);
                }

                PulseGlow(fade, rise);
                PulseBody(fade, rise);
                yield return null;
            }

            RestoreGlow();
            RestoreBody();
            Cleanup();
            Destroy(this);
        }

        static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

        static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
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

        void CacheBody()
        {
            _body = GetComponent<SpriteRenderer>();
            if (_body == null)
                return;

            _bodyBaseColor = _body.color;
        }

        void PulseGlow(float fade, float rise)
        {
            if (_glow == null)
                return;

            _glow.transform.localScale = _glowBaseScale * (1f + rise * 0.55f);
            var glowTint = GlowFlashColor;
            glowTint.a = Mathf.Clamp01(_glowBaseColor.a + fade * 0.75f);
            _glow.color = Color.Lerp(_glowBaseColor, glowTint, fade);
        }

        void PulseBody(float fade, float rise)
        {
            if (_body == null)
                return;

            var flash = Color.Lerp(_bodyBaseColor, BodyFlashColor, fade * (0.55f + rise * 0.45f));
            _body.color = flash;
        }

        void RestoreGlow()
        {
            if (_glow == null)
                return;

            _glow.transform.localScale = _glowBaseScale;
            _glow.color = _glowBaseColor;
        }

        void RestoreBody()
        {
            if (_body == null)
                return;

            _body.color = _bodyBaseColor;
        }

        void OnDestroy()
        {
            RestoreGlow();
            RestoreBody();
            Cleanup();
        }

        public void StopAndCleanup()
        {
            StopAllCoroutines();
            RestoreGlow();
            RestoreBody();
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
            if (_crackSprite == null)
                _crackSprite = CreateCrackSprite();

            if (_shardSprite == null)
                _shardSprite = CreateShardSprite();

            if (_ringSprite == null)
                _ringSprite = CreateRingSprite();
        }

        static Sprite CreateCrackSprite()
        {
            const int width = 18;
            const int height = 56;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var ny = y / (float)(height - 1);
                var nx = (x - width * 0.5f) / (width * 0.5f);
                var jag = Mathf.Sin(ny * 20f) * 0.24f;
                var dist = Mathf.Abs(nx - jag);
                var alpha = Mathf.Clamp01(1f - dist * 2.5f) * (0.45f + ny * 0.75f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 24f);
        }

        static Sprite CreateShardSprite()
        {
            const int size = 22;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x - size * 0.5f) / (size * 0.5f);
                var dy = (y - size * 0.5f) / (size * 0.5f);
                var along = dy * 0.85f + Mathf.Abs(dx) * 0.35f;
                var across = Mathf.Abs(dx) * 1.35f;
                var alpha = Mathf.Clamp01(1f - across) * Mathf.Clamp01(1f - along);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24f);
        }

        static Sprite CreateRingSprite()
        {
            const int size = 48;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.5f);
            const float outer = size * 0.48f;
            const float inner = size * 0.34f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dist = Vector2.Distance(new Vector2(x, y), center);
                var alpha = dist <= outer && dist >= inner ? 1f : 0f;
                if (alpha > 0f)
                {
                    var edge = Mathf.Min(dist - inner, outer - dist) / (outer - inner);
                    alpha *= Mathf.Clamp01(edge * 3f);
                }

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24f);
        }
    }
}
