using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENetUnpack
{
    public class Packet
    {
        public float Time { get; set; }
        public int Length { get; set; }
        public byte[] Bytes { get; set; }
    }
    public class Player
    {
        public string name { get; set; }
        public string champion { get; set; }
        public int team { get; set; }
    }
    public class Replay
    {
        public string replayName { get; set; }
        public int accountId { get; set; }
        public List<Player> players { get; set; }
        public string serverAddress { get; set; }
        public int serverPort { get; set; }
        public string encryptionKey { get; set; }
        public string clientVersion { get; set; }
        public List<Packet> packets { get; set; }
    }
}
