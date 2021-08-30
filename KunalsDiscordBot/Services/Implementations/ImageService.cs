using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using DSharpPlus.CommandsNext;

using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Core.Modules.ImageCommands;
using KunalsDiscordBot.Core.Attributes.ImageCommands;
using KunalsDiscordBot.Core.Exceptions.ImageCommands;

namespace KunalsDiscordBot.Services.Images
{
    public class ImageService : IImageService
    {
        private readonly EditData[] edits;

        public ImageService(PepperConfigurationManager configManager) => edits = configManager.imageData.edits;

        public EditData GetEditData(string fileName) => edits.FirstOrDefault(x => x.fileName == fileName);

        public string GetFileByCommand(in Command command)
        {
            var attribute = command.CustomAttributes.FirstOrDefault(x => x is WithFileAttribute);

            if (attribute == null)
                throw new WithFileAttributeMissingException(command.Name);

            return ((WithFileAttribute)attribute).fileName;
        }

        public void GetFontAndBrush(string fontName, int fontSize, Color fontColor, out Font font, out SolidBrush brush)
        {
            font = new Font(fontName, fontSize);
            brush = new SolidBrush(fontColor);
        }

        public ImageCollection DownLoadImages(TupleBag<string, int> urls)
        {
            var images = new ImageCollection();

            using (var client = new WebClient())
            {
                for(int i=0;i<urls.Count;i++)
                {
                    var image = new ImageGraphic(new MemoryStream(client.DownloadData(urls[i].Key)));

                    for (int k = 0; k < urls[i].Value; k++)
                        images.Add(image);
                }
            }

            return images;
        }
    }
}
