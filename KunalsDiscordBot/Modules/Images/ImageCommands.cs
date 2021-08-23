using System.IO;
using System.Linq;
using System.Net;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Services.Images;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Modules.ImageCommands;
using KunalsDiscordBot.Core.Attributes.ImageCommands;
using KunalsDiscordBot.Core.Configurations.Attributes;

namespace KunalsDiscordBot.Modules.Images
{
    [Group("Image")]
    [Decor("Chartreuse", ":camera:")]
    [ModuleLifespan(ModuleLifespan.Transient), Description("Image Manipulation! Make memes with pre-built and more!")]
    [RequireBotPermissions(Permissions.SendMessages | Permissions.AttachFiles | Permissions.EmbedLinks),ConfigData(ConfigValueSet.Images)]
    public class ImageCommands : PepperCommandModule
    {
        public override PepperCommandModuleInfo ModuleInfo { get; protected set; } 

        private readonly IImageService service;

        public ImageCommands(IImageService _service, IModuleService moduleService)
        {
            service = _service;
            ModuleInfo = moduleService.ModuleInfo[ConfigValueSet.Images];
        }

        [Command("abandon")]
        [Description("Abandon meme")]
        [WithFile("abandon.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Abandon(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = new[] { message };

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    //have to change this to load using the config
                    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData.x[i], editData.y[i], editData.length[i], editData.breadth[i], drawFont, drawBrush);
                }

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("violence")]
        [Description("Violence meme")]
        [WithFile("violence.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Violence(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = new[] { message };

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData.x[i], editData.y[i], editData.length[i], editData.breadth[i], drawFont, drawBrush);
                }

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("billy")]
        [Description("Billy what have you done meme")]
        [WithFile("billy.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Billy(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = new[] { message };
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData.x[i], editData.y[i], editData.length[i], editData.breadth[i], drawFont, drawBrush);
                }

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("Right")]
        [Description("For the better right? use `,` to seperate the sentences")]
        [WithFile("right.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Right(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = message.Split(',');
            if (sentences.Length < 3)
                sentences = new[] { "Im going to use this command", "You're gonna split the 3 sentences with ,'s right?", "You will split them right?" };

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData.x[i], editData.y[i], editData.length[i], editData.breadth[i], drawFont, drawBrush);
                }

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("Brother")]
        [Description("Why do you hate me brother?")]
        [WithFile("brother.jpeg")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Brother(CommandContext ctx, DiscordMember other)
        {
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            List<ImageGraphic> images = service.DownLoadImages(new TupleBag<string, int>(new List<(string, int)>
            {
                (ctx.Member.AvatarUrl, 3 ),
                (other.AvatarUrl, 1 )
            }));

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < images.Count; i++)
                {
                    await images[i].Resize(editData.size[i], editData.size[i]);
 
                    RectangleF srcRect = new RectangleF(0, 0, editData.size[i], editData.size[i]);                  
                    await graphicalImage.DrawImage(images[i], editData.x[i], editData.y[i], srcRect, GraphicsUnit.Pixel);
                }

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);

                foreach (var image in images)
                    image.Dispose();
            }
        }

        [Command("Yesno")]
        [Description("Yes, no")]
        [WithFile("yesno.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task YesNo(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = message.Split(',');
            if (sentences.Length < 2)
                sentences = new[] { "Splitting sentences using comas", "Using the command anyway" };

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData.x[i], editData.y[i], editData.length[i], editData.breadth[i], drawFont, drawBrush);
                }

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("YesnoPewds")]
        [Description("Yes, no but pewdipie style")]
        [WithFile("yesnoPewds.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task YesNoPewds(CommandContext ctx, [RemainingText] string message)
        {
            string[] sentences = message.Split(',');
            if (sentences.Length < 2)
                sentences = new[] { "Splitting sentences using comas", "Using the command anyway" };

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath)) 
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData.x[i], editData.y[i], editData.length[i], editData.breadth[i], drawFont, drawBrush);
                }

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("Invert")]
        [Description("Inverts a user avatur url")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Invert(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            using (var client = new WebClient())
            {
                using (var graphicalImage = new ImageGraphic(new MemoryStream(client.DownloadData(member.AvatarUrl))))
                {
                    await graphicalImage.Invert();

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                  .WithFiles(new Dictionary<string, Stream>() { { $"{string.Concat(ctx.Member.DisplayName.Reverse())}.png", ms } })
                                  .WithReply(ctx.Message.Id)
                                  .SendAsync(ctx.Channel);
                }
            }
        }
    }
}
