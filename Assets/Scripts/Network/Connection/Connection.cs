using Network.UnityServer;
using Network.UnityServer.Handlers;
using Singleton;
using UnityEngine;

namespace Network.Connection
{
    public class Connection : Singleton<Connection>
    {
        [SerializeField] public UNetworkServerManager networkServerManager;

        public static Connection test;
        public UNetworkServerRulesHandler RulesHandler;
        public UNetworkServerDataHandler DataHandler;
        
        public ConnectionRules.GeneralRules GeneralRules;
        public ConnectionRules.InputRules InputRules;
        public ConnectionRules.OutputRules OutputRules;
        private void Awake()
        {
            test = Instance;
            networkServerManager.Server.RulesHandler.UpdateGeneralRules(new ConnectionRules.GeneralRules());
            networkServerManager.Server.RulesHandler.UpdateInputRules(new ConnectionRules.InputRules());
            networkServerManager.Server.RulesHandler.UpdateOutputRules(new ConnectionRules.OutputRules());
            
            RulesHandler = networkServerManager.Server.RulesHandler;
            DataHandler = networkServerManager.Server.DataHandler;
            
            GeneralRules = networkServerManager.Server.GeneralRules as ConnectionRules.GeneralRules;
            InputRules = networkServerManager.Server.InputRules as ConnectionRules.InputRules;
            OutputRules = networkServerManager.Server.OutputRules as ConnectionRules.OutputRules;
            
            RulesHandler.AddNewRule((ushort)ConnectionRules.PacketType.HandShake, InputRules!.HandShake); 
        }
    }
}