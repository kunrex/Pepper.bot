using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Core.Help;
using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Modules;

namespace KunalsDiscordBot.Modules.Help
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public sealed class HelpCommands : PepperCommandModule
    {
        public override PepperCommandModuleInfo ModuleInfo { get; protected set; }

        public HelpCommands()
        {

        }

        [Command("Help")]
        [Description("Displays command help")]
        [Cooldown(1, 5, CooldownBucketType.User)]
        public async Task Help(CommandContext ctx, [Description("The command to show help for")] params string[] command)
        {
            if (command == null || !command.Any())
            {
                var sorted = ctx.CommandsNext.FilteredRegisteredCommands().OrderBy(x => x.Name).Where(x => !x.RunChecksAsync(ctx, true).GetAwaiter().GetResult().Any()).ToList();

                var embeds = new List<DiscordEmbedBuilder>() { new HelpFormatter(ctx.Member.DisplayName).WithSubcommands(sorted).Build() };

                foreach (var sortedCommand in sorted.Where(x => x is CommandGroup))
                {
                    var casted = (CommandGroup)sortedCommand;

                    var executables = casted.Children.Where(x => !x.RunChecksAsync(ctx, true).GetAwaiter().GetResult().Any());
                    embeds.Add(new HelpFormatter(ctx.Member.DisplayName).WithSubcommands(executables).Build());
                }

                var pages = embeds.Select(x => new Page("", x));
                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, default, PaginationBehaviour.WrapAround, ButtonPaginationBehavior.Disable, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);
            }
            else
            {
                var helpBuilder = new HelpFormatter(ctx.Member.DisplayName);
                var searchIn = ctx.CommandsNext.FilteredRegisteredCommands().ToList();
                Command searchCommand = null;

                foreach(var _command in command)
                {
                    if(searchIn == null)
                    {
                        searchCommand = null;
                        break;
                    }

                    searchCommand = searchIn.FirstOrDefault(x => x.Name.ToLowerInvariant() == _command.ToLowerInvariant() || (x.Aliases != null && x.Aliases.Select(xs => xs.ToLowerInvariant()).Contains(_command.ToLowerInvariant())));

                    if (searchCommand == null)
                        break;

                    var failedChecks = await searchCommand.RunChecksAsync(ctx, true).ConfigureAwait(false);
                    if (failedChecks.Any())
                        throw new ChecksFailedException(searchCommand, ctx, failedChecks);

                    searchIn = searchCommand is CommandGroup ? (searchCommand as CommandGroup).Children.ToList() : null;
                }

                if (searchCommand == null)
                    throw new CommandNotFoundException(string.Join(" ", command));

                helpBuilder.WithCommand(searchCommand);

                if (searchCommand is CommandGroup group)
                {
                    var subCommands = group.Children.Where(x => !x.IsHidden);
                    var executables = new List<Command>();
                    foreach (var subCommand in subCommands)
                    {
                        if (subCommand.ExecutionChecks == null || !subCommand.ExecutionChecks.Any())
                        {
                            executables.Add(subCommand);
                            continue;
                        }

                        var candidateFailedChecks = await subCommand.RunChecksAsync(ctx, true).ConfigureAwait(false);
                        if (!candidateFailedChecks.Any())
                            executables.Add(subCommand);
                    }

                    if (executables.Any())
                        helpBuilder.WithSubcommands(executables.OrderBy(xc => xc.Name));
                }

                await ctx.Channel.SendMessageAsync(helpBuilder.Build());
            }
        }
    }
}
