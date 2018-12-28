using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENetUnpack.ReplayParser
{
    // BDO starts for Black Desert online
    public static class BDODecompress
    {
        private struct Command
        {
            public int CommandSize { get; private set; }
            public int Distance { get; private set; }
            public int Length { get; private set; }
            public Command(byte[] input, int inputIndex)
            {
                uint raw = BitConverter.ToUInt32(input, inputIndex);
                switch(raw & 0x02)
                {
                    case 0:
                        Length = 3;
                        Distance = (int)((raw >> 2) & 0b0011_1111);
                        CommandSize = 1;
                        break;
                    case 1:
                        Length = 3;
                        Distance = (int)((raw >> 2) & 0b0011_1111_1111_1111);
                        CommandSize = 2;
                        break;
                    case 2:
                        Length = (int)((raw >> 2) & 01111) + 3;
                        Distance = (int)((raw >> 6) & 0b0011_1111_1111);
                        CommandSize = 2;
                        break;
                    default://case 3:
                        var length = (int)((raw >> 2) & 0b0001_1111);
                        if(length != 0)
                        {
                            Length = length + 2;
                            Distance = (int)((raw >> 7) & 0b0001_1111_1111_1111_1111);
                            CommandSize = 3;
                        }
                        else
                        {
                            Length = (int)((raw >> 7) & 0b1111_1111) + 3;
                            Distance = (int)((raw >> 15) & 0b0001_1111_1111_1111_1111);
                            CommandSize = 4;
                        }
                        break;
                }
            }
        }

        public static byte[] Decompress(byte[] input)
        {
            byte flags = input[0];
            int compressedSize = (flags & 0x2) != 0 ? BitConverter.ToInt32(input, 1) : input[1];
            int decompressedSize = (flags & 0x2) != 0 ? BitConverter.ToInt32(input, 5) : input[5];
            int inputIndex = (flags & 0x2) != 0 ? 9 : 3;
            int outputIndex = 0;

            if(compressedSize != input.Length)
            {
                throw new ArgumentOutOfRangeException("Compressed size doesn't match input size!");
            }

            var output = new byte[decompressedSize];

            if ((flags & 0x1) == 0)
            {
                Buffer.BlockCopy(input, inputIndex, output, outputIndex, decompressedSize);
                return output;
            }

            for (uint block = 1; inputIndex < compressedSize; block <<= 1)
            {
                if (block == 1)
                {
                    block = BitConverter.ToUInt32(input, inputIndex);
                    inputIndex += 4;
                }
                if ((block & 1) != 0)
                {
                    var command = new Command(input, inputIndex);
                    for (var i = 0; i < command.Length; i++)
                    {
                        output[outputIndex - command.Distance + i] = output[outputIndex + i];
                    }
                    inputIndex += command.CommandSize;
                    outputIndex += command.Length;
                }
                else
                {
                    output[outputIndex] = input[inputIndex];
                    inputIndex += 1;
                    outputIndex += 1;
                }
            }
            return output;
        }
    }
}
