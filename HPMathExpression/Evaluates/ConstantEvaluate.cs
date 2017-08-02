using HPTypes;

namespace HPMathExpression
{
    internal class ConstantEvaluate : MathExpression
    {
        public ConstantEvaluate(string number)
        {
            this.number = number;
        }

        private string number;

        public override HPType Evaluate()
        {
            return HPType.CreateType(number);
        }
    }
}