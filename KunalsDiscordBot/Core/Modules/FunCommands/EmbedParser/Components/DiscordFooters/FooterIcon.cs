using System;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.DiscordFooters
{
    public class FooterIcon : ImageUrl
    {
        public override bool Outer => false;
        public override string Id { get => "footericon"; }

        public FooterIcon()
        {
            Regex = new Regex("(.*)");
        }

        public override DiscordEmbedBuilder Modify(DiscordEmbedBuilder builder) => builder;
    }
}
