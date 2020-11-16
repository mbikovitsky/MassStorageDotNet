using System.Runtime.InteropServices;
using Utils;

namespace MassStorageDotNet.Scsi
{
#pragma warning disable CA1812
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal sealed class ReadCapacity16CdbResult
    {
        private ulong _logicalBlockAddress;

        private uint _blockLength;

        private byte _flags;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
        private readonly byte[] _reserved = new byte[19];

        static ReadCapacity16CdbResult()
        {
            Native.EnsureMarshaledSize<ReadCapacity16CdbResult>(32);
        }

        public ulong LogicalBlockAddress
        {
            get => Endian.BigToNative(_logicalBlockAddress);
            set => _logicalBlockAddress = Endian.NativeToBig(value);
        }

        public uint BlockLength
        {
            get => Endian.BigToNative(_blockLength);
            set => _blockLength = Endian.NativeToBig(value);
        }

        public ReadCapacity16CdbResultFlags Flags
        {
            get => (ReadCapacity16CdbResultFlags) Endian.BigToNative(_flags);
            set => _flags = Endian.NativeToBig((byte) value);
        }
    }
#pragma warning restore CA1812
}
