using System.Collections.Generic;
using Go;
using Network.Connection.Room;
using Network.UnityServer;
using Network.UnityTools;
using UnityEngine;

namespace Network.Connection.Player
{
    public class GlobalPlayer : UNetworkClient
    {
        private void HandShake(ushort clientId)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)Connection.PacketType.HandShake);
            
            CurrentServer.DataHandler.SendDataTcp(clientId, packet);
        }
        private void DisconnectingPlayer(ushort clientId)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)Connection.PacketType.DisconnectingPlayer);
            
            CurrentServer.DataHandler.SendDataToAllExceptClientTcp(clientId, packet);
        }
        public override void OnDisconnectClient()
        {
            Debug.Log($"[{Index}] Client was disconnected!");
            DisconnectingPlayer(Index);
        }
        public override void OnConnectClient()
        {
            Debug.Log($"[{Index}] Client was connected!");
            HandShake(Index);
        }

    }
}