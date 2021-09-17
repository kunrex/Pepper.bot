using System;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Jobs
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
