using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;

namespace KunalsDiscordBot.Core.ArgumentConverters
{
    public class TimeSpanArgumentConverter : IArgumentConverter<TimeSpan>
    {
        const string regexString = "([0-9])+([A-z])\\w";

        public Task<Optional<TimeSpan>> ConvertAsync(string value, CommandContext ctx)
        {
            if (TimeSpan.TryParse(value, out var time))
                return Task.FromResult(Optional.FromValue(TimeSpan.Parse(value)));

            var regexp = new System.Text.RegularExpressions.Regex(regexString);
            if (!regexp.IsMatch(value))
                return Task.FromResult(Optional.FromNoValue<TimeSpan>());

            string unit = value.Substring(value.Length - 2);
            string magnitude = value.Substring(0, value.Length - 2);

            if (!int.TryParse(magnitude, out var val))
                return Task.FromResult(Optional.FromNoValue<TimeSpan>());

            int magnitudeToInt = int.Parse(magnitude);
            switch(unit)
            {
                case "sc":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromSeconds(magnitudeToInt)));
                case "mi":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromMinutes(magnitudeToInt)));
                case "hr":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromHours(magnitudeToInt)));
                case "dy":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromDays(magnitudeToInt)));
                case "wk":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromDays(magnitudeToInt * 7)));
                case "mo":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromDays(magnitudeToInt * 30)));
                case "yr":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromDays(magnitudeToInt * 365)));
                default:
                    return Task.FromResult(Optional.FromNoValue<TimeSpan>());
            }
        }
    }
}
