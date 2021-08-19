
namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Jobs
{
    public struct WorkInfo
    {
        public string description { get; set; }
        public string correctResult { get; set; }

        public int tries { get; set; }
        public int timeToDo { get; set; }
    }
}
