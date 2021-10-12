using System;
using System.Linq;
using System.Collections.Generic;

using KunalsDiscordBot.Core.Modules.MathCommands.Exceptions;

namespace KunalsDiscordBot.Core.Modules.MathCommands.Evaluation
{
    public static class LinearEquationLexer
    {
        public static readonly Token Plus = new Token("+", TokenType.Addition, null);
        public static readonly Token Minus = new Token("-", TokenType.Subtraction, null);
        public static readonly Token Star = new Token("*", TokenType.Multiplication, null);
        public static readonly Token Slash = new Token("/", TokenType.Division, null);

        public static readonly Token One = new Token("1", TokenType.Constant, null);
        public static readonly Token Zero = new Token("0", TokenType.Constant, null);

        public static Token CreateBrackets(List<Token> tokens) => new Token("()", TokenType.Brackets, tokens);
        public static Token Create1(List<Token> tokens) => new Token("1", TokenType.Constant, tokens);

        public static char GetVariable(string equation)
        {
            var characters = equation.Where(x => char.IsLetter(x)).Distinct().ToList();

            if (!characters.Any() || characters.Count > 1)
                throw new EvaluationException("A maximum and minimum of one character is needed for an equation");

            return characters[0];
        }

        public static List<Token> GetTokens(string equation, char variable)
        {
            var tokens = new List<Token>();
            Token currentToken = null;
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

                        if ((expected & TokenType.Addition) != TokenType.Addition)
                            throw new InvalidCharacterException('+', expected);

                        if (currentToken != null)
                        {
                            tokens.Add(currentToken);
                            currentToken = null;
                        }

                        tokens.Add(Plus);
                        expected = TokenType.NonOperator;
                        break;
                    case '-':
                        if (complexFound)
                        {
                            complex += '-';
                            break;
                        }

                        if ((expected & TokenType.Subtraction) != TokenType.Subtraction)
                            throw new InvalidCharacterException('-', expected);

                        if (currentToken != null)
                        {
                            tokens.Add(currentToken);
                            currentToken = null;
                        }

                        tokens.Add(Minus);
                        expected = TokenType.NonOperator;
                        break;
                    case '/':
                        if (complexFound)
                        {
                            complex += '/';
                            break;
                        }

                        if ((expected & TokenType.Division) != TokenType.Division)
                            throw new InvalidCharacterException('/', expected);

                        if (currentToken != null)
                        {
                            tokens.Add(currentToken);
                            currentToken = null;
                        }

                        tokens.Add(Slash);
                        expected = TokenType.NonOperator;
                        break;
                    case '*':
                        if (complexFound)
                        {
                            complex += '*';
                            break;
                        }

                        if ((expected & TokenType.Multiplication) != TokenType.Multiplication)
                            throw new InvalidCharacterException('*', expected);

                        if (currentToken != null)
                        {
                            tokens.Add(currentToken);
                            currentToken = null;
                        }

                        tokens.Add(Star);
                        expected = TokenType.NonOperator;
                        break;
                    case var x when x == variable:
                        if (complexFound)
                        {
                            complex += x;
                            break;
                        }

                        if ((expected & TokenType.Variable) != TokenType.Variable)
                            throw new InvalidCharacterException(x, expected);

                        if (currentToken == null)
                            currentToken = new Token(variable.ToString(), TokenType.Variable, null);
                        else if (currentToken.Type == TokenType.Constant)
                            currentToken.AddToValue(x);
                        else
                            currentToken.SubTokens.Add(new Token(variable.ToString(), TokenType.Variable, null));

                        expected = TokenType.Operator | TokenType.Brackets;
                        break;
                    case var x when char.IsNumber(x):
                        if (complexFound)
                        {
                            complex += x;
                            break;
                        }

                        if ((expected & TokenType.Constant) != TokenType.Constant)
                            throw new InvalidCharacterException('x', expected);

                        if (currentToken == null)
                            currentToken = new Token(x.ToString(), TokenType.Constant, new List<Token>());
                        else if (currentToken.Type == TokenType.Constant)
                            currentToken.AddToValue(x);

                        expected = TokenType.Any;
                        break;
                    case '.':
                        if (complexFound)
                        {
                            complex += '.';
                            break;
                        }

                        if ((expected & TokenType.Constant) != TokenType.Constant)
                            throw new InvalidCharacterException('.', expected);

                        currentToken.AddToValue('.');

                        expected = TokenType.Constant | TokenType.Variable;
                        break;
                    case '(':
                        if (!complexFound && (expected & TokenType.Brackets) != TokenType.Brackets)
                            throw new InvalidCharacterException('(', expected);

                        if (complexFound)
                        {
                            nested++;
                            complex += '(';
                        }
                        else if (nested == 0)
                        {
                            complexFound = true;
                            nested++;
                        }

                        expected = TokenType.NonOperator;
                        break;
                    case ')':
                        if (!complexFound && (expected & TokenType.Brackets) != TokenType.Brackets)
                            throw new InvalidCharacterException(')', expected);

                        nested--;

                        if (nested == 0)
                        {
                            complexFound = false;
                            var tokensToAdd = GetTokens(complex, variable);

                            if (currentToken == null)
                                currentToken = Create1(tokensToAdd);
                            else
                                foreach (var token in tokensToAdd)
                                    currentToken.AddSubToken(token);

                            complex = string.Empty;
                        }
                        else
                            complex += ')';

                        expected = TokenType.Operator;
                        break;
                    default:
                        throw new InvalidCharacterException(character, expected);
                }
            }

            if (currentToken != null)
                tokens.Add(currentToken);

            return tokens;
        }

        public static List<Token> GetTokens(string equation)
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

                        if ((expected & TokenType.Addition) != TokenType.Addition)
                            throw new InvalidCharacterException('+', expected);

                        if (current != null)
                        {
                            tokens.Add(current);
                            current = null;
                        }

                        tokens.Add(Plus);
                        expected = TokenType.NonOperator;
                        break;
                    case '-':
                        if (complexFound)
                        {
                            complex += '-';
                            break;
                        }

                        if ((expected & TokenType.Subtraction) != TokenType.Subtraction)
                            throw new InvalidCharacterException('-', expected);

                        if (current != null)
                        {
                            tokens.Add(current);
                            current = null;
                        }

                        tokens.Add(Minus);
                        expected = TokenType.NonOperator;
                        break;
                    case '/':
                        if (complexFound)
                        {
                            complex += '/';
                            break;
                        }

                        if ((expected & TokenType.Division) != TokenType.Division)
                            throw new InvalidCharacterException('/', expected);

                        if (current != null)
                        {
                            tokens.Add(current);
                            current = null;
                        }

                        tokens.Add(Slash);
                        expected = TokenType.NonOperator;
                        break;
                    case '*':
                        if (complexFound)
                        {
                            complex += '*';
                            break;
                        }

                        if ((expected & TokenType.Multiplication) != TokenType.Multiplication)
                            throw new InvalidCharacterException('*', expected);

                        if (current != null)
                        {
                            tokens.Add(current);
                            current = null;
                        }

                        tokens.Add(Star);
                        expected = TokenType.NonOperator;
                        break;
                    case var x when char.IsNumber(x):
                        if (complexFound)
                        {
                            complex += x;
                            break;
                        }

                        if ((expected & TokenType.Constant) != TokenType.Constant)
                            throw new InvalidCharacterException('x', expected);

                        if (current == null)
                            current = new Token(x.ToString(), TokenType.Constant, new List<Token>());
                        else if (current.Type == TokenType.Constant)
                            current.AddToValue(x);

                        expected = TokenType.Any;
                        break;
                    case '.':
                        if (complexFound)
                        {
                            complex += '.';
                            break;
                        }

                        if ((expected & TokenType.Constant) != TokenType.Constant)
                            throw new InvalidCharacterException('.', expected);

                        current.AddToValue('.');

                        expected = TokenType.Constant | TokenType.Variable;
                        break;
                    case '(':
                        if (!complexFound && (expected & TokenType.Brackets) != TokenType.Brackets)
                            throw new InvalidCharacterException('(', expected);

                        if (complexFound)
                        {
                            nested++;
                            complex += '(';
                        }
                        else if (nested == 0)
                        {
                            complexFound = true;
                            nested++;
                        }

                        expected = TokenType.NonOperator;
                        break;
                    case ')':
                        if (!complexFound && (expected & TokenType.Brackets) != TokenType.Brackets)
                            throw new InvalidCharacterException(')', expected);

                        nested--;

                        if (nested == 0)
                        {
                            complexFound = false;
                            var tokensToAdd = GetTokens(complex);

                            if (current == null)
                                current = Create1(tokensToAdd);
                            else
                                foreach (var token in tokensToAdd)
                                    current.AddSubToken(token);

                            complex = string.Empty;
                        }
                        else
                            complex += ')';

                        expected = TokenType.Operator;
                        break;
                    default:
                        throw new InvalidCharacterException(character, expected);
                }
            }

            if (current != null)
                tokens.Add(current);

            return tokens;
        }
    }
}
