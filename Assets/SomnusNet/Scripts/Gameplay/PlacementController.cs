using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.UI;
using SomnusNet.Units;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SomnusNet.Gameplay
{
    public class PlacementController : MonoBehaviour
    {
        [SerializeField] GameCatalog catalog;
        [SerializeField] GameObject blobPrefab;
        [SerializeField] BlobUpgradePanel upgradePanel;

        BlobKind? _selected;

        public BlobKind? Selected => _selected;

        public void SelectBlob(BlobKind kind) => _selected = BlobKinds.Normalize(kind);

        public void ClearSelectionForStoryIntro() => _selected = null;

        public static event System.Action<DreamBlob> BlobPlaced;

        public void ClearSelection() => _selected = null;

        public bool CanPlaceSelectedAt(GridManager grid, int col, int row)
        {
            if (grid == null || !_selected.HasValue)
                return grid != null && grid.CanPlaceBlob(col, row);

            if (catalog == null || !catalog.TryGetBlob(_selected.Value, out var def))
                return grid.CanPlaceBlob(col, row);

            return grid.CanPlaceBlob(col, row, def.canPlaceOnWater);
        }

        void Start()
        {
            if (!StoryModeRules.Active && !LevelLayoutSession.IsLayoutMaker)
                SelectBlob(BlobKind.Gatekeeper);

            if (upgradePanel == null)
                upgradePanel = BlobUpgradePanel.Instance;
        }

        void Update()
        {
            if (!LevelLayoutSession.IsReady)
                return;

            var phase = GameManager.Instance.Phase;
            if (phase != GamePhase.Playing && phase != GamePhase.RoundIntermission) return;
            if (!Input.GetMouseButtonDown(0)) return;
            if (IsPointerOverBlockingUi()) return;

            var camera = Camera.main;
            if (camera == null) return;

            var world = camera.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;

            var grid = GridManager.Instance;
            if (upgradePanel == null)
                upgradePanel = BlobUpgradePanel.Instance;

            if (TryPlaceSelectedBlob(grid, world))
                return;

            if (StoryModeIntroController.Instance != null && !StoryModeIntroController.Instance.IsIntroComplete)
                return;

            if (GameManager.Instance.CanOpenUpgradeMenu)
                TryHandleUpgradeSelection(grid, world);
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

        bool TryPlaceSelectedBlob(GridManager grid, Vector3 world)
        {
            if (!_selected.HasValue)
                return false;

            if (IsStoryPlacementBlocked())
                return false;

            var round = GameManager.Instance != null ? GameManager.Instance.CurrentRound : 1;
            if (!LevelShopRules.IsBlobUnlocked(_selected.Value, round))
                return false;

            if (!grid.IsWorldOnGrid(world))
                return false;

            if (!catalog.TryGetBlob(_selected.Value, out var def))
                return false;

            if (!grid.WorldToBestPlaceableCell(world, out var col, out var row, def.canPlaceOnWater))
                return false;

            if (!PonderEconomy.Instance.TrySpend(def.ponderCost))
                return false;

            var go = Instantiate(blobPrefab);
            var blob = go.GetComponent<DreamBlob>();
            blob.Initialize(def);
            if (!grid.TryPlace(blob, col, row))
            {
                Destroy(go);
                PonderEconomy.Instance.Add(def.ponderCost);
                return false;
            }

            BlobPlaced?.Invoke(blob);
            StoryModeIntroController.Instance?.NotifyFirstBlobPlaced(blob);
            StoryModeHarmonyTutorial.NotifyBlobPlaced(blob);
            return true;
        }

        void TryHandleUpgradeSelection(GridManager grid, Vector3 world)
        {
            if (!grid.IsWorldOnGrid(world))
            {
                upgradePanel?.ClosePanel();
                return;
            }

            grid.WorldToNearestCell(world, out var col, out var row);
            var existing = grid.Get(col, row);
            if (existing == null || !existing.IsAlive)
                return;

            var cellCenter = grid.CellToWorld(col, row);
            var maxSelectDist = grid.cellSize * 0.55f;
            if ((world - cellCenter).sqrMagnitude > maxSelectDist * maxSelectDist)
                return;

            if (upgradePanel != null && upgradePanel.SelectedBlob == existing)
                upgradePanel.ClosePanel();
            else
                upgradePanel?.SelectBlob(existing);
        }
    }
}
