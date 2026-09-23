using System.Collections;
using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.UI;
using SomnusNet.Units;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.Visual
{
    public class BondCompleteSparkleEffect : MonoBehaviour
    {
        const float Duration = 0.75f;
        const int ShardCount = 10;
        const int WorldSortOrder = 26;

        static readonly Color ShardInner = new(1f, 0.94f, 0.55f, 1f);
        static readonly Color ShardOuter = new(0.92f, 0.72f, 0.18f, 1f);
        static readonly Color RingColor = new(1f, 0.86f, 0.32f, 0.82f);

        static Sprite _shardSprite;
        static Sprite _ringSprite;

        enum Mode
        {
            UiIcon,
            WorldBlob
        }

        Mode _mode;
        RectTransform _icon;
        Transform _blobRoot;
        Image _ring;
        Image[] _uiShards;
        SpriteRenderer _worldRing;
        SpriteRenderer[] _worldShards;
        float _burstRadius;
        float _shardSize;

        public static void PlayOnIcon(RectTransform icon)
        {
            var host = BondEffectOverlay.CreateIconHost(icon);
            if (host == null)
                return;

            var go = new GameObject("BondCompleteSparkle");
            go.transform.SetParent(host, false);

            var rt = go.AddComponent<RectTransform>();
            StretchFull(rt);

            var effect = go.AddComponent<BondCompleteSparkleEffect>();
            effect._mode = Mode.UiIcon;
            effect._icon = icon;
        }

        public static void PlayOnBlob(DreamBlob blob)
        {
            if (blob == null || !blob.IsAlive)
                return;

            var go = new GameObject("BondCompleteSparkleWorld");
            go.transform.position = blob.transform.position;
            var effect = go.AddComponent<BondCompleteSparkleEffect>();
            effect._mode = Mode.WorldBlob;
            effect._blobRoot = blob.transform;
        }

        public static void PlayOnGroupMembers(TypeHudLayout.Kind kind)
        {
            var roster = GetRoster(kind);
            if (roster == null || roster.Length == 0 || GridManager.Instance == null)
                return;

            GridManager.Instance.ForEachBlob(blob =>
            {
                if (blob == null || !blob.IsAlive)
                    return;

                foreach (var memberKind in roster)
                {
                    if (blob.Kind != memberKind)
                        continue;

                    PlayOnBlob(blob);
                    return;
                }
            });
        }

        static BlobKind[] GetRoster(TypeHudLayout.Kind kind) => kind switch
        {
            TypeHudLayout.Kind.DreamTeam => DreamTeamTypeRules.MemberKinds,
            TypeHudLayout.Kind.BrokenRings => BrokenRingsTypeRules.MemberKinds,
            TypeHudLayout.Kind.SlimySupport => SlimySupportTypeRules.MemberKinds,
            TypeHudLayout.Kind.BenchTrio => BenchTrioTypeRules.MemberKinds,
            _ => null
        };

        void Start() => StartCoroutine(Animate());

        IEnumerator Animate()
        {
            EnsureSprites();

            if (_mode == Mode.UiIcon)
                yield return AnimateUi();
            else
                yield return AnimateWorld();
        }

        IEnumerator AnimateUi()
        {
            if (_icon != null)
                BondEffectOverlay.SyncHostToIcon(_icon, transform.parent as RectTransform);

            var iconSize = _icon != null
                ? Mathf.Max(Mathf.Min(_icon.rect.width, _icon.rect.height), 28f)
                : 48f;
            _burstRadius = iconSize * 0.72f;
            _shardSize = iconSize * 0.22f;

            _uiShards = new Image[ShardCount];

            var ringGo = new GameObject("BondCompleteRing", typeof(RectTransform));
            ringGo.transform.SetParent(transform, false);
            var ringRt = ringGo.GetComponent<RectTransform>();
            ringRt.anchorMin = ringRt.anchorMax = new Vector2(0.5f, 0.5f);
            ringRt.anchoredPosition = Vector2.zero;
            ringRt.sizeDelta = Vector2.one * iconSize * 1.15f;
            _ring = ringGo.AddComponent<Image>();
            _ring.sprite = _ringSprite;
            _ring.raycastTarget = false;

            for (var i = 0; i < ShardCount; i++)
            {
                var shardGo = new GameObject($"Shard_{i}", typeof(RectTransform));
                shardGo.transform.SetParent(transform, false);
                var rt = shardGo.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.one * _shardSize;

                var img = shardGo.AddComponent<Image>();
                img.sprite = _shardSprite;
                img.raycastTarget = false;
                _uiShards[i] = img;
            }

            var elapsed = 0f;
            while (elapsed < Duration)
            {
                if (_icon != null)
                    BondEffectOverlay.SyncHostToIcon(_icon, transform.parent as RectTransform);

                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / Duration);
                ApplyFrame(t, true);
                yield return null;
            }

            Destroy(transform.parent != null ? transform.parent.gameObject : gameObject);
        }

        IEnumerator AnimateWorld()
        {
            var baseScale = GridManager.Instance != null ? GridManager.Instance.BlobVisualScale : 1f;
            _burstRadius = 0.72f * baseScale;
            _shardSize = 0.2f * baseScale;

            _worldShards = new SpriteRenderer[ShardCount];

            var ringGo = new GameObject("BondCompleteRing");
            ringGo.transform.SetParent(transform);
            ringGo.transform.localPosition = Vector3.zero;
            _worldRing = ringGo.AddComponent<SpriteRenderer>();
            _worldRing.sprite = _ringSprite;
            _worldRing.sortingOrder = WorldSortOrder;

            for (var i = 0; i < ShardCount; i++)
            {
                var shardGo = new GameObject($"Shard_{i}");
                shardGo.transform.SetParent(transform);
                var sr = shardGo.AddComponent<SpriteRenderer>();
                sr.sprite = _shardSprite;
                sr.sortingOrder = WorldSortOrder + 1;
                _worldShards[i] = sr;
            }

            var elapsed = 0f;
            while (elapsed < Duration)
            {
                if (_blobRoot != null)
                    transform.position = _blobRoot.position;

                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / Duration);
                ApplyFrame(t, false);
                yield return null;
            }

            Destroy(gameObject);
        }

        void ApplyFrame(float t, bool ui)
        {
            var pop = t < 0.18f ? t / 0.18f : 1f;
            var fade = t < 0.4f ? 1f : 1f - ((t - 0.4f) / 0.6f);

            if (ui && _ring != null)
            {
                var ringScale = (0.55f + pop * 0.95f) * (1f - t * 0.25f);
                _ring.rectTransform.localScale = Vector3.one * ringScale;
                var ringTint = RingColor;
                ringTint.a = fade * 0.85f;
                _ring.color = ringTint;
                _ring.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Time.time * 140f);
            }
            else if (!ui && _worldRing != null)
            {
                var ringScale = (0.55f + pop * 0.95f) * (1f - t * 0.25f);
                _worldRing.transform.localScale = Vector3.one * ringScale * 1.2f;
                var ringTint = RingColor;
                ringTint.a = fade * 0.85f;
                _worldRing.color = ringTint;
                _worldRing.transform.rotation = Quaternion.Euler(0f, 0f, Time.time * 140f);
            }

            for (var i = 0; i < ShardCount; i++)
            {
                var angle = i * (Mathf.PI * 2f / ShardCount) + t * 1.4f;
                var radius = _burstRadius * (0.25f + t * 0.95f);
                var twinkle = 0.7f + 0.3f * Mathf.Sin((t * 10f + i) * Mathf.PI);
                var tint = Color.Lerp(ShardOuter, ShardInner, twinkle);
                tint.a = fade * twinkle;

                if (ui && _uiShards != null && _uiShards[i] != null)
                {
                    var rt = _uiShards[i].rectTransform;
                    rt.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    _uiShards[i].color = tint;
                    rt.localScale = Vector3.one * (0.85f + twinkle * 0.35f);
                    rt.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 45f);
                }
                else if (!ui && _worldShards != null && _worldShards[i] != null)
                {
                    var pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
                    _worldShards[i].transform.localPosition = pos;
                    _worldShards[i].color = tint;
                    _worldShards[i].transform.localScale = Vector3.one * (_shardSize * (0.85f + twinkle * 0.35f));
                    _worldShards[i].transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 45f);
                }
            }
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void EnsureSprites()
        {
            if (_shardSprite == null)
            {
                const int size = 20;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var center = new Vector2(size * 0.5f, size * 0.5f);
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var dx = Mathf.Abs(x - center.x) / center.x;
                    var dy = Mathf.Abs(y - center.y) / center.y;
                    var diamond = dx + dy;
                    var alpha = Mathf.Clamp01(1.15f - diamond);
                    alpha *= alpha;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }

                tex.Apply();
                _shardSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24f);
            }

            if (_ringSprite != null)
                return;

            const int ringSize = 24;
            var ringTex = new Texture2D(ringSize, ringSize, TextureFormat.RGBA32, false);
            var ringCenter = new Vector2(ringSize * 0.5f, ringSize * 0.5f);
            for (var y = 0; y < ringSize; y++)
            for (var x = 0; x < ringSize; x++)
            {
                var dist = Vector2.Distance(new Vector2(x, y), ringCenter) / (ringSize * 0.5f);
                var alpha = dist > 0.72f && dist < 0.95f ? 1f : 0f;
                ringTex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

            ringTex.Apply();
            _ringSprite = Sprite.Create(ringTex, new Rect(0, 0, ringSize, ringSize), new Vector2(0.5f, 0.5f), 24f);
        }
    }
}
