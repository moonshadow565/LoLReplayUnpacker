using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENetUnpack
{
    class Program
    {
        static void Main(string[] args)
        {
            var packets = ReplayParser.Replay.ReadPackets(File.OpenRead("000000001.lrf"));
            var json = JsonConvert.SerializeObject(packets);
            File.WriteAllText("000000001.json", json);
        }
    }
}
