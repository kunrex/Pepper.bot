using System.Drawing;
using System.Drawing.Imaging;

using KunalsDiscordBot.Core.Modules.ImageCommands.Enums;

namespace KunalsDiscordBot.Extensions
{
    public static partial class PepperBotExtensions
    {
        public static bool IsIndexed(this Image image) => image.PixelFormat == PixelFormat.Format1bppIndexed || image.PixelFormat == PixelFormat.Format4bppIndexed || image.PixelFormat == PixelFormat.Format8bppIndexed;

        public static Color ColorScale(this Color color, Colors colorScale)
        {
            switch (colorScale)
            {
                case Colors.Red:
                    return Color.FromArgb(color.R, 0, 0);
                case Colors.Blue:
                    return Color.FromArgb(0, 0 , color.B);
                default:
                    return Color.FromArgb(0, color.G, 0);
            }
        }
    }
}
