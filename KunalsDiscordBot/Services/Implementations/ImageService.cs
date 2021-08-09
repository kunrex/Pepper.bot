﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using DSharpPlus.CommandsNext;

using KunalsDiscordBot.Modules.Images;
using KunalsDiscordBot.Core.Attributes.ImageCommands;
using KunalsDiscordBot.Core.Exceptions.ImageCommands;
using KunalsDiscordBot.Core.Configurations;

namespace KunalsDiscordBot.Services.Images
{
    public class ImageService : IImageService
    {
        private readonly EditData[] edits;
        public ImageService(PepperConfigurationManager configManager) => edits = configManager.imageData.edits;

        public EditData GetEditData(string fileName) => edits.FirstOrDefault(x => x.fileName == fileName);

        public string GetFileByCommand(in CommandContext ctx)
        {
            var attribute = ctx.Command.CustomAttributes.FirstOrDefault(x => x is WithFileAttribute);

            if (attribute == null)
                throw new WithFileAttributeMissingException(ctx.Command.Name);

            return ((WithFileAttribute)attribute).fileName;
        }

        public void GetFontAndBrush(string fontName, int fontSize, Color fontColor, out Font font, out SolidBrush brush)
        {
            font = new Font(fontName, fontSize);
            brush = new SolidBrush(fontColor);
        }

        public List<ImageGraphic> GetImages(Dictionary<string, int> urls)
        {
            List<ImageGraphic> images = new List<ImageGraphic>();
            using (var client = new WebClient())
            {
                foreach (var url in urls)
                {
                    var image = new ImageGraphic(new MemoryStream(client.DownloadData(url.Key)));

                    for (int i = 0; i < url.Value; i++)
                        images.Add(image);
                }
            }

            return images;
        }
    }
}