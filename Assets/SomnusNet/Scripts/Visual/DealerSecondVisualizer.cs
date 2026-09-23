using System.Collections.Generic;
using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.UI;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Visual
{
    /// <summary>Red glow on the blob whose attack speed Dealer copies, or on Dealer itself when self-buffing, while its upgrade panel is open.</summary>
    public class DealerSecondVisualizer : MonoBehaviour
    {
        static readonly Color BandFill = new(1f, 0.35f, 0.32f, 0.18f);
        static readonly Color BandRing = new(1f, 0.42f, 0.38f, 0.42f);

        static Sprite _cellSprite;
        static Sprite _ringSprite;

        readonly Dictionary<DreamBlob, BandMarker> _markers = new();
        readonly List<DreamBlob> _stale = new();

        Transform _root;

        struct BandMarker
        {
            public SpriteRenderer fill;
            public SpriteRenderer ring;
        }

        public static void EnsureExists()
        {
            if (Object.FindFirstObjectByType<DealerSecondVisualizer>() != null) return;

            var grid = GridManager.Instance;
            if (grid != null)
                grid.gameObject.AddComponent<DealerSecondVisualizer>();
        }

        void Awake()
        {
            _root = new GameObject("DealerSecondOverlay").transform;
            _root.SetParent(transform);
        }

        void LateUpdate()
        {
            SyncCopiedBlobMarker();
            PulseMarkers();
        }

        void OnDestroy() => ClearMarkers();

        void SyncCopiedBlobMarker()
        {
            DreamBlob highlight = null;

            var panel = BlobUpgradePanel.Instance;
            var selected = panel != null && panel.IsOpen ? panel.SelectedBlob : null;
            if (selected != null && selected.IsAlive && selected.Kind == BlobKind.Dealer && selected.HasSecond)
            {
                var copied = DealerSecondSynergy.GetCopiedBlob(selected);
                highlight = copied != null ? copied : selected;
            }

            _stale.Clear();
            foreach (var blob in _markers.Keys)
            {
                if (blob == null || !blob.IsAlive || blob != highlight)
                    _stale.Add(blob);
            }

            foreach (var blob in _stale)
                RemoveMarker(blob);

            if (highlight != null)
                EnsureMarker(highlight);
        }

        void EnsureMarker(DreamBlob blob)
        {
            if (_markers.ContainsKey(blob)) return;

            EnsureSprites();
            var grid = GridManager.Instance;
            if (grid == null) return;

            var cellGo = new GameObject($"DealerSecond_{blob.Column}_{blob.Lane}");
            cellGo.transform.SetParent(_root);
            cellGo.transform.position = grid.CellToWorld(blob.Column, blob.Lane);

            var fill = cellGo.AddComponent<SpriteRenderer>();
            fill.sprite = _cellSprite;
            fill.drawMode = SpriteDrawMode.Sliced;
            fill.size = Vector2.one * grid.cellSize * 0.94f;
            fill.color = BandFill;
            fill.sortingOrder = -7;

            var ringGo = new GameObject("Ring");
            ringGo.transform.SetParent(cellGo.transform);
            ringGo.transform.localPosition = Vector3.zero;
            var ring = ringGo.AddComponent<SpriteRenderer>();
            ring.sprite = _ringSprite;
            ring.drawMode = SpriteDrawMode.Sliced;
            ring.size = Vector2.one * grid.cellSize * 1.02f;
            ring.color = BandRing;
            ring.sortingOrder = -6;

            _markers[blob] = new BandMarker { fill = fill, ring = ring };
        }

        void RemoveMarker(DreamBlob blob)
        {
            if (!_markers.TryGetValue(blob, out var marker)) return;

            if (marker.fill != null)
                Destroy(marker.fill.gameObject);
            _markers.Remove(blob);
        }

        void ClearMarkers()
        {
            foreach (var marker in _markers.Values)
            {
                if (marker.fill != null)
                    Destroy(marker.fill.gameObject);
            }

            _markers.Clear();
        }

        void PulseMarkers()
        {
            var pulse = 0.88f + 0.12f * Mathf.Sin(Time.time * 2.4f);
            foreach (var marker in _markers.Values)
            {
                if (marker.fill != null)
                {
                    var fill = BandFill;
                    fill.a *= pulse;
                    marker.fill.color = fill;
                }

                if (marker.ring != null)
                {
                    var ring = BandRing;
                    ring.a *= pulse;
                    marker.ring.color = ring;
                }
            }
        }

        static void EnsureSprites()
        {
            if (_cellSprite == null)
            {
                const int s = 8;
                var tex = new Texture2D(s, s);
                for (var y = 0; y < s; y++)
                for (var x = 0; x < s; x++)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.9f));
                tex.Apply();
                _cellSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
            }

            if (_ringSprite != null) return;

            const int ringSize = 16;
            var ringTex = new Texture2D(ringSize, ringSize);
            var center = new Vector2(ringSize / 2f, ringSize / 2f);
            for (var y = 0; y < ringSize; y++)
            for (var x = 0; x < ringSize; x++)
            {
                var dist = Vector2.Distance(new Vector2(x, y), center);
                var alpha = dist > ringSize * 0.32f && dist < ringSize * 0.46f ? 1f : 0f;
                ringTex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

            ringTex.Apply();
            _ringSprite = Sprite.Create(ringTex, new Rect(0, 0, ringSize, ringSize), new Vector2(0.5f, 0.5f), ringSize);
        }
    }
}
