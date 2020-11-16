using System;
using Microsoft.Win32.SafeHandles;

namespace LibUsbWrapper.Native
{
    internal sealed class LibUsbDeviceHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public LibUsbDeviceHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.libusb_unref_device(DangerousGetHandle());
            return true;
        }

        internal static LibUsbDeviceHandle RefDevice(IntPtr rawHandle)
        {
            if (rawHandle == IntPtr.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(rawHandle), rawHandle, null);
            }

            return NativeMethods.libusb_ref_device(rawHandle);
        }
    }
}
