using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace KunalsDiscordBot.Modules.Games.Complex.UNO
{
    public enum CardType
    {
        number,
        powerplay,
    }

    public enum CardColor
    {
        red,
        green,
        blue,
        yellow,
        none
    }

    public abstract class Card
    {
        public CardType cardType { get; protected set; }
        public CardColor cardColor { get; protected set; }

        public string fileName { get; protected set; }
        public string cardName { get; protected set; }

        public static IReadOnlyList<string> Path = new List<string>() { "Modules", "Games", "Games", "ComplexGames", "UNO", "Cards.json" };
        public static readonly CardLinks cardLinks = System.Text.Json.JsonSerializer.Deserialize<CardLinks>(System.IO.File.ReadAllText(System.IO.Path.Combine("Modules", "Games", "Games", "ComplexGames", "UNO", "Cards.json")));

        public static Link GetLink(string fileName) => cardLinks.links.FirstOrDefault(x => x.card == fileName);

        public Card(CardType type, CardColor color)
        {
            cardType = type;
            cardColor = color;
        }

        protected abstract string GetFileName();
        protected abstract string GetCardName();
    }

    public class CardLinks
    {
        public Link[] links { get; set; }
    }

    public class Link
    {
        public string card { get; set; }
        public string link { get; set; }
    }
}
