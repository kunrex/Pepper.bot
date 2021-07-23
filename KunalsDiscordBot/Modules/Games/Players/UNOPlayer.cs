using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using KunalsDiscordBot.Modules.Games.Complex;
using KunalsDiscordBot.Modules.Games.Complex.UNO;
using KunalsDiscordBot.Services;
using KunalsDiscordBot.Services.Images;
using System.Text.RegularExpressions;
using System.Linq;
using KunalsDiscordBot.Modules.Games.Complex.UNO.Cards;

namespace KunalsDiscordBot.Modules.Games.Players
{
    public class UNOPlayer : DiscordPlayer
    {
        public static int maxImagesPerRow = 6;
        public static int widthInPixel = 168;
        public static int heightInPixel = 259;
        public static int gapInPxiels = 10;

        public DiscordChannel dmChannel { get; private set; }

        private PaginationEmojis emojis;

        private Regex validInputRegex = new Regex("([1-9][,]?)+");
        private List<Card> cards { get; set; }

        public UNOPlayer(DiscordMember _member) : base(_member)
        {
            member = _member;
        }

        public void InitialisePlayer(List<Card> _cards, PaginationEmojis allEmojis)
        {
            cards = _cards;
            emojis = allEmojis;
        }

        public async override Task<bool> Ready(DiscordChannel channel)
        {
            dmChannel = channel;

            await dmChannel.SendMessageAsync("Cards recieved").ConfigureAwait(false);
            return true;
        }

        public async void SendPaginatedMessage(List<Page> pages, PaginationEmojis emojis)
        {
            await dmChannel.SendPaginatedMessageAsync(member, pages, emojis, DSharpPlus.Interactivity.Enums.PaginationBehaviour.WrapAround, DSharpPlus.Interactivity.Enums.PaginationDeletion.DeleteMessage, TimeSpan.FromMinutes(2));
        }

        public void PrintCards()
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

            SendPaginatedMessage(pages, emojis);
        }

        public async Task<InputResult> GetInput(DiscordClient client, Card currentCard)
        {
            if(await CheckToDraw(currentCard))
            {
                await dmChannel.SendMessageAsync("None of the cards you have can be played on the current card, drawing 1");
                return new InputResult
                {
                    wasCompleted = true,
                    type = InputResult.Type.inValid
                };
            }

            var interactivity = client.GetInteractivity();
            bool comepleted = false;

            while(!comepleted)
            {
                await dmChannel.SendMessageAsync("Which Card would you like to play?.\nEnter the index number of the card to play it " +
                    "and use `,` to split (without spaces) the index' if you're playing 2 or more cards. Enter `leave` to leave the game");

                var message = await interactivity.WaitForMessageAsync(x => x.Channel.Id == dmChannel.Id && x.Author.Id == member.Id, TimeSpan.FromMinutes(UNOGame.timeLimit));

                if (message.TimedOut)
                    return new InputResult
                    {
                        wasCompleted = false,
                        type = InputResult.Type.afk
                    };
                else if(message.Result.Content.ToLower().Equals("leave"))
                    return new InputResult
                    {
                        wasCompleted = false,
                        type = InputResult.Type.end
                    };
                else
                {
                    if(!MatchRegex(message.Result.Content))
                    {
                        await dmChannel.SendMessageAsync("Use the appropriate input format");
                        continue;
                    }

                    var indexs = GetIndexes(message.Result.Content);
                    if(indexs == null)
                    {
                        await dmChannel.SendMessageAsync($"Thats not valid input. Check the index' you're entering and you can't play more than {UNOGame.maxCardsInATurn} card(s) per turn.");
                        continue;
                    }

                    if (indexs.HasDuplicates())
                    {
                        await dmChannel.SendMessageAsync($"Dupliacates have been found in your input");
                        continue;
                    }
                    var inputCards = cards.GetElemantsWithIndex(indexs.ToArray()).ToList();

                    if (currentCard is IChangeColorCard)
                    {
                        var casted = (IChangeColorCard)currentCard;

                        if (casted.colorToChange != inputCards[0].cardColor && inputCards[0].cardColor != CardColor.none)
                        {
                            await dmChannel.SendMessageAsync($"The current color is {casted.colorToChange}, you can't play cards of a different color").ConfigureAwait(false);
                        }
                    }

                    if (!CheckStacking(inputCards))
                    {
                        await dmChannel.SendMessageAsync($"The cards you have chosen cannot be stacked on top in general or can't be stacked on each other in this specific order ");
                        continue;
                    }

                    if (!currentCard.ValidNextCardCheck(inputCards[0]))
                    {
                        await dmChannel.SendMessageAsync($"These cards cannot be played on the current card");
                        continue;
                    }

                    if (inputCards[inputCards.Count - 1] is IChangeColorCard)
                        ((IChangeColorCard)inputCards[inputCards.Count - 1]).colorToChange = await GetCardColor(interactivity);

                    foreach (var index in indexs)
                        cards.RemoveAt(index);

                    return new InputResult
                    {
                        wasCompleted = true,
                        type = InputResult.Type.valid,
                        cards = inputCards
                    };
                }
            }

            return new InputResult
            {
                wasCompleted = true
            };
        }

        public async Task<InputResult> TryPlay(Card currentCard, DiscordClient client)
        {
            if (await CheckToDraw(currentCard))
            {
                await dmChannel.SendMessageAsync("Even after drawing, none of the cards can be played. Turn skipped");
                return new InputResult
                {
                    wasCompleted = true,
                    type = InputResult.Type.inValid
                };
            }

            await dmChannel.SendMessageAsync("The drawed card can be played, type `y` to play the card. `n` to keep the card.\nTime - 10 seconds");
            var interactivity = client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel.Id == dmChannel.Id && x.Author.Id == member.Id, TimeSpan.FromSeconds(10));
            if(message.TimedOut || message.Result.Content.ToLower() == "y")
            {
                if(message.TimedOut)
                    await dmChannel.SendMessageAsync("Time out, playing card");

                return new InputResult
                {
                    wasCompleted = true,
                    type = InputResult.Type.valid,
                    cards = new List<Card> {  cards[cards.Count - 1] }
                };
            }
            else
            {
                await dmChannel.SendMessageAsync("Card kept");
                return new InputResult
                {
                    wasCompleted = false
                };
            }
        }

        private Task<bool> CheckToDraw(Card currenctCard)
        {
            foreach (var card in cards)
                if (currenctCard.ValidNextCardCheck(card))
                    return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public async Task AddCards(List<Card> newCrads)
        {
            cards.AddRange(newCrads);

            await dmChannel.SendMessageAsync($"Drawed {newCrads.Count} cards");
        }

        private async Task<CardColor> GetCardColor(InteractivityExtension interactivity)
        {
            await dmChannel.SendMessageAsync("Which color would you like to change to?");

            var message = await interactivity.WaitForMessageAsync(x => x.Channel.Id == dmChannel.Id && x.Author.Id == member.Id, TimeSpan.FromMinutes(0.5f));
            if(message.TimedOut)
            {
                await dmChannel.SendMessageAsync("Time out, choosing the color red");
                return CardColor.red;
            }
            else
            {
                var content = message.Result.Content;

                var types = Enum.GetValues(typeof(CardColor)).OfType<CardColor>().ToList();
                if(content.Length == 1)
                {
                    var color = types.FirstOrDefault(x => x.ToString()[0] == content[0]);
                    if(color == CardColor.none)//not a parsed value
                    {
                        await dmChannel.SendMessageAsync("Invalid color, choosing the color red");
                        return CardColor.red;
                    }

                    return color;
                }
                else
                {
                    var color = types.FirstOrDefault(x => x.ToString() == content.ToLower());
                    if (color == CardColor.none)//not a parsed value
                    {
                        await dmChannel.SendMessageAsync("Invalid color, choosing the color red");
                        return CardColor.red;
                    }

                    return color;
                }
            }
        }

        private bool MatchRegex(string input) => validInputRegex.IsMatch(input);

        private List<int> GetIndexes(string input)
        {
            List<int> indexs = new List<int>();
            int i = 0;
            var val = string.Empty;

            foreach (var c in input)
            {
                if (c == ',')
                {
                    if (i == UNOGame.maxCardsInATurn)
                        return null;

                    var index = int.Parse(val) - 1;
                    if (index < 0 || index >= cards.Count)
                        return null;

                    indexs.Add(int.Parse(val) - 1);
                    i++;
                    val = string.Empty;
                }
                else
                    val += c;
            }
            indexs.Add(int.Parse(val) - 1);

            return indexs;
        }

        private bool CheckStacking(List<Card> cards)
        {
            for (int i = 0; i < cards.Count - 2; i++)
                if (!cards[i].Stack(cards[i + 1]))
                    return false;

            return true;
        }
    }
}
