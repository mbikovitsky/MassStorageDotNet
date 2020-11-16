using System;
using System.Threading;
using LibUsbWrapper.Native;

namespace LibUsbWrapper
{
    internal sealed class LibUsbContext
    {
        private readonly LibUsbContextHandle _context;

        private static readonly Lazy<LibUsbContext> LazyInstance =
            new Lazy<LibUsbContext>(() => new LibUsbContext(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static LibUsbContext Instance => LazyInstance.Value;

        private LibUsbContext()
        {
            var error = NativeMethods.libusb_init(out _context);
            if (error != LibUsbError.Success)
            {
                throw new LibUsbException(error);
            }
        }

        public UsbDeviceCollection GetDeviceList()
        {
            var numberOfElements = NativeMethods.libusb_get_device_list(_context, out var list);
            if (numberOfElements.ToInt64() < 0)
            {
                throw new LibUsbException((LibUsbError) numberOfElements.ToInt32());
            }

            using (list)
            {
                return new UsbDeviceCollection(list);
            }
        }
    }
}
