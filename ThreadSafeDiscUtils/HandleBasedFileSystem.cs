using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DiscUtils;
using DiscUtils.Streams;
using Utils;

namespace ThreadSafeDiscUtils
{
    public class HandleBasedFileSystem : DiscFileSystem
    {
        private readonly RemoveLock _fileSystemLock = new RemoveLock();
        private readonly ThreadSafeFileSystem _wrappedFileSystem;

        private readonly object _handleTableLock = new object();
        private long _currentHandleValue;
        private readonly Dictionary<long, HandleInfo> _handles = new Dictionary<long, HandleInfo>();
        private bool _disposeOnLastStream;

        private sealed class HandleInfo
        {
            internal HandleInfo(string path, FileMode mode, FileAccess access, FileShare share)
            {
                Path = path;
                IsDirectory = false;
                Mode = mode;
                Access = access;
                Share = share;
            }

            internal HandleInfo(string path)
            {
                Path = path;
                IsDirectory = true;
            }

            public string Path { get; set; }

            public bool IsDirectory { get; }

            public FileMode Mode { get; }

            public FileAccess Access { get; }

            public FileShare Share { get; }

            public long Position { get; set; }

            public long Length { get; set; }
        }

        private HandleBasedFileSystem(ThreadSafeFileSystem wrappedFileSystem)
        {
            _wrappedFileSystem = wrappedFileSystem ?? throw new ArgumentNullException(nameof(wrappedFileSystem));

            FriendlyName = $"{_wrappedFileSystem.FriendlyName} (Handle-based)";
            CanWrite = _wrappedFileSystem.CanWrite;
            VolumeLabel = _wrappedFileSystem.VolumeLabel;
        }

        public static HandleBasedFileSystem Create(ThreadSafeFileSystem wrappedFileSystem)
        {
            try
            {
                return new HandleBasedFileSystem(wrappedFileSystem);
            }
            catch
            {
                wrappedFileSystem?.Dispose();
                throw;
            }
        }

        public override void CopyFile(string sourceFile, string destinationFile, bool overwrite)
        {
            void Copier()
            {
                // Do everything under the handle table lock to prevent new handles
                // from being opened while we're mutating the FS.
                lock (_handleTableLock)
                {
                    // We don't care about the target file, only about the target name, when
                    // overwriting. This is because the target name will be unlinked by the operation.
                    if (overwrite && IsPathInUse(destinationFile))
                    {
                        throw new IOException("Destination file in use");
                    }

                    if (!IsShareCompatible(sourceFile, FileAccess.Read, FileShare.Read))
                    {
                        throw new UnauthorizedAccessException("Sharing violation");
                    }

                    _wrappedFileSystem.CopyFile(sourceFile, destinationFile, overwrite);
                }
            }

            WithFileSystem(Copier);
        }

        public override void CreateDirectory(string path)
        {
            WithFileSystem(() => _wrappedFileSystem.CreateDirectory(path));
        }

        public override void DeleteDirectory(string path)
        {
            // TODO: Do all filesystem prevent deletion of non-empty directories?
            WithFileSystem(() => _wrappedFileSystem.DeleteDirectory(path));
        }

        public override void DeleteFile(string path)
        {
            void Deleter()
            {
                lock (_handleTableLock)
                {
                    if (IsPathInUse(path))
                    {
                        throw new IOException("File in use");
                    }

                    _wrappedFileSystem.DeleteFile(path);
                }
            }

            WithFileSystem(Deleter);
        }

        public override bool DirectoryExists(string path)
        {
            return WithFileSystem(() => _wrappedFileSystem.DirectoryExists(path));
        }

        public override bool FileExists(string path)
        {
            return WithFileSystem(() => _wrappedFileSystem.FileExists(path));
        }

        public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return WithFileSystem(() => _wrappedFileSystem.GetDirectories(path, searchPattern, searchOption));
        }

        public override string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return WithFileSystem(() => _wrappedFileSystem.GetFiles(path, searchPattern, searchOption));
        }

        public override string[] GetFileSystemEntries(string path)
        {
            return WithFileSystem(() => _wrappedFileSystem.GetFileSystemEntries(path));
        }

        public override string[] GetFileSystemEntries(string path, string searchPattern)
        {
            return WithFileSystem(() => _wrappedFileSystem.GetFileSystemEntries(path, searchPattern));
        }

        public override void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName)
        {
            void Mover()
            {
                // Do everything under the handle table lock to prevent new handles
                // from being opened while we're mutating the FS.
                lock (_handleTableLock)
                {
                    // We don't care about the source directory, only about the source name.
                    // This is because the source name will be unlinked by the operation.
                    if (IsPathInUse(sourceDirectoryName))
                    {
                        throw new IOException("Source directory in use");
                    }

                    _wrappedFileSystem.MoveDirectory(sourceDirectoryName, destinationDirectoryName);
                }
            }

            WithFileSystem(Mover);
        }

        public override void MoveFile(string sourceName, string destinationName, bool overwrite)
        {
            void Mover()
            {
                // TODO: Symlinks?
                if (ArePathsEqual(sourceName, destinationName))
                {
                    return;
                }

                // Do everything under the handle table lock to prevent new handles
                // from being opened while we're mutating the FS.
                lock (_handleTableLock)
                {
                    // We don't care about the target file, only about the target name, when
                    // overwriting. This is because the target name will be unlinked by the operation.
                    if (overwrite && IsPathInUse(destinationName))
                    {
                        throw new IOException("Destination file in use");
                    }

                    // Get all handles that reference the same source name.
                    var sourceHandles = _handles.Values.Where(handleInfo => ArePathsEqual(handleInfo.Path, sourceName)).ToList();

                    // To move the source file, everybody has to have it open with Delete share.
                    if (sourceHandles.Any(handleInfo => (handleInfo.Share & FileShare.Delete) == 0))
                    {
                        throw new IOException("Source file in use");
                    }

                    // Actually move the file.
                    _wrappedFileSystem.MoveFile(sourceName, destinationName, overwrite);

                    // Now update all handles to reference the new path.
                    foreach (var handle in sourceHandles)
                    {
                        handle.Path = destinationName;
                    }
                }
            }

            WithFileSystem(Mover);
        }

        public override SparseStream OpenFile(string path, FileMode mode, FileAccess access)
        {
            var share = ((access & FileAccess.Write) != 0) ? FileShare.None : FileShare.Read;
            return OpenFile(path, mode, access, share);
        }

        public SparseStream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            SparseStream OpenCallback()
            {
                long handle;
                lock (_handleTableLock)
                {
                    if (_disposeOnLastStream)
                    {
                        throw new ObjectDisposedException(nameof(HandleBasedFileSystem));
                    }

                    if (!IsShareCompatible(path, access, share))
                    {
                        throw new UnauthorizedAccessException("Sharing violation");
                    }

                    var newMode = mode == FileMode.Append ? FileMode.Append : FileMode.Open;
                    var newAccess = access;
                    if (mode == FileMode.Truncate)
                    {
                        // Can't read from truncated files, apparently.
                        newAccess = access & ~FileAccess.Read;
                    }

                    var handleInfo = new HandleInfo(path, newMode, newAccess, share);

                    handle = GetNextHandleValue();
                    _handles.Add(handle, handleInfo);
                    try
                    {
                        // This will check the path, mode, and access
                        _wrappedFileSystem.CreateFile(path, mode, access);
                    }
                    catch
                    {
                        _handles.Remove(handle);
                        throw;
                    }
                }

                try
                {
                    // There is an edge case here, wherein FileStream creation fails but the OpenFile
                    // call above had side effects. However, FileStream is lightweight, so a constructor
                    // failure indicates we are in deep shit anyway.
                    return new FileStream(
                        this,
                        handle,
                        (access & FileAccess.Read) != 0,
                        true,
                        (access & FileAccess.Write) != 0);
                }
                catch
                {
                    CloseHandle(handle);
                    throw;
                }
            }

            return WithFileSystem(OpenCallback);
        }

        public DirectoryReference OpenDirectory(string path, FileMode mode)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            DirectoryReference OpenCallback()
            {
                long handle;
                lock (_handleTableLock)
                {
                    if (_disposeOnLastStream)
                    {
                        throw new ObjectDisposedException(nameof(HandleBasedFileSystem));
                    }

                    var handleInfo = new HandleInfo(path);

                    handle = GetNextHandleValue();
                    _handles.Add(handle, handleInfo);
                    try
                    {
                        // This will check the path, mode, and access
                        switch (mode)
                        {
                            case FileMode.Create:
                            case FileMode.OpenOrCreate:
                                if (!_wrappedFileSystem.DirectoryExists(path))
                                {
                                    if (_wrappedFileSystem.FileExists(path))
                                    {
                                        throw new IOException("Path already exists");
                                    }
                                    _wrappedFileSystem.CreateDirectory(path);
                                }
                                break;

                            case FileMode.CreateNew:
                                if (_wrappedFileSystem.Exists(path))
                                {
                                    throw new IOException("Path already exists");
                                }
                                _wrappedFileSystem.CreateDirectory(path);
                                break;

                            case FileMode.Open:
                                if (!_wrappedFileSystem.DirectoryExists(path))
                                {
                                    throw new DirectoryNotFoundException();
                                }
                                break;

                            default:
                                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                        }
                    }
                    catch
                    {
                        _handles.Remove(handle);
                        throw;
                    }
                }

                try
                {
                    // There is an edge case here, wherein DirectoryReference creation fails but
                    // the code above had side effects. However, DirectoryReference is lightweight,
                    // so a constructor failure indicates we are in deep shit anyway.
                    return new DirectoryReference(this, handle);
                }
                catch
                {
                    CloseHandle(handle);
                    throw;
                }
            }

            return WithFileSystem(OpenCallback);
        }

        public override FileAttributes GetAttributes(string path)
        {
            return WithFileSystem(() => _wrappedFileSystem.GetAttributes(path));
        }

        public override void SetAttributes(string path, FileAttributes newValue)
        {
            WithFileSystem(() => _wrappedFileSystem.SetAttributes(path, newValue));
        }

        public override DateTime GetCreationTimeUtc(string path)
        {
            return WithFileSystem(() => _wrappedFileSystem.GetCreationTimeUtc(path));
        }

        public override void SetCreationTimeUtc(string path, DateTime newTime)
        {
            WithFileSystem(() => _wrappedFileSystem.SetCreationTimeUtc(path, newTime));
        }

        public override DateTime GetLastAccessTimeUtc(string path)
        {
            return WithFileSystem(() => _wrappedFileSystem.GetLastAccessTimeUtc(path));
        }

        public override void SetLastAccessTimeUtc(string path, DateTime newTime)
        {
            WithFileSystem(() => _wrappedFileSystem.SetLastAccessTimeUtc(path, newTime));
        }

        public override DateTime GetLastWriteTimeUtc(string path)
        {
            return WithFileSystem(() => _wrappedFileSystem.GetLastWriteTimeUtc(path));
        }

        public override void SetLastWriteTimeUtc(string path, DateTime newTime)
        {
            WithFileSystem(() => _wrappedFileSystem.SetLastWriteTimeUtc(path, newTime));
        }

        public override long GetFileLength(string path)
        {
            return WithFileSystem(() => _wrappedFileSystem.GetFileLength(path));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _wrappedFileSystem.Dispose();
                _fileSystemLock.Dispose();
            }
            base.Dispose(disposing);
        }

        public override string FriendlyName { get; }

        public override bool CanWrite { get; }

        public override string VolumeLabel { get; }

        public override bool IsThreadSafe => true;

        public override long Size => WithFileSystem(() => _wrappedFileSystem.Size);

        public override long UsedSpace => WithFileSystem(() => _wrappedFileSystem.UsedSpace);

        public override long AvailableSpace => WithFileSystem(() => _wrappedFileSystem.AvailableSpace);

        public bool Unmount(bool force)
        {
            if (!_fileSystemLock.Acquire())
            {
                throw new ObjectDisposedException(nameof(_wrappedFileSystem));
            }

            if (!force)
            {
                lock (_handleTableLock)
                {
                    if (_handles.Count != 0)
                    {
                        return false;
                    }

                    _fileSystemLock.ReleaseAndWait();
                    _wrappedFileSystem.Dispose();
                    return true;
                }
            }

            _fileSystemLock.ReleaseAndWait();
            _wrappedFileSystem.Dispose();
            return true;
        }

        public void DisposeOnLastStream()
        {
            bool disposeNow;
            lock (_handleTableLock)
            {
                _disposeOnLastStream = true;
                disposeNow = _handles.Count == 0;
            }

            if (disposeNow)
            {
                Dispose();
            }
        }

        public bool ArePathsEquivalent(string path1, string path2)
        {
            return WithFileSystem(() => _wrappedFileSystem.ArePathsEquivalent(path1, path2));
        }

        public bool IsCaseSensitive()
        {
            return WithFileSystem(() => _wrappedFileSystem.IsCaseSensitive());
        }

        private bool IsShareCompatible(string path, FileAccess requestedAccess, FileShare requestedShare)
        {
            var handles = GetHandlesForFile(path).ToList();

            if ((requestedAccess & FileAccess.Write) != 0)
            {
                if ((requestedShare & FileShare.Write) == 0)
                {
                    if (handles.Any(pair => (pair.Value.Access & FileAccess.Write) != 0))
                    {
                        return false;
                    }
                }
                else
                {
                    if (handles.Any(pair => (pair.Value.Share & FileShare.Write) == 0))
                    {
                        return false;
                    }
                }
            }

            if ((requestedAccess & FileAccess.Read) != 0)
            {
                if ((requestedShare & FileShare.Read) == 0)
                {
                    if (handles.Any(pair => (pair.Value.Access & FileAccess.Read) != 0))
                    {
                        return false;
                    }
                }
                else
                {
                    if (handles.Any(pair => (pair.Value.Share & FileShare.Read) == 0))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private IEnumerable<KeyValuePair<long, HandleInfo>> GetHandlesForFile(string path)
        {
            return _handles.Where(pair => ArePathsEquivalent(pair.Value.Path, path));
        }

        private bool IsPathInUse(string path)
        {
            return _handles.Values.Any(info => PathBeginsWith(info.Path, path));
        }

        private bool PathBeginsWith(string path, string prefix)
        {
            // TODO: Is this the correct string comparison?
            return path.StartsWith(prefix,
                IsCaseSensitive() ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        private bool ArePathsEqual(string path1, string path2)
        {
            return path1.Equals(path2,
                IsCaseSensitive() ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        private long GetNextHandleValue()
        {
            return Interlocked.Increment(ref _currentHandleValue);
        }

#pragma warning disable CA1801 // Remove unused parameter
        internal void FlushFile(long handle)
#pragma warning restore CA1801 // Remove unused parameter
        {
            // Nothing to do, since we close the stream every time.
        }

        internal int ReadFile(long handle, byte[] buffer, int offset, int count)
        {
            int Reader(HandleInfo info)
            {
                var read = _wrappedFileSystem.ReadFile(info.Path, info.Position, buffer, offset, count);
                info.Position += read;
                return read;
            }

            return WithFileHandle(handle, Reader);
        }

        internal long SeekFile(long handle, long offset, SeekOrigin origin)
        {
            long Seeker(HandleInfo info)
            {
                var newPosition = origin switch
                {
                    SeekOrigin.Begin => offset,
                    SeekOrigin.Current => info.Position + offset,
                    SeekOrigin.End => info.Length - 1 + offset,
                    _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null),
                };
                if (newPosition < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
                }

                info.Position = newPosition;
                return newPosition;
            }

            return WithFileHandle(handle, Seeker);
        }

        internal void SetLengthFile(long handle, long value)
        {
            void LengthSetter(HandleInfo info)
            {
                _wrappedFileSystem.SetLengthFile(info.Path, value);
                info.Length = value;
            }

            WithFileHandle(handle, LengthSetter);
        }

        internal void WriteFile(long handle, byte[] buffer, int offset, int count)
        {
            void Writer(HandleInfo info)
            {
                var written = _wrappedFileSystem.WriteFile(info.Path, info.Position, buffer, offset, count);
                info.Position += written;
            }

            WithFileHandle(handle, Writer);
        }

        internal long GetLengthFile(long handle)
        {
            return WithFileHandle(handle, info => info.Length);
        }

        internal IEnumerable<StreamExtent> GetExtentsFile(long handle)
        {
            return WithFileHandle(handle, info => _wrappedFileSystem.GetExtentsFile(info.Path));
        }

        internal void CloseHandle(long handle)
        {
            bool disposeNow;
            lock (_handleTableLock)
            {
                try
                {
                    _handles.Remove(handle);
                }
                catch (KeyNotFoundException)
                {
                }

                disposeNow = _disposeOnLastStream && _handles.Count == 0;
            }

            if (disposeNow)
            {
                Dispose();
            }
        }

        private void WithFileHandle(long handle, Action<HandleInfo> function)
        {
            int Callback(HandleInfo handleInfo)
            {
                function(handleInfo);
                return 0;
            }

            WithFileHandle(handle, Callback);
        }

        private T WithFileHandle<T>(long handle, Func<HandleInfo, T> function)
        {
            T Callback()
            {
                HandleInfo handleInfo;
                lock (_handleTableLock)
                {
                    handleInfo = _handles[handle];
                }

                if (handleInfo.IsDirectory)
                {
                    // TODO: Better exception
                    throw new InvalidCastException();
                }

                return function(handleInfo);
            }

            return WithFileSystem(Callback);
        }

        private void WithFileSystem(Action function)
        {
            int Callback()
            {
                function();
                return 0;
            }

            WithFileSystem(Callback);
        }

        private T WithFileSystem<T>(Func<T> function)
        {
            if (!_fileSystemLock.Acquire())
            {
                throw new ObjectDisposedException(nameof(_wrappedFileSystem));
            }

            try
            {
                return function();
            }
            finally
            {
                _fileSystemLock.Release();
            }
        }
    }
}
