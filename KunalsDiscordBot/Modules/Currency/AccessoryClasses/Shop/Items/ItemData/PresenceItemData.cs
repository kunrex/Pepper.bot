using System;
namespace KunalsDiscordBot.Modules.Currency.Shops.Items
{
    public struct PresenceItemData 
    {
        [Flags]
        public enum PresenceCommand
        {
            Meme = 0,
            Game = 2,
            Code = 4,
            Hunt = 8,
            Fish = 16 
        }

        public readonly PresenceCommand allowedCommands;

        public PresenceItemData(PresenceCommand command) => allowedCommands = command;
    }
}
