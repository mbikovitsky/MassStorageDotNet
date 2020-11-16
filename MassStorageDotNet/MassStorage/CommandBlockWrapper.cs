using System;
using System.Linq;
using System.Runtime.InteropServices;
using Utils;

namespace MassStorageDotNet.MassStorage
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class CommandBlockWrapper
    {
        private readonly uint dCBWSignature = Endian.NativeToLittle(0x43425355);

        private uint dCBWTag;

        private uint dCBWDataTransferLength;

        private byte bmCBWFlags;

        private byte bCBWLUN;

        private byte bCBWCBLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        private readonly byte[] CBWCB = new byte[16];

        static CommandBlockWrapper()
        {
            Native.EnsureMarshaledSize<CommandBlockWrapper>(31);
        }

        public uint Signature => Endian.LittleToNative(dCBWSignature);

        public uint Tag
        {
            get => Endian.LittleToNative(dCBWTag);
            set => dCBWTag = Endian.NativeToLittle(value);
        }

        public uint DataTransferLength
        {
            get => Endian.LittleToNative(dCBWDataTransferLength);
            set => dCBWDataTransferLength = Endian.NativeToLittle(value);
        }

        public CommandBlockWrapperFlags Flags
        {
            get => (CommandBlockWrapperFlags) Endian.LittleToNative(bmCBWFlags);
            set => bmCBWFlags = Endian.NativeToLittle((byte) value);
        }

        public byte LUN
        {
            get => Endian.LittleToNative(bCBWLUN);

            set
            {
                if (value > 15)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                bCBWLUN = Endian.NativeToLittle(value);
            }
        }

        public byte CommandBlockLength => Endian.LittleToNative(bCBWCBLength);

        public byte[] CommandBlock
        {
            get => CBWCB.Take(CommandBlockLength).ToArray();

            set
            {
                if (value.Length < 1 || value.Length > CBWCB.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                Array.Copy(value, CBWCB, value.Length);
                bCBWCBLength = Endian.NativeToLittle((byte) value.Length);
            }
        }
    }
}
