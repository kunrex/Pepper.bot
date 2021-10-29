using System;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;

using KunalsDiscordBot.Core.Modules.ImageCommands.Enums;

namespace KunalsDiscordBot.Core.Modules.ImageCommands
{
    public interface IImage
    {
        public int Width { get; }
        public int Height { get; }

        public Task DrawString(string message, int x, int y, int length, int breadth, Font font, Brush brush);
        public Task DrawString(string message, int x, int y, Font font, Brush brush);

        public Task DrawImage(ImageGraphic other, int x, int y, RectangleF rect, GraphicsUnit unit);
        public Task DrawImageRotated(ImageGraphic other, int angle, int x, int y, RectangleF rect, GraphicsUnit unit);

        public Task Rotate(int angle);
        public Task Resize(int width, int height);
        public Task RotateFlip(RotateFlipType rotateFlipType);

        public Task Invert();
        public Task GrayScale();
        public Task SetOpcaity(int opacity);
        public Task ColorScale(Colors color);

        public Task Blur(int blurSize);
        public Task Pixelate(int pixelSize);

        public Task<MemoryStream> ToMemoryStream();
    }
}
