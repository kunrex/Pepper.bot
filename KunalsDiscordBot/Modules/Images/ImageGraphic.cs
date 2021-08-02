using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using KunalsDiscordBot.Extensions;

namespace KunalsDiscordBot.Modules.Images
{
    public class ImageGraphic : GraphicDisposable
    {
        public Image image { get; private set; }
        public Graphics graphics { get; private set; }

        public ImageGraphic(string filePath)
        {
            image = Bitmap.FromFile(filePath);
            graphics = Graphics.FromImage(image);
        }

        public ImageGraphic(Stream stream)
        {
            image = Bitmap.FromStream(stream);
            graphics = Graphics.FromImage(image);
        }

        public Task DrawString(string message, EditData data, int index, Font font, Brush brush)
        {
            graphics.DrawString(message, font, brush, new RectangleF(data.x[index], data.y[index], data.length[index], data.breadth[index]));

            return Task.CompletedTask;
        }

        public Task DrawImage(Image other, int x, int y, RectangleF rect, GraphicsUnit unit)
        {
            graphics.DrawImage(other, x, y, rect, unit);

            return Task.CompletedTask;
        }

        public async Task Resize(int width, int height) => await image.Resize(width, height);

        ~ImageGraphic()
        {
            image.Dispose();
            graphics.Dispose();
        }
    }
}
