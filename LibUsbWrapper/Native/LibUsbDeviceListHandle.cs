using Microsoft.Win32.SafeHandles;

namespace LibUsbWrapper.Native
{
    internal sealed class LibUsbDeviceListHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public LibUsbDeviceListHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.libusb_free_device_list(DangerousGetHandle(), true);
            return true;
        }
    }
}
