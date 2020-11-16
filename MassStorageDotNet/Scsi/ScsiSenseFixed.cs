using System.Linq;
using System.Runtime.InteropServices;
using Utils;

namespace MassStorageDotNet.Scsi
{
#pragma warning disable CA1812
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal sealed class ScsiSenseFixed
    {
        private readonly byte _responseCode;

        private readonly byte _obsolete;

        private readonly byte _flags;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private readonly byte[] _information = new byte[4];

        private readonly byte _additionalSenseLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private readonly byte[] _commandSpecificInformation = new byte[4];

        private readonly byte _additionalSenseCode;

        private readonly byte _additionalSenseCodeQualifier;

        private readonly byte _fieldReplaceableUnitCode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        private readonly byte[] _senseKeySpecificInformation = new byte[3];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 234)]
        private readonly byte[] _additionalSenseBytes = new byte[234];

        static ScsiSenseFixed()
        {
            Native.EnsureMarshaledSize<ScsiSenseFixed>(252);
        }

        public byte ResponseCode => (byte) (Endian.BigToNative(_responseCode) & 0x7f);

        public bool Valid => (Endian.BigToNative(_responseCode) & 0x80) != 0;

        public byte SenseKey => (byte) (Endian.BigToNative(_flags) & 0x0F);

        public ScsiSenseFixedFlags Flags => (ScsiSenseFixedFlags) (Endian.BigToNative(_flags) & 0xF0);

        public byte[] Information => Endian.BigToNative(_information);

        public byte[] CommandSpecificInformation => Endian.BigToNative(_commandSpecificInformation);

        public byte AdditionalSenseCode => Endian.BigToNative(_additionalSenseCode);

        public byte AdditionalSenseCodeQualifier => Endian.BigToNative(_additionalSenseCodeQualifier);

        public byte FieldReplaceableUnitCode => Endian.BigToNative(_fieldReplaceableUnitCode);

        public byte[] SenseKeySpecificInformation => (byte[]) _senseKeySpecificInformation.Clone();

        public byte[] AdditionalSenseBytes => _additionalSenseBytes.Take(AdditionalSenseLength - 10).ToArray();

        private byte AdditionalSenseLength => Endian.BigToNative(_additionalSenseLength);
    }
#pragma warning restore CA1812
}
