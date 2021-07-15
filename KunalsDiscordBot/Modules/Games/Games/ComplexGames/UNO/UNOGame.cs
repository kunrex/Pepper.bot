using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using KunalsDiscordBot.Modules.Games.Players;
using KunalsDiscordBot.Modules.Games.Complex.UNO.Cards;
using KunalsDiscordBot.Modules.Games.Complex.UNO;

namespace KunalsDiscordBot.Modules.Games.Complex
{
    public class UNOGame : ComplexBoardGame<UNOPlayer>
    {
        public static readonly List<CardColor> colors = Enum.GetValues(typeof(CardColor)).Cast<CardColor>().ToList();
        public static readonly List<CardType> types = Enum.GetValues(typeof(CardType)).Cast<CardType>().ToList();

        public static int maxPlayers = 5;

        public DiscordClient client { get; private set; }
        private bool gameOver { get; set; }

        private List<UNOPlayer> playersWhoFinished { get; set; }
        private List<DiscordChannel> dmChannels { get; set; }
        private List<Card> cards { get; set; } = GetDeck().Shuffle().ToList();

        public static List<Card> GetDeck()
        {
            var cards = new List<Card>();
            //4 wild cards
            for (int i = 0; i < 4; i++)
                cards.Add(new PowerCard(CardType.powerplay, CardColor.none, PowerType.Wild));

            //4 wild cards
            for (int i = 0; i < 4; i++)
                cards.Add(new PowerCard(CardType.powerplay, CardColor.none, PowerType.plus4));

            foreach (var color in colors.Where(x => x != CardColor.none).ToList())
            {
                //add number cards
                cards.Add(new NumberCard(CardType.number, color, 0));
                for (int i = 1; i <= 2; i++)
                    for (int k = 1;k <= 9; k++)
                        cards.Add(new NumberCard(CardType.number, color, i));

                //add reverse, skip and +2
                var actionCards = Enum.GetValues(typeof(PowerType)).Cast<PowerType>().Where(x => x != PowerType.Wild && x != PowerType.plus4).ToList();
                for (int i = 0; i < actionCards.Count; i++)
                    for (int k = 1; k <= 2; k++)
                        cards.Add(new PowerCard(CardType.powerplay, color, actionCards[i]));
            }

            return cards;
        }

        public UNOGame(List<DiscordMember> members, DiscordClient _client)
        {
            Console.WriteLine("here");
            var _players = new List<UNOPlayer>();
            foreach (var member in members)
                _players.Add(new UNOPlayer(member, new List<Card>()));

            Console.WriteLine("here");
            client = _client;
            players = _players;

            Console.WriteLine("here");
            currentPlayer = players[0];
            gameOver = false;
            Console.WriteLine("here");
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
            var str = string.Empty;
            foreach (var card in cards)
                str += card.fileName + "\n";
            Console.WriteLine("here");
            var embed = new DiscordEmbedBuilder
            {
                Description = str
            };

            await (await currentPlayer.member.CreateDmChannelAsync()).SendMessageAsync(embed);

            /*            dmChannels = new List<DiscordChannel>();

            foreach (var player in players)
                dmChannels.Add(await player.member.CreateDmChannelAsync());

             * 
             * List<Task<bool>> awaitReady = new List<Task<bool>>();
            for(int i =0;i<players.Count;i++)
                awaitReady.Add(players[i].Ready(dmChannels[i]));

            var task = Task.WhenAll(awaitReady);
            await task;*/
        }
    }
}
