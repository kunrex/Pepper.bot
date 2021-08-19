namespace KunalsDiscordBot.Core.Modules.GameCommands.UNO.Cards
{
    public class ReverseCard : PowerCard
    {
        public ReverseCard(CardColor color, CardType type = CardType.Reverse) : base(color, type)
        {

        }

        public override CardType stackables => CardType.Reverse;
        public override bool ValidNextCardCheck(Card card) => card.cardType == CardType.Wild || card.cardType == CardType.plus4 || card.cardType == CardType.Reverse || card.cardColor == cardColor;
    }
}
