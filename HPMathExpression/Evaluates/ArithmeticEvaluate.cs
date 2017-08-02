using System;
using System.Collections.Generic;
using HPTypes;
using LoadIdentifierInterface;

namespace HPMathExpression
{
    internal class ArithmeticEvaluate : MathExpression
    {
        public ArithmeticEvaluate(List<IEvaluate<HPType>> addEvals, List<string> addOpers)
        {
            this.addEvals = addEvals;
            this.addOpers = addOpers;
        }

        private List<IEvaluate<HPType>> addEvals;
        private List<string> addOpers;

        public override HPType Evaluate()
        {
            var arithExpr = addEvals[0].Evaluate();
            if (addEvals.Count == 1)
                return arithExpr;

            switch (arithExpr.Type)
            {
                case VariableType.FLOAT:
                case VariableType.INT:
                    return AddSubtractEval();
                default:
                    throw new Exception(string.Format("Cannot apply operator '+' or '-' to operands of '{0}' ",
                                                      arithExpr.Type));
            }
        }

        private HPType AddSubtractEval()
        {
            // Get all values as float
            var isFloatOperator = false;
            var values = new List<float>();
            foreach (var add in addEvals)
            {
                var eval = add.Evaluate();
                if(eval.Type == VariableType.BOOL)
                    throw new Exception(string.Format("Cannot apply operator '+' or '-' to operands of '{0}' ",
                                  VariableType.BOOL));
                if (eval.Type == VariableType.FLOAT)
                    isFloatOperator = true;
                values.Add(float.Parse(eval.Literal));
            }

            // Add and subtract as float
            var index = 0;
            var result = values[index++];
            foreach (var op in addOpers)
            {
                var nextArg = values[index++];
                switch (op)
                {
                    case "+":
                        result += nextArg;
                        break;

                    case "-":
                        result -= nextArg;
                        break;
                }
            }

            // If there is no float HPType evaluate, return as int HPType
            return isFloatOperator ? HPType.CreateType(result) : HPType.CreateType((int) result);
        }
    }
}