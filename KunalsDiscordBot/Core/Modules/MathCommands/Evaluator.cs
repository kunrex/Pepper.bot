using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using KunalsDiscordBot.Core.Modules.MathCommands.Evaluation;

namespace KunalsDiscordBot.Core.Modules.MathCommands
{
    public sealed class Evaluator : IMathEvaluator
    {
        public string Expression { get; private set; }

        private List<Token> tokensEvaluated = new List<Token>();
        public List<Token> TokensEvaulated { get => tokensEvaluated; }

        public Evaluator(string expression) => Expression = expression;

        public Task<string> Solve() => Task.FromResult(LinearEquationLexer.Create1(LinearEquationLexer.GetTokens(Expression)).Simplyfy().Value);
    }
}
