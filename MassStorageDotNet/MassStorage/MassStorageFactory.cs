using System;
using System.Collections.Generic;
using System.Linq;
using MassStorageDotNet.Usb;

namespace MassStorageDotNet.MassStorage
{
    public sealed class MassStorageFactory
    {
        private readonly IUsbManager _manager;

        public MassStorageFactory(IUsbManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public MassStorageDevice Create(IUsbDevice device, bool writable)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            var massStorageDeviceInfo = ParseMassStorageDevice(device);

            if (!massStorageDeviceInfo.HasValue)
            {
                throw new ArgumentException("Supplied USB device is not supported", nameof(device));
            }

            return MassStorageDevice.Create(_manager.OpenDevice(device), massStorageDeviceInfo.Value, writable);
        }

        public IEnumerable<IUsbDevice> GetSupportedDevices()
        {
            return _manager.GetDevices().Where(IsSupportedMassStorageDevice).ToArray();
        }

        private static bool IsSupportedMassStorageDevice(IUsbDevice device)
        {
            return ParseMassStorageDevice(device).HasValue;
        }

        private static MassStorageDeviceInfo? ParseMassStorageDevice(IUsbDevice device)
        {
            // See the Universal Serial Bus Mass Storage Class Bulk-Only Transport spec.,
            // section 4.1 Device Descriptor.
            if (device.DeviceClass != UsbClass.Unspecified || device.DeviceSubClass != 0 || device.DeviceProtocol != 0)
            {
                return null;
            }

            // The spec. is kinda vague here, but it looks like we
            // should only check the default configuration descriptor.
            // Let's assume it's the one with ID == 1.
            var configuration = device.Configurations.SingleOrDefault(usbConfiguration => usbConfiguration.Id == 1);
            if (null == configuration)
            {
                return null;
            }

            // There should be at least one interface.
            var @interface = configuration.Interfaces.FirstOrDefault(IsSupportedInterface);
            if (null == @interface)
            {
                return null;
            }

            // There should be at least one bulk-in endpoint.
            var bulkInEndpoint = FindEndpoint(@interface, EndpointDirection.In);
            if (null == bulkInEndpoint)
            {
                return null;
            }

            // There should be at least one bulk-out endpoint.
            var bulkOutEndpoint = FindEndpoint(@interface, EndpointDirection.Out);
            if (null == bulkOutEndpoint)
            {
                return null;
            }

            return new MassStorageDeviceInfo
            {
                Configuration = configuration,
                Interface = @interface,
                BulkInEndpoint = bulkInEndpoint,
                BulkOutEndpoint = bulkOutEndpoint,
            };
        }

        private static bool IsSupportedInterface(IUsbInterface usbInterface)
        {
            // Currently we only support the SCSI transparent command set (InterfaceSubClass == 0x06)
            // and the Bulk-Only transport (InterfaceProtocol == 0x50).
            return usbInterface.InterfaceClass == UsbClass.MassStorage &&
                   usbInterface.InterfaceSubClass == 0x06 &&
                   usbInterface.InterfaceProtocol == 0x50;
        }

        private static IUsbEndpoint FindEndpoint(IUsbInterface @interface, EndpointDirection direction)
        {
            return @interface.Endpoints.FirstOrDefault(endpoint =>
                endpoint.Type == EndpointType.Bulk && endpoint.Direction == direction);
        }
    }
}
