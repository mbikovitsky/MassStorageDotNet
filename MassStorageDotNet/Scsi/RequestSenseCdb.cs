using System.Runtime.InteropServices;
using Utils;

namespace MassStorageDotNet.Scsi
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal sealed class RequestSenseCdb : ICdb
    {
        private readonly byte _operationCode = Endian.NativeToBig(0x03);

        private byte _flags;

        private readonly ushort _reserved = 0;

        private byte _allocationLength;

        private byte _control;

        static RequestSenseCdb()
        {
            Native.EnsureMarshaledSize<RequestSenseCdb>(6);
        }

        public byte OperationCode => Endian.BigToNative(_operationCode);

        public RequestSenseCdbFlags Flags
        {
            get => (RequestSenseCdbFlags) Endian.BigToNative(_flags);
            set => _flags = Endian.NativeToBig((byte) value);
        }

        public byte AllocationLength
        {
            get => Endian.BigToNative(_allocationLength);
            set => _allocationLength = Endian.NativeToBig(value);
        }

        public byte Control
        {
            get => Endian.BigToNative(_control);
            set => _control = Endian.NativeToBig(value);
        }
    }
}
