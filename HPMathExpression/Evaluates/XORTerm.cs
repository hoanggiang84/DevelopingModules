using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HPTypes;
using LoadIdentifierInterface;

namespace HPMathExpression
{
    internal class XORTerm : MathExpression
    {
        public XORTerm(List<IEvaluate<HPType>> termEvals)
        {
            this.termEvals = termEvals;
        }

        private List<IEvaluate<HPType>> termEvals;

        public override HPType Evaluate()
        {
            var firstValue = termEvals[0].Evaluate();
            if (termEvals.Count == 1)
                return firstValue;

            switch (firstValue.Type)
            {
                case VariableType.INT:
                    return XORInteterEvals();

                case VariableType.BOOL:
                    return XORBoolEvals();

                default:    // Float
                    throw new Exception(string.Format("Cannot apply operator '^' to operands of '{0}'",
                                                      VariableType.FLOAT));
            }
        }

        private HPType XORBoolEvals()
        {
            try
            {
                var evalValues = new List<bool>();
                foreach (var eval in termEvals)
                {
                    var evalLiteral = eval.Evaluate();
                    if (evalLiteral.Type != VariableType.BOOL)
                        throw new Exception(string.Format("Cannot apply operator '^' to operands of '{0}' and '{1}'",
                                                          VariableType.BOOL, evalLiteral.Type));
                    evalValues.Add(bool.Parse(evalLiteral.Literal));
                }
                var result = evalValues.Aggregate(false, (current, value) => current ^ value);
                return HPType.CreateType(result.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid XOR Operator", ex);
            }
        }

        private HPType XORInteterEvals()
        {
            try
            {
                var evalValues = new List<int>();
                foreach (var eval in termEvals)
                {
                    var evalLiteral = eval.Evaluate();
                    if (evalLiteral.Type != VariableType.INT)
                        throw new Exception(string.Format("Cannot apply operator '^' to operands of '{0}' and '{1}'",
                                                          VariableType.INT, evalLiteral.Type));
                    evalValues.Add(int.Parse(evalLiteral.Literal));
                }
                var result = evalValues.Aggregate(0, (current, value) => current ^ value);
                return HPType.CreateType(result.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid XOR Operator", ex);
            }
        }
    }
}