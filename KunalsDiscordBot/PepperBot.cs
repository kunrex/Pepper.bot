using System;
using System.Linq;
using System.Text;
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
using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Reddit;
using KunalsDiscordBot.Core.Events;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Services.Moderation;
using KunalsDiscordBot.Services.Configuration;
using KunalsDiscordBot.Core.ArgumentConverters;
using KunalsDiscordBot.Core.Modules.FunCommands;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Attributes.ModerationCommands;

namespace KunalsDiscordBot
{
    public class PepperBot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }

        public LavalinkExtension LavaLink { get; private set; }
        public LavalinkConfiguration LavaLinkConfig { get; private set; }

        public IServiceProvider Services { get; private set; }
        public PepperBotConfig Configuration { get; private set; }

        public int ShardId { get; private set; }

        public PepperBot (IServiceProvider _services, PepperBotConfig _config, int _shardId)
        {
            Configuration = _config;
            Services = _services;
            ShardId = _shardId;

            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = Configuration.discordConfig.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers,
                ShardCount = Configuration.discordConfig.shardCount,
                ShardId = ShardId,

                LargeThreshold = 250,
                GatewayCompressionLevel = GatewayCompressionLevel.Stream
                /*LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true*/
            });

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(Configuration.discordConfig.timeOut),
                AckPaginationButtons = true
            });

            Client.GuildCreated += OnGuildCreated;
            Client.GuildDeleted += OnGuildDeleted;
            Client.GuildMemberAdded += OnGuildMemberAdded;
            Client.GuildMemberRemoved += OnGuildMemberRemoved;

            var endPoint = new ConnectionEndpoint
            {
                Hostname = Configuration.lavalinkConfig.hostname,
                Port = Configuration.lavalinkConfig.port
            };

            LavaLinkConfig = new LavalinkConfiguration
            {
                Password = Configuration.lavalinkConfig.password,
                RestEndpoint = endPoint,
                SocketEndpoint = endPoint
            };

            LavaLink = Client.UseLavalink();

            Commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = Configuration.discordConfig.prefixes,
                EnableDms = Configuration.discordConfig.dms,
                EnableMentionPrefix = true,
                CaseSensitive = false,
                Services = _services,
                DmHelp = false,
            });

            Commands.CommandErrored += CommandErrored;

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(BaseCommandModule)) && !x.IsAbstract))
                Commands.RegisterCommands(type);

            Commands.SetHelpFormatter<HelpFormatter>();

            Commands.RegisterConverter(new BoolArgumentConverter());
            Commands.RegisterConverter(new TimeSpanArgumentConverter());
            Commands.RegisterConverter(new EnumArgumentConverter<RedditPostFilter>());
            Commands.RegisterConverter(new EnumArgumentConverter<ConfigValue>());
            Commands.RegisterConverter(new EnumArgumentConverter<RockPaperScissors>());
        }

        public async Task ConnectAsync()
        {
            await Client.ConnectAsync(new DiscordActivity
            {
                Name = Configuration.discordConfig.activityText,
                ActivityType = (ActivityType)Configuration.discordConfig.activityType
            });

            try
            {
                await LavaLink.ConnectAsync(LavaLinkConfig);
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

            await CheckModerationMutes();
        }

        private async Task CheckModerationMutes()
        {
            var modService = (IModerationService)Services.GetService(typeof(IModerationService));
            var serverService = (IServerService)Services.GetService(typeof(IServerService));

            foreach (var guild in Client.Guilds.Where(x => x.Value.Permissions.HasValue).Where(x => (x.Value.Permissions & Permissions.Administrator) == Permissions.Administrator))//all servers where the bot is an admin
            {
                ulong id = (ulong)(await serverService.GetModerationData(guild.Value.Id)).MutedRoleId;
                var role = guild.Value.Roles.FirstOrDefault(x => x.Value.Id == id).Value;

                foreach (var mute in await modService.GetMutes(guild.Value.Id))
                {
                    var span = DateTime.Now - DateTime.Parse(mute.StartTime);
                 
                    var member = guild.Value.Members.FirstOrDefault(x => x.Value.Id == (ulong)mute.UserId).Value;
                    var muteTime = TimeSpan.Parse(mute.Time);

                    if (span > muteTime || !TimeSpan.TryParse(mute.Time, out var x))
                        await member.RevokeRoleAsync(role).ConfigureAwait(false);
                    else
                        BotEventFactory.CreateScheduledEvent().WithSpan(muteTime - span).WithEvent((s, e) =>
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
            var serverService = (IServerService)Services.GetService(typeof(IServerService));
            var profile = await serverService.GetServerProfile(e.Context.Guild.Id);

            var log = profile.LogErrors == 1;

            if (exception is CommandNotFoundException && log)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "The given command wasn't found",
                    Description = $"Did you mispell something? Use the `pep help` command for help",
                    Color = DiscordColor.Red
                }.WithFooter("You gotta use a command that exists", Configuration.discordConfig.errorLink);
            }
            else if (exception is InvalidOverloadException && log)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "No version of the command uses has these parameters",
                    Description = $"Did you miss a parameter? Use the `pep help` command for help",
                    Color = DiscordColor.Red
                }.WithFooter("You gotta use a command that exists", Configuration.discordConfig.errorLink);
            }
            else if (exception is CustomCommandException)//ignore
            { }
            else if (exception is ChecksFailedException cfe)
            {
                string title = string.Empty, description = string.Empty;
                string footer = "That wasn't supposed to happen";

                if (cfe.FailedChecks.FirstOrDefault(x => x is RequireBotPermissionsAttribute) != null)
                {
                    var attribute = (RequireBotPermissionsAttribute)cfe.FailedChecks.First(x => x is RequireBotPermissionsAttribute);

                    title = "Permission denied";
                    description = $"The bot lacks the permissions necessary to run this command.\n Permissions Required: {attribute.Permissions.FormatePermissions()}";

                    footer = "Permissions be all";
                }
                else if (cfe.FailedChecks.FirstOrDefault(x => x is RequireUserPermissionsAttribute || x is ModeratorNeededAttribute) != null)
                {
                    var permissions = new StringBuilder();

                    var requireUserPerms = (RequireUserPermissionsAttribute)cfe.FailedChecks.FirstOrDefault(x => x is RequireUserPermissionsAttribute);
                    if (requireUserPerms != null)
                        permissions.Append(requireUserPerms.Permissions.FormatePermissions());

                    if (cfe.FailedChecks.FirstOrDefault(x => x is ModeratorNeededAttribute) != null)
                        permissions.Append($"{(requireUserPerms == null ? "" : ", ")}`Moderator`");

                    title = "Permission denied";
                    description = $"You lack the permissions necessary to run this command.\n Permissions Required: {permissions.ToString()}";

                    footer = "Permissions be all";
                }
                else if(cfe.FailedChecks.FirstOrDefault(x => x is CooldownAttribute) != null)
                {
                    title = "Chill out";
                    var casted = (CooldownAttribute)cfe.FailedChecks.First(x => x is CooldownAttribute);
                    var cooldown = casted.GetRemainingCooldown(e.Context);

                    description = $"You just used this command and can use it after this much time:\n {cooldown.Days} days, {cooldown.Hours} hours, {cooldown.Minutes} minutes {cooldown.Seconds} seconds";
                    footer = "Spam ain't cool";
                }

                embed = new DiscordEmbedBuilder
                {
                    Title = title,
                    Description = description,
                    Color = DiscordColor.Red
                }.WithFooter(footer, Configuration.discordConfig.errorLink);
            }
            else if(log)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "A problem occured while executing the command",
                    Description = $"Exception: {exception.Message} at {Formatter.InlineCode(e.Command.QualifiedName)}",
                    Color = DiscordColor.Red,
                }.WithFooter("Well that wasn't supposed to happen", Configuration.discordConfig.errorLink);
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

                var configService = (IConfigurationService)Services.GetService(typeof(IConfigurationService));
                await channel.SendMessageAsync((await configService.GetPepperBotInfo(s.Guilds.Count, s.ShardCount, ShardId))
                    .WithFooter("Pepper").WithThumbnail(s.CurrentUser.AvatarUrl, 30, 30)).ConfigureAwait(false);

                var serverService = (IServerService)Services.GetService(typeof(IServerService));
                await serverService.CreateServerProfile(e.Guild.Id);
            });

            return Task.CompletedTask;
        }

        private Task OnGuildDeleted(DiscordClient s, GuildDeleteEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var serverService = (IServerService)Services.GetService(typeof(IServerService));
                var modService = (IModerationService)Services.GetService(typeof(IModerationService));

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
                var serverService = (IServerService)Services.GetService(typeof(IServerService));
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
                var serverService = (IServerService)Services.GetService(typeof(IServerService));
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
