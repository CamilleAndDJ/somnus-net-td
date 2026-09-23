using System.Collections;
using System.Collections.Generic;
using SomnusNet.Core;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Visual
{
    public class LanternPulseEffect : MonoBehaviour
    {
        const float WaveStepDelay = 0.09f;
        const float RippleDuration = 0.42f;
        const float RingWaveDuration = 0.55f;

        static readonly Color WaveFrontColor = new(0.82f, 0.94f, 1f, 0.78f);
        static readonly Color WaveTrailColor = new(0.48f, 0.68f, 0.98f, 0.38f);
        static readonly Color RingColor = new(0.62f, 0.82f, 1f, 0.55f);

        static Sprite _ringSprite;
        static Sprite _softSprite;

        public static void Play(DreamBlob lantern, int rangeSize)
        {
            var grid = GridManager.Instance;
            if (grid == null || lantern == null)
                return;

            var go = new GameObject("LanternPulseWave");
            go.transform.position = lantern.transform.position;
            var effect = go.AddComponent<LanternPulseEffect>();
            effect.StartCoroutine(effect.Animate(lantern, grid, rangeSize));
        }

        IEnumerator Animate(DreamBlob lantern, GridManager grid, int rangeSize)
        {
            EnsureSprites();

            var originCol = lantern.Column;
            var originRow = lantern.Lane;
            grid.GetBlobRangeBounds(originCol, originRow, out var minCol, out var maxCol, out var minRow,
                out var maxRow, rangeSize);

            var cellsByRing = new Dictionary<int, List<Vector3>>();
            var maxRing = 0;
            for (var c = minCol; c <= maxCol; c++)
            for (var r = minRow; r <= maxRow; r++)
            {
                if (!grid.IsInside(c, r))
                    continue;

                var ring = Mathf.Max(Mathf.Abs(c - originCol), Mathf.Abs(r - originRow));
                maxRing = Mathf.Max(maxRing, ring);
                if (!cellsByRing.TryGetValue(ring, out var list))
                {
                    list = new List<Vector3>();
                    cellsByRing[ring] = list;
                }

                list.Add(grid.CellToWorld(c, r));
            }

            StartCoroutine(AnimateRingWaves(grid.cellSize));

            for (var ring = 0; ring <= maxRing; ring++)
            {
                if (cellsByRing.TryGetValue(ring, out var cells))
                {
                    foreach (var pos in cells)
                        StartCoroutine(AnimateCellRipple(pos, grid.cellSize, ring == 0));
                }

                yield return new WaitForSeconds(WaveStepDelay);
            }

            yield return new WaitForSeconds(RippleDuration + 0.08f);
            Destroy(gameObject);
        }

        IEnumerator AnimateRingWaves(float cellSize)
        {
            for (var i = 0; i < 2; i++)
            {
                var ringGo = new GameObject($"LanternPulseRing_{i}");
                ringGo.transform.SetParent(transform);
                ringGo.transform.localPosition = Vector3.zero;
                var sr = ringGo.AddComponent<SpriteRenderer>();
                sr.sprite = _ringSprite;
                sr.color = RingColor;
                sr.sortingOrder = 11;
                StartCoroutine(ExpandRing(sr, cellSize * (1.1f + i * 0.35f), RingWaveDuration));
                yield return new WaitForSeconds(WaveStepDelay * 0.85f);
            }
        }

        static IEnumerator ExpandRing(SpriteRenderer sr, float targetScale, float duration)
        {
            if (sr == null)
                yield break;

            var start = targetScale * 0.25f;
            var elapsed = 0f;
            var baseColor = sr.color;
            while (elapsed < duration && sr != null)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                var scale = Mathf.Lerp(start, targetScale, t);
                sr.transform.localScale = Vector3.one * scale;
                sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - t));
                yield return null;
            }

            if (sr != null)
                Destroy(sr.gameObject);
        }

        static IEnumerator AnimateCellRipple(Vector3 position, float cellSize, bool center)
        {
            var cellGo = new GameObject("LanternPulseRipple");
            cellGo.transform.position = position;
            var sr = cellGo.AddComponent<SpriteRenderer>();
            sr.sprite = _softSprite;
            sr.sortingOrder = center ? 13 : 12;
            sr.color = center ? WaveFrontColor : WaveTrailColor;

            var startScale = cellSize * 0.22f;
            var endScale = cellSize * (center ? 1.05f : 0.92f);
            var elapsed = 0f;
            while (elapsed < RippleDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / RippleDuration;
                var ease = 1f - (1f - t) * (1f - t);
                sr.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, ease);
                var alpha = center
                    ? Mathf.Lerp(WaveFrontColor.a, 0f, t)
                    : Mathf.Lerp(WaveTrailColor.a, 0f, t * 1.08f);
                var baseColor = center ? WaveFrontColor : WaveTrailColor;
                sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                yield return null;
            }

            Destroy(cellGo);
        }

        static void EnsureSprites()
        {
            if (_ringSprite == null)
                _ringSprite = CreateRingSprite();

            if (_softSprite == null)
                _softSprite = CreateSoftSprite();
        }

        static Sprite CreateRingSprite()
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.5f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dist = Vector2.Distance(new Vector2(x, y), center) / (size * 0.42f);
                var alpha = dist > 0.62f && dist < 1f ? Mathf.Clamp01(1.25f - dist) : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }

        static Sprite CreateSoftSprite()
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.5f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dist = Vector2.Distance(new Vector2(x, y), center) / (size * 0.38f);
                var alpha = dist <= 1f ? Mathf.Clamp01(1.05f - dist) : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * 0.85f));
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
