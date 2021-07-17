using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using DSharpPlus.CommandsNext;
using Imgur;

using KunalsDiscordBot.Modules.Images;
using KunalsDiscordBot.Core.Attributes.ImageCommands;
using KunalsDiscordBot.Core.Exceptions.ImageCommands;
using System.Net.Http;
using Imgur.API.Authentication;
using Imgur.API.Endpoints;

namespace KunalsDiscordBot.Services.Images
{
    public class ImageService : IImageService
    {
        private readonly EditData[] edits = JsonSerializer.Deserialize<ImageData>(File.ReadAllText(Path.Combine("Modules", "Images", "ImageData.json"))).Edits;

        public EditData GetEditData(string fileName) => edits.FirstOrDefault(x => x.fileName == fileName);

        public string GetFileByCommand(in CommandContext ctx)
        {
            var attribute = ctx.Command.CustomAttributes.FirstOrDefault(x => x is WithFileAttribute);

            if (attribute == null)
                throw new WithFileAttributeMissingException(ctx.Command.Name);

            return ((WithFileAttribute)attribute).fileName;
        }

        public void GetImages(string filePath, out Image image, out Image saveImage)
        {
            image = Bitmap.FromFile(filePath);
            saveImage = Bitmap.FromFile(filePath);
        }

        public string[] GetSentences(string sentence, int num)
        {
            if (num == 1)
                return new[] { sentence };

            string currentWord = string.Empty;
            string[] sentences = new string[num];
            int index = 0;

            foreach (var character in sentence)
            {
                switch (character)
                {
                    case ',':
                        if (index == num - 1)
                            break;

                        sentences[index] = currentWord;
                        currentWord = string.Empty;
                        index++;
                        break;
                    default:
                        currentWord += character;
                        break;
                }
            }
            sentences[num - 1] = currentWord;

            return sentences;
        }

        public void GetFontAndBrush(string fontName, int fontSize, Color fontColor, out Font font, out SolidBrush brush)
        {
            font = new Font(fontName, fontSize);
            brush = new SolidBrush(fontColor);
        }

        public Image ResizeImage(Image image, int width, int height)
        {
            var rect = new Rectangle(0, 0, width, height);
            var newImage = new Bitmap(width, height);

            newImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return newImage;
        }

        public List<Image> GetImages(Dictionary<string, int> urls)
        {
            List<Image> images = new List<Image>();
            var client = new WebClient();

            foreach(var url in urls)
            {
                var image = Image.FromStream(new MemoryStream(client.DownloadData(url.Key)));

                for (int i = 0; i < url.Value; i++)
                    images.Add(image);
            }

            return images;
        }

        public Bitmap GetNewBitmap(int height, int width) => new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

        public Image GetImageFromFile(string filePath)
        {
            if (!Directory.Exists(filePath))
                throw new Exception("Path given does not exist");

            byte[] data = File.ReadAllBytes(filePath);

            using (MemoryStream stream = new MemoryStream(data))
            {
               return Bitmap.FromStream(stream);
            }
        }
    }
}
