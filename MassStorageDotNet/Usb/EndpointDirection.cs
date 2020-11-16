namespace MassStorageDotNet.Usb
{
#pragma warning disable CA1028 // Enum Storage should be Int32
    public enum EndpointDirection : byte
#pragma warning restore CA1028 // Enum Storage should be Int32
    {
        In = 0x80,
        DeviceToHost = In,

        Out = 0x00,
        HostToDevice = Out,
    }
}