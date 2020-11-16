namespace MassStorageDotNet.Usb
{
    public interface IUsbEndpoint
    {
        byte EndpointAddress { get; }

        byte Attributes { get; }

        ushort MaxPacketSize { get; }

        byte Interval { get; }

        EndpointDirection Direction { get; }

        byte EndpointNumber { get; }

        EndpointType Type { get; }
    }
}
