using ENetUnpack.ReplayParser;
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
            var filename = "2715367.lrf";
            ENetLeagueVersion? version = null;
            if(args.Length > 0)
            {
                filename = args[0];
            }
            if(args.Length > 1)
            {
                version = (ENetLeagueVersion)Enum.Parse(typeof(ENetLeagueVersion), args[1]);
            }
            if(!filename.EndsWith(".lrf"))
            {
                Console.Error.WriteLine("Filename should end with .lrf!");
            }

            var packets = Replay.ReadPackets(File.OpenRead(filename), version);
            var json = JsonConvert.SerializeObject(packets, Formatting.Indented);
            File.WriteAllText(filename.Replace(".lrf", ".rlp.json"), json);
        }
    }
}
