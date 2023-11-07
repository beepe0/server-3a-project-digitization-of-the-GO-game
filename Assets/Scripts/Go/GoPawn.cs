using System;
using System.Collections.Generic;
using System.Linq;
using CustomEditor.Attributes;
using UnityEngine;

namespace Go
{
    [Serializable]
    public class GoPawn
    { 
        [ReadOnlyInspector] public ushort index;
        [ReadOnlyInspector] public uint blockTime;
        [ReadOnlyInspector] public bool isClosed;
        [ReadOnlyInspector] public bool isBlocked;
        [ReadOnlyInspector] public NodeType pawnType;

        [ReadOnlyInspector] public GameObject pawnObject;
        [ReadOnlyInspector] public MeshRenderer pawnMeshRenderer;
        [ReadOnlyInspector] public Vector3 pawnPosition;
        
        [NonSerialized]
        public GoGame MainGame;
        [NonSerialized]
        public GoPawn[] Neighbours;
        [NonSerialized]
        public List<GoPawn> listOfConnectedNeighbours;
        [NonSerialized]
        public GoPawn lider;
        [NonSerialized] 
        public static Vector2[] OffsetNeighbours = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        
        public GoPawn(GoGame mainGame, ushort index, GameObject pawnObject)
        {
            this.Neighbours = new GoPawn[4];

            this.index = index;
            this.MainGame = mainGame;
            this.pawnObject = pawnObject;
            this.pawnPosition = pawnObject.transform.position;
            this.pawnMeshRenderer = pawnObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
        }
		
		public void CloseMe(ushort clientId)
        {
			this.isClosed = false;
            this.isBlocked = this.lider.listOfConnectedNeighbours.Count == 1 && this.GetNumberOfEnemyNeighbours() == 4;
            this.blockTime = this.isBlocked ? MainGame.Board.numberOfSteps + 1 : 0;
            this.pawnType = NodeType.None;
            this.pawnMeshRenderer.material = MainGame.Settings.materialPawnNone;
            // this.listOfConnectedNeighbours = null;
            this.lider = null;
            this.pawnObject.SetActive(false);
            
            this.MainGame.Board.openPawns.Remove(this);
            
            this.MainGame.PawnClose(clientId, this);
		}

		public GoPawn OpenMe(ushort clientId, NodeType nodeType)
		{
            Debug.Log($"BT: {this.blockTime}, NoS: {MainGame.Board.numberOfSteps}");
            if (this.isClosed || (this.isBlocked && (this.blockTime - MainGame.Board.numberOfSteps) > 0)) return null;
    
            //this.index = (ushort)MainGame.Board.openPawns.Count;
            this.isClosed = true;
            this.isBlocked = false;
            this.pawnType = nodeType;
            this.pawnMeshRenderer.material = nodeType == NodeType.PawnA ? MainGame.Settings.materialPawnA : MainGame.Settings.materialPawnB;
            this.pawnObject.transform.localScale = new Vector3(MainGame.Settings.pawnsSize, 0.5f, MainGame.Settings.pawnsSize);
            this.pawnObject.SetActive(true);
            
            this.MainGame.Board.openPawns.Add(this);
            
            this.MainGame.PawnOpen(clientId, this);
            return this;
        }

        public ushort GetNumberOfEmptyNeighbours()
        {
            ushort count = 0;
            foreach (GoPawn node in Neighbours)
            {
                if (node == null || node.isClosed) continue;
                count++;
            }

            return count;
        }
        public ushort GetNumberOfMyNeighbours()
        {
            ushort count = 0;
            foreach (GoPawn node in Neighbours)
            {
                if (node == null || (node.pawnType != this.pawnType)) continue;
                count++;
            }

            return count;
        }

        public ushort GetNumberOfEnemyNeighbours()
        {
            ushort count = 0;
            foreach (GoPawn node in Neighbours)
            {
                if (node == null || (node.pawnType == this.pawnType || node.pawnType == NodeType.None)) continue;
                count++;
            }

            return count;
        }
        
        public ushort GetNumberOfMyNeighboursAndEmpty()
        {
            ushort count = 0;
            foreach (GoPawn node in Neighbours)
            {
                if (node == null || (node.isClosed && node.pawnType != this.pawnType)) continue;
                count++;
            }

            return count;
        }

        public ushort GetNumberOfNeighbours()
        {
            ushort count = 0;
            foreach (GoPawn node in Neighbours)
            {
                if (node == null) continue;
                count++;
            }

            return count;
        }
        
        public GoPawn GetFirstMyNeighbour()
        {
            foreach (GoPawn node in Neighbours)
            {
                if (node != null && (node.pawnType == this.pawnType)) return node;
            }

            return null;
        }

        public GoPawn GetBetterMyNeighbourOption()
        {
            int indexBestOption;
            GoPawn bestOption;
            List<GoPawn> tempOfMyNeighbours = new List<GoPawn>();
            
            tempOfMyNeighbours.AddRange(this.Neighbours.Where(e => (e != null && e.pawnType == this.pawnType)));
            indexBestOption = tempOfMyNeighbours.FindIndex(e =>
                e.lider.listOfConnectedNeighbours.Count ==
                tempOfMyNeighbours.Max(v => v.lider.listOfConnectedNeighbours.Count));
            bestOption = tempOfMyNeighbours[indexBestOption];

            for (int i = 0; i < tempOfMyNeighbours.Count; i++)
            {
                if (tempOfMyNeighbours.Count > 1 && i != indexBestOption && bestOption.lider.listOfConnectedNeighbours != tempOfMyNeighbours[i].lider.listOfConnectedNeighbours)
                {
                    bestOption.lider.listOfConnectedNeighbours.AddRange(tempOfMyNeighbours[i].lider.listOfConnectedNeighbours);
                    tempOfMyNeighbours[i].lider.listOfConnectedNeighbours = bestOption.lider.listOfConnectedNeighbours;
                    tempOfMyNeighbours[i].lider = bestOption.lider;
                }
            }

            return bestOption;
        }

        public bool CanLive()
        {
            foreach(GoPawn gp in lider.listOfConnectedNeighbours)
            {
                if(gp.GetNumberOfEmptyNeighbours() > 0) return true;
            }
            return false;
        }

        public void RemoveAllFromListOfConnectedNeighbours(ushort clientId) =>
            listOfConnectedNeighbours.ForEach(e => e.CloseMe(clientId) );
    }

    public enum NodeType : byte
    {
        None,
        PawnA,
        PawnB,
    }
}