using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using LibUsbWrapper.Native;

namespace LibUsbWrapper
{
    [Serializable]
#pragma warning disable CA1032 // Implement standard exception constructors
    public class LibUsbException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
    {
        internal LibUsbException(LibUsbError error) : base(StrError(error))
        {
            Error = error;
        }

        protected LibUsbException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Error = (LibUsbError) info.GetValue(nameof(Error), typeof(LibUsbError));
        }

        public LibUsbError Error { get; }

        private static string StrError(LibUsbError error)
        {
            var stringPointer = NativeMethods.libusb_strerror(error);
            return stringPointer == IntPtr.Zero ? "" : Marshal.PtrToStringAnsi(stringPointer);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Error), Error);
        }
    }
}
