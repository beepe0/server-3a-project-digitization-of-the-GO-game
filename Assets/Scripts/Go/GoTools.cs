using UnityEngine;

namespace Go
{
    public static class GoTools
    {
        public static short ConvertMatrixToLine(Vector2 boardSize, Vector2 xy) => (xy.x < 0 || xy.x >= boardSize.x || xy.y <= -boardSize.y || xy.y > 0) ? (short)-1 : (short)((boardSize.x * (short)Mathf.Abs(xy.y)) + (short)(xy.x));
        public static short ConvertRayToLine(Vector2 xy, Vector2 offset, Vector2 boardSize, float cellsSize) => ConvertMatrixToLine(boardSize, new Vector2(Mathf.Floor(((xy + offset) * cellsSize).x), Mathf.Ceil(((xy + offset) * cellsSize).y)));
    }
}