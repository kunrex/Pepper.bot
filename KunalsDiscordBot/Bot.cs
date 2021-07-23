//System name spaces
using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections.Generic;

//D# name spaces
using DSharpPlus.Net;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;

//Custom name spaces
using KunalsDiscordBot.ArgumentConverters;

using KunalsDiscordBot.Help;
using System.Reflection;
using DSharpPlus.Entities;
using System.Linq;
using DSharpPlus.CommandsNext.Attributes;
using KunalsDiscordBot.Services;
using KunalsDiscordBot.Events;
using KunalsDiscordBot.Services.Moderation;
using KunalsDiscordBot.Services.General;
using DSharpPlus.CommandsNext.Exceptions;
using KunalsDiscordBot.Core.Exceptions;

//Look into turret bot on github
//Create an error to throw when a check fails
//Use the BeforeExecution and AfterExecution methods in the modules to perform all checks, if any check fails throw said error
//in the CommandErrored event, add a method to handle said exception

namespace KunalsDiscordBot
{
    public class Bot
    {
        public DiscordClient client { get; private set; }
        public CommandsNextExtension commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }

        private readonly IServiceProvider services;

        public static readonly string KunalsID = System.Text.Json.JsonSerializer.Deserialize<ConfigData>(File.ReadAllText("Config.json")).KunalsID;

        public Bot (IServiceProvider _services)
        {
            string fileData = File.ReadAllText("Config.json");
            var configData = System.Text.Json.JsonSerializer.Deserialize<ConfigData>(fileData);

            var config = new DiscordConfiguration
            {
                Token = configData.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents  = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers
                /*LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true*/
            };

            client = new DiscordClient(config);

            client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(configData.timeOut)
            });

            client.GuildCreated += OnGuildCreated;
            client.GuildDeleted += OnGuildDeleted;
            client.GuildMemberAdded += OnGuildMemberAdded;
            client.GuildMemberRemoved += OnGuildMemberRemoved;

            var endPoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };

            var lavaLinkConfig = new LavalinkConfiguration
            {
                Password = "pepperrocks",
                RestEndpoint = endPoint,
                SocketEndpoint = endPoint
            };

            var lavaLink = client.UseLavalink();
            services = _services;

            CommandsNextConfiguration commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = configData.prefixes,
                EnableDms = configData.dms,
                EnableMentionPrefix = true,
                CaseSensitive = false,
                Services = _services,
                DmHelp = false,            
            };

            commands = client.UseCommandsNext(commandsConfig);
            commands.CommandErrored += CommandErrored;

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(BaseCommandModule)) && !x.IsAbstract))
                commands.RegisterCommands(type);

            commands.SetHelpFormatter<HelpFormatter>();

            commands.RegisterConverter(new BoolArgumentConverter());
            commands.RegisterConverter(new TimeSpanArgumentConverter());

            ConnectAsync(client, lavaLink, lavaLinkConfig);
        }

        private async void ConnectAsync(DiscordClient client, LavalinkExtension lavaLink, LavalinkConfiguration config)
        {
            await client.ConnectAsync(new DiscordActivity
            {
                Name = "pep help",
                ActivityType = ActivityType.Playing,
            });

            try
            {
                await lavaLink.ConnectAsync(config);
                Console.WriteLine("Lavalink connection success");
            }
            catch
            {
                Console.WriteLine("Lavalink connection failed");
            }
            finally
            {
                Console.WriteLine("Lavalink connection section complete");
            }

            await CheckMutes();
        }

        private async Task CheckMutes()
        {
            var modService = (IModerationService)services.GetService(typeof(IModerationService));
            var serverService = (IServerService)services.GetService(typeof(IServerService));

            foreach (var guild in client.Guilds.Where(x => x.Value.Permissions.HasValue).Where(x => (x.Value.Permissions & Permissions.Administrator) == Permissions.Administrator))//all servers where the bot is an admin
            {
                foreach(var mute in await modService.GetMutes(guild.Value.Id))
                {
                    var span = DateTime.Now - DateTime.Parse(mute.StartTime);
                    ulong id = (ulong)(await serverService.GetServerProfile(guild.Value.Id)).MutedRoleId;

                    var role = guild.Value.Roles.FirstOrDefault(x => x.Value.Id == id).Value;
                    var profile = await modService.GetModerationProfile(mute.ModerationProfileId);
                    var member = guild.Value.Members.FirstOrDefault(x => x.Value.Id == (ulong)profile.DiscordId).Value;

                    if (!TimeSpan.TryParse(mute.Time, out var x) || span > TimeSpan.Parse(mute.Time))
                        await member.RevokeRoleAsync(role).ConfigureAwait(false);
                    else
                        BotEventFactory.CreateEvent().WithSpan(span).WithEvent((s, e) =>
                        {
                            Task.Run(async () => await member.RevokeRoleAsync(role).ConfigureAwait(false));
                        }).Execute();
                }
            }
        }

        private async Task CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            DiscordEmbedBuilder embed = null;

            var exception = e.Exception;
            var serverService = (IServerService)services.GetService(typeof(IServerService));
            var profile = await serverService.GetServerProfile(e.Context.Guild.Id);

            var log = profile.LogErrors == 1;

            if (exception is CommandNotFoundException && log)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "The given command wasn't found",
                    Description = $"Did you mispell something? Use the `pep help` command for help",
                    Color = DiscordColor.Red
                };
            }
            else if (exception is InvalidOverloadException && log)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "No version of the command uses has these parameters",
                    Description = $"Did you miss a parameter? Use the `pep help` command for help",
                    Color = DiscordColor.Red
                };
            }
            else if (exception is CustomCommandException)//ignore
            { }
            else if (exception is ChecksFailedException cfe)
            {
                string title = string.Empty, description = string.Empty;

                if (cfe.FailedChecks.FirstOrDefault(x => x is RequireBotPermissionsAttribute) != null)
                {
                    title = "Permission denied";
                    description = $"The bot lacks the permissions necessary to run this command.";
                }
                else if(cfe.FailedChecks.FirstOrDefault(x => x is RequireUserPermissionsAttribute) != null)
                {
                    title = "Permission denied";
                    description = $"You lack the permissions necessary to run this command.";
                }
                else if(cfe.FailedChecks.FirstOrDefault(x => x is CooldownAttribute) != null)
                {
                    title = "Chill out";
                    var casted = (CooldownAttribute)cfe.FailedChecks.FirstOrDefault(x => x is CooldownAttribute);

                    description = $"You just used this command and can use it after this much time:\n{casted.GetRemainingCooldown(e.Context)}";
                }

                embed = new DiscordEmbedBuilder
                {
                    Title = title,
                    Description = description,
                    Color = DiscordColor.Red
                };
            }
            else
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "A problem occured while executing the command",
                    Description = $"Exception: {exception.Message} at {Formatter.InlineCode(e.Command.QualifiedName)}",
                    Color = DiscordColor.Red
                };
            }

            await e.Context.RespondAsync(embed).ConfigureAwait(false);
        }

        private Task OnGuildCreated(DiscordClient s, GuildCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var channel = e.Guild.GetDefaultChannel();
                if (channel == null)
                    return;

                await channel.SendMessageAsync(BotService.GetBotInfo(s, null, 30)).ConfigureAwait(false);
                var serverService = (IServerService)services.GetService(typeof(IServerService));

                await serverService.CreateServerProfile(e.Guild.Id);
            });
            return Task.CompletedTask;
        }

        private Task OnGuildDeleted(DiscordClient s, GuildDeleteEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var serverService = (IServerService)services.GetService(typeof(IServerService));
                var profile = await serverService.GetServerProfile(e.Guild.Id);

                var channel = e.Guild.Channels.FirstOrDefault(x => x.Value.Id == ((ulong)profile.LogChannel)).Value;
                if (channel == null)
                    channel = e.Guild.GetDefaultChannel();

                if (channel == null)
                    return;

                await channel.SendMessageAsync(BotService.GetBotInfo(s, null, 30)).ConfigureAwait(false);
                await serverService.RemoveServerProfile(e.Guild.Id);
            });

            return Task.CompletedTask;
        }

        private Task OnGuildMemberAdded(DiscordClient s, GuildMemberAddEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var serverService = (IServerService)services.GetService(typeof(IServerService));
                var profile = await serverService.GetServerProfile(e.Guild.Id);

                if (profile.LogNewMembers == 0)
                    return;

                var channel = e.Guild.Channels.FirstOrDefault(x => x.Value.Id == ((ulong)profile.LogChannel)).Value;
                if (channel == null)
                    channel = e.Guild.GetDefaultChannel();

                if (channel == null)
                    return;

                await channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = $"Everybody Welcome {e.Member.Username}!",
                    Description = $"Hey there {e.Member.Username}, welcome to the server! {(profile.RulesChannelId == 0 ? "" : $"Check out the rules at {(e.Guild.Channels.FirstOrDefault(x => x.Value.Id == (ulong) profile.RulesChannelId)).Value.Mention}")}",
                    Thumbnail = BotService.GetEmbedThumbnail(e.Member, 30),
                    Footer = BotService.GetEmbedFooter($"At {DateTime.Now}"),
                    Color = DiscordColor.SpringGreen
                }).ConfigureAwait(false);
            });

            return Task.CompletedTask;
        }

        private Task OnGuildMemberRemoved(DiscordClient s, GuildMemberRemoveEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var serverService = (IServerService)services.GetService(typeof(IServerService));
                var profile = await serverService.GetServerProfile(e.Guild.Id);

                if (profile.LogNewMembers == 0)
                    return;

                var channel = e.Guild.Channels.FirstOrDefault(x => x.Value.Id == ((ulong)profile.LogChannel)).Value;
                if (channel == null)
                    channel = e.Guild.GetDefaultChannel();

                if (channel == null)
                    return;

                await channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = $"See You Later {e.Member.Username}!",
                    Description = $"Hope you had a great time",
                    Thumbnail = BotService.GetEmbedThumbnail(e.Member, 30),
                    Footer = BotService.GetEmbedFooter($"At {DateTime.Now}"),
                    Color = DiscordColor.CornflowerBlue
                }).ConfigureAwait(false);
            });

            return Task.CompletedTask;
        }

        private class ConfigData
        {
            public string token { get; set; }
            public string[] prefixes { get; set; }
            public bool dms { get; set; }
            public int timeOut { get; set; }
            public string KunalsID { get; set; }
        }
    }
}
