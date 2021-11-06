using System;
using System.Linq;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;

using KunalsDiscordBot.Core.DiscordModels;

namespace KunalsDiscordBot.Extensions
{
    public static partial class PepperBotExtensions
    {
        public static IEnumerable<Page> GetPages<T>(this InteractivityExtension interactivity, IEnumerable<T> list, Func<T, (string, string)> func, EmbedSkeleton skeleton, int perPage = 7, bool inLine = default)
        {
            var embeds = new List<DiscordEmbedBuilder>();
            DiscordEmbedBuilder current = null;

            int index = 0;

            foreach (var elemant in list)
            {
                if (index % perPage == 0)
                {
                    current = new DiscordEmbedBuilder().WithTitle(skeleton.Title)
                        .WithDescription(skeleton.Description)
                        .WithColor(skeleton.Color);

                    if (skeleton.Footer != null)
                        current.Footer = skeleton.Footer;
                    if (skeleton.Author != null)
                        current.Author = skeleton.Author;
                    if (skeleton.Thumbnail != null)
                        current.Thumbnail = skeleton.Thumbnail;

                    embeds.Add(current);
                }

                (string _heading, string _value) = func.Invoke(elemant);
                current.AddField($"{++index}. {_heading}", _value, inLine);
            }

            if (embeds.Count == 0)
                return null;

            return embeds.Select(x => new Page(null, x));
        }

        public static int GetCommandCount(this CommandsNextExtension commandNext)
        {
            int count = 0;
            var commands = commandNext.FilteredRegisteredCommands().ToArray();

            foreach (var command in commands)
                count += command.GetSubCommandsCount();

            return count;
        }

        public static int GetSubCommandsCount(this Command command)
        {
            if(command is CommandGroup group)
            {
                int count = group.Children.Select(x => x.GetSubCommandsCount()).Sum();
                return count;
            }

            return 1;
        }
    }
}
