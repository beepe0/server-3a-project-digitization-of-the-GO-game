using Network.UnityServer;
using Network.UnityTools;
using UnityEngine;

namespace Network.Connection
{
    public abstract class ConnectionRules
    {
        public enum PacketType : ushort
        {
            HandShake,
            SynchronizePosition
        }
        public class GeneralRules : UNetworkServerIORules.IGeneralRules
        {
            public void OnWelcome(ushort clientId)
            {
                Connection.Instance.OutputRules.HandShake(clientId);
            }
            public void OnDisconnect(ushort clientId)
            {
                Debug.Log($"[ID: {clientId}] The client disconnected!");
            }
            public void OnClose()
            {
                Debug.Log($"The server closed!");
            }
        }
        public class InputRules : UNetworkServerIORules.IInputRules
        {
            public void HandShake(ushort clientId, UNetworkReadablePacket inputPacket)
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
        public class OutputRules : UNetworkServerIORules.IOutputRules
        {
            public void HandShake(ushort clientId)
            {
                UNetworkIOPacket packet = new UNetworkIOPacket((ushort)PacketType.HandShake);
                
                packet.Write("S-OK!");
                
                Connection.Instance.DataHandler.SendDataTcp(clientId, packet);
            }
        }
    }
}