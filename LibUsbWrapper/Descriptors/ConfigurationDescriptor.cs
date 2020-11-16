using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LibUsbWrapper.Native;
using Utils;

namespace LibUsbWrapper.Descriptors
{
    public sealed class ConfigurationDescriptor : IDescriptor
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        internal ConfigurationDescriptor(LibUsbConfigDescriptorHandle descriptor)
        {
            Params.ValidateSafeHandle(descriptor);

            var configDescriptor = Marshal.PtrToStructure<LibUsbConfigDescriptor>(descriptor.DangerousGetHandle());
            Length = configDescriptor.bLength;
            DescriptorType = configDescriptor.bDescriptorType;
            TotalLength = configDescriptor.wTotalLength;
            ConfigurationValue = configDescriptor.bConfigurationValue;
            ConfigurationStringIndex = configDescriptor.iConfiguration;
            Attributes = configDescriptor.bmAttributes;
            MaxPower = configDescriptor.MaxPower;
            InterfaceDescriptors = ReadInterfaceDescriptors(configDescriptor).ToArray();
            ExtraDescriptors = Utils.Native.ReadBytes(configDescriptor.extra, configDescriptor.extra_length);
        }

        public byte Length { get; }

        public byte DescriptorType { get; }

        public ushort TotalLength { get; }

        public byte ConfigurationValue { get; }

        public byte ConfigurationStringIndex { get; }

        public byte Attributes { get; }

        public byte MaxPower { get; }

        public IReadOnlyList<InterfaceDescriptor> InterfaceDescriptors { get; }

        public IReadOnlyList<byte> ExtraDescriptors { get; }

        private static IEnumerable<InterfaceDescriptor> ReadInterfaceDescriptors(LibUsbConfigDescriptor descriptor)
        {
            var interfaces = Utils.Native.ReadArray<LibUsbInterface>(descriptor.@interface, descriptor.bNumInterfaces);
            foreach (var @interface in interfaces)
            {
                var altSettings = Utils.Native.ReadArray<LibUsbInterfaceDescriptor>(@interface.altsetting, @interface.num_altsetting);
                foreach (var altSetting in altSettings)
                {
                    yield return new InterfaceDescriptor(altSetting);
                }
            }
        }
    }
}
