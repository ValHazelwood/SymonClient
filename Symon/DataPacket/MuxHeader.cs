using System;
using System.Runtime.InteropServices;

namespace Symon
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MuxHeader
    {
        public uint CRC32;
        public uint reserved;
        public uint timestamp;
        public ushort packetLength;
        public byte version;
        
        public MuxHeader(bool notDefault)
        {
            this.CRC32 = 0;
            this.reserved = 0;
            this.timestamp = 0;
            this.packetLength = 0;
            this.version = (byte)MT_Values.SymonVersionVal;

        }
    }

}
