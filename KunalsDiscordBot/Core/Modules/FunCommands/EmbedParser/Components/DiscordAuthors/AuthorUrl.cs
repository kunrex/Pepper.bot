using System;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.DiscordAuthors
{
    public class AuthorUrl : Url
    {
        public override bool Outer => false;
        public override string Id { get => "authorurl"; }

        public AuthorUrl() 
        {
            Regex = new Regex("(.*)");
        }

        public override DiscordEmbedBuilder Modify(DiscordEmbedBuilder builder) => builder;
    }
}
