using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Services;
using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Services.Games;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Core.Modules.GameCommands;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Attributes.GameCommands;
using KunalsDiscordBot.Core.Configurations.Attributes;

namespace KunalsDiscordBot.Modules.Games
{
    [Group("Games")]
    [Decor("IndianRed", ":video_game:")]
    [Description("Commands to play games such as UNO and Battleship with other server members.")]
    [ModuleLifespan(ModuleLifespan.Transient), ConfigData(ConfigValueSet.Games)]
    [RequireBotPermissions(Permissions.SendMessages | Permissions.EmbedLinks | Permissions.AccessChannels)]
    public class GameCommands : PepperCommandModule
    {
        public override PepperCommandModuleInfo ModuleInfo { get; protected set; }

        private readonly IServerService serverService;
        private readonly IGameService gameService;

        public GameCommands(IServerService service, IGameService _gameService, IModuleService moduleService)
        {
            serverService = service;
            gameService = _gameService;
            ModuleInfo = moduleService.ModuleInfo[ConfigValueSet.Games];
        }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var configPermsCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckConfigigurationPermissionsAttribute) != null;

            if (configPermsCheck)
            {
                var profile = await serverService.GetServerProfile(ctx.Guild.Id).ConfigureAwait(false);

                if (profile.RestrictPermissionsToAdmin == 1 && (ctx.Member.PermissionsIn(ctx.Channel) & DSharpPlus.Permissions.Administrator) != DSharpPlus.Permissions.Administrator)
                {
                    await ctx.RespondAsync(":x: You need to be an admin to run this command").ConfigureAwait(false);
                    throw new CustomCommandException();
                }
            }

            var requireConnect4Channel = ctx.Command.CustomAttributes.FirstOrDefault(x => x is RequireConnect4ChannelAttribute) != null;
            if(requireConnect4Channel)
            {
                var profile = await serverService.GetGameData(ctx.Guild.Id).ConfigureAwait(false);

                if (profile.Connect4Channel == 0)
                {
                    await ctx.RespondAsync(":x: This server does not have a connect4 channel assigned, assign one using the `games connect4channel` command").ConfigureAwait(false);
                    throw new CustomCommandException();
                }
            }

            var requireTicTacToeChannel = ctx.Command.CustomAttributes.FirstOrDefault(x => x is RequireTicTacToeChannelAttribute) != null;
            if (requireTicTacToeChannel)
            {
                var profile = await serverService.GetGameData(ctx.Guild.Id).ConfigureAwait(false);

                if (profile.TicTacToeChannel == 0)
                {
                    await ctx.RespondAsync(":x: This server does not have a tictactoe channel assigned, assign one using the `games tictactoechannel` command").ConfigureAwait(false);
                    throw new CustomCommandException();
                }
            }

            await base.BeforeExecutionAsync(ctx);
        }

        [Command("Connect4Channel")]
        [Description("Assigns the connect4 channel for a server")]
        [CheckConfigigurationPermissions, ConfigData(ConfigValue.Connect4Channel)]
        public async Task Connect4Channel(CommandContext ctx, DiscordChannel channel)
        {
            await serverService.SetConnect4Channel(ctx.Guild.Id, channel.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Saved {channel.Mention} as the Connect4 channel for guild: `{ctx.Guild.Name}`",
                Color = ModuleInfo.Color
            }.WithFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("TicTacToeChannel")]
        [Description("Assigns the tictactoe channel for a server")]
        [CheckConfigigurationPermissions, ConfigData(ConfigValue.TicTacToeChannel)]
        public async Task TicTacToeChannel(CommandContext ctx, DiscordChannel channel)
        {
            await serverService.SetTicTacToeChannel(ctx.Guild.Id, channel.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Saved {channel.Mention} as the TicTacToe channel for guild: `{ctx.Guild.Name}`",
                Color = ModuleInfo.Color
            }.WithFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("Connect4")]
        [Description("The Connect 4 game, play with a friend"), RequireConnect4Channel]
        public async Task Connect(CommandContext ctx, DiscordMember other, int numberOfCells = 5)
        {
            if (ctx.User.Equals(other))
            {
                await ctx.Channel.SendMessageAsync("You can't play against yourself genius").ConfigureAwait(false);
                return;
            }
            else if (other.IsBot)
            {
                await ctx.Channel.SendMessageAsync("You can't play against a bot dum dum").ConfigureAwait(false);
                return;
            }

            var game = await gameService.StartGame<ConnectFour>(ctx.Guild.Id, new List<DiscordMember>()
            {
                ctx.Member,
                other
            }, ctx.Client, ctx.Guild.GetChannel((ulong)(await serverService.GetGameData(ctx.Guild.Id)).Connect4Channel), numberOfCells);

            if (game == null)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Failed to start match, If a game already is going on in the server then wait for it to finish.",
                    Color = ModuleInfo.Color
                }.WithFooter($"Member: {ctx.Member.Username}, at {DateTime.Now}"));
        }

        [Command("TicTacToe")]
        [Description("The TicTacToe game, play with a friend"), RequireTicTacToeChannel]
        public async Task TicTacToe(CommandContext ctx, DiscordMember other)
        {
            if (ctx.User.Equals(other))
            {
                await ctx.Channel.SendMessageAsync("You can't play against yourself genius").ConfigureAwait(false);
                return;
            }
            else if (other.IsBot)
            {
                await ctx.Channel.SendMessageAsync("You can't play against a bot dum dum").ConfigureAwait(false);
                return;
            }

            var game = await gameService.StartGame<TicTacToe>(ctx.Guild.Id, new List<DiscordMember>()
            {
                ctx.Member,
                other
            }, ctx.Client, ctx.Guild.GetChannel((ulong)(await serverService.GetGameData(ctx.Guild.Id)).TicTacToeChannel), default);

            Console.WriteLine((game == null) + "hahahaha");

            if (game == null)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Failed to start match, If a game already is going on in the server then wait for it to finish.",
                    Color = ModuleInfo.Color
                }.WithFooter($"Member: {ctx.Member.Username}, at {DateTime.Now}"));
        }

        [Command("BattleShip")]
        [Description("The BattleShip game, play with a friend. Make sure you allow DM's from server members")]
        public async Task PlayBattleShip(CommandContext ctx, DiscordMember other)
        {
            if (ctx.User.Equals(other))
            {
                await ctx.Channel.SendMessageAsync("You can't play against yourself genius").ConfigureAwait(false);
                return;
            }
            else if (other.IsBot)
            {
                await ctx.Channel.SendMessageAsync("You can't play against a bot dum dum").ConfigureAwait(false);
                return;
            }
            else if(await gameService.GetDMGame<BattleShip>(ctx.Member.Id) != null || await gameService.GetDMGame<BattleShip>(other.Id) != null)
            {
                await ctx.Channel.SendMessageAsync("One of the players is already in a match").ConfigureAwait(false);
                return;
            }

            var game = gameService.StartGame<BattleShip>(ctx.Guild.Id, new List<DiscordMember>()
            {
                ctx.Member,
                other
            }, ctx.Client);

            if (game == null)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Failed to start match",
                    Color = ModuleInfo.Color
                }.WithFooter($"Member: {ctx.Member.Id}, at {DateTime.Now}"));
            else
                await ctx.Channel.SendMessageAsync("Started").ConfigureAwait(false);
        }

        [Command("UNO")]
        [Description("Play UNO with server members!")]
        public async Task UNO(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync($"Who wants to join in for a game of UNO? React with :arrow_up: on this message when the timer starts to join. React with :arrow_double_up: to force start. Reactions are collected for 1 minute");
            var emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_up:");
            var forceEmoji = DiscordEmoji.FromName(ctx.Client, ":arrow_double_up:");
            List<DiscordMember> players = new List<DiscordMember>();

            await message.CreateReactionAsync(emoji);
            await message.CreateReactionAsync(forceEmoji);

            await Task.Delay(TimeSpan.FromSeconds(1));

            var interactivity = ctx.Client.GetInteractivity();
            bool force = false;

            var time = DateTime.Now;
            var timeSpan = TimeSpan.FromMinutes(1);
            await ctx.Channel.SendMessageAsync("Timer started!");

            while (!force)
            {
                var result = await interactivity.WaitForReactionAsync(x => x.Emoji == emoji || x.Emoji == forceEmoji, timeSpan);

                if(result.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Time Up");
                    break;
                }
             
                if (result.Result.Emoji.Name == forceEmoji.Name)
                    force = true;
                else
                {
                    timeSpan -= DateTime.Now - time;
                    time = DateTime.Now;

                    if (players.FirstOrDefault(x => x.Id == result.Result.User.Id) != null)
                    {
                        await ctx.Channel.SendMessageAsync($"{result.Result.User.Username} you've already joined?");
                        continue;
                    }

                    players.Add(await ctx.Guild.GetMemberAsync(result.Result.User.Id));
                    await ctx.Channel.SendMessageAsync($"{result.Result.User.Username} has joined!");

                    if(players.Count == UNOGame.maxPlayers)
                    {
                        await ctx.Channel.SendMessageAsync($"Max limit ({UNOGame.maxPlayers}) reached");
                    }
                }
            }

            if(players.Count < 2)
            {
                await ctx.Channel.SendMessageAsync("Too less players joined so I ain't starting a match");
                return;
            }

            var game = gameService.StartGame<UNOGame>(ctx.Guild.Id, players, ctx.Client);

            if (game == null)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Failed to start match"
                }.WithFooter($"Member: {ctx.Member.Id}, at {DateTime.Now}"));
            else
                await ctx.Channel.SendMessageAsync("Started").ConfigureAwait(false);
        }

        [Command("Penalty")]
        public async Task Penalty(CommandContext ctx)
        {
            var penalty = await gameService.StartGame<Penalty>(ctx.Member.Id, new List<DiscordMember>() { ctx.Member }, ctx.Client, ctx.Channel, ctx.Message.Id );

            if (penalty == null)
                await ctx.RespondAsync("You're already playing a match of penalty, finish that first");
        }

        [Command("Spectate")]
        [Description("Spectate an ongoing match")]
        public async Task Spectate(CommandContext ctx, DiscordUser user, [RemainingText]string game)
        {
            Type type = Game.GetGameByName(game);

            if (type == null)
            {
                await ctx.Channel.SendMessageAsync("Game not found").ConfigureAwait(false);
                return;
            }

            switch(type)
            {
                case var t when t == typeof(BattleShip):
                    var added = await gameService.AddSpectator<BattleShip>(user.Id, ctx.Member);

                    if (!added)
                        await ctx.Channel.SendMessageAsync("Failed to add spectator").ConfigureAwait(false);
                    break;
                case var t when t == typeof(UNOGame):
                    added = await gameService.AddSpectator<UNOGame>(user.Id, ctx.Member);

                    if (!added)
                        await ctx.Channel.SendMessageAsync("Failed to add spectator").ConfigureAwait(false);
                    break;
            }
        }
    }
}
