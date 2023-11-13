using System;
using System.Collections.Generic;
using Network.Connection;
using Network.UnityTools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Go
{
    public class GoRules : MonoBehaviour
    { 
        private GoGame _goGame;
        private ushort _lastIndex;
        
        public void PawnInitialization(ushort clientId, UNetworkReadablePacket readablePacket)
        {
            GoPawn goPawn;
            short convertMatrixToLine = GoTools.ConvertRayToLine(new Vector2(readablePacket.ReadFloat(), readablePacket.ReadFloat()), _goGame.Board.offset, _goGame.Settings.boardSize, _goGame.Settings.cellsSize);

            if (convertMatrixToLine >= 0 && convertMatrixToLine < _goGame.Board.pawns.Length && _lastIndex != clientId)
            {
                goPawn = _goGame.Board.pawns[convertMatrixToLine].OpenMe(clientId, (_goGame.Board.numberOfSteps % 2 == 0) ? NodeType.PawnA : NodeType.PawnB);
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
                        _goGame.Board.numberOfSteps++;
                        _lastIndex = clientId;
                    }
                }
            }
            
            UpdateBoard();
        }

        public void PawnPass(ushort clientId, UNetworkReadablePacket readablePacket)
        {
            _goGame.Board.numberOfSteps++;
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)Connection.PacketType.ConsoleCommand);
            packet.Write($"passed [{_goGame.Board.numberOfSteps}]");
            packet.Write(false);
            packet.Write(true);
            _goGame.Conn.DataHandler.SendDataToAllTcp(clientId, packet);
        }
        public void UpdateBoard()
        {
            for (int i = 0; i < _goGame.Board.openPawns.Count && _goGame.Board.openPawns.Count > 0; i++)
            {
                GoPawn goPawn = _goGame.Board.openPawns[i];
                if(goPawn.lider != null && !goPawn.CanLive())
                {
                    goPawn.lider.RemoveAllFromListOfConnectedNeighbours(0);
                }
            }
        }
    }
}