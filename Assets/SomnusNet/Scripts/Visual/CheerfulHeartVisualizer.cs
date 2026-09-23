using System.Collections.Generic;
using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.UI;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Visual
{
    /// <summary>
    /// Red heart on Cheerful with no Best Friend (lonely or neighbors claimed). Green heart on Best Friend while Cheerful upgrade panel is open.
    /// </summary>
    public class CheerfulHeartVisualizer : MonoBehaviour
    {
        static readonly Color LonelyHeart = new(1f, 0.2f, 0.32f, 1f);
        static readonly Color LonelyHeartOutline = new(0.55f, 0.02f, 0.12f, 0.98f);
        static readonly Color BestFriendHeart = new(0.38f, 1f, 0.48f, 1f);
        static readonly Color BestFriendHeartOutline = new(0.05f, 0.42f, 0.14f, 0.98f);

        readonly Dictionary<DreamBlob, HeartMarker> _markers = new();
        readonly List<DreamBlob> _stale = new();

        Transform _root;
        float _scale;

        struct HeartMarker
        {
            public SpriteRenderer outline;
            public SpriteRenderer heart;
            public bool lonely;
        }

        public static void EnsureExists()
        {
            if (Object.FindFirstObjectByType<CheerfulHeartVisualizer>() != null)
                return;

            var grid = GridManager.Instance;
            if (grid != null)
                grid.gameObject.AddComponent<CheerfulHeartVisualizer>();
        }

        void Awake()
        {
            _root = new GameObject("CheerfulHeartOverlay").transform;
            _root.SetParent(transform);
            _scale = GridManager.Instance != null ? GridManager.Instance.BlobVisualScale * 0.42f : 0.42f;
        }

        void LateUpdate()
        {
            SyncMarkers();
            PulseMarkers();
        }

        void OnDestroy() => ClearMarkers();

        void SyncMarkers()
        {
            var desired = new Dictionary<DreamBlob, bool>();

            var grid = GridManager.Instance;
            if (grid != null)
            {
                grid.ForEachBlob(blob =>
                {
                    if (blob == null || !blob.IsAlive || blob.Kind != BlobKind.Cheerful)
                        return;

                    if (CheerfulBestFriendSynergy.SelectBestFriend(blob) == null)
                        desired[blob] = true;
                });
            }

            var panel = BlobUpgradePanel.Instance;
            var selected = panel != null && panel.IsOpen ? panel.SelectedBlob : null;
            if (selected != null && selected.IsAlive && selected.Kind == BlobKind.Cheerful)
            {
                var friend = CheerfulBestFriendSynergy.SelectBestFriend(selected);
                if (friend != null)
                    desired[friend] = false;
            }

            _stale.Clear();
            foreach (var pair in _markers)
            {
                if (pair.Key == null || !pair.Key.IsAlive || !desired.ContainsKey(pair.Key))
                    _stale.Add(pair.Key);
            }

            foreach (var blob in _stale)
                RemoveMarker(blob);

            foreach (var pair in desired)
                EnsureMarker(pair.Key, pair.Value);
        }

        void EnsureMarker(DreamBlob blob, bool lonely)
        {
            if (_markers.TryGetValue(blob, out var existing) && existing.lonely == lonely)
                return;

            if (existing.heart != null)
                Destroy(existing.heart.transform.parent != null ? existing.heart.transform.parent.gameObject : existing.heart.gameObject);

            var rootGo = new GameObject(lonely
                ? $"CheerfulLonelyHeart_{blob.Column}_{blob.Lane}"
                : $"CheerfulBestFriendHeart_{blob.Column}_{blob.Lane}");
            rootGo.transform.SetParent(_root);
            rootGo.transform.position = blob.transform.position + Vector3.up * (_scale * 0.95f);

            var outlineGo = new GameObject("Outline");
            outlineGo.transform.SetParent(rootGo.transform, false);
            var outline = outlineGo.AddComponent<SpriteRenderer>();
            outline.sprite = CheerfulHeartSprites.HeartSprite;
            outline.color = lonely ? LonelyHeartOutline : BestFriendHeartOutline;
            outline.sortingOrder = 17;

            var heartGo = new GameObject("Heart");
            heartGo.transform.SetParent(rootGo.transform, false);
            var heart = heartGo.AddComponent<SpriteRenderer>();
            heart.sprite = CheerfulHeartSprites.HeartSprite;
            heart.color = lonely ? LonelyHeart : BestFriendHeart;
            heart.sortingOrder = 18;

            _markers[blob] = new HeartMarker { outline = outline, heart = heart, lonely = lonely };
        }

        void RemoveMarker(DreamBlob blob)
        {
            if (!_markers.TryGetValue(blob, out var marker))
                return;

            if (marker.heart != null)
                Destroy(marker.heart.transform.parent != null ? marker.heart.transform.parent.gameObject : marker.heart.gameObject);
            _markers.Remove(blob);
        }

        void ClearMarkers()
        {
            foreach (var marker in _markers.Values)
            {
                if (marker.heart != null)
                    Destroy(marker.heart.transform.parent != null ? marker.heart.transform.parent.gameObject : marker.heart.gameObject);
            }

            _markers.Clear();
        }

        void PulseMarkers()
        {
            var pulse = 0.92f + 0.08f * Mathf.Sin(Time.time * 3.2f);
            foreach (var pair in _markers)
            {
                if (pair.Key == null || !pair.Key.IsAlive || pair.Value.heart == null)
                    continue;

                var root = pair.Value.heart.transform.parent;
                var pos = pair.Key.transform.position + Vector3.up * (_scale * 0.95f);
                if (root != null)
                    root.position = pos;

                var heartScale = Vector3.one * _scale * pulse;
                var outlineScale = heartScale * 1.14f;
                pair.Value.heart.transform.localScale = heartScale;
                if (pair.Value.outline != null)
                    pair.Value.outline.transform.localScale = outlineScale;

                var heartColor = pair.Value.lonely ? LonelyHeart : BestFriendHeart;
                var outlineColor = pair.Value.lonely ? LonelyHeartOutline : BestFriendHeartOutline;
                pair.Value.heart.color = heartColor;
                if (pair.Value.outline != null)
                    pair.Value.outline.color = outlineColor;
            }
        }
    }
}
