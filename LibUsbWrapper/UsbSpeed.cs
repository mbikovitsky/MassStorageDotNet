namespace LibUsbWrapper
{
    // ReSharper disable once EnumUnderlyingTypeIsInt
    public enum UsbSpeed : int
    {
        /** The OS doesn't report or know the device speed. */
        Unknown = 0,

        /** The device is operating at low speed (1.5MBit/s). */
        Low = 1,

        /** The device is operating at full speed (12MBit/s). */
        Full = 2,

        /** The device is operating at high speed (480MBit/s). */
        High = 3,

        /** The device is operating at super speed (5000MBit/s). */
        Super = 4,

        /** The device is operating at super speed plus (10000MBit/s). */
        SuperPlus = 5,
    }
}
