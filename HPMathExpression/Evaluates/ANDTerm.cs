using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HPTypes;
using LoadIdentifierInterface;

namespace HPMathExpression
{
    internal class ANDTerm : IEvaluate<HPType>
    {
        public ANDTerm(List<IEvaluate<HPType>> notFactor)
        {
            this.notFactor = notFactor;
        }

        private List<IEvaluate<HPType>> notFactor;

        public HPType Evaluate()
        {
            var firstValue = notFactor[0].Evaluate();
            if (notFactor.Count == 1)
                return firstValue;

            switch (firstValue.Type)
            {
                case VariableType.INT:
                    return ANDInteterEvals();

                case VariableType.BOOL:
                    return ANDBoolEvals();

                default:    // Float
                    throw new Exception(string.Format("Cannot apply operator '&' to operands of '{0}'",
                                                      VariableType.FLOAT));
            }
        }

        private HPType ANDBoolEvals()
        {
            try
            {
                var evalValues = new List<bool>();
                foreach (var eval in notFactor)
                {
                    var evalLiteral = eval.Evaluate();
                    if (evalLiteral.Type != VariableType.BOOL)
                        throw new Exception(string.Format("Cannot apply operator '&' to operands of '{0}' and '{1}'",
                                                          VariableType.BOOL, evalLiteral.Type));
                    evalValues.Add(bool.Parse(evalLiteral.Literal));
                }
                var result = evalValues.Aggregate(true, (current, value) => current && value);
                return HPType.CreateType(result.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid AND Operator", ex);
            }
        }

        private HPType ANDInteterEvals()
        {
            try
            {
                var evalValues = new List<int>();
                foreach (var eval in notFactor)
                {
                    var evalLiteral = eval.Evaluate();
                    if (evalLiteral.Type != VariableType.INT)
                        throw new Exception(string.Format("Cannot apply operator '&' to operands of '{0}' and '{1}'",
                                                          VariableType.INT, evalLiteral.Type));
                    evalValues.Add(int.Parse(evalLiteral.Literal));
                }
                var result = evalValues.Aggregate(-1, (current, value) => current & value);
                return HPType.CreateType(result.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid AND Operator", ex);
            }
        }
    }
}