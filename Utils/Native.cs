using System;
using System.Runtime.InteropServices;

namespace Utils
{
    public static class Native
    {
        public static void EnsureMarshaledSize<T>(int size)
        {
            if (Marshal.SizeOf<T>() != size)
            {
                // TODO: better exception
                throw new Exception($"Marshaled size of {nameof(T)} is incorrect.");
            }
        }

        public static byte[] StructureToBytes<T>(T structure)
        {
            var bytes = new byte[Marshal.SizeOf<T>()];

            using (var pinned = new PinnedObject<byte[]>(bytes))
            {
                Marshal.StructureToPtr(structure, pinned.Address, false);
            }

            return bytes;
        }

        public static T BytesToStructure<T>(byte[] bytes)
        {
            Params.ValidateBuffer(bytes, 0, Marshal.SizeOf<T>());

            using (var pinned = new PinnedObject<byte[]>(bytes))
            {
                return Marshal.PtrToStructure<T>(pinned.Address);
            }
        }

        public static byte[] ReadBytes(IntPtr address, int size)
        {
            if (size == 0)
            {
                return Array.Empty<byte>();
            }

            var bytes = new byte[size];
            Marshal.Copy(address, bytes, 0, size);
            return bytes;
        }

        public static T[] ReadArray<T>(IntPtr address, int length)
        {
            if (length == 0)
            {
                return Array.Empty<T>();
            }

            var result = new T[length];
            for (int index = 0; index < length; index++)
            {
                result[index] = Marshal.PtrToStructure<T>(address + index * Marshal.SizeOf<T>());
            }

            return result;
        }
    }
}
