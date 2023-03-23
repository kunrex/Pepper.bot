using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;

using ImageMagick;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Modules.ImageCommands.Enums;

namespace KunalsDiscordBot.Core.Modules.ImageCommands
{
    public class ImageGraphic : CustomDisposable, IImage
    {
        public Image image { get; private set; }

        public int Width => image.Width;
        public int Height => image.Height;

        public ImageGraphic(string filePath)
        {
            var bmap = Bitmap.FromFile(filePath);
            if (bmap.IsIndexed())
            {
                image = new Bitmap(bmap.Width, bmap.Height);
                using (Graphics g = Graphics.FromImage(image))
                    g.DrawImage(bmap, new Point(0, 0));

                bmap.Dispose();
            }
            else
                image = bmap;
        }

        public ImageGraphic(Stream stream)
        {
            var bmap = Bitmap.FromStream(stream);
            if (bmap.IsIndexed())
            {
                image = new Bitmap(bmap.Width, bmap.Height);
                using (Graphics g = Graphics.FromImage(image))
                    g.DrawImage(bmap, new Point(0, 0));

                bmap.Dispose();
            }
            else
                image = bmap;
        }

        public ImageGraphic(Image imageToSet) => image = imageToSet;

        public Task DrawString(string message, int x, int y, int length, int breadth, Font font, Brush brush)
        {
            using (var graphics = Graphics.FromImage(image))
                graphics.DrawString(message, font, brush, new RectangleF(x, y, length, breadth));

            return Task.CompletedTask;
        }

        public Task DrawString(string message, int x, int y, Font font, Brush brush)
        {
            using (var graphics = Graphics.FromImage(image))
                graphics.DrawString(message, font, brush, new PointF(x, y));

            return Task.CompletedTask;
        }

        public Task DrawString(string message, int x, int y, Font font, Brush brush, Color outlineColor)
        {
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (var path = new GraphicsPath())
                {
                    path.AddString(message, font.FontFamily, (int)FontStyle.Regular, font.Size, new Point(x, y), new StringFormat());

                    graphics.FillPath(brush, path);
                    using (var pen = new Pen(outlineColor, 3))
                        graphics.DrawPath(pen, path);
                }
            }

            return Task.CompletedTask;
        }

        public Task DrawString(string message, int x, int y, int length, int breadth, Font font, Brush brush, Color outlineColor)
        {
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (var path = new GraphicsPath())
                {
                    path.AddString(message, font.FontFamily, (int)FontStyle.Regular, font.Size, new RectangleF(x, y, length, breadth), new StringFormat());

                    graphics.FillPath(brush, path);
                    using (var pen = new Pen(outlineColor))
                        graphics.DrawPath(pen, path);
                }
            }

            return Task.CompletedTask;
        }

        public Task DrawImageRotated(ImageGraphic other, int angle, int x, int y, RectangleF rect, GraphicsUnit unit)
        {
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.RotateTransform(angle);

                graphics.DrawImage(other.image, x, y, rect, unit);
            }

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

            for (int i = 0; i < bmap.Width; i++)
                for (int k = 0; k < bmap.Height; k++)
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
            image = ResizeImage(width, height);

            return Task.CompletedTask;
        }

        private Image ResizeImage(int width, int height)
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

            image.Dispose();
            return newImage;
        }

        public Task Rotate(int angle)
        {
            image = RotateImage(angle);

            return Task.CompletedTask;
        }

        private Image RotateImage(int angle)
        {
            Bitmap rotatedImage = new Bitmap(image.Width, image.Height);
            rotatedImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(rotatedImage))
            {
                graphics.TranslateTransform(image.Width / 2, image.Height / 2);
                graphics.RotateTransform(angle);
                graphics.TranslateTransform(-image.Width / 2, -image.Height / 2);

                graphics.DrawImage(image, new Point(0, 0));
            }

            image.Dispose();
            return rotatedImage;
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

            image = new Bitmap(bmap);
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

        public Task ColorScale(int r, int g, int b)
        {
            Bitmap bmap = (Bitmap)image;
            Color c;

            r = 255 - r;
            g = 255 - g;
            b = 255 - b;

            for (int i = 0; i < bmap.Width; i++)
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    Color newColor = Color.FromArgb(Math.Clamp(c.R - r, 0, 255), Math.Clamp(c.G - g, 0, 255), Math.Clamp(c.B - b, 0, 255));

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

        public Task ClearTransperency()
        {
            Bitmap nonTransperentImage = new Bitmap(image.Width, image.Height);
            nonTransperentImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(nonTransperentImage))
            {
                graphics.Clear(Color.Black);
                graphics.DrawImage(image, new Point(0, 0));
            }

            image.Dispose();
            image = nonTransperentImage;

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

            for (int x = 0; x < bmap.Width; x += pixelSize)
                for (int y = 0; y < bmap.Height; y += pixelSize)
                {
                    int red = 0, blue = 0, green = 0, pixelCount = 0;

                    for (int i = x; i < x + pixelSize && i < bmap.Width; i++)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (image != null)
                    image.Dispose();
            }

            base.Dispose(disposing);
        }

        public static explicit operator MagickImage(ImageGraphic graphic) => new MagickImage(graphic.ToMemoryStream().GetAwaiter().GetResult());
    }
}