using LibUsbWrapper.Native;

namespace LibUsbWrapper.Descriptors
{
    public sealed class DeviceDescriptor : IDescriptor
    {
        private readonly LibUsbDeviceDescriptor _descriptor;

        internal DeviceDescriptor(LibUsbDeviceDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public byte Length => _descriptor.bLength;

        public byte DescriptorType => _descriptor.bDescriptorType;

        public ushort BcdUSB => _descriptor.bcdUSB;

        public byte DeviceClass => _descriptor.bDeviceClass;

        public byte DeviceSubClass => _descriptor.bDeviceSubClass;

        public byte DeviceProtocol => _descriptor.bDeviceProtocol;

        public byte MaxPacketSize0 => _descriptor.bMaxPacketSize0;

        public ushort VendorID => _descriptor.idVendor;

        public ushort ProductID => _descriptor.idProduct;

        public ushort BcdDevice => _descriptor.bcdDevice;

        public byte ManufacturerStringIndex => _descriptor.iManufacturer;

        public byte ProductStringIndex => _descriptor.iProduct;

        public byte SerialNumberStringIndex => _descriptor.iSerialNumber;

        public byte NumConfigurations => _descriptor.bNumConfigurations;
    }
}
