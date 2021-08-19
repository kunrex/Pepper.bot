using System;

namespace KunalsDiscordBot.Core.Exceptions.CurrencyCommands
{
    public class PresenceAttributeNotFoundException : Exception
    {
        public PresenceAttributeNotFoundException(string item) : base($"The presence item was not found on the {item} command")
        {

        }
    }
}
