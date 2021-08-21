using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KunalsDiscordBot.Core
{
    public class CustomDisposable : IAsyncDisposable, IDisposable
    {
        protected bool disposed { get; set; } = false;

        public CustomDisposable() { }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        ~CustomDisposable() => Dispose(false);

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
