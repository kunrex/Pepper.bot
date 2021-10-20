using System;
using System.Threading.Tasks;
using System.Collections.Generic;


using KunalsDiscordBot.Core.Modules.MathCommands.Evaluation;

namespace KunalsDiscordBot.Core.Modules.MathCommands
{
    public interface IMathEvaluator
    {
        public Task<string> Solve();
    }
}
