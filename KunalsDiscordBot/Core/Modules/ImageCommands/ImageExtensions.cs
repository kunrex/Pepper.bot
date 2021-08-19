using System.Drawing;
using System.Drawing.Imaging;

namespace KunalsDiscordBot.Extensions
{
    public static partial class PepperBotExtensions
    {
        public static bool IsIndexed(this Image image) => image.PixelFormat == PixelFormat.Format1bppIndexed || image.PixelFormat == PixelFormat.Format4bppIndexed || image.PixelFormat == PixelFormat.Format8bppIndexed;
    }
}
