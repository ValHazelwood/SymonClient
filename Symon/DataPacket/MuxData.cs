using System.Runtime.InteropServices;

namespace Symon
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MuxData
    {
        public MuxHeader header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] data;

        public MuxData(bool notDefault)
        {
            header = new MuxHeader();
            data = new byte[256];
        }
    }

}
