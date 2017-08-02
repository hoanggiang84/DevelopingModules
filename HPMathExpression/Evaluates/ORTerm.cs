using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using HPTypes;
using LoadIdentifierInterface;

namespace HPMathExpression
{
    internal class ORTerm : MathExpression
    {
        public ORTerm(List<IEvaluate<HPType>> termEvals)
        {
            _term_evals = termEvals;
        }

        private List<IEvaluate<HPType>> _term_evals;

        public override HPType Evaluate()
        {
            var first_val = _term_evals[0].Evaluate();
            if (_term_evals.Count == 1)
                return first_val;

            switch (first_val.Type)
            {
                case VariableType.INT:
                    return OR_integer_evals();

                case VariableType.BOOL:
                    return OR_bool_evals();

                default:    // Float
                    throw new Exception(string.Format("Cannot apply operator '|' to operands of '{0}'",
                                                      first_val.Type));
            }
        }

        private HPType OR_bool_evals()
        {
            try
            {
                var bool_vals = new List<bool>();
                foreach (var eval in _term_evals)
                {
                    var eval_value = eval.Evaluate();
                    if (eval_value.Type != VariableType.BOOL)
                        throw new Exception(string.Format("Cannot apply operator '|' to operands of '{0}' and '{1}'",
                                                          VariableType.BOOL, eval_value.Type));
                    bool_vals.Add(bool.Parse(eval_value.Literal));
                }
                var result = bool_vals.Aggregate(false, (current, value) => current || value);
                return HPType.CreateType(result.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid OR Operator", ex);
            }
        }

        private HPType OR_integer_evals()
        {
            try
            {
                var int_vals = new List<int>();
                foreach (var eval in _term_evals)
                {
                    var eval_value = eval.Evaluate();
                    if(eval_value.Type != VariableType.INT)
                        throw new Exception(string.Format("Cannot apply operator '|' to operands of '{0}' and '{1}'",
                                                          VariableType.INT, eval_value.Type));
                    int_vals.Add(int.Parse(eval_value.Literal));
                }
                var result = int_vals.Aggregate(0, (current, value) => current | value);
                return HPType.CreateType(result.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid OR Operator", ex);
            }
        }
    }
}