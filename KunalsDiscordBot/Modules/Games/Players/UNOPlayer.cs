using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using KunalsDiscordBot.Modules.Games.Complex.UNO;

namespace KunalsDiscordBot.Modules.Games.Players
{
    public class UNOPlayer : DiscordPlayer
    {
        public DiscordChannel dmChannel { get; private set; }

        private List<Card> Cards { get; set; }

        public UNOPlayer(DiscordMember _member, List<Card> cards) : base(_member)
        {
            member = _member;

            Cards = cards;
        }

        public async override Task<bool> Ready(DiscordChannel channel)
        {
            dmChannel = channel;

            await dmChannel.SendMessageAsync("Cards recieved").ConfigureAwait(false);
            return true;
        }

        public async Task<DiscordEmbedBuilder> GetPlayerEmbed()
        {
            return new DiscordEmbedBuilder
            {
                Title = $"{member.Username}'s Cards"
            };
        }
    }
}
