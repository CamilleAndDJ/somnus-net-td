using System.Collections.Generic;
using SomnusNet.Data;
using SomnusNet.Core;
using SomnusNet.UI;
using SomnusNet.Units;
using SomnusNet.Visual;
using UnityEngine;

namespace SomnusNet.Core
{
    public class GridManager : MonoBehaviour
    {
        public const int ColumnCount = 17;
        public const int RowCount = 11;
        public const int LaneCount = RowCount;
        public const int PathRow = RowCount / 2;
        public const int SpawnColumn = ColumnCount - 1;
        public const int BlobRangeSize = 5;

        public const int SwerveDown = 3;
        public const int SwerveForward1 = 3;
        public const int SwerveUp = 6;
        public const int SwerveForward2 = 3;
        public const int SwerveDownReconnect = 3;

        public const float BlobSpriteWorldSize = 64f / 40f;
        public const float GlitchSpriteWorldSize = 80f / 40f;

        public const float CameraOrthoSize = 5.25f;
        public const float CameraYOffset = 0.65f;
        public const float BottomUiMargin = 1.9f;

        public int PathCenterColumn => (ColumnCount - 1) / 2;
        public int SwerveRejoinColumn => PathCenterColumn - (SwerveForward1 + SwerveForward2) / 2;
        public int SwerveStartColumn => PathCenterColumn + (SwerveForward1 + SwerveForward2) / 2;

        public float cellSize = 0.78f;
        public Vector2 origin;

        DreamBlob[,] _occupants;
        bool[,] _pathCells;
        bool[,] _waterCells;
        PathLane[,] _pathLanes;
        Vector3[] _glitchWaypoints;
        readonly List<Vector3[]> _glitchWaypointPaths = new();

        public static GridManager Instance { get; private set; }

        public float BlobVisualScale => cellSize * 0.82f / BlobSpriteWorldSize;
        public float GlitchVisualScale => cellSize * 0.82f / GlitchSpriteWorldSize;

        const float CoreLeftOfColumnZero = 0.95f;

        public float CoreLossX => CoreWorldPosition.x + cellSize * 0.35f;

        /// <summary>Dream Core — fixed at row 6 (PathRow), just left of column 0.</summary>
        public Vector3 CoreWorldPosition =>
            new(CellToWorld(0, PathRow).x - cellSize * CoreLeftOfColumnZero, origin.y + PathRow * cellSize, 0f);

        public bool HasPath => LevelLayoutSession.IsLayoutMaker
                               || _glitchWaypointPaths.Count > 0
                               || _glitchWaypoints is { Length: > 0 };

        public int GlitchPathCount => _glitchWaypointPaths.Count > 0
            ? _glitchWaypointPaths.Count
            : _glitchWaypoints is { Length: > 0 } ? 1 : 0;

        public IReadOnlyList<Vector3> GlitchWaypoints => _glitchWaypoints;

        public IReadOnlyList<Vector3> GetGlitchWaypoints(int pathIndex)
        {
            if (_glitchWaypointPaths.Count > 0)
            {
                if (pathIndex < 0 || pathIndex >= _glitchWaypointPaths.Count)
                    return System.Array.Empty<Vector3>();
                return _glitchWaypointPaths[pathIndex];
            }

            return pathIndex == 0 ? _glitchWaypoints : System.Array.Empty<Vector3>();
        }

        public PathLane GetPathLane(int col, int row) =>
            IsInside(col, row) && _pathLanes != null ? _pathLanes[col, row] : PathLane.None;

        void Awake()
        {
            Instance = this;
            ApplyBoardLayout();
            _occupants = new DreamBlob[ColumnCount, RowCount];
            if (LevelLayoutSession.IsReady)
                BuildPath();
        }

        public void InitializePath()
        {
            BuildPath();
            AlignDreamCore();
        }

        void Start()
        {
            HarmonyBandVisualizer.EnsureExists();
            AlignDreamCore();
        }

        void AlignDreamCore()
        {
            var core = GameObject.Find("DreamCore");
            if (core != null)
                core.transform.position = CoreWorldPosition;
        }

        /// <summary>Call from editor setup before reading layout-dependent positions.</summary>
        public void ApplyBoardLayout()
        {
            origin.x = -(ColumnCount - 1) * cellSize * 0.5f;
            origin.y = CameraYOffset - CameraOrthoSize + BottomUiMargin;
        }

        void BuildPath()
        {
            _pathCells = new bool[ColumnCount, RowCount];
            _waterCells = new bool[ColumnCount, RowCount];
            _pathLanes = new PathLane[ColumnCount, RowCount];
            _glitchWaypointPaths.Clear();

            var layout = LevelLayoutSession.Selected;
            if (layout == LevelLayoutId.LayoutMaker)
            {
                LayoutMakerPaintState.Reset();
                ApplyLayoutMakerPaint();
                BuildWater(layout);
                return;
            }

            var primaryCells = LevelLayoutCatalog.GetPrimaryPathCells(layout);
            if (primaryCells != null)
            {
                if (layout == LevelLayoutId.OuterRing)
                    MarkPathCells(LevelLayoutCatalog.GetOuterRingAllPathCells(), PathLane.Primary);

                _glitchWaypointPaths.Add(BuildWaypointPath(primaryCells, PathLane.Primary));

                var secondaryCells = LevelLayoutCatalog.GetSecondaryPathCells(layout);
                if (secondaryCells != null)
                    _glitchWaypointPaths.Add(BuildWaypointPath(secondaryCells, PathLane.Secondary));
            }
            else
            {
                var waypoints = new List<Vector3>();
                BuildBeginingPath(waypoints);
                _glitchWaypointPaths.Add(waypoints.ToArray());
            }

            _glitchWaypoints = _glitchWaypointPaths.Count > 0
                ? _glitchWaypointPaths[0]
                : System.Array.Empty<Vector3>();

            BuildWater(layout);
        }

        void BuildWater(LevelLayoutId layout)
        {
            var cells = LevelLayoutCatalog.GetWaterCells(layout);
            if (cells == null)
                return;

            foreach (var cell in cells)
            {
                if (!IsInside(cell.Col, cell.Row) || IsPathCell(cell.Col, cell.Row))
                    continue;

                _waterCells[cell.Col, cell.Row] = true;
            }
        }

        Vector3[] BuildWaypointPath(IReadOnlyList<GridCell> cells, PathLane lane)
        {
            var waypoints = new List<Vector3>();
            foreach (var cell in cells)
                AddPathCell(waypoints, cell.Col, cell.Row, lane);
            return waypoints.ToArray();
        }

        void BuildBeginingPath(List<Vector3> waypoints)
        {
            var swerveStart = SwerveStartColumn;
            var rejoinCol = SwerveRejoinColumn;

            TraceHorizontal(waypoints, SpawnColumn, swerveStart, PathRow);
            TraceVertical(waypoints, swerveStart, PathRow, PathRow - SwerveDown);
            TraceHorizontal(waypoints, swerveStart, swerveStart - SwerveForward1, PathRow - SwerveDown);
            TraceVertical(waypoints, swerveStart - SwerveForward1, PathRow - SwerveDown, PathRow + SwerveUp - SwerveDown);
            TraceHorizontal(waypoints, swerveStart - SwerveForward1, rejoinCol, PathRow + SwerveUp - SwerveDown);
            TraceVertical(waypoints, rejoinCol, PathRow + SwerveUp - SwerveDown, PathRow);
            TraceHorizontal(waypoints, rejoinCol, 0, PathRow);
        }

        void TraceHorizontal(List<Vector3> waypoints, int fromCol, int toCol, int row)
        {
            var step = fromCol >= toCol ? -1 : 1;
            for (var c = fromCol; c != toCol; c += step)
                AddPathCell(waypoints, c, row);
            AddPathCell(waypoints, toCol, row);
        }

        void TraceVertical(List<Vector3> waypoints, int col, int fromRow, int toRow)
        {
            var step = fromRow >= toRow ? -1 : 1;
            for (var r = fromRow; r != toRow; r += step)
                AddPathCell(waypoints, col, r);
            AddPathCell(waypoints, col, toRow);
        }

        public void ApplyLayoutMakerPaint()
        {
            _pathCells = new bool[ColumnCount, RowCount];
            _pathLanes = new PathLane[ColumnCount, RowCount];
            _glitchWaypointPaths.Clear();
            _glitchWaypoints = System.Array.Empty<Vector3>();

            for (var col = 0; col < ColumnCount; col++)
            for (var row = 0; row < RowCount; row++)
            {
                var lane = LayoutMakerPaintState.GetPathLane(col, row);
                if (lane == PathLane.None)
                    continue;

                _pathCells[col, row] = true;
                _pathLanes[col, row] = lane;
            }
        }

        void MarkPathCells(IReadOnlyList<GridCell> cells, PathLane lane)
        {
            foreach (var cell in cells)
            {
                if (!IsInside(cell.Col, cell.Row))
                    continue;

                _pathCells[cell.Col, cell.Row] = true;
                _pathLanes[cell.Col, cell.Row] |= lane;
            }
        }

        void AddPathCell(List<Vector3> waypoints, int col, int row, PathLane lane = PathLane.Primary)
        {
            if (!IsInside(col, row))
                return;

            _pathCells[col, row] = true;
            _pathLanes[col, row] |= lane;

            var world = CellToWorld(col, row);
            if (waypoints.Count == 0 || waypoints[^1] != world)
                waypoints.Add(world);
        }

        public Vector3 CellToWorld(int col, int row)
        {
            return new Vector3(origin.x + col * cellSize, origin.y + row * cellSize, 0f);
        }

        public void WorldToNearestCell(Vector3 world, out int col, out int row)
        {
            var local = world - (Vector3)origin;
            col = Mathf.Clamp(Mathf.RoundToInt(local.x / cellSize), 0, ColumnCount - 1);
            row = Mathf.Clamp(Mathf.RoundToInt(local.y / cellSize), 0, RowCount - 1);
        }

        public bool WorldToBestPlaceableCell(Vector3 world, out int col, out int row, bool canPlaceOnWater = false)
        {
            WorldToNearestCell(world, out col, out row);
            if (CanPlaceBlob(col, row, canPlaceOnWater))
                return true;

            var bestDist = float.MaxValue;
            var bestCol = col;
            var bestRow = row;
            var found = false;

            for (var dc = -1; dc <= 1; dc++)
            for (var dr = -1; dr <= 1; dr++)
            {
                var c = col + dc;
                var r = row + dr;
                if (!CanPlaceBlob(c, r, canPlaceOnWater))
                    continue;

                var dist = (world - CellToWorld(c, r)).sqrMagnitude;
                if (dist >= bestDist)
                    continue;

                bestDist = dist;
                bestCol = c;
                bestRow = r;
                found = true;
            }

            if (!found)
                return false;

            var maxSnap = cellSize * 0.65f;
            if (bestDist > maxSnap * maxSnap)
                return false;

            col = bestCol;
            row = bestRow;
            return true;
        }

        public bool IsWorldOnGrid(Vector3 world)
        {
            var half = cellSize * 0.5f;
            var minX = origin.x - half;
            var maxX = origin.x + (ColumnCount - 1) * cellSize + half;
            var minY = origin.y - half;
            var maxY = origin.y + (RowCount - 1) * cellSize + half;
            return world.x >= minX && world.x <= maxX && world.y >= minY && world.y <= maxY;
        }

        public bool IsInside(int col, int row) =>
            col >= 0 && col < ColumnCount && row >= 0 && row < RowCount;

        public bool IsPathCell(int col, int row) =>
            IsInside(col, row) && _pathCells != null && _pathCells[col, row];

        public bool IsWaterCell(int col, int row) =>
            IsInside(col, row) && _waterCells != null && _waterCells[col, row];

        public bool CanPlaceBlob(int col, int row, bool canPlaceOnWater = false)
        {
            if (!IsInside(col, row) || IsPathCell(col, row))
                return false;

            if (IsWaterCell(col, row) && !canPlaceOnWater)
                return false;

            var occupant = _occupants[col, row];
            if (occupant == null)
                return true;

            if (!occupant.IsAlive)
            {
                _occupants[col, row] = null;
                return true;
            }

            return false;
        }

        public bool IsEmpty(int col, int row) => CanPlaceBlob(col, row);

        public DreamBlob Get(int col, int row) =>
            IsInside(col, row) ? _occupants[col, row] : null;

        public void GetBlobRangeBounds(int blobCol, int blobRow, out int minCol, out int maxCol,
            out int minRow, out int maxRow, int size = BlobRangeSize)
        {
            var half = (size - 1) / 2;
            minCol = blobCol - half;
            maxCol = blobCol + half;
            minRow = blobRow - half;
            maxRow = blobRow + half;
        }

        public bool IsInBlobRange(int blobCol, int blobRow, int targetCol, int targetRow, int size = BlobRangeSize)
        {
            GetBlobRangeBounds(blobCol, blobRow, out var minCol, out var maxCol, out var minRow, out var maxRow, size);
            return targetCol >= minCol && targetCol <= maxCol
                && targetRow >= minRow && targetRow <= maxRow;
        }

        public bool TryPlace(DreamBlob blob, int col, int row)
        {
            var canPlaceOnWater = blob != null && blob.CanPlaceOnWater;
            if (!CanPlaceBlob(col, row, canPlaceOnWater)) return false;
            _occupants[col, row] = blob;
            blob.BindGrid(col, row);
            blob.transform.position = CellToWorld(col, row);
            TypeHudLayout.RecordTypesFromBlob(blob);
            BondVisualFeedback.NotifyBlobPlaced(blob);
            return true;
        }

        public bool HasAnyBlobOnField()
        {
            for (var col = 0; col < ColumnCount; col++)
            for (var row = 0; row < RowCount; row++)
            {
                if (_occupants[col, row] != null)
                    return true;
            }

            return false;
        }

        public void ClearCell(DreamBlob blob)
        {
            if (blob == null) return;
            if (IsInside(blob.Column, blob.Lane) && _occupants[blob.Column, blob.Lane] == blob)
            {
                _occupants[blob.Column, blob.Lane] = null;
                BondVisualFeedback.SyncCountsFromField();
            }
        }

        public int CountUniqueHarmonyTypesOnField() =>
            CountUniqueKindsWithType(HarmonyTypeRules.HarmonyLabel);

        public int CountUniqueKindsWithType(string typeLabel)
        {
            var kinds = new HashSet<BlobKind>();
            ForEachBlob(blob =>
            {
                if (blob.IsAlive && blob.HasType(typeLabel))
                    kinds.Add(blob.Kind);
            });
            return kinds.Count;
        }

        public int CountUniqueDreamTeamMemberKindsOnField() =>
            CountRosterMembersOnField(DreamTeamTypeRules.MemberKinds);

        public int CountRosterMembersOnField(BlobKind[] roster)
        {
            if (roster == null)
                return 0;

            var count = 0;
            foreach (var kind in roster)
            {
                if (HasAliveBlobKind(kind))
                    count++;
            }

            return count;
        }

        public bool AreAllRosterMembersOnField(BlobKind[] roster) =>
            roster != null && roster.Length > 0 && CountRosterMembersOnField(roster) >= roster.Length;

        public int CountUniqueBlobKindsOnField()
        {
            var kinds = new HashSet<BlobKind>();
            ForEachBlob(blob =>
            {
                if (blob.IsAlive)
                    kinds.Add(blob.Kind);
            });
            return kinds.Count;
        }

        public bool HasAliveBlobKind(BlobKind kind)
        {
            var found = false;
            ForEachBlob(blob =>
            {
                if (blob.IsAlive && blob.Kind == kind)
                    found = true;
            });
            return found;
        }

        public bool AreBlobsInSharedBand(DreamBlob a, DreamBlob b, int bandSize)
        {
            if (a == null || b == null || !a.IsAlive || !b.IsAlive)
                return false;

            GetBlobRangeBounds(a.Column, a.Lane, out var aMinCol, out var aMaxCol, out var aMinRow, out var aMaxRow,
                bandSize);
            return b.Column >= aMinCol && b.Column <= aMaxCol && b.Lane >= aMinRow && b.Lane <= aMaxRow;
        }

        public void ForEachGlitchInBlobBand(int blobCol, int blobRow, int bandSize, System.Action<Glitch> action)
        {
            if (action == null) return;

            GetBlobRangeBounds(blobCol, blobRow, out var minCol, out var maxCol, out var minRow, out var maxRow,
                bandSize);

            foreach (var glitch in GlitchRegistry.All)
            {
                if (glitch == null || !glitch.IsAlive) continue;
                WorldToNearestCell(glitch.transform.position, out var gCol, out var gRow);
                if (gCol < minCol || gCol > maxCol || gRow < minRow || gRow > maxRow) continue;
                action(glitch);
            }
        }

        public void ForEachBlob(System.Action<DreamBlob> action)
        {
            for (var col = 0; col < ColumnCount; col++)
            for (var row = 0; row < RowCount; row++)
            {
                var blob = _occupants[col, row];
                if (blob != null)
                    action(blob);
            }
        }

        public void ForEachAdjacentBlob(int col, int row, System.Action<DreamBlob> action)
        {
            for (var dc = -1; dc <= 1; dc++)
            for (var dr = -1; dr <= 1; dr++)
            {
                if (dc == 0 && dr == 0)
                    continue;

                var c = col + dc;
                var r = row + dr;
                if (!IsInside(c, r))
                    continue;

                var blob = _occupants[c, r];
                if (blob != null)
                    action(blob);
            }
        }

        /// <summary>
        /// Blobs linked when Harmony blobs overlap in 3×3 (transitive through Harmony only).
        /// All blob types within those bands count toward size; only Harmony blobs receive the buff.
        /// </summary>
        public int GetHarmonyClusterSize(DreamBlob start)
        {
            _harmonyClusterScratch.Clear();
            CollectHarmonyCluster(start, _harmonyClusterScratch);
            return _harmonyClusterScratch.Count;
        }

        public void CollectHarmonyCluster(DreamBlob start, HashSet<DreamBlob> members)
        {
            members.Clear();
            if (start == null || !start.IsAlive || !start.IsHarmonyType)
                return;

            var queue = new Queue<DreamBlob>();
            members.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                GetBlobRangeBounds(current.Column, current.Lane, out var minCol, out var maxCol,
                    out var minRow, out var maxRow, HarmonyTypeRules.BandRangeSize);

                for (var col = minCol; col <= maxCol; col++)
                for (var row = minRow; row <= maxRow; row++)
                {
                    if (!IsInside(col, row)) continue;
                    var blob = _occupants[col, row];
                    if (blob == null || !blob.IsAlive || members.Contains(blob))
                        continue;

                    members.Add(blob);
                    if (blob.IsHarmonyType)
                        queue.Enqueue(blob);
                }
            }
        }

        public void ForEachHarmonyBlob(System.Action<DreamBlob> action)
        {
            for (var col = 0; col < ColumnCount; col++)
            for (var row = 0; row < RowCount; row++)
            {
                var blob = _occupants[col, row];
                if (blob != null && blob.IsAlive && blob.IsHarmonyType)
                    action(blob);
            }
        }

        readonly HashSet<DreamBlob> _harmonyClusterScratch = new();
    }
}
