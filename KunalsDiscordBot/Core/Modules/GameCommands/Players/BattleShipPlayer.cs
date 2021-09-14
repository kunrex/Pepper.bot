using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Core.Modules.GameCommands.Battleship;
using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Players
{
    public class BattleShipPlayer : DiscordPlayer<BattleShipCommunicator>
    {
        public BattleShipPlayer(DiscordMember _member) : base(_member)
        {
            ships = new Ship[BattleShip.numOfShips];
        }

        public Ship[] ships { get; private set; }
        private int[,] board;

        public async Task<int[,]> GetBoard()
        {
            await Task.CompletedTask;
            return board;
        }

        public async override Task<bool> Ready(DiscordChannel channel)
        {
            communicator = new BattleShipCommunicator(new Regex("([a-l,A-L]) ([1-9]|1[012])"), TimeSpan.FromSeconds(BattleShip.time), channel, new Regex("([a-l,A-L]) ([1-9]|1[012]) ([hsSH])"));
            board = new int[BattleShip.BoardSize, BattleShip.BoardSize];

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

            var setUpMessage = await communicator.SendMessage($"This is your Board. \n Every time a ship is entered, it will be edited to show your placement", embed: Embed).ConfigureAwait(false);
            int[] numOfBlocks = BattleShip.shipSizes;

            for (int i = 0; i < BattleShip.numOfShips; i++)
            {
                bool isCompleted = false;

                while (!isCompleted)
                {
                    var result = await communicator.ShipInput(interactivity, $"Enter the position for a {numOfBlocks[i] * 2 + 1} unit ship.\n Format -> <Column> <Row> <Placement>. Ex: a 3 h.\n \"h\" for a horzintally placed ship, \"s\" for a vertical one"
                        , new InputData
                        {
                            Conditions = x => x.Channel == communicator.dmChannel && x.Author == member,
                            LeaveMessage = "end",
                            Span = TimeSpan.FromSeconds(BattleShip.time),
                            RegexMatchFailExpression = "Please use the appropriate input format"
                        });

                    if (result.Equals(DiscordCommunicator.afkInputvalue))
                        return new InputResult { WasCompleted = false, Type = InputResult.ResultType.Afk };
                    if (result.Equals(DiscordCommunicator.quitInputvalue))
                        return new InputResult { WasCompleted = false, Type = InputResult.ResultType.End };
                    if (result.Equals(DiscordCommunicator.inputFormatNotFollow))
                        continue;
                    else
                    {
                       if(!TryParseAndAdd(result, numOfBlocks[i], i))
                       {
                            await communicator.SendMessage("Invalid position, enter again");

                            continue;
                       }

                        isCompleted = true;
                    }

                    await communicator.EditMessage(setUpMessage, new DiscordEmbedBuilder
                    {
                        Title = "Your Board",
                        Description = await GetBoardToPrint(true)
                    });
                }
            }

            await communicator.SendMessage("All Ship Positions Recorded").ConfigureAwait(false);

            return new InputResult
            {
                WasCompleted = true,
                Type = InputResult.ResultType.Valid
            };
        }

        public async Task<InputResult> GetAttackPos(DiscordClient client, int[,] other)
        {
            var interactivity = client.GetInteractivity();

            bool isCompleted = false;
            Coordinate ordinate = new Coordinate();

            while (!isCompleted)
            {
                var result = await communicator.Input(interactivity, "Its your turn type the position in which you want to attack.\n Format -> <Column> <Row>, Ex: a 6", new InputData
                {
                    Conditions = x => x.Channel == communicator.dmChannel && x.Author == member,
                    Span = TimeSpan.FromSeconds(BattleShip.time),
                    LeaveMessage = "end",
                     RegexMatchFailExpression = "Please use the appropriate input format"
                });

                if (result.Equals(DiscordCommunicator.afkInputvalue))
                    return new InputResult { WasCompleted = false, Type = InputResult.ResultType.Afk };
                if (result.Equals(DiscordCommunicator.quitInputvalue))
                    return new InputResult { WasCompleted = false, Type = InputResult.ResultType.End };
                else if (result.Equals(DiscordCommunicator.inputFormatNotFollow))
                    continue;
                else if (TryParseAndAdd(result, out ordinate) && await IsValidAttackplacement(ordinate, other))
                    isCompleted = true;
                else
                    await communicator.SendMessage("Invalid Position");
            }

            return new InputResult
            {
                Ordinate = ordinate,
                WasCompleted = true,
                Type = InputResult.ResultType.Valid
            };
        }

        private bool IsValidShipPosition(Coordinate position, bool isVertical, int numOfBlocks, int shipIndex)
        {
            if (board[position.y, position.x] != 0)
                return false;

            List<Coordinate> validCoOrdinates = new List<Coordinate>();
            for (int i = 0; i < 2; i++)
            {
                for (int k = 1; k <= numOfBlocks; k++)
                {
                    if (i == 0)
                    {
                        Coordinate ordinate = new Coordinate { x = isVertical ? position.x : position.x - k, y = isVertical ? position.y - k : position.y };

                        if (!Coordinate.EvaluatePosition(ordinate.x, ordinate.y, BattleShip.BoardSize, BattleShip.BoardSize) || board[ordinate.y, ordinate.x] != 0)
                        {
                            foreach (Coordinate coOrdinate in validCoOrdinates)
                                board[coOrdinate.y, coOrdinate.x] = 0;

                            return false;
                        }

                        validCoOrdinates.Add(ordinate);
                        board[ordinate.y, ordinate.x] = -1;
                    }
                    else
                    {
                        Coordinate ordinate = new Coordinate { x = isVertical ? position.x : position.x + k, y = isVertical ? position.y + k : position.y };

                        if (!Coordinate.EvaluatePosition(ordinate.x, ordinate.y, BattleShip.BoardSize, BattleShip.BoardSize) || board[ordinate.y, ordinate.x] != 0)
                        {
                            foreach (Coordinate coOrdinate in validCoOrdinates)
                                board[coOrdinate.y, coOrdinate.x] = 0;

                            return false;
                        }

                        validCoOrdinates.Add(ordinate);
                        board[ordinate.y, ordinate.x] = -1;
                    }
                }
            }

            validCoOrdinates.Add(position);

            foreach (Coordinate ordinate in validCoOrdinates)
                board[ordinate.y, ordinate.x] = 2;

            List<Coordinate> ordinates = new List<Coordinate>();

            foreach (Coordinate ordinate in validCoOrdinates)
                ordinates.Add(new Coordinate { x = ordinate.x, y = ordinate.y, type = Coordinate.OrdinateType.ship, value = 2 });

            ships[shipIndex] = new Ship(ordinates);

            return true;
        }

        private bool TryParseAndAdd(string message, out Coordinate ordinate) => ExtractCoordinate(message, out ordinate) && IsValid(ordinate);

        private bool TryParseAndAdd(string message, int numberOfBlocks, int shipIndex)
        {
            if (TryExtractValues(message, out Coordinate position, out bool isVertical))
                return IsValidShipPosition(position, isVertical, numberOfBlocks, shipIndex);
            else
                return false;
        }

        private bool TryExtractValues(string message, out Coordinate position, out bool isVertical)
        {
            isVertical = false;
            position = new Coordinate();

            position.x = char.ToLower(message[0]) - 97;
            if (position.x < 0 || position.x >= BattleShip.BoardSize)
                return false;

            position.y = int.Parse(message.Substring(2, message.Length - 3)) - 1;
            if (position.y < 0 || position.y >= BattleShip.BoardSize)
                return false;

            isVertical = message[message.Length - 1].ToString().ToUpper().Equals("S");

            return true;
        }

        private bool ExtractCoordinate(string message, out Coordinate ordinate)
        {
            ordinate = new Coordinate
            {
                x = char.ToLower(message[0]) - 97,
                y = int.Parse(message.Substring(2, message.Length - 2)) - 1
            };

            return true;
        }

        private bool IsValid(Coordinate ordinate) => Coordinate.EvaluatePosition(ordinate.x, ordinate.y, BattleShip.BoardSize, BattleShip.BoardSize);

        private async Task<bool> IsValidAttackplacement(Coordinate ordinate, int[,] other)
        {
            await Task.CompletedTask;
            return other[ordinate.y, ordinate.x] != 1 && other[ordinate.y, ordinate.x] != 3;
        }

        public Task<string> GetBoardToPrint(bool ourPlayer = true)
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

            return Task.FromResult(boardToString);
        }

        public async Task<(bool, bool)> SetAttackPos(Coordinate ordinate)
        {
            Coordinate battleShipCoOrdinate = new Coordinate { x = ordinate.x, y = ordinate.y };

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

        public async Task SendMessage(string message) => await communicator.SendMessage(message);
        public async Task SendMessage(string message, DiscordEmbedBuilder embed) => await communicator.SendMessage(message, embed);
    }
}
