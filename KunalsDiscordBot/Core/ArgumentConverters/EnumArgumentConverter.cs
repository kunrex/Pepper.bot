using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;

namespace KunalsDiscordBot.Core.ArgumentConverters
{
    public class EnumArgumentConverter<T> : IArgumentConverter<T> where T : Enum
    {
        public Task<Optional<T>> ConvertAsync(string value, CommandContext ctx)
        {
            if (Enum.TryParse(typeof(T), value, true, out var x))
                return Task.FromResult(Optional.FromValue((T)Enum.Parse(typeof(T), value, true)));

            return Task.FromResult(Optional.FromNoValue<T>());
        }
    }
}
