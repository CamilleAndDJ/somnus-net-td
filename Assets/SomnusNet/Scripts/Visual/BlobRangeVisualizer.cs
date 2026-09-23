using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.Visual
{
    /// <summary>Draws the blob's 5×5 attack range in the game view.</summary>
    public class BlobRangeVisualizer : MonoBehaviour
    {
        static Sprite _cellSprite;

        void Start()
        {
            BuildRangeOverlay();
        }

        void BuildRangeOverlay()
        {
            var blob = GetComponent<Units.DreamBlob>();
            var grid = GridManager.Instance;
            if (blob == null || grid == null) return;

            EnsureCellSprite();
            var root = new GameObject("RangeOverlay");
            root.transform.SetParent(transform);
            root.transform.localPosition = Vector3.zero;

            grid.GetBlobRangeBounds(blob.Column, blob.Lane, out var minCol, out var maxCol,
                out var minRow, out var maxRow, blob.RangeSize);

            var fillColor = blob.RangeSize < GridManager.BlobRangeSize
                ? new Color(1f, 0.42f, 0.18f, 0.16f)
                : blob.RangeSize > GridManager.BlobRangeSize
                    ? new Color(0.45f, 0.72f, 1f, 0.14f)
                    : new Color(0.45f, 0.85f, 1f, 0.1f);

            for (var c = minCol; c <= maxCol; c++)
            for (var r = minRow; r <= maxRow; r++)
            {
                if (!grid.IsInside(c, r) || grid.IsPathCell(c, r)) continue;

                var go = new GameObject($"Range_{c}_{r}");
                go.transform.SetParent(root.transform);
                go.transform.position = grid.CellToWorld(c, r);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _cellSprite;
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size = Vector2.one * grid.cellSize * 0.92f;
                sr.color = fillColor;
                sr.sortingOrder = -8;
            }
        }

        static void EnsureCellSprite()
        {
            if (_cellSprite != null) return;
            const int s = 8;
            var tex = new Texture2D(s, s);
            for (var y = 0; y < s; y++)
            for (var x = 0; x < s; x++)
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.85f));
            tex.Apply();
            _cellSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
        }
    }
}
