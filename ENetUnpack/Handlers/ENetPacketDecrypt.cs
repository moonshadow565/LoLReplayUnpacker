using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENetUnpack.Handlers
{
    public class ENetPacketDecrypt : IENetPacketAdder
    {
        protected BlowFish _blowfish;
        protected IENetPacketAdder _adder;
        public ENetPacketDecrypt(byte[] key)
        {
            _blowfish = new BlowFish(key);
            _adder = new ENetPacketAdderBase();
        }
        public ENetPacketDecrypt(byte[] key, IENetPacketAdder adder)
        {
            _blowfish = new BlowFish(key);
            _adder = adder;
        }
        public ENetPacketDecrypt(string base64key)
        {
            _blowfish = new BlowFish(Convert.FromBase64String(base64key));
            _adder = new ENetPacketAdderBase();
        }
        public ENetPacketDecrypt(string base64key, IENetPacketAdder adder)
        {
            _blowfish = new BlowFish(Convert.FromBase64String(base64key));
            _adder = adder;
        }

        public List<ENetPacket> Packets => _adder.Packets;

        public void AddPacket(byte channel, byte[] data, ENetPacketFlags flags, float time)
        {
            _adder.AddPacket(channel, channel > 7 ? data : _blowfish.Decrypt(data), flags, time);
        }
    }
}
