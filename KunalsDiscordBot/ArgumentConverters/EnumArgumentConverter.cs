using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using KunalsDiscordBot.Core.Reddit;

namespace KunalsDiscordBot.ArgumentConverters
{
    public class EnumArgumentConverter<T> : IArgumentConverter<T> where T : Enum
    {
        public Task<Optional<T>> ConvertAsync(string value, CommandContext ctx)
        {
            if (Enum.TryParse(typeof(T), value, out var x))
                return Task.FromResult(Optional.FromValue((T)Enum.Parse(typeof(T), value)));

            return Task.FromResult(Optional.FromNoValue<T>());
        }
    }
}
