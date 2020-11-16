using System;

namespace MassStorageDotNet.Scsi
{
    [Flags]
    internal enum ScsiSenseFixedFlags : byte
    {
        SdatOvfl = 1 << 4,
        SenseDataOverflow = SdatOvfl,

        Ili = 1 << 5,
        IncorrectLengthIndicator = Ili,

        Eom = 1 << 6,
        EndOfMedium = Eom,

        Filemark = 1 << 7,
    }
}
