using System;
using System.Threading;

namespace Utils
{
    public class RemoveLock : IDisposable
    {
        private readonly CountdownEvent _event = new CountdownEvent(1);

        public bool Acquire()
        {
            return _event.TryAddCount();
        }

        public void Release()
        {
            _event.Signal();
        }

        public void ReleaseAndWait()
        {
            if (!_event.Signal(2))
            {
                _event.Wait();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _event.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
