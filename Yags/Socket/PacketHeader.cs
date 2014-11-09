using System.Runtime.InteropServices;

namespace Yags.Socket
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PacketHeader
    {
        public static int Size;
        public uint Version;
        public uint DataLength;
    }
}
