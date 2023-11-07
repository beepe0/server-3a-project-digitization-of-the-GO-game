using System.Collections.Generic;
using CustomEditor.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Go
{
    public class GoBoard : MonoBehaviour
    {
        public GoPawn[] pawns;
        public List<GoPawn> openPawns;
        [ReadOnlyInspector] public GameObject pawnCursor;
        [ReadOnlyInspector] public Vector2 offset;
        [ReadOnlyInspector] public Vector2 pawnOffset;
        [ReadOnlyInspector] public uint numberOfSteps;
    }
} 