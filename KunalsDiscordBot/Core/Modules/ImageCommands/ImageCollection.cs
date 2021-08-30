using System;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules.ImageCommands
{
    public class ImageCollection : CustomDisposable
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

        public void Add(ImageGraphic other) => Images.Add(other);

        public void Remove(ImageGraphic other) => Images.Remove(other);

        public void RemovAt(int index) => Images.RemoveAt(index);

        public void Clear() => Images = new List<ImageGraphic>();

        public ImageGraphic this[int index]
        {
            get => Images[index];
        }

        ~ImageCollection()
        {
           if(Images != null)
            foreach (var image in Images)
                image.Dispose();
        }
    }
}
