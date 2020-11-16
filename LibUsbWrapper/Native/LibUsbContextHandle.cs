using Microsoft.Win32.SafeHandles;

namespace LibUsbWrapper.Native
{
    internal sealed class LibUsbContextHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public LibUsbContextHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.libusb_exit(DangerousGetHandle());
            return true;
        }
    }
}
