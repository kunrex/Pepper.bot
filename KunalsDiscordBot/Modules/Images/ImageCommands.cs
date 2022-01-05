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

using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Services.Images;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Core.DiscordModels;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Modules.ImageCommands;
using KunalsDiscordBot.Core.Attributes.ImageCommands;
using KunalsDiscordBot.Core.Configurations.Attributes;
using KunalsDiscordBot.Core.Modules.ImageCommands.Enums;
using KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics;

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
            if (string.IsNullOrEmpty(message))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(message, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

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
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(message, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

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
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string[] sentences = new[] { message };
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(message, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

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
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string[] sentences = message.Split(',').Select(x => x.Trim()).ToArray();
            if (sentences.Length < 3)
                sentences = new[] { "Im going to use this command", "You're gonna split the 3 sentences with ,'s right?", "You will split them right?" };

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush(editData.Font, editData.Size[i], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData.X[i], editData.Y[i], editData.Length[i], editData.Breadth[i], drawFont, drawBrush);
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
                    for (int i = 0; i < images.FrameCount; i++)
                    {
                        await images[i].Resize(editData.Size[i], editData.Size[i]);

                        RectangleF srcRect = new RectangleF(0, 0, editData.Size[i], editData.Size[i]);
                        await graphicalImage.DrawImage(images[i], editData.X[i], editData.Y[i], srcRect, GraphicsUnit.Pixel);
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
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string[] sentences = message.Split(',').Select(x => x.Trim()).ToArray();
            if (sentences.Length < 2)
                sentences = new[] { "Splitting sentences using comas", "Using the command anyway" };

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush(editData.Font, editData.Size[i], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData.X[i], editData.Y[i], editData.Length[i], editData.Breadth[i], drawFont, drawBrush);
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
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string[] sentences = message.Split(',').Select(x => x.Trim()).ToArray();
            if (sentences.Length < 2)
                sentences = new[] { "Splitting sentences using comas", "Using the command anyway" };

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush(editData.Font, editData.Size[i], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData.X[i], editData.Y[i], editData.Length[i], editData.Breadth[i], drawFont, drawBrush);
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
                    await graphicalImage.Resize(600, 600);
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
            if (scale < 0 || scale > 15)
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
        public async Task ColorScale(CommandContext ctx, DiscordMember member, Colors color = Colors.Red)
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

        [Command("ColorScale")]
        [Description("ColorScale an user avatur")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task ColorScale(CommandContext ctx, int red, int green, int blue, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            using (var client = new WebClient())
            {
                using (var graphicalImage = new ImageGraphic(new MemoryStream(client.DownloadData(member.AvatarUrl))))
                {
                    await graphicalImage.Resize(600, 600);
                    await graphicalImage.ColorScale(red, green, blue);

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                  .WithFiles(new Dictionary<string, Stream>() { { $"Colorscaled_{member.DisplayName}.png", ms } })
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
        public async Task Imagify(CommandContext ctx, bool invert, Deforms deform, ColorScales color)
        {
            int scale = 1;
            Colors colorScaleColor = Colors.Red;
            var messageData = new MessageData
            {
                Reply = true,
                ReplyId = ctx.Message.Id
            };

            if (deform != Deforms.None)
            {
                var messageStep = new MessageStep("What scale would do you want for the deformation", "", 10)
                    .WithMesssageData(messageData);
                var result = await messageStep.ProcessStep(ctx.Channel, ctx.Member, ctx.Client, false);

                scale = int.TryParse(result.Result, out var x) ? int.Parse(result.Result) : -1;

                if (scale < 0 || scale > 10)
                {
                    await ctx.RespondAsync("How about you keep the scale within 0 and 10 next time?");
                    return;
                }
            }

            if (color == ColorScales.ColorScale)
            {
                var replyStep = new ReplyStep("What color would you like to colorize to?", "", 10, Enum.GetNames(typeof(Colors)).ToList())
                     .WithMesssageData(messageData);
                var result = await replyStep.ProcessStep(ctx.Channel, ctx.Member, ctx.Client, false);

                if (!result.WasCompleted)
                {
                    await ctx.RespondAsync("well I didn't get a valid reply so ¯\\_(ツ)_/¯");
                    return;
                }

                colorScaleColor = (Colors)await ctx.CommandsNext.ConvertArgument<Colors>(result.Result, ctx);
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

                    switch (color)
                    {
                        case ColorScales.GrayScale:
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
        public async Task Imagify(CommandContext ctx, DiscordMember member, bool invert, Deforms deform, ColorScales color)
        {
            int scale = 1;
            Colors colorScaleColor = Colors.Red;
            var messageData = new MessageData
            {
                Reply = true,
                ReplyId = ctx.Message.Id
            };

            if (deform != Deforms.None)
            {
                var messageStep = new MessageStep("What scale would do you want for the deformation?", "", 10)
                    .WithMesssageData(messageData);
                var result = await messageStep.ProcessStep(ctx.Channel, ctx.Member, ctx.Client, false);

                scale = int.TryParse(result.Result, out var x) ? int.Parse(result.Result) : -1;

                if (scale < 0 || scale > 10)
                {
                    await ctx.RespondAsync("How about you keep the scale within 0 and 10 next time?");
                    return;
                }
            }

            if (color == ColorScales.ColorScale)
            {
                var replyStep = new ReplyStep("What color would you like to colorize to?", "", 10, Enum.GetNames(typeof(Colors)).ToList())
                    .WithMesssageData(messageData);
                var result = await replyStep.ProcessStep(ctx.Channel, ctx.Member, ctx.Client, false);

                if (!result.WasCompleted)
                {
                    await ctx.RespondAsync("well I didn't get a valid reply so ¯\\_(ツ)_/¯");
                    return;
                }

                colorScaleColor = (Colors)await ctx.CommandsNext.ConvertArgument<Colors>(result.Result, ctx);
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
                        case ColorScales.GrayScale:
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
                for (int i = 0; i < images.FrameCount; i++)
                    await images[i].Resize(600, 600);

                await images[1].SetOpcaity(150);
                await images[0].DrawImage(images[1], 0, 0, new RectangleF(0, 0, images[1].Width, images[1].Height), GraphicsUnit.Pixel);

                using (var ms = await images[0].ToMemoryStream())
                    await new DiscordMessageBuilder()
                              .WithFiles(new Dictionary<string, Stream>() { { "Combined.png", ms } })
                              .WithReply(ctx.Message.Id)
                              .SendAsync(ctx.Channel);
            }
        }

        [Command("Rotate")]
        [Description("Rotate a user pfp")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Hate(CommandContext ctx, DiscordMember member, int angle)
        {
            angle %= 360;

            using (var collection = service.DownLoadImages(new TupleBag<string, int>(new List<(string, int)> { (member.AvatarUrl, 1) })))
            {
                await collection[0].Resize(600, 600);
                await collection[0].Rotate(angle);

                using (var stream = await collection[0].ToMemoryStream())
                    await new DiscordMessageBuilder()
                             .WithFiles(new Dictionary<string, Stream>() { { "Rotated.png", stream } })
                             .WithReply(ctx.Message.Id)
                             .SendAsync(ctx.Channel);
            }
        }

        [Command("Rotate")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Hate(CommandContext ctx, int angle)
        {
            angle %= 360;

            using (var collection = service.DownLoadImages(new TupleBag<string, int>(new List<(string, int)> { (ctx.Member.AvatarUrl, 1) })))
            {
                await collection[0].Resize(600, 600);
                await collection[0].Rotate(angle);

                using (var stream = await collection[0].ToMemoryStream())
                    await new DiscordMessageBuilder()
                             .WithFiles(new Dictionary<string, Stream>() { { "Rotated.png", stream } })
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
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(message, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

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
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(message, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

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
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string[] sentences = message.Split(',').Select(x => x.Trim()).ToArray();
            if (sentences.Length < 2)
                sentences = new[] { $"{ctx.Member.DisplayName} used this command", "But they didn't split the sentences with commas" };

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush(editData.Font, editData.Size[i], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData.X[i], editData.Y[i], editData.Length[i], editData.Breadth[i], drawFont, drawBrush);
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
                    await images[0].Resize(editData.Size[0], editData.Size[0]);

                    RectangleF srcRect = new RectangleF(0, 0, editData.Size[0], editData.Size[0]);
                    await graphicalImage.DrawImage(images[0], editData.X[0], editData.Y[0], srcRect, GraphicsUnit.Pixel);

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                     .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                     .WithReply(ctx.Message.Id)
                                     .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Headache")]
        [Description("Headaches aren't pog")]
        [WithFile("headache.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Headache(CommandContext ctx, [RemainingText] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(message, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("Lightning")]
        [Description("lightning kill tree")]
        [WithFile("lightning.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Lightning(CommandContext ctx, [RemainingText] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(message, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("Scary")]
        [Description("m o n k a p r a y")]
        [WithFile("scary.jpg")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Scary(CommandContext ctx, [RemainingText] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string[] sentences = message.Split(',').Select(x => x.Trim()).ToArray();
            if (sentences.Length < 4)
                sentences = new[] { $"{ctx.Member.DisplayName} opening discord", $"{ctx.Member.DisplayName} finding this channel",
                $"{ctx.Member.DisplayName} typing", $"{ctx.Member.DisplayName} running this command without `,`'s"};

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush(editData.Font, editData.Size[i], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData.X[i], editData.Y[i], editData.Length[i], editData.Breadth[i], drawFont, drawBrush);
                }

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("What")]
        [Description("**THE WHAT**")]
        [WithFile("what.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task What(CommandContext ctx, [RemainingText] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);

                await graphicalImage.DrawString(message, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("Googlesearch")]
        [Description("google do know everything")]
        [WithFile("googlesearch.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Googlesearch(CommandContext ctx, [RemainingText] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            List<string> sentences = message.Split(',').Select(x => x.Trim()).ToList();
            if (sentences.Count < 3)
                sentences = new List<string> { $"How should {ctx.Member.DisplayName} use this command", "They split the search and the result by a ','", "Using Image commands in Pepper" };

            sentences.Add(sentences[2]);

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                for (int i = 0; i < sentences.Count; i++)
                {
                    var color = Color.FromName(editData.Colors[i]);
                    service.GetFontAndBrush(editData.Font, editData.Size[i], color, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);

                    await graphicalImage.DrawString(sentences[i], editData.X[i], editData.Y[i], editData.Length[i], editData.Breadth[i], drawFont, drawBrush);
                }

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("SBC")]
        [Description("This nub")]
        [WithFile("sbc.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task SBC(CommandContext ctx, [RemainingText] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);

                await graphicalImage.DrawString(message, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                                 .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                 .WithReply(ctx.Message.Id)
                                 .SendAsync(ctx.Channel);
            }
        }

        [Command("Hate")]
        [Description("I hate this more")]
        [WithFile("hate.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Hate(CommandContext ctx, DiscordMember member)
        {
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                using (var list = service.DownLoadImages(new TupleBag<string, int>(new List<(string, int)> { (member.AvatarUrl, 1) })))
                {
                    await list[0].Resize(editData.Breadth[0], editData.Length[0]);
                    await graphicalImage.DrawImageRotated(list[0], editData.Size[0], editData.X[0], editData.Y[0], new Rectangle(0, 0, editData.Length[0], editData.Breadth[0]), GraphicsUnit.Pixel);

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                                    .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                                    .WithReply(ctx.Message.Id)
                                    .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Gifify")]
        [Description("Combine user pfps into a gif")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Gifify(CommandContext ctx, params DiscordMember[] members)
        {
            if(members.Length > 5)
            {
                await ctx.RespondAsync("Keep the maximum to 5 members");
                return;
            }

            using (var list = service.DownLoadImages(new TupleBag<string, int>(members.Select(x => (x.AvatarUrl, 1)).ToList())))
            {
                await list.Resize(600, 600);

                using (var ms = await list.ToMemoryStream(100))
                    await new DiscordMessageBuilder()
                                .WithFiles(new Dictionary<string, Stream>() { { "gifified.gif", ms } })
                                .WithReply(ctx.Message.Id)
                                .SendAsync(ctx.Channel);
            }
        }

        [Command("Spin")]
        [Description("Spinni boi go brr")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Spin(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            using (var list = service.DownLoadImages(new TupleBag<string, int>(Enumerable.Range(1, 4).Select(x => (member.AvatarUrl, 1)).ToList())))
            {
                await list.Resize(600, 600);

                for (int angle = 45, i = 0; angle < 360; angle += 90, i++)
                    await list[i].Rotate(angle);

                using (var ms = await list.ToMemoryStream(10, true))
                    await new DiscordMessageBuilder()
                                .WithFiles(new Dictionary<string, Stream>() { { "spin.gif", ms } })
                                .WithReply(ctx.Message.Id)
                                .SendAsync(ctx.Channel);
            }
        }

        [Command("Communism")]
        [WithFile("communism.gif")]
        [Description("Our gif command")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Communism(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            using (var collection = new ImageCollection(filePath))
            {
                using (var list = service.DownLoadImages(new TupleBag<string, int>((member.AvatarUrl, 1))))
                {
                    await list[0].Resize(collection.Width, collection.Height);
                    await list[0].SetOpcaity(150);

                    await collection.DrawImage(list[0], 0, 0, new RectangleF(0, 0, collection.Width, collection.Height), GraphicsUnit.Pixel);

                    using (var ms = await collection.ToMemoryStream(13, false))
                        await new DiscordMessageBuilder()
                                    .WithFiles(new Dictionary<string, Stream>() { { "communism.gif", ms } })
                                    .WithReply(ctx.Message.Id)
                                    .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Cancer")]
        [Description("its true, its cancer")]
        [WithFile("cancer.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Cancer(CommandContext ctx, DiscordMember member)
        {
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                using (var list = service.DownLoadImages(new TupleBag<string, int>((member.AvatarUrl, 1))))
                {
                    await list[0].Resize(editData.Size[0], editData.Size[0]);
                    await graphicalImage.DrawImage(list[0], editData.X[0], editData.Y[0], new RectangleF(0, 0, editData.Size[0], editData.Size[0]), GraphicsUnit.Pixel);

                    using (var ms = await graphicalImage.ToMemoryStream())
                        await new DiscordMessageBuilder()
                            .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                            .WithReply(ctx.Message.Id)
                            .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Rainbow")]
        [Description("colorfull colorscales")]
        [WithFile("rainbow.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Rainbow(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                using (var list = service.DownLoadImages(new TupleBag<string, int>((member.AvatarUrl, 1))))
                {
                    await list[0].Resize(graphicalImage.Width, graphicalImage.Height);
                    await list[0].DrawImage(graphicalImage, 0, 0, new RectangleF(0, 0, graphicalImage.Width, graphicalImage.Height), GraphicsUnit.Pixel);

                    using (var ms = await list[0].ToMemoryStream())
                        await new DiscordMessageBuilder()
                            .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                            .WithReply(ctx.Message.Id)
                            .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Hell")]
        [Description("colorfull colorscales")]
        [WithFile("hell.png")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Hell(CommandContext ctx, [RemainingText] string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            sentence = sentence.Insert(0, "How about ");
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out var font, out var brush);
                await graphicalImage.DrawString(sentence, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], font, brush);

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                        .WithReply(ctx.Message.Id)
                        .SendAsync(ctx.Channel);
            }
        }

        [Command("Chad")]
        [Description("An absolute chad")]
        [WithFile("chad.gif")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Chad(CommandContext ctx, [RemainingText] string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string[] sentences = sentence.Split(',').Select(x => x.Trim()).ToArray();
            if (sentences.Length < 2)
                sentences = new[] { $"Using this command without splitting sentences", "Using this command properly" };

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageCollection(filePath))
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    service.GetFontAndBrush(editData.Font, editData.Size[i], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                    await graphicalImage.DrawString(sentences[i], editData.X[i], editData.Y[i], editData.Length[i], editData.Breadth[i], drawFont, drawBrush);
                }

                using (var ms = await graphicalImage.ToMemoryStream(10, false))
                    await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                        .WithReply(ctx.Message.Id)
                        .SendAsync(ctx.Channel);
            }
        }

        [Command("Youre")]
        [WithFile("youre.gif")]
        [Description("Spelling mistake = instant argument win")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Youre(CommandContext ctx, string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                await ctx.RespondAsync("At least give me a valid word");
                return;
            }

            word += '*';
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageCollection(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(word, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream(5, false))
                    await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                        .WithReply(ctx.Message.Id)
                        .SendAsync(ctx.Channel);
            }
        }

        [Command("Burn")]
        [WithFile("burn.jpeg")]
        [Description("Let it burn")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Burn(CommandContext ctx, DiscordMember other)
        {
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageCollection(filePath))
            {
                using (var list = service.DownLoadImages(new TupleBag<string, int>((other.AvatarUrl, 1))))
                {
                    await list[0].Resize(editData.Length[0], editData.Breadth[0]);
                    await graphicalImage.DrawImage(list[0], editData.X[0], editData.Y[0], new RectangleF(0, 0, editData.Length[0], editData.Breadth[0]), GraphicsUnit.Pixel);

                    using (var ms = await graphicalImage.ToMemoryStream(5, false))
                        await new DiscordMessageBuilder()
                            .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                            .WithReply(ctx.Message.Id)
                            .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Burn")]
        [WithFile("burn.jpeg")]
        [Description("let it burn")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Burn(CommandContext ctx, [RemainingText] string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageCollection(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(sentence, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream(5, false))
                    await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                        .WithReply(ctx.Message.Id)
                        .SendAsync(ctx.Channel);
            }
        }

        [Command("Senate")]
        [WithFile("senate.gif")]
        [Description("**I am the Senate**")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Senate(CommandContext ctx, [RemainingText] string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageCollection(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.White, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString($"I am the {word}", editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream(5, false))
                    await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                        .WithReply(ctx.Message.Id)
                        .SendAsync(ctx.Channel);
            }
        }

        [Command("Handicapped")]
        [WithFile("handicapped.jpeg")]
        [Description("Im sorry to bother you sir")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Handicapped(CommandContext ctx, [RemainingText] string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(sentence, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                        .WithReply(ctx.Message.Id)
                        .SendAsync(ctx.Channel);
            }
        }

        [Command("Money")]
        [WithFile("money.gif")]
        [Description("fast money go brrr")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Money(CommandContext ctx, [RemainingText] string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageCollection(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.Black, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(sentence, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream(5, false))
                    await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                        .WithReply(ctx.Message.Id)
                        .SendAsync(ctx.Channel);
            }
        }

        [Command("ArtOfWar")]
        [WithFile("artofwar.png")]
        [Description("wise men leave wise quotes")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task ArtOfWar(CommandContext ctx, [RemainingText] string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
            {
                await ctx.RespondAsync("At least give me a valid sentence");
                return;
            }

            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageGraphic(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.White, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString(sentence, editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream())
                    await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                        .WithReply(ctx.Message.Id)
                        .SendAsync(ctx.Channel);
            }
        }

        [Command("Delete")]
        [WithFile("delete.jpg")]
        [Description("delete the trash")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Delete(CommandContext ctx, DiscordMember other)
        {
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageCollection(filePath))
            {
                using (var list = service.DownLoadImages(new TupleBag<string, int>((other.AvatarUrl, 1))))
                {
                    await list[0].Resize(editData.Length[0], editData.Breadth[0]);
                    await graphicalImage.DrawImage(list[0], editData.X[0], editData.Y[0], new RectangleF(0, 0, editData.Length[0], editData.Breadth[0]), GraphicsUnit.Pixel);

                    using (var ms = await graphicalImage.ToMemoryStream(5, false))
                        await new DiscordMessageBuilder()
                            .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                            .WithReply(ctx.Message.Id)
                            .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Something")]
        [WithFile("something.jpg")]
        [Description("delete the trash")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Something(CommandContext ctx, DiscordMember other)
        {
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageCollection(filePath))
            {
                using (var list = service.DownLoadImages(new TupleBag<string, int>((other.AvatarUrl, 1))))
                {
                    await list[0].Resize(editData.Length[0], editData.Breadth[0]);
                    await graphicalImage.DrawImage(list[0], editData.X[0], editData.Y[0], new RectangleF(0, 0, editData.Length[0], editData.Breadth[0]), GraphicsUnit.Pixel);

                    using (var ms = await graphicalImage.ToMemoryStream(5, false))
                        await new DiscordMessageBuilder()
                            .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                            .WithReply(ctx.Message.Id)
                            .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("Garbage")]
        [WithFile("garbage.gif")]
        [Description("Your entire life is garbage")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Garbage(CommandContext ctx, DiscordMember other)
        {
            string fileName = service.GetFileByCommand(ctx.Command);
            string filePath = Path.Combine("Modules", "Images", "Images", fileName);

            EditData editData = service.GetEditData(fileName);

            using (var graphicalImage = new ImageCollection(filePath))
            {
                service.GetFontAndBrush(editData.Font, editData.Size[0], Color.White, editData.FontStyle, out Font drawFont, out SolidBrush drawBrush);
                await graphicalImage.DrawString($"Thank you {other.DisplayName}, your entire life is garbage.", editData.X[0], editData.Y[0], editData.Length[0], editData.Breadth[0], drawFont, drawBrush);

                using (var ms = await graphicalImage.ToMemoryStream(5, false))
                    await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
                        .WithReply(ctx.Message.Id)
                        .SendAsync(ctx.Channel);
            }
        }
    }
}