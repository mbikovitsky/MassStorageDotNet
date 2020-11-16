namespace LibUsbWrapper.Descriptors
{
    public interface IDescriptor
    {
        byte Length { get; }

        // TODO: Enum
        byte DescriptorType { get; }
    }
}
