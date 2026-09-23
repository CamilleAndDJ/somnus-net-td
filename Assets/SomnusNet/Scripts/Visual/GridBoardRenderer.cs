using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.Visual
{
    /// <summary>Draws the playable grid in the Game view (not just editor gizmos).</summary>
    public class GridBoardRenderer : MonoBehaviour
    {
        [SerializeField] GridManager grid;
        [SerializeField] Color cellFill = new(0.32f, 0.26f, 0.52f, 0.14f);
        [SerializeField] Color cellBorder = new(0.58f, 0.5f, 0.88f, 0.45f);
        [SerializeField] Color pathFill = new(0.55f, 0.35f, 0.65f, 0.35f);
        [SerializeField] Color pathBorder = new(0.75f, 0.55f, 0.95f, 0.65f);
        [SerializeField] Color secondaryPathFill = new(0.34f, 0.58f, 0.62f, 0.38f);
        [SerializeField] Color secondaryPathBorder = new(0.48f, 0.78f, 0.82f, 0.68f);
        [SerializeField] Color waterFill = new(0.20f, 0.44f, 0.78f, 0.40f);
        [SerializeField] Color waterBorder = new(0.34f, 0.62f, 0.92f, 0.68f);
        [SerializeField] Color spawnColumnFill = new(0.45f, 0.28f, 0.55f, 0.22f);

        static Sprite _cellSprite;

        Transform _cellsRoot;

        void Start()
        {
            if (grid == null)
                grid = GridManager.Instance;

            if (LevelLayoutSession.IsReady)
                BuildGrid();
        }

        public void BuildGrid()
        {
            if (grid == null)
                grid = GridManager.Instance;
            if (grid == null || !grid.HasPath)
                return;

            if (_cellsRoot != null)
                Destroy(_cellsRoot.gameObject);

            EnsureCellSprite();
            _cellsRoot = new GameObject("GridCells").transform;
            _cellsRoot.SetParent(transform);

            for (var c = 0; c < GridManager.ColumnCount; c++)
            for (var r = 0; r < GridManager.RowCount; r++)
            {
                var pathLane = LevelLayoutSession.IsLayoutMaker
                    ? LayoutMakerPaintState.GetPathLane(c, r)
                    : grid.GetPathLane(c, r);
                var isPath = pathLane != PathLane.None;
                var isWater = !isPath && grid.IsWaterCell(c, r);
                var isSpawn = LevelLayoutCatalog.IsSpawnEntrance(LevelLayoutSession.Selected, c, r, isPath);
                var spawnLane = isSpawn
                    ? LevelLayoutCatalog.GetSpawnEntranceLane(LevelLayoutSession.Selected, c, r)
                    : PathLane.None;
                var fillColor = isPath
                    ? (isSpawn ? GetSpawnFillColor(spawnLane, pathLane) : GetPathFillColor(pathLane))
                    : isWater
                        ? waterFill
                        : cellFill;
                var borderColor = isPath
                    ? (isSpawn ? GetSpawnBorderColor(spawnLane, pathLane) : GetPathBorderColor(pathLane))
                    : isWater
                        ? waterBorder
                        : cellBorder;

                var go = new GameObject($"Cell_{c}_{r}");
                go.transform.SetParent(_cellsRoot);
                go.transform.position = grid.CellToWorld(c, r);

                var fill = go.AddComponent<SpriteRenderer>();
                fill.sprite = _cellSprite;
                fill.drawMode = SpriteDrawMode.Sliced;
                fill.size = Vector2.one * grid.cellSize * 0.94f;
                fill.color = fillColor;
                fill.sortingOrder = -12;

                var borderGo = new GameObject("Border");
                borderGo.transform.SetParent(go.transform);
                borderGo.transform.localPosition = Vector3.zero;
                var border = borderGo.AddComponent<SpriteRenderer>();
                border.sprite = _cellSprite;
                border.drawMode = SpriteDrawMode.Sliced;
                border.size = Vector2.one * grid.cellSize * 0.98f;
                border.color = borderColor;
                border.sortingOrder = -11;
            }
        }

        Color GetSpawnFillColor(PathLane spawnLane, PathLane pathLane)
        {
            if (spawnLane == PathLane.Secondary)
                return secondaryPathFill;

            if (spawnLane == PathLane.Primary)
                return pathFill;

            if (pathLane != PathLane.None)
                return GetPathFillColor(pathLane);

            return spawnColumnFill;
        }

        Color GetSpawnBorderColor(PathLane spawnLane, PathLane pathLane)
        {
            if (spawnLane == PathLane.Secondary)
                return secondaryPathBorder;

            if (spawnLane == PathLane.Primary)
                return pathBorder;

            if (pathLane != PathLane.None)
                return GetPathBorderColor(pathLane);

            return pathBorder;
        }

        Color GetPathFillColor(PathLane lane)
        {
            var hasPrimary = lane.HasFlag(PathLane.Primary);
            var hasSecondary = lane.HasFlag(PathLane.Secondary);
            if (hasPrimary && hasSecondary)
                return MixColors(pathFill, secondaryPathFill);
            if (hasSecondary)
                return secondaryPathFill;
            return pathFill;
        }

        Color GetPathBorderColor(PathLane lane)
        {
            var hasPrimary = lane.HasFlag(PathLane.Primary);
            var hasSecondary = lane.HasFlag(PathLane.Secondary);
            if (hasPrimary && hasSecondary)
                return MixColors(pathBorder, secondaryPathBorder);
            if (hasSecondary)
                return secondaryPathBorder;
            return pathBorder;
        }

        static Color MixColors(Color a, Color b) =>
            new((a.r + b.r) * 0.5f, (a.g + b.g) * 0.5f, (a.b + b.b) * 0.5f, (a.a + b.a) * 0.5f);

        static void EnsureCellSprite()
        {
            if (_cellSprite != null) return;
            const int s = 8;
            var tex = new Texture2D(s, s);
            for (var y = 0; y < s; y++)
            for (var x = 0; x < s; x++)
            {
                var edge = x == 0 || y == 0 || x == s - 1 || y == s - 1;
                tex.SetPixel(x, y, edge ? Color.white : new Color(1f, 1f, 1f, 0.85f));
            }
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            _cellSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
        }
    }
}
