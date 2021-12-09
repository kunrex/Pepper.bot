using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Players
{
    public class RockPaperScissorsPlayer : DiscordPlayer<RockPaperScissorsCommunicator>
    {
        public RockPaperScissorsPlayer(DiscordMember _member) : base(_member)
        {

        }

        public override Task<bool> Ready(DiscordChannel channel)
        {
            communicator = new RockPaperScissorsCommunicator(channel);
            return Task.FromResult(true);
        }

        public async Task<InputResult> Input(DiscordClient client, DiscordMessage message)
        {
            var result = await communicator.Input(client.GetInteractivity(), message, member, new InputData
            {
                Span = TimeSpan.FromSeconds(10),
                InputType = InputType.Button
            });

            if (result == DiscordCommunicator.afkInputvalue)
                return new InputResult
                {
                    WasCompleted = false,
                    Type = InputResult.ResultType.Afk
                };
            else
                return new InputResult
                {
                    WasCompleted = true,
                    Type = InputResult.ResultType.Valid,
                    Ordinate = new Coordinate { x = int.Parse(result) }
                };
        }
    }
}
