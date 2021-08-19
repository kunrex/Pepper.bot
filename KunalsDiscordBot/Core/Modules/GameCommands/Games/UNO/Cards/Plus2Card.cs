
namespace KunalsDiscordBot.Core.Modules.GameCommands.UNO.Cards
{
    public class Plus2Card : PowerCard
    {
        public Plus2Card(CardColor color, CardType type = CardType.plus2) : base(color, type)
        {

        }

        public override CardType stackables => CardType.plus2 | CardType.plus4;
        public override bool ValidNextCardCheck(Card card) => card.cardType == CardType.plus2 || card.cardType == CardType.Wild || card.cardType == CardType.plus4 || card.cardColor == cardColor;
    }
}
