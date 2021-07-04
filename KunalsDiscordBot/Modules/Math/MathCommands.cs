//System name spaces
using System.Threading.Tasks;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

//Custom name spcaes
using KunalsDiscordBot.Attributes;
using KunalsDiscordBot.Services.Math;

using Math = System.Math;

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
            var solver = new LinearEquationSolver($"{equation} .");

            string answer = solver.Solve(solver.GetPolynomials(equation));
            await ctx.RespondAsync(answer).ConfigureAwait(false);
        }

        [Command("add")]
        [Description("Adds the given 2 numbers")]
        public async Task Add(CommandContext ctx, float number1, float number2) => await ctx.RespondAsync((number1 + number2).ToString()).ConfigureAwait(false);

        [Command("subtract")]
        [Description("Subtracts the given 2 numbers")]
        public async Task Sub(CommandContext ctx, float number1, float number2) => await ctx.RespondAsync((number1 - number2).ToString()).ConfigureAwait(false);

        [Command("multiply")]
        [Description("Multiplies the given 2 numbers")]
        public async Task Mul(CommandContext ctx, float number1, float number2) => await ctx.RespondAsync((number1 * number2).ToString()).ConfigureAwait(false);

        [Command("divide")]
        [Description("Divides the given 2 numbers")]
        public async Task Div(CommandContext ctx, float number1, float number2) => await ctx.RespondAsync((number1 / number2).ToString()).ConfigureAwait(false);

        [Command("sin")]
        [Description("Find the sin of a number")]
        public async Task Sin(CommandContext ctx, float number) => await ctx.RespondAsync(System.Math.Sin(number).ToString()).ConfigureAwait(false);

        [Command("cos")]
        [Description("Find the cos of a number")]
        public async Task Cos(CommandContext ctx, float number) => await ctx.RespondAsync(System.Math.Cos(number).ToString()).ConfigureAwait(false);

        [Command("tan")]
        [Description("Find the tan of a number")]
        public async Task Tan(CommandContext ctx, float number) => await ctx.RespondAsync(System.Math.Tan(number).ToString()).ConfigureAwait(false);

        [Command("cosecant")]
        [Aliases("cosec")]
        [Description("Find the cosecant of a number")]
        public async Task Cosecant(CommandContext ctx, float number) => await ctx.RespondAsync((1f / System.Math.Sin(number)).ToString()).ConfigureAwait(false);

        [Command("secant")]
        [Aliases("sec")]
        [Description("Find the cosecant of a number")]
        public async Task Secant(CommandContext ctx, float number) => await ctx.RespondAsync((1f / System.Math.Cos(number)).ToString()).ConfigureAwait(false);

        [Command("cotangent")]
        [Aliases("cot")]
        [Description("Find the cosecant of a number")]
        public async Task Cotangent(CommandContext ctx, float number) => await ctx.RespondAsync((1f / System.Math.Tan(number)).ToString()).ConfigureAwait(false);

        [Command("pi")]
        [Description("value of PI")]
        public async Task Pi(CommandContext ctx) => await ctx.RespondAsync(System.Math.PI.ToString()).ConfigureAwait(false);

        [Command("e")]
        [Description("value of E")]
        public async Task E(CommandContext ctx) => await ctx.RespondAsync(System.Math.E.ToString()).ConfigureAwait(false);

        [Command("squareroot")]
        [Aliases("sqrt")]
        [Description("Find the square root of a number")]
        public async Task Sqrt(CommandContext ctx, float number) => await ctx.RespondAsync((System.Math.Sqrt(number)).ToString()).ConfigureAwait(false);

        [Command("root")]
        [Description("Finds the N'th root of a number")]
        public async Task Root(CommandContext ctx, float number, float root) => await ctx.RespondAsync((System.Math.Pow(number, 1.0 / root)).ToString()).ConfigureAwait(false);

        [Command("pow")]
        [Description("Finds the N'th power of a number")]
        public async Task Pow(CommandContext ctx, float number, float root) => await ctx.RespondAsync((System.Math.Pow(number, root)).ToString()).ConfigureAwait(false);
    }
}
