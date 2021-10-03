using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Players
{
    public class PenaltyPlayer : DiscordPlayer<PenaltyCommunicator>
    {
        public PenaltyPlayer(DiscordMember _member) : base(_member)
        {
          
        }

        public override Task<bool> Ready(DiscordChannel channel)
        {
            communicator = new PenaltyCommunicator(channel);
            return Task.FromResult(true);
        }

        public async Task<InputResult> Input(DiscordClient client)
        {
            var result = await communicator.Input(client.GetInteractivity(), string.Empty, new InputData
            {
                Span = Penalty.totalTime,
                Conditions = x => x.Channel.Id == communicator.Channel.Id && x.Author.Id == member.Id
            });

            if (result == DiscordCommunicator.afkInputvalue)
                return new InputResult
                {
                    WasCompleted = false,
                    Type = InputResult.ResultType.Afk
                };
            else if (result == DiscordCommunicator.inputFormatNotFollow)
                return new InputResult
                {
                    WasCompleted = false,
                    Type = InputResult.ResultType.InValid
                };
            else
                return new InputResult
                {
                    WasCompleted = true,
                    Type = InputResult.ResultType.Valid,
                    Ordinate = new Coordinate { x = int.Parse(result) - 1 }
                };
        }

        public async Task<DiscordMessage> SendMessage(DiscordMessageBuilder message) => await communicator.SendMessage(message);
        public async Task<DiscordMessage> EditMessage(DiscordMessage message, DiscordMessageBuilder newMessage) => await communicator.EditMessage(message, newMessage);
    }
}
