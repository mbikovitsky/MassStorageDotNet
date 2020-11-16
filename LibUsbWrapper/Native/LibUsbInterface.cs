using System;
using System.Runtime.InteropServices;

namespace LibUsbWrapper.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct LibUsbInterface
    {
        /** Array of interface descriptors. The length of this array is determined
         * by the num_altsetting field. */
        public IntPtr altsetting;

        /** The number of alternate settings that belong to this interface */
        public int num_altsetting;
    }
}
