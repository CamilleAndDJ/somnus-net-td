using SomnusNet.Core;
using UnityEngine;

namespace SomnusNet.Visual
{
    public class GridVisualizer : MonoBehaviour
    {
        [SerializeField] GridManager grid;
        [SerializeField] Color cellColor = new(0.35f, 0.28f, 0.55f, 0.12f);
        [SerializeField] Color laneLineColor = new(0.5f, 0.45f, 0.85f, 0.2f);

        void OnDrawGizmos()
        {
            if (grid == null) return;
            Gizmos.color = cellColor;
            for (var c = 0; c < GridManager.ColumnCount; c++)
            for (var l = 0; l < GridManager.LaneCount; l++)
            {
                var center = grid.CellToWorld(c, l);
                Gizmos.DrawWireCube(center, Vector3.one * grid.cellSize * 0.92f);
            }
        }
    }
}
