//System name spaces
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using KunalsDiscordBot.Attributes;


namespace KunalsDiscordBot.Modules.Images
{
    [Group("Image")]
    [Decor("Chartreuse", ":camera:")]
    public class ImageCommands : BaseCommandModule
    {
        private readonly EditData[] edits = JsonSerializer.Deserialize<ImageData>(File.ReadAllText(Path.Combine("Modules", "Images", "ImageData.json"))).Edits;
        private readonly string genralPath = Path.Combine("Modules", "Images", "Images");

        [Command("abandon")]
        [Description("Abandon meme")]
        public async Task Abandon(CommandContext ctx, [RemainingText] string message)
        { 
            string fileName = "abandon.png";
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = GetEditData(fileName);
            GetFontAndBrush("Arial", editData.size[0], Color.Black, out Font drawFont, out SolidBrush drawBrush);
            GetImages(filePath, out Image image, out Image saveImage);

            Graphics graphics = Graphics.FromImage(image);
            graphics.DrawString(message, drawFont, drawBrush, new PointF(editData.x[0], editData.y[0]));

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
        public async Task Violence(CommandContext ctx, [RemainingText] string message)
        {
            string fileName = "violence.png";
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = GetEditData(fileName);
            GetFontAndBrush("Arial", editData.size[0], Color.Black, out Font drawFont, out SolidBrush drawBrush);
            GetImages(filePath, out Image image, out Image saveImage);

            Graphics graphics = Graphics.FromImage(image);
            graphics.DrawString(message, drawFont, drawBrush, new PointF(editData.x[0], editData.y[0]));

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
        public async Task Right(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = await GetSentences(message, 3);    

            string fileName = "right.png";
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = GetEditData(fileName);
            GetImages(filePath, out Image image, out Image saveImage);

            Graphics graphics = Graphics.FromImage(image);

            for (int i = 0; i < sentences.Length; i++)
            {
                GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

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

        [Command("Yesno")]
        [Description("Yes, no")]
        public async Task YesNo(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = await GetSentences(message, 2);

            string fileName = "yesno.png";
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = GetEditData(fileName);
            GetImages(filePath, out Image image, out Image saveImage);

            Graphics graphics = Graphics.FromImage(image);

            for (int i = 0; i < sentences.Length; i++)
            {
                GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

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
        public async Task YesNoPewds(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = await GetSentences(message, 2);

            string fileName = "yesnoPewds.png";
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = GetEditData(fileName);
            GetImages(filePath, out Image image, out Image saveImage);

            Graphics graphics = Graphics.FromImage(image);

            for (int i = 0; i < sentences.Length; i++)
            {
                GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

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

        private async Task<string[]> GetSentences(string sentence, int num)
        {
            string currentWord = string.Empty;
            string[] sentences = new string[num];
            int index = 0;

            foreach (var character in sentence)
            {
                switch (character)
                {
                    case ',':
                        if (index ==  num - 1)
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

            await Task.CompletedTask;
            return sentences;
        }

        private EditData GetEditData(string fileName) => edits.First(x => x.fileName == fileName);

        private void GetImages(string filePath, out Image image, out Image saveImage)
        {
            image = Bitmap.FromFile(filePath);
            saveImage = Bitmap.FromFile(filePath);
        }

        private void GetFontAndBrush(string fontName, int fontSize, Color fontColor, out Font font, out SolidBrush brush)
        {
            font = new Font(fontName, fontSize);
            brush = new SolidBrush(fontColor);
        }

        private class ImageData
        {
            public EditData[] Edits { get; set; }
        }

        private class EditData
        {
            public string fileName { get; set; }
            public int[] x { get; set; }
            public int[] y { get; set; }
            public int[] size { get; set; }
        }
    }
}
