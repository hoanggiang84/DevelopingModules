using System;
using System.Collections.Generic;
using HPTypes;
using LoadIdentifierInterface;

namespace HPMathExpression
{
    internal class TermEvaluate : MathExpression
    {
        public TermEvaluate(List<IEvaluate<HPType>> mulEvals, List<string> mulOpers)
        {
            this.mulEvals = mulEvals;
            this.mulOpers = mulOpers;
        }

        private readonly List<IEvaluate<HPType>> mulEvals;
        private readonly List<string> mulOpers;

        public override HPType Evaluate()
        {
            var arithExpr = mulEvals[0].Evaluate();
            if (mulEvals.Count == 1)
                return arithExpr;

            switch (arithExpr.Type)
            {
                case VariableType.FLOAT:
                case VariableType.INT:
                    return MulDivEval();
                default:
                    throw new Exception(string.Format("Cannot apply operator '*', '/' or '%' to operands of '{0}' ",
                                                      arithExpr.Type));
            }
        }

        public HPType MulDivEval()
        {
            // Get all values as float
            var isFloatOperator = false;
            var values = new List<float>();
            foreach (var mul in mulEvals)
            {
                var eval = mul.Evaluate();
                if (eval.Type == VariableType.BOOL)
                    throw new Exception(string.Format("Cannot apply operator '*', '/' or '%' to operands of '{0}' ",
                                  VariableType.BOOL));
                if (eval.Type == VariableType.FLOAT)
                    isFloatOperator = true;
                values.Add(float.Parse(eval.Literal));
            }

            // Add and subtract as float
            var index = 0;
            var result = values[index++];
            foreach (var mulOp in mulOpers)
            {
                var nextArg = values[index++] ;
                switch (mulOp)
                {
                    case "*":
                        result *= nextArg;
                        break;

                    case "/":
                        if (nextArg == 0)
                            throw Error("Divide by Zero");
                        result /= nextArg;
                        break;

                    case "%":
                        if (nextArg == 0)
                            throw Error("Divide by Zero");
                        result %= nextArg;
                        break;
                }
            }

            // If there is no float HPType evaluate, return as int HPType
            return isFloatOperator ? HPType.CreateType(result) : HPType.CreateType((int)result);
        }
    }
}