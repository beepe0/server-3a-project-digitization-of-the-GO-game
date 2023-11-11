using System;
using Network.Connection;
using UnityEngine;

namespace DebugCON
{
    public class ConnectionCON : MonoBehaviour
    {
#if !UNITY_EDITOR
        public Connection connection;

        private string _ip = "";
        private string _port = "";
      
        private void ConnectionPanel()
        {
            Console.Write("Internet Protocol: ");
            _ip = Console.ReadLine();
            Console.Write("Port: ");
            _port = Console.ReadLine();
            connection.serverInternetProtocol = _ip;
            connection.serverPort = ushort.Parse(_port);
            connection.StartServer();
        }
        private void Awake()
        {
            Console.Clear();
            ConnectionPanel();
        }
#endif
    }
}