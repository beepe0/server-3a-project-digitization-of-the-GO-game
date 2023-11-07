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
        }
        public override void OnDisconnectClient(ushort clientId)
        {
            Debug.Log($"[{clientId}] Client was disconnected!");
        }
        public override void OnConnectClient(ushort clientId)
        {
            Debug.Log($"[{clientId}] Client was connected!");
            Send(clientId);
        }

        private void Send(ushort clientId)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket(0);
            
            packet.Write($"Hello, your index - {clientId}");
            
            DataHandler.SendDataTcp(clientId, packet);
        }
    }
}