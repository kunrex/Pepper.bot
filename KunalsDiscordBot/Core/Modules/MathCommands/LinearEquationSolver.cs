using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules.MathCommands.Evaluation
{ 
    public sealed class LinearEquationSolver
    {
        public char Variable { get; private set; }

        private readonly string equation;
        public string Equation { get => equation; }

        private List<Token> RHS { get; set; }
        private List<Token> LHS { get; set; }

        public LinearEquationSolver(string _equation)
        {
            equation = _equation;

            Variable = LinearEquationLexer.GetVariable(equation);
            var sides = equation.Split('=');

            RHS = LinearEquationLexer.GetTokens(sides[1], Variable);
            LHS = LinearEquationLexer.GetTokens(sides[0], Variable);
        }

        public Task<string> Solve()
        {
            Simplify();
            Transfer();

            if ((RHS[0].Type & TokenType.Operator) == RHS[0].Type)
                RHS.Insert(0, new Token("0", TokenType.Constant, null));
            if ((LHS[0].Type & TokenType.Operator) == LHS[0].Type)
                LHS.Insert(0, new Token("0x", TokenType.Variable, null));

            var rhs = new Token("1", TokenType.Constant, RHS).Simplyfy(Variable);
            var lhs = new Token("1", TokenType.Constant, LHS).Simplyfy(Variable);

            var rhsNumber = rhs.GetNumberPart();
            var lhsNumber = lhs.GetNumberPart();

            return Task.FromResult($"{Variable} is {rhsNumber / lhsNumber}");
        }

        private void Simplify()
        {
            var newTokens = new List<Token>();
            for (int i = 0; i < RHS.Count; i++)
                newTokens.Add(RHS[i].Simplyfy(Variable));

            RHS = new List<Token>();
            foreach (var token in newTokens)
                if (token.HasSubTokens)
                    RHS.AddRange(token.SubTokens);
                else
                    RHS.Add(token);

            newTokens = new List<Token>();
            for (int i = 0; i < LHS.Count; i++)
                newTokens.Add(LHS[i].Simplyfy(Variable));

            LHS = new List<Token>();
            foreach (var token in newTokens)
                if (token.HasSubTokens)
                    LHS.AddRange(token.SubTokens);
                else
                    LHS.Add(token);
        }

        private void Transfer()
        {
            for (int i = 0; i < RHS.Count; i++)
            {
                var token = RHS[i];

                if (token.Type == TokenType.Variable)//wrong side
                {
                    Token operation = null;
                    if (i == 0)
                        operation = new Token("-", TokenType.Subtraction, null);
                    else
                    {
                        operation = RHS[i - 1].Type == TokenType.Addition ? new Token("-", TokenType.Subtraction, null) : new Token("+", TokenType.Addition, null);
                        RHS.RemoveAt(i - 1);
                        i--;
                    }

                    RHS.Remove(token);
                    i--;

                    LHS.Add(operation);
                    LHS.Add(token);
                }
            }

            for (int i = 0; i < LHS.Count; i++)
            {
                var token = LHS[i];

                if (token.Type == TokenType.Constant)//wrong side
                {
                    Token operation = null;
                    if (i == 0)
                        operation = new Token("-", TokenType.Subtraction, null);
                    else
                    {
                        operation = LHS[i - 1].Type == TokenType.Addition ? new Token("-", TokenType.Subtraction, null) : new Token("+", TokenType.Addition, null);
                        LHS.RemoveAt(i - 1);
                        i--;
                    }
                    LHS.Remove(token);
                    i--;

                    RHS.Add(operation);
                    RHS.Add(token);
                }
            }
        }
    }
}
