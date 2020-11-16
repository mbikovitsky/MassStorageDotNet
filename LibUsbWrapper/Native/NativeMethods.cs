using System;
using System.Runtime.InteropServices;

namespace LibUsbWrapper.Native
{
    internal static class NativeMethods
    {
        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern byte libusb_get_bus_number(LibUsbDeviceHandle dev);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern byte libusb_get_port_number(LibUsbDeviceHandle dev);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern int libusb_get_port_numbers(
            LibUsbDeviceHandle dev,
            [Out] byte[] portNumbers,
            int portNumbersLen);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern byte libusb_get_device_address(LibUsbDeviceHandle dev);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern UsbSpeed libusb_get_device_speed(LibUsbDeviceHandle dev);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern LibUsbError libusb_get_device_descriptor(
            LibUsbDeviceHandle dev,
            out LibUsbDeviceDescriptor desc);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern LibUsbError libusb_get_config_descriptor(
            LibUsbDeviceHandle dev,
            byte configIndex,
            out LibUsbConfigDescriptorHandle config);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern LibUsbError libusb_open(LibUsbDeviceHandle dev, out LibUsbDeviceHandleHandle devHandle);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr libusb_strerror(LibUsbError error);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern void libusb_free_config_descriptor(IntPtr config);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern void libusb_exit(IntPtr ctx);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern LibUsbDeviceHandle libusb_ref_device(IntPtr dev);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern void libusb_unref_device(IntPtr dev);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern void libusb_close(IntPtr devHandle);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern void libusb_free_device_list(
            IntPtr list,
            [MarshalAs(UnmanagedType.Bool)] bool unrefDevices);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern LibUsbError libusb_init(out LibUsbContextHandle context);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr libusb_get_device_list(LibUsbContextHandle ctx, out LibUsbDeviceListHandle list);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr libusb_get_device(LibUsbDeviceHandleHandle devHandle);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern int libusb_get_string_descriptor_ascii(
            LibUsbDeviceHandleHandle devHandle,
            byte descIndex,
            [Out] byte[] data,
            int length);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        private static extern int libusb_control_transfer(
            LibUsbDeviceHandleHandle devHandle,
            byte requestType,
            byte request,
            ushort value,
            ushort index,
            byte[] data,
            ushort length,
            uint timeout);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern int libusb_control_transfer(
            LibUsbDeviceHandleHandle devHandle,
            byte requestType,
            byte request,
            ushort value,
            ushort index,
            IntPtr data,
            ushort length,
            uint timeout);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern LibUsbError libusb_bulk_transfer(
            LibUsbDeviceHandleHandle devHandle,
            byte endpoint,
            IntPtr data,
            int length,
            out int transferred,
            uint timeout);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern LibUsbError libusb_set_configuration(
            LibUsbDeviceHandleHandle devHandle,
            int configuration);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern LibUsbError libusb_claim_interface(
            LibUsbDeviceHandleHandle devHandle,
            byte interfaceNumber);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern LibUsbError libusb_release_interface(
            LibUsbDeviceHandleHandle devHandle,
            byte interfaceNumber);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern LibUsbError libusb_clear_halt(
            LibUsbDeviceHandleHandle devHandle,
            byte endpoint);

        [DllImport(Configuration.LibUsbDll, CallingConvention = CallingConvention.Winapi)]
        public static extern LibUsbError libusb_set_interface_alt_setting(
            LibUsbDeviceHandleHandle devHandle,
            int interfaceNumber, int alternateSetting);
    }
}
