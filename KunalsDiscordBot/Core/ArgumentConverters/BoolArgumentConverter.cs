using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.ArgumentConverters
{
    public class BoolArgumentConverter : IArgumentConverter<bool>
    {
        public Task<Optional<bool>> ConvertAsync(string value, CommandContext ctx)
        {
            if (bool.TryParse(value, out var boolean))
            {
                return Task.FromResult(Optional.FromValue(boolean));
            }

            switch (value.ToLower())
            {
                case "yes":
                case "y":
                case "t":
                    return Task.FromResult(Optional.FromValue(true));

                case "no":
                case "n":
                case "f":
                    return Task.FromResult(Optional.FromValue(false));

                default:
                    return Task.FromResult(Optional.FromNoValue<bool>());
            }
        }
    }
}
