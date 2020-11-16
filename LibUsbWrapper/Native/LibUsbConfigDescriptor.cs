using System;
using System.Runtime.InteropServices;

namespace LibUsbWrapper.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct LibUsbConfigDescriptor
    {
        /** Size of this descriptor (in bytes) */
        public byte bLength;

        /** Descriptor type. Will have value
         * \ref libusb_descriptor_type::LIBUSB_DT_CONFIG LIBUSB_DT_CONFIG
         * in this context. */
        public byte bDescriptorType;

        /** Total length of data returned for this configuration */
        public ushort wTotalLength;

        /** Number of interfaces supported by this configuration */
        public byte bNumInterfaces;

        /** Identifier value for this configuration */
        public byte bConfigurationValue;

        /** Index of string descriptor describing this configuration */
        public byte iConfiguration;

        /** Configuration characteristics */
        public byte bmAttributes;

        /** Maximum power consumption of the USB device from this bus in this
         * configuration when the device is fully operation. Expressed in units
         * of 2 mA when the device is operating in high-speed mode and in units
         * of 8 mA when the device is operating in super-speed mode. */
        public byte MaxPower;

        /** Array of interfaces supported by this configuration. The length of
         * this array is determined by the bNumInterfaces field. */
        public IntPtr @interface;

        /** Extra descriptors. If libusb encounters unknown configuration
         * descriptors, it will store them here, should you wish to parse them. */
        public IntPtr extra;

        /** Length of the extra descriptors, in bytes. */
        public int extra_length;
    }
}
