using System;

namespace MassStorageDotNet.MassStorage
{
    [Flags]
    internal enum CommandBlockWrapperFlags : byte
    {
        DataOut = 0,
        HostToDevice = DataOut,

        DataIn = 1 << 7,
        DeviceToHost = DataIn,
    }
}
