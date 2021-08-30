using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Modules.ImageCommands.Enums;

namespace KunalsDiscordBot.Core.Modules.ImageCommands
{
    public class ImageGraphic : CustomDisposable
    {
        public Image image { get; private set; }

        public int Width
        {
            get => image.Width;
        }

        public int Height
        {
            get => image.Height;
        }

        public ImageGraphic(string filePath) => image = Bitmap.FromFile(filePath);

        public ImageGraphic(Stream stream) => image = Bitmap.FromStream(stream);

        public Task DrawString(string message, int x, int y, int length, int breadth, Font font, Brush brush)
        {
            using(var graphics = Graphics.FromImage(image))
                graphics.DrawString(message, font, brush, new RectangleF(x, y, length, breadth));

            return Task.CompletedTask;
        }

        public Task DrawString(string message, int x, int y, Font font, Brush brush)
        {
            using (var graphics = Graphics.FromImage(image))
                graphics.DrawString(message, font, brush, new PointF(x, y));

            return Task.CompletedTask;
        }

        public Task DrawImage(ImageGraphic other, int x, int y, RectangleF rect, GraphicsUnit unit)
        {
            using (var graphics = Graphics.FromImage(image))
                graphics.DrawImage(other.image, x, y, rect, unit);

            return Task.CompletedTask;
        }

        public Task SetOpcaity(int opacity)
        {
            var bmap = (Bitmap)image;

            for(int i =0;i<bmap.Width;i++)
                for(int k = 0;k<bmap.Height;k++)
                {
                    var color = bmap.GetPixel(i, k);
                    var newColor = Color.FromArgb(opacity, color);

                    bmap.SetPixel(i, k, newColor);
                }

            image = new Bitmap(bmap);
            bmap.Dispose();

            return Task.CompletedTask;
        }

        public Task Resize(int width, int height)
        {
            image = ResizeImage(image, width, height);

            return Task.CompletedTask;
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            var rect = new Rectangle(0, 0, width, height);
            var newImage = new Bitmap(width, height);

            newImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return newImage;
        }

        public Task RotateFlip(RotateFlipType rotateFlipType)
        {
            image.RotateFlip(rotateFlipType);

            return Task.CompletedTask;
        }

        public Task Invert()
        {
            Bitmap bmap = (Bitmap)image;
            Color c;

            for (int i = 0; i < bmap.Width; i++)
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    bmap.SetPixel(i, j, Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B));
                }

            image =  new Bitmap(bmap);
            bmap.Dispose();

            return Task.CompletedTask;
        }

        public Task ColorScale(Colors color)
        {
            Bitmap bmap = (Bitmap)image;
            Color c;

            for (int i = 0; i < bmap.Width; i++)
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    Color newColor = c.ColorScale(color);
                    bmap.SetPixel(i, j, newColor);
                }

            image = new Bitmap(bmap);
            bmap.Dispose();

            return Task.CompletedTask;
        }

        public Task GrayScale()
        {
            Bitmap bmap = (Bitmap)image;
            Color c;

            for (int i = 0; i < bmap.Width; i++)
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    int grayScale = (int)((c.R * 0.3) + (c.G * 0.59) + (c.B * 0.11));
                    bmap.SetPixel(i, j, Color.FromArgb(c.A, grayScale, grayScale, grayScale));
                }

            image = new Bitmap(bmap);
            bmap.Dispose();

            return Task.CompletedTask;
        }

        public Task Blur(int blurSize)
        {
            Bitmap bmap = (Bitmap)image;
            Color c;

            for (int x = 0; x < bmap.Width; x++)
                for (int y = 0; y < bmap.Width; y++)
                {
                    int red = 0, blue = 0, green = 0, pixelCount = 0;

                    for (int i = x; i < (x + blurSize) && i < bmap.Width; i++)
                        for (int k = y; k < (y + blurSize) && k < bmap.Height; k++)
                        {
                            c = bmap.GetPixel(i, k);
                            red += c.R;
                            blue += c.B;
                            green += c.G;
                            pixelCount++;
                        }

                    red /= pixelCount;
                    blue /= pixelCount;
                    green /= pixelCount;

                    for (int i = x; i < x + blurSize && i < bmap.Width; i++)
                        for (int k = y; k < y + blurSize && k < bmap.Height; k++)
                            bmap.SetPixel(i, k, Color.FromArgb(red, green, blue));
                }

            image = new Bitmap(bmap);
            bmap.Dispose();

            return Task.CompletedTask;
        }

        public Task Pixelate(int pixelSize)
        {
            Bitmap bmap = (Bitmap)image;
            Color c;

            for (int x = 0; x < bmap.Width; x+= pixelSize)
                for (int y = 0; y < bmap.Width; y+= pixelSize)
                {
                    int red = 0, blue = 0, green = 0, pixelCount = 0;

                    for(int i = x; i< x + pixelSize && i < bmap.Width; i++)
                        for (int k = y; k < y + pixelSize && k < bmap.Height; k++)
                        {
                            c = bmap.GetPixel(x, y);
                            red += c.R;
                            blue += c.B;
                            green += c.G;
                            pixelCount++;
                        }

                    red /= pixelCount;
                    blue /= pixelCount;
                    green /= pixelCount;

                    for (int i = x; i < x + pixelSize && i < bmap.Width; i++)
                        for (int k = y; k < y + pixelSize && k < bmap.Height; k++)
                            bmap.SetPixel(i, k, Color.FromArgb(red, green, blue));
                }

            image = new Bitmap(bmap);
            bmap.Dispose();

            return Task.CompletedTask;
        }

        public Task<MemoryStream> ToMemoryStream()
        {
            var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            return Task.FromResult(ms);
        }

        ~ImageGraphic()
        {
            if(image != null)
                image.Dispose();
        }
    }
}
