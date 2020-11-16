namespace MassStorageDotNet.Usb
{
#pragma warning disable CA1028 // Enum Storage should be Int32
    public enum EndpointType : byte
#pragma warning restore CA1028 // Enum Storage should be Int32
    {
        Control = 0x00,
        Isochronous = 0x01,
        Bulk = 0x02,
        Interrupt = 0x03,
    }
}
