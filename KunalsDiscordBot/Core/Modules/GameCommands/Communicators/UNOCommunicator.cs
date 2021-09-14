using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;

using KunalsDiscordBot.Services;
using KunalsDiscordBot.Core.Modules.GameCommands.UNO.Cards;
using KunalsDiscordBot.Core.Modules.GameCommands.Communicators.Interfaces;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators
{
    public class UNOCommunicator : DiscordCommunicator, ITextInputCommunicator
    {
        public DiscordChannel channel { get; private set; }

        public UNOCommunicator(Regex expression, TimeSpan span, DiscordChannel channel) : base(expression, span)
        {
            this.channel = channel;
        }

        public async Task<string> Input(InteractivityExtension interactivity, string inputMessage, InputData data)
        {
            await SendMessage(inputMessage);

            var message = await WaitForMessage(interactivity, data.Conditions, data.Span);

            if (message.TimedOut)
                return afkInputvalue;
            else if (message.Result.Content.ToLower().Equals(data.LeaveMessage))
                return quitInputvalue;
            else if (message.Result.Content.ToLower().Equals(data.ExtraInputValues["draw"].Item1))
            {
                await SendMessage("Drawing card").ConfigureAwait(false);

                return data.ExtraInputValues["draw"].Item2;
            }
            else if(!MatchRegex(message.Result.Content))
            {
                await SendMessage(data.RegexMatchFailExpression);

                return inputFormatNotFollow;
            }

            return message.Result.Content;
        }

        public async Task<string> QuestionInput(InteractivityExtension interactivity, string question, InputData data)
        {
            await SendMessage(question).ConfigureAwait(false);
            var message = await WaitForMessage(interactivity, data.Conditions, data.Span);

            if (message.TimedOut)
                return data.ExtraInputValues["no"].Item2;
            if (message.Result.Content.ToLower().Equals(data.ExtraInputValues["yes"].Item1))
                return data.ExtraInputValues["yes"].Item2;

            return data.ExtraInputValues["no"].Item2;
        }

        public async Task<CardColor> GetCardColor(InteractivityExtension interactivity, InputData data)
        {
            var message = await WaitForMessage(interactivity, data.Conditions, data.Span);

            if(message.TimedOut)
            {
                await SendMessage(data.ExtraOutputValues["time out"]);

                return CardColor.red;
            }
            else
            {
                var content = message.Result.Content;

                if (Enum.TryParse(typeof(CardColor), content, out var x))
                    return (CardColor)Enum.Parse(typeof(CardColor), content);

                await SendMessage(data.ExtraOutputValues["invalid"]);
                return CardColor.red;
            }
        }

        public async Task<bool> CheckUNO(InteractivityExtension interactivity, InputData data)
        {
            var message = await WaitForMessage(interactivity, data.Conditions, data.Span);

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
                    Color = UNOGame.UNOColor
                }.WithFooter($"{index}/{cards.Count}. (You can view your cards using this message for {UNOGame.timeLimit} minute(s))")
                 .AddField("Card", card.cardName);

                pages.Add(new Page(null, embed));
                index++;
            }

            return Task.FromResult(pages);
        }

        public async Task<InteractivityResult<DiscordMessage>> WaitForMessage(InteractivityExtension interactivity, InputData data) => await WaitForMessage(interactivity, data.Conditions, data.Span);

        public async Task<DiscordMessage> SendEmbedToPlayer(DiscordEmbed embed) => await SendEmbedToPlayer(channel, embed);
        public async Task<DiscordMessage> SendMessage(string message) => await SendMessageToPlayer(channel, message);
        public async Task<DiscordMessage> SendMessage(string message, DiscordEmbed embed) => await SendMessageToPlayer(channel, message, embed);
        public async Task SendPageinatedMessage(DiscordUser user, List<Page> pages, PaginationEmojis emojis, PaginationBehaviour pagination, PaginationDeletion deletion)
            => await SendPageinatedMessage(channel, user, pages, emojis, pagination, deletion, timeSpan);
    }
}
