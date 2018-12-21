using ENetUnpack.Handlers;
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
            var fileName = "test.json";
            if (args.Length > 0)
                fileName = args[0];
            var json = File.ReadAllText(fileName);
            var replay = JsonConvert.DeserializeObject<Replay>(json);
            var unbatcher = new ENetPacketUnbatcher();
            var decrypt = new ENetPacketDecrypt(replay.encryptionKey, unbatcher);
            var handler = new ENetPacketExtractor(decrypt);
            foreach(var rPacket  in replay.packets)
            {
                using (var reader = new BinaryReader(new MemoryStream(rPacket.Bytes)))
                {
                    handler.Read(reader, rPacket.Time, ENetLeagueVersion.Patch_4_20);
                }
            }      
            var json2 = JsonConvert.SerializeObject(handler.Packets, Formatting.Indented);
            File.WriteAllText(fileName.Replace(".json", ".unpacked.json"), json2);
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}
