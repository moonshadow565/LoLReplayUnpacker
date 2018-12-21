using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENetUnpack.Handlers
{
    public class ENetPacketAdderBase : IENetPacketAdder
    {
        public List<ENetPacket> Packets { get; set; } = new List<ENetPacket>();

        public void AddPacket(byte channel, byte[] data, ENetPacketFlags flags, float time)
        {
            Packets.Add(new ENetPacket
            {
                Channel = channel,
                Bytes = data,
                Flags = flags,
                Time = time,
            });
        }
    }
}
