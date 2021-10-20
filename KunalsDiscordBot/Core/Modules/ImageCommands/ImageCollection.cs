using System;
using System.Collections;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules.ImageCommands
{
    public class ImageCollection : CustomDisposable, IEnumerable<ImageGraphic>
    {
        private List<ImageGraphic> Images { get; set; }

        public ImageCollection()
        {
            Images = new List<ImageGraphic>();
        }

        public int Count
        {
            get => Images.Count;
        }

        public ImageGraphic this[int index]
        {
            get => Images[index];
        }

        public void Add(ImageGraphic other) => Images.Add(other);

        public void Remove(ImageGraphic other) => Images.Remove(other);

        public void RemovAt(int index) => Images.RemoveAt(index);

        public void Clear() => Images = new List<ImageGraphic>();

        public void DisposeAndClear()
        {
            foreach (var image in Images)
                image.Dispose();

            Images = new List<ImageGraphic>();
        }

        private IEnumerator<ImageGraphic> _GetEnumerator() => new ImageEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => _GetEnumerator();
        public IEnumerator<ImageGraphic> GetEnumerator() => _GetEnumerator();

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                if (Images != null)
                {
                    foreach (var image in Images)
                        image.Dispose();

                    Images = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
