using System.Runtime.InteropServices;
using Utils;

namespace MassStorageDotNet.Scsi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal sealed class ReadCapacity16Cdb : ICdb
    {
        private readonly byte _operationCode = Endian.NativeToBig(0x9E);

        private readonly byte _serviceAction = Endian.NativeToBig(0x10);

        private ulong _logicalBlockAddress;

        private uint _allocationLength;

        private byte _flags;

        private byte _control;

        static ReadCapacity16Cdb()
        {
            Native.EnsureMarshaledSize<ReadCapacity16Cdb>(16);
        }

        public byte OperationCode => Endian.BigToNative(_operationCode);

        public byte ServiceAction => Endian.BigToNative(_serviceAction);

        public ulong LogicalBlockAddress
        {
            get => Endian.BigToNative(_logicalBlockAddress);
            set => _logicalBlockAddress = Endian.NativeToBig(value);
        }

        public uint AllocationLength
        {
            get => Endian.BigToNative(_allocationLength);
            set => _allocationLength = Endian.NativeToBig(value);
        }

        public ReadCapacity16CdbFlags Flags
        {
            get => (ReadCapacity16CdbFlags) Endian.BigToNative(_flags);
            set => _flags = Endian.NativeToBig((byte) value);
        }

        public byte Control
        {
            get => Endian.BigToNative(_control);
            set => _control = Endian.NativeToBig(value);
        }
    }
}
