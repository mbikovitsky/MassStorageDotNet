using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LibUsbWrapper.Native;
using Utils;

namespace LibUsbWrapper
{
    public sealed class UsbDeviceCollection : IDisposable, IReadOnlyList<UsbDevice>
    {
        private readonly UsbDevice[] _devices;

        internal UsbDeviceCollection(LibUsbDeviceListHandle list)
        {
            Params.ValidateSafeHandle(list);
            _devices = GetDevices(list);
        }

        public void Dispose()
        {
            foreach (var device in _devices)
            {
                device.Dispose();
            }
        }

        public IEnumerator<UsbDevice> GetEnumerator()
        {
            return _devices.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _devices.GetEnumerator();
        }

        public int Count => _devices.Length;

        public UsbDevice this[int index] => _devices[index];

        private static UsbDevice[] GetDevices(LibUsbDeviceListHandle list)
        {
            var rawHandles = GetRawHandles(list);

            var devices = new UsbDevice[rawHandles.Length];
            try
            {
                for (int index = 0; index < rawHandles.Length; index++)
                {
                    devices[index] = UsbDevice.Create(rawHandles[index]);
                }

                return devices;
            }
            catch
            {
                foreach (var device in devices)
                {
                    device?.Dispose();
                }

                throw;
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static IntPtr[] GetRawHandles(LibUsbDeviceListHandle list)
        {
            var handles = new List<IntPtr>();

            for (int index = 0;; ++index)
            {
                var currentHandle = Marshal.ReadIntPtr(list.DangerousGetHandle(), index * Marshal.SizeOf<IntPtr>());
                if (currentHandle == IntPtr.Zero)
                {
                    break;
                }

                handles.Add(currentHandle);
            }

            return handles.ToArray();
        }
    }
}
