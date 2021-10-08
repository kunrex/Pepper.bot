using System;
using System.Linq;
using System.Collections.Generic;

using KunalsDiscordBot.Core.Modules.MathCommands.Exceptions;

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
        Brackets = 32,
        Operator = Addition | Subtraction | Multiplication | Division,
        PrimaryOperator = Multiplication | Division,
        SecondaryOperator = Addition | Subtraction,
        NonOperator = Variable | Constant | Brackets,
        Any = Operator | NonOperator
    }

    public class Token : IEvaluable
    {
        public string Value { get; private set; }
        public TokenType Type { get; private set; }

        public bool HasSubTokens { get => subTokens != null && subTokens.Any(); }

        private List<Token> subTokens;
        public IList<Token> SubTokens { get => subTokens == null ? null : subTokens.AsReadOnly(); }

        public char this[int index] => Value[index];
        public int Length { get => Value.Length; }

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
                var last = SubTokens.FirstOrDefault(x => x.Type == TokenType.Brackets);
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
                var last = subTokens.Count - 1;

                for (int i = 0; i < subTokens.Count; i++)
                {
                    var subToken = subTokens[i];
                    if ((subToken.Type & TokenType.Operator) == subToken.Type)
                    {
                        tokens.Add(subToken);
                        continue;
                    }

                    var prevOperation = i == 0 ? TokenType.Addition : subTokens[i - 1].Type;
                    var nextOperation = i == subTokens.Count - 1 ? TokenType.Addition : subTokens[i + 1].Type;

                    var simplified = subToken.Simplyfy(variable);

                    if (simplified.HasSubTokens && prevOperation != TokenType.Division && prevOperation != TokenType.Multiplication
                        && nextOperation != TokenType.Division && nextOperation != TokenType.Multiplication)
                        tokens.AddRange(simplified.GetDeepestSubTokens());
                    else
                        tokens.Add(simplified);
                }

                subTokens = tokens;
                last = subTokens.Count - 1;
                int prevCount = subTokens.Count;

                while (subTokens.Count != 1)
                {
                    var firstOperator = subTokens.FirstOrDefault(x => x.Type == TokenType.Division || x.Type == TokenType.Multiplication);
                    if (firstOperator == null)
                        firstOperator = subTokens.First(x => (TokenType.Operator & x.Type) == x.Type);

                    var index = subTokens.IndexOf(firstOperator);
                    var evaluted = new OperationTask(subTokens[index + 1], subTokens[index - 1], firstOperator.Type).Evaluate();

                    subTokens.RemoveRange(index - 1, 3);

                    if (evaluted.HasSubTokens && evaluted.GetDeepestSubTokens().FirstOrDefault(x => x.Type == TokenType.Division) == null)
                        subTokens.InsertRange(index - 1, evaluted.GetDeepestSubTokens());
                    else
                        subTokens.Insert(index - 1, evaluted);

                    if (prevCount == subTokens.Count)
                        break;

                    prevCount = subTokens.Count;
                }

                var final = subTokens.Count == 1 ? subTokens[0] : new Token("()", TokenType.Brackets, subTokens);
                subTokens = null;

                var result = new OperationTask(final, this, TokenType.Multiplication).Evaluate();
                return result;
            }

            return this;
        }

        public string Format() => $"{Value}{(!HasSubTokens ? "" : $" [{string.Join(',', SubTokens.Select(x => x.Format()))}]")}";

        public Token Evaluate() => this;

        public float GetNumberPart()
        {
            if ((Type & TokenType.Operator) == Type || Type == TokenType.Brackets)
                return 0;

            if (Type == TokenType.Constant)
                return float.Parse(Value);
            else
                return Value.Length == 1 ? 1 : float.Parse(Value.Substring(0, Value.Length - 1));
        }

        public List<Token> GetDeepestSubTokens()
        {
            if (!HasSubTokens)
                return new List<Token>() { this };

            if (subTokens.Count == 1)
                return subTokens[0].GetDeepestSubTokens();

            return subTokens;
        }

        public static Token operator +(Token a, Token b)
        {
            if (a.HasSubTokens || b.HasSubTokens)
                return TryFractionalOperators(a, b, TokenType.Addition);

            if (a.Type != b.Type)
            {
                if (a.GetNumberPart() == 0)
                    return b;
                if (b.GetNumberPart() == 0)
                    return a;

                return new Token("()", TokenType.Brackets, new List<Token>() { a, LinearEquationLexer.Plus, b });
            }
            else if (a.Type == TokenType.Constant)
                return new Token($"{a.GetNumberPart() + b.GetNumberPart()}", TokenType.Constant, null);
            else
                return new Token($"{a.GetNumberPart() + b.GetNumberPart()}{a[a.Length - 1]}", TokenType.Variable, null);
        }

        public static Token operator -(Token a, Token b)
        {
            if (a.HasSubTokens || b.HasSubTokens)
                return TryFractionalOperators(a, b, TokenType.Subtraction);

            if (a.Type != b.Type)
            {
                if (a.GetNumberPart() == 0)
                    return new Token($"{b.GetNumberPart() * -1}{b[b.Length - 1]}", b.Type == TokenType.Variable ? TokenType.Variable : TokenType.Constant, null);
                if (b.GetNumberPart() == 0)
                    return a;

                return new Token("()", TokenType.Brackets, new List<Token>() { a, LinearEquationLexer.Minus, b });
            }
            else if (a.Type == TokenType.Constant)
                return new Token($"{a.GetNumberPart() - b.GetNumberPart()}", TokenType.Constant, null);
            else
                return new Token($"{a.GetNumberPart() - b.GetNumberPart()}{a[a.Length - 1]}", TokenType.Variable, null);
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

                if (a.Type == TokenType.Variable && b.Type == TokenType.Variable)
                    throw new EvaluationException("Powers greater than 1 detected");
                else if (a.Type == TokenType.Variable)
                    value += a[a.Length - 1];
                else if (b.Type == TokenType.Variable)
                    value += b[b.Length - 1];

                return new Token(value, char.IsLetter(value[value.Length - 1]) ? TokenType.Variable : TokenType.Constant, null);
            }
            else
            {
                var complex = a.HasSubTokens ? a : b;
                var simple = complex == a ? b : a;
                var tokens = new List<Token>();

                foreach (var token in complex.subTokens)
                    tokens.Add(simple * token);

                return tokens.Count == 1 ? tokens[0] : new Token("()", TokenType.Brackets, tokens);
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
                {
                    if (b.Type == TokenType.Constant)
                        return new Token($"{value}{a[a.Length - 1]}", TokenType.Variable, null);
                    else
                        return new Token(value, TokenType.Constant, null);
                }
                else
                {
                    if (b.Type == TokenType.Constant)
                        return new Token(value, TokenType.Constant, null);
                    else
                        return new Token("()", TokenType.Brackets, new List<Token>()
                        {
                            new Token(value, TokenType.Constant, null),
                            LinearEquationLexer.Slash,
                            new Token($"{b[b.Length - 1]}", TokenType.Variable, null)
                        });
                }
            }
            else if (a.HasSubTokens && b.HasSubTokens)
            {
                var divsion = a.subTokens.FirstOrDefault(x => x.Type == TokenType.Division);

                if (divsion != null)
                {
                    int index = a.subTokens.IndexOf(divsion);
                    var numerator = a.subTokens[index - 1];
                    var denominator = a.subTokens[index + 1];

                    return numerator * b / denominator;
                }
                else
                {
                    divsion = b.subTokens.FirstOrDefault(x => x.Type == TokenType.Division);

                    if (divsion != null)
                    {
                        int index = b.subTokens.IndexOf(divsion);
                        var numerator = b.subTokens[index - 1];
                        var denominator = b.subTokens[index + 1];

                        return denominator * a / numerator;
                    }

                    return new Token("()", TokenType.Brackets, new List<Token>() { a, LinearEquationLexer.Slash, b });
                }
            }
            else
            {
                var complex = a.HasSubTokens ? a : b;
                var simple = complex == a ? b : a;

                if (complex.subTokens.FirstOrDefault(x => x.Type == TokenType.Division) != null)
                {
                    var index = complex.subTokens.IndexOf(complex.subTokens.FirstOrDefault(x => x.Type == TokenType.Division));

                    var numerator = complex.subTokens[index - 1];
                    var denominator = complex.subTokens[index + 1];

                    return numerator / (simple * denominator);
                }
                else
                {
                    var tokens = new List<Token>();

                    foreach (var token in complex.subTokens)
                    {
                        if ((token.Type & TokenType.Operator) == token.Type)
                        {
                            tokens.Add(token);
                            continue;
                        }

                        tokens.Add(complex == a ? token / simple : simple / token);
                    }

                    return tokens.Count == 1 ? tokens[0] : new Token("()", TokenType.Brackets, tokens);
                }
            }
        }

        private static Token TryFractionalOperators(Token a, Token b, TokenType operatorType)
        {
            List<Token> aTokens = a.GetDeepestSubTokens(), bTokens = b.GetDeepestSubTokens();


            var divisionTokenInA = aTokens.FirstOrDefault(x => x.Type == TokenType.Division);
            var divisionTokenInB = bTokens.FirstOrDefault(x => x.Type == TokenType.Division);

            int divisionAIndex = 0, divsionBIndex = 0;

            Token denominatorA = null, denominatorB = null;
            Token tomuliplyB = null, tomuliplyA = null, numeratorA = null, numeratorB = null;

            if (divisionTokenInA != null)//if there is a division operator (fraction)
            {
                divisionAIndex = aTokens.IndexOf(divisionTokenInA);

                denominatorA = aTokens[divisionAIndex + 1];//get the denominator
                numeratorA = aTokens[divisionAIndex - 1];//get the numerator

                tomuliplyB = new Token(denominatorA.GetNumberPart().ToString(), TokenType.Constant, null);//get the numberpart

                if (denominatorA.Type == TokenType.Variable)//if a is a variable
                    tomuliplyB.AddToValue(denominatorA[denominatorA.Length - 1]);//add variable
            }
            else
            {
                tomuliplyB = LinearEquationLexer.One;
                numeratorA = a;
            }

            if (divisionTokenInB != null)//if there is division operator (fraction)
            {
                divsionBIndex = bTokens.IndexOf(divisionTokenInB);

                denominatorB = bTokens[divsionBIndex + 1];//get the denominator
                numeratorB = bTokens[divsionBIndex - 1];//get the numerator

                tomuliplyA = new Token(denominatorB.GetNumberPart().ToString(), TokenType.Constant, null);//get the numberpart

                if (denominatorB.Type == TokenType.Variable)//if b is a variable 
                    tomuliplyA.AddToValue(denominatorB[denominatorB.Length - 1]);
            }
            else
            {
                tomuliplyA = LinearEquationLexer.One;
                numeratorB = b;
            }

            var denominator = tomuliplyA * tomuliplyB;
            var numerator = operatorType == TokenType.Addition ? tomuliplyA * numeratorA + tomuliplyB * numeratorB
                : tomuliplyA * numeratorA - tomuliplyB * numeratorB;

            return new Token("()", TokenType.Brackets, new List<Token>()
            {
                numerator,
                LinearEquationLexer.Slash,
                denominator
            });
        }
    }
}
