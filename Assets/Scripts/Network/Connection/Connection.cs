using Network.UServer;
using Singleton;
using UnityEngine;

namespace Network.Connection
{
    public class Connection : Singleton<Connection>
    {
        private void Awake()
        {
            UNetworkCore.GeneralRules = new ConnectionRules.GeneralRules();
            UNetworkCore.InputRules = new ConnectionRules.InputRules();
            UNetworkCore.OutputRules = new ConnectionRules.OutputRules();
            
            UNetworkCore.RulesHandler.AddNewRule((ushort)ConnectionRules.PacketType.OnWelcome, UNetworkCore.InputRules.OnWelcome); 
            UNetworkCore.RulesHandler.AddNewRule((ushort)ConnectionRules.PacketType.SynchronizePosition, ((ConnectionRules.InputRules)UNetworkCore.InputRules).SynchronizePosition); 
        }
    }
}