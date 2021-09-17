using System;
namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.CurrencyModels
{
    public partial struct CurrencyModel
    {
        private readonly string name;
        public string Name => name;

        private readonly int mimimumCoins;
        public int MimimumCoins => mimimumCoins;

        private readonly int maximumCoins;
        public int MaximumCoins => maximumCoins;

        private readonly string succeedMessage;
        public string SuccedMessage => succeedMessage;

        private readonly string failureMessage;
        public string FailureMessage => failureMessage;

        public CurrencyModel(string _name, int _minimumCoins, int _maximumCoins, string _succeedMessage, string _failurMessage)
        {
            name = _name;

            mimimumCoins = _minimumCoins;
            maximumCoins = _maximumCoins;

            succeedMessage = _succeedMessage;
            failureMessage = _failurMessage;
        }
    }
}
