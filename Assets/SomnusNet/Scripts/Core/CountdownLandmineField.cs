using System.Collections.Generic;
using SomnusNet.Units;
using SomnusNet.Visual;
using UnityEngine;

namespace SomnusNet.Core
{
    public static class CountdownLandmineField
    {
        struct MineCell
        {
            public int count;
            public int damage;
            public DreamBlob owner;
            public GameObject visualRoot;
        }

        static readonly Dictionary<long, MineCell> _cells = new();
        static readonly Dictionary<long, int> _pendingCounts = new();

        public static Sprite MineSprite => CreateMineSprite();

        public static void ClearAll()
        {
            foreach (var entry in _cells.Values)
            {
                if (entry.visualRoot != null)
                    Object.Destroy(entry.visualRoot);
            }

            _cells.Clear();
            _pendingCounts.Clear();
        }

        public static bool TryGetMinePlacementTarget(DreamBlob owner, int blobCol, int blobRow, int rangeSize,
            out int col, out int row)
        {
            col = 0;
            row = 0;

            if (owner == null || !owner.IsAlive || GridManager.Instance == null)
                return false;

            var maxMines = CountdownRules.GetMaxMinesPerTile(owner);
            var grid = GridManager.Instance;
            grid.GetBlobRangeBounds(blobCol, blobRow, out var minCol, out var maxCol, out var minRow, out var maxRow,
                rangeSize);

            var candidates = new List<(int col, int row, int count)>();
            for (var c = minCol; c <= maxCol; c++)
            for (var r = minRow; r <= maxRow; r++)
            {
                if (!grid.IsPathCell(c, r))
                    continue;

                var key = CellKey(c, r);
                var count = GetEffectiveMineCount(key);
                if (count >= maxMines)
                    continue;

                candidates.Add((c, r, count));
            }

            if (candidates.Count == 0)
                return false;

            var minCount = int.MaxValue;
            foreach (var candidate in candidates)
                minCount = Mathf.Min(minCount, candidate.count);

            var best = new List<(int col, int row)>();
            foreach (var candidate in candidates)
            {
                if (candidate.count == minCount)
                    best.Add((candidate.col, candidate.row));
            }

            var pick = best[Random.Range(0, best.Count)];
            col = pick.col;
            row = pick.row;
            return true;
        }

        public static void ReservePendingMine(int col, int row)
        {
            var key = CellKey(col, row);
            _pendingCounts.TryGetValue(key, out var pending);
            _pendingCounts[key] = pending + 1;
        }

        public static void ReleasePendingMine(int col, int row)
        {
            var key = CellKey(col, row);
            if (!_pendingCounts.TryGetValue(key, out var pending))
                return;

            pending--;
            if (pending <= 0)
                _pendingCounts.Remove(key);
            else
                _pendingCounts[key] = pending;
        }

        public static void CommitMineAt(int col, int row, int damage, DreamBlob owner)
        {
            ReleasePendingMine(col, row);
            if (owner == null || !owner.IsAlive)
                return;

            AddMineAt(col, row, damage, owner, CountdownRules.GetMaxMinesPerTile(owner));
        }

        public static bool TryPlaceMine(DreamBlob owner, int blobCol, int blobRow, int rangeSize, int damage)
        {
            if (!TryGetMinePlacementTarget(owner, blobCol, blobRow, rangeSize, out var col, out var row))
                return false;

            ReservePendingMine(col, row);
            CountdownMinePlaceEffect.Play(owner, col, row, damage, null);
            return true;
        }

        static int GetEffectiveMineCount(long key)
        {
            var count = _cells.TryGetValue(key, out var cell) ? cell.count : 0;
            if (_pendingCounts.TryGetValue(key, out var pending))
                count += pending;
            return count;
        }

        public static void TryTriggerAt(int col, int row, Glitch trigger)
        {
            if (trigger == null || !trigger.IsTargetable || GridManager.Instance == null)
                return;

            if (!LoopedModifierRules.CanTriggerLandmines(trigger))
                return;

            var grid = GridManager.Instance;
            if (!grid.IsPathCell(col, row))
                return;

            var key = CellKey(col, row);
            if (!_cells.TryGetValue(key, out var cell) || cell.count <= 0)
                return;

            var owner = cell.owner;
            var detonateAll = owner != null && owner.IsAlive && owner.HasHigher;
            var mineCount = detonateAll ? cell.count : 1;
            var damage = cell.damage;
            var blastRange = CountdownRules.GetBlastRangeSize(owner);

            if (detonateAll)
            {
                _cells.Remove(key);
                if (cell.visualRoot != null)
                    Object.Destroy(cell.visualRoot);
            }
            else
            {
                cell.count -= 1;
                if (cell.count <= 0)
                {
                    _cells.Remove(key);
                    if (cell.visualRoot != null)
                        Object.Destroy(cell.visualRoot);
                }
                else
                {
                    _cells[key] = cell;
                    RefreshVisual(ref cell, col, row, CountdownRules.GetMaxMinesPerTile(owner));
                    _cells[key] = cell;
                }
            }

            Detonate(col, row, damage, owner, mineCount, blastRange);
        }

        static void AddMineAt(int col, int row, int damage, DreamBlob owner, int maxMines)
        {
            var key = CellKey(col, row);
            if (!_cells.TryGetValue(key, out var cell))
            {
                cell = new MineCell
                {
                    count = 0,
                    damage = damage,
                    owner = owner
                };
            }

            cell.count = Mathf.Min(maxMines, cell.count + 1);
            cell.damage = damage;
            cell.owner = owner;
            RefreshVisual(ref cell, col, row, maxMines);
            _cells[key] = cell;
        }

        static void RefreshVisual(ref MineCell cell, int col, int row, int maxMines)
        {
            var grid = GridManager.Instance;
            if (grid == null)
                return;

            if (cell.visualRoot == null)
            {
                cell.visualRoot = new GameObject($"CountdownMine_{col}_{row}");
                cell.visualRoot.transform.position = grid.CellToWorld(col, row);
            }

            while (cell.visualRoot.transform.childCount < cell.count)
            {
                var index = cell.visualRoot.transform.childCount;
                var mineGo = new GameObject($"Stack_{index}");
                mineGo.transform.SetParent(cell.visualRoot.transform);
                mineGo.transform.localPosition = MineStackOffset(index, maxMines);
                var sr = mineGo.AddComponent<SpriteRenderer>();
                sr.sprite = CreateMineSprite();
                sr.sortingOrder = 8;
                var scale = grid.cellSize * 0.32f * (1f + index * 0.06f);
                mineGo.transform.localScale = Vector3.one * scale;
                sr.color = MineStackTint(index, maxMines);
            }

            var excess = cell.visualRoot.transform.childCount - cell.count;
            for (var i = 0; i < excess; i++)
            {
                var childIndex = cell.visualRoot.transform.childCount - 1;
                if (childIndex < 0)
                    break;

                Object.Destroy(cell.visualRoot.transform.GetChild(childIndex).gameObject);
            }
        }

        static Color MineStackTint(int index, int maxMines)
        {
            var t = maxMines <= 1 ? 0f : index / (float)(maxMines - 1);
            return Color.Lerp(new Color(1f, 0.55f, 0.18f, 0.88f), new Color(0.82f, 0.12f, 0.08f, 0.96f), t);
        }

        static Vector3 MineStackOffset(int index, int maxMines)
        {
            if (index <= 0)
                return Vector3.zero;

            var angle = index * (360f / Mathf.Max(3, maxMines)) * Mathf.Deg2Rad;
            var radius = 0.05f + index * 0.025f;
            return new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        }

        static void Detonate(int col, int row, int fallbackDamage, DreamBlob owner, int mineCount, int blastRangeSize)
        {
            var grid = GridManager.Instance;
            if (grid == null)
                return;

            SpawnBlastFlash(col, row, blastRangeSize, mineCount);

            var targets = new List<Glitch>();
            grid.ForEachGlitchInBlobBand(col, row, blastRangeSize, glitch =>
            {
                if (glitch != null && glitch.IsTargetable)
                    targets.Add(glitch);
            });

            foreach (var glitch in targets)
            {
                if (glitch == null || !glitch.IsTargetable)
                    continue;
                if (!LoopedModifierRules.CanAreaEffectDamage(glitch, owner))
                    continue;

                var damage = owner != null && owner.IsAlive
                    ? owner.ComputeDamageAgainst(glitch)
                    : fallbackDamage;

                for (var i = 0; i < mineCount; i++)
                    glitch.TakeDamage(damage, owner);
            }
        }

        static void SpawnBlastFlash(int col, int row, int blastRangeSize, int mineCount)
        {
            var grid = GridManager.Instance;
            if (grid == null)
                return;

            var root = new GameObject("CountdownMineBlast");
            root.transform.position = grid.CellToWorld(col, row);
            grid.GetBlobRangeBounds(col, row, out var minCol, out var maxCol, out var minRow, out var maxRow,
                blastRangeSize);

            var scaleBoost = 0.9f + Mathf.Min(0.35f, (mineCount - 1) * 0.08f);
            for (var c = minCol; c <= maxCol; c++)
            for (var r = minRow; r <= maxRow; r++)
            {
                if (!grid.IsInside(c, r))
                    continue;

                var cellGo = new GameObject($"BlastCell_{c}_{r}");
                cellGo.transform.SetParent(root.transform);
                cellGo.transform.position = grid.CellToWorld(c, r);
                var sr = cellGo.AddComponent<SpriteRenderer>();
                sr.sprite = CreateMineSprite();
                sr.color = grid.IsPathCell(c, r)
                    ? new Color(1f, 0.72f, 0.22f, 0.78f)
                    : new Color(1f, 0.58f, 0.16f, 0.45f);
                sr.sortingOrder = grid.IsPathCell(c, r) ? 10 : 12;
                cellGo.transform.localScale = Vector3.one * (grid.cellSize * scaleBoost);
            }

            Object.Destroy(root, 0.35f);
        }

        static Sprite _mineSprite;

        static Sprite CreateMineSprite()
        {
            if (_mineSprite != null)
                return _mineSprite;

            const int size = 24;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size * 0.5f, size * 0.5f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x - center.x) / center.x;
                var dy = (y - center.y) / center.y;
                var dist = Mathf.Sqrt(dx * dx + dy * dy);
                var alpha = Mathf.Clamp01(1f - dist);
                alpha *= alpha;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

            tex.Apply();
            _mineSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24f);
            return _mineSprite;
        }

        static long CellKey(int col, int row) => ((long)col << 32) | (uint)row;
    }
}
