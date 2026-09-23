using SomnusNet.Core;
using SomnusNet.Gameplay;
using SomnusNet.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SomnusNet.Visual
{
    public class PlacementHoverVisualizer : MonoBehaviour
    {
        static readonly Color ValidOutlineColor = new(1f, 1f, 1f, 0.42f);
        static readonly Color InvalidOutlineColor = new(1f, 0.34f, 0.34f, 0.42f);

        PlacementController _placement;
        SpriteRenderer _outlineRenderer;
        static Sprite _outlineSprite;

        public static void EnsureExists()
        {
            if (FindFirstObjectByType<PlacementHoverVisualizer>() != null)
                return;

            var grid = GridManager.Instance;
            if (grid == null)
                return;

            var go = new GameObject("PlacementHoverVisualizer");
            go.transform.SetParent(grid.transform, false);
            go.AddComponent<PlacementHoverVisualizer>();
        }

        void Awake()
        {
            _placement = FindFirstObjectByType<PlacementController>();
            EnsureOutlineSprite();
            BuildOutlineRenderer();
            HideOutline();
        }

        void Update()
        {
            if (_outlineRenderer == null)
                return;

            if (!ShouldShowHover(out var grid, out var world))
            {
                HideOutline();
                return;
            }

            grid.WorldToNearestCell(world, out var col, out var row);
            _outlineRenderer.transform.position = grid.CellToWorld(col, row);
            _outlineRenderer.size = Vector2.one * grid.cellSize * 0.98f;
            _outlineRenderer.color = _placement.CanPlaceSelectedAt(grid, col, row)
                ? ValidOutlineColor
                : InvalidOutlineColor;
            _outlineRenderer.enabled = true;
        }

        bool ShouldShowHover(out GridManager grid, out Vector3 world)
        {
            grid = GridManager.Instance;
            world = default;

            if (grid == null || !grid.HasPath || !LevelLayoutSession.IsReady
                || _placement == null || !_placement.Selected.HasValue)
                return false;

            var manager = GameManager.Instance;
            if (manager == null)
                return false;

            var phase = manager.Phase;
            if (phase != GamePhase.Playing && phase != GamePhase.RoundIntermission)
                return false;

            if (IsStoryPlacementBlocked())
                return false;

            var round = manager.CurrentRound;
            if (!LevelShopRules.IsBlobUnlocked(_placement.Selected.Value, round))
                return false;

            if (IsPointerOverBlockingUi())
                return false;

            var camera = Camera.main;
            if (camera == null)
                return false;

            world = camera.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            return grid.IsWorldOnGrid(world);
        }

        void BuildOutlineRenderer()
        {
            var outlineGo = new GameObject("PlacementHoverOutline");
            outlineGo.transform.SetParent(transform, false);

            _outlineRenderer = outlineGo.AddComponent<SpriteRenderer>();
            _outlineRenderer.sprite = _outlineSprite;
            _outlineRenderer.drawMode = SpriteDrawMode.Sliced;
            _outlineRenderer.sortingOrder = 8;
        }

        void HideOutline()
        {
            if (_outlineRenderer != null)
                _outlineRenderer.enabled = false;
        }

        static void EnsureOutlineSprite()
        {
            if (_outlineSprite != null)
                return;

            const int size = 8;
            var tex = new Texture2D(size, size);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var edge = x == 0 || y == 0 || x == size - 1 || y == size - 1;
                tex.SetPixel(x, y, edge ? Color.white : Color.clear);
            }

            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            _outlineSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static bool IsStoryPlacementBlocked()
        {
            return StoryModeIntroController.Instance != null
                   && !StoryModeIntroController.Instance.CanPlaceBlobs;
        }

        static bool IsPointerOverBlockingUi()
        {
            if (EventSystem.current == null)
                return false;

            var pointerId = Input.touchCount > 0 ? Input.GetTouch(0).fingerId : -1;
            if (!EventSystem.current.IsPointerOverGameObject(pointerId))
                return false;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition,
                pointerId = pointerId
            };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var hit in results)
            {
                if (hit.gameObject.GetComponent<Selectable>() != null)
                    return true;
            }

            return false;
        }
    }
}
