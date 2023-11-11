using System;
using System.Text;
using Go;
using Network.UnityServer;
using Network.UnityTools;
using UnityEngine;

namespace Network.Connection
{
    public class Connection : UNetworkServer
    {
        [SerializeField] private GoGame _goGame;
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
            RulesHandler.AddRule((ushort)PacketType.ConsoleCommand, ConsoleCommand);
        }
        public override void OnDisconnectClient(ushort clientId)
        {
            Debug.Log($"[{clientId}] Client was disconnected!");
            DisconnectingPlayer(clientId);
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
                _goGame.StartGame(clientId);
                ConnectingPlayer(clientId);
            }
            else
            { 
                Debug.Log($"Client number {id}:{clientId}:{readablePacket.Index} didn't connected successfully!");
                Clients[clientId].Close();
            }
        }
        private void DisconnectingPlayer(ushort clientId)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)PacketType.DisconnectingPlayer);
            
            DataHandler.SendDataToAllExceptClientTcp(clientId, packet);
        }
        private void ConnectingPlayer(ushort clientId)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)PacketType.ConnectingPlayer);
            
            DataHandler.SendDataToAllExceptClientTcp(clientId, packet);
        }
        private void ConsoleCommand(ushort clientId, UNetworkReadablePacket readablePacket)
        {
            string value = readablePacket.ReadString();
            bool isGlobal = true; 
            string answer = "";

            string[] keys = value.Split(' ');

            switch (keys[0])
            {
                case "global" :
                    if (keys[1].Equals("say"))
                    {
                        for(int i = 2; i < keys.Length; i++) answer += keys[i] + " ";
                    }
                    else if(keys[1].Equals("clear-desk"))
                    {
                        _goGame.Board.ClearDesk(clientId);
                        answer = "was cleared the desk!";
                    }
                    break;
                default: 
                    isGlobal = false;
                    answer = $"command \"{value}\" wasn't found!";
                    break;
            }
            
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)PacketType.ConsoleCommand);
            packet.Write(answer);
            
            if (isGlobal)
                DataHandler.SendDataToAllTcp(clientId, packet);
            else
                DataHandler.SendDataTcp(clientId, packet);
            
        }
        public enum PacketType : byte
        {
            HandShake,
            DisconnectingPlayer,
            ConnectingPlayer,
            StartGame,
            UpdatePlayer,
            PawnOpen,
            PawnClose,
            ConsoleCommand,
        }
    }
}