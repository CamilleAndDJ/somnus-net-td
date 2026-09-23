using System.Collections.Generic;
using SomnusNet.Core;
using SomnusNet.UI;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Visual
{
    /// <summary>Soft glow on a linked band (all blob types) when a Harmony blob in that band is selected.</summary>
    public class HarmonyBandVisualizer : MonoBehaviour
    {
        static readonly Color BandFill = new(1f, 0.96f, 0.78f, 0.16f);
        static readonly Color BandRing = new(1f, 0.98f, 0.88f, 0.34f);

        static Sprite _cellSprite;
        static Sprite _ringSprite;

        readonly Dictionary<DreamBlob, BandMarker> _markers = new();
        readonly HashSet<DreamBlob> _cluster = new();
        readonly HashSet<DreamBlob> _linkedMembers = new();
        readonly List<DreamBlob> _stale = new();

        Transform _root;

        struct BandMarker
        {
            public SpriteRenderer fill;
            public SpriteRenderer ring;
        }

        public static void EnsureExists()
        {
            if (!HarmonyTypeRules.FeatureEnabled) return;
            if (Object.FindFirstObjectByType<HarmonyBandVisualizer>() != null) return;

            var grid = GridManager.Instance;
            if (grid != null)
                grid.gameObject.AddComponent<HarmonyBandVisualizer>();
        }

        void Awake()
        {
            if (!HarmonyTypeRules.FeatureEnabled)
            {
                enabled = false;
                return;
            }

            _root = new GameObject("HarmonyBandOverlay").transform;
            _root.SetParent(transform);
        }

        void LateUpdate()
        {
            if (!HarmonyTypeRules.FeatureEnabled)
            {
                ClearMarkers();
                return;
            }

            SyncLinkedBandMarkers();
            PulseMarkers();
        }

        void OnDestroy() => ClearMarkers();

        void SyncLinkedBandMarkers()
        {
            var grid = GridManager.Instance;
            if (grid == null) return;

            _linkedMembers.Clear();

            var panel = BlobUpgradePanel.Instance;
            var selected = panel != null && panel.IsOpen ? panel.SelectedBlob : null;
            if (selected != null && selected.IsAlive && selected.IsHarmonyType)
            {
                _cluster.Clear();
                grid.CollectHarmonyCluster(selected, _cluster);
                if (_cluster.Count >= 2)
                {
                    foreach (var blob in _cluster)
                        _linkedMembers.Add(blob);
                }
            }

            _stale.Clear();
            foreach (var blob in _markers.Keys)
            {
                if (blob == null || !blob.IsAlive || !_linkedMembers.Contains(blob))
                    _stale.Add(blob);
            }

            foreach (var blob in _stale)
                RemoveMarker(blob);

            foreach (var blob in _linkedMembers)
                EnsureMarker(blob);
        }

        void EnsureMarker(DreamBlob blob)
        {
            if (_markers.ContainsKey(blob)) return;

            EnsureSprites();
            var grid = GridManager.Instance;
            if (grid == null) return;

            var cellGo = new GameObject($"HarmonyBand_{blob.Column}_{blob.Lane}");
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
