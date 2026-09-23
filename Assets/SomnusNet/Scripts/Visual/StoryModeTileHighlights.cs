using System.Collections.Generic;
using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.Visual
{
    public class StoryModeTileHighlights : MonoBehaviour
    {
        const float CircleScale = 0.22f;
        static readonly Color HighlightColor = new(1f, 0.95f, 0.55f, 0.92f);

        static Sprite _circleSprite;
        readonly List<SpriteRenderer> _markers = new();

        public static StoryModeTileHighlights Instance { get; private set; }

        public static void ShowPlaceableTiles()
        {
            EnsureExists();
            Instance?.Rebuild();
        }

        public static void Hide()
        {
            if (Instance == null)
                return;

            Instance.ClearMarkers();
            Instance.enabled = false;
        }

        static void EnsureExists()
        {
            if (Instance != null)
                return;

            var grid = GridManager.Instance;
            if (grid == null)
                return;

            var go = new GameObject("StoryModeTileHighlights");
            go.transform.SetParent(grid.transform, false);
            go.AddComponent<StoryModeTileHighlights>();
        }

        void Awake() => Instance = this;

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Rebuild()
        {
            ClearMarkers();
            enabled = true;

            var grid = GridManager.Instance;
            if (grid == null)
                return;

            EnsureCircleSprite();

            for (var col = 0; col < GridManager.ColumnCount; col++)
            for (var row = 0; row < GridManager.RowCount; row++)
            {
                if (!grid.CanPlaceBlob(col, row))
                    continue;

                var markerGo = new GameObject($"PlaceHint_{col}_{row}");
                markerGo.transform.SetParent(transform, false);
                markerGo.transform.position = grid.CellToWorld(col, row);

                var sr = markerGo.AddComponent<SpriteRenderer>();
                sr.sprite = _circleSprite;
                sr.color = HighlightColor;
                sr.sortingOrder = 4;
                markerGo.transform.localScale = Vector3.one * grid.cellSize * CircleScale;

                _markers.Add(sr);
            }
        }

        void ClearMarkers()
        {
            foreach (var marker in _markers)
            {
                if (marker != null)
                    Destroy(marker.gameObject);
            }

            _markers.Clear();
        }

        static void EnsureCircleSprite()
        {
            if (_circleSprite != null)
                return;

            const int size = 32;
            var tex = new Texture2D(size, size);
            var center = new Vector2(size / 2f, size / 2f);
            var radius = size * 0.42f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dist = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
            }

            tex.Apply();
            _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
