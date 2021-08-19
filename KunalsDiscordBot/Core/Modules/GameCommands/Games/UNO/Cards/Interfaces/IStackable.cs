namespace KunalsDiscordBot.Core.Modules.GameCommands.UNO.Cards
{
    public interface IStackable
    {
        public CardType stackables { get; }

        public bool Stack(Card card);
    }
}
