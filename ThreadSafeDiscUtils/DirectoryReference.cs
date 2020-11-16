using System;

namespace ThreadSafeDiscUtils
{
    public class DirectoryReference : IDisposable
    {
        private bool _disposed;
        private readonly HandleBasedFileSystem _fileSystem;
        private readonly long _directoryHandle;

        internal DirectoryReference(HandleBasedFileSystem fileSystem, long directoryHandle)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _directoryHandle = directoryHandle;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _fileSystem.CloseHandle(_directoryHandle);
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
