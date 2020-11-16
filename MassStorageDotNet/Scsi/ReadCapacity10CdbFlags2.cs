using System;

namespace MassStorageDotNet.Scsi
{
    [Flags]
    internal enum ReadCapacity10CdbFlags2 : byte
    {
        PMI = 1,
        PartialMediumIndicator = PMI,
    }
}