using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using KunalsDiscordBot.Modules.Games.Complex;

namespace KunalsDiscordBot.Modules.Games.Players.Spectators
{
    public class DiscordSpectator : DiscordPlayer
    {
        public DiscordSpectator(DiscordMember _member, DiscordClient _client, BattleShip game) : base(_member)
        {
            member = _member;
            client = _client;
            battleShipGame = game;
        }
        private DiscordDmChannel dmChannel { get; set; }
        private DiscordClient client { get; set; }
        private BattleShip battleShipGame { get; set; }

        private bool isEnded = false;

        public void End() => isEnded = true;

        public override async Task<bool> Ready(DiscordDmChannel channel)
        {
            dmChannel = channel;
            await SendMessage("Starting to Spectate..., type `leave` to stop spectating");

            WaitForLeaveMessage();

            return true;
        }

        public async Task SendMessage(DiscordEmbedBuilder embed) => await dmChannel.SendMessageAsync(embed).ConfigureAwait(false);
        public async Task SendMessage(string message) => await dmChannel.SendMessageAsync(message).ConfigureAwait(false);

        private async void WaitForLeaveMessage()
        {
            var interactivity = client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == dmChannel && x.Content.ToLower() == "leave" && x.Author.Id == member.Id, TimeSpan.FromHours(1));

            if (isEnded)
                return;

            await dmChannel.SendMessageAsync("Leaving specator");
            await battleShipGame.RemoveSpectator(this);
        }
    }
}
