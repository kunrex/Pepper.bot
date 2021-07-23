using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using KunalsDiscordBot.Modules.Games.Players;
using KunalsDiscordBot.Modules.Games.Complex.UNO.Cards;
using KunalsDiscordBot.Modules.Games.Complex.UNO;
using KunalsDiscordBot.Services.Images;
using DSharpPlus.Interactivity;
using KunalsDiscordBot.Services;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.Enums;

namespace KunalsDiscordBot.Modules.Games.Complex
{
    internal enum GameDirection
    {
        forward,
        reverse
    }

    public class UNOGame : ComplexBoardGame<UNOPlayer>
    {
        public static readonly List<CardColor> colors = Enum.GetValues(typeof(CardColor)).Cast<CardColor>().ToList();
        public static int maxPlayers = 5, startCardNumber = 8, timeLimit = 1, maxCardsInATurn = 4;

        public DiscordClient client { get; private set; }
        private bool gameOver { get; set; }

        private List<UNOPlayer> playersWhoFinished { get; set; }
        private List<DiscordChannel> dmChannels { get; set; }
        private List<Card> cards { get; set; } = GetDeck().Shuffle().ToList();
        private Card currenctCard { get; set; }

        public Dictionary<string, DiscordEmoji> controls { get; private set; }
        public PaginationEmojis emojis { get; private set; }

        private StackData? currentStackData { get; set; } = null;
        private GameDirection direction = GameDirection.forward;

        public static List<Card> GetDeck()
        {
            var cards = new List<Card>();
            //4 wild cards
            for (int i = 0; i < 4; i++)
                cards.Add(new WildCard(CardColor.none, CardType.Wild));

            //4 wild cards
            for (int i = 0; i < 4; i++)
                cards.Add(new Plus4Card(CardColor.none, CardType.plus4));

            foreach (var color in colors.Where(x => x != CardColor.none).ToList())
            {
                //add number cards
                cards.Add(new NumberCard(color, 0));
                for (int i = 1; i <= 2; i++)
                    for (int k = 1;k <= 9; k++)
                        cards.Add(new NumberCard(color,  i));

                //add reverse, skip and +2
                for (int k = 1; k <= 2; k++)
                {
                    cards.Add(new SkipCard(color));
                    cards.Add(new ReverseCard(color));
                    cards.Add(new Plus2Card(color));
                }
            }

            return cards;
        }

        public UNOGame(List<DiscordMember> members, DiscordClient _client)
        {
            var _players = new List<UNOPlayer>();
            foreach (var member in members)
                _players.Add(new UNOPlayer(member));

            client = _client;
            players = _players;

            currentPlayer = players[0];
            gameOver = false;

            controls = new Dictionary<string, DiscordEmoji>()
            {
                {"Left",  DiscordEmoji.FromName(client, ":arrow_backward:")},
                {"Right",  DiscordEmoji.FromName(client, ":arrow_forward:")},
            };
            emojis = new PaginationEmojis()
            {
                Left = controls["Left"],
                Right = controls["Right"],
                Stop = null
            };

            SetUp();
        }

        protected async override void PlayGame()
        {
            foreach(var player in players.Where(x => x.member.Id != currentPlayer.member.Id).ToList())
                player.PrintCards();

            while (!gameOver)
            {
                await SendMessageToAllPlayers(null, new DiscordEmbedBuilder
                {
                    Title = $"{currentPlayer.member.Username}'s Turn",
                    ImageUrl = Card.GetLink(currenctCard.fileName).link + ".png"
                }.AddField("Current Card:", "** **"));

                currentPlayer.PrintCards();
                await Task.Delay(TimeSpan.FromSeconds(1));

                var result = await currentPlayer.GetInput(client, currenctCard);

                if (!result.wasCompleted)
                {
                    await SendMessageToAllPlayers($"{currentPlayer.member.Username} {(result.type == InputResult.Type.end ? "has left the game" : "has gone afk")}").ConfigureAwait(false);
                    players.Remove(currentPlayer);

                    if (players.Count == 1)
                    {
                        await SendMessageToAllPlayers("There are now too less players left to play the game").ConfigureAwait(false);
                        break;
                    }
                }
                else if (result.type == InputResult.Type.inValid)
                {
                    await AddcardsToPlayer(1);

                    var checkResult = await currentPlayer.TryPlay(currenctCard, client);
                    if (checkResult.wasCompleted)
                        result = checkResult;
                }

                cards.AddRange(result.cards.Shuffle());//cycle cards
                await Task.Run(async() => await SendPaginatedMessageToAllPlayers(GetPages($"{currentPlayer.member.Username}' plays:", result.cards.Select(x => Card.GetLink(x.fileName).link + ".png").ToList()), emojis));
                await CheckAfterTurnStacks(result.cards);
            }
        }

        private async Task CheckAfterTurnStacks(List<Card> cardsPlayed)
        {
            if (cardsPlayed.Count == 0 || cards == null)
                return;

            if (cardsPlayed[0].cardType.AsStackType() == StackType.none && currentStackData == null)
                return;

            if (currentStackData != null)
            {
                if (cardsPlayed[0].cardType.AsStackType() == currentStackData.Value.stackType)
                {
                    currentStackData = new StackData
                    {
                        stackType = currentStackData.Value.stackType,
                        stack = currentStackData.Value.stack + cardsPlayed.Count
                    };

                    await NextPlayer();
                }
                else
                {
                    switch (currentStackData.Value.stackType)
                    {
                        case StackType.cards:
                            await AddcardsToPlayer(currentStackData.Value.stack);
                            await NextPlayer();
                            break;
                    }
                }
            }
            else
            {
                switch (cardsPlayed[0].cardType)
                {
                    case CardType.Reverse:
                        if (cardsPlayed.Count % 2 == 1)
                            direction = direction == GameDirection.forward ? GameDirection.reverse : GameDirection.forward;

                        await NextPlayer();
                        return;
                    case CardType.Skip:
                        currentStackData = new StackData
                        {
                            stackType = StackType.skip,
                            stack = cardsPlayed.Count
                        };

                        await NextPlayer();
                        return;
                    case CardType.plus2:
                        int val = 0;
                        foreach (var card in cardsPlayed)
                            val += card is Plus2Card ? 2 : 4;

                        currentStackData = new StackData
                        {
                            stackType = StackType.cards,
                            stack = val
                        };
                        int index = (players.IndexOf(currentPlayer) + (direction == GameDirection.forward ? currentStackData.Value.stack : -currentStackData.Value.stack)) % players.Count;
                        currentPlayer = players[index];
                        return;
                    case CardType.plus4:
                        currentStackData = new StackData
                        {
                            stackType = StackType.cards,
                            stack = cardsPlayed.Count * 4
                        };

                        await NextPlayer();
                        return;
                }
            }           
        }

        private async Task AddcardsToPlayer(int num)
        {
            var cardsToAdd = cards.Take(num).ToList();
            cards.RemoveRange(0, num);

            await currentPlayer.AddCards(cardsToAdd);
        }

        private Task NextPlayer()
        {
            int index = players.IndexOf(currentPlayer);

            index += direction == GameDirection.forward ? 1 : -1;
            if (direction == GameDirection.forward && index == players.Count)
                index = 0;
            else
                if (index == -1)
                    index = players.Count - 1;

            currentPlayer = players[index];
            return Task.CompletedTask;
        }

        protected async override Task PrintBoard()
        {
           
        }

        protected async override void SetUp()
        {
            Console.WriteLine("here");

            dmChannels = new List<DiscordChannel>();
            foreach (var player in players)
            {
                dmChannels.Add(await player.member.CreateDmChannelAsync());
                player.InitialisePlayer(cards.Take(startCardNumber).ToList(), emojis);

                cards.RemoveRange(0, startCardNumber);
            }

            Console.WriteLine("here");
            List <Task<bool>> awaitReady = new List<Task<bool>>();
            for (int i = 0; i < players.Count; i++)
                awaitReady.Add(players[i].Ready(dmChannels[i]));

            Console.WriteLine("here");
            var task = Task.WhenAll(awaitReady);
            await task;
            currenctCard = cards[0];

            Console.WriteLine("here");
            await SendMessageToAllPlayers(null, new DiscordEmbedBuilder
            {
                Title = "UNO!"
            }.AddField("Host:", "Me", true)
             .AddField("Starting Cards Number:", startCardNumber.ToString(), true)
             .AddField("Players: ", string.Join(',', players)));
            await SendMessageToAllPlayers("Starting Game!").ConfigureAwait(false);

            PlayGame();
        }

        private async Task SendMessageToAllPlayers(string message = null, DiscordEmbedBuilder embed = null)
        {
            var messageBuild = new DiscordMessageBuilder();

            if (message != null)
                messageBuild.WithContent(message);
            if (embed != null)
                messageBuild.WithEmbed(embed);

            foreach (var channel in dmChannels)
                await messageBuild.SendAsync(channel);
        }

        private Task SendPaginatedMessageToAllPlayers(List<Page> pages, PaginationEmojis emojis)
        {
            foreach (var player in players)
                player.SendPaginatedMessage(pages, emojis);

            return Task.CompletedTask;
        }

        private async Task SendMessageToSpecificPlayer(int index, string message = null, DiscordEmbedBuilder embed = null)
        {
            var channel = dmChannels[index];
            var messageBuild = new DiscordMessageBuilder();

            if (message != null)
                messageBuild.WithContent(message);
            if (embed != null)
                messageBuild.WithEmbed(embed);

            await messageBuild.SendAsync(channel);
        }

        private List<Page> GetPages(string title, List<string> urls)
        {
            List<Page> pages = new List<Page>();
            int i = 1;

            foreach (var url in urls)
                pages.Add(new Page
                {
                    Embed = new DiscordEmbedBuilder
                    {
                        Title = title,
                        ImageUrl = url,
                        Footer = BotService.GetEmbedFooter($"Card {i++}/{urls.Count}")
                    }
                }); 

            return pages;
        }
    }
}
