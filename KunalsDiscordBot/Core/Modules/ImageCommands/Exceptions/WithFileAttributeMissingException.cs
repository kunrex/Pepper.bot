using System;
namespace KunalsDiscordBot.Core.Exceptions.ImageCommands
{
    public class WithFileAttributeMissingException : Exception
    {
        public WithFileAttributeMissingException(string command): base($"The WithFileAttribute was not found on the {command}.")
        {

        }
    }
}
