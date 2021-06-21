//System name spaces
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

//D# name spaces
using DSharpPlus.Entities;
using DSharpPlus;

using KunalsDiscordBot.Modules.Games.Players;
using KunalsDiscordBot.Modules.Games.Players.Spectators;

namespace KunalsDiscordBot.Modules.Games.Complex
{
    public class BattleShip : ComplexBoardGame<BattleShipPlayer>
    {
        public static readonly int BoardSize = 10;

        public DiscordDmChannel ctx1 { get; private set; }
        public DiscordDmChannel ctx2 { get; private set; }

        public DiscordClient client;

        private bool gameOver { get; set; }

        public static readonly string WATER = ":blue_circle:";
        public static readonly string HIT = ":white_circle:";
        public static readonly string SHIP = ":brown_square:";
        public static readonly string BLANK = ":black_large_square:";
        public static readonly string SHIPHIT = ":red_square:";

        public static readonly int time = 60;
        public static readonly int numOfShips = 5;

        public static readonly string[] letters = { ":regional_indicator_a:", ":regional_indicator_b:", ":regional_indicator_c:", ":regional_indicator_d:", ":regional_indicator_e:", ":regional_indicator_f:", ":regional_indicator_g:", ":regional_indicator_h:", ":regional_indicator_i:", ":regional_indicator_j:" };
        public static readonly string[] number = { ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:", ":nine:", ":keycap_ten:" };

        public static List<BattleShipPlayer> currentPlayers = new List<BattleShipPlayer>();
        private List<DiscordSpectator> spectators = new List<DiscordSpectator>();

        public BattleShip(DiscordMember user1, DiscordMember user2, DiscordClient _client)
        {
            players = new List<BattleShipPlayer>();

            var player1 = new BattleShipPlayer(user1, this);
            var player2 = new BattleShipPlayer(user2, this);

            players.Add(player1);
            players.Add(player2);

            currentPlayers.Add(player1);
            currentPlayers.Add(player2);

            client = _client;

            gameOver = false;

            SetUp();
        }

        protected override async void SetUp()
        {
            //create dms
            ctx1 = await players[0].member.CreateDmChannelAsync().ConfigureAwait(false);
            ctx2 = await players[1].member.CreateDmChannelAsync().ConfigureAwait(false);

            //ready
            await players[0].Ready(ctx1);
            await players[1].Ready(ctx2);

            await SendMessageToAllSpectators("Both players ready to play, starting ship position placement...");

            //run both input methods "simultaneously"
            var player1Ships = Task.Run(() => players[0].GetShips(client));
            var player2Ships = Task.Run(() => players[1].GetShips(client));

            await player1Ships;
            await player2Ships;

            if (!player1Ships.Result.wasCompleted)//checks if any one ended the game
            {
                string message = $"{players[0].member.Username} {(player1Ships.Result.type == InputResult.Type.end ? "has ended the game." : "has gone AFK") }";

                await SendMessageToBoth(message);
                await SendMessageToAllSpectators(message);

                await RemovePlayers();
                return;
            }
            else if(!player2Ships.Result.wasCompleted)
            {
                string message = $"{players[1].member.Username} {(player2Ships.Result.type == InputResult.Type.end ? "has ended the game." : "has gone AFK") }";

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

        private async Task RemovePlayers()
        {
            currentPlayers.Remove(players[0]);
            currentPlayers.Remove(players[1]);

            foreach (var spectator in spectators)
                spectator.End();

            spectators = null;

            await Task.CompletedTask;
        }

        protected override async void PlayGame()
        {
            //print the board for both players
            await PrintBoard();
            await PrintBoardForSpectators();

            currentPlayer = players[0];

            try
            {
                while (!gameOver)
                {
                    var otherPlayer = currentPlayer == players[0] ? players[1] : players[0];

                    var result = await currentPlayer.GetAttackPos(client, await otherPlayer.GetBoard());

                    if (!result.wasCompleted)//checks if player one ended the game
                    {
                        string endMessage = $"{currentPlayer.member.Username} {(result.type == InputResult.Type.end ? "has ended the game." : "has gone AFK") }";

                        await SendMessageToBoth(endMessage);
                        await SendMessageToAllSpectators(endMessage);

                        gameOver = true;

                        await RemovePlayers();

                        return;
                    }

                    var message = $"Position entered by {currentPlayer.member.Username} => {(char)(result.ordinate.x + 97)} {result.ordinate.y + 1}";

                    await SendMessageToBoth(message);
                    await SendMessageToAllSpectators(message);

                    (bool hit, bool isDead) = await otherPlayer.SetAttackPos(result.ordinate);

                    if (hit)
                    {
                        await currentPlayer.SendMessage($"{(isDead ? "A ship has been sunk!" : "A ship was hit!")}");
                        await otherPlayer.SendMessage($"{(isDead ? "Your ship has been sunk!" : "Your ship has hit!")}");

                        await SendMessageToAllSpectators($"{(isDead ? $"{otherPlayer.member.Username}'s ship has been sunk": $"{otherPlayer.member.Username}'s ship has been hit")}");
                    }
                    else
                    {
                        var missMessage = "miss!";

                        await SendMessageToBoth(missMessage);
                        await SendMessageToAllSpectators(missMessage);
                    }

                    bool lose = await otherPlayer.CheckIfLost();

                    if(lose)
                    {
                        var winMessage = $"{currentPlayer.member.Username} Wins!";

                        await SendMessageToBoth(winMessage);
                        await SendMessageToAllSpectators(winMessage);

                        await PrintBoard();
                        await PrintBoardForSpectators();

                        await RemovePlayers();

                        gameOver = true;
                        return;
                    }

                    await PrintBoard();
                    await PrintBoardForSpectators();

                    currentPlayer = currentPlayer == players[0] ? players[1] : players[0];
                }
            }
            catch(Exception e)
            {
                await SendMessageToAllSpectators("Unknown error occured ending spectation");
                await SendMessageToBoth($"Unkown error -  {e.Message}  occured, tell Kunal").ConfigureAwait(false);

                gameOver = true;
                await RemovePlayers();

                return;
            }
        }

        protected override async Task PrintBoard()
        {
            var boardForPlayer1 = Task.Run(() => PrintBoardForPlayer(players[0], players[1]));
            var boardForPlayer2 = Task.Run(() => PrintBoardForPlayer(players[1], players[0]));

            await boardForPlayer1;
            await boardForPlayer2;
        }

        private async Task SendMessageToBoth(object message)
        {
            await ctx1.SendMessageAsync($"{message}").ConfigureAwait(false);
            await ctx2.SendMessageAsync($"{message}").ConfigureAwait(false);
        }

        private async Task<bool> PrintBoardForPlayer(BattleShipPlayer currentPlayer, BattleShipPlayer otherPlayer)
        {
            var currentPlayerBoard = await currentPlayer.GetBoardToPrint(true);
            var otherPlayerBoard = await otherPlayer.GetBoardToPrint(false);

            var BoardEmbed = new DiscordEmbedBuilder
            {
                Title = $"{currentPlayer.member.Username} vs {otherPlayer.member.Username}",
                Description = currentPlayerBoard
            };

            var AttackEmbed = new DiscordEmbedBuilder
            {
                Title = $"{currentPlayer.member.Username} vs {otherPlayer.member.Username}",
                Description = otherPlayerBoard
            };

            var userChannel = currentPlayer == players[0] ? ctx1 : ctx2;

            if (userChannel.Equals(null))
                return false;

            await userChannel.SendMessageAsync("Your Board -", embed: BoardEmbed).ConfigureAwait(false);
            await userChannel.SendMessageAsync("Your Attacks -", embed: AttackEmbed).ConfigureAwait(false);

            return true;
        }

        private async Task PrintBoardForSpectators()
        {
            var currentPlayerBoard = await players[0].GetBoardToPrint(false);
            var otherPlayerBoard = await players[1].GetBoardToPrint(false);

            var plyer1Board = new DiscordEmbedBuilder
            {
                Title = $"{players[0].member.Username}'s board",
                Description = currentPlayerBoard
            };

            var player2Board = new DiscordEmbedBuilder
            {
                Title = $"{players[1].member.Username}'s board",
                Description = otherPlayerBoard
            };

            await SendMessageToAllSpectators(embed: plyer1Board);
            await SendMessageToAllSpectators(embed: player2Board);
        }

        private async Task SendMessageToAllSpectators(string message = null, DiscordEmbedBuilder embed = null)
        {
            List<Task> tasks = new List<Task>();
            foreach(var spectator in spectators)
            {             
                if (message != null)
                    tasks.Add(Task.Run(() => spectator.SendMessage(message)));
                if (embed != null)
                    tasks.Add(Task.Run(() => spectator.SendMessage(embed)));
            }

            foreach (var task in tasks)
                await task;
        }

        public async Task AddSpectator(DiscordMember _member)
        {
            var spectator = new DiscordSpectator(_member, client, this);

            spectators.Add(spectator);
            var channel = await _member.CreateDmChannelAsync();

            await spectator.Ready(channel);
        }

        public async Task RemoveSpectator(DiscordSpectator spectator)
        {
            spectators.Remove(spectators.Find(x => x.member.Id == spectator.member.Id));
            await Task.CompletedTask;
        }
    }
}
