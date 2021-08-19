using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Services;
using KunalsDiscordBot.Core.Help;
using KunalsDiscordBot.Core.Reddit;
using KunalsDiscordBot.Core.Events;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Services.Moderation;
using KunalsDiscordBot.Core.ArgumentConverters;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Modules.FunCommands;

namespace KunalsDiscordBot
{
    public class PepperBot
    {
        public DiscordClient client { get; private set; }

        public CommandsNextExtension commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }

        public LavalinkExtension lavaLink { get; private set; }
        public LavalinkConfiguration lavaLinkConfig { get; private set; }

        public IServiceProvider services { get; private set; }
        public PepperBotConfig configuration { get; private set; }

        public int shardId { get; set; }

        public PepperBot (IServiceProvider _services, PepperBotConfig config, int _shardId)
        {
            configuration = config;
            services = _services;
            shardId = _shardId;

            client = new DiscordClient(new DiscordConfiguration
            {
                Token = configuration.discordConfig.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers,
                ShardCount = config.discordConfig.shardCount,
                ShardId = shardId,

                LargeThreshold = 250,
                GatewayCompressionLevel = GatewayCompressionLevel.Stream
                /*LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true*/
            });

            client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(configuration.discordConfig.timeOut)
            });

            client.GuildCreated += OnGuildCreated;
            client.GuildDeleted += OnGuildDeleted;
            client.GuildMemberAdded += OnGuildMemberAdded;
            client.GuildMemberRemoved += OnGuildMemberRemoved;

            var endPoint = new ConnectionEndpoint
            {
                Hostname = configuration.lavalinkConfig.hostname,
                Port = configuration.lavalinkConfig.port
            };

            lavaLinkConfig = new LavalinkConfiguration
            {
                Password = configuration.lavalinkConfig.password,
                RestEndpoint = endPoint,
                SocketEndpoint = endPoint
            };

            lavaLink = client.UseLavalink();

            CommandsNextConfiguration commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = configuration.discordConfig.prefixes,
                EnableDms = configuration.discordConfig.dms,
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
            commands.RegisterConverter(new EnumArgumentConverter<RedditPostFilter>());
            commands.RegisterConverter(new EnumArgumentConverter<ConfigValue>());
            commands.RegisterConverter(new EnumArgumentConverter<RockPaperScissors>());
        }

        public async Task ConnectAsync()
        {
            await client.ConnectAsync(new DiscordActivity
            {
                Name = configuration.discordConfig.activityText,
                ActivityType = (ActivityType)configuration.discordConfig.activityType
            });

            try
            {
                await lavaLink.ConnectAsync(lavaLinkConfig);
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
                    ulong id = (ulong)(await serverService.GetModerationData(guild.Value.Id)).MutedRoleId;

                    var role = guild.Value.Roles.FirstOrDefault(x => x.Value.Id == id).Value;
                    var member = guild.Value.Members.FirstOrDefault(x => x.Value.Id == (ulong)mute.UserId).Value;

                    if (!TimeSpan.TryParse(mute.Time, out var x) || span > TimeSpan.Parse(mute.Time))
                        await member.RevokeRoleAsync(role).ConfigureAwait(false);
                    else
                        BotEventFactory.CreateScheduledEvent().WithSpan(span).WithEvent((s, e) =>
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
                }.WithFooter("You gotta use a command that exists", configuration.discordConfig.errorLink);
            }
            else if (exception is InvalidOverloadException && log)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "No version of the command uses has these parameters",
                    Description = $"Did you miss a parameter? Use the `pep help` command for help",
                    Color = DiscordColor.Red
                }.WithFooter("You gotta use a command that exists", configuration.discordConfig.errorLink);
            }
            else if (exception is CustomCommandException)//ignore
            { }
            else if (exception is ChecksFailedException cfe)
            {
                string title = string.Empty, description = string.Empty;
                string footer = "That wasn't supposed to happen";

                if (cfe.FailedChecks.FirstOrDefault(x => x is RequireBotPermissionsAttribute) != null)
                {
                    title = "Permission denied";
                    description = $"The bot lacks the permissions necessary to run this command.";

                    footer = "Permissions be all";
                }
                else if(cfe.FailedChecks.FirstOrDefault(x => x is RequireUserPermissionsAttribute) != null)
                {
                    title = "Permission denied";
                    description = $"You lack the permissions necessary to run this command.";

                    footer = "Permissions be all";
                }
                else if(cfe.FailedChecks.FirstOrDefault(x => x is CooldownAttribute) != null)
                {
                    title = "Chill out";
                    var casted = (CooldownAttribute)cfe.FailedChecks.FirstOrDefault(x => x is CooldownAttribute);

                    description = $"You just used this command and can use it after this much time:\n{casted.GetRemainingCooldown(e.Context)}";
                    footer = "Spam ain't cool";
                }

                embed = new DiscordEmbedBuilder
                {
                    Title = title,
                    Description = description,
                    Color = DiscordColor.Red
                }.WithFooter(footer, configuration.discordConfig.errorLink);
            }
            else if(log)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "A problem occured while executing the command",
                    Description = $"Exception: {exception.Message} at {Formatter.InlineCode(e.Command.QualifiedName)}",
                    Color = DiscordColor.Red,
                }.WithFooter("Well that wasn't supposed to happen", configuration.discordConfig.errorLink);
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
                var modService = (IModerationService)services.GetService(typeof(IModerationService));

                var profile = await serverService.GetServerProfile(e.Guild.Id);
          
                var channel = e.Guild.Channels.FirstOrDefault(x => x.Value.Id == ((ulong)profile.WelcomeChannel)).Value;
                if (channel == null)
                    channel = e.Guild.GetDefaultChannel();

                if (channel != null)
                    await channel.SendMessageAsync(BotService.GetLeaveEmbed().WithThumbnail(s.CurrentUser.AvatarUrl, 30, 30))
                    .ConfigureAwait(false);

                await modService.ClearAllServerModerationData(e.Guild.Id);
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

                var channel = e.Guild.Channels.FirstOrDefault(x => x.Value.Id == ((ulong)profile.WelcomeChannel)).Value;
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

                var channel = e.Guild.Channels.FirstOrDefault(x => x.Value.Id == ((ulong)profile.WelcomeChannel)).Value;
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
    }
}
