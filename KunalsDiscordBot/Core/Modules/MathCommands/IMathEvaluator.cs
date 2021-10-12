using System;
using System.Threading.Tasks;

namespace KunalsDiscordBot.Core.Modules.MathCommands
{
    public interface IMathEvaluator
    {
        public Task<string> Solve();
    }
}
