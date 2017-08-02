using HPTypes;
using LoadIdentifierInterface;

namespace HPMathExpression
{
    internal class SignedFactorEvaluate : MathExpression
    {
        public SignedFactorEvaluate(IEvaluate<HPType> factor, string addOp)
        {
            Factor = factor;
            AddOp = addOp;
        }

        private readonly IEvaluate<HPType> Factor;
        private readonly string AddOp = string.Empty;
        public override HPType Evaluate()
        {
            var eval = Factor.Evaluate();

            if (AddOp == "-")
            {
                var newLiteral = "-" + eval.Literal;
                eval.SetLiteral(newLiteral);
                return eval;
            }
            return Factor.Evaluate();
        }
    }
}