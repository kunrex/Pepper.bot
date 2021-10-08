using System;

namespace KunalsDiscordBot.Core.Modules.MathCommands.Exceptions
{
    public class EvaluationException : Exception
    {
        public EvaluationException(string message) : base(message)
        {

        }
    }
}
