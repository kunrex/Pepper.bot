//System name spaces
using System.Threading.Tasks;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

//Custom name spcaes
using KunalsDiscordBot.Attributes;

using KunalsDiscordBot.Services.Math;

namespace KunalsDiscordBot.Modules.Math
{
    [Group("Math")]
    [Decor("MidnightBlue", ":1234:")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class MathCommands : BaseCommandModule
    {

        [Command("Solve")]
        [Description("Solves an equation")]
        public async Task Solve(CommandContext ctx, [RemainingText] string equation)
        {
            var solver = new LinearEquationSolver(equation);

            string answer = solver.Solve(solver.GetPolynomials(equation));
            await ctx.Channel.SendMessageAsync(answer).ConfigureAwait(false);
        }

        #region Basic Operations
        [Command("add")]
        [Description("Adds the given 2 numbers")]
        public async Task Add(CommandContext ctx, float number1, float number2)
        {
            await ctx.Channel.SendMessageAsync((number1 + number2).ToString()).ConfigureAwait(false);
        }

        [Command("subtract")]
        [Description("Subtracts the given 2 numbers")]
        public async Task Sub(CommandContext ctx, float number1, float number2)
        {
            await ctx.Channel.SendMessageAsync((number1 - number2).ToString()).ConfigureAwait(false);
        }

        [Command("multiply")]
        [Description("Multiplies the given 2 numbers")]
        public async Task Mul(CommandContext ctx, float number1, float number2)
        {
            await ctx.Channel.SendMessageAsync((number1 * number2).ToString()).ConfigureAwait(false);
        }

        [Command("divide")]
        [Description("Divides the given 2 numbers")]
        public async Task Div(CommandContext ctx, float number1, float number2)
        {
            await ctx.Channel.SendMessageAsync((number1 / number2).ToString()).ConfigureAwait(false);
        }
        #endregion
    }
}
