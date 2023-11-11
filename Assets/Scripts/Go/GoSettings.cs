using UnityEngine;

namespace Go
{
    public class GoSettings : MonoBehaviour
    {
        [Header("Pawns")]
        public GameObject prefabPawnCursor;
        public GameObject prefabPawnAB;
        public Material materialPawnA, materialPawnB, materialPawnNone;
        public float pawnsSize;

        [Header("Board")] 
        [SerializeField]
        public Material boardMaterial;
        public Vector2Int boardSize;
        [Range(2, 20)]
        public float cellsSize;
        [Range(1, 10)]
        public float cellsCoefSize;
    }
}