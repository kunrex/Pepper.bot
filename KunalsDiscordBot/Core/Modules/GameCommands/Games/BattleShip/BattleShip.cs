using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Events;
using KunalsDiscordBot.Core.Modules.GameCommands.Players;
using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;
using KunalsDiscordBot.Core.Modules.GameCommands.Players.Spectators;

namespace KunalsDiscordBot.Core.Modules.GameCommands
{
    public sealed class BattleShip : DiscordGame<BattleShipPlayer, BattleShipCommunicator>, ISpectatorGame
    {
        public static readonly int BoardSize = 10;

        public static readonly string WATER = ":blue_circle:";
        public static readonly string HIT = ":white_circle:";
        public static readonly string SHIP = ":brown_square:";
        public static readonly string BLANK = ":black_large_square:";
        public static readonly string SHIPHIT = ":red_square:";

        public static readonly int time = 60;
        public static readonly int numOfShips = 5;

        public static readonly string[] letters = { ":regional_indicator_a:", ":regional_indicator_b:", ":regional_indicator_c:", ":regional_indicator_d:", ":regional_indicator_e:", ":regional_indicator_f:", ":regional_indicator_g:", ":regional_indicator_h:", ":regional_indicator_i:", ":regional_indicator_j:" };
        public static readonly string[] number = { ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:", ":nine:", ":keycap_ten:" };
        public static readonly int[] shipSizes = { 1, 1, 2, 2, 3 };

        public List<DiscordSpectator> Spectators { get; set; } = new List<DiscordSpectator>();

        public BattleShip(DiscordClient _client, List<DiscordMember> _players) : base(_client, _players)
        {
            Players = _players.Select(x => new BattleShipPlayer(x)).ToList();

            OnGameOver = new SimpleBotEvent();
            SetUp();
        }

        protected override async void SetUp()
        {
            await Players[0].Ready(await Players[0].member.CreateDmChannelAsync().ConfigureAwait(false));
            await Players[1].Ready(await Players[1].member.CreateDmChannelAsync().ConfigureAwait(false));

            await SendMessageToAllSpectators("Both players ready to play, starting ship position placement...");

            //run both input methods "simultaneously"
            var player1Ships = Task.Run(() => Players[0].GetShips(Client));
            var player2Ships = Task.Run(() => Players[1].GetShips(Client));

            await player1Ships;
            await player2Ships;

            if (!player1Ships.Result.WasCompleted)//checks if any one ended the game
            {
                string message = $"{Players[0].member.Username} {(player1Ships.Result.Type == InputResult.ResultType.End ? "has ended the game." : "has gone AFK") }";

                await SendMessageToBoth(message);
                await SendMessageToAllSpectators(message);

                await RemovePlayers();
                return;
            }
            else if (!player2Ships.Result.WasCompleted)
            {
                string message = $"{Players[1].member.Username} {(player2Ships.Result.Type == InputResult.ResultType.End ? "has ended the game." : "has gone AFK") }";

                await SendMessageToBoth(message);
                await SendMessageToAllSpectators(message);

                await RemovePlayers();
                return;
            }

            //both players are ready to players
            await SendMessageToBoth("Both players ready, starting game");
            await SendMessageToAllSpectators("Both players ready, starting game");

            PlayGame();
        }

        protected override async void PlayGame()
        {
            //print the board for both players
            await PrintBoard();
            await PrintBoardForSpectators();

            CurrentPlayer = Players[0];

            try
            {
                while (!GameOver)
                {
                    var otherPlayer = CurrentPlayer == Players[0] ? Players[1] : Players[0];

                    var result = await CurrentPlayer.GetAttackPos(Client, await otherPlayer.GetBoard());

                    if (!result.WasCompleted)//checks if player one ended the game
                    {
                        string endMessage = $"{CurrentPlayer.member.Username} {(result.Type == InputResult.ResultType.End ? "has ended the game." : "has gone AFK") }";

                        await SendMessageToBoth(endMessage);
                        await SendMessageToAllSpectators(endMessage);

                        GameOver = true;

                        await RemovePlayers();

                        continue;
                    }

                    var message = $"Position entered by {CurrentPlayer.member.Username} => {(char)(result.Ordinate.x + 97)} {result.Ordinate.y + 1}";

                    await SendMessageToBoth(message);
                    await SendMessageToAllSpectators(message);

                    (bool hit, bool isDead) = await otherPlayer.SetAttackPos(result.Ordinate);

                    if (hit)
                    {
                        await CurrentPlayer.SendMessage($"{(isDead ? "A ship has been sunk!" : "A ship was hit!")}");
                        await otherPlayer.SendMessage($"{(isDead ? "Your ship has been sunk!" : "Your ship has hit!")}");

                        await SendMessageToAllSpectators($"{(isDead ? $"{otherPlayer.member.Username}'s ship has been sunk" : $"{otherPlayer.member.Username}'s ship has been hit")}");
                    }
                    else
                    {
                        var missMessage = "miss!";

                        await SendMessageToBoth(missMessage);
                        await SendMessageToAllSpectators(missMessage);
                    }

                    bool lose = await otherPlayer.CheckIfLost();

                    if (lose)
                    {
                        var winMessage = $"{CurrentPlayer.member.Username} Wins!";

                        await SendMessageToBoth(winMessage);
                        await SendMessageToAllSpectators(winMessage);

                        await PrintBoard();
                        await PrintBoardForSpectators();

                        await RemovePlayers();

                        GameOver = true;
                        continue;
                    }

                    await PrintBoard();
                    await PrintBoardForSpectators();

                    CurrentPlayer = CurrentPlayer == Players[0] ? Players[1] : Players[0];
                }
            }
            catch (Exception e)
            {
                await SendMessageToAllSpectators("Unknown error occured ending spectation");
                await SendMessageToBoth($"Unkown error -  {e.Message}  occured").ConfigureAwait(false);

                GameOver = true;
                await RemovePlayers();
            }

            OnGameOver.Invoke();
        }

        protected override async Task PrintBoard()
        {
            var boardForPlayer1 = Task.Run(() => PrintBoardForPlayer(Players[0], Players[1]));
            var boardForPlayer2 = Task.Run(() => PrintBoardForPlayer(Players[1], Players[0]));

            await boardForPlayer1;
            await boardForPlayer2;
        }

        private async Task RemovePlayers()
        {
            foreach (var spectator in Spectators)
                spectator.End();

            Spectators = null;

            await Task.CompletedTask;
        }

        private async Task SendMessageToBoth(object message)
        {
            foreach (var player in Players)
                await player.SendMessage($"{message}").ConfigureAwait(false);
        }

        private async Task<bool> PrintBoardForPlayer(BattleShipPlayer currentPlayer, BattleShipPlayer otherPlayer)
        {
            var currentPlayerBoard = await currentPlayer.GetBoardToPrint(true);
            var otherPlayerBoard = await otherPlayer.GetBoardToPrint(false);
            var vs = $"{currentPlayer.member.Username} vs {otherPlayer.member.Username}";

            var BoardEmbed = new DiscordEmbedBuilder
            {
                Title = vs,
                Description = currentPlayerBoard
            };

            var AttackEmbed = new DiscordEmbedBuilder
            {
                Title = vs,
                Description = otherPlayerBoard
            };

            await currentPlayer.SendMessage("Your Board", BoardEmbed).ConfigureAwait(false);
            await currentPlayer.SendMessage("Your Attacks", AttackEmbed).ConfigureAwait(false);

            return true;
        }

        private async Task PrintBoardForSpectators()
        {
            var currentPlayerBoard = await Players[0].GetBoardToPrint(false);
            var otherPlayerBoard = await Players[1].GetBoardToPrint(false);

            var plyer1Board = new DiscordEmbedBuilder
            {
                Title = $"{Players[0].member.Username}'s board",
                Description = currentPlayerBoard
            };

            var player2Board = new DiscordEmbedBuilder
            {
                Title = $"{Players[1].member.Username}'s board",
                Description = otherPlayerBoard
            };

            await SendMessageToAllSpectators(embed: plyer1Board);
            await SendMessageToAllSpectators(embed: player2Board);
        }

        private async Task SendMessageToAllSpectators(string message = null, DiscordEmbedBuilder embed = null)
        {
            List<Task> tasks = new List<Task>();
            foreach (var spectator in Spectators)
            {
                if (message != null)
                    tasks.Add(Task.Run(() => spectator.SendMessage(message)));
                if (embed != null)
                    tasks.Add(Task.Run(() => spectator.SendMessage(embed)));
            }

            foreach (var task in tasks)
                await task;
        }

        public async Task<bool> AddSpectator(DiscordMember _member)
        {
            if (Spectators.Count == maxSpectators || Players.FirstOrDefault(x => x.member.Id == _member.Id) != null)
                return false;

            var spectator = new DiscordSpectator(_member, Client, this);

            Spectators.Add(spectator);
            var channel = await _member.CreateDmChannelAsync();

            await spectator.Ready(channel);
            return true;
        }
    }
}