//System name spaces
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using KunalsDiscordBot.Modules.Games.Communicators;

namespace KunalsDiscordBot.Modules.Games.Players
{
    public abstract class DiscordPlayer<Communicator> where Communicator : DiscordCommunicator 
    {
        public DiscordPlayer(DiscordMember _member)
        {
            member = _member;
        }

        public DiscordMember member { get; protected set; }
        public Communicator communicator { get; protected set; }

        public abstract Task<bool> Ready(DiscordChannel channel);
    }
}
