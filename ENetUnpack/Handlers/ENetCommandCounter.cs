using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENetUnpack.Handlers
{
    public class ENetCommandCounter : ENetProtocolHandler
    {
        public Dictionary<ENetProtocolCommand, int> Counter = new Dictionary<ENetProtocolCommand, int>
        {
            [ENetProtocolCommand.NONE] = 0,
            [ENetProtocolCommand.ACKNOWLEDGE] = 0,
            [ENetProtocolCommand.CONNECT] = 0,
            [ENetProtocolCommand.VERIFY_CONNECT] = 0,
            [ENetProtocolCommand.DISCONNECT] = 0,
            [ENetProtocolCommand.PING] = 0,
            [ENetProtocolCommand.SEND_RELIABLE] = 0,
            [ENetProtocolCommand.SEND_UNRELIABLE] = 0,
            [ENetProtocolCommand.SEND_FRAGMENT] = 0,
            [ENetProtocolCommand.SEND_UNSEQUENCED] = 0,
            [ENetProtocolCommand.BANDWIDTH_LIMIT] = 0,
            [ENetProtocolCommand.THROTTLE_CONFIGURE] = 0,
        };
        public override bool HandleProtocol(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, ENetProtocol protocol)
        {
            Counter[protocolCommandHeader.Command]++;
            return base.HandleProtocol(protocolHeader, protocolCommandHeader, protocol);
        }
        public void PrintCounter()
        {
            foreach (var kvp in Counter)
            {
                Console.WriteLine($"{kvp.Key.ToString()} = {kvp.Value}");
            }
        }
    }
}
