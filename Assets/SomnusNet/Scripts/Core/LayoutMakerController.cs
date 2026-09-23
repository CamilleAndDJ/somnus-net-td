using SomnusNet.Gameplay;
using SomnusNet.UI;
using SomnusNet.Visual;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SomnusNet.Core
{
    public class LayoutMakerController : MonoBehaviour
    {
        Transform _labelRoot;
        RectTransform _uiRoot;

        public static void EnsureExists()
        {
            if (!LevelLayoutSession.IsLayoutMaker)
                return;

            if (FindAnyObjectByType<LayoutMakerController>() != null)
                return;

            var grid = GridManager.Instance;
            if (grid == null)
                return;

            var go = new GameObject("LayoutMakerController");
            go.transform.SetParent(grid.transform, false);
            go.AddComponent<LayoutMakerController>();
        }

        void Start()
        {
            if (!LevelLayoutSession.IsLayoutMaker)
            {
                Destroy(gameObject);
                return;
            }

            BuildCellLabels();
            BuildPresetButtons();
        }

        void Update()
        {
            if (!LevelLayoutSession.IsLayoutMaker)
                return;

            if (!Input.GetMouseButtonDown(0))
                return;

            if (IsPointerOverBlockingUi())
                return;

            var placement = FindAnyObjectByType<PlacementController>();
            if (placement != null && placement.Selected.HasValue)
                return;

            var grid = GridManager.Instance;
            var camera = Camera.main;
            if (grid == null || camera == null)
                return;

            var world = camera.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            if (!grid.IsWorldOnGrid(world))
                return;

            grid.WorldToNearestCell(world, out var col, out var row);
            LayoutMakerPaintState.Cycle(col, row);
            RefreshPaintedGrid();
        }

        void BuildPresetButtons()
        {
            if (_uiRoot != null)
                return;

            var es = FindAnyObjectByType<EventSystem>();
            if (es == null)
            {
                var esGo = new GameObject("LayoutMakerEventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            var canvasGo = new GameObject("LayoutMakerUi");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _uiRoot = UiPanelFactory.CreateRect(canvasGo.transform, "LayoutMakerButtons",
                Vector2.zero, Vector2.one);

            var beginButton = UiPanelFactory.CreateButton(_uiRoot, "FillBeginingButton",
                new Vector2(0.84f, 0.56f), new Vector2(0.97f, 0.64f),
                LevelLayoutCatalog.GetDisplayName(LevelLayoutId.Begining), 18);
            beginButton.onClick.AddListener(() => ApplyPreset(LevelLayoutId.Begining));

            var doubleTroubleButton = UiPanelFactory.CreateButton(_uiRoot, "FillDoubleTroubleButton",
                new Vector2(0.84f, 0.44f), new Vector2(0.97f, 0.52f),
                LevelLayoutCatalog.GetDisplayName(LevelLayoutId.DoubleTrouble), 18);
            doubleTroubleButton.onClick.AddListener(() => ApplyPreset(LevelLayoutId.DoubleTrouble));
        }

        void ApplyPreset(LevelLayoutId layout)
        {
            LayoutMakerPaintState.ApplyPreset(layout);
            RefreshPaintedGrid();
        }

        void RefreshPaintedGrid()
        {
            var grid = GridManager.Instance;
            if (grid != null)
                grid.ApplyLayoutMakerPaint();

            var board = FindAnyObjectByType<GridBoardRenderer>();
            if (board != null)
                board.BuildGrid();
        }

        void BuildCellLabels()
        {
            if (_labelRoot != null)
                return;

            var grid = GridManager.Instance;
            if (grid == null)
                return;

            _labelRoot = new GameObject("LayoutMakerLabels").transform;
            _labelRoot.SetParent(transform, false);

            for (var col = 0; col < GridManager.ColumnCount; col++)
            for (var row = 0; row < GridManager.RowCount; row++)
            {
                var labelGo = new GameObject($"Label_{col}_{row}");
                labelGo.transform.SetParent(_labelRoot, false);
                labelGo.transform.position = grid.CellToWorld(col, row);

                var text = labelGo.AddComponent<TextMesh>();
                text.text = LevelLayoutCatalog.GetUserCellLabel(col, row);
                text.fontSize = 32;
                text.characterSize = 0.045f;
                text.anchor = TextAnchor.MiddleCenter;
                text.alignment = TextAlignment.Center;
                text.color = new Color(0.95f, 0.95f, 1f, 0.82f);
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                var meshRenderer = labelGo.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                    meshRenderer.sortingOrder = 6;
            }
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
