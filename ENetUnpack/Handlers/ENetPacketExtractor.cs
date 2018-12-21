using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENetUnpack.Handlers
{
    public interface IENetPacketAdder
    {
        void AddPacket(byte channel, byte[] data, ENetPacketFlags flags, float time);
        List<ENetPacket> Packets { get; }
    }

    public class ENetPacketExtractor : ENetProtocolHandler, IENetPacketAdder
    {
        public List<ENetPacket> Packets => _adder.Packets;
        private IENetPacketAdder _adder;

        public ENetPacketExtractor()
        {
            _adder = new ENetPacketAdderBase();
        }
        
        public ENetPacketExtractor(IENetPacketAdder adder)
        {
            _adder = adder;
        }

        public void AddPacket(byte channel, byte[] data, ENetPacketFlags flags, float time)
        {
            _adder.AddPacket(channel, data, flags, time);
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
            AddPacket(commandHeader.Channel, command.Data, ENetPacketFlags.Reliable, protocolHeader.TimeRecieved);
            return true;
        }

        public bool Handle(ENetProtocolSendUnsequenced command, ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader commandHeader)
        {
            AddPacket(commandHeader.Channel, command.Data, ENetPacketFlags.Unsequenced, protocolHeader.TimeRecieved);
            return true;
        }

        public bool Handle(ENetProtocolSendUnreliable command, ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader commandHeader)
        {
            AddPacket(commandHeader.Channel, command.Data, ENetPacketFlags.None, protocolHeader.TimeRecieved);
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
                AddPacket(commandHeader.Channel, buffer.Buffer, ENetPacketFlags.Reliable, protocolHeader.TimeRecieved);
                channel.Remove(command.StartSequenceNumber);
            }
            return true;
        }
    }
}
