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

namespace KunalsDiscordBot.Modules.Games.Complex
{
    public class UNOGame : ComplexBoardGame<UNOPlayer>
    {
        public static readonly List<CardColor> colors = Enum.GetValues(typeof(CardColor)).Cast<CardColor>().ToList();
        public static int maxPlayers = 5, startCardNumber = 8;

        public DiscordClient client { get; private set; }
        private bool gameOver { get; set; }

        private List<UNOPlayer> playersWhoFinished { get; set; }
        private List<DiscordChannel> dmChannels { get; set; }
        private List<Card> cards { get; set; } = GetDeck().Shuffle().ToList();
        private Card currenctCard { get; set; }

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
            SetUp();
        }

        protected async override void PlayGame()
        {
            
        }

        protected async override Task PrintBoard()
        {
            
        }

        protected async override void SetUp()
        {
            dmChannels = new List<DiscordChannel>();
            foreach (var player in players)
            {
                dmChannels.Add(await player.member.CreateDmChannelAsync());
                player.InitialisePlayer(cards.Take(startCardNumber).ToList(), client);

                cards.RemoveRange(0, startCardNumber);
            }

            List <Task<bool>> awaitReady = new List<Task<bool>>();
            for (int i = 0; i < players.Count; i++)
                awaitReady.Add(players[i].Ready(dmChannels[i]));

            var task = Task.WhenAll(awaitReady);
            await task;
            currenctCard = cards[0];

            await SendMessageToAllPlayers(null, new DiscordEmbedBuilder
            {
                Title = "UNO!"
            }.AddField("Host:", "Me", true)
             .AddField("Starting Cards Number:", startCardNumber.ToString(), true)
             .AddField("Players: ", string.Join(',', players)));
            await SendMessageToAllPlayers("Starting Game!").ConfigureAwait(false);

            await SendMessageToAllPlayers(null, new DiscordEmbedBuilder
            {
                Title = $"{currentPlayer.member.Username}'s Turn",
                ImageUrl = Card.GetLink(currenctCard.fileName).link + ".png"
            }.AddField("Current Card:", "** **"));

            await currentPlayer.PrintCards();
        }

        private async Task SendMessageToAllPlayers(string message = null, DiscordEmbedBuilder embed = null)
        {
            foreach(var channel in dmChannels)
            {
                var messageBuild = new DiscordMessageBuilder();

                if(message != null)
                    messageBuild.WithContent(message);
                if (embed != null)
                    messageBuild.WithEmbed(embed);

                await messageBuild.SendAsync(channel);
            }
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
    }
}
