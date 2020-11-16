using System;
using System.Runtime.InteropServices;

namespace Utils
{
    public static class Params
    {
        public static void ValidateBuffer(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, null);
            }

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("Accessing past end of buffer");
            }
        }

        public static void ValidateSafeHandle(SafeHandle handle)
        {
            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            if (handle.IsClosed)
            {
                throw new ArgumentException("Handle is closed", nameof(handle));
            }

            if (handle.IsInvalid)
            {
                throw new ArgumentException("Handle is invalid", nameof(handle));
            }
        }
    }
}
