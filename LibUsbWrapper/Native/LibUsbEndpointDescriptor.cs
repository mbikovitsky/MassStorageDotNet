using System;
using System.Runtime.InteropServices;

namespace LibUsbWrapper.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct LibUsbEndpointDescriptor
    {
        /** Size of this descriptor (in bytes) */
        public byte bLength;

        /** Descriptor type. Will have value
         * \ref libusb_descriptor_type::LIBUSB_DT_ENDPOINT LIBUSB_DT_ENDPOINT in
         * this context. */
        public byte bDescriptorType;

        /** The address of the endpoint described by this descriptor. Bits 0:3 are
         * the endpoint number. Bits 4:6 are reserved. Bit 7 indicates direction,
         * see \ref libusb_endpoint_direction.
         */
        public byte bEndpointAddress;

        /** Attributes which apply to the endpoint when it is configured using
         * the bConfigurationValue. Bits 0:1 determine the transfer type and
         * correspond to \ref libusb_transfer_type. Bits 2:3 are only used for
         * isochronous endpoints and correspond to \ref libusb_iso_sync_type.
         * Bits 4:5 are also only used for isochronous endpoints and correspond to
         * \ref libusb_iso_usage_type. Bits 6:7 are reserved.
         */
        public byte bmAttributes;

        /** Maximum packet size this endpoint is capable of sending/receiving. */
        public ushort wMaxPacketSize;

        /** Interval for polling endpoint for data transfers. */
        public byte bInterval;

        /** For audio devices only: the rate at which synchronization feedback
         * is provided. */
        public byte bRefresh;

        /** For audio devices only: the address if the synch endpoint */
        public byte bSynchAddress;

        /** Extra descriptors. If libusb encounters unknown endpoint descriptors,
         * it will store them here, should you wish to parse them. */
        public IntPtr extra;

        /** Length of the extra descriptors, in bytes. */
        public int extra_length;
    }
}
