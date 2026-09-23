namespace SomnusNet.Core
{
    enum LayoutMakerPaintMode
    {
        None = 0,
        Primary = 1,
        Secondary = 2,
        Both = 3
    }

    public static class LayoutMakerPaintState
    {
        const int ColumnCount = GridManager.ColumnCount;
        const int RowCount = GridManager.RowCount;

        static LayoutMakerPaintMode[,] _modes;
        static bool _initialized;

        public static void Reset()
        {
            _modes = new LayoutMakerPaintMode[ColumnCount, RowCount];
            _initialized = true;
        }

        public static PathLane GetPathLane(int col, int row)
        {
            EnsureInitialized();
            if (col < 0 || col >= ColumnCount || row < 0 || row >= RowCount)
                return PathLane.None;

            return ToPathLane(_modes[col, row]);
        }

        public static PathLane Cycle(int col, int row)
        {
            EnsureInitialized();
            if (col < 0 || col >= ColumnCount || row < 0 || row >= RowCount)
                return PathLane.None;

            _modes[col, row] = _modes[col, row] switch
            {
                LayoutMakerPaintMode.None => LayoutMakerPaintMode.Primary,
                LayoutMakerPaintMode.Primary => LayoutMakerPaintMode.Secondary,
                LayoutMakerPaintMode.Secondary => LayoutMakerPaintMode.None,
                LayoutMakerPaintMode.Both => LayoutMakerPaintMode.None,
                _ => LayoutMakerPaintMode.None
            };

            return ToPathLane(_modes[col, row]);
        }

        public static void ApplyPreset(LevelLayoutId layout)
        {
            Reset();

            if (layout == LevelLayoutId.Begining)
            {
                foreach (var cell in LevelLayoutCatalog.GetBeginningPathCells())
                    _modes[cell.Col, cell.Row] = LayoutMakerPaintMode.Primary;
                return;
            }

            if (layout != LevelLayoutId.DoubleTrouble)
                return;

            foreach (var cell in LevelLayoutCatalog.GetPrimaryPathCells(layout))
                _modes[cell.Col, cell.Row] = LayoutMakerPaintMode.Primary;

            foreach (var cell in LevelLayoutCatalog.GetSecondaryPathCells(layout))
            {
                ref var mode = ref _modes[cell.Col, cell.Row];
                mode = mode == LayoutMakerPaintMode.Primary
                    ? LayoutMakerPaintMode.Both
                    : LayoutMakerPaintMode.Secondary;
            }
        }

        static PathLane ToPathLane(LayoutMakerPaintMode mode) => mode switch
        {
            LayoutMakerPaintMode.Primary => PathLane.Primary,
            LayoutMakerPaintMode.Secondary => PathLane.Secondary,
            LayoutMakerPaintMode.Both => PathLane.Primary | PathLane.Secondary,
            _ => PathLane.None
        };

        static void EnsureInitialized()
        {
            if (_initialized && _modes != null)
                return;

            Reset();
        }
    }
}
