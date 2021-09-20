using System;
using System.Linq;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules.MathCommands.Evaluation
{
    public static class LinearEquationLexer
    {
        public static char GetVariable(string equation)
        {
            var characters = equation.Where(x => char.IsLetter(x)).Distinct().ToList();

            if (!characters.Any() || characters.Count > 1)
                throw new InvalidOperationException();

            return characters[0];
        }

        public static List<Token> GetTokens(string equation, char variable)
        {
            var tokens = new List<Token>();
            Token current = null;
            TokenType expected = TokenType.NonOperator;
            string complex = string.Empty; int nested = 0; bool complexFound = false;

            for (int i = 0; i < equation.Length; i++)
            {
                var character = equation[i];

                switch (character)
                {
                    case var x when char.IsWhiteSpace(x):
                        break;
                    case '+':
                        if (complexFound)
                        {
                            complex += '+';
                            break;
                        }
                        if (current != null)
                        {
                            tokens.Add(current);
                            current = null;
                        }

                        tokens.Add(new Token("+", TokenType.Addition, null));
                        expected = TokenType.NonOperator;
                        break;
                    case '-':
                        if (complexFound)
                        {
                            complex += '-';
                            break;
                        }
                        if (current != null)
                        {
                            tokens.Add(current);
                            current = null;
                        }

                        tokens.Add(new Token("-", TokenType.Subtraction, null));
                        expected = TokenType.NonOperator;
                        break;
                    case '/':
                        if (complexFound)
                        {
                            complex += '/';
                            break;
                        }
                        if (current != null)
                        {
                            tokens.Add(current);
                            current = null;
                        }

                        tokens.Add(new Token("/", TokenType.Division, null));
                        expected = TokenType.NonOperator;
                        break;
                    case '*':
                        if (complexFound)
                        {
                            complex += '*';
                            break;
                        }
                        if (current != null)
                        {
                            tokens.Add(current);
                            current = null;
                        }

                        tokens.Add(new Token("*", TokenType.Multiplication, null));
                        expected = TokenType.NonOperator;
                        break;
                    case var x when x == variable:
                        if (complexFound)
                        {
                            complex += x;
                            break;
                        }
                        var newToken = new Token(variable.ToString(), TokenType.Variable, null);

                        if (current == null)
                            current = newToken;
                        else if (current.Type == TokenType.Constant)
                            current.AddToValue(x);
                        else
                            current.SubTokens.Add(newToken);
                        break;
                    case var x when char.IsNumber(x):
                        if (complexFound)
                        {
                            complex += x;
                            break;
                        }
                        newToken = new Token(x.ToString(), TokenType.Constant, new List<Token>());

                        if (current == null)
                            current = newToken;
                        else if (current.Type == TokenType.Constant)
                            current.AddToValue(x);
                        break;
                    case '.':
                        if (complexFound)
                        {
                            complex += '.';
                            break;
                        }

                        current.AddToValue('.');
                        break;
                    case '(':
                        if (complexFound)
                        {
                            nested++;
                            complex += '(';
                        }
                        else
                        {
                            if (nested == 0)
                            {
                                complexFound = true;
                                nested++;
                            }
                        }

                        break;
                    case ')':
                        if (complexFound)
                            nested--;

                        if (nested == 0)
                        {
                            complexFound = false;
                            var tokensToAdd = GetTokens(complex, variable);

                            if (current == null)
                                current = new Token("1", TokenType.Constant, tokensToAdd);
                            else
                                foreach (var token in tokensToAdd)
                                    current.AddSubToken(token);
                            complex = string.Empty;
                        }
                        else
                            complex += ')';
                        break;
                }
            }

            if (current != null)
                tokens.Add(current);

            return tokens;
        }
    }
}
