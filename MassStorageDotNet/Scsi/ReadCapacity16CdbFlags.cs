using System;

namespace MassStorageDotNet.Scsi
{
    [Flags]
    internal enum ReadCapacity16CdbFlags : byte
    {
        PMI = 1,
        PartialMediumIndicator = PMI,
    }
}