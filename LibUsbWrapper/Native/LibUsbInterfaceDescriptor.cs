using System;
using System.Runtime.InteropServices;

namespace LibUsbWrapper.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct LibUsbInterfaceDescriptor
    {
        /** Size of this descriptor (in bytes) */
        public byte bLength;

        /** Descriptor type. Will have value
         * \ref libusb_descriptor_type::LIBUSB_DT_INTERFACE LIBUSB_DT_INTERFACE
         * in this context. */
        public byte bDescriptorType;

        /** Number of this interface */
        public byte bInterfaceNumber;

        /** Value used to select this alternate setting for this interface */
        public byte bAlternateSetting;

        /** Number of endpoints used by this interface (excluding the control
         * endpoint). */
        public byte bNumEndpoints;

        /** USB-IF class code for this interface. See \ref libusb_class_code. */
        public byte bInterfaceClass;

        /** USB-IF subclass code for this interface, qualified by the
         * bInterfaceClass value */
        public byte bInterfaceSubClass;

        /** USB-IF protocol code for this interface, qualified by the
         * bInterfaceClass and bInterfaceSubClass values */
        public byte bInterfaceProtocol;

        /** Index of string descriptor describing this interface */
        public byte iInterface;

        /** Array of endpoint descriptors. This length of this array is determined
         * by the bNumEndpoints field. */
        public IntPtr endpoint;

        /** Extra descriptors. If libusb encounters unknown interface descriptors,
         * it will store them here, should you wish to parse them. */
        public IntPtr extra;

        /** Length of the extra descriptors, in bytes. */
        public int extra_length;
    }
}
