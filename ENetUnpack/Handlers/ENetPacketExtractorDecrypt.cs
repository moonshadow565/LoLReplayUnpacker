using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENetUnpack.Handlers
{
    public class ENetPacketExtractorDecrypt : ENetPacketExtractor
    {
        protected BlowFish _blowfish;
        public ENetPacketExtractorDecrypt(byte[] key)
        {
            _blowfish = new BlowFish(key);
        }
        public ENetPacketExtractorDecrypt(string base64key)
        {
            _blowfish = new BlowFish(Convert.FromBase64String(base64key));
        }
        public override void AddPacket(byte channel, byte[] data, ENetPacketFlags flags, float time)
        {
            base.AddPacket(channel, channel > 7 ? data : _blowfish.Decrypt(data), flags, time);
        }
    }
}
