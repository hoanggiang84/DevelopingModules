using HPTypes;

namespace HPMathExpression
{
    internal class BoolLiteral : MathExpression
    {
        public BoolLiteral(string boolLiteral)
        {
            this.boolLiteral = boolLiteral;
        }

        private string boolLiteral;

        public override HPType Evaluate()
        {
            bool.Parse(boolLiteral);
            return HPType.CreateType(boolLiteral);
        }
    }
}