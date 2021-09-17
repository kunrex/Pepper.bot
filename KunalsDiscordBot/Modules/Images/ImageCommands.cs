using System;
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
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Services.Images;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Modules.ImageCommands;
using KunalsDiscordBot.Core.Attributes.ImageCommands;
using KunalsDiscordBot.Core.Configurations.Attributes;
using KunalsDiscordBot.Core.Modules.ImageCommands.Enums;

namespace KunalsDiscordBot.Modules.Images
{
    [Group("Image")]
    [Decor("Chartreuse", ":camera:")]
    [ModuleLifespan(ModuleLifespan.Transient), Description("Image Manipulation! Make memes with pre-built and and manipulate user profiles!")]
    [RequireBotPermissions(Permissions.SendMessages | Permissions.AttachFiles | Permissions.EmbedLinks), ConfigData(ConfigValueSet.Images)]
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
            if(string.IsNullOrEmpty(message))
            {
                await ctx.Channel.SendMessageAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush("Arial", editData.size[0], Color.Black, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(message, editData.x[0], editData.y[0], editData.length[0], editData.breadth[0], drawFont, drawBrush);

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
            if (string.IsNullOrEmpty(message))
            {
                await ctx.Channel.SendMessageAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush("Arial", editData.size[0], Color.Black, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(message, editData.x[0], editData.y[0], editData.length[0], editData.breadth[0], drawFont, drawBrush);

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
            if (string.IsNullOrEmpty(message))
            {
                await ctx.Channel.SendMessageAsync("At least give me a valid sentence");
                return;
            }

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
            if (string.IsNullOrEmpty(message))
            {
                await ctx.Channel.SendMessageAsync("At least give me a valid sentence");
                return;
            }

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

            using (var images = service.DownLoadImages(new TupleBag<string, int>(new List<(string, int)>
            {
                (ctx.Member.AvatarUrl, 3 ),
                (other.AvatarUrl, 1 )
            })))
            {
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
                }
            }
        }

        [Command("Yesno")]
        [Description("Yes, no")]
        [WithFile("yesno.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task YesNo(CommandContext ctx, [RemainingText] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                await ctx.Channel.SendMessageAsync("At least give me a valid sentence");
                return;
            }

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
            if (string.IsNullOrEmpty(message))
            {
                await ctx.Channel.SendMessageAsync("At least give me a valid sentence");
                return;
            }

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
        [Description("Inverts an user avatur")]
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

        [Command("Pixelate")]
        [Description("Pixelate an user avatur")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Pixelate(CommandContext ctx, int scale, DiscordMember member = null)
        {
            if(scale < 0 || scale > 15)
            {
                await ctx.Channel.SendMessageAsync("How about you limit the scale from 0 => 15 next time?");
                return;
            }

            member = member == null ? ctx.Member : member;

            using (var client = new WebClient())
            {
                using (var graphicalImage = new ImageGraphic(new MemoryStream(client.DownloadData(member.AvatarUrl))))
                {
                    await graphicalImage.Resize(600, 600);
                    await graphicalImage.Pixelate(scale);

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                  .WithFiles(new Dictionary<string, Stream>() { { $"Pixelated_{ctx.Member.DisplayName}.png", ms } })
                                  .WithReply(ctx.Message.Id)
                                  .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Blur")]
        [Description("Blurs an user avatur")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Blur(CommandContext ctx, int scale, DiscordMember member = null)
        {
            if (scale < 0 || scale > 10)
            {
                await ctx.Channel.SendMessageAsync("How about you limit the scale from 0 => 10 next time?");
                return;
            }

            member = member == null ? ctx.Member : member;

            using (var client = new WebClient())
            {
                using (var graphicalImage = new ImageGraphic(new MemoryStream(client.DownloadData(member.AvatarUrl))))
                {
                    await graphicalImage.Resize(600, 600);
                    await graphicalImage.Blur(scale);

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                  .WithFiles(new Dictionary<string, Stream>() { { $"Blur_{ctx.Member.DisplayName}.png", ms } })
                                  .WithReply(ctx.Message.Id)
                                  .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("ColorScale")]
        [Description("ColorScale an user avatur")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task ColorScale(CommandContext ctx, DiscordMember member = null, Colors color = Colors.Red)
        {
            member = member == null ? ctx.Member : member;

            using (var client = new WebClient())
            {
                using (var graphicalImage = new ImageGraphic(new MemoryStream(client.DownloadData(member.AvatarUrl))))
                {
                    await graphicalImage.Resize(600, 600);
                    await graphicalImage.ColorScale(color);

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                  .WithFiles(new Dictionary<string, Stream>() { { $"Colorscaled_{ctx.Member.DisplayName}.png", ms } })
                                  .WithReply(ctx.Message.Id)
                                  .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("ColorScale")]
        [Description("ColorScale an user avatur")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task ColorScale(CommandContext ctx, Colors color = Colors.Red)
        {
            using (var client = new WebClient())
            {
                using (var graphicalImage = new ImageGraphic(new MemoryStream(client.DownloadData(ctx.Member.AvatarUrl))))
                {
                    await graphicalImage.Resize(600, 600);
                    await graphicalImage.ColorScale(color);

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                  .WithFiles(new Dictionary<string, Stream>() { { $"Colorscaled_{ctx.Member.DisplayName}.png", ms } })
                                  .WithReply(ctx.Message.Id)
                                  .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Avatar")]
        [Description("Displays an users avatar")]
        [Cooldown(1, 5, CooldownBucketType.User)]
        public async Task Avatar(CommandContext ctx, DiscordMember member = null) => await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
        {
            ImageUrl = member == null ? ctx.Member.AvatarUrl : member.AvatarUrl,
            Color = ModuleInfo.Color
        }.WithFooter($"Requested by: {ctx.Member.DisplayName}"));

        [Command("GrayScale")]
        [Description("GrayScales an user avatur")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task GrayScale(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            using (var client = new WebClient())
            {
                using (var graphicalImage = new ImageGraphic(new MemoryStream(client.DownloadData(member.AvatarUrl))))
                {
                    await graphicalImage.Resize(600, 600);
                    await graphicalImage.GrayScale();

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                  .WithFiles(new Dictionary<string, Stream>() { { $"Grayscaled_{ctx.Member.DisplayName}.png", ms } })
                                  .WithReply(ctx.Message.Id)
                                  .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Imagify")]
        [Description("Combines multiple image manipulation functions on an user avatur")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Imagify(CommandContext ctx, bool invert, Deforms deform, int scale)
        {
            if(scale < 0 || scale > 10)
            {
                await ctx.Channel.SendMessageAsync("How about you keep the scale withing 0 and 10 next time?");
                return;
            }

            using (var client = new WebClient())
            {
                using (var graphicalImage = new ImageGraphic(new MemoryStream(client.DownloadData(ctx.Member.AvatarUrl))))
                {
                    await graphicalImage.Resize(600, 600);

                    if (invert)
                        await graphicalImage.Invert();

                    switch (deform)
                    {
                        case Deforms.Pixelate:
                            await graphicalImage.Pixelate(scale);
                            break;
                        case Deforms.Blur:
                            await graphicalImage.Blur(scale);
                            break;
                    }

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                  .WithFiles(new Dictionary<string, Stream>() { { $"Imagified_{ctx.Member.DisplayName}.png", ms } })
                                  .WithReply(ctx.Message.Id)
                                  .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Imagify")]
        [Description("Combines multiple image manipulation functions on an user avatur")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Imagify(CommandContext ctx, DiscordMember member, bool invert, Deforms deform, int scale)
        {
            if (scale < 0 || scale > 10)
            {
                await ctx.Channel.SendMessageAsync("How about you keep the scale withing 0 and 10 next time?");
                return;
            }

            using (var client = new WebClient())
            {
                using (var graphicalImage = new ImageGraphic(new MemoryStream(client.DownloadData(member.AvatarUrl))))
                {
                    await graphicalImage.Resize(600, 600);

                    if (invert)
                        await graphicalImage.Invert();

                    switch (deform)
                    {
                        case Deforms.Pixelate:
                            await graphicalImage.Pixelate(scale);
                            break;
                        case Deforms.Blur:
                            await graphicalImage.Blur(scale);
                            break;
                    }

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                  .WithFiles(new Dictionary<string, Stream>() { { $"Imagified_{ctx.Member.DisplayName}.png", ms } })
                                  .WithReply(ctx.Message.Id)
                                  .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Imagify")]
        [Description("Combines multiple image manipulation functions on an user avatur")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Imagify(CommandContext ctx, bool invert, Deforms deform, int scale, ColorScales color)
        {
            if (scale < 0 || scale > 10)
            {
                await ctx.Channel.SendMessageAsync("How about you keep the scale withing 0 and 10 next time?");
                return;
            }

            Colors colorScaleColor = Colors.Red;

            if (color != ColorScales.GreyScale)
            {
                await ctx.RespondAsync("What color would you like to colorize to?");
                var result = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.Member.Id, TimeSpan.FromSeconds(10));

                if (result.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("well you didn't reply so :shrug:");
                    return;
                }

                colorScaleColor = (Colors)await ctx.CommandsNext.ConvertArgument<Colors>(result.Result.Content, ctx);
            }

            using (var client = new WebClient())
            {
                using (var graphicalImage = new ImageGraphic(new MemoryStream(client.DownloadData(ctx.Member.AvatarUrl))))
                {
                    await graphicalImage.Resize(600, 600);

                    if (invert)
                        await graphicalImage.Invert();

                    switch(deform)
                    {
                        case Deforms.Pixelate:
                            await graphicalImage.Pixelate(scale);
                            break;
                        case Deforms.Blur:
                            await graphicalImage.Blur(scale);
                            break;
                    }

                    switch (color)
                    {
                        case ColorScales.GreyScale:
                            await graphicalImage.GrayScale();
                            break;
                        case ColorScales.ColorScale:
                            await graphicalImage.ColorScale(colorScaleColor);
                            break;
                    }

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                  .WithFiles(new Dictionary<string, Stream>() { { $"Imagified_{ctx.Member.DisplayName}.png", ms } })
                                  .WithReply(ctx.Message.Id)
                                  .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Imagify")]
        [Description("Combines multiple image manipulation functions on an user avatur")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Imagify(CommandContext ctx, DiscordMember member, bool invert, Deforms deform, int scale, ColorScales color)
        {
            if (scale < 0 || scale > 10)
            {
                await ctx.Channel.SendMessageAsync("How about you keep the scale withing 0 and 10 next time?");
                return;
            }

            Colors colorScaleColor = Colors.Red;

            if (color != ColorScales.GreyScale)
            {
                await ctx.RespondAsync("What color would you like to colorize to?");
                var result = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.Member.Id, TimeSpan.FromSeconds(10));

                if (result.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("well you didn't reply so :shrug:");
                    return;
                }

                colorScaleColor = (Colors)await ctx.CommandsNext.ConvertArgument<Colors>(result.Result.Content, ctx);
            }

            using (var client = new WebClient())
            {
                using (var graphicalImage = new ImageGraphic(new MemoryStream(client.DownloadData(member.AvatarUrl))))
                {
                    await graphicalImage.Resize(600, 600);

                    if (invert)
                        await graphicalImage.Invert();

                    switch (deform)
                    {
                        case Deforms.Pixelate:
                            await graphicalImage.Pixelate(scale);
                            break;
                        case Deforms.Blur:
                            await graphicalImage.Blur(scale);
                            break;
                    }

                    switch (color)
                    {
                        case ColorScales.GreyScale:
                            await graphicalImage.GrayScale();
                            break;
                        case ColorScales.ColorScale:
                            await graphicalImage.ColorScale(colorScaleColor);
                            break;
                    }

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                  .WithFiles(new Dictionary<string, Stream>() { { $"Imagified_{ctx.Member.DisplayName}.png", ms } })
                                  .WithReply(ctx.Message.Id)
                                  .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Combine")]
        [Description("Combine 2 user profiles"), Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Combine(CommandContext ctx, DiscordMember baseProfile, DiscordMember additiveProfile)
        {
            using (var images = service.DownLoadImages(new TupleBag<string, int>(new List<(string, int)>
            {
                (baseProfile.AvatarUrl, 1),
                (additiveProfile.AvatarUrl, 1),
            })))
            {
                for (int i = 0; i < images.Count; i++)
                    await images[i].Resize(600, 600);

                await images[1].SetOpcaity(150);
                await images[0].DrawImage(images[1], 0, 0, new RectangleF(0, 0, images[1].Width, images[1].Height), GraphicsUnit.Pixel);

                using (var ms = await images[0].ToMemoryStream())
                    await new DiscordMessageBuilder()
                              .WithFiles(new Dictionary<string, Stream>() { { $"Combined.png", ms } })
                              .WithReply(ctx.Message.Id)
                              .SendAsync(ctx.Channel);
            }
        }

        [Command("Sleep")]
        [Description("when you're sleeping and your brain remembers something")]
        [WithFile("sleep.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Sleep(CommandContext ctx, [RemainingText] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                await ctx.Channel.SendMessageAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush("Arial", editData.size[0], Color.Black, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(message, editData.x[0], editData.y[0], editData.length[0], editData.breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("ChangeMyMind")]
        [Description("Try changing it")]
        [WithFile("changeMyMind.jpeg")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task ChangeMyMind(CommandContext ctx, [RemainingText] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                await ctx.Channel.SendMessageAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush("Arial", editData.size[0], Color.Black, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(message, editData.x[0], editData.y[0], editData.length[0], editData.breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("DinoJoke")]
        [Description("bad joke go brrr")]
        [WithFile("dinoJoke.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task DinoJoke(CommandContext ctx, [RemainingText] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                await ctx.Channel.SendMessageAsync("At least give me a valid sentence");
                return;
            }

            string[] sentences = message.Split(',');
            if (sentences.Length < 2)
                sentences = new[] { $"{ctx.Member.DisplayName} used this command", "But they didn't split the sentences with commas" };

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

        [Command("Wanted")]
        [Description("Dead or alive")]
        [WithFile("wanted.jpg")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Wanted(CommandContext ctx, DiscordMember other = null)
        {
            other = other == null ? ctx.Member : other;

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var images = service.DownLoadImages(new TupleBag<string, int>(new List<(string, int)>
            {
                (other.AvatarUrl, 1 )
            })))
            {
                using (var graphicalImage = new ImageGraphic(filePath))
                {
                    await images[0].Resize(editData.size[0], editData.size[0]);

                    RectangleF srcRect = new RectangleF(0, 0, editData.size[0], editData.size[0]);
                    await graphicalImage.DrawImage(images[0], editData.x[0], editData.y[0], srcRect, GraphicsUnit.Pixel);

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                     .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                     .WithReply(ctx.Message.Id)
                                     .SendAsync(ctx.Channel);
                }
            }
        }
    }
}
