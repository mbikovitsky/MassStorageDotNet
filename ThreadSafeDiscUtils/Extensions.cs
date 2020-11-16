using System;
using DiscUtils;

namespace ThreadSafeDiscUtils
{
    internal static class Extensions
    {
        public static bool ArePathsEquivalent(this IFileSystem fileSystem, string path1, string path2)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            if (path1 == null)
            {
                throw new ArgumentNullException(nameof(path1));
            }

            if (path2 == null)
            {
                throw new ArgumentNullException(nameof(path2));
            }

            if (!fileSystem.Exists(path1) || !fileSystem.Exists(path2))
            {
                return false;
            }

            return fileSystem switch
            {
                IWindowsFileSystem system => (system.GetFileId(path1) == system.GetFileId(path2)),
                IUnixFileSystem system => (system.GetUnixFileInfo(path1).Inode == system.GetUnixFileInfo(path2).Inode),
                _ => throw new NotSupportedException("Filesystem type not supported")
            };
        }

        public static bool IsCaseSensitive(this IFileSystem fileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            return fileSystem switch
            {
                IWindowsFileSystem _ => false,
                IUnixFileSystem _ => true,
                _ => throw new NotSupportedException("Filesystem type not supported")
            };
        }
    }
}
