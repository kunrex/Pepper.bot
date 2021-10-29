using System;
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
                case Colors.Magenta:
                    return Color.FromArgb(color.R, 0, color.B);
                case Colors.Yellow:
                    return Color.FromArgb(color.R, color.G, 0);
                case Colors.Cyan:
                    return Color.FromArgb(0, color.G, color.B);
                case Colors.Purple:
                    return Color.FromArgb(color.R / 2, 0, color.B / 2);
                case Colors.Orange:
                    return Color.FromArgb(color.R, Math.Clamp(color.G - 74, 0, 255), 0);
                case Colors.Brown:
                    return Color.FromArgb(Math.Clamp(color.R - 63, 0, 255), Math.Clamp(color.G / 2 - 53, 0, 255), 0);
                case Colors.Silver:
                    return Color.FromArgb(Math.Clamp(color.R - 115, 0, 255), Math.Clamp(color.R - 63, 0, 255), Math.Clamp(color.R - 63, 0, 255));
                case Colors.Gold:
                    return Color.FromArgb(color.R, Math.Clamp(color.G - 40, 0 , 255), 0);
                case Colors.CandyRed:
                    return Color.FromArgb(color.R, 0, Math.Clamp(color.B / 2 - 113, 0, 255));
                default:
                    return Color.FromArgb(0, color.G, 0);
            }
        }
    }
}
