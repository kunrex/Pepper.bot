using System;
namespace KunalsDiscordBot.Core.Modules.MathCommands.Evaluation
{
    public interface IEvaluable
    {
        public Token Evaluate();
    }
}
