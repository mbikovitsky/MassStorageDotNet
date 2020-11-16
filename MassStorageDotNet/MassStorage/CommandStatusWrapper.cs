using System.Runtime.InteropServices;
using Utils;

namespace MassStorageDotNet.MassStorage
{
#pragma warning disable CA1812
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class CommandStatusWrapper
    {
        private const uint ExpectedSignature = 0x53425355;

        private uint dCSWSignature;

        private uint dCSWTag;

        private uint dCSWDataResidue;

        private byte bCSWStatus;

        static CommandStatusWrapper()
        {
            Native.EnsureMarshaledSize<CommandStatusWrapper>(13);
        }

        public uint Signature
        {
            get => Endian.LittleToNative(dCSWSignature);
            set => dCSWSignature = Endian.NativeToLittle(value);
        }

        public uint Tag
        {
            get => Endian.LittleToNative(dCSWTag);
            set => dCSWTag = Endian.NativeToLittle(value);
        }

        public uint DataResidue
        {
            get => Endian.LittleToNative(dCSWDataResidue);
            set => dCSWDataResidue = Endian.NativeToLittle(value);
        }

        public CommandStatus Status
        {
            get => (CommandStatus) Endian.LittleToNative(bCSWStatus);
            set => bCSWStatus = Endian.NativeToLittle((byte) value);
        }

        public bool IsValid(uint expectedTag)
        {
            return Signature == ExpectedSignature && Tag == expectedTag;
        }

        public bool IsMeaningful(CommandBlockWrapper originalCbw)
        {
            switch (Status)
            {
                case CommandStatus.Success:
                case CommandStatus.Failure:
                    return DataResidue <= originalCbw.DataTransferLength;

                case CommandStatus.PhaseError:
                    return true;

                default:
                    return false;
            }
        }
    }
#pragma warning restore CA1812
}
