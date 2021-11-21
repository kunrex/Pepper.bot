using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Events;
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
        public static readonly List<Card> Deck = GetDeck();

        public static int maxPlayersPerGame = 5, numberOfCardsDistributed = 8, inputTimeLimit = 1, maxCardsInATurn = 4, unoMissPenalty = 4;
        public static DiscordColor UNOColor = DiscordColor.Red;
        public static readonly string playing = "Playing...";

        private List<UNOPlayer> PlayersWhoFinished { get; set; } = new List<UNOPlayer>();

        private List<Card> CardsInDeck { get; set; } = new List<Card>(Deck).Shuffle().ToList();
        private Card CurrentCard { get; set; }

        private int? CardStacks { get; set; } = null;
        public List<DiscordSpectator> Spectators { get; set; } = new List<DiscordSpectator>();

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
            Players = _players.Select(x => new UNOPlayer(x)).ToList();
            CurrentPlayer = Players[0];

            OnGameOver = new SimpleBotEvent();
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
             .AddField("Number of cards given:", numberOfCardsDistributed.ToString(), true)
             .AddField("Players: ", string.Join(", ", Players.Select(x => x.member.Username.Insert(0, "`").Insert(x.member.Username.Length + 1, "`")))));

            PlayGame();
        }

        private async Task DistributeCards()
        {
            foreach (var player in Players)
            {
                player.AssignCards(CardsInDeck.Take(numberOfCardsDistributed).ToList());
                CardsInDeck.RemoveRange(0, numberOfCardsDistributed);
            }

            List<Task<bool>> awaitReady = new List<Task<bool>>();
            foreach(var player in Players)
                awaitReady.Add(player.Ready(await player.member.CreateDmChannelAsync(), Client));
            await Task.WhenAll(awaitReady);

            CurrentCard = CardsInDeck.First(x => x is NumberCard);
            CardsInDeck.Remove(CurrentCard);
        }

        protected async override void PlayGame()
        {
            while (!GameOver)
            {
                await PrintBoard();
                var result = await CurrentPlayer.GetInput(Client, CurrentCard);

                if (!result.WasCompleted)
                {
                    await SendMessageToAllPlayers($"{CurrentPlayer.member.Username} {(result.Type == InputResult.ResultType.End ? "has left the game" : "has gone afk")}").ConfigureAwait(false);
                    var end = await RemovePlayer();

                    if (end)
                    {
                        await SendEndMessage("Anyway..").ConfigureAwait(false);

                        GameOver = true;
                        continue;
                    }
                }
                else if (result.Type == InputResult.ResultType.InValid)
                {
                    await AddcardsToPlayer(1);

                    var checkResult = await CurrentPlayer.TryPlay(CurrentCard, Client);
                    if (checkResult.WasCompleted)
                        result = checkResult;
                }

                if (result.Cards != null && result.Cards.Count > 0)
                {
                    await Task.Run(async () => await SendDeckToAllPlayers(result.Cards, $"{CurrentPlayer.member.Username} Plays"));
                    await ProcessTurn(result.Cards);
                    CardsInDeck.AddRange(result.Cards.Shuffle());//cycle cards
                }
                else
                    await NextPlayer(CurrentCard);

                if (Players.Count == 1)//only one player left
                {
                    await SendMessageToAllPlayers("Theres only 1 player left now").ConfigureAwait(false);
                    await SendEndMessage("Game Over!", true).ConfigureAwait(false);

                    GameOver = true;
                    continue;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));

                List<Task> tasks = Players.Select(x => Task.Run(() => x.Ready("Are you ready to proceed to the next round? I will auto-ready after 1 minute", TimeSpan.FromMinutes(inputTimeLimit), Client))).ToList();
                await Task.WhenAll(tasks);

                await SendMessageToAllPlayers("All players ready");
            }

            OnGameOver.Invoke();
        }

        protected async override Task PrintBoard()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"{CurrentPlayer.member.Username}'s Turn",
                ImageUrl = Card.GetLink(CurrentCard.fileName).link + ".png",
                Color = UNOColor
            };

            embed.AddField("Direction:", direction.ToString());
            if (CurrentCard is IChangeColorCard)
                embed.AddField("Current Color:", ((IChangeColorCard)CurrentCard).colorToChange.ToString());
            else if (CardStacks != null)
                embed.AddField("Number of cards that will be drawed:", CardStacks.Value.ToString());

            embed.AddField("Current Card:",  CurrentCard.cardName);
            await SendMessageToAllPlayers(null, embed);

            await CurrentPlayer.PrintAllCards();
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        private async Task ProcessTurn(List<Card> cardsPlayed)
        {
            int toSkip = 1;
            bool plus2OrPlus4 = false;

            switch (cardsPlayed[0].cardType)
            {
                case CardType.plus2:
                case CardType.plus4:
                    plus2OrPlus4 = true;

                    int cardCount = 0;
                    if (CardStacks == null)
                        CardStacks = 0;

                    foreach (var card in cardsPlayed)
                        if (card is Plus2Card)
                            cardCount += 2;
                        else
                            cardCount += 4;
                    CardStacks += cardCount;
                    goto case CardType.Wild;
                case CardType.Skip:
                    toSkip = cardsPlayed.Count;
                    await SendMessageToAllPlayers($"{cardsPlayed.Count} player(s) skipped lol");
                    break;
                case CardType.Reverse:
                    direction = cardsPlayed.Count % 2 == 0 ? direction : (direction == GameDirection.forward ? GameDirection.reverse : GameDirection.forward);
                    goto case CardType.Wild;
                case CardType.number:
                case CardType.Wild:
                    if (!plus2OrPlus4 && CardStacks != null)
                    {
                        await AddcardsToPlayer(CardStacks.Value);
                        CardStacks = null;
                    }

                    await NextPlayer(cardsPlayed[cardsPlayed.Count - 1], toSkip);
                    break;
            }
        }

        private async Task<bool> RemovePlayer(bool won = false)
        {
            Players.Remove(CurrentPlayer);

            if (won)
            {
                PlayersWhoFinished.Add(CurrentPlayer);
                await SendMessageToAllPlayers($"{CurrentPlayer} has no more cards left comes in position {PlayersWhoFinished.Count}!");
            }

            if (Players.Count == 1)
            {
                await SendMessageToAllPlayers("There are now too less players left to play the game").ConfigureAwait(false);
                return true;
            }

            return false;
        }

        private async Task AddcardsToPlayer(int num)
        {
            var cardsToAdd = CardsInDeck.Take(num).ToList();
            CardsInDeck.RemoveRange(0, num);

            await SendMessageToAllPlayers($"{CurrentPlayer.member.Username} has drawed {num} card(s)");
            await CurrentPlayer.AddCards(cardsToAdd);
        }

        private async Task NextPlayer(Card newCard, int toSkip = 1)
        {
            if (CurrentPlayer.cards.Count <= 1)//UNO time
            {
                if (!await CurrentPlayer.UNOTime(Client))
                {
                    await SendMessageToAllPlayers(null, new DiscordEmbedBuilder
                    {
                        Description = $"{CurrentPlayer.member.Username} forgot to say UNO lmao",
                        Color = UNOColor
                    }).ConfigureAwait(false);
                    await AddcardsToPlayer(unoMissPenalty);
                }
                else
                {
                    await SendMessageToAllPlayers(null, new DiscordEmbedBuilder
                    {
                        Description = $"{CurrentPlayer.member.Username} {(CurrentPlayer.cards.Count == 0 ? "has no more cards left" : "has 1 card left")} and they said UNO \\:(",
                        Color = UNOColor
                    }).ConfigureAwait(false);
                }
            }

            int index = Players.IndexOf(CurrentPlayer);
            var prevPlayer = CurrentPlayer;

            index += direction == GameDirection.forward ? toSkip : -toSkip;
            index = (index + (Players.Count)) % Players.Count;

            CurrentPlayer = Players[index];
            CurrentCard = newCard;

            if (prevPlayer.cards.Count == 0)
                await RemovePlayer(true);
        }

        private async Task SendEndMessage(string message = null, bool completed = false)
        {
            var players = PlayersWhoFinished.Count == 0 ? null : string.Join(", ", PlayersWhoFinished.Select(x => x.member.Username.Insert(0, "`").Insert(x.member.Username.Length - 1, "`")));

            await SendMessageToAllPlayers(message, new DiscordEmbedBuilder
            {
                Title = "Here are the results!",
                Color = UNOColor,
            }.AddField("Winner!", completed ? PlayersWhoFinished[0].member.Username : "No one")
             .AddField("Players who finished", players == null ? "No one" : players)
             .WithFooter("Hope you ppl had a good time"));
        }

        private async Task SendMessageToAllPlayers(string message = null, DiscordEmbedBuilder embed = null)
        {
            var messageBuild = new DiscordMessageBuilder();

            if (message != null)
                messageBuild.WithContent(message);
            if (embed != null)
                messageBuild.WithEmbed(embed);

            await Task.WhenAll(Players.Select(x => x.SendMessage(messageBuild)));
            await Task.WhenAll(Spectators.Select(x => x.SendMessage(messageBuild)));
        }

        private async Task SendDeckToAllPlayers(List<Card> cards, string title)
        {
            await Task.WhenAll(Players.Select(x => Task.Run(() => x.PrintCards(cards, title))));
            await Task.WhenAll(Spectators.Select(x => Task.Run(async () => x.SendMessage(await CurrentPlayer.communicator.GetPrintableDeck(cards, title), default))));
        }

        public async Task<bool> AddSpectator(DiscordMember _member)
        {
            if (Spectators.Count == maxSpectators || Players.FirstOrDefault(x => x.member.Id == _member.Id) != null)
                return false;
            
            var spectator = new DiscordSpectator(_member, Client, this);

            Spectators.Add(spectator);
            var channel = await _member.CreateDmChannelAsync();

            await spectator.Ready(channel);
            return true;
        }
    }
}
