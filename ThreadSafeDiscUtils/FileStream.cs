using System;
using System.Collections.Generic;
using System.IO;
using DiscUtils.Streams;

namespace ThreadSafeDiscUtils
{
    public class FileStream : SparseStream
    {
        private bool _disposed;
        private readonly HandleBasedFileSystem _fileSystem;
        private readonly long _fileHandle;

        internal FileStream(HandleBasedFileSystem fileSystem, long fileHandle, bool canRead, bool canSeek,
            bool canWrite)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _fileHandle = fileHandle;

            CanRead = canRead;
            CanSeek = canSeek;
            CanWrite = canWrite;
        }

        public override void Flush()
        {
            _fileSystem.FlushFile(_fileHandle);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _fileSystem.ReadFile(_fileHandle, buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _fileSystem.SeekFile(_fileHandle, offset, origin);
        }

        public override void SetLength(long value)
        {
            _fileSystem.SetLengthFile(_fileHandle, value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _fileSystem.WriteFile(_fileHandle, buffer, offset, count);
        }

        public override bool CanRead { get; }

        public override bool CanSeek { get; }

        public override bool CanWrite { get; }

        public override long Length => _fileSystem.GetLengthFile(_fileHandle);

        public override long Position
        {
            get => Seek(0, SeekOrigin.Current);

            set => Seek(value, SeekOrigin.Begin);
        }

        public override IEnumerable<StreamExtent> Extents => _fileSystem.GetExtentsFile(_fileHandle);

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _fileSystem.CloseHandle(_fileHandle);
                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
