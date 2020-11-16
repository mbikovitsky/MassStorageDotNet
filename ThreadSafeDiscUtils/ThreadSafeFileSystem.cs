using System;
using System.Collections.Generic;
using System.IO;
using DiscUtils;
using DiscUtils.Streams;

namespace ThreadSafeDiscUtils
{
    public class ThreadSafeFileSystem : DiscFileSystem
    {
        private readonly object _lock = new object();
        private readonly DiscFileSystem _wrappedFileSystem;

        private ThreadSafeFileSystem(DiscFileSystem wrappedFileSystem)
        {
            _wrappedFileSystem = wrappedFileSystem ?? throw new ArgumentNullException(nameof(wrappedFileSystem));

            FriendlyName = $"{_wrappedFileSystem.FriendlyName} (Thread-safe)";
            CanWrite = _wrappedFileSystem.CanWrite;
            VolumeLabel = _wrappedFileSystem.VolumeLabel;
        }

        public static ThreadSafeFileSystem Create(DiscFileSystem wrappedFileSystem)
        {
            try
            {
                return new ThreadSafeFileSystem(wrappedFileSystem);
            }
            catch
            {
                wrappedFileSystem?.Dispose();
                throw;
            }
        }

        public override void CopyFile(string sourceFile, string destinationFile)
        {
            lock (_lock)
            {
                _wrappedFileSystem.CopyFile(sourceFile, destinationFile);
            }
        }

        public override void CopyFile(string sourceFile, string destinationFile, bool overwrite)
        {
            lock (_lock)
            {
                _wrappedFileSystem.CopyFile(sourceFile, destinationFile, overwrite);
            }
        }

        public override void CreateDirectory(string path)
        {
            lock (_lock)
            {
                _wrappedFileSystem.CreateDirectory(path);
            }
        }

        public override void DeleteDirectory(string path)
        {
            lock (_lock)
            {
                _wrappedFileSystem.DeleteDirectory(path);
            }
        }

        public override void DeleteFile(string path)
        {
            lock (_lock)
            {
                _wrappedFileSystem.DeleteFile(path);
            }
        }

        public override bool DirectoryExists(string path)
        {
            lock (_lock)
            {
                return _wrappedFileSystem.DirectoryExists(path);
            }
        }

        public override bool FileExists(string path)
        {
            lock (_lock)
            {
                return _wrappedFileSystem.FileExists(path);
            }
        }

        public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            lock (_lock)
            {
                return _wrappedFileSystem.GetDirectories(path, searchPattern, searchOption);
            }
        }

        public override string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            lock (_lock)
            {
                return _wrappedFileSystem.GetFiles(path, searchPattern, searchOption);
            }
        }

        public override string[] GetFileSystemEntries(string path)
        {
            lock (_lock)
            {
                return _wrappedFileSystem.GetFileSystemEntries(path);
            }
        }

        public override string[] GetFileSystemEntries(string path, string searchPattern)
        {
            lock (_lock)
            {
                return _wrappedFileSystem.GetFileSystemEntries(path, searchPattern);
            }
        }

        public override void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName)
        {
            lock (_lock)
            {
                _wrappedFileSystem.MoveDirectory(sourceDirectoryName, destinationDirectoryName);
            }
        }

        public override void MoveFile(string sourceName, string destinationName)
        {
            lock (_lock)
            {
                _wrappedFileSystem.MoveFile(sourceName, destinationName);
            }
        }

        public override void MoveFile(string sourceName, string destinationName, bool overwrite)
        {
            lock (_lock)
            {
                _wrappedFileSystem.MoveFile(sourceName, destinationName, overwrite);
            }
        }

        public override SparseStream OpenFile(string path, FileMode mode)
        {
            throw new NotSupportedException();
        }

        public override SparseStream OpenFile(string path, FileMode mode, FileAccess access)
        {
            throw new NotSupportedException();
        }

        public override FileAttributes GetAttributes(string path)
        {
            lock (_lock)
            {
                return _wrappedFileSystem.GetAttributes(path);
            }
        }

        public override void SetAttributes(string path, FileAttributes newValue)
        {
            lock (_lock)
            {
                _wrappedFileSystem.SetAttributes(path, newValue);
            }
        }

        public override DateTime GetCreationTimeUtc(string path)
        {
            lock (_lock)
            {
                return _wrappedFileSystem.GetCreationTimeUtc(path);
            }
        }

        public override void SetCreationTimeUtc(string path, DateTime newTime)
        {
            lock (_lock)
            {
                _wrappedFileSystem.SetCreationTimeUtc(path, newTime);
            }
        }

        public override DateTime GetLastAccessTimeUtc(string path)
        {
            lock (_lock)
            {
                return _wrappedFileSystem.GetLastAccessTimeUtc(path);
            }
        }

        public override void SetLastAccessTimeUtc(string path, DateTime newTime)
        {
            lock (_lock)
            {
                _wrappedFileSystem.SetLastAccessTimeUtc(path, newTime);
            }
        }

        public override DateTime GetLastWriteTimeUtc(string path)
        {
            lock (_lock)
            {
                return _wrappedFileSystem.GetLastWriteTimeUtc(path);
            }
        }

        public override void SetLastWriteTimeUtc(string path, DateTime newTime)
        {
            lock (_lock)
            {
                _wrappedFileSystem.SetLastWriteTimeUtc(path, newTime);
            }
        }

        public override long GetFileLength(string path)
        {
            lock (_lock)
            {
                return _wrappedFileSystem.GetFileLength(path);
            }
        }

        public override byte[] ReadBootCode()
        {
            lock (_lock)
            {
                return _wrappedFileSystem.ReadBootCode();
            }
        }

        public override string FriendlyName { get; }

        public override bool CanWrite { get; }

        public override string VolumeLabel { get; }

        public override bool IsThreadSafe => true;

        public override long Size
        {
            get
            {
                lock (_lock)
                {
                    return _wrappedFileSystem.Size;
                }
            }
        }

        public override long UsedSpace
        {
            get
            {
                lock (_lock)
                {
                    return _wrappedFileSystem.UsedSpace;
                }
            }
        }

        public override long AvailableSpace
        {
            get
            {
                lock (_lock)
                {
                    return _wrappedFileSystem.AvailableSpace;
                }
            }
        }

        public override DiscFileSystemOptions Options => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    _wrappedFileSystem.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public void CreateFile(string path, FileMode mode, FileAccess access)
        {
            lock (_lock)
            {
                using (_wrappedFileSystem.OpenFile(path, mode, access))
                {
                }
            }
        }

        public int ReadFile(string path, long position, byte[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                using var file = _wrappedFileSystem.OpenFile(path, FileMode.Open, FileAccess.Read);
                file.Position = position;
                return file.Read(buffer, offset, count);
            }
        }

        public int WriteFile(string path, long position, byte[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                using var file = _wrappedFileSystem.OpenFile(path, FileMode.Open, FileAccess.Write);
                file.Position = position;
                file.Write(buffer, offset, count);
                return (int)(file.Position - position);
            }
        }

        public void SetLengthFile(string path, long length)
        {
            lock (_lock)
            {
                using var file = _wrappedFileSystem.OpenFile(path, FileMode.Open, FileAccess.Write);
                file.SetLength(length);
            }
        }

        public IEnumerable<StreamExtent> GetExtentsFile(string path)
        {
            lock (_lock)
            {
                using var file = _wrappedFileSystem.OpenFile(path, FileMode.Open, FileAccess.Read);
                return file.Extents;
            }
        }

        public bool ArePathsEquivalent(string path1, string path2)
        {
            lock (_lock)
            {
                return _wrappedFileSystem.ArePathsEquivalent(path1, path2);
            }
        }

        public bool IsCaseSensitive()
        {
            lock (_lock)
            {
                return _wrappedFileSystem.IsCaseSensitive();
            }
        }
    }
}
