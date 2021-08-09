﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KunalsDiscordBot.Modules.Images
{
    public class GraphicDisposable : IAsyncDisposable, IDisposable
    {
        protected bool disposed { get; set; } = false;
        private Component component { get; set; } = new Component();

        public GraphicDisposable() { }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    component.Dispose();

                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        ~GraphicDisposable() => Dispose(disposing: false);

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}