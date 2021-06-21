//System name spaces
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using DSharpPlus;

using KunalsDiscordBot.Modules.Games.Players;

namespace KunalsDiscordBot.Modules.Games
{
    public class BattleShip : Game
    {
        public static readonly int BoardSize = 10;

        public BattleShipPlayer player1 { get; private set; }
        public BattleShipPlayer player2 { get; private set; }
        private BattleShipPlayer currentPlayer { get; set; }

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

        public BattleShip(DiscordMember user1, DiscordMember user2, DiscordClient _client)
        {
            player1 = new BattleShipPlayer(user1);
            player2 = new BattleShipPlayer(user2);
            client = _client;

            gameOver = false;

            currentPlayers.Add(player1);
            currentPlayers.Add(player2);

            SetUp();
        }

        private async void SetUp()
        {
            //create dms
            ctx1 = await player1.member.CreateDmChannelAsync().ConfigureAwait(false);
            ctx2 = await player2.member.CreateDmChannelAsync().ConfigureAwait(false);

            //ready
            await player1.Ready(ctx1);
            await player2.Ready(ctx2);

            //run both input methods "simultaneously"
            var player1Ships = Task.Run(() => player1.GetShips(client));
            var player2Ships = Task.Run(() => player2.GetShips(client));

            await player1Ships;
            await player2Ships;

            if (!player1Ships.Result.wasCompleted)//checks if any one ended the game
            {
                await SendMessageToBoth($"{player1.member.Username} {(player1Ships.Result.type == InputResult.Type.end ? "has ended the game." : "has gone AFK") }");
                await RemovePlayers();
                return;
            }
            else if(!player2Ships.Result.wasCompleted)
            {
                await SendMessageToBoth($"{player2.member.Username} {(player2Ships.Result.type == InputResult.Type.end ? "has ended the game." : "has gone AFK") }");
                await RemovePlayers();
                return;
            }

            //both players are ready to players
            await SendMessageToBoth("Both players ready, starting game");

            PlayGame();
        }

        private async Task RemovePlayers()
        {
            currentPlayers.Remove(player1);
            currentPlayers.Remove(player2);

            await Task.CompletedTask;
        }

        private async void PlayGame()
        {
            //print the board for both players
            await PrintBoard();
            currentPlayer = player1;

            try
            {
                while (!gameOver)
                {
                    var otherPlayer = currentPlayer == player1 ? player2 : player1;

                    var result = await currentPlayer.GetAttackPos(client, await otherPlayer.GetBoard());

                    if (!result.wasCompleted)//checks if player one ended the game
                    {
                        await SendMessageToBoth($"{currentPlayer.member.Username} {(result.type == InputResult.Type.end ? "has ended the game." : "has gone AFK") }");
                        gameOver = true;

                        await RemovePlayers();

                        return;
                    }

                    await SendMessageToBoth($"Position entered by {currentPlayer.member.Username} => {(char)(result.ordinate.x + 97)} {result.ordinate.y + 1}");

                    (bool hit, bool isDead) = await otherPlayer.SetAttackPos(result.ordinate);

                    if (hit)
                    {
                        await currentPlayer.SendMessage($"{(isDead ? "A ship has been sunk!" : "A ship was hit!")}");
                        await otherPlayer.SendMessage($"{(isDead ? "Your ship has been sunk!" : "Your ship has hit!")}");
                    }
                    else
                        await SendMessageToBoth("miss!");

                    bool lose = await otherPlayer.CheckIfLost();

                    if(lose)
                    {
                        await SendMessageToBoth($"{currentPlayer.member.Username} Wins!");
                        await PrintBoard();

                        await RemovePlayers();

                        gameOver = true;
                        return;
                    }

                    await PrintBoard();
                    currentPlayer = currentPlayer == player1 ? player2 : player1;
                }
            }
            catch(Exception e)
            {
                await SendMessageToBoth($"Unkown error -  {e.Message}  occured, tell Kunal").ConfigureAwait(false);
                gameOver = true;

                await RemovePlayers();

                return;
            }
        }

        private async Task PrintBoard()
        {
            var boardForPlayer1 = Task.Run(() => PrintBoardForPlayer(player1, player2));
            var boardForPlayer2 = Task.Run(() => PrintBoardForPlayer(player2, player1));

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

            var userChannel = currentPlayer == player1 ? ctx1 : ctx2;

            if (userChannel.Equals(null))
                return false;

            await userChannel.SendMessageAsync("Your Board -", embed: BoardEmbed).ConfigureAwait(false);
            await userChannel.SendMessageAsync("Your Attacks -", embed: AttackEmbed).ConfigureAwait(false);

            return true;
        }
    }
}
