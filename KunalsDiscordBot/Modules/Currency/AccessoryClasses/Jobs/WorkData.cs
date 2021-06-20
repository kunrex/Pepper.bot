using System;
using System.Collections.Generic;

namespace KunalsDiscordBot.Modules.Currency.Jobs
{
    public class WorkData<T> 
    {
        public readonly int totalTime;
        public readonly int numberOfTurns;

        public readonly T workData;

        public WorkData(int time, int numOfTurns, T data)
        {
            totalTime = time;
            numberOfTurns = numOfTurns;

            workData = data;
        }
    }
}
