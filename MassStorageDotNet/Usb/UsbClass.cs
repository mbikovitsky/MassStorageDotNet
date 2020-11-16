namespace MassStorageDotNet.Usb
{
#pragma warning disable CA1028 // Enum Storage should be Int32
    public enum UsbClass : byte
#pragma warning restore CA1028 // Enum Storage should be Int32
    {
        Unspecified = 0x00,
        Audio = 0x01,
        Communications = 0x02,
        Hid = 0x03,
        Physical = 0x05,
        Image = 0x06,
        Printer = 0x07,
        MassStorage = 0x08,
        Hub = 0x09,
        CdcData = 0x0A,
        SmartCard = 0x0B,
        ContentSecurity = 0x0D,
        Video = 0x0E,
        PersonalHealthcare = 0x0F,
        AudioVideo = 0x10,
        Billboard = 0x11,
        TypeCBridge = 0x12,
        DiagnosticDevice = 0xDC,
        Wireless = 0xE0,
        Miscellaneous = 0xEF,
        Application = 0xFE,
        VendorSpec = 0xFF,
    }
}
