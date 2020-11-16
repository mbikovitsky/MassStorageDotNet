using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LibUsbWrapper.Descriptors;
using LibUsbWrapper.Native;
using Utils;

namespace LibUsbWrapper
{
    public sealed class UsbDevice : IDisposable
    {
        private readonly LibUsbDeviceHandle _device;
        private readonly Lazy<DeviceDescriptor> _descriptor;
        private readonly Lazy<byte[]> _portNumbers;
        private readonly Lazy<ConfigurationDescriptor[]> _configurations;

        private UsbDevice(LibUsbDeviceHandle device)
        {
            Params.ValidateSafeHandle(device);

            _device = device;
            _descriptor = new Lazy<DeviceDescriptor>(GetDeviceDescriptor, LazyThreadSafetyMode.ExecutionAndPublication);
            _portNumbers = new Lazy<byte[]>(GetPortNumbers, LazyThreadSafetyMode.ExecutionAndPublication);
            _configurations = new Lazy<ConfigurationDescriptor[]>(GetConfigurationDescriptors, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        internal static UsbDevice Create(IntPtr rawHandle)
        {
            if (rawHandle == IntPtr.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(rawHandle), rawHandle, null);
            }

            var device = LibUsbDeviceHandle.RefDevice(rawHandle);
            try
            {
                return new UsbDevice(device);
            }
            catch
            {
                device.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            _device.Dispose();
        }

        public byte BusNumber => NativeMethods.libusb_get_bus_number(_device);

        public byte PortNumber => NativeMethods.libusb_get_port_number(_device);

        public IReadOnlyList<byte> PortNumbers => _portNumbers.Value;

        public byte Address => NativeMethods.libusb_get_device_address(_device);

        public UsbSpeed Speed => NativeMethods.libusb_get_device_speed(_device);

        public DeviceDescriptor Descriptor => _descriptor.Value;

        public IReadOnlyList<ConfigurationDescriptor> ConfigurationDescriptors => _configurations.Value;

        public UsbDeviceConnection Open()
        {
            var error = NativeMethods.libusb_open(_device, out var deviceHandle);
            if (error != LibUsbError.Success)
            {
                throw new LibUsbException(error);
            }

            return UsbDeviceConnection.Create(deviceHandle);
        }

        public static UsbDeviceCollection GetDeviceList()
        {
            return LibUsbContext.Instance.GetDeviceList();
        }

        private DeviceDescriptor GetDeviceDescriptor()
        {
            if (_device.IsClosed)
            {
                throw new ObjectDisposedException(nameof(UsbDevice));
            }

            var error = NativeMethods.libusb_get_device_descriptor(_device, out var desc);
            if (error != LibUsbError.Success)
            {
                throw new LibUsbException(error);
            }
            return new DeviceDescriptor(desc);
        }

        private byte[] GetPortNumbers()
        {
            var portNumbers = new byte[7];
            var returned = NativeMethods.libusb_get_port_numbers(_device, portNumbers, portNumbers.Length);
            if (returned < 0)
            {
                throw new LibUsbException((LibUsbError)returned);
            }

            return portNumbers.Take(returned).ToArray();
        }

        private ConfigurationDescriptor[] GetConfigurationDescriptors()
        {
            if (_device.IsClosed)
            {
                throw new ObjectDisposedException(nameof(UsbDevice));
            }

            var result = new ConfigurationDescriptor[Descriptor.NumConfigurations];

            for (byte configuration = 0; configuration < Descriptor.NumConfigurations; configuration++)
            {
                var error = NativeMethods.libusb_get_config_descriptor(_device, configuration, out var configDescriptor);
                if (error != LibUsbError.Success)
                {
                    throw new LibUsbException(error);
                }

                using (configDescriptor)
                {
                    result[configuration] = new ConfigurationDescriptor(configDescriptor);
                }
            }

            return result;
        }
    }
}
