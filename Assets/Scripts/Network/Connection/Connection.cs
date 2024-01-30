using System.Collections.Generic;
using System.Globalization;
using Go;
using Network.Connection.Player;
using Network.Connection.Room;
using Network.UnityServer;
using Network.UnityTools;
using UnityEngine;

namespace Network.Connection
{
    public class Connection : UNetworkServer
    {
        [SerializeField] public ConnectionManager connectionManager;

        [SerializeField] private GlobalRoom[] previewRoom;
        
        public Stack<ushort> waitingPlayers = new Stack<ushort>();
        
        private void Awake()
        {
            if (dontDestroyOnLoad) DontDestroyOnLoad(this);
            if (startOnAwake) StartServer<GlobalRoom, GlobalPlayer>(this);
            //t
            previewRoom = new GlobalRoom[Rooms.Values.Count];
            foreach (var r in Rooms.Values)
            {
                previewRoom[r.Index] = r as GlobalRoom;
            }
            //t           
        }
        private void FixedUpdate() => UNetworkUpdate.Update();
        private void OnApplicationQuit() => CloseServer();
        protected override void OnCloseServer()
        {
            Debug.Log("OnCloseServer!");
        }
        protected override void OnStartServer()
        {
            Debug.Log("OnStartServer!");
            RulesHandler.AddRule((ushort)PacketType.HandShake, HandShake);
            RulesHandler.AddRule((ushort)PacketType.ConsoleCommand, ConsoleCommand);
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
                Clients[clientId].Disconnect();
            }
        }
        private void ConnectingPlayer(ushort clientId)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)PacketType.ConnectingPlayer);
            
            DataHandler.SendDataToAllExceptClientTcp(clientId, packet);
        }
        //t
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
                    if(keys[1].Equals("say"))
                    {
                        for(int i = 2; i < keys.Length; i++) answer += keys[i] + " ";
                    }
                    else if(keys[1].Equals("clear-board"))
                    {
                        GetClient<GlobalPlayer>(clientId).GetCurrentSession<GlobalRoom>().mainGame.Board.ClearBoard(clientId);
                        answer = "the board cleared!";
                    }
                    else if(keys[1].Equals("find-game"))
                    {
                        isGlobal = true;
             
                        if(!waitingPlayers.Contains(clientId)) waitingPlayers.Push(clientId);
                        else return;
                        
                        if(waitingPlayers.Count > 1)
                        {
                            foreach (var uNetworkRoom in Rooms.Values)
                            {
                                if (uNetworkRoom is GlobalRoom { IsOpened: false } room)
                                {
                                    room.Create(Instantiate(connectionManager.PrefabBoard, GameObject.FindWithTag("Table").transform).GetComponent<GoGame>());
                                    room.Enter(waitingPlayers);
                                    break;
                                }
                            }
                        }
                        answer = "the game found!";
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
        //t
        public enum PacketType : byte
        {
            HandShake,
            DisconnectingPlayer,
            ConnectingPlayer,
            StartGame,
            UpdatePlayer,
            PawnOpen,
            PawnClose,
            PawnPass,
            ConsoleCommand,
            UpdateListOfRooms
        }
    }
}