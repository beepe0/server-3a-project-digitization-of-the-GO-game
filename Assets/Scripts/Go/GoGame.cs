using Network.Connection;
using Network.UnityServer;
using Network.UnityTools;
using UnityEngine;

namespace Go
{
    [RequireComponent(typeof(GoSettings))]
    [RequireComponent(typeof(GoRules))]
    [RequireComponent(typeof(GoBoard))]
    public class GoGame : MonoBehaviour
    {
        [SerializeField] private Connection conn;
        
        [SerializeField] private GoSettings goSettings;
        [SerializeField] private GoRules goRules;
        [SerializeField] private GoBoard goBoard;
        
        public Connection Conn => conn;
        public GoSettings Settings => goSettings;
        public GoRules Rules => goRules;
        public GoBoard Board => goBoard;

        private void Awake()
        {
            conn = GameObject.FindWithTag("Network").GetComponent<Connection>();
            goSettings = gameObject.GetComponent<GoSettings>();
            goRules = gameObject.GetComponent<GoRules>();
            goBoard = gameObject.GetComponent<GoBoard>();
            
            conn.RulesHandler.AddRule((ushort)Connection.PacketType.StartGame, StartGame);
            conn.RulesHandler.AddRule((ushort)Connection.PacketType.PawnOpen, goRules.PawnInitialization);
        }
        private void Start()
        {
            goRules.GameInitialization(this);
        }
        private void StartGame(ushort clientId, UNetworkReadablePacket readablePacket)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)Connection.PacketType.StartGame);
            packet.Write(goSettings.pawnsSize);
            packet.Write(goSettings.boardSize.x);
            packet.Write(goSettings.boardSize.y);
            packet.Write(goSettings.cellsSize);
            packet.Write(goSettings.cellsCoefSize);
            conn.DataHandler.SendDataTcp(clientId, packet);

            foreach (UNetworkClient client in conn.Clients.Values)
            {
                packet = new UNetworkIOPacket((ushort)Connection.PacketType.UpdatePlayer);
                
                if (client.TcpHandler is { IsTcpConnect: true } && client.Index == clientId)
                {
                    packet.Write(clientId);
                    conn.DataHandler.SendDataToAllExceptClientTcp(clientId, packet);
                }
                else if (client.TcpHandler is { IsTcpConnect: true } && client.Index != clientId)
                {
                    packet.Write(client.Index);
                    conn.DataHandler.SendDataTcp(clientId, packet);
                }
            }
        }
        public void PawnOpen(ushort clientId, GoPawn goPawn)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)Connection.PacketType.PawnOpen);
            packet.Write(goPawn.index);
            packet.Write((byte)goPawn.pawnType);
            conn.DataHandler.SendDataToAllTcp(clientId, packet);
        }
        public void PawnClose(ushort clientId, GoPawn goPawn)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)Connection.PacketType.PawnClose);
            packet.Write(goPawn.index);
            conn.DataHandler.SendDataToAllTcp(clientId, packet);
        }
    }
}
