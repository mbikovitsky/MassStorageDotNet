using System.Collections.Generic;

namespace MassStorageDotNet.Usb
{
    public interface IUsbManager
    {
        IEnumerable<IUsbDevice> GetDevices();

        IUsbConnection OpenDevice(IUsbDevice device);
    }
}
