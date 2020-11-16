using System;

namespace MassStorageDotNet.Usb
{
    public class StallException : UsbException
    {
        public StallException()
        {
        }

        public StallException(string message) : base(message)
        {
        }

        public StallException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
