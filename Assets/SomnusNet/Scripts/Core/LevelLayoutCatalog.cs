using System.Collections.Generic;

namespace SomnusNet.Core
{
    [System.Flags]
    public enum PathLane
    {
        None = 0,
        Primary = 1,
        Secondary = 2
    }

    public readonly struct GridCell
    {
        public readonly int Col;
        public readonly int Row;

        public GridCell(int col, int row)
        {
            Col = col;
            Row = row;
        }

        public static GridCell FromUserCell(int userRow, int userCol) =>
            new(userCol - 1, GridManager.RowCount - userRow);
    }

    public static class LevelLayoutCatalog
    {
        public static GridCell CoreCell => GridCell.FromUserCell(6, 1);

        public static string GetDisplayName(LevelLayoutId layout) => layout switch
        {
            LevelLayoutId.DoubleTrouble => "Double Trouble",
            LevelLayoutId.OuterRing => "Outer Ring",
            LevelLayoutId.NotebookTest => "Notebook Test",
            LevelLayoutId.LayoutMaker => "Layout Maker",
            _ => "Begining"
        };

        public static string GetUserCellLabel(int col, int row) =>
            $"r{GridManager.RowCount - row}c{col + 1}";

        public static GridCell GetCoreCell(LevelLayoutId layout) => CoreCell;

        public static bool HasDualPaths(LevelLayoutId layout) =>
            layout == LevelLayoutId.DoubleTrouble;

        public static bool SkipsGlitchSpawns(LevelLayoutId layout) =>
            layout == LevelLayoutId.LayoutMaker;

        public static IReadOnlyList<GridCell> GetPrimaryPathCells(LevelLayoutId layout) => layout switch
        {
            LevelLayoutId.DoubleTrouble => DoubleTroublePrimaryPath,
            LevelLayoutId.OuterRing => OuterRingRoutePath,
            _ => null
        };

        public static IReadOnlyList<GridCell> GetOuterRingAllPathCells() => OuterRingAllPathCells;

        public static IReadOnlyList<GridCell> GetBeginningPathCells()
        {
            var cells = new List<GridCell>();
            var seen = new HashSet<(int col, int row)>();

            void Add(int col, int row)
            {
                if (col < 0 || col >= GridManager.ColumnCount || row < 0 || row >= GridManager.RowCount)
                    return;

                if (!seen.Add((col, row)))
                    return;

                cells.Add(new GridCell(col, row));
            }

            void TraceHorizontal(int fromCol, int toCol, int row)
            {
                var step = fromCol >= toCol ? -1 : 1;
                for (var c = fromCol; c != toCol; c += step)
                    Add(c, row);
                Add(toCol, row);
            }

            void TraceVertical(int col, int fromRow, int toRow)
            {
                var step = fromRow >= toRow ? -1 : 1;
                for (var r = fromRow; r != toRow; r += step)
                    Add(col, r);
                Add(col, toRow);
            }

            var pathRow = GridManager.PathRow;
            var swerveStart = (GridManager.ColumnCount - 1) / 2
                              + (GridManager.SwerveForward1 + GridManager.SwerveForward2) / 2;
            var rejoinCol = (GridManager.ColumnCount - 1) / 2
                            - (GridManager.SwerveForward1 + GridManager.SwerveForward2) / 2;

            TraceHorizontal(GridManager.SpawnColumn, swerveStart, pathRow);
            TraceVertical(swerveStart, pathRow, pathRow - GridManager.SwerveDown);
            TraceHorizontal(swerveStart, swerveStart - GridManager.SwerveForward1, pathRow - GridManager.SwerveDown);
            TraceVertical(swerveStart - GridManager.SwerveForward1, pathRow - GridManager.SwerveDown,
                pathRow + GridManager.SwerveUp - GridManager.SwerveDown);
            TraceHorizontal(swerveStart - GridManager.SwerveForward1, rejoinCol,
                pathRow + GridManager.SwerveUp - GridManager.SwerveDown);
            TraceVertical(rejoinCol, pathRow + GridManager.SwerveUp - GridManager.SwerveDown, pathRow);
            TraceHorizontal(rejoinCol, 0, pathRow);

            return cells;
        }

        public static IReadOnlyList<GridCell> GetSecondaryPathCells(LevelLayoutId layout) => layout switch
        {
            LevelLayoutId.DoubleTrouble => DoubleTroubleSecondaryPath,
            _ => null
        };

        public static IReadOnlyList<GridCell> GetWaterCells(LevelLayoutId layout) => layout switch
        {
            LevelLayoutId.DoubleTrouble => DoubleTroubleWaterCells,
            _ => null
        };

        public static PathLane GetSpawnEntranceLane(LevelLayoutId layout, int col, int row)
        {
            if (layout != LevelLayoutId.DoubleTrouble)
                return PathLane.None;

            if (MatchesUserCell(col, row, 1, 17))
                return PathLane.Primary;

            if (MatchesUserCell(col, row, 11, 17))
                return PathLane.Secondary;

            return PathLane.None;
        }

        public static bool IsSpawnEntrance(LevelLayoutId layout, int col, int row, bool isPathCell)
        {
            if (!isPathCell)
                return false;

            if (layout == LevelLayoutId.LayoutMaker)
                return false;

            if (layout == LevelLayoutId.DoubleTrouble)
                return GetSpawnEntranceLane(layout, col, row) != PathLane.None;

            return col == GridManager.SpawnColumn;
        }

        static readonly GridCell[] OuterRingRoutePath = BuildOuterRingRoutePath();

        static readonly GridCell[] OuterRingAllPathCells = BuildOuterRingAllPathCells();

        static readonly GridCell[] DoubleTroublePrimaryPath = BuildDoubleTroublePrimaryPath();

        static readonly GridCell[] DoubleTroubleSecondaryPath = BuildDoubleTroubleSecondaryPath();

        static readonly GridCell[] DoubleTroubleWaterCells = BuildDoubleTroubleWaterCells();

        static GridCell[] BuildDoubleTroubleWaterCells()
        {
            const int rectMinUserRow = 4;
            const int rectMaxUserRow = 8;
            const int rectMinUserCol = 5;
            const int rectMaxUserCol = 13;
            const int dryMinUserRow = 5;
            const int dryMaxUserRow = 7;
            const int dryMinUserCol = 8;
            const int dryMaxUserCol = 10;

            var cells = new List<GridCell>();
            for (var userRow = rectMinUserRow; userRow <= rectMaxUserRow; userRow++)
            for (var userCol = rectMinUserCol; userCol <= rectMaxUserCol; userCol++)
            {
                if (userRow >= dryMinUserRow && userRow <= dryMaxUserRow
                    && userCol >= dryMinUserCol && userCol <= dryMaxUserCol)
                    continue;

                cells.Add(GridCell.FromUserCell(userRow, userCol));
            }

            return cells.ToArray();
        }

        static GridCell[] BuildOuterRingRoutePath() => BuildUserRoutePath(cells =>
        {
            TraceHorizontalUserRoute(cells, 6, 17, 14);
            TraceOuterRingTopArc(cells);
            TraceOuterRingBottomArc(cells);
            TraceOuterRingTopArc(cells);
            TraceHorizontalUserRoute(cells, 6, 5, 1);
        });

        static void TraceOuterRingTopArc(List<GridCell> cells)
        {
            TraceVerticalUserRoute(cells, 13, 6, 3);
            TraceHorizontalUserRoute(cells, 3, 13, 5);
            TraceVerticalUserRoute(cells, 5, 3, 6);
        }

        static void TraceOuterRingBottomArc(List<GridCell> cells)
        {
            TraceVerticalUserRoute(cells, 5, 6, 9);
            TraceHorizontalUserRoute(cells, 9, 5, 13);
            TraceVerticalUserRoute(cells, 13, 9, 6);
        }

        static GridCell[] BuildOuterRingAllPathCells() => BuildUserPath(cells =>
        {
            TraceHorizontalUser(cells, 6, 17, 14);
            TraceHorizontalUser(cells, 6, 5, 1);
            TraceVerticalUser(cells, 5, 3, 9);
            TraceVerticalUser(cells, 13, 3, 9);
            TraceHorizontalUser(cells, 3, 5, 13);
            TraceHorizontalUser(cells, 9, 5, 13);
        });

        static GridCell[] BuildDoubleTroublePrimaryPath() => BuildUserPath(cells =>
        {
            TraceHorizontalUser(cells, 1, 17, 15);
            TraceVerticalUser(cells, 15, 1, 5);
            AddUserCell(cells, 5, 16);
            TraceVerticalUser(cells, 16, 5, 7);
            TraceHorizontalUser(cells, 7, 16, 14);
            TraceVerticalUser(cells, 14, 7, 9);
            TraceHorizontalUser(cells, 9, 14, 4);
            TraceVerticalUser(cells, 4, 9, 7);
            TraceHorizontalUser(cells, 7, 4, 2);
            TraceVerticalUser(cells, 2, 7, 5);
            AddUserCell(cells, 5, 3);
            TraceVerticalUser(cells, 3, 5, 1);
            TraceHorizontalUser(cells, 1, 3, 1);
        });

        static GridCell[] BuildDoubleTroubleSecondaryPath() => BuildUserPath(cells =>
        {
            TraceHorizontalUser(cells, 11, 17, 15);
            TraceVerticalUser(cells, 15, 11, 7);
            AddUserCell(cells, 7, 14);
            TraceVerticalUser(cells, 14, 7, 3);
            TraceHorizontalUser(cells, 3, 14, 4);
            TraceVerticalUser(cells, 4, 3, 7);
            AddUserCell(cells, 7, 3);
            TraceVerticalUser(cells, 3, 7, 11);
            TraceHorizontalUser(cells, 11, 3, 1);
        });

        static GridCell[] BuildUserPath(System.Action<List<GridCell>> trace)
        {
            var cells = new List<GridCell>();
            trace(cells);
            return cells.ToArray();
        }

        static GridCell[] BuildUserRoutePath(System.Action<List<GridCell>> trace)
        {
            var cells = new List<GridCell>();
            trace(cells);
            return cells.ToArray();
        }

        static void AddUserCell(List<GridCell> cells, int userRow, int userCol)
        {
            var cell = GridCell.FromUserCell(userRow, userCol);
            if (cells.Count > 0 && cells[cells.Count - 1].Col == cell.Col && cells[cells.Count - 1].Row == cell.Row)
                return;

            if (cells.Exists(existing => existing.Col == cell.Col && existing.Row == cell.Row))
                return;

            cells.Add(cell);
        }

        static void AddUserRouteCell(List<GridCell> cells, int userRow, int userCol)
        {
            var cell = GridCell.FromUserCell(userRow, userCol);
            if (cells.Count > 0 && cells[cells.Count - 1].Col == cell.Col && cells[cells.Count - 1].Row == cell.Row)
                return;

            cells.Add(cell);
        }

        static void TraceHorizontalUser(List<GridCell> cells, int userRow, int fromCol, int toCol)
        {
            var step = fromCol >= toCol ? -1 : 1;
            for (var col = fromCol; col != toCol; col += step)
                AddUserCell(cells, userRow, col);
            AddUserCell(cells, userRow, toCol);
        }

        static void TraceVerticalUser(List<GridCell> cells, int userCol, int fromRow, int toRow)
        {
            var step = fromRow >= toRow ? -1 : 1;
            for (var row = fromRow; row != toRow; row += step)
                AddUserCell(cells, row, userCol);
            AddUserCell(cells, toRow, userCol);
        }

        static void TraceHorizontalUserRoute(List<GridCell> cells, int userRow, int fromCol, int toCol)
        {
            var step = fromCol >= toCol ? -1 : 1;
            for (var col = fromCol; col != toCol; col += step)
                AddUserRouteCell(cells, userRow, col);
            AddUserRouteCell(cells, userRow, toCol);
        }

        static void TraceVerticalUserRoute(List<GridCell> cells, int userCol, int fromRow, int toRow)
        {
            var step = fromRow >= toRow ? -1 : 1;
            for (var row = fromRow; row != toRow; row += step)
                AddUserRouteCell(cells, row, userCol);
            AddUserRouteCell(cells, toRow, userCol);
        }

        static bool MatchesUserCell(int col, int row, int userRow, int userCol)
        {
            var cell = GridCell.FromUserCell(userRow, userCol);
            return col == cell.Col && row == cell.Row;
        }
    }
}
