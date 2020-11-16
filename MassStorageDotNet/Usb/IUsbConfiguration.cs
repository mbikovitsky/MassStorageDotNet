using System.Collections.Generic;

namespace MassStorageDotNet.Usb
{
    public interface IUsbConfiguration
    {
        byte Id { get; }

        byte MaxPower { get; }

        string Name { get; }

        bool IsRemoteWakeup { get; }

        bool IsSelfPowered { get; }

        IReadOnlyList<IUsbInterface> Interfaces { get; }
    }
}
