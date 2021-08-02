//System name spaces
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Net;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using KunalsDiscordBot.Attributes;

using KunalsDiscordBot.Services.Images;
using KunalsDiscordBot.Core.Attributes.ImageCommands;
using System.Drawing.Imaging;
using KunalsDiscordBot.Extensions;

namespace KunalsDiscordBot.Modules.Images
{
    [Group("Image")]
    [Decor("Chartreuse", ":camera:")]
    public class ImageCommands : BaseCommandModule
    {
        private readonly IImageService service;

        public ImageCommands(IImageService _service) => service = _service;

        [Command("abandon")]
        [Description("Abandon meme")]
        [WithFile("abandon.png")]
        public async Task Abandon(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = new[] { message };

            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    //have to change this to load using the config
                    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData, i, drawFont, drawBrush);
                }

                using (var ms = new MemoryStream())
                {
                    graphicalImage.image.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    await new DiscordMessageBuilder()
                             .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                             .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("violence")]
        [Description("Violence meme")]
        [WithFile("violence.png")]
        public async Task Violence(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = new[] { message };

            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData, i, drawFont, drawBrush);
                }

                using (var ms = new MemoryStream())
                {
                    graphicalImage.image.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    await new DiscordMessageBuilder()
                             .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                             .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("billy")]
        [Description("Billy what have you done meme")]
        [WithFile("billy.png")]
        public async Task Billy(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = new[] { message };
            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData, i, drawFont, drawBrush);
                }

                using (var ms = new MemoryStream())
                {
                    graphicalImage.image.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    await new DiscordMessageBuilder()
                             .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                             .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Right")]
        [Description("For the better right? use `,` to seperate the sentences")]
        [WithFile("right.png")]
        public async Task Right(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = message.Split(',');
            if (sentences.Length < 3)
                sentences = new[] { "Im going to use this command", "You're gonna split the 3 sentences with ,'s right?", "You will split them right?" };

            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData, i, drawFont, drawBrush);
                }

                using (var ms = new MemoryStream())
                {
                    graphicalImage.image.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    await new DiscordMessageBuilder()
                             .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                             .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Brother")]
        [Description("Why do you hate me brother?")]
        [WithFile("brother.jpeg")]
        public async Task Brother(CommandContext ctx, DiscordMember other)
        {
            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            List<Image> images = service.GetImages(new Dictionary<string, int>
            {
                {ctx.Member.AvatarUrl, 3 },
                {other.AvatarUrl, 1 }
            });

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < images.Count; i++)
                {
                    RectangleF srcRect = new RectangleF(0, 0, images[i].Width, images[i].Width);

                    await graphicalImage.DrawImage(images[i], editData.x[i], editData.y[i], srcRect, GraphicsUnit.Pixel);
                }

                using (var ms = new MemoryStream())
                {
                    graphicalImage.image.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    await new DiscordMessageBuilder()
                             .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                             .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Yesno")]
        [Description("Yes, no")]
        [WithFile("yesno.png")]
        public async Task YesNo(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = message.Split(',');
            if (sentences.Length < 2)
                sentences = new[] { "Splitting sentences using comas", "Using the command anyway" };

            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData, i, drawFont, drawBrush);
                }

                using (var ms = new MemoryStream())
                {
                    graphicalImage.image.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    await new DiscordMessageBuilder()
                             .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                             .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("YesnoPewds")]
        [Description("Yes, no but pewdipie style")]
        [WithFile("yesnoPewds.png")]
        public async Task YesNoPewds(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = message.Split(',');
            if (sentences.Length < 2)
                sentences = new[] { "Splitting sentences using comas", "Using the command anyway" };

            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath)) 
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData, i, drawFont, drawBrush);
                }

                using (var ms = new MemoryStream())
                {
                    graphicalImage.image.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    await new DiscordMessageBuilder()
                             .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                             .SendAsync(ctx.Channel);
                }
            }
        }
    }
}
