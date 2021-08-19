using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using KunalsDiscordBot.Services;
using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Modules.GameCommands.Players;
using KunalsDiscordBot.Core.Modules.GameCommands.UNO.Cards;
using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;
using KunalsDiscordBot.Core.Modules.GameCommands.Players.Spectators;

namespace KunalsDiscordBot.Core.Modules.GameCommands
{
    internal enum GameDirection
    {
        forward,
        reverse
    }

    public class UNOGame : DiscordGame<UNOPlayer, UNOCommunicator>, ISpectatorGame
    {
        public static int maxPlayers = 5, startCardNumber = 8, timeLimit = 1, maxCardsInATurn = 4, unoMissPenalty = 4;
        public static DiscordColor UNOColor = DiscordColor.Red;
        public static readonly string playing = "Playing...";

        private List<UNOPlayer> playersWhoFinished { get; set; } = new List<UNOPlayer>();

        private List<Card> cardsInDeck { get; set; } = GetDeck().Shuffle().ToList();
        private Card currentCard { get; set; }

        public PaginationEmojis emojis { get; private set; }

        private int? cardStacks { get; set; } = null;
        public List<DiscordSpectator> spectators { get; set; } = new List<DiscordSpectator>();

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

            var colors = Enum.GetValues(typeof(CardColor)).Cast<CardColor>().ToList();
            foreach (var color in colors.Where(x => x != CardColor.none).ToList())
            {
                //add number cards
                cards.Add(new NumberCard(color, 0));
                for (int i = 1; i <= 2; i++)
                    for (int k = 1;k <= 9; k++)
                        cards.Add(new NumberCard(color,  k));

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

        public UNOGame(DiscordClient _client, List<DiscordMember> _players) : base(_client, _players)
        {
            client = _client;
            emojis = new PaginationEmojis()
            {
                Left = DiscordEmoji.FromName(client, ":arrow_backward:"),
                Right = DiscordEmoji.FromName(client, ":arrow_forward:"),
                Stop = null
            };

            players = _players.Select(x => new UNOPlayer(x, emojis)).ToList();
            currentPlayer = players[0];

            SetUp();
        }

        protected async override void SetUp()
        {
            await DistributeCards();

            await SendMessageToAllPlayers("All players ready, Starting Game!", new DiscordEmbedBuilder
            {
                Title = "UNO!",
                Color = UNOColor
            }.AddField("Host:", "Me", true)
             .AddField("Number of cards given:", startCardNumber.ToString(), true)
             .AddField("Players: ", string.Join(", ", players.Select(x => x.member.Username.Insert(0, "`").Insert(x.member.Username.Length + 1, "`")))));

            PlayGame();
        }

        private async Task DistributeCards()
        {
            foreach (var player in players)
            {
                player.AssignCards(cardsInDeck.Take(startCardNumber).ToList());
                cardsInDeck.RemoveRange(0, startCardNumber);
            }

            List<Task<bool>> awaitReady = new List<Task<bool>>();
            foreach(var player in players)
                awaitReady.Add(player.Ready(await player.member.CreateDmChannelAsync(), client));
            await Task.WhenAll(awaitReady);

            currentCard = cardsInDeck.First(x => x is NumberCard);
            cardsInDeck.Remove(currentCard);
        }

        protected async override void PlayGame()
        {
            while (!GameOver)
            {
                await PrintBoard();
                var result = await currentPlayer.GetInput(client, currentCard);

                if (!result.wasCompleted)
                {
                    await SendMessageToAllPlayers($"{currentPlayer.member.Username} {(result.type == InputResult.Type.end ? "has left the game" : "has gone afk")}").ConfigureAwait(false);
                    var end = await RemovePlayer();

                    if (end)
                    {
                        await SendEndMessage("Anyway..").ConfigureAwait(false);

                        GameOver = true;
                        continue;
                    }
                }
                else if (result.type == InputResult.Type.inValid)
                {
                    await AddcardsToPlayer(1);

                    var checkResult = await currentPlayer.TryPlay(currentCard, client);
                    if (checkResult.wasCompleted)
                        result = checkResult;
                }

                if (result.cards != null && result.cards.Count > 0)
                {
                    await Task.Run(async () => await SendDeckToAllPlayers(result.cards, $"{currentPlayer.member.Username} Plays"));
                    await ProcessTurn(result.cards);
                    cardsInDeck.AddRange(result.cards.Shuffle());//cycle cards
                }
                else
                    await NextPlayer(currentCard);

                if (players.Count == 1)//only one player left
                {
                    await SendMessageToAllPlayers("Theres only 1 player left now").ConfigureAwait(false);
                    await SendEndMessage("Game Over!", true).ConfigureAwait(false);

                    GameOver = true;
                    continue;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));

                List<Task> tasks = players.Select(x => Task.Run(() => x.Ready("Are you ready to proceed to the next round? I will auto-ready after 1 minute", TimeSpan.FromMinutes(timeLimit), client))).ToList();
                await Task.WhenAll(tasks);

                await SendMessageToAllPlayers("All players ready");
            }

            if (OnGameOver != null)
                OnGameOver();
        }

        protected async override Task PrintBoard()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"{currentPlayer.member.Username}'s Turn",
                ImageUrl = Card.GetLink(currentCard.fileName).link + ".png",
                Color = UNOColor
            };

            embed.AddField("Direction:", direction.ToString());
            if (currentCard is IChangeColorCard)
                embed.AddField("Current Color:", ((IChangeColorCard)currentCard).colorToChange.ToString());
            else if (cardStacks != null)
                embed.AddField("Number of cards that will be drawed:", cardStacks.Value.ToString());

            embed.AddField("Current Card:", "** **");
            await SendMessageToAllPlayers(null, embed);

            await currentPlayer.PrintAllCards();
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        private async Task ProcessTurn(List<Card> cardsPlayed)
        {
            if (cardStacks == null)
            {
                switch (cardsPlayed[0].cardType)
                {
                    case CardType.Skip:
                        int newIndex = players.IndexOf(currentPlayer);
                        newIndex += direction == GameDirection.forward ? cardsPlayed.Count : -cardsPlayed.Count;
                        newIndex = (newIndex + players.Count) % players.Count;

                        currentPlayer = players[newIndex];
                        currentCard = cardsPlayed[cardsPlayed.Count - 1];

                        await SendMessageToAllPlayers($"{cardsPlayed.Count} player(s) skipped lol");
                        break;
                    case CardType.Reverse:
                        direction = cardsPlayed.Count % 2 == 0 ? direction : (direction == GameDirection.forward ? GameDirection.reverse : GameDirection.forward);
                        await NextPlayer(cardsPlayed[cardsPlayed.Count - 1]);
                        break;
                    case var x when x == CardType.plus2 || x == CardType.plus4:
                        int cardCount = 0;
                        foreach (var card in cardsPlayed)
                            if (card is Plus2Card)
                                cardCount += 2;
                            else
                                cardCount += 4;
                        cardStacks = cardCount;
                        await NextPlayer(cardsPlayed[cardsPlayed.Count - 1]);
                        break;
                    case var x when x == CardType.number || x== CardType.Wild:
                        await NextPlayer(cardsPlayed[cardsPlayed.Count - 1]);
                        break;
                }
            }
            else if (cardsPlayed[0].cardType == CardType.plus2 || cardsPlayed[0].cardType == CardType.plus4)//adding
            {
                int cardCount = 0;
                foreach (var card in cardsPlayed)
                    if (card is Plus2Card)
                        cardCount += 2;
                    else
                        cardCount += 4;
                cardStacks += cardCount;

                await NextPlayer(cardsPlayed[cardsPlayed.Count - 1]);
            }
            else
            {
                await AddcardsToPlayer(cardStacks.Value);
                await NextPlayer(cardsPlayed[cardsPlayed.Count - 1]);

                cardStacks = null;
            }
        }

        private async Task<bool> RemovePlayer(bool won = false)
        {
            players.Remove(currentPlayer);

            if (won)
            {
                playersWhoFinished.Add(currentPlayer);
                await SendMessageToAllPlayers($"{currentPlayer} has no more cards left comes in position {playersWhoFinished.Count}!");
            }

            if (players.Count == 1)
            {
                await SendMessageToAllPlayers("There are now too less players left to play the game").ConfigureAwait(false);
                return true;
            }

            return false;
        }

        private async Task AddcardsToPlayer(int num)
        {
            var cardsToAdd = cardsInDeck.Take(num).ToList();
            cardsInDeck.RemoveRange(0, num);

            await SendMessageToAllPlayers($"{currentPlayer.member.Username} has drawed {num} card(s)");
            await currentPlayer.AddCards(cardsToAdd);
        }

        private async Task NextPlayer(Card newCard)
        {
            if (currentPlayer.cards.Count <= 1)//UNO time
            {
                if (!await currentPlayer.UNOTime(client))
                {
                    await SendMessageToAllPlayers(null, new DiscordEmbedBuilder
                    {
                        Description = $"{currentPlayer.member.Username} forgot to say UNO lmao",
                        Color = UNOColor
                    }).ConfigureAwait(false);
                    await AddcardsToPlayer(unoMissPenalty);
                }
                else
                {
                    await SendMessageToAllPlayers(null, new DiscordEmbedBuilder
                    {
                        Description = $"{currentPlayer.member.Username} {(currentPlayer.cards.Count == 0 ? "has no more cards left" : "has 1 card left")} and they said UNO \\:(",
                        Color = UNOColor
                    }).ConfigureAwait(false);
                }
            }

            int index = players.IndexOf(currentPlayer);
            var prevPlayer = currentPlayer;

            Console.WriteLine(index);

            index += direction == GameDirection.forward ? 1 : -1;
            index = (index + (players.Count)) % players.Count;

            currentPlayer = players[index];
            currentCard = newCard;

            if (prevPlayer.cards.Count == 0)
                await RemovePlayer(true);
        }

        private async Task SendEndMessage(string message = null, bool completed = false)
        {
            var players = playersWhoFinished.Count == 0 ? null : string.Join(", ", playersWhoFinished.Select(x => x.member.Username.Insert(0, "`").Insert(x.member.Username.Length - 1, "`")));

            await SendMessageToAllPlayers(message, new DiscordEmbedBuilder
            {
                Title = "Here are the results!",
                Color = UNOColor,
                Footer = BotService.GetEmbedFooter("Hope you ppl had a good time")
            }.AddField("Winner!", completed ? playersWhoFinished[0].member.Username : "No one")
             .AddField("Players who finished", players == null ? "No one" : players));
        }

        private async Task SendMessageToAllPlayers(string message = null, DiscordEmbedBuilder embed = null)
        {
            var messageBuild = new DiscordMessageBuilder();

            if (message != null)
                messageBuild.WithContent(message);
            if (embed != null)
                messageBuild.WithEmbed(embed);

            await Task.WhenAll(players.Select(x => x.SendMessage(messageBuild)));
            await Task.WhenAll(spectators.Select(x => x.SendMessage(messageBuild)));
        }

        private async Task SendDeckToAllPlayers(List<Card> cards, string title)
        {
            await Task.WhenAll(players.Select(x => Task.Run(() => x.PrintCards(cards, title))));
            await Task.WhenAll(spectators.Select(x => Task.Run(async () => x.SendMessage(await currentPlayer.communicator.GetPrintableDeck(cards, title), emojis))));
        }

        public async Task<bool> AddSpectator(DiscordMember _member)
        {
            if (spectators.Count == maxSpectators || players.FirstOrDefault(x => x.member.Id == _member.Id) != null)
                return false;

            var spectator = new DiscordSpectator(_member, client, this);

            spectators.Add(spectator);
            var channel = await _member.CreateDmChannelAsync();

            await spectator.Ready(channel);
            return true;
        }
    }
}
