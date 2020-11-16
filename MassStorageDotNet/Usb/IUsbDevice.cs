using System.Collections.Generic;

namespace MassStorageDotNet.Usb
{
    public interface IUsbDevice
    {
        UsbClass DeviceClass { get; }

        byte DeviceSubClass { get; }

        byte DeviceProtocol { get; }

        ushort VendorId { get; }

        ushort ProductId { get; }

        string ProductName { get; }

        string SerialNumber { get; }

        string Manufacturer { get; }

        IReadOnlyList<IUsbConfiguration> Configurations { get; }
    }
}
