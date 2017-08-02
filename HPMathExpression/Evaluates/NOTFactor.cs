using System;
using System.Globalization;
using HPTypes;
using LoadIdentifierInterface;

namespace HPMathExpression
{
    internal class NOTFactor : MathExpression
    {
        public NOTFactor(IEvaluate<HPType> boolFactor, string notSign)
        {
            this.boolFactor = boolFactor;
            this.notSign = notSign;
        }

        private IEvaluate<HPType> boolFactor;
        private string notSign;

        public override HPType Evaluate()
        {
            var factorLiteral = boolFactor.Evaluate();
            if (string.IsNullOrEmpty(notSign))
                return factorLiteral;

            switch (factorLiteral.Type)
            {
                case VariableType.INT:
                    var intVal = int.Parse(factorLiteral.Literal);
                    return HPType.CreateType((~intVal).ToString(CultureInfo.InvariantCulture));

                case VariableType.BOOL:
                    var boolVal = bool.Parse(factorLiteral.Literal);
                    return HPType.CreateType((!boolVal).ToString(CultureInfo.InvariantCulture));

                default:
                    throw new Exception(string.Format("Cannot apply operator '!' to operands of '{0}'",
                                  VariableType.FLOAT));
            }
        }
    }
}