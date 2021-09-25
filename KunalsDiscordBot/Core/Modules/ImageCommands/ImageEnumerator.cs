using System;
using System.Collections;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules.ImageCommands
{
    public class ImageEnumerator : IEnumerator
    {
        private readonly ImageCollection collection;
        public ImageCollection Collection { get => collection; }

        private int current { get; set; } = -1;

        public ImageEnumerator(ImageCollection _collection)
        {
            collection = _collection;
        }

        public object Current 
        {
            get
            {
                try
                {
                    return collection[current];
                }
                catch(IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public bool MoveNext()
        {
            current++;
            return current < collection.Count;
        }

        public void Reset() => current = -1;
    }
}
