namespace MassStorageDotNet.MassStorage
{
#pragma warning disable CA1032 // Implement standard exception constructors
    public class ScsiException : MassStorageException
#pragma warning restore CA1032 // Implement standard exception constructors
    {
        internal ScsiException(byte key, byte code, byte qualifier) : base(FormatMessage(key, code, qualifier))
        {
            Key = key;
            Code = code;
            Qualifier = qualifier;
        }

        public byte Key { get; }

        public byte Code { get; }

        public byte Qualifier { get; }

        private static string FormatMessage(byte key, byte code, byte qualifier)
        {
            return $"SCSI Sense: {key:X2} {code:X2} {qualifier:X2}";
        }
    }
}
