using System;

namespace MassStorageDotNet.Scsi
{
    [Flags]
    internal enum RequestSenseCdbFlags : byte
    {
        Desc = 1,
        DescriptorFormat = Desc,
    }
}
