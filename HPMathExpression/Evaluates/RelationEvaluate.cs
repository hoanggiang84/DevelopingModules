using System;
using System.Collections.Generic;
using HPTypes;
using LoadIdentifierInterface;

namespace HPMathExpression
{
    internal class RelationEvaluate : MathExpression
    {
        public RelationEvaluate(IEvaluate<HPType> firstArg, IEvaluate<HPType> secondArg, string relOper)
        {
            this.firstArg = firstArg;
            this.secondArg = secondArg;
            this.relOper = relOper;
        }

        private IEvaluate<HPType> firstArg;
        private IEvaluate<HPType> secondArg;
        private string relOper;

        public override HPType Evaluate()
        {
            var firstEval = firstArg.Evaluate();
            var secondEval = secondArg.Evaluate();

            if ((firstEval.Type == VariableType.BOOL && secondEval.Type != VariableType.BOOL
                || firstEval.Type != VariableType.BOOL && secondEval.Type == VariableType.BOOL))
                throw new Exception(string.Format("Cannot apply operator '{3}' to operands of '{0}' and '{1}' or '{2}'",
                                                  VariableType.BOOL, VariableType.INT, VariableType.FLOAT, relOper));
            return RelationOperation[relOper].Invoke(firstEval, secondEval);
        }

        protected static Dictionary<string, Func<HPType, HPType, HPType>> RelationOperation =
            new Dictionary<string, Func<HPType, HPType, HPType>>
                {
                    {"=", Equal},
                    {"<>", NotEqual},
                    {"<", Less},
                    {"<=", LessEqual},
                    {">", Greater},
                    {">=", GreaterEqual}
                };

        #region Compare Operations
        private static HPType GreaterEqual(HPType firstEval, HPType secondEval)
        {
            var result = false;
            if ((firstEval.Type == VariableType.BOOL || secondEval.Type == VariableType.BOOL))
                throw new Exception(string.Format("Cannot apply operator '{1}' to operands of '{0}'",
                                  VariableType.BOOL, ">="));

            if (firstEval.Type == VariableType.FLOAT
                || secondEval.Type == VariableType.FLOAT)
            {
                result = (float.Parse(firstEval.Literal) >= float.Parse(secondEval.Literal));
                return HPType.CreateType(result.ToString());
            }

            result = (int.Parse(firstEval.Literal) >= int.Parse(secondEval.Literal));
            return HPType.CreateType(result.ToString());
        }

        private static HPType Greater(HPType firstEval, HPType secondEval)
        {
            var result = false;
            if ((firstEval.Type == VariableType.BOOL || secondEval.Type == VariableType.BOOL))
                throw new Exception(string.Format("Cannot apply operator '{1}' to operands of '{0}'",
                                  VariableType.BOOL, ">"));

            if (firstEval.Type == VariableType.FLOAT
                || secondEval.Type == VariableType.FLOAT)
            {
                result = (float.Parse(firstEval.Literal) > float.Parse(secondEval.Literal));
                return HPType.CreateType(result.ToString());
            }

            result = (int.Parse(firstEval.Literal) > int.Parse(secondEval.Literal));
            return HPType.CreateType(result.ToString());
        }

        private static HPType LessEqual(HPType firstEval, HPType secondEval)
        {
            var result = false;
            if ((firstEval.Type == VariableType.BOOL || secondEval.Type == VariableType.BOOL))
                throw new Exception(string.Format("Cannot apply operator '{1}' to operands of '{0}'",
                                  VariableType.BOOL, "<="));

            if (firstEval.Type == VariableType.FLOAT
                || secondEval.Type == VariableType.FLOAT)
            {
                result = (float.Parse(firstEval.Literal) <= float.Parse(secondEval.Literal));
                return HPType.CreateType(result.ToString());
            }

            result = (int.Parse(firstEval.Literal) <= int.Parse(secondEval.Literal));
            return HPType.CreateType(result.ToString());
        }
        
        private static HPType Less(HPType firstEval, HPType secondEval)
        {
            var result = false;
            if ((firstEval.Type == VariableType.BOOL || secondEval.Type == VariableType.BOOL))
                throw new Exception(string.Format("Cannot apply operator '{1}' to operands of '{0}'",
                                  VariableType.BOOL, "<"));

            if (firstEval.Type == VariableType.FLOAT
                || secondEval.Type == VariableType.FLOAT)
            {
                result = (float.Parse(firstEval.Literal) < float.Parse(secondEval.Literal));
                return HPType.CreateType(result.ToString());
            }

            result = (int.Parse(firstEval.Literal) < int.Parse(secondEval.Literal));
            return HPType.CreateType(result.ToString());
        }

        private static HPType NotEqual(HPType firstEval, HPType secondEval)
        {
            var result = false;
            if ((firstEval.Type == VariableType.BOOL && secondEval.Type == VariableType.BOOL))
            {
                result = (bool.Parse(firstEval.Literal) != bool.Parse(secondEval.Literal));
                return HPType.CreateType(result.ToString());
            }

            if (firstEval.Type == VariableType.FLOAT
                || secondEval.Type == VariableType.FLOAT)
            {
                result = (float.Parse(firstEval.Literal) != float.Parse(secondEval.Literal));
                return HPType.CreateType(result.ToString());
            }

            result = (int.Parse(firstEval.Literal) != int.Parse(secondEval.Literal));
            return HPType.CreateType(result.ToString());
        }

        private static HPType Equal(HPType firstEval, HPType secondEval)
        {
            var result = false;
            if ((firstEval.Type == VariableType.BOOL && secondEval.Type == VariableType.BOOL))
            {
                result = (bool.Parse(firstEval.Literal) == bool.Parse(secondEval.Literal));
                return HPType.CreateType(result.ToString());
            }

            if (firstEval.Type == VariableType.FLOAT
                || secondEval.Type == VariableType.FLOAT)
            {
                result = (float.Parse(firstEval.Literal) == float.Parse(secondEval.Literal));
                return HPType.CreateType(result.ToString());
            }

            result = (int.Parse(firstEval.Literal) == int.Parse(secondEval.Literal));
            return HPType.CreateType(result.ToString());
        }
        #endregion
    }
}