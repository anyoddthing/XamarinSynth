using System;

namespace SynthTest
{
    public abstract class DisposableObject : IDisposable
    {
        public bool Disposed { get; private set;}      

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DisposableObject()
        {
            if (!Disposed)
            {
                Logger.Error(GetType()).Error("WARNING: Object finalized without being disposed!");
            }

            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                }

                DisposeUnmanagedResources();
                Disposed = true;
            }
        }

        protected virtual void DisposeManagedResources() { }
        protected virtual void DisposeUnmanagedResources() { }
    }
}

