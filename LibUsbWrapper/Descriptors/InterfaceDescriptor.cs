using System.Collections.Generic;
using System.Linq;
using LibUsbWrapper.Native;

namespace LibUsbWrapper.Descriptors
{
    public sealed class InterfaceDescriptor : IDescriptor
    {
        internal InterfaceDescriptor(LibUsbInterfaceDescriptor descriptor)
        {
            Length = descriptor.bLength;
            DescriptorType = descriptor.bDescriptorType;
            InterfaceNumber = descriptor.bInterfaceNumber;
            AlternateSetting = descriptor.bAlternateSetting;
            InterfaceClass = descriptor.bInterfaceClass;
            InterfaceSubClass = descriptor.bInterfaceSubClass;
            InterfaceProtocol = descriptor.bInterfaceProtocol;
            InterfaceStringIndex = descriptor.iInterface;
            EndpointDescriptors = ReadEndpointDescriptors(descriptor).ToArray();
            ExtraDescriptors = Utils.Native.ReadBytes(descriptor.extra, descriptor.extra_length);
        }

        public byte Length { get; }

        public byte DescriptorType { get; }

        public byte InterfaceNumber { get; }

        public byte AlternateSetting { get; }

        public byte InterfaceClass { get; }

        public byte InterfaceSubClass { get; }

        public byte InterfaceProtocol { get; }

        public byte InterfaceStringIndex { get; }

        public IReadOnlyList<EndpointDescriptor> EndpointDescriptors { get; }

        public IReadOnlyList<byte> ExtraDescriptors { get; }

        private static IEnumerable<EndpointDescriptor> ReadEndpointDescriptors(LibUsbInterfaceDescriptor descriptor)
        {
            var rawEndpointDescriptors = Utils.Native.ReadArray<LibUsbEndpointDescriptor>(descriptor.endpoint, descriptor.bNumEndpoints);
            foreach (var rawEndpointDescriptor in rawEndpointDescriptors)
            {
                yield return new EndpointDescriptor(rawEndpointDescriptor);
            }
        }
    }
}
