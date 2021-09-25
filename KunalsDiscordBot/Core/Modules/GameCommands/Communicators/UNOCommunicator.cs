using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Modules.GameCommands.UNO.Cards;
using KunalsDiscordBot.Core.Modules.GameCommands.Communicators.Interfaces;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators
{
    public class UNOCommunicator : DiscordCommunicator, IComponentInputCommunicator
    {
        public DiscordChannel DMChannel { get; private set; }

        private TimeSpan CardsDisplaySpan { get; }

        public UNOCommunicator(DiscordChannel _DMChannel, TimeSpan _cardsDisplaySpan) 
        {
            DMChannel = _DMChannel;

            CardsDisplaySpan = _cardsDisplaySpan;
        }

        public async Task<string> Input(InteractivityExtension interactivity, DiscordMessage message, DiscordUser user, InputData inputData)
        {
            if (inputData.InputType != InputType.Dropdown)
                throw new InvalidOperationException();

            var builder = new DiscordMessageBuilder().WithContent(message.Content).AddEmbeds(message.Embeds);

            var options = new List<DiscordSelectComponentOption>();
            foreach(var value in inputData.ExtraInputValues)
                options.Add(new DiscordSelectComponentOption(value.Key, value.Value.Item2));
            options.Add(new DiscordSelectComponentOption("Leave", inputData.LeaveMessage));

            builder.AddComponents(new DiscordSelectComponent("Input", "Choose a maximum of 4 cards to play", options, false, 1, 4));
            message = await message.ModifyAsync(builder);

            var result = await WaitForSelection(interactivity, message, user, "Input", inputData.Span);
            await message.ClearComponents();

            if(result.TimedOut)
                return afkInputvalue;
            else if (result.Result.Values.FirstOrDefault(x => x == inputData.LeaveMessage) != null)
                return quitInputvalue;
            else if (result.Result.Values.FirstOrDefault(x => x == inputData.ExtraInputValues["draw"].Item2) != null)
            {
                await SendMessage("Drawing card").ConfigureAwait(false);

                return inputData.ExtraInputValues["draw"].Item2;
            }

            return string.Join(',', result.Result.Values);
        }

        public async Task<string> QuestionInput(InteractivityExtension interactivity, string question, DiscordUser user, InputData data)
        {
            var builder = new DiscordMessageBuilder().WithContent(question).AddComponents(data.ExtraInputValues.Select(x =>
            new DiscordButtonComponent(ButtonStyle.Primary, x.Value.Item1, x.Key)));

            var message = await builder.SendAsync(DMChannel);
            var result = await WaitForButton(interactivity, message, user, data.Span);

            await message.ClearComponents();
            return result.TimedOut ? data.ExtraInputValues["no"].Item1 : data.ExtraInputValues.First(x => x.Value.Item1 == result.Result.Id).Value.Item2;
        }

        public async Task<CardColor> GetCardColor(InteractivityExtension interactivity, string messageToUser, DiscordUser user, InputData data)
        {
            var values = ((CardColor[])Enum.GetValues(typeof(CardColor))).ToList();
            values.Remove(CardColor.none);

            var builder = new DiscordMessageBuilder().WithContent(messageToUser).AddComponents(new DiscordSelectComponent("Color", "Choose a color", 
                values.Select(x => new DiscordSelectComponentOption(x.ToString(), x.ToString()))));

            var message = await builder.SendAsync(DMChannel);
            var result = await WaitForSelection(interactivity, message, user, "Color", data.Span);
            await message.ClearComponents();

            if (result.TimedOut)
            {
                await SendMessage(data.ExtraOutputValues["time out"]);

                return CardColor.red;
            }

            return Enum.Parse<CardColor>(result.Result.Values[0]);
        }

        public async Task<bool> CheckUNO(InteractivityExtension interactivity, InputData inputData)
        {
            await SendMessage("Umm you forgetting something?");
            var message = await WaitForMessage(interactivity, inputData.Conditions, inputData.Span);

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
                }.WithFooter($"{index}/{cards.Count}. (You can view the following cards using this message for {UNOGame.timeLimit} minute(s))")
                 .AddField("Card", card.cardName);

                pages.Add(new Page(null, embed));
                index++;
            }

            return Task.FromResult(pages);
        }

        public async Task<InteractivityResult<DiscordMessage>> WaitForMessage(InteractivityExtension interactivity, InputData inputData) => await WaitForMessage(interactivity, inputData.Conditions, inputData.Span);

        public async Task<DiscordMessage> SendEmbedToPlayer(DiscordEmbed embed) => await SendEmbedToPlayer(DMChannel, embed);
        public async Task<DiscordMessage> SendMessage(string message) => await SendMessageToPlayer(DMChannel, message);
        public async Task<DiscordMessage> SendMessage(string message, DiscordEmbed embed) => await SendMessageToPlayer(DMChannel, message, embed);
        public async Task SendPageinatedMessage(DiscordUser user, List<Page> pages, PaginationButtons buttons, PaginationBehaviour pagination, ButtonPaginationBehavior deletion)
            => await SendPageinatedMessage(DMChannel, user, pages, buttons, pagination, deletion, CardsDisplaySpan);
    }
}
