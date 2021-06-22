//System name spaces
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

//D# name spaces
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus;
using KunalsDiscordBot.Modules.Games.Complex;
using KunalsDiscordBot.Modules.Games.Complex.Battleship;

namespace KunalsDiscordBot.Modules.Games.Players
{
    public class BattleShipPlayer : DiscordPlayer
    {
        public BattleShipPlayer(DiscordMember _member) : base(_member)
        {
            member = _member;

            ships = new Ship[BattleShip.numOfShips];
        }

        public Ship[] ships { get; private set; }
        private int[,] board;

        private DiscordDmChannel dmChannel { get; set; }

        public async Task<int[,]> GetBoard()
        {
            await Task.CompletedTask;
            return board;
        }

        public async override Task<bool> Ready(DiscordDmChannel channel)
        {
            board = new int[BattleShip.BoardSize, BattleShip.BoardSize];
            dmChannel = channel;

            await Task.CompletedTask;
            return true;
        }

        public async Task<InputResult> GetShips(DiscordClient client)
        {
            var interactivity = client.GetInteractivity();
            var Embed = new DiscordEmbedBuilder
            {
                Title = "Board",
                Description = await GetBoardToPrint(true)
            };

            var setUpMessage = await dmChannel.SendMessageAsync($"This is your Board. \n Every time a ship is entered, it will be edited to show your placement", embed: Embed).ConfigureAwait(false);
            int[] numOfBlocks = BattleShip.shipSizes;

            for (int i = 0; i < BattleShip.numOfShips; i++)
            {
                bool isCompleted = false;

                while (!isCompleted)
                {
                    await dmChannel.SendMessageAsync($"Enter the position for a {numOfBlocks[i] * 2 + 1} unit ship.\n Format -> <Column> <Row> <Placement>. Ex: a 3 h.\n \"h\" for a horzintally placed ship, \"s\" for a vertical one").ConfigureAwait(false);

                    var message = await interactivity.WaitForMessageAsync(x => x.Channel == dmChannel && x.Author == member, TimeSpan.FromSeconds(BattleShip.time)).ConfigureAwait(false);

                    if (message.TimedOut)
                        return new InputResult { wasCompleted = false, type = InputResult.Type.afk };
                    else if (message.Result.Content.ToLower().Equals("end"))
                        return new InputResult { wasCompleted = false, type = InputResult.Type.end };
                    else if (TryParseAndAdd(message.Result.Content, numOfBlocks[i], i))
                        isCompleted = true;
                    else
                        await dmChannel.SendMessageAsync("Invalid position, enter again").ConfigureAwait(false);
                
                    DiscordEmbed embed = new DiscordEmbedBuilder
                    {
                        Title = "Your Board",
                        Description = await GetBoardToPrint(true)
                    };

                    await setUpMessage.ModifyAsync(embed: embed).ConfigureAwait(false);
                }
            }

            await dmChannel.SendMessageAsync("All Ship Positions Recorded").ConfigureAwait(false);

            return new InputResult
            {
                wasCompleted = true,
                type = InputResult.Type.valid
            };
        }

        public async Task<InputResult> GetAttackPos(DiscordClient client, int[,] other)
        {
            var interactivity = client.GetInteractivity();

            bool isCompleted = false;
            CoOrdinate ordinate = new CoOrdinate();

            while (!isCompleted)
            {
                await dmChannel.SendMessageAsync("Its your turn type the position in which you want to attack.\n Format -> <Column> <Row>, Ex: a 6");
                var message = await interactivity.WaitForMessageAsync(x => x.Channel == dmChannel && x.Author == member, TimeSpan.FromSeconds(BattleShip.time)).ConfigureAwait(false);

                if (message.TimedOut)
                    return new InputResult { wasCompleted = false, type = InputResult.Type.afk };
                else if (message.Result.Content.ToLower().Equals("end"))
                    return new InputResult { wasCompleted = false, type = InputResult.Type.end };
                else if (TryParseAndAdd(message.Result.Content, out ordinate) && await IsValidAttackplacement(ordinate, other))
                    isCompleted = true;
                else
                    await dmChannel.SendMessageAsync("Invalid Position");
            }

            return new InputResult
            {
                ordinate = ordinate,
                wasCompleted = true,
                type = InputResult.Type.valid
            };
        }

        private bool TryParseAndAdd(string message, int numberOfBlocks, int shipIndex)
        {
            if (IsFormatted(message, out CoOrdinate position, out bool isVertical))
                return IsValidShipPosition(position, isVertical, numberOfBlocks, shipIndex);
            else
                return false;
        }

        private bool IsValidShipPosition(CoOrdinate position, bool isVertical, int numOfBlocks, int shipIndex)
        {
            if (board[position.y, position.x] != 0)
                return false;

            List<CoOrdinate> validCoOrdinates = new List<CoOrdinate>();
            for (int i = 0; i < 2; i++)
            {
                for (int k = 1; k <= numOfBlocks; k++)
                {
                    if (i == 0)
                    {
                        CoOrdinate ordinate = new CoOrdinate { x = isVertical ? position.x : position.x - k, y = isVertical ? position.y - k : position.y };

                        if (!CoOrdinate.EvaluatePosition(ordinate.x, ordinate.y, BattleShip.BoardSize, BattleShip.BoardSize) || board[ordinate.y, ordinate.x] != 0)
                        {
                            foreach (CoOrdinate coOrdinate in validCoOrdinates)
                                board[coOrdinate.y, coOrdinate.x] = 0;

                            return false;
                        }

                        validCoOrdinates.Add(ordinate);
                        board[ordinate.y, ordinate.x] = -1;
                    }
                    else
                    {
                        CoOrdinate ordinate = new CoOrdinate { x = isVertical ? position.x : position.x + k, y = isVertical ? position.y + k : position.y };

                        if (!CoOrdinate.EvaluatePosition(ordinate.x, ordinate.y, BattleShip.BoardSize, BattleShip.BoardSize) || board[ordinate.y, ordinate.x] != 0)
                        {
                            foreach (CoOrdinate coOrdinate in validCoOrdinates)
                                board[coOrdinate.y, coOrdinate.x] = 0;

                            return false;
                        }

                        validCoOrdinates.Add(ordinate);
                        board[ordinate.y, ordinate.x] = -1;
                    }
                }
            }

            validCoOrdinates.Add(position);

            foreach (CoOrdinate ordinate in validCoOrdinates)
                board[ordinate.y, ordinate.x] = 2;

            List<BattleShipCoOrdinate> ordinates = new List<BattleShipCoOrdinate>();

            foreach (CoOrdinate ordinate in validCoOrdinates)
                ordinates.Add(new BattleShipCoOrdinate { x = ordinate.x, y = ordinate.y, type = BattleShipCoOrdinate.OrdinateType.ship, value = 2 });

            ships[shipIndex] = new Ship(ordinates);

            return true;
        }

        private bool TryParseAndAdd(string message, out CoOrdinate ordinate) => IsFormatted(message, out ordinate) && IsValid(ordinate);

        private bool IsFormatted(string message, out CoOrdinate position, out bool isVertical)
        {
            isVertical = false;
            position = new CoOrdinate();

            if (message.Length < 5 || message.Length > 6)
                return false;

            //gets the column
            if (!char.IsLetter(message[0]))
                return false;

            int index = message[0] - 97;
            if (index < 0 || index > 12)
                return false;

            position.x = index;

            //get the row
            string rowToString = string.Empty;

            for (int i = 2; i < message.Length; i++)
            {
                if (char.IsWhiteSpace(message[i]))
                    break;

                rowToString += message[i];
            }

            if (!int.TryParse(rowToString, out int x))
                return false;

            int row = int.Parse(rowToString) - 1;

            if (row < 0 || row >= BattleShip.BoardSize)
                return false;

            position.y = row;

            //get if straight
            char c = message[message.Length - 1];

            if (!char.IsLetter(c))
                return false;

            isVertical = c.ToString().ToUpper().Equals("S");

            return true;

        }

        private bool IsFormatted(string message, out CoOrdinate ordinate)
        {
            ordinate = new CoOrdinate();

            if (message.Length < 2 || message.Length > 4)
                return false;

            //gets the column
            if (!char.IsLetter(message[0]))
                return false;

            int index = message[0] - 97;
            if (index < 0 || index >= BattleShip.BoardSize)
                return false;

            ordinate.x = index;

            //get the row
            string rowToString = string.Empty;

            for (int i = 2; i < message.Length; i++)
            {
                if (char.IsWhiteSpace(message[i]))
                    break;

                rowToString += message[i];
            }

            if (!int.TryParse(rowToString, out int x))
                return false;

            int row = int.Parse(rowToString) - 1;

            if (row < 0 || row >= BattleShip.BoardSize)
                return false;

            ordinate.y = row;

            return true;
        }

        private bool IsValid(CoOrdinate ordinate) => CoOrdinate.EvaluatePosition(ordinate.x, ordinate.y, BattleShip.BoardSize, BattleShip.BoardSize);

        private async Task<bool> IsValidAttackplacement(CoOrdinate ordinate, int[,] other)
        {
            await Task.CompletedTask;
            return other[ordinate.y, ordinate.x] != 1 && other[ordinate.y, ordinate.x] != 3;
        }

        public async Task<string> GetBoardToPrint(bool ourPlayer = true)
        {
            string boardToString = $"{BattleShip.BLANK}{BattleShip.BLANK}{BattleShip.letters.Aggregate((i, j) => i + j)}\n\n";//im using BLANK here cause its basically a black square acting as a blank space, apparently embeds don't allow spaces

            for (int i = 0; i < board.GetLength(0); i++)
            {
                boardToString += $"{BattleShip.number[i]}{BattleShip.BLANK}";
                for (int k = 0; k < board.GetLength(1); k++)
                {
                    int index = board[i, k];

                    switch (index)
                    {
                        case 0:
                            boardToString += BattleShip.WATER;
                            break;
                        case 1:
                            boardToString += BattleShip.HIT;
                            break;
                        case  var num when num == 2:
                            if (ourPlayer)
                                boardToString += BattleShip.SHIP;
                            else
                                boardToString += BattleShip.WATER;
                            break;
                        case 3:
                            boardToString += BattleShip.SHIPHIT;
                            break;
                    }
                }

                boardToString += "\n";
            }

            await Task.CompletedTask;
            return boardToString;
        }

        public async Task<(bool, bool)> SetAttackPos(CoOrdinate ordinate)
        {
            BattleShipCoOrdinate battleShipCoOrdinate = new BattleShipCoOrdinate { x = ordinate.x, y = ordinate.y };

            board[battleShipCoOrdinate.y, battleShipCoOrdinate.x] = 1;

            foreach(Ship ship in ships)
            {
                (bool hit, bool isDead) = await ship.CheckForHit(battleShipCoOrdinate);

                if (hit)
                {
                    board[battleShipCoOrdinate.y, battleShipCoOrdinate.x] = 3;
                    return (true, isDead);
                }
            }

            await Task.CompletedTask;
            return (false, false);
        }

        public async Task SendMessage(string message) => await dmChannel.SendMessageAsync(message);

        public async Task<bool> CheckIfLost()
        {
            foreach (var ship in ships)
            {
                if (!ship.isDead)
                    return false;
            }

            await Task.CompletedTask;
            return true;
        }
    }
}
