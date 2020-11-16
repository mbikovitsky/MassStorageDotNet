namespace MassStorageDotNet.MassStorage
{
    internal enum CommandStatus : byte
    {
        Success = 0x00,
        Failure = 0x01,
        PhaseError = 0x02,
    }
}
