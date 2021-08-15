using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using KunalsDiscordBot.Modules.Games.UNO;
using KunalsDiscordBot.Modules.Games.UNO.Cards;
using KunalsDiscordBot.Services;

namespace KunalsDiscordBot.Modules.Games.Communicators
{
    public class UNOCommunicator : DiscordCommunicator
    {
        public DiscordChannel channel { get; private set; }

        public UNOCommunicator(Regex expression, TimeSpan span, DiscordChannel channel) : base(expression, span)
        {
            this.channel = channel;
        }

        public async override Task<string> Input(InteractivityExtension interactivity, string inputMessage, InputData data)
        {
            await SendMessage(inputMessage);

            var message = await WaitForMessage(interactivity, data.conditions, data.span);

            if (message.TimedOut)
                return afkInputvalue;
            else if (message.Result.Content.ToLower().Equals(data.leaveMessage))
                return quitInputvalue;
            else if (message.Result.Content.ToLower().Equals(data.extraInputValues["draw"].Item1))
            {
                await SendMessage("Drawing card").ConfigureAwait(false);

                return data.extraInputValues["draw"].Item2;
            }
            else if(!MatchRegex(message.Result.Content))
            {
                await SendMessage(data.regexMatchFailExpression);

                return inputFormatNotFollow;
            }

            return message.Result.Content;
        }

        public async Task<string> QuestionInput(InteractivityExtension interactivity, string question, InputData data)
        {
            await SendMessage(question).ConfigureAwait(false);
            var message = await WaitForMessage(interactivity, data.conditions, data.span);

            if (message.TimedOut)
                return data.extraInputValues["no"].Item2;
            if (message.Result.Content.ToLower().Equals(data.extraInputValues["yes"].Item1))
                return data.extraInputValues["yes"].Item2;

            return data.extraInputValues["no"].Item2;
        }

        public async Task<CardColor> GetCardColor(InteractivityExtension interactivity, InputData data)
        {
            var message = await WaitForMessage(interactivity, data.conditions, data.span);

            if(message.TimedOut)
            {
                await SendMessage(data.extraOutputValues["time out"]);

                return CardColor.red;
            }
            else
            {
                var content = message.Result.Content;

                if (Enum.TryParse(typeof(CardColor), content, out var x))
                    return (CardColor)Enum.Parse(typeof(CardColor), content);

                await SendMessage(data.extraOutputValues["invalid"]);
                return CardColor.red;
            }
        }

        public async Task<bool> CheckUNO(InteractivityExtension interactivity, InputData data)
        {
            var message = await WaitForMessage(interactivity, data.conditions, data.span);

            return !message.TimedOut;
        }

        public Task<List<Page>> GetPrintableDeck(List<Card> cards, string title)
        {
            var pages = new List<Page>();
            int index = 1;

            foreach (var card in cards)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = title,
                    ImageUrl = Card.GetLink(card.fileName).link + ".png",
                    Footer = BotService.GetEmbedFooter($"{index}/{cards.Count}. (You can view your cards using this message for {UNOGame.timeLimit} minute(s))"),
                    Color = UNOGame.UNOColor
                }.AddField("Card", card.cardName);

                pages.Add(new Page(null, embed));
                index++;
            }

            return Task.FromResult(pages);
        }

        public async Task<InteractivityResult<DiscordMessage>> WaitForMessage(InteractivityExtension interactivity, InputData data) => await WaitForMessage(interactivity, data.conditions, data.span);

        public async Task<DiscordMessage> SendEmbedToPlayer(DiscordEmbed embed) => await SendEmbedToPlayer(channel, embed);
        public async Task<DiscordMessage> SendMessage(string message) => await SendMessageToPlayer(channel, message);
        public async Task<DiscordMessage> SendMessage(string message, DiscordEmbed embed) => await SendMessageToPlayer(channel, message, embed);
        public async Task SendPageinatedMessage(DiscordUser user, List<Page> pages, PaginationEmojis emojis, PaginationBehaviour pagination, PaginationDeletion deletion)
            => await SendPageinatedMessage(channel, user, pages, emojis, pagination, deletion, timeSpan);
    }
}
