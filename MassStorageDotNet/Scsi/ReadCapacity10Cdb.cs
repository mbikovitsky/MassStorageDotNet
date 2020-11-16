using System.Runtime.InteropServices;
using Utils;

namespace MassStorageDotNet.Scsi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal sealed class ReadCapacity10Cdb : ICdb
    {
        private readonly byte _operationCode = Endian.NativeToBig(0x25);

        private byte _flags1;

        private uint _logicalBlockAddress;

        private readonly ushort _reserved;

        private byte _flags2;

        private byte _control;

        static ReadCapacity10Cdb()
        {
            Native.EnsureMarshaledSize<ReadCapacity10Cdb>(10);
        }

        public byte OperationCode => Endian.BigToNative(_operationCode);

        public ReadCapacity10CdbFlags1 Flags1
        {
            get => (ReadCapacity10CdbFlags1) Endian.BigToNative(_flags1);
            set => _flags1 = Endian.NativeToBig((byte) value);
        }

        public uint LogicalBlockAddress
        {
            get => Endian.BigToNative(_logicalBlockAddress);
            set => _logicalBlockAddress = Endian.NativeToBig(value);
        }

        public ReadCapacity10CdbFlags2 Flags2
        {
            get => (ReadCapacity10CdbFlags2) Endian.BigToNative(_flags2);
            set => _flags2 = Endian.NativeToBig((byte) value);
        }

        public byte Control
        {
            get => Endian.BigToNative(_control);
            set => _control = Endian.NativeToBig(value);
        }
    }
}
