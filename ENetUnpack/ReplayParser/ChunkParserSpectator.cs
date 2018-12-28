using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENetUnpack.ReplayParser
{
    public class ChunkParserSpectator : HttpProtocolHandler, IChunkParser
    {
        private BlowFish _blowfish;
        private PacketAdder _packetAdder = new PacketAdder();

        public List<ENetPacket> Packets => _packetAdder.Packets;

        public ChunkParserSpectator(byte[] key)
        {
            _blowfish = new BlowFish(key);
        }

        public ChunkParserSpectator(string base64key)
        {
            _blowfish = new BlowFish(Convert.FromBase64String(base64key));
        }

        public void Read(byte[] data, float time)
        {
            base.Read(data, time);
        }
    }
}
