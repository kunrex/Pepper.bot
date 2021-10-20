using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Modules.GameCommands.UNO.Cards;
using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Players
{
    public class UNOPlayer : DiscordPlayer<UNOCommunicator>
    {
        private bool autoReady { get; set; } = false;

        public List<Card> cards { get; private set; }

        public UNOPlayer(DiscordMember _member) : base(_member)
        {
            
        }

        public void AssignCards(List<Card> _cards) => cards = _cards;

        public override Task<bool> Ready(DiscordChannel channel) => Task.FromResult(false);

        public async Task<bool> Ready(DiscordChannel channel, DiscordClient client)
        {
            communicator = new UNOCommunicator(channel, TimeSpan.FromMinutes(UNOGame.inputTimeLimit));
            await communicator.SendMessage("Waiting for players to get ready...");

            await communicator.SendMessage("Cards recieved").ConfigureAwait(false);
            await PrintAllCards();
            await Task.Delay(TimeSpan.FromSeconds(1));//gap

            await CheckAutoContinue(client); 
            await communicator.QuestionInput(client.GetInteractivity(), "Are you ready to play the game? The game will auto start in 1 minute", member, new InputData
            {
                ExtraInputValues = new Dictionary<string, (string, string)>()
                {
                    {"yes", ("yes", "yes") }
                },
                Span = TimeSpan.FromMinutes(1),
            });

            return true;
        }

        private async Task CheckAutoContinue(DiscordClient client)
        {
            string yesReturn = "yes";
            var result = await communicator.QuestionInput(client.GetInteractivity(), "Do you want me to auto-ready on your part after each turn? Type `y` for yes. Anything else will be taken as no.\n Time: 10 seconds", member, new InputData
            {
                ExtraInputValues = new Dictionary<string, (string, string)>
                {
                    { "no",  ("n", "no")},
                    { "yes",  ("y", yesReturn)}
                },
                Span = TimeSpan.FromSeconds(10)
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

            await communicator.QuestionInput(client.GetInteractivity(), "Are you ready to proceed to the next round? I will auto-continue after 1 minute", member, new InputData
            {
                ExtraInputValues = new Dictionary<string, (string, string)>
                {
                    { "yes",  ("y", "yes")}
                },
                Span = TimeSpan.FromMinutes(1)
            });
        }

        public async Task<InputResult> GetInput(DiscordClient client, Card currentCard)
        {
            if(await CheckToDraw(currentCard))
            {
                await communicator.SendMessage("None of the cards you have can be played on the current card, drawing 1");
                return new InputResult
                {
                    WasCompleted = true,
                    Type = InputResult.ResultType.InValid
                };
            }

            var interactivity = client.GetInteractivity();
            bool comepleted = false;
            string drawCheck = "draw card";

            while(!comepleted)
            {
                var message = await communicator.SendMessage("Which Card(s) would you like to play?.\n Use the select component to choose the index' of the cards" +
                    ", you can play a maximum of 4 cards per turn. Choose `leave` to leave the game. Choose `draw` to draw a card");
                var options = new Dictionary<string, (string, string)>();
                for (int i = 0; i < cards.Count; i++)
                {
                    var value = $"{i + 1}";
                    options.Add(value, (value, value));
                }
                options.Add("draw", ("draw", drawCheck));

                var result = await communicator.Input(interactivity, message, member,
                    new InputData
                    {
                        Span = TimeSpan.FromMinutes(UNOGame.inputTimeLimit),
                        LeaveMessage = "leave",
                        ExtraInputValues = options,
                        InputType = InputType.Dropdown
                    });

                if (result.Equals(DiscordCommunicator.afkInputvalue))
                    return new InputResult
                    {
                        WasCompleted = false,
                        Type = InputResult.ResultType.Afk
                    };
                else if (result.Equals(DiscordCommunicator.quitInputvalue))
                    return new InputResult
                    {
                        WasCompleted = false,
                        Type = InputResult.ResultType.End
                    };
                else if (result.Equals(drawCheck))
                    return new InputResult
                    {
                        WasCompleted = true,
                        Type = InputResult.ResultType.InValid
                    };
                else
                {
                    var indexes = GetIndexes(result);
                    var inputCards = cards.GetElemantsWithIndex(indexes.ToArray()).ToList();

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
                        ((IChangeColorCard)inputCards[inputCards.Count - 1]).colorToChange = await communicator.GetCardColor(interactivity, "What color would you like to change to?", member, new InputData
                        {
                            ExtraOutputValues = new Dictionary<string, string>
                            {
                                {"time out", "Time out, I'm just gonna choose red" },
                            },
                            Span = TimeSpan.FromSeconds(10)
                        });

                    foreach (var card in inputCards)
                        cards.Remove(card);

                    return new InputResult
                    {
                        WasCompleted = true,
                        Type = InputResult.ResultType.Valid,
                        Cards = inputCards
                    };
                }
            }

            return new InputResult
            {
                WasCompleted = true
            };
        }

        public async Task<InputResult> TryPlay(Card currentCard, DiscordClient client)
        {
            if (await CheckToDraw(currentCard))
            {
                await communicator.SendMessage("Even after drawing, none of the cards can be played. Turn skipped");
                return new InputResult
                {
                    WasCompleted = true,
                    Type = InputResult.ResultType.InValid
                };
            }

            string yesReturn = "yes";
            var result = await communicator.QuestionInput(client.GetInteractivity(), "The drawed card can be played, Do you want to play it?.\nTime - 10 seconds", member, new InputData
            {               
                ExtraInputValues = new Dictionary<string, (string, string)>
                {
                    { "no",  ("no", "no")},
                    { "yes",  ("y", yesReturn)}
                },
                Span = TimeSpan.FromSeconds(10)
            });

            if (result == yesReturn)
            {
                await communicator.SendMessage("Playing card");
                cards.RemoveAt(cards.Count - 1);

                return new InputResult
                {
                    WasCompleted = true,
                    Type = InputResult.ResultType.Valid,
                    Cards = new List<Card> { cards[cards.Count - 1] }
                };
            }
            else
            {
                await communicator.SendMessage("Card kept");
                return new InputResult
                {
                    WasCompleted = false
                };
            }
        }

        public async Task<bool> UNOTime(DiscordClient client) => await communicator.CheckUNO(client.GetInteractivity(), new InputData
        {
            Conditions = x => x.Author.Id == member.Id && x.Channel.Id == communicator.DMChannel.Id && x.Content.ToLower().Equals("uno"),
            Span = TimeSpan.FromSeconds(5)
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
            _ = Task.Run(async () => await SendMessage(await communicator.GetPrintableDeck(cards, "Your Cards")));

            return Task.CompletedTask;
        }

        public Task PrintCards(int start, int number, string title)
        {
            _ = Task.Run(async () => await SendMessage(await communicator.GetPrintableDeck(cards.Skip(start).Take(start + number).ToList(), title)));

            return Task.CompletedTask;
        }

        public Task PrintCards(List<Card> cardsToPrint, string title)
        {
            _ = Task.Run(async () => await SendMessage(await communicator.GetPrintableDeck(cardsToPrint, title)));

            return Task.CompletedTask;
        }

        public Task SendMessage(List<Page> pages)
        {
            Task.Run(async () => await communicator.SendPageinatedMessage(member, pages, default, PaginationBehaviour.WrapAround, ButtonPaginationBehavior.Disable));

            return Task.CompletedTask;
        }

        public async Task SendMessage(DiscordMessageBuilder message) => await message.SendAsync(communicator.DMChannel);
    }
}
