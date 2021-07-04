using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using KunalsDiscordBot.Modules.Games.Complex.Pacman;
using System.Linq;
using DSharpPlus.Interactivity.EventHandling;

namespace KunalsDiscordBot.Modules.Games.Players
{
    public sealed class PacmanPlayer : DiscordPlayer
    {
        public enum Direction
        {
            up,
            down,
            right,
            left
        }

        public PacmanPlayer(DiscordMember _member, Dictionary<PacmanControl, DiscordEmoji> _controls) : base(_member)
        {
            member = _member;
            controls = _controls;
        }

        private DiscordMessage boardMessage { get; set; }

        public Direction direction { get; private set; }
        public Coordinate position { get; private set; }

        private readonly Dictionary<PacmanControl, DiscordEmoji> controls;

        private int pellets;
        public int Pellets
        {
            get => pellets;

            private set
            {
                pellets = value;
                if (OnPelletCollected != null)
                    OnPelletCollected(pellets);
            }
        }

        public Action<int> OnPelletCollected { get; set; }
        public async override Task<bool> Ready(DiscordChannel channel)
        {
            var message = await channel.SendMessageAsync("Starting Game in\n3").ConfigureAwait(false);

            for(int i = 3; i >= 1;i--)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                await message.ModifyAsync($"Starting Game in\n{i - 1}").ConfigureAwait(false);
            }
            pellets = 0;

            return true;
        }

        public void InitilizePlayer(Coordinate ordinate, Direction dir)
        {
            position = ordinate;
            direction = dir;
        }

        public void SetBoardMessage(DiscordMessage messgage) => boardMessage = messgage;

        public async Task<InputResult> GetInput(DiscordClient client, DiscordChannel channel)
        {
            var interactivity = client.GetInteractivity();

            var reaction = await interactivity.WaitForReactionAsync(x => x.Channel.Id == channel.Id && x.User.Id == member.Id && x.Message.Id == boardMessage.Id, TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            if (reaction.TimedOut)
                return new InputResult
                {
                    wasCompleted = false,
                    type = InputResult.Type.afk,
                    ordinate = CalculatePosition(direction)
                };
            else
            {
                Console.WriteLine(reaction.Result.Emoji.Name);

                var ordinate = new Coordinate();

                switch(reaction.Result.Emoji.Name)
                {
                    case var x when x == controls[PacmanControl.Up].Name:
                        ordinate.x = position.x;
                        ordinate.y = position.y - 1;
                        break;
                    case var x when x == controls[PacmanControl.Down].Name:
                        ordinate.x = position.x;
                        ordinate.y = position.y + 1;
                        break;
                    case var x when x == controls[PacmanControl.Right].Name:
                        ordinate.x = position.x + 1;
                        ordinate.y = position.y;
                        break;
                    case var x when x == controls[PacmanControl.Left].Name:
                        ordinate.x = position.x - 1;
                        ordinate.y = position.y;
                        break;
                    default:
                        return new InputResult
                        {
                            wasCompleted = true,
                            type = InputResult.Type.inValid
                        };
                }

                return new InputResult
                {
                    wasCompleted = true,
                    type = InputResult.Type.valid,
                    ordinate = ordinate
                };
            }

        }

        public void ChangePosAndDirection(Coordinate _position, Direction dir)
        {
            position = _position;
            direction = dir;
        }

        private Coordinate CalculatePosition(Direction dir)
        {
            switch(dir)
            {
                case Direction.up:
                    return new Coordinate
                    {
                        x = position.x,
                        y = position.y + 1
                    };
                case Direction.down:
                    return new Coordinate
                    {
                        x = position.x,
                        y = position.y - 1
                    };
                case Direction.right:
                    return new Coordinate
                    {
                        x = position.x + 1,
                        y = position.y
                    };
                case Direction.left:
                    return new Coordinate
                    {
                        x = position.x - 1,
                        y = position.y
                    };
                default:
                    return position;
            }
        }
    }
}
