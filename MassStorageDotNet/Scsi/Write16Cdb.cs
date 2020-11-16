using System;
using System.Runtime.InteropServices;
using Utils;

namespace MassStorageDotNet.Scsi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal sealed class Write16Cdb : ICdb
    {
        private readonly byte _operationCode = Endian.NativeToBig(0x8A);

        private byte _flags;

        private ulong _logicalBlockAddress;

        private uint _transferLength;

        private byte _groupNumber;

        private byte _control;

        static Write16Cdb()
        {
            Native.EnsureMarshaledSize<Write16Cdb>(16);
        }

        public byte OperationCode => Endian.BigToNative(_operationCode);

        public byte Flags
        {
            get => Endian.BigToNative(_flags);
            set => _flags = Endian.NativeToBig(value);
        }

        public ulong LogicalBlockAddress
        {
            get => Endian.BigToNative(_logicalBlockAddress);
            set => _logicalBlockAddress = Endian.NativeToBig(value);
        }

        public uint TransferLength
        {
            get => Endian.BigToNative(_transferLength);
            set => _transferLength = Endian.NativeToBig(value);
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

        public byte Control
        {
            get => Endian.BigToNative(_control);
            set => _control = Endian.NativeToBig(value);
        }
    }
}
