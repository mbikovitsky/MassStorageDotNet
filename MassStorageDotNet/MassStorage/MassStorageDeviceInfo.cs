using MassStorageDotNet.Usb;

namespace MassStorageDotNet.MassStorage
{
    internal struct MassStorageDeviceInfo
    {
        public IUsbConfiguration Configuration;
        public IUsbInterface Interface;
        public IUsbEndpoint BulkInEndpoint;
        public IUsbEndpoint BulkOutEndpoint;
    }
}
