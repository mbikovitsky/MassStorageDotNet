using System;

namespace MassStorageDotNet.Scsi
{
    [Flags]
    internal enum ReadCapacity16CdbResultFlags : byte
    {
        PROT_EN = 1,

        RTO_EN = 1 << 1,
        ReferenceTagOwnEnable = RTO_EN,
    }
}
