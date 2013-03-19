using System;

namespace Symon
{
    public class Crc
    {
        const uint polynomial = 0x04c11db7;
        uint[] table = new uint[256];

        public uint ComputeChecksum(byte[] bytes)
        {
            int index;
            uint crc;
            int len = bytes.Length;

            crc = 0xffffffff;

            for (index = 0; len > 0; ++index, --len)
            {
                crc = (crc << 8) ^ table[(crc >> 24) ^ bytes[index]];
            }

            return ~crc;
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            uint crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        public Crc()
        {
            uint value;

            for (uint i = 0; i < table.Length; ++i)
            {
                value = i << 24;

                for (uint j = 8; j > 0; --j)
                {
                    if ((value & 0x80000000) != 0)
                    {
                        value = (value << 1) ^ polynomial;
                    }
                    else
                    {
                        value = (value << 1);
                    }
                }

                table[i] = value;
            }
        }
    }

}
