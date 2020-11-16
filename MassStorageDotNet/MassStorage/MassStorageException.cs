using System;
using System.IO;

namespace MassStorageDotNet.MassStorage
{
    public class MassStorageException : IOException
    {
        public MassStorageException()
        {
        }

        public MassStorageException(string message) : base(message)
        {
        }

        public MassStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
