using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using KunalsDiscordBot.Extensions;

namespace KunalsDiscordBot.Modules.Images
{
    public class ImageGraphic : GraphicDisposable
    {
        public Image image { get; private set; }
        public Graphics graphics { get; private set; }

        public int width
        {
            get => image.Width;
        }

        public int height
        {
            get => image.Height;
        }

        public ImageGraphic(string filePath)
        {
            image = Bitmap.FromFile(filePath);
            graphics = Graphics.FromImage(image);
        }

        public ImageGraphic(Stream stream)
        {
            image = Bitmap.FromStream(stream);

            if (image.IsIndexed())
                using (var tempBitmap = new Bitmap(image.Width, image.Height))
                {
                    using (Graphics g = Graphics.FromImage(tempBitmap))
                        g.DrawImage(image, 0, 0);

                    image = new Bitmap(tempBitmap);
                    graphics = Graphics.FromImage(image);
                }
            else
                graphics = Graphics.FromImage(image);
        }

        public Task DrawString(string message, EditData data, int index, Font font, Brush brush)
        {
            graphics.DrawString(message, font, brush, new RectangleF(data.x[index], data.y[index], data.length[index], data.breadth[index]));

            return Task.CompletedTask;
        }

        public Task DrawImage(ImageGraphic other, int x, int y, RectangleF rect, GraphicsUnit unit)
        {
            graphics.DrawImage(other.image, x, y, rect, unit);

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

        public void RotateFlip(RotateFlipType rotateFlipType) => image.RotateFlip(rotateFlipType);

        public void Invert()
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
        }

        public MemoryStream ToMemoryStream()
        {
            var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            return ms;
        }

        ~ImageGraphic()
        {
            if(image != null)
                image.Dispose();

            if(graphics != null)
                graphics.Dispose();
        }
    }
}
