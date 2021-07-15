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
                Intents  = DiscordIntents.AllUnprivileged
                /*LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true*/
            };

            client = new DiscordClient(config);

            client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(configData.timeOut)
            });

            client.GuildCreated += OnGuildCreated;

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
            await client.ConnectAsync();

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

            await CheckData();
        }

        private async Task CheckData()
        {
            var modService = (IModerationService)services.GetService(typeof(IModerationService));
            var serverService = (IServerService)services.GetService(typeof(IServerService));

            foreach (var guild in client.Guilds.Where(x => (x.Value.Permissions & Permissions.Administrator)== Permissions.Administrator))//all servers where the bot is an admin
            {
                foreach(var mute in await modService.GetMutes(guild.Value.Id))
                {
                    var span = DateTime.Now - DateTime.Parse(mute.StartTime);
                    ulong id = await serverService.GetMuteRoleId(guild.Value.Id);

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

            if (exception is CommandNotFoundException)
            {
                //according to database
            }
            else if (exception is InvalidOverloadException)
            {
                //according to database
            }
            else if (exception is CustomCommandException)//ignore
            { }
            else if (exception is ChecksFailedException cfe)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Permission denied",
                    Description = $"You lack permissions necessary to run this command.",
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

                await e.Guild.GetDefaultChannel().SendMessageAsync(BotService.GetBotInfo(s, null, 30)).ConfigureAwait(false);
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
