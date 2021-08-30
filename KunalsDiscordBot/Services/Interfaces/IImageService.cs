using System.Drawing;
using System.Collections.Generic;

using DSharpPlus.CommandsNext;

using KunalsDiscordBot.Core.Modules.ImageCommands;

namespace KunalsDiscordBot.Services.Images
{
    public interface IImageService
    {
        public EditData GetEditData(string fileName);

        public string GetFileByCommand(in Command ctx);

        public void GetFontAndBrush(string fontName, int fontSize, Color fontColor, out Font font, out SolidBrush brush);

        public ImageCollection DownLoadImages(TupleBag<string, int> urls);
    }
}
