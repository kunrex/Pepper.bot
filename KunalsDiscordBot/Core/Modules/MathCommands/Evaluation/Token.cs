using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KunalsDiscordBot.Core.Modules.MathCommands.Evaluation
{
    [Flags]
    public enum TokenType
    {
        Addition = 0,
        Subtraction = 1,
        Multiplication = 2,
        Division = 4,
        Variable = 8,
        Constant = 16,
        OpenBracket = 32,
        CloseBracket = 64,
        Equal = 128,
        Simplest = Variable,
        Operator = Addition | Subtraction | Multiplication | Division,
        NonOperator = Variable | Constant | OpenBracket | CloseBracket,
        Symbol = OpenBracket | CloseBracket,
        Any = Operator | NonOperator | Equal
    }

    public class Token : IEvaluable
    {
        public static readonly Regex simplestFormRegex = new Regex("([0-9]+[A-Z,a-z]?)");

        public string Value { get; private set; }
        public TokenType Type { get; private set; }
        public bool HasSubTokens { get => subTokens != null && subTokens.Any(); }

        private List<Token> subTokens;
        public IList<Token> SubTokens { get => subTokens == null ? null : subTokens.AsReadOnly(); }

        public Token(string value, TokenType type, List<Token> tokens)
        {
            Value = value;
            Type = type;
            subTokens = tokens;
        }

        public string AddToValue(string toAdd)
        {
            if (Type == TokenType.Constant)
            {
                Value += toAdd;

                if (!float.TryParse(toAdd, out var x))
                    Type = TokenType.Variable;
            }

            return Value;
        }
        public string AddToValue(char toAdd)
        {
            if (Type == TokenType.Constant)
            {
                Value += toAdd;

                if (char.IsLetter(toAdd))
                    Type = TokenType.Variable;
            }

            return Value;
        }

        public void AddSubToken(Token token)
        {
            if (SubTokens == null)
                subTokens = new List<Token>() { token };
            else if (!SubTokens.Any())
                subTokens.Add(token);
            else
            {
                var last = SubTokens.FirstOrDefault(x => x.Type == TokenType.OpenBracket);
                if (last == null)
                    subTokens.Add(token);
                else
                    last.subTokens.Add(token);
            }
        }

        public Token Simplyfy(char variable)
        {
            if (subTokens != null && subTokens.Any())
            {
                var tokens = new List<Token>();
                for (int i = 0; i < subTokens.Count; i++)
                {
                    var subToken = subTokens[i];
                    var operation = i == 0 ? TokenType.Addition : subTokens[i - 1].Type;

                    var simplified = subToken.Simplyfy(variable);

                    if (simplified.HasSubTokens && operation != TokenType.Division && operation != TokenType.Multiplication)
                        tokens.AddRange(simplified.subTokens);
                    else
                        tokens.Add(simplified);
                }

                subTokens = tokens;
                int prevCount = subTokens.Count;

                while (subTokens.Count != 1)
                {
                    var firstOperator = subTokens.FirstOrDefault(x => x.Type == TokenType.Division || x.Type == TokenType.Multiplication);
                    if (firstOperator == null)
                        firstOperator = subTokens.First(x => (TokenType.Operator & x.Type) == x.Type);

                    var index = subTokens.IndexOf(firstOperator);
                    var evaluted = new OperationTask(subTokens[index + 1], subTokens[index - 1], firstOperator.Type).Evaluate();

                    subTokens.RemoveRange(index - 1, 3);

                    if (evaluted.HasSubTokens)
                        subTokens.InsertRange(index - 1, evaluted.subTokens);
                    else
                        subTokens.Insert(index - 1, evaluted);

                    if (prevCount == subTokens.Count)
                        break;
                    prevCount = subTokens.Count;
                }

                var final = subTokens.Count == 1 ? subTokens[0] : new Token("()", TokenType.OpenBracket, subTokens);
                subTokens = null;

                return new OperationTask(final, this, TokenType.Multiplication).Evaluate();
            }

            return this;
        }

        public string Format() => $"{Value}{(SubTokens == null || !SubTokens.Any() ? "" : $" [{string.Join(',', SubTokens.Select(x => x.Format()))}]")}";

        public Token Evaluate() => this;

        public float GetNumberPart()
        {
            if ((Type & TokenType.Operator) == Type || (Type & TokenType.Symbol) == Type)
                return 0;

            if (Type == TokenType.Constant)
                return float.Parse(Value);
            else
                return Value.Length == 1 ? 1 : float.Parse(Value.Substring(0, Value.Length - 1));
        }

        public static Token operator +(Token a, Token b)
        {
            if (a.Type != b.Type)
                return new Token("()", TokenType.OpenBracket, new List<Token>() { a, new Token("+", TokenType.Addition, null), b });
            else if (a.Type == TokenType.Constant)
                return new Token($"{a.GetNumberPart() + b.GetNumberPart()}", TokenType.Constant, null);
            else
                return new Token($"{a.GetNumberPart() + b.GetNumberPart()}{a.Value[a.Value.Length - 1]}", TokenType.Variable, null);
        }

        public static Token operator -(Token a, Token b)
        {
            if (a.Type != b.Type)
                return new Token("()", TokenType.OpenBracket, new List<Token>() { a, new Token("-", TokenType.Addition, null), b });
            else if (a.Type == TokenType.Constant)
                return new Token($"{a.GetNumberPart() - b.GetNumberPart()}", TokenType.Constant, null);
            else
                return new Token($"{a.GetNumberPart() - b.GetNumberPart()}{a.Value[a.Value.Length - 1]}", TokenType.Variable, null);
        }

        public static Token operator *(Token a, Token b)
        {
            if ((a.Type & TokenType.Operator) == a.Type)
                return a;
            else if ((b.Type & TokenType.Operator) == b.Type)
                return b;

            if (!a.HasSubTokens && !b.HasSubTokens)
            {
                var value = $"{a.GetNumberPart() * b.GetNumberPart()}";

                if (a.Type == TokenType.Variable)
                    value += a.Value[a.Value.Length - 1];
                else if (b.Type == TokenType.Variable)
                    value += b.Value[b.Value.Length - 1];

                return new Token(value, char.IsLetter(value[value.Length - 1]) ? TokenType.Variable : TokenType.Constant, null);
            }
            else
            {
                var complex = a.HasSubTokens ? a : b;
                var simple = complex == a ? b : a;
                var tokens = new List<Token>();

                foreach (var token in complex.subTokens)
                    tokens.Add(simple * token);

                return tokens.Count == 1 ? tokens[0] : new Token("()", TokenType.OpenBracket, tokens);
            }
        }

        public static Token operator /(Token a, Token b)
        {
            if ((a.Type & TokenType.Operator) == a.Type)
                return a;
            else if ((b.Type & TokenType.Operator) == b.Type)
                return b;

            if (!a.HasSubTokens && !b.HasSubTokens)
            {
                var value = $"{a.GetNumberPart() / b.GetNumberPart()}";

                if (a.Type == TokenType.Variable)
                    value += a.Value[a.Value.Length - 1];
                else if (b.Type == TokenType.Variable)
                    value += b.Value[b.Value.Length - 1];

                return new Token(value, char.IsLetter(value[value.Length - 1]) ? TokenType.Variable : TokenType.Constant, null);
            }
            else
            {
                var complex = a.HasSubTokens ? a : b;
                var simple = complex == a ? b : a;
                var tokens = new List<Token>();

                foreach (var token in complex.subTokens)
                    tokens.Add(simple / token);

                return tokens.Count == 1 ? tokens[0] : new Token("()", TokenType.OpenBracket, tokens);
            }
        }
    }
}
