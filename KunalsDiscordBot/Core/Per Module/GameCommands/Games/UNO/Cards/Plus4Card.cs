
namespace KunalsDiscordBot.Core.Modules.GameCommands.UNO.Cards
{
    public class Plus4Card : PowerCard, IChangeColorCard
    {
        public Plus4Card(CardColor color, CardType type = CardType.plus4) : base(color, type)
        {

        }

        public override CardType stackables => CardType.plus4;
        public CardColor colorToChange { get; set; }

        public override bool ValidNextCardCheck(Card card) => card.cardType == CardType.Wild || card.cardType == CardType.plus4 || card.cardColor == colorToChange;
    }
}
