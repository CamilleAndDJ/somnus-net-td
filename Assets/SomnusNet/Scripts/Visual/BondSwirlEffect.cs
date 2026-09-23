using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.Visual
{
    public class BondSwirlEffect : MonoBehaviour
    {
        const float Duration = 0.62f;
        const int ArcCount = 3;

        static readonly Color ArcColorA = new(0.62f, 0.88f, 1f, 0.92f);
        static readonly Color ArcColorB = new(0.78f, 0.94f, 1f, 0.88f);

        static Sprite _arcSprite;

        RectTransform _icon;
        RectTransform _root;
        Image[] _arcs;
        float _orbitRadius;
        float _arcSize;

        public static void PlayOnIcon(RectTransform icon)
        {
            var host = BondEffectOverlay.CreateIconHost(icon);
            if (host == null)
                return;

            var go = new GameObject("BondSwirl");
            go.transform.SetParent(host, false);

            var rt = go.AddComponent<RectTransform>();
            StretchFull(rt);

            var effect = go.AddComponent<BondSwirlEffect>();
            effect._icon = icon;
        }

        void Start() => StartCoroutine(Animate());

        IEnumerator Animate()
        {
            EnsureArcSprite();

            if (_icon != null)
                BondEffectOverlay.SyncHostToIcon(_icon, transform.parent as RectTransform);

            var iconSize = _icon != null
                ? Mathf.Max(Mathf.Min(_icon.rect.width, _icon.rect.height), 28f)
                : 48f;
            _orbitRadius = iconSize * 0.62f;
            _arcSize = iconSize * 0.42f;

            _arcs = new Image[ArcCount];
            _root = new GameObject("BondSwirlRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            _root.SetParent(transform, false);
            StretchFull(_root);

            for (var i = 0; i < ArcCount; i++)
            {
                var arcGo = new GameObject($"Arc_{i}", typeof(RectTransform));
                arcGo.transform.SetParent(_root, false);
                var rt = arcGo.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(_arcSize, _arcSize * 0.48f);

                var img = arcGo.AddComponent<Image>();
                img.sprite = _arcSprite;
                img.raycastTarget = false;
                _arcs[i] = img;
            }

            var elapsed = 0f;
            while (elapsed < Duration)
            {
                if (_icon != null)
                    BondEffectOverlay.SyncHostToIcon(_icon, transform.parent as RectTransform);

                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / Duration);
                var fade = t < 0.45f ? 1f : 1f - ((t - 0.45f) / 0.55f);
                var radius = _orbitRadius * (0.65f + t * 0.55f);

                for (var i = 0; i < ArcCount; i++)
                {
                    var img = _arcs[i];
                    if (img == null)
                        continue;

                    var rt = img.rectTransform;
                    var spin = (t * 540f + i * 120f) * Mathf.Deg2Rad;
                    rt.anchoredPosition = new Vector2(Mathf.Cos(spin), Mathf.Sin(spin)) * radius;
                    rt.localRotation = Quaternion.Euler(0f, 0f, spin * Mathf.Rad2Deg + 90f + i * 35f);
                    rt.localScale = Vector3.one * (0.85f + 0.2f * i);

                    var tint = Color.Lerp(ArcColorA, ArcColorB, i / (float)(ArcCount - 1));
                    tint.a = fade * 0.9f;
                    img.color = tint;
                }

                yield return null;
            }

            Destroy(transform.parent != null ? transform.parent.gameObject : gameObject);
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void EnsureArcSprite()
        {
            if (_arcSprite != null)
                return;

            const int width = 32;
            const int height = 16;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var along = x / (float)(width - 1);
                var across = Mathf.Abs(y - height * 0.5f) / (height * 0.5f);
                var band = along > 0.08f && along < 0.92f ? 1f : 0f;
                var thickness = 1f - across;
                thickness *= thickness;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, band * thickness));
            }

            tex.Apply();
            _arcSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 24f);
        }
    }
}
