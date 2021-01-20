using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Utils
{
    public class Disposer:IDisposable
    {

        public Disposer(IDisposable disposable)
        {

            var destructor = disposable.GetType().GetMethod("Finalize",
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);
            managedDispose = disposable.Dispose;
            if (destructor != null)
            {
                unmanagedDispose = ()=>destructor.Invoke(disposable,new object[]{});
            }
        }

        public static IDisposable Create(Action managedDispose, Action unmanagedDispose = null)
        {
            return new Disposer(managedDispose, unmanagedDispose);
        }

        private readonly Action managedDispose;
        private readonly Action unmanagedDispose;
        private bool _disposed;

        public Disposer(Action managedDispose,Action unmanagedDispose = null)
        {
            this.managedDispose = managedDispose;
            this.unmanagedDispose = unmanagedDispose;
        }

        private void ReleaseUnmanagedResources()
        {
            if(unmanagedDispose!=null)
                unmanagedDispose();
        }

        private void ReleaseManagedResources()
        {
            if (managedDispose != null)
                managedDispose();
        }

        ~Disposer()
        {
            ReleaseUnmanagedResources();
        }

        public void Dispose()
        {
            managedDispose();
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

       
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                ReleaseManagedResources();
            }

            ReleaseUnmanagedResources();
            _disposed = true;
        }

      
    }

    public class AsyncDisposer : IAsyncDisposable,IDisposable
    {
        public AsyncDisposer(IAsyncDisposable disposable)
        {

            var destructor = disposable.GetType().GetMethod("Finalize",
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            managedDispose = new Task(async ()=> await disposable.DisposeAsync());

            if (destructor != null)
            {
                unmanagedDispose = () => destructor.Invoke(disposable, new object[] { });
            }
        }
        public static IAsyncDisposable Create(Task managedDispose, Action unmanagedDispose = null)
        {
            return new AsyncDisposer(managedDispose, unmanagedDispose);
        }

        private readonly Task managedDispose;
        private readonly Action unmanagedDispose;
        private bool _disposed;

        public AsyncDisposer(Task managedDispose, Action unmanagedDispose = null)
        {
            this.managedDispose = managedDispose;
            this.unmanagedDispose = unmanagedDispose;
        }


        private void ReleaseManagedResources()
        {
            ReleaseManagedResourcesAsync().Wait();
        }

        private void ReleaseUnmanagedResources()
        {
            if(_disposed)
                return;
            if (unmanagedDispose != null)
                unmanagedDispose();
        }

        private async Task ReleaseManagedResourcesAsync()
        {
            if (_disposed)
                return;
            if (managedDispose != null)
                await managedDispose;
        }

        ~AsyncDisposer()
        {
            ReleaseUnmanagedResources();
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            ReleaseManagedResources();
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
                return;

            await ReleaseManagedResourcesAsync();
            ReleaseUnmanagedResources();

            _disposed = true;
        }
    }
}