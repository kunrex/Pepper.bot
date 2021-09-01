using System.Collections.Generic;

using KunalsDiscordBot.Core.DialogueHandlers.Steps;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Jobs
{
    public abstract class WorkData<WorkReturnType>
    {
        protected readonly int totalTime;
        public int TotalTime { get => totalTime ; }

        public readonly int numberOfTurns;
        public int NumberOfTurns { get => numberOfTurns; }

        public WorkData(int time, int numOfTurns)
        {
            totalTime = time;
            numberOfTurns = numOfTurns;
        }

        public abstract WorkReturnType GetWork();
    }
}
