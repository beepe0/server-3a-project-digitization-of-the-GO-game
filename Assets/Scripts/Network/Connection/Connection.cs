using Network.UnityServer;
using Network.UnityTools;
using UnityEngine;

namespace Network.Connection
{
    public class Connection : UNetworkServer
    {
        private void Awake() {
            if (dontDestroyOnLoad) DontDestroyOnLoad(this);
            if (startOnAwake) StartServer();
        }
        private void FixedUpdate() => UNetworkUpdate.Update();
        private void OnApplicationQuit() => CloseServer();
        public override void OnCloseServer()
        {
            Debug.Log("OnCloseServer!");
        }
        public override void OnStartServer()
        {
            Debug.Log("OnStartServer!");
            RulesHandler.AddRule((ushort)PacketType.HandShake, HandShake);
        }
        public override void OnDisconnectClient(ushort clientId)
        {
            Debug.Log($"[{clientId}] Client was disconnected!");
        }
        public override void OnConnectClient(ushort clientId)
        {
            Debug.Log($"[{clientId}] Client was connected!");
            HandShake(clientId);
        }
        private void HandShake(ushort clientId)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)PacketType.HandShake);
            
            DataHandler.SendDataTcp(clientId, packet);
        }
        private void HandShake(ushort clientId, UNetworkReadablePacket readablePacket)
        {
            ushort id;
            if ((id = readablePacket.Index) == clientId)
            {
                Debug.Log($"Client number {id}:{clientId}:{readablePacket.Index} is connected successfully!");
            }
            else
            { 
                Debug.Log($"Client number {id}:{clientId}:{readablePacket.Index} didn't connected successfully!");
                Clients[clientId].Close();
            }
        }
        public enum PacketType : byte
        {
            HandShake,
            StartGame,
            UpdatePlayer,
            PawnOpen,
            PawnClose,
        }
    }
}