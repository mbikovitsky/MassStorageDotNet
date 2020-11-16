using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using DiscUtils;
using DokanNet;
using ThreadSafeDiscUtils;
using FileAccess = DokanNet.FileAccess;
using FileStream = ThreadSafeDiscUtils.FileStream;

namespace DokanDiscUtils
{
    public class DokanOperations : IDokanOperations
    {
        private readonly object _createLock = new object();
        private readonly HandleBasedFileSystem _fileSystem;

        private DokanOperations(DiscFileSystem fileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

#pragma warning disable CA2000 // Dispose objects before losing scope
            _fileSystem = HandleBasedFileSystem.Create(ThreadSafeFileSystem.Create(fileSystem));
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        public static DokanOperations Create(DiscFileSystem fileSystem)
        {
            try
            {
                return new DokanOperations(fileSystem);
            }
            catch
            {
                fileSystem?.Dispose();
                throw;
            }
        }

        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options,
            FileAttributes attributes, IDokanFileInfo info)
        {
            if (fileName == null || info == null)
            {
                return NtStatus.InvalidParameter;
            }

            System.IO.FileAccess mappedAccess;
            if ((access & FileAccess.MaximumAllowed) != 0)
            {
                mappedAccess = _fileSystem.CanWrite ? System.IO.FileAccess.ReadWrite : System.IO.FileAccess.Read;
            }
            else
            {
                mappedAccess = MapAccess(access);
                if ((mappedAccess & System.IO.FileAccess.Write) != 0 && !_fileSystem.CanWrite)
                {
                    return NtStatus.AccessDenied;
                }
            }

            lock (_createLock)
            {
                if (!info.IsDirectory)
                {
                    if (_fileSystem.DirectoryExists(fileName))
                    {
                        info.IsDirectory = true;
                    }
                }

                if (info.IsDirectory)
                {
                    if (_fileSystem.FileExists(fileName))
                    {
                        return NtStatus.NotADirectory;
                    }

                    var directoryExists = false;
                    if (mode == FileMode.Create || mode == FileMode.OpenOrCreate)
                    {
                        directoryExists = _fileSystem.DirectoryExists(fileName);
                    }

                    try
                    {
                        info.Context = _fileSystem.OpenDirectory(fileName, mode);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        return NtStatus.ObjectNameNotFound;
                    }
                    catch (IOException)
                    {
                        // Can't use ObjectNameCollision, since that has a special meaning for Dokan.
                        return NtStatus.Unsuccessful;
                    }

                    return directoryExists ? NtStatus.ObjectNameCollision : NtStatus.Success;
                }

                var fileExists = false;
                if (mode == FileMode.Create || mode == FileMode.OpenOrCreate)
                {
                    fileExists = _fileSystem.FileExists(fileName);
                }

                try
                {
                    info.Context = _fileSystem.OpenFile(fileName, mode, mappedAccess, share);
                }
                catch (UnauthorizedAccessException)
                {
                    return NtStatus.SharingViolation;
                }
                catch (FileNotFoundException)
                {
                    return NtStatus.ObjectNameNotFound;
                }
                catch (IOException)
                {
                    // Can't use ObjectNameCollision, since that has a special meaning for Dokan.
                    return NtStatus.Unsuccessful;
                }

                return fileExists ? NtStatus.ObjectNameCollision : NtStatus.Success;
            }
        }

        private static System.IO.FileAccess MapAccess(FileAccess access)
        {
            System.IO.FileAccess result = 0;

            if ((access & FileAccess.ReadData) != 0 ||
                (access & FileAccess.ReadExtendedAttributes) != 0 ||
                (access & FileAccess.Execute) != 0 ||
                (access & FileAccess.ReadAttributes) != 0 ||
                (access & FileAccess.ReadPermissions) != 0 ||
                (access & FileAccess.Synchronize) != 0 ||
                (access & FileAccess.AccessSystemSecurity) != 0 ||
                (access & FileAccess.GenericAll) != 0 ||
                (access & FileAccess.GenericExecute) != 0 ||
                (access & FileAccess.GenericRead) != 0)
            {
                result |= System.IO.FileAccess.Read;
            }

            if ((access & FileAccess.WriteData) != 0 ||
                (access & FileAccess.AppendData) != 0 ||
                (access & FileAccess.WriteExtendedAttributes) != 0 ||
                (access & FileAccess.DeleteChild) != 0 ||
                (access & FileAccess.WriteAttributes) != 0 ||
                (access & FileAccess.Delete) != 0 ||
                (access & FileAccess.ChangePermissions) != 0 ||
                (access & FileAccess.SetOwnership) != 0 ||
                (access & FileAccess.AccessSystemSecurity) != 0 ||
                (access & FileAccess.GenericAll) != 0 ||
                (access & FileAccess.GenericWrite) != 0)
            {
                result |= System.IO.FileAccess.Write;
            }

            return result;
        }

        public void Cleanup(string fileName, IDokanFileInfo info)
        {
        }

        public void CloseFile(string fileName, IDokanFileInfo info)
        {
            (info?.Context as FileStream)?.Dispose();
            (info?.Context as DirectoryReference)?.Dispose();
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            if (buffer == null || info == null)
            {
                bytesRead = 0;
                return NtStatus.InvalidParameter;
            }

            var stream = (FileStream) info.Context;

            stream.Position = offset;
            bytesRead = stream.Read(buffer, 0, buffer.Length);

            return NtStatus.Success;
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            if (buffer == null || info == null)
            {
                bytesWritten = 0;
                return NtStatus.InvalidParameter;
            }

            var stream = (FileStream) info.Context;

            stream.Position = offset;
            stream.Write(buffer, 0, buffer.Length);
            bytesWritten = (int) (stream.Position - offset);

            return NtStatus.Success;
        }

        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            if (info == null)
            {
                return NtStatus.InvalidParameter;
            }

            ((FileStream) info.Context).Flush();
            return NtStatus.Success;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            fileInfo = GetFileInformation(fileName);
            return NtStatus.Success;
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
        {
            files = FindFiles(fileName).ToList();
            return NtStatus.Success;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            files = FindFilesWithPattern(fileName, searchPattern).ToList();
            return NtStatus.Success;
        }

        private IEnumerable<FileInformation> FindFiles(string fileName)
        {
            return FindFilesWithPattern(fileName, "*");
        }

        private IEnumerable<FileInformation> FindFilesWithPattern(string fileName, string searchPattern)
        {
            return _fileSystem
                .GetFileSystemEntries(fileName)
                .Select(s => s.Remove(0, fileName.Length))
                .Where(filename =>
                    DokanHelper.DokanIsNameInExpression(searchPattern, filename, !_fileSystem.IsCaseSensitive()))
                .Select(GetFileInformation);
        }

        private FileInformation GetFileInformation(string fileName)
        {
            return GetFileInformation(fileName, null);
        }

        private FileInformation GetFileInformation(string fileName, FileStream? stream)
        {
            return new FileInformation
            {
                FileName = fileName,
                Attributes = _fileSystem.GetAttributes(fileName),
                CreationTime = _fileSystem.GetCreationTime(fileName),
                LastAccessTime = _fileSystem.GetLastAccessTime(fileName),
                LastWriteTime = _fileSystem.GetLastWriteTime(fileName),
                Length = stream?.Length ?? _fileSystem.GetFileLength(fileName)
            };
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            _fileSystem.SetAttributes(fileName, attributes);
            return NtStatus.Success;
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime,
            IDokanFileInfo info)
        {
            if (creationTime != null)
            {
                _fileSystem.SetCreationTime(fileName, creationTime.Value);
            }

            if (lastAccessTime != null)
            {
                _fileSystem.SetLastAccessTime(fileName, lastAccessTime.Value);
            }

            if (lastWriteTime != null)
            {
                _fileSystem.SetLastWriteTime(fileName, lastWriteTime.Value);
            }

            return NtStatus.Success;
        }

        public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            // Assume deletion is fine, this is best-effort anyway.
            return NtStatus.Success;
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            // Assume deletion is fine, this is best-effort anyway.
            return _fileSystem.GetFileSystemEntries(fileName).Any() ? NtStatus.DirectoryNotEmpty : NtStatus.Success;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            if (oldName == null || newName == null || info == null)
            {
                return NtStatus.InvalidParameter;
            }

            if (info.Context == null)
            {
                _fileSystem.MoveDirectory(oldName, newName);
            }
            else
            {
                _fileSystem.MoveFile(oldName, newName, replace);
            }

            return NtStatus.Success;
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            if (info?.Context == null)
            {
                return NtStatus.InvalidParameter;
            }

            ((FileStream) info.Context).SetLength(length);
            return NtStatus.Success;
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            return SetEndOfFile(fileName, length, info);
        }

        public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            return NtStatus.NotImplemented;
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            return NtStatus.NotImplemented;
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes,
            IDokanFileInfo info)
        {
            freeBytesAvailable = _fileSystem.AvailableSpace;
            totalNumberOfBytes = _fileSystem.Size;
            totalNumberOfFreeBytes = freeBytesAvailable;

            return NtStatus.Success;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName,
            out uint maximumComponentLength, IDokanFileInfo info)
        {
            volumeLabel = _fileSystem.VolumeLabel;
            fileSystemName = _fileSystem.FriendlyName;

            features = FileSystemFeatures.CasePreservedNames;

            if (_fileSystem.IsCaseSensitive())
            {
                features |= FileSystemFeatures.CaseSensitiveSearch;
            }

            if (!_fileSystem.CanWrite)
            {
                features |= FileSystemFeatures.ReadOnlyVolume;
            }

            maximumComponentLength = 255; // TODO: Get the correct value

            return NtStatus.Success;
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
            IDokanFileInfo info)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            security = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            return NtStatus.NotImplemented;
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
            IDokanFileInfo info)
        {
            return NtStatus.NotImplemented;
        }

        public NtStatus Mounted(IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            _fileSystem.Unmount(true);
            _fileSystem.DisposeOnLastStream();
            return NtStatus.Success;
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            streams = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            return NtStatus.NotImplemented;
        }
    }
}
