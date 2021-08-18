using System.Drawing;
using System.Collections.Generic;

using DSharpPlus.CommandsNext;

using KunalsDiscordBot.Core.Modules.ImageCommands;

namespace KunalsDiscordBot.Services.Images
{
    public interface IImageService
    {
        public void GetFontAndBrush(string fontName, int fontSize, Color fontColor, out Font font, out SolidBrush brush);

        public EditData GetEditData(string fileName);

        public string GetFileByCommand(in CommandContext ctx);
        public List<ImageGraphic> GetImages(Dictionary<string, int> urls);
    }
}
