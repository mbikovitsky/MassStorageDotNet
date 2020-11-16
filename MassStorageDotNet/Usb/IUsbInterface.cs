using System.Collections.Generic;

namespace MassStorageDotNet.Usb
{
    public interface IUsbInterface
    {
        byte AlternateSetting { get; }

        byte InterfaceNumber { get; }

        UsbClass InterfaceClass { get; }

        byte InterfaceSubClass { get; }

        byte InterfaceProtocol { get; }

        string Name { get; }

        IReadOnlyList<IUsbEndpoint> Endpoints { get; }
    }
}
