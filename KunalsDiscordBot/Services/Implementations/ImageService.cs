using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;

using DSharpPlus.CommandsNext;

using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Core.Modules.ImageCommands;
using KunalsDiscordBot.Core.Attributes.ImageCommands;
using KunalsDiscordBot.Core.Exceptions.ImageCommands;

namespace KunalsDiscordBot.Services.Images
{
    public class ImageService : IImageService
    {
        private readonly EditData[] edits;

        public ImageService(PepperConfigurationManager configManager) => edits = configManager.ImageData.edits;

        public EditData GetEditData(string fileName) => edits.FirstOrDefault(x => x.FileName == fileName);

        public string GetFileByCommand(in Command command)
        {
            var attribute = command.CustomAttributes.FirstOrDefault(x => x is WithFileAttribute);

            if (attribute == null)
                throw new WithFileAttributeMissingException(command.Name);

            return ((WithFileAttribute)attribute).fileName;
        }

        public void GetFontAndBrush(string fontName, int fontSize, Color fontColor, int fontStyles, out Font font, out SolidBrush brush)
        {
            font = new Font(fontName, fontSize, (FontStyle)fontStyles);
            brush = new SolidBrush(fontColor);
        }

        public ImageCollection DownLoadImages(TupleBag<string, int> urls)
        {
            var images = new ImageCollection();

            using (var client = new WebClient())
            {
                for(int i=0;i<urls.Count;i++)
                {
                    var image = new ImageGraphic(new MemoryStream(client.DownloadData(urls[i].Item1)));

                    for (int k = 0; k < urls[i].Item2; k++)
                        images.Add(image);
                }
            }

            return images;
        }
    }
}
