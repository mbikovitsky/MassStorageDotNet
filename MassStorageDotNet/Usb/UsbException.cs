using System;

namespace MassStorageDotNet.Usb
{
    public class UsbException : Exception
    {
        public UsbException()
        {
        }

        public UsbException(string message) : base(message)
        {
        }

        public UsbException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
