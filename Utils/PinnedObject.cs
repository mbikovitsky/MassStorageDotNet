using System;
using System.Runtime.InteropServices;

namespace Utils
{
    /// <summary>
    /// An <see cref="IDisposable"/> wrapper around pinned <see cref="GCHandle"/>s.
    /// </summary>
    /// <typeparam name="T">Type of the object being pinned</typeparam>
    public class PinnedObject<T> : IDisposable where T : class
    {
        private GCHandle _handle;

        /// <summary>
        /// Constructs a new <see cref="PinnedObject{T}"/> that will pin the given object in memory.
        /// </summary>
        /// <param name="value">Object to pin</param>
        public PinnedObject(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            _handle = GCHandle.Alloc(value, GCHandleType.Pinned);
        }

        /// <summary>
        /// The unmanaged address of the pinned object.
        /// </summary>
        public IntPtr Address => _handle.AddrOfPinnedObject();

        private void ReleaseUnmanagedResources()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        ~PinnedObject()
        {
            Dispose(false);
        }
    }
}
