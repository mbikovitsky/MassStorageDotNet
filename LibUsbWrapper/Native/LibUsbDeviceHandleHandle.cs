using Microsoft.Win32.SafeHandles;

namespace LibUsbWrapper.Native
{
    internal sealed class LibUsbDeviceHandleHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public LibUsbDeviceHandleHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.libusb_close(DangerousGetHandle());
            return true;
        }
    }
}
