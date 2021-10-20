using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using KunalsDiscordBot.Core.Modules.MathCommands.Evaluation;
using KunalsDiscordBot.Core.Modules.MathCommands.Exceptions;

namespace KunalsDiscordBot.Core.Modules.MathCommands
{
    public sealed class LinearEquationSolver : IMathEvaluator
    {
        public char Variable { get; private set; }

        private readonly string equation;
        public string Equation { get => equation; }

        private List<Token> RHS { get; set; }
        private List<Token> LHS { get; set; }

        private List<Token> tokensEvaluated = new List<Token>();
        public List<Token> TokensEvaulated { get => tokensEvaluated; }

        public LinearEquationSolver(string _equation)
        {
            equation = _equation;

            Variable = LinearEquationLexer.GetVariable(equation);
        }

        public Task<string> Solve()
        {
            if (!equation.Contains('='))
                throw new EvaluationException("No `=` detected");

            var sides = equation.Split('=');

            RHS = LinearEquationLexer.GetTokens(sides[1], Variable);
            LHS = LinearEquationLexer.GetTokens(sides[0], Variable);

            Simplify();
            return Task.FromResult($"{Variable} is {TransferAndSolve()}");
        }

        private void Simplify()
        {
            var rhs = new Token("1", TokenType.Constant, RHS).Simplyfy();
            var lhs = new Token("1", TokenType.Constant, LHS).Simplyfy();

            RHS = rhs.GetDeepestSubTokens();
            LHS = lhs.GetDeepestSubTokens();
        }

        private float TransferAndSolve()
        {
            while (RHS.Count > 1 || LHS.Count > 1)
            {
                SegregateAndFlip(RHS, TokenType.Variable, out var numbers, out var toAddVariables, out var lhsM);
                SegregateAndFlip(LHS, TokenType.Constant, out var toAddNumbers, out var variables, out var rhsM);

                Token simplifiedLHS = new Token("1", TokenType.Constant, variables).Simplyfy(),
                    simplifiedRHS = new Token("1", TokenType.Constant, numbers).Simplyfy();

                simplifiedRHS *= rhsM;
                simplifiedLHS *= lhsM;

                numbers = simplifiedRHS.GetDeepestSubTokens();
                variables = simplifiedLHS.GetDeepestSubTokens();

                if (toAddNumbers.Count != 1)
                {
                    toAddNumbers.Insert(0, LinearEquationLexer.Plus);
                    numbers.AddRange(toAddNumbers);
                }

                if (toAddVariables.Count != 1)
                {
                    toAddVariables.Insert(0, LinearEquationLexer.Plus);
                    variables.AddRange(toAddVariables);
                }

                var rhs = new Token("1", TokenType.Constant, numbers).Simplyfy();
                var lhs = new Token("1", TokenType.Constant, variables).Simplyfy();

                RHS = rhs.GetDeepestSubTokens();
                LHS = lhs.GetDeepestSubTokens();
            }

            if (RHS[0].Type == LHS[0].Type)
                throw new EvaluationException(RHS[0].Value == LHS[0].Value ? "Equation has infinetly many solutions" : "Equation has no solutions");

            return RHS[0].GetNumberPart() / LHS[0].GetNumberPart();
        }

        private void SegregateAndFlip(List<Token> toCheck, TokenType typeToCheckFor, out List<Token> numbers, out List<Token> variables, out Token multiplicand)
        {
            multiplicand = LinearEquationLexer.One;
            numbers = new List<Token>() { LinearEquationLexer.Zero };
            variables = new List<Token>() { LinearEquationLexer.Zero };

            for (int i = 0; i < toCheck.Count; i += 2)
            {
                var token = toCheck[i];
                var prevOperator = i == 0 ? TokenType.Addition : toCheck[i - 1].Type;

                if ((prevOperator & TokenType.SecondaryOperator) == prevOperator)
                {
                    if (token.Type == typeToCheckFor)
                    {
                        if (token.Type == TokenType.Constant)
                        {
                            numbers.Add(prevOperator == TokenType.Addition ? LinearEquationLexer.Minus : LinearEquationLexer.Plus);
                            numbers.Add(token);
                        }
                        else
                        {
                            variables.Add(prevOperator == TokenType.Addition ? LinearEquationLexer.Minus : LinearEquationLexer.Plus);
                            variables.Add(token);
                        }
                    }
                    else
                    {
                        if (token.Type == TokenType.Constant)
                        {
                            numbers.Add(prevOperator == TokenType.Addition ? LinearEquationLexer.Plus : LinearEquationLexer.Minus);
                            numbers.Add(token);
                        }
                        else
                        {
                            variables.Add(prevOperator == TokenType.Addition ? LinearEquationLexer.Plus : LinearEquationLexer.Minus);
                            variables.Add(token);
                        }
                    }
                }
                else
                    multiplicand *= token;
            }

        }
    }
}
