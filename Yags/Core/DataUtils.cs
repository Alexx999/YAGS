using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Yags.Core
{
    static class DataUtils
    {
        public static T BytesToStruct<T>(byte[] bytes, int startPos) where T : struct
        {
            Debug.Assert(bytes != null);
            Debug.Assert(bytes.Length >= Marshal.SizeOf(typeof(T)) + startPos);
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject() + startPos, typeof (T));
            handle.Free();
            return stuff;
        }

        public static T BytesToStruct<T>(byte[] bytes) where T : struct
        {
            return BytesToStruct<T>(bytes, 0);
        }

        public static void StructToBytes<T>(T value, byte[] bytes, int startPos) where T : struct
        {
            Debug.Assert(bytes != null);
            Debug.Assert(bytes.Length >= Marshal.SizeOf(typeof(T)) + startPos);
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject() + startPos, false);
            handle.Free();
        }

        public static void StructToBytes<T>(T value, byte[] bytes) where T : struct
        {
            StructToBytes(value, bytes, 0);
        }
    }
}
