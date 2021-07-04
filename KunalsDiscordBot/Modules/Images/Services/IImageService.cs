using System;
using System.Collections.Generic;
using System.Drawing;
using DSharpPlus.CommandsNext;

namespace KunalsDiscordBot.Modules.Images.Services
{
    public interface IImageService
    {
        public Image ResizeImage(Image image, int width, int height);
        public void GetImages(string filePath, out Image image, out Image saveImage);

        public void GetFontAndBrush(string fontName, int fontSize, Color fontColor, out Font font, out SolidBrush brush);

        public EditData GetEditData(string fileName);
        public string[] GetSentences(string sentence, int num);

        public string GetFileByCommand(in CommandContext ctx);
        public List<Image> GetImages(Dictionary<string, int> urls);
    }
}
