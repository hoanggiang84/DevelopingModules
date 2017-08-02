using HPTypes;
using HPVariableRepository;
using LoadIdentifierInterface;

namespace HPMathExpression
{
    internal class VariableIndexer : MathExpression
    {
        private string varName;
        private IEvaluate<HPType> index;

        public VariableIndexer(string varName, IEvaluate<HPType> index, 
            VariableRepository variables)
        {
            this.varName = varName;
            this.index = index;
            evaluate_variables = variables;
        }

        public override HPType Evaluate()
        {
            var indexString = index.Evaluate();
            var variableString = varName + indexString.Literal;
            return evaluate_variables.LoadVariable(variableString);
        }
    }
}