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
        
        public void ClearDesk(ushort clientId)
        {
            foreach (GoPawn goPawn in pawns)
            {
                if (goPawn.isClosed)
                {
                    goPawn.isClosed = false;
                    goPawn.isBlocked = false;
                    goPawn.blockTime = 0;
                    goPawn.pawnType = NodeType.None;
                    goPawn.lider = null;
                    goPawn.pawnObject.SetActive(false);
                    
                    openPawns.Remove(goPawn);

                    goPawn.MainGame.PawnClose(clientId, goPawn);
                }
            }
        }
    }
} 