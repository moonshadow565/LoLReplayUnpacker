using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENetUnpack.ReplayParser
{
    public class PacketAdder
    {
        public List<ENetPacket> Packets { get; } = new List<ENetPacket>();

        public void AddPacket(byte[] data, float time, byte channel, ENetPacketFlags flags)
        {
            if (data[0] != 0xFF)
            {
                Packets.Add(new ENetPacket
                {
                    Channel = channel,
                    Bytes = data,
                    Flags = flags,
                    Time = time,
                });
            }
            else
            {
                using (var reader = new BinaryReader(new MemoryStream(data)))
                {
                    Ubatch(channel, reader, flags, time);
                }
            }

        }

        private void Ubatch(byte channel, BinaryReader reader, ENetPacketFlags flags, float time)
        {
            reader.ReadByte();
            int count = reader.ReadByte();
            if ((reader.BaseStream.Length) < 3 || count == 0)
            {
                return;
            }
            byte packetSize = 0;
            byte packetLastID = 0;
            int packetLastNetID = 0;
            byte[] packetData = null;
            for (int i = 0; i < count; i++)
            {
                packetSize = reader.ReadByte();
                if (i == 0)
                {
                    packetLastID = reader.ReadByte();
                    packetLastNetID = reader.ReadInt32();
                    packetData = reader.ReadBytes(packetSize - 5);
                }
                else
                {
                    if ((packetSize & 1) == 0) //if this is true re-use old packetID
                    {
                        packetLastID = reader.ReadByte();
                    }
                    if ((packetSize & 2) == 0)
                    {
                        packetLastNetID = reader.ReadInt32();
                    }
                    else
                    {
                        packetLastNetID += reader.ReadSByte();
                    }
                    if ((packetSize >> 2) == 0x3F)
                    {
                        packetSize = reader.ReadByte();
                    }
                    else
                    {
                        packetSize = (byte)(packetSize >> 2);
                    }
                    packetData = reader.ReadBytes(packetSize);
                }
                using (var stream = new MemoryStream())
                {
                    using (var packetWriter = new BinaryWriter(stream, Encoding.UTF8, true))
                    {
                        packetWriter.Write(packetLastID);
                        packetWriter.Write(packetLastNetID);
                        packetWriter.Write(packetData);
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var packetReader = new BinaryReader(stream, Encoding.UTF8, true))
                    {
                        Packets.Add(new ENetPacket
                        {
                            Channel = channel,
                            Bytes = packetReader.ReadBytes((int)stream.Length),
                            Flags = flags,
                            Time = time,
                        });
                    }
                }
            }
        }
    }
}
