using System;
using System.Collections.Generic;
using System.Linq;
using LibUsbWrapper;
using LibUsbWrapper.Descriptors;
using MassStorageDotNet.Usb;

namespace TestApp
{
    internal class UsbConfigurationAdapter : IUsbConfiguration
    {
        public UsbConfigurationAdapter(UsbDeviceConnection connection, ConfigurationDescriptor configDescriptor)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (configDescriptor == null)
            {
                throw new ArgumentNullException(nameof(configDescriptor));
            }

            Id = configDescriptor.ConfigurationValue;
            MaxPower = configDescriptor.MaxPower;
            Name = connection.GetStringDescriptorAscii(configDescriptor.ConfigurationStringIndex);
            IsRemoteWakeup = 0 != (configDescriptor.Attributes & 0x20);
            IsSelfPowered = 0 != (configDescriptor.Attributes & 0x40);

            Interfaces = configDescriptor.InterfaceDescriptors
                .Select(descriptor => new UsbInterfaceAdapter(connection, descriptor))
                .ToArray();
        }

        public byte Id { get; }

        public byte MaxPower { get; }

        public string Name { get; }

        public bool IsRemoteWakeup { get; }

        public bool IsSelfPowered { get; }

        public IReadOnlyList<IUsbInterface> Interfaces { get; }
    }
}
