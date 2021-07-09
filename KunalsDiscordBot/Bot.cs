//System name spaces
using System.Threading.Tasks;
using System;
using System.IO;

//D# name spaces
using DSharpPlus.Net;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;

//Custom name spaces
using KunalsDiscordBot.Modules.General;
using KunalsDiscordBot.Modules.Fun;
using KunalsDiscordBot.Modules.Math;
using KunalsDiscordBot.Modules.Games;
using KunalsDiscordBot.Modules.School;
using KunalsDiscordBot.Modules.Music;
using KunalsDiscordBot.Modules.Images;
using KunalsDiscordBot.Modules.Moderation;
using KunalsDiscordBot.Modules.Moderation.SoftModeration;
using KunalsDiscordBot.Modules.Currency;
using KunalsDiscordBot.ArgumentConverters;

using KunalsDiscordBot.Help;
using System.Reflection;
using DSharpPlus.Entities;
using System.Linq;
using DSharpPlus.CommandsNext.Attributes;
using KunalsDiscordBot.Services;

namespace KunalsDiscordBot
{
    public class Bot
    {
        public DiscordClient client { get; private set; }
        public CommandsNextExtension commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }

        public static readonly string KunalsID = System.Text.Json.JsonSerializer.Deserialize<ConfigData>(File.ReadAllText("Config.json")).KunalsID;

        public Bot (IServiceProvider services)
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

            //client.Ready += OnClientReady;
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

            CommandsNextConfiguration commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = configData.prefixes,
                EnableDms = configData.dms,
                EnableMentionPrefix = true,
                CaseSensitive = false,
                Services = services,
                DmHelp = false,            
            };

            commands = client.UseCommandsNext(commandsConfig);

            commands.RegisterCommands<GeneralCommands>();
            commands.RegisterCommands<MathCommands>();
            commands.RegisterCommands<GameCommands>();
            commands.RegisterCommands<FunCommands>();
            //commands.RegisterCommands<SchoolCommands>();
            commands.RegisterCommands<MusicCommands>();
            commands.RegisterCommands<ImageCommands>();
            commands.RegisterCommands<ModerationCommands>();
            commands.RegisterCommands<SoftModerationCommands>();
            commands.RegisterCommands<CurrencyCommands>();

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
            }
            catch
            {

            }
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
