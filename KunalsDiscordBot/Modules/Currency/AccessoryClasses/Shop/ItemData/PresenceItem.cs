using System;
namespace KunalsDiscordBot.Modules.Currency.Shops.Items
{
    public struct PresenceItem 
    {
        public enum PresenceCommand
        {
            Meme,
            Game,
            Code,
            Hunt,
            Fish 
        }

        public readonly PresenceCommand allowedCommands;

        public PresenceItem(PresenceCommand command) => allowedCommands = command;
    }
}
