using System.Collections.Generic;
using System.Linq;
using LibUsbWrapper;
using MassStorageDotNet.Usb;
using IUsbDevice = MassStorageDotNet.Usb.IUsbDevice;

namespace TestApp
{
    internal sealed class UsbManagerAdapter : IUsbManager
    {
        public IEnumerable<IUsbDevice> GetDevices()
        {
            using var list = UsbDevice.GetDeviceList();
            return list.Where(IsSuitableDevice).Select(device => new UsbDeviceAdapter(device)).ToList();
        }

        public IUsbConnection OpenDevice(IUsbDevice device)
        {
            using var list = UsbDevice.GetDeviceList();
            var foundDevice = list.Single(usbDevice =>
                usbDevice.PortNumbers.SequenceEqual(((UsbDeviceAdapter) device).PortNumbers));
            return UsbConnectionAdapter.Create(foundDevice.Open());
        }

        private static bool IsSuitableDevice(UsbDevice device)
        {
            try
            {
                using (device.Open())
                {
                }
            }
            catch (LibUsbException)
            {
                return false;
            }

            return true;
        }
    }
}
