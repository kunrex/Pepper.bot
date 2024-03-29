﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules.ImageCommands
{
    public class ImageEnumerator : IEnumerator<ImageGraphic>
    {
        private readonly ImageCollection collection;
        public ImageCollection Collection { get => collection; }

        private int current { get; set; } = -1;

        public ImageEnumerator(ImageCollection _collection)
        {
            collection = _collection;
        }

        private ImageGraphic _Current
        {
            get
            {
                try
                {
                    return collection[current];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public object Current => _Current;

        ImageGraphic IEnumerator<ImageGraphic>.Current => _Current;

        public bool MoveNext()
        {
            current++;
            return current < collection.FrameCount;
        }

        public void Reset() => current = -1;

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
