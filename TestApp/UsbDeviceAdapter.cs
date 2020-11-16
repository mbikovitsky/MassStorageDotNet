using System;
using System.Collections.Generic;
using System.Linq;
using LibUsbWrapper;
using MassStorageDotNet.Usb;

namespace TestApp
{
    internal class UsbDeviceAdapter : IUsbDevice
    {
        public UsbDeviceAdapter(UsbDevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            using var connection = device.Open();

            var deviceDescriptor = device.Descriptor;

            DeviceClass = (UsbClass) deviceDescriptor.DeviceClass;
            DeviceSubClass = deviceDescriptor.DeviceSubClass;
            DeviceProtocol = deviceDescriptor.DeviceProtocol;
            VendorId = deviceDescriptor.VendorID;
            ProductId = deviceDescriptor.ProductID;

            ProductName = connection.ProductString;
            SerialNumber = connection.SerialString;
            Manufacturer = connection.ManufacturerString;

            Configurations = device.ConfigurationDescriptors
                .Select(descriptor => new UsbConfigurationAdapter(connection, descriptor))
                .ToArray();

            PortNumbers = device.PortNumbers;
        }

        public UsbClass DeviceClass { get; }

        public byte DeviceSubClass { get; }

        public byte DeviceProtocol { get; }

        public ushort VendorId { get; }

        public ushort ProductId { get; }

        public string ProductName { get; }

        public string SerialNumber { get; }

        public string Manufacturer { get; }

        public IReadOnlyList<IUsbConfiguration> Configurations { get; }

        public IReadOnlyList<byte> PortNumbers { get; }
    }
}
