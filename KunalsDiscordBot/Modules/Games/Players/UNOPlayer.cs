using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using KunalsDiscordBot.Modules.Games.Complex.UNO;
using KunalsDiscordBot.Services;
using KunalsDiscordBot.Services.Images;

namespace KunalsDiscordBot.Modules.Games.Players
{
    public class UNOPlayer : DiscordPlayer
    {
        public static int maxImagesPerRow = 6;
        public static int widthInPixel = 168;
        public static int heightInPixel = 259;
        public static int gapInPxiels = 10;

        public DiscordChannel dmChannel { get; private set; }
        private Dictionary<string, DiscordEmoji> controls { get; set; }

        private List<Card> cards { get; set; }

        public UNOPlayer(DiscordMember _member) : base(_member)
        {
            member = _member;
        }

        public void InitialisePlayer(List<Card> _cards, DiscordClient client)
        {
            cards = _cards;
            controls = new Dictionary<string, DiscordEmoji>()
            {
                {"Left",  DiscordEmoji.FromName(client, ":arrow_backward:")},
                {"Right",  DiscordEmoji.FromName(client, ":arrow_forward:")},
            };
        }

        public async override Task<bool> Ready(DiscordChannel channel)
        {
            dmChannel = channel;

            await dmChannel.SendMessageAsync("Cards recieved").ConfigureAwait(false);
            return true;
        }

        public async Task PrintCards()
        {
            var pages = new List<Page>();
            int index = 1;

            foreach(var card in cards)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Your Cards",
                    ImageUrl = Card.GetLink(card.fileName).link + ".png",
                    Footer = BotService.GetEmbedFooter($"{index}/{cards.Count}. (You can view your cards using this message for 2 minutes)")
                }.AddField("Card", card.cardName);

                pages.Add(new Page(null, embed));
                index++;
            }

            var emojis = new PaginationEmojis()
            {
                Left = controls["Left"],
                Right = controls["Right"],
                Stop = null
            };

            await dmChannel.SendPaginatedMessageAsync(member, pages, emojis, DSharpPlus.Interactivity.Enums.PaginationBehaviour.WrapAround, DSharpPlus.Interactivity.Enums.PaginationDeletion.DeleteMessage, TimeSpan.FromMinutes(2));
        }
    }
}
