using Microsoft.Win32.SafeHandles;

namespace LibUsbWrapper.Native
{
    internal sealed class LibUsbConfigDescriptorHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public LibUsbConfigDescriptorHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.libusb_free_config_descriptor(DangerousGetHandle());
            return true;
        }
    }
}
