using System;

namespace MassStorageDotNet.Usb
{
    public class TimeoutException : UsbException
    {
        public TimeoutException()
        {
        }

        public TimeoutException(string message) : base(message)
        {
        }

        public TimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
