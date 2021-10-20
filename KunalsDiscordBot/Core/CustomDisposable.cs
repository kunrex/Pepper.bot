using System;
using System.Threading.Tasks;

namespace KunalsDiscordBot.Core
{
    public class CustomDisposable : IAsyncDisposable, IDisposable
    {
        protected bool disposed = false;

        public CustomDisposable() { }

        public void Dispose()
        {
            if (!disposed)
                Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => disposed = true;

        ~CustomDisposable() => Dispose(false);

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
