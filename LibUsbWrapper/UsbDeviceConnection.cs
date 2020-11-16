using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using LibUsbWrapper.Native;
using Utils;

namespace LibUsbWrapper
{
    public sealed class UsbDeviceConnection : IDisposable
    {
        private readonly LibUsbDeviceHandleHandle _deviceHandle;
        private readonly Lazy<string> _productString;
        private readonly Lazy<string> _serialString;
        private readonly Lazy<string> _manufacturerString;

        private UsbDeviceConnection(LibUsbDeviceHandleHandle deviceHandle)
        {
            Utils.Params.ValidateSafeHandle(deviceHandle);

            _deviceHandle = deviceHandle;
            _productString = new Lazy<string>(GetProductString, LazyThreadSafetyMode.ExecutionAndPublication);
            _serialString = new Lazy<string>(GetSerialString, LazyThreadSafetyMode.ExecutionAndPublication);
            _manufacturerString = new Lazy<string>(GetManufacturerString, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        internal static UsbDeviceConnection Create(LibUsbDeviceHandleHandle deviceHandle)
        {
            try
            {
                return new UsbDeviceConnection(deviceHandle);
            }
            catch
            {
                deviceHandle?.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            _deviceHandle.Dispose();
        }

        public string ProductString => _productString.Value;

        public string SerialString => _serialString.Value;

        public string ManufacturerString => _manufacturerString.Value;

        public UsbDevice GetDevice()
        {
            EnsureNotDisposed();

            return UsbDevice.Create(NativeMethods.libusb_get_device(_deviceHandle));
        }

        public string GetStringDescriptorAscii(byte descriptorIndex)
        {
            EnsureNotDisposed();

            if (descriptorIndex == 0)
            {
                return "";
            }

            // String descriptors contain at most 256 bytes of data. As do all USB descriptors.
            var bytes = new byte[byte.MaxValue + 1];

            var returned = NativeMethods.libusb_get_string_descriptor_ascii(_deviceHandle, descriptorIndex, bytes, bytes.Length);
            if (returned < 0)
            {
                throw new LibUsbException((LibUsbError) returned);
            }

            return Encoding.ASCII.GetString(bytes, 0, returned);
        }

        public ushort ControlTransfer(
            byte requestType,
            byte request,
            ushort value,
            ushort index,
            byte[]? data,
            ushort length,
            uint timeout)
        {
            return ControlTransfer(requestType, request, value, index, data, 0, length, timeout);
        }

        public ushort ControlTransfer(
            byte requestType,
            byte request,
            ushort value,
            ushort index,
            byte[]? data,
            int offset,
            ushort length,
            uint timeout)
        {
            data ??= Array.Empty<byte>();

            Params.ValidateBuffer(data, offset, length);

            int transferred;
            using (var pinned = new PinnedObject<byte[]>(data))
            {
                var startAddress = pinned.Address + offset;
                transferred = NativeMethods.libusb_control_transfer(
                    _deviceHandle,
                    requestType,
                    request,
                    value,
                    index,
                    startAddress,
                    length,
                    timeout);
            }
            if (transferred < 0)
            {
                throw new LibUsbException((LibUsbError)transferred);
            }

            Debug.Assert(transferred <= ushort.MaxValue);
            return (ushort)transferred;
        }

        public int BulkTransfer(byte endpoint, byte[] data, int length, uint timeout)
        {
            return BulkTransfer(endpoint, data, 0, length, timeout);
        }

        public int BulkTransfer(byte endpoint, byte[] data, int offset, int length, uint timeout)
        {
            Params.ValidateBuffer(data, offset, length);

            LibUsbError error;
            int transferred;
            using (var pinned = new PinnedObject<byte[]>(data))
            {
                var startAddress = pinned.Address + offset;
                error = NativeMethods.libusb_bulk_transfer(_deviceHandle, endpoint, startAddress, length,
                    out transferred, timeout);
            }
            if (error != LibUsbError.Success)
            {
                throw new LibUsbException(error);
            }

            return transferred;
        }

        public void SetConfiguration(int configuration)
        {
            var error = NativeMethods.libusb_set_configuration(_deviceHandle, configuration);
            if (error != LibUsbError.Success)
            {
                throw new LibUsbException(error);
            }
        }

        public void ClaimInterface(byte interfaceNumber)
        {
            var error = NativeMethods.libusb_claim_interface(_deviceHandle, interfaceNumber);
            if (error != LibUsbError.Success)
            {
                throw new LibUsbException(error);
            }
        }

        public void ReleaseInterface(byte interfaceNumber)
        {
            var error = NativeMethods.libusb_release_interface(_deviceHandle, interfaceNumber);
            if (error != LibUsbError.Success)
            {
                throw new LibUsbException(error);
            }
        }

        public void SetInterface(byte interfaceNumber, byte alternateSetting)
        {
            var error = NativeMethods.libusb_set_interface_alt_setting(_deviceHandle, interfaceNumber, alternateSetting);
            if (error != LibUsbError.Success)
            {
                throw new LibUsbException(error);
            }
        }

        public void ClearStall(byte endpoint)
        {
            var error = NativeMethods.libusb_clear_halt(_deviceHandle, endpoint);
            if (error != LibUsbError.Success)
            {
                throw new LibUsbException(error);
            }
        }

        private string GetProductString()
        {
            using var device = GetDevice();
            return GetStringDescriptorAscii(device.Descriptor.ProductStringIndex);
        }

        private string GetSerialString()
        {
            using var device = GetDevice();
            return GetStringDescriptorAscii(device.Descriptor.SerialNumberStringIndex);
        }

        private string GetManufacturerString()
        {
            using var device = GetDevice();
            return GetStringDescriptorAscii(device.Descriptor.ManufacturerStringIndex);
        }

        private void EnsureNotDisposed()
        {
            if (_deviceHandle.IsClosed)
            {
                throw new ObjectDisposedException(nameof(UsbDeviceConnection));
            }
        }
    }
}
