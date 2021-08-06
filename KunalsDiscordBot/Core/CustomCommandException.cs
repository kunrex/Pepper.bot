using System;
namespace KunalsDiscordBot.Core.Exceptions
{
    public class CustomCommandException : Exception
    {
        public CustomCommandException(string message = "An Exception Occured") : base(message)
        {

        }
    }
}
