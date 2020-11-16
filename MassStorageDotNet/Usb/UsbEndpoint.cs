namespace MassStorageDotNet.Usb
{
    public abstract class UsbEndpoint : IUsbEndpoint
    {
        public abstract byte EndpointAddress { get; }

        public abstract byte Attributes { get; }

        public abstract ushort MaxPacketSize { get; }

        public abstract byte Interval { get; }

        public virtual EndpointDirection Direction => (EndpointDirection) (EndpointAddress & 0x80);

        public virtual byte EndpointNumber => (byte) (EndpointAddress & 0x0F);

        public virtual EndpointType Type => (EndpointType) (Attributes & 0x03);
    }
}
