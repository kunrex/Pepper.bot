using System;

using KunalsDiscordBot.Core.Modules.MathCommands.Evaluation;

namespace KunalsDiscordBot.Core.Modules.MathCommands.Exceptions
{
    public class InvalidCharacterException : Exception
    {
        public InvalidCharacterException(char invalidCharacter, TokenType expected) : base($"Invalid character `{invalidCharacter}` detected, {expected} character expected")
        {

        }
    }
}
