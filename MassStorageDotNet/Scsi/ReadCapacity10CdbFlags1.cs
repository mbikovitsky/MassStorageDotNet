using System;

namespace MassStorageDotNet.Scsi
{
    [Flags]
    internal enum ReadCapacity10CdbFlags1 : byte
    {
        RELADR = 1,
        RelativeAddress = RELADR,
    }
}
