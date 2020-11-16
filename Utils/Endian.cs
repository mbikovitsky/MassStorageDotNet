using System;

namespace Utils
{
    public static class Endian
    {
        public static byte ByteSwap(byte value)
        {
            return value;
        }

        public static ushort ByteSwap(ushort value)
        {
            var lowByte = (ushort) (value & 0xFF);
            var highByte = (ushort) (value >> 8);

            return (ushort) ((lowByte << 8) | highByte);
        }

        public static uint ByteSwap(uint value)
        {
            return ((value & 0x000000FFu) << 24) |
                   ((value & 0x0000FF00u) << 8)  |
                   ((value & 0x00FF0000u) >> 8)  |
                   ((value & 0xFF000000u) >> 24);
        }

        public static ulong ByteSwap(ulong value)
        {
            return ((value & 0x00000000000000FFul) << 56) |
                   ((value & 0x000000000000FF00ul) << 40) |
                   ((value & 0x0000000000FF0000ul) << 24) |
                   ((value & 0x00000000FF000000ul) << 8)  |
                   ((value & 0x000000FF00000000ul) >> 8)  |
                   ((value & 0x0000FF0000000000ul) >> 24) |
                   ((value & 0x00FF000000000000ul) >> 40) |
                   ((value & 0xFF00000000000000ul) >> 56);
        }

        public static byte[] ByteSwap(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var result = (byte[]) value.Clone();
            Array.Reverse(result);
            return result;
        }

        public static byte NativeToBig(byte value)
        {
            return BitConverter.IsLittleEndian ? ByteSwap(value) : value;
        }

        public static ushort NativeToBig(ushort value)
        {
            return BitConverter.IsLittleEndian ? ByteSwap(value) : value;
        }

        public static uint NativeToBig(uint value)
        {
            return BitConverter.IsLittleEndian ? ByteSwap(value) : value;
        }

        public static ulong NativeToBig(ulong value)
        {
            return BitConverter.IsLittleEndian ? ByteSwap(value) : value;
        }

        public static byte[] NativeToBig(byte[] value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return (byte[]) (BitConverter.IsLittleEndian ? ByteSwap(value) : value.Clone());
        }

        public static byte NativeToLittle(byte value)
        {
            return BitConverter.IsLittleEndian ? value : ByteSwap(value);
        }

        public static ushort NativeToLittle(ushort value)
        {
            return BitConverter.IsLittleEndian ? value : ByteSwap(value);
        }

        public static uint NativeToLittle(uint value)
        {
            return BitConverter.IsLittleEndian ? value : ByteSwap(value);
        }

        public static ulong NativeToLittle(ulong value)
        {
            return BitConverter.IsLittleEndian ? value : ByteSwap(value);
        }

        public static byte[] NativeToLittle(byte[] value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return (byte[]) (BitConverter.IsLittleEndian ? value.Clone() : ByteSwap(value));
        }

        public static byte BigToNative(byte value)
        {
            return BitConverter.IsLittleEndian ? ByteSwap(value) : value;
        }

        public static ushort BigToNative(ushort value)
        {
            return BitConverter.IsLittleEndian ? ByteSwap(value) : value;
        }

        public static uint BigToNative(uint value)
        {
            return BitConverter.IsLittleEndian ? ByteSwap(value) : value;
        }

        public static ulong BigToNative(ulong value)
        {
            return BitConverter.IsLittleEndian ? ByteSwap(value) : value;
        }

        public static byte[] BigToNative(byte[] value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return (byte[]) (BitConverter.IsLittleEndian ? ByteSwap(value) : value.Clone());
        }

        public static byte LittleToNative(byte value)
        {
            return BitConverter.IsLittleEndian ? value : ByteSwap(value);
        }

        public static ushort LittleToNative(ushort value)
        {
            return BitConverter.IsLittleEndian ? value : ByteSwap(value);
        }

        public static uint LittleToNative(uint value)
        {
            return BitConverter.IsLittleEndian ? value : ByteSwap(value);
        }

        public static ulong LittleToNative(ulong value)
        {
            return BitConverter.IsLittleEndian ? value : ByteSwap(value);
        }

        public static byte[] LittleToNative(byte[] value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return (byte[]) (BitConverter.IsLittleEndian ? value.Clone() : ByteSwap(value));
        }
    }
}
