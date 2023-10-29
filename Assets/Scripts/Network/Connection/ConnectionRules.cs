using Network.UNTools;
using Network.UServer;
using UnityEngine;

namespace Network.Connection
{
    public class ConnectionRules
    {
        public enum PacketType : ushort
        {
            OnWelcome,
            SynchronizePosition
        }
        public class GeneralRules : UNetworkIORules.IGeneralRules
        {
            public void OnDisconnect(ushort clientId)
            {
                Debug.Log($"[ID: {clientId}] The client disconnected!");
            }

            public void OnClose()
            {
                Debug.Log($"The server closed!");
            }
        }
        public class InputRules : UNetworkIORules.IInputRules
        {
            public void OnWelcome(ushort clientId, UNetworkReadablePacket inputPacket)
            {
                Debug.Log($"ID: {inputPacket.Index}, ID: {clientId} LN: {inputPacket.Length}, PT: {(PacketType)inputPacket.PacketNumber}, DT: {inputPacket.ReadString()}");
            }
            
            public void SynchronizePosition(ushort clientId, UNetworkReadablePacket inputPacket)
            {
                Vector3 pos = new Vector3(inputPacket.ReadFloat(), inputPacket.ReadFloat(), inputPacket.ReadFloat());
                Debug.Log($"ID: {inputPacket.Index}, ID: {clientId} LN: {inputPacket.Length}, PT: {(PacketType)inputPacket.PacketNumber}, DT: {pos}");
                Connection.Instance.gameObject.transform.position = pos;
            }
        }
        public class OutputRules : UNetworkIORules.IOutputRules
        {
            public void OnWelcome(ushort clientId)
            {
                UNetworkIOPacket packet = new UNetworkIOPacket((ushort)PacketType.OnWelcome);
                
                packet.Write("123");
                
                UNetworkCore.DataHandler.Tcp.SendData(clientId, packet);
            }
        }
    }
}