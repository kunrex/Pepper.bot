using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using KunalsDiscordBot.Modules.Games.Players;

namespace KunalsDiscordBot.Modules.Games.Complex.Pacman
{
    public enum PacmanControl
    {
        Up,
        Right,
        Down,
        Left,
        Help,
        Quit
    }

    public class PacMan : ComplexBoardGame<PacmanPlayer>
    {
        public DiscordChannel channel { get; private set; }
        public DiscordClient client { get; private set; }

        private DiscordMessage boardMessage { get; set; }
        private DiscordMessage dataMessage { get; set; }

        private int totalPellets { get; set; }
        private int pelletsCollected { get; set; }
        private int afkStreak { get; set; }

        private const string ghost = " G ";
        private const string pellet = " • ";
        private const string space = "   ";
        private const string player = " P ";
        private const string wall = "[ ]";
        private const int afkStopPLaying = 30;

        private readonly Dictionary<PacmanControl, DiscordEmoji> controls;

        private bool gameOver { get; set; } = false;
        private bool shrink { get; set; } = false;

        int[,] board = new int[25, 18]
        {
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1},
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1},
            {2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 1, 2},
            {2, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2},
            {2, 1, 2, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2},
            {2, 1, 2, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 2, 1, 2},
            {2, 1, 2, 1, 2, 1, 2, 2, 2, 2, 2, 2, 1, 2, 1, 2, 1, 2},
            {2, 1, 2, 1, 1, 1, 2, 0, 2, 2, 0, 2, 1, 2, 1, 2, 1, 2},
            {2, 1, 2, 1, 2, 1, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 2},
            {2, 1, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 1, 2, 1, 2},
            {2, 1, 1, 1, 2, 1, 2, 0, 2, 2, 0, 2, 1, 2, 1, 2, 1, 2},
            {2, 2, 2, 1, 2, 1, 2, 2, 2, 2, 2, 2, 1, 2, 1, 2, 1, 2},
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {2, 1, 2, 1, 2, 1, 2, 2, 0, 0, 2, 2, 1, 2, 1, 2, 1, 2},
            {2, 1, 2, 1, 2, 1, 2, 0, 0, 0, 0, 2, 1, 2, 1, 1, 1, 2},
            {2, 1, 2, 1, 1, 1, 2, 4, 4, 4, 4, 2, 1, 2, 1, 2, 1, 2},
            {2, 1, 2, 1, 2, 1, 2, 2, 2, 2, 2, 2, 1, 2, 1, 2, 1, 2},
            {2, 1, 2, 1, 2, 1, 1, 1, 1, 3, 1, 1, 1, 1, 1, 2, 1, 2},
            {2, 1, 1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 1, 2, 1, 2},
            {2, 1, 2, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 2, 1, 2},
            {2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2},
            {2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {2, 1, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 1},
            {2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        };

        public PacMan(DiscordMember member, DiscordChannel _channel, DiscordClient _client)
        {
            players = new List<PacmanPlayer>();

            client = _client;
            channel = _channel;

            controls = new Dictionary<PacmanControl, DiscordEmoji>
            {
                {PacmanControl.Up, DiscordEmoji.FromName(client, ":arrow_up:")},
                {PacmanControl.Down,DiscordEmoji.FromName(client, ":arrow_down:")},
                {PacmanControl.Right,  DiscordEmoji.FromName(client, ":arrow_right:")},
                {PacmanControl.Left, DiscordEmoji.FromName(client, ":arrow_left:")},
            };

            var player = new PacmanPlayer(member, controls);
            players.Add(player);
            currentPlayer = player;

            SetUp();
        }

        protected async override void SetUp()
        {
            totalPellets = FindNumOfPellets(out var playerPos);
            currentPlayer.InitilizePlayer(playerPos, PacmanPlayer.Direction.left);

            await MessageSetUp();
            await currentPlayer.Ready(channel);
            currentPlayer.OnPelletCollected += OnPelletCollected;

            PlayGame();
        }

        private void OnPelletCollected(int val) => pelletsCollected = val;

        protected async Task MessageSetUp()
        {
            string description = GetBoard();

            await channel.SendMessageAsync("**Pac Man**").ConfigureAwait(false);
            Console.WriteLine(description.Length);

            boardMessage = await channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Description = description
            }).ConfigureAwait(false);
            currentPlayer.SetBoardMessage(boardMessage);

            foreach (var emoji in controls)
                await boardMessage.CreateReactionAsync(emoji.Value).ConfigureAwait(false);

            dataMessage = await channel.SendMessageAsync(new DiscordEmbedBuilder()
            .AddField("Total Pellets: ", totalPellets.ToString())
            .AddField("Pellets: ", pelletsCollected.ToString())).ConfigureAwait(false);
        }

        protected async override void PlayGame()
        {
            while (!gameOver)
            {
                await PrintBoard().ConfigureAwait(false);
                shrink ^= true;

                var result = await currentPlayer.GetInput(client, channel);

                if (!result.wasCompleted)
                {
                    afkStreak++;
                    if(afkStreak >= afkStopPLaying)
                    {
                        //end game
                    }
                }

                await UpdateBoard(result.ordinate);
            }
        }

        private async Task UpdateBoard(Coordinate newPlayerCoordinate)
        {
            int val = board[newPlayerCoordinate.y, newPlayerCoordinate.x];

            if (val == 2)//wall
                return;

            var direction = Coordinate.GetDirection(currentPlayer.position, newPlayerCoordinate);

            board[currentPlayer.position.y, currentPlayer.position.x] = 0;

            currentPlayer.ChangePosAndDirection(newPlayerCoordinate, direction);
            board[currentPlayer.position.y, currentPlayer.position.x] = 3;

            await Task.CompletedTask;
        }

        protected async override Task PrintBoard()
        {
            DiscordEmbed embed = new DiscordEmbedBuilder
            {
                Description = GetBoard()
            };

            await boardMessage.ModifyAsync(embed).ConfigureAwait(false);

            embed = new DiscordEmbedBuilder()
                .AddField("Total Pellets: ", totalPellets.ToString())
                .AddField("Pellets: ", pelletsCollected.ToString());
            await dataMessage.ModifyAsync(embed);
        }

        private int FindNumOfPellets(out Coordinate playerPos)
        {
            playerPos = new Coordinate();
            int num = 0;

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int k = 0; k < board.GetLength(1); k++)
                {
                    if (board[i, k] == 1)
                        num++;

                    if (board[i, k] == 3)
                        playerPos = new Coordinate
                        {
                            x = k,
                            y = i
                        };
                }
            }

            return num;
        }

        private string GetBoard()
        {
            string description = "```css\n";

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int k = 0; k < board.GetLength(1); k++)
                {
                    switch (board[i, k])
                    {
                        case 0:
                            description += space;
                            break;
                        case 1:
                            description += pellet;
                            break;
                        case 2:
                            description += wall;
                            break;
                        case 3:
                            description += shrink ? player.ToLower() : player;
                            break;
                        case 4:
                            description += shrink ? ghost.ToLower() : ghost; 
                            break;
                    }
                }

                description += "\n";
            }
            description += "```";

            return description;
        }
    }
}
