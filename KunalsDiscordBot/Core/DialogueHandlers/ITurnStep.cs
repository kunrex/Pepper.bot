using System;

namespace KunalsDiscordBot.Core.DialogueHandlers
{
    public interface ITurnStep
    {
        public int Tries { get; }
        public string TryAgainMessage { get; }
    }
}
