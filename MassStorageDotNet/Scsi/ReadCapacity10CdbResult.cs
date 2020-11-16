using System.Runtime.InteropServices;
using Utils;

namespace MassStorageDotNet.Scsi
{
#pragma warning disable CA1812
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal sealed class ReadCapacity10CdbResult
    {
        private readonly uint _logicalBlockAddress;

        private readonly uint _blockLength;

        static ReadCapacity10CdbResult()
        {
            Native.EnsureMarshaledSize<ReadCapacity10CdbResult>(8);
        }

        public uint LogicalBlockAddress => Endian.BigToNative(_logicalBlockAddress);

        public uint BlockLength => Endian.BigToNative(_blockLength);
    }
#pragma warning restore CA1812
}
