using System;
using System.Runtime.InteropServices;
using Utils;

namespace MassStorageDotNet.Scsi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal sealed class Read10Cdb : ICdb
    {
        private readonly byte _operationCode = Endian.NativeToBig(0x28);

        private byte _flags;

        private uint _logicalBlockAddress;

        private byte _groupNumber;

        private ushort _transferLength;

        private byte _control;

        static Read10Cdb()
        {
            Native.EnsureMarshaledSize<Read10Cdb>(10);
        }

        public byte OperationCode => Endian.BigToNative(_operationCode);

        public byte Flags
        {
            get => Endian.BigToNative(_flags);
            set => _flags = Endian.NativeToBig(value);
        }

        public uint LogicalBlockAddress
        {
            get => Endian.BigToNative(_logicalBlockAddress);
            set => _logicalBlockAddress = Endian.NativeToBig(value);
        }

        public byte GroupNumber
        {
            get => Endian.BigToNative(_groupNumber);

            set
            {
                if (value > 0b11111)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Group number too large");
                }

                _groupNumber = Endian.NativeToBig(value);
            }
        }

        public ushort TransferLength
        {
            get => Endian.BigToNative(_transferLength);
            set => _transferLength = Endian.NativeToBig(value);
        }

        public byte Control
        {
            get => Endian.BigToNative(_control);
            set => _control = Endian.NativeToBig(value);
        }
    }
}
