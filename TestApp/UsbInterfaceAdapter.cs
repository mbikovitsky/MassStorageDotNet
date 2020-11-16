using System;
using System.Collections.Generic;
using System.Linq;
using LibUsbWrapper;
using LibUsbWrapper.Descriptors;
using MassStorageDotNet.Usb;

namespace TestApp
{
    internal class UsbInterfaceAdapter : IUsbInterface
    {
        public UsbInterfaceAdapter(UsbDeviceConnection connection, InterfaceDescriptor interfaceDescriptor)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (interfaceDescriptor == null)
            {
                throw new ArgumentNullException(nameof(interfaceDescriptor));
            }

            AlternateSetting = interfaceDescriptor.AlternateSetting;
            InterfaceNumber = interfaceDescriptor.InterfaceNumber;
            InterfaceClass = (UsbClass) interfaceDescriptor.InterfaceClass;
            InterfaceSubClass = interfaceDescriptor.InterfaceSubClass;
            InterfaceProtocol = interfaceDescriptor.InterfaceProtocol;
            Name = connection.GetStringDescriptorAscii(interfaceDescriptor.InterfaceStringIndex);

            Endpoints = interfaceDescriptor.EndpointDescriptors
                .Select(descriptor => new UsbEndpointAdapter(descriptor))
                .ToArray();
        }

        public byte AlternateSetting { get; }

        public byte InterfaceNumber { get; }

        public UsbClass InterfaceClass { get; }

        public byte InterfaceSubClass { get; }

        public byte InterfaceProtocol { get; }

        public string Name { get; }

        public IReadOnlyList<IUsbEndpoint> Endpoints { get; }
    }
}
