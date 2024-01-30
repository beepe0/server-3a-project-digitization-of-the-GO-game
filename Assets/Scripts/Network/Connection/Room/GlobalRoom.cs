using System;
using System.Collections.Generic;
using CustomEditor.Attributes;
using Go;
using Network.Connection.Player;
using Network.UnityServer;
using Network.UnityTools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Network.Connection.Room
{
    [Serializable]
    public class GlobalRoom : UNetworkRoom
    {
        public GoGame mainGame;
        
        public void Create(GoGame mainGame)
        {
            base.Open();
            
            this.mainGame = mainGame;
            mainGame.gameObject.name = " ";
            mainGame.Settings.boardSize = new Vector2Int(13, 13);
            mainGame.Settings.cellsSize = 10;
            mainGame.Settings.cellsCoefSize = 1.5f;
            mainGame.InitializingGame(mainGame);
        }
        public void Enter(Stack<ushort> waitingPlayers)//???
        {
            for (ushort i = 0; i < CurrentServer.slotsInRoom; i++)
            {
                base.Enter(waitingPlayers.Pop());
            }

            StartGame(CurrentServer.Index);
        }
        public void StartGame(ushort clientId)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)Connection.PacketType.StartGame);
            packet.Write(mainGame.Settings.pawnsSize);
            packet.Write(mainGame.Settings.boardSize.x);
            packet.Write(mainGame.Settings.boardSize.y);
            packet.Write(mainGame.Settings.cellsSize);
            packet.Write(mainGame.Settings.cellsCoefSize);
            packet.Write(Index);

            if (mainGame.Board.openPawns.Count > 0)
            {
                packet.Write(mainGame.Board.openPawns.Count);
                foreach (GoPawn goPawn in mainGame.Board.openPawns)
                {
                    packet.Write(goPawn.index);
                    packet.Write((byte)goPawn.pawnType);
                }
            }
            
            CurrentServer.DataHandler.SendDataToAllTcp(clientId, packet);
            
            foreach (UNetworkClient c in Clients.Values)
            {
                foreach (UNetworkClient client in Clients.Values)
                {
                    packet = new UNetworkIOPacket((ushort)Connection.PacketType.UpdatePlayer);
                    if (client.TcpHandler is { IsTcpConnect: true } && client.Index == c.Index)
                    {
                        packet.Write(c.Index);
                        CurrentServer.DataHandler.SendDataToAllTcp(c.Index, packet);
                    }
                    else if (client.TcpHandler is { IsTcpConnect: true } && client.Index != c.Index)
                    {
                        packet.Write(client.Index);
                        CurrentServer.DataHandler.SendDataTcp(c.Index, packet);
                    }
                }
            }
        }
        public override void OnCreateRoom()
        {
            Debug.Log($"[{Index}] Room was created by client!");
        }
        public override void OnCloseRoom()
        {
            Debug.Log($"[{Index}] Room was closed!");
        }
        public override void OnEntryToRoomClient(ushort clientId)
        {
            Debug.Log($"[{clientId}] Client was enter to a room!");
        }
        public override void OnExitFromRoomClient(ushort clientId)
        {
            Debug.Log($"[{clientId}] Client was leave from a room!");
        }
    }
}