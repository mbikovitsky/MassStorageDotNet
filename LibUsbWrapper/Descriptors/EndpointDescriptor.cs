using System.Collections.Generic;
using LibUsbWrapper.Native;

namespace LibUsbWrapper.Descriptors
{
    public sealed class EndpointDescriptor : IDescriptor
    {
        internal EndpointDescriptor(LibUsbEndpointDescriptor descriptor)
        {
            Length = descriptor.bLength;
            DescriptorType = descriptor.bDescriptorType;
            EndpointAddress = descriptor.bEndpointAddress;
            Attributes = descriptor.bmAttributes;
            MaxPacketSize = descriptor.wMaxPacketSize;
            Interval = descriptor.bInterval;
            Refresh = descriptor.bRefresh;
            SynchAddress = descriptor.bSynchAddress;
            ExtraDescriptors = Utils.Native.ReadBytes(descriptor.extra, descriptor.extra_length);
        }

        public byte Length { get; }

        public byte DescriptorType { get; }

        public byte EndpointAddress { get; }

        public byte Attributes { get; }

        public ushort MaxPacketSize { get; }

        public byte Interval { get; }

        public byte Refresh { get; }

        public byte SynchAddress { get; }

        public IReadOnlyList<byte> ExtraDescriptors { get; }
    }
}
