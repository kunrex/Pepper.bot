//System name spaces
using System.Threading.Tasks;
using System.Reflection;
using System;
using System.Collections.Generic;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Attributes;
using KunalsDiscordBot.Services.Images;
using System.Linq;
using KunalsDiscordBot.Core.Attributes.GeneralCommands;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Services;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Core.Attributes.GameCommands;
using KunalsDiscordBot.Services.Games;

namespace KunalsDiscordBot.Modules.Games
{
    [Group("Games")]
    [Decor("IndianRed", ":video_game:")]
    [Description("A set of commands to play popular games with other server members")]
    [ModuleLifespan(ModuleLifespan.Transient), ConfigData(ConfigValueSet.Games)]     
    public class GameCommands : BaseCommandModule
    {
        public static readonly DiscordColor Color = typeof(GameCommands).GetCustomAttribute<DecorAttribute>().color;

        private readonly IServerService serverService;
        private readonly IGameService gameService;

        public GameCommands(IServerService service, IGameService _gameService)
        {
            serverService = service;
            gameService = _gameService;
        }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var configPermsCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckConfigPermsAttribute) != null;

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
        [CheckConfigPerms, ConfigData(ConfigValue.Connect4Channel)]
        public async Task Connect4Channel(CommandContext ctx, DiscordChannel channel)
        {
            await serverService.SetConnect4Channel(ctx.Guild.Id, channel.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Saved {channel.Mention} as the Connect4 channel for guild: `{ctx.Guild.Name}`",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("TicTacToeChannel")]
        [Description("Assigns the tictactoe channel for a server")]
        [CheckConfigPerms, ConfigData(ConfigValue.TicTacToeChannel)]
        public async Task TicTacToeChannel(CommandContext ctx, DiscordChannel channel)
        {
            await serverService.SetTicTacToeChannel(ctx.Guild.Id, channel.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Saved {channel.Mention} as the TicTacToe channel for guild: `{ctx.Guild.Name}`",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
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
                    Color = Color
                }.WithFooter($"Member: {ctx.Member.Username}, at {DateTime.Now}"));
        }

        [Command("TicTacToe")]
        [Description("The TicTacToe game, play with a friend"), RequireTicTacToeChannel]
        public async Task TicTacToe(CommandContext ctx, DiscordMember other, int numberOfCells = 3)
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
            }, ctx.Client, ctx.Guild.GetChannel((ulong)(await serverService.GetGameData(ctx.Guild.Id)).TicTacToeChannel), numberOfCells);

            Console.WriteLine((game == null) + "hahahaha");

            if (game == null)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Failed to start match, If a game already is going on in the server then wait for it to finish.",
                    Color = Color
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
                    Color = Color
                }.WithFooter($"Member: {ctx.Member.Id}, at {DateTime.Now}"));
            else
                await ctx.Channel.SendMessageAsync("Started").ConfigureAwait(false);
        }

        //AI
        [Command("RPS")]
        [Description("Rock Paper Scissors")]
        public async Task RockPaperScissors(CommandContext ctx, string option)
        {
            int optionToInt = 0;
            switch (option.ToLower())
            {
                case var val when val == "paper" || val == "p":
                    optionToInt = 1;
                    break;
                case var val when val == "scissors" || val == "s":
                    optionToInt = 2;
                    break;
            }

            RockPaperScissor rockPaperScissor = new RockPaperScissor(optionToInt, ctx);
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
