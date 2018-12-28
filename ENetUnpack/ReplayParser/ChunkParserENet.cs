using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENetUnpack.ReplayParser
{
    public class ChunkParserENet : ENetProtocolHandler, IChunkParser
    {
        private ENetLeagueVersion _enetLeagueVersion;
        private BlowFish _blowfish;
        private PacketAdder _packetAdder = new PacketAdder();

        public List<ENetPacket> Packets => _packetAdder.Packets;

        public ChunkParserENet(ENetLeagueVersion eNetLeagueVersion, byte[] key)
        {
            _enetLeagueVersion = eNetLeagueVersion;
            _blowfish = new BlowFish(key);
        }

        public ChunkParserENet(ENetLeagueVersion eNetLeagueVersion, string base64key)
        {
            _enetLeagueVersion = eNetLeagueVersion;
            _blowfish = new BlowFish(Convert.FromBase64String(base64key));
        }

        public void AddPacket(byte[] data, float time, byte channel, ENetPacketFlags flags)
        {
            if(channel > 7)
            {
                _packetAdder.AddPacket(data, time, channel, flags);
            }
            else
            {
                _packetAdder.AddPacket(_blowfish.Decrypt(data), time, channel, flags);
            }
        }

        public override bool HandleProtocol(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, ENetProtocol protocol)
        {
            dynamic dinProtocol = protocol;
            return Handle(dinProtocol, protocolHeader, protocolCommandHeader);
        }

        public bool Handle(ENetProtocol command, ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader commandHeader)
        {
            return true;
        }

        public bool Handle(ENetProtocolSendReliable command, ENetProtocolHeader protocolHeader,ENetProtocolCommandHeader commandHeader)
        {
            AddPacket(command.Data, protocolHeader.TimeRecieved, commandHeader.Channel, ENetPacketFlags.Reliable);
            return true;
        }

        public bool Handle(ENetProtocolSendUnsequenced command, ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader commandHeader)
        {
            AddPacket(command.Data, protocolHeader.TimeRecieved, commandHeader.Channel, ENetPacketFlags.Unsequenced);
            return true;
        }

        public bool Handle(ENetProtocolSendUnreliable command, ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader commandHeader)
        {
            AddPacket(command.Data, protocolHeader.TimeRecieved, commandHeader.Channel, ENetPacketFlags.None);
            return true;
        }

        protected class FragmentBuffer
        {
            public int nextReliableSequenceNumber = 0;
            public int FragmentCount = 0;
            public int FragmentsLeft = 0;
            public byte[] Buffer = new byte[0];
        }

        private static Dictionary<byte, Dictionary<ushort, FragmentBuffer>> MakeChannelBuffers()
        {
            var tmp = new Dictionary<byte, Dictionary<ushort, FragmentBuffer>>();
            for (int i = 0; i < 255; i++)
            {
                tmp[(byte)i] = new Dictionary<ushort, FragmentBuffer>();
            }
            return tmp;
        }
        
        protected Dictionary<byte, Dictionary<ushort, FragmentBuffer>> ChannelFragmentBuffer = MakeChannelBuffers();

        public bool Handle(ENetProtocolSendFragment command, ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader commandHeader)
        {
            if (command.FragmentOffset >= command.TotalLength ||
                command.FragmentOffset + command.Data.Length > command.TotalLength ||
                command.FragmentNumber >= command.FragmentCount)
            {
                return false;
            }

            var channel = ChannelFragmentBuffer[commandHeader.Channel];
            FragmentBuffer buffer;
            if (channel.ContainsKey(command.StartSequenceNumber))
            {
                buffer = channel[command.StartSequenceNumber];
            }
            else
            {
                if (command.StartSequenceNumber != commandHeader.ReliableSequenceNumber)
                {
                    return true;
                }
                buffer = new FragmentBuffer();
                buffer.Buffer = new byte[command.TotalLength];
                buffer.FragmentCount = (int)command.FragmentCount;
                buffer.nextReliableSequenceNumber = commandHeader.ReliableSequenceNumber;
                buffer.FragmentsLeft = (int)command.FragmentCount;
                channel[command.StartSequenceNumber] = buffer;
            }
            if (buffer.nextReliableSequenceNumber != commandHeader.ReliableSequenceNumber)
            {
                return true;
            }
            if (buffer.FragmentCount != command.FragmentCount)
            {
                return false;
            }
            if (buffer.Buffer.Length != command.TotalLength)
            {
                return false;
            }

            buffer.nextReliableSequenceNumber++;
            buffer.FragmentsLeft--;

            Buffer.BlockCopy(command.Data, 0, buffer.Buffer, (int)command.FragmentOffset, command.Data.Length);
            if (buffer.FragmentsLeft <= 0)
            {
                AddPacket(buffer.Buffer, protocolHeader.TimeRecieved, commandHeader.Channel, ENetPacketFlags.Reliable);
                channel.Remove(command.StartSequenceNumber);
            }
            return true;
        }

        public void Read(byte[] data, float time)
        {
            using(var reader = new BinaryReader(new MemoryStream(data)))
            {
                Read(reader, time, _enetLeagueVersion);
            }
        }
    }
}
