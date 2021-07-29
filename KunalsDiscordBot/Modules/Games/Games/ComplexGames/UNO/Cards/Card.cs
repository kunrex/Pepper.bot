using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using KunalsDiscordBot.Modules.Games.Complex.UNO.Cards;

namespace KunalsDiscordBot.Modules.Games.Complex.UNO
{
    public enum CardColor
    {
        none,
        red,
        green,
        blue,
        yellow
    }

    [Flags]
    public enum CardType
    {
        plus2,
        Skip,
        Reverse,
        plus4,
        Wild,
        number
    }

    public abstract class Card : IStackable
    {
        public static readonly CardLinks cardLinks = System.Text.Json.JsonSerializer.Deserialize<CardLinks>(System.IO.File.ReadAllText(System.IO.Path.Combine("Modules", "Games", "Games", "ComplexGames", "UNO", "Cards.json")));

        public CardColor cardColor { get; protected set; }
        public CardType cardType { get; protected set; }

        public string fileName { get; protected set; }
        public string cardName { get; protected set; }

        public abstract CardType stackables { get; }
        public static Link GetLink(string fileName) => cardLinks.links.FirstOrDefault(x => x.card == fileName);

        public Card(CardColor color) => cardColor = color;

        protected abstract string GetFileName();
        protected abstract string GetCardName();
        public abstract bool ValidNextCardCheck(Card card);

        public abstract bool Stack(Card card);
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
