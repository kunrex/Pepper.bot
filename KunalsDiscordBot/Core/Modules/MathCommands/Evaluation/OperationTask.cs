using System;
using System.Linq;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules.MathCommands.Evaluation
{
    public class OperationTask : IEvaluable
    {
        public IEvaluable RHS, LHS;
        public TokenType Operator;

        public OperationTask(IEvaluable rhs, IEvaluable lhs, TokenType _operator)
        {
            RHS = rhs;
            LHS = lhs;
            Operator = _operator;
        }

        public Token Evaluate()
        {
            var rhs = RHS.Evaluate();
            var lhs = LHS.Evaluate();

            switch (Operator)
            {
                case TokenType.Addition:
                    return lhs + rhs;
                case TokenType.Subtraction:
                    return lhs - rhs;
                case TokenType.Multiplication:
                    return lhs * rhs;
                case TokenType.Division:
                    return lhs / rhs;
            }

            return null;
        }
    }
}
