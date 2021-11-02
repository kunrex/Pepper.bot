using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using ImageMagick;

using KunalsDiscordBot.Core.Modules.ImageCommands.Enums;
using System.Drawing.Imaging;

namespace KunalsDiscordBot.Core.Modules.ImageCommands
{
    public class ImageCollection : CustomDisposable, IEnumerable<ImageGraphic>, IImage
    {
        private List<ImageGraphic> Images { get; set; }

        public int Width { get => Images[0].Width; }
        public int Height { get => Images[0].Height; }

        public int FrameCount { get; private set; }

        public ImageCollection()
        {
            Images = new List<ImageGraphic>();
        }

        public ImageCollection(Stream stream)
        {
            Images = new List<ImageGraphic>();

            using (var bmap = Bitmap.FromStream(stream))
                CollectFrames(bmap);
        }

        public ImageCollection(string file)
        {
            Images = new List<ImageGraphic>();

            using (var bmap = Bitmap.FromFile(file))
                CollectFrames(bmap);
        }

        private void CollectFrames(Image bmap)
        {
            var dimension = new FrameDimension(bmap.FrameDimensionsList[0]);
            FrameCount = bmap.GetFrameCount(dimension);

            for (int i = 0; i < FrameCount; i++)
            {
                bmap.SelectActiveFrame(dimension, i);

                var image = new Bitmap(bmap.Width, bmap.Height);
                using (var graphics = Graphics.FromImage(image))
                    graphics.DrawImage(bmap, new Point(0, 0));

                Images.Add(new ImageGraphic(image));
            }
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

        public async Task DrawString(string message, int x, int y, Font font, Brush brush)
        {
            foreach (var image in Images)
                await image.DrawString(message, x, y, font, brush);
        }

        public async Task DrawString(string message, int x, int y, int length, int breadth, Font font, Brush brush)
        {
            foreach (var image in Images)
                await image.DrawString(message, x, y, length, breadth, font, brush);
        }

        public async Task DrawImage(ImageGraphic other, int x, int y, RectangleF rect, GraphicsUnit unit)
        {
            foreach (var image in Images)
                await image.DrawImage(other, x, y, rect, unit);
        }

        public async Task DrawImageRotated(ImageGraphic other, int angle, int x, int y, RectangleF rect, GraphicsUnit unit)
        {
            foreach (var image in Images)
                await image.DrawImageRotated(other, angle, x, y, rect, unit);
        }

        public async Task Rotate(int angle)
        {
            foreach (var image in Images)
                await image.Rotate(angle);
        }

        public async Task SetOpcaity(int opacity)
        {
            foreach (var image in Images)
                await image.SetOpcaity(opacity);
        }

        public async Task Resize(int width, int height)
        {
            foreach (var image in Images)
                await image.Resize(width, height);
        }

        public async Task RotateFlip(RotateFlipType rotateFlipType)
        {
            foreach (var image in Images)
                await image.RotateFlip(rotateFlipType);
        }

        public async Task Invert()
        {
            foreach (var image in Images)
                await image.Invert();
        }

        public async Task GrayScale()
        {
            foreach (var image in Images)
                await image.GrayScale();
        }

        public async Task ColorScale(Colors color)
        {
            foreach (var image in Images)
                await image.ColorScale(color);
        }

        public async Task ColorScale(int r, int g, int b)
        {
            foreach (var image in Images)
                await image.ColorScale(r, g, b);
        }

        public async Task ClearTransperency()
        {
            foreach (var image in Images)
                await image.ClearTransperency();
        }

        public async Task Blur(int blurSize)
        {
            foreach (var image in Images)
                await image.Blur(blurSize);
        }

        public async Task Pixelate(int pixelSize)
        {
            foreach (var image in Images)
                await image.Pixelate(pixelSize);
        }

        public Task<MemoryStream> ToMemoryStream()
        {
            using (var collection = new MagickImageCollection())
            {
                collection.Add((MagickImage)Images[0]);
                collection[0].AnimationDelay = 100;
                collection[0].AnimationIterations = 0;

                for(int i = 1; i < Images.Count;i++)
                {
                    collection.Add((MagickImage)Images[i]);
                    collection[i].AnimationDelay = 100;
                }

                collection.Optimize();

                var stream = new MemoryStream();
                collection.Write(stream, MagickFormat.Gif);
                stream.Position = 0;

                foreach (var image in collection)
                    image.Dispose();

                return Task.FromResult(stream);
            }
        }

        public async Task<MemoryStream> ToMemoryStream(int delay, bool clearTransperency = true)
        {
            using (var collection = new MagickImageCollection())
            {
                if(clearTransperency)
                    await ClearTransperency();

                collection.Add((MagickImage)Images[0]);
                collection[0].AnimationDelay = delay;
                collection[0].AnimationIterations = 0;

                for (int i = 1; i < Images.Count; i++)
                {
                    collection.Add((MagickImage)Images[i]);
                    collection[i].AnimationDelay = delay;
                }

                collection.Optimize();

                var stream = new MemoryStream();
                collection.Write(stream, MagickFormat.Gif);
                stream.Position = 0;

                foreach (var image in collection)
                    image.Dispose();

                return stream;
            }
        }
    }
}
