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

using KunalsDiscordBot.Modules.Images.Services;
using KunalsDiscordBot.Core.Attributes.ImageCommands;

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
            string[] sentences = service.GetSentences(message, 1);

            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);
            service.GetImages(filePath, out Image image, out Image saveImage);

            Graphics graphics = Graphics.FromImage(image);

            for (int i = 0; i < sentences.Length; i++)
            {
                service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                graphics.DrawString(sentences[i], drawFont, drawBrush, new PointF(editData.x[i], editData.y[i]));
            }

            image.Save(filePath);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var msg = await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, fs } })
                        .SendAsync(ctx.Channel);

                fs.Close();
            }

            saveImage.Save(filePath);
        }

        [Command("violence")]
        [Description("Violence meme")]
        [WithFile("violence.png")]
        public async Task Violence(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = service.GetSentences(message, 1);

            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);
            service.GetImages(filePath, out Image image, out Image saveImage);

            Graphics graphics = Graphics.FromImage(image);

            for (int i = 0; i < sentences.Length; i++)
            {
                service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                graphics.DrawString(sentences[i], drawFont, drawBrush, new PointF(editData.x[i], editData.y[i]));
            }

            image.Save(filePath);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var msg = await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, fs } })
                        .SendAsync(ctx.Channel);

                fs.Close();
            }

            saveImage.Save(filePath);
        }

        [Command("billy")]
        [Description("Billy what have you done meme")]
        [WithFile("billy.png")]
        public async Task Billy(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = service.GetSentences(message, 1);

            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);
            service.GetImages(filePath, out Image image, out Image saveImage);

            Graphics graphics = Graphics.FromImage(image);

            for (int i = 0; i < sentences.Length; i++)
            {
                service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                graphics.DrawString(sentences[i], drawFont, drawBrush, new PointF(editData.x[i], editData.y[i]));
            }

            image.Save(filePath);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var msg = await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, fs } })
                        .SendAsync(ctx.Channel);

                fs.Close();
            }

            saveImage.Save(filePath);
        }

        [Command("Right")]
        [Description("For the better right? use `,` to seperate the sentences")]
        [WithFile("right.png")]
        public async Task Right(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = service.GetSentences(message, 3);    

            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);
            service.GetImages(filePath, out Image image, out Image saveImage);

            Graphics graphics = Graphics.FromImage(image);

            for (int i = 0; i < sentences.Length; i++)
            {
                service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                graphics.DrawString(sentences[i], drawFont, drawBrush, new PointF(editData.x[i], editData.y[i]));
            }

            image.Save(filePath);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var msg = await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, fs } })
                        .SendAsync(ctx.Channel);

                fs.Close();
            }

            saveImage.Save(filePath);
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

            service.GetImages(filePath, out Image image, out Image saveImage);

            Graphics graphics = Graphics.FromImage(image);

            for (int i = 0; i < images.Count; i++)
            {
                RectangleF srcRect = new RectangleF(0, 0, images[i].Width, images[i].Width);

                graphics.DrawImage(service.ResizeImage(images[i], editData.size[i], editData.size[i]), editData.x[i], editData.y[i], srcRect, GraphicsUnit.Pixel);
            }

            image.Save(filePath);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var msg = await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, fs } })
                        .SendAsync(ctx.Channel);

                fs.Close();
            }

            saveImage.Save(filePath);
        }

        [Command("Yesno")]
        [Description("Yes, no")]
        [WithFile("yesno.png")]
        public async Task YesNo(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = service.GetSentences(message, 2);

            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);
            service.GetImages(filePath, out Image image, out Image saveImage);

            Graphics graphics = Graphics.FromImage(image);

            for (int i = 0; i < sentences.Length; i++)
            {
                service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                graphics.DrawString(sentences[i], drawFont, drawBrush, new PointF(editData.x[i], editData.y[i]));
            }

            image.Save(filePath);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var msg = await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, fs } })
                        .SendAsync(ctx.Channel);

                fs.Close();
            }

            saveImage.Save(filePath);
        }

        [Command("YesnoPewds")]
        [Description("Yes, no but pewdipie style")]
        [WithFile("yesnoPewds.png")]
        public async Task YesNoPewds(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = service.GetSentences(message, 2);

            string fileName = service.GetFileByCommand(ctx);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);
            service.GetImages(filePath, out Image image, out Image saveImage);

            Graphics graphics = Graphics.FromImage(image);

            for (int i = 0; i < sentences.Length; i++)
            {
                service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                graphics.DrawString(sentences[i], drawFont, drawBrush, new PointF(editData.x[i], editData.y[i]));
            }

            image.Save(filePath);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var msg = await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, fs } })
                        .SendAsync(ctx.Channel);

                fs.Close();
            }

            saveImage.Save(filePath);
        }
    }
}
