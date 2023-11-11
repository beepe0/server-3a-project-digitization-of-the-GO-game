using System.Collections.Generic;
using Network.UnityTools;
using UnityEngine;

namespace Go
{
    public class GoRules : MonoBehaviour
    {
        private GoSettings _goSettings;
        private GoBoard _goBoard;
        private ushort _lastIndex;
        public void GameInitialization(GoGame goGame)
        {
            _goSettings = goGame.Settings;
            _goBoard = goGame.Board;
            
            gameObject.transform.localScale = new Vector3((_goSettings.boardSize.x - 1) / _goSettings.cellsSize, 1, (_goSettings.boardSize.y - 1) / _goSettings.cellsSize);
            _goSettings.boardMaterial.mainTextureScale = new Vector2((_goSettings.boardSize.x - 1), (_goSettings.boardSize.y - 1));
            _goSettings.pawnsSize = (20 / _goSettings.cellsSize) / _goSettings.cellsCoefSize;
            
            _goBoard.pawnCursor = Instantiate(_goSettings.prefabPawnCursor, gameObject.transform);
            _goBoard.offset = new Vector2(gameObject.transform.localScale.x / 2, -gameObject.transform.localScale.z / 2);
            _goBoard.pawnOffset = new Vector2(_goBoard.offset.x, -_goBoard.offset.y);
            _goBoard.pawns = new GoPawn[_goSettings.boardSize.x * _goSettings.boardSize.y];

            for (int x = 0; x < _goSettings.boardSize.x; x++)
            {
                for (int y = 0; y > -_goSettings.boardSize.y; y--)
                {
                    short convertMatrixToLine = GoTools.ConvertMatrixToLine(_goSettings.boardSize, new Vector2(x, y));
                    
                    Vector3 newPos = new Vector3(x / _goSettings.cellsSize - (_goBoard.pawnOffset.x), 0.5f, y / _goSettings.cellsSize + (_goBoard.pawnOffset.y));
                    GameObject pawnObject = Instantiate(_goSettings.prefabPawnAB, newPos, Quaternion.identity, gameObject.transform);
                    GoPawn node = new GoPawn(goGame, (ushort)convertMatrixToLine, pawnObject);

                    pawnObject.SetActive(false);
                    pawnObject.name = $"xyz: {newPos}";
                    pawnObject.transform.SetParent(gameObject.transform);
                    
                    for(ushort i = 0; i < 4; i++)
                    {
                        short mtl = GoTools.ConvertMatrixToLine(_goSettings.boardSize, new Vector2(x + GoPawn.OffsetNeighbours[i].x, y + GoPawn.OffsetNeighbours[i].y));
                        node.Neighbours[i] = mtl >= 0 && mtl < _goBoard.pawns.Length ? _goBoard.pawns[mtl] : null;
                    }
                    
                    _goBoard.pawns[convertMatrixToLine] = node;
                }
            }
            
            for (int x = 0; x < _goSettings.boardSize.x; x++)
            {
                for (int y = 0; y > -_goSettings.boardSize.y; y--)
                {
                    int convertMatrixToLine = GoTools.ConvertMatrixToLine(_goSettings.boardSize, new Vector2(x, y));
                    
                    for(ushort i = 0; i < 4; i++)
                    {
                        short mtl = GoTools.ConvertMatrixToLine(_goSettings.boardSize, new Vector2(x + GoPawn.OffsetNeighbours[i].x, y + GoPawn.OffsetNeighbours[i].y));
                        _goBoard.pawns[convertMatrixToLine].Neighbours[i] = mtl >= 0 && mtl < _goBoard.pawns.Length ? _goBoard.pawns[mtl] : null;
                    }
                }
            }
        }
        
        public void PawnInitialization(ushort clientId, UNetworkReadablePacket readablePacket)
        {
            GoPawn goPawn;
            short convertMatrixToLine = GoTools.ConvertRayToLine(new Vector2(readablePacket.ReadFloat(), readablePacket.ReadFloat()), _goBoard.offset, _goSettings.boardSize, _goSettings.cellsSize);

            if (convertMatrixToLine >= 0 && convertMatrixToLine < _goBoard.pawns.Length && _lastIndex != clientId)
            {
                goPawn = _goBoard.pawns[convertMatrixToLine].OpenMe(clientId, (_goBoard.numberOfSteps % 2 == 0) ? NodeType.PawnA : NodeType.PawnB);
                if (goPawn == null) return;
                
                ushort numberOfEmptyNeighbours = goPawn.GetNumberOfEmptyNeighbours();
                ushort numberOfMyNeighbours = goPawn.GetNumberOfMyNeighbours();
                ushort numberOfEnemyNeighbours = goPawn.GetNumberOfEnemyNeighbours();
                ushort numberOfNeighbours = goPawn.GetNumberOfNeighbours();
                
                if (goPawn.lider == null && (numberOfEmptyNeighbours == numberOfNeighbours || (numberOfEmptyNeighbours + numberOfEnemyNeighbours) == numberOfNeighbours))
                {
                    goPawn.listOfConnectedNeighbours = new List<GoPawn>{goPawn};
                    goPawn.lider = goPawn;
                }
                else if (goPawn.lider == null && numberOfMyNeighbours > 0)
                {
                    GoPawn betterOption = goPawn.GetBetterMyNeighbourOption();
                    betterOption.lider.listOfConnectedNeighbours.Add(goPawn);
                    goPawn.lider = betterOption.lider;
                }
                
                if (goPawn.lider != null)
                { 
                    if (!goPawn.CanLive() && goPawn.lider.listOfConnectedNeighbours.Count > 1)
                    {
                        goPawn.lider.listOfConnectedNeighbours.Remove(goPawn);
                        goPawn.CloseMe(clientId);
                    }else 
                    {
                        _goBoard.numberOfSteps++;
                        _lastIndex = clientId;
                    }
                }
            }
            
            UpdateBoard();
        }

        public void PawnPass() => _goBoard.numberOfSteps++;

        public void UpdateBoard()
        {
            for (int i = 0; i < _goBoard.openPawns.Count && _goBoard.openPawns.Count > 0; i++)
            {
                GoPawn goPawn = _goBoard.openPawns[i];
                if(goPawn.lider != null && !goPawn.CanLive())
                {
                    goPawn.lider.RemoveAllFromListOfConnectedNeighbours(0);
                }
            }
        }
    }
}