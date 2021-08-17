using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using KunalsDiscordBot.Modules.Games.Communicators;
using DSharpPlus.Interactivity.Enums;
using System.Text.RegularExpressions;
using System.Linq;
using KunalsDiscordBot.Modules.Games.UNO.Cards;

namespace KunalsDiscordBot.Modules.Games.Players
{
    public class UNOPlayer : DiscordPlayer<UNOCommunicator>
    {
        private bool autoReady { get; set; } = false;

        public List<Card> cards { get; private set; }
        private PaginationEmojis emojis { get; set; }

        public UNOPlayer(DiscordMember _member, PaginationEmojis allEmojis) : base(_member)
        {
            emojis = allEmojis;
        }

        public void AssignCards(List<Card> _cards) => cards = _cards;

        public override Task<bool> Ready(DiscordChannel channel) => Task.FromResult(false);

        public async Task<bool> Ready(DiscordChannel channel, DiscordClient client)
        {
            communicator = new UNOCommunicator(new Regex("([1-9][,]?)+"), TimeSpan.FromMinutes(UNOGame.timeLimit), channel);
            await communicator.SendMessage("Waiting for players to get ready...");

            await communicator.SendMessage("Cards recieved").ConfigureAwait(false);
            await PrintAllCards();
            await Task.Delay(TimeSpan.FromSeconds(1));//gap

            await CheckAutoContinue(client); 

            await communicator.SendMessage("Are you ready to play the game? Enter anything to start. The game will auto start in 1 minute").ConfigureAwait(false);
            await communicator.WaitForMessage(client.GetInteractivity(), new InputData
            {
                conditions = x => x.Author.Id == member.Id && x.Channel.Id == channel.Id,
                span = TimeSpan.FromMinutes(1)
            });

            return true;
        }

        private async Task CheckAutoContinue(DiscordClient client)
        {
            string yesReturn = "yes";
            var result = await communicator.QuestionInput(client.GetInteractivity(), "Do you want me to auto-ready on your part after each turn? Type `y` for yes. Anything else will be taken as no.\n Time: 10 seconds", new InputData
            {
                extraInputValues = new Dictionary<string, (string, string)>
                {
                    { "no",  ("", "no")},
                    { "yes",  ("y", yesReturn)}
                },
                conditions = x => x.Channel.Id == communicator.channel.Id && x.Author.Id == member.Id,
                span = TimeSpan.FromSeconds(10)
            });

            if (result == yesReturn)
                autoReady = true;

            await communicator.SendMessage($"Auto-Ready set to {autoReady}");
        }

        public async Task Ready(string messsage, TimeSpan span, DiscordClient client)
        {
            if (autoReady)
            {
                await communicator.SendMessage("Autoreadied");
                return;
            }

            await communicator.SendMessage(messsage).ConfigureAwait(false);
            await communicator.WaitForMessage(client.GetInteractivity(), new InputData
            {
                conditions = x => x.Author.Id == member.Id && x.Channel.Id == communicator.channel.Id,
                span = span
            }); 
        }

        public async Task<InputResult> GetInput(DiscordClient client, Card currentCard)
        {
            if(await CheckToDraw(currentCard))
            {
                await communicator.SendMessage("None of the cards you have can be played on the current card, drawing 1");
                return new InputResult
                {
                    wasCompleted = true,
                    type = InputResult.Type.inValid
                };
            }

            var interactivity = client.GetInteractivity();
            bool comepleted = false;
            string drawCheck = "draw card";

            while(!comepleted)
            {
                var message = await communicator.Input(interactivity, "Which Card would you like to play?.\nEnter the index number of the card to play it " +
                    "and use `,` to split (without spaces) the index' if you're playing 2 or more cards. Enter `leave` to leave the game. Enter `draw` to draw a card",
                    new InputData
                    {
                        conditions = x => x.Channel.Id == communicator.channel.Id && x.Author.Id == member.Id,
                        span = TimeSpan.FromMinutes(UNOGame.timeLimit),
                        leaveMessage = "leave",
                        extraInputValues = new Dictionary<string, (string, string)>
                        {
                            {"draw", ("draw", drawCheck) }
                        },
                        regexMatchFailExpression = "Please use the input format"
                    });

                if (message.Equals(DiscordCommunicator.afkInputvalue))
                    return new InputResult
                    {
                        wasCompleted = false,
                        type = InputResult.Type.afk
                    };
                else if (message.Equals(DiscordCommunicator.quitInputvalue))
                    return new InputResult
                    {
                        wasCompleted = false,
                        type = InputResult.Type.end
                    };
                else if (message.Equals(drawCheck))
                {
                    await communicator.SendMessage("Drawing card").ConfigureAwait(false);

                    return new InputResult
                    {
                        wasCompleted = true,
                        type = InputResult.Type.inValid
                    };
                }
                else
                {
                    var indexs = GetIndexes(message);
                    if (indexs == null)
                    {
                        await communicator.SendMessage($"Thats not valid input. Check the index' you're entering. \nPS: you also can't play more than {UNOGame.maxCardsInATurn} card(s) per turn.");
                        continue;
                    }

                    if (indexs.HasDuplicates())
                    {
                        await communicator.SendMessage($"Dupliacates don't work my friend");
                        continue;
                    }
                    var inputCards = cards.GetElemantsWithIndex(indexs.ToArray()).ToList();

                    if (currentCard is IChangeColorCard)
                    {
                        var casted = (IChangeColorCard)currentCard;

                        if (casted.colorToChange != inputCards[0].cardColor && inputCards[0].cardColor != CardColor.none)
                        {
                            await communicator.SendMessage($"The current color is {casted.colorToChange}, you can't play cards of a different color").ConfigureAwait(false);
                            continue;
                        }
                    }

                    if (!CheckStacking(inputCards))
                    {
                        await communicator.SendMessage($"The cards you have chosen cannot be stacked on top in general or can't be stacked on each other in this specific order");
                        continue;
                    }

                    if (!currentCard.ValidNextCardCheck(inputCards[0]))
                    {
                        await communicator.SendMessage($"These cards cannot be played on the current card");
                        continue;
                    }

                    if (inputCards[inputCards.Count - 1] is IChangeColorCard)
                        ((IChangeColorCard)inputCards[inputCards.Count - 1]).colorToChange = await communicator.GetCardColor(interactivity, new InputData
                        {
                            extraOutputValues = new Dictionary<string, string>
                            {
                                {"time out", "Time out, I'm just gonna choose red" },
                                {"invalid", "Thats not even a valid color, I'm gonna go ahead with red" }
                            },
                            span = TimeSpan.FromSeconds(10)
                        });

                    foreach (var card in inputCards)
                        cards.Remove(card);

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
                await communicator.SendMessage("Even after drawing, none of the cards can be played. Turn skipped");
                return new InputResult
                {
                    wasCompleted = true,
                    type = InputResult.Type.inValid
                };
            }

            await communicator.SendMessage("The drawed card can be played, type `y` to play the card. Anything else and the card will be kept.\nTime - 10 seconds");
            var interactivity = client.GetInteractivity();

            string yesReturn = "yes";
            var result = await communicator.QuestionInput(client.GetInteractivity(), "The drawed card can be played, type `y` to play the card. Anything else and the card will be kept.\nTime - 10 seconds", new InputData
            {               
                extraInputValues = new Dictionary<string, (string, string)>
                {
                    { "no",  ("", "no")},
                    { "no",  ("y", yesReturn)}
                },
                conditions = x => x.Channel.Id == communicator.channel.Id && x.Author.Id == member.Id,
                span = TimeSpan.FromSeconds(10)
            });

            if (result == yesReturn)
            {
                await communicator.SendMessage("Playing card");

                return new InputResult
                {
                    wasCompleted = true,
                    type = InputResult.Type.valid,
                    cards = new List<Card> { cards[cards.Count - 1] }
                };
            }
            else
            {
                await communicator.SendMessage("Card kept");
                return new InputResult
                {
                    wasCompleted = false
                };
            }
        }

        public async Task<bool> UNOTime(DiscordClient client) => await communicator.CheckUNO(client.GetInteractivity(), new InputData
        {
            conditions = x => x.Author.Id == member.Id && x.Channel.Id == communicator.channel.Id && x.Content.ToLower().Equals("uno"),
            span = TimeSpan.FromSeconds(5)
        });

        private Task<bool> CheckToDraw(Card currenctCard)
        {
            foreach (var card in cards)
                if (currenctCard.ValidNextCardCheck(card))
                    return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task AddCards(List<Card> newCards)
        {
            int index = cards.Count;
            cards.AddRange(newCards);

            PrintCards(index, newCards.Count, "Cards Drawed");

            return Task.CompletedTask;
        }

        private List<int> GetIndexes(string input)
        {
            int[] indexs = Array.ConvertAll(input.Split(','), x => int.Parse(x) - 1);

            if (indexs.Length >= UNOGame.maxCardsInATurn)
                return null;

            foreach (var val in indexs)
                if (val < 0 || val >= cards.Count)
                    return null;

            return indexs.ToList();
        }

        private bool CheckStacking(List<Card> cards)
        {
            for (int i = 0; i < cards.Count - 1; i++)
                if (!cards[i].Stack(cards[i + 1]))
                    return false;

            return true;
        }

        public Task PrintAllCards()
        {
            _ = Task.Run(async () => await SendMessage(await communicator.GetPrintableDeck(cards, "Your Cards"), emojis));

            return Task.CompletedTask;
        }

        public Task PrintCards(int start, int number, string title)
        {
            _ = Task.Run(async () => await SendMessage(await communicator.GetPrintableDeck(cards.Skip(start).Take(start + number).ToList(), title), emojis));

            return Task.CompletedTask;
        }

        public Task PrintCards(List<Card> cardsToPrint, string title)
        {
            _ = Task.Run(async () => await SendMessage(await communicator.GetPrintableDeck(cardsToPrint, title), emojis));

            return Task.CompletedTask;
        }

        public Task SendMessage(List<Page> pages, PaginationEmojis emojis)
        {
            Task.Run(async () => await communicator.SendPageinatedMessage(member, pages, emojis, PaginationBehaviour.WrapAround, PaginationDeletion.DeleteMessage));

            return Task.CompletedTask;
        }

        public async Task SendMessage(DiscordMessageBuilder message) => await message.SendAsync(communicator.channel);

    }
}
