using System;
using LibUsbWrapper.Descriptors;
using MassStorageDotNet.Usb;

namespace TestApp
{
    internal class UsbEndpointAdapter : UsbEndpoint
    {
        public UsbEndpointAdapter(EndpointDescriptor endpointDescriptor)
        {
            if (endpointDescriptor == null)
            {
                throw new ArgumentNullException(nameof(endpointDescriptor));
            }

            EndpointAddress = endpointDescriptor.EndpointAddress;
            Attributes = endpointDescriptor.Attributes;
            MaxPacketSize = endpointDescriptor.MaxPacketSize;
            Interval = endpointDescriptor.Interval;
        }

        public override byte EndpointAddress { get; }

        public override byte Attributes { get; }

        public override ushort MaxPacketSize { get; }

        public override byte Interval { get; }
    }
}
