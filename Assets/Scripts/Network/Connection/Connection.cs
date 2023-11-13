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
        [SerializeField] private GameObject _prefabBoard;
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
            bool clearField = true;
            bool showAnswer = true;
            string answer = "";

            string[] keys = value.Split(' ');

            switch (keys[0])
            {
                case "global" :
                    if (keys[1].Equals("say"))
                    {
                        for(int i = 2; i < keys  .Length; i++) answer += keys[i] + " ";
                    }
                    else if(keys[1].Equals("clear-board"))
                    {
                        _goGame.Board.ClearBoard(clientId);
                        answer = "cleared the board!";
                    }
                    else if (keys[1].Equals("create-game"))
                    {
                        isGlobal = true;
                        if (_goGame != null) Destroy(_goGame.gameObject);
                        
                        _goGame = Instantiate(_prefabBoard, GameObject.FindWithTag("Table").transform).GetComponent<GoGame>();
                        
                        _goGame.Settings.boardSize = new Vector2Int(int.Parse(keys[2]), int.Parse(keys[3]));
                        _goGame.Settings.cellsSize = float.Parse(keys[4]);
                        _goGame.Settings.cellsCoefSize = float.Parse(keys[5]);
                        _goGame.InitializingGame(_goGame);
                        answer = "created the game!";
                    }
                    else if (keys[1].Equals("join-game"))
                    {
                        isGlobal = false;
                        showAnswer = false;
                        _goGame.JoinGame(clientId);
                    }
                    break;
                default:
                    clearField = false;
                    isGlobal = false;
                    answer = $"command \"{value}\" wasn't found!";
                    break;
            }
            
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)PacketType.ConsoleCommand);
            packet.Write(answer);
            packet.Write(clearField);
            packet.Write(showAnswer);
            
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
            JoinGame,
            CreateGame,
            UpdatePlayer,
            PawnOpen,
            PawnClose,
            PawnPass,
            ConsoleCommand,
        }
    }
}