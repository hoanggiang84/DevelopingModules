using System.Collections.Generic;
using System.Linq;
using HPTypes;
using HPVariableRepository;
using MacroVariableDB;
using LoadIdentifierInterface;

namespace HPMathExpression
{
    internal class IdentifierEvaluate : MathExpression
    {
        public IdentifierEvaluate(string identifierExpr, 
            List<IEvaluate<HPType>> argEvals, 
            VariableRepository variables)
        {
            Name = identifierExpr;
            this.argEvals = argEvals;
            evaluate_variables = variables;
        }

        private string Name;
        private List<IEvaluate<HPType>> argEvals;

        public override HPType Evaluate()
        {
            if(argEvals == null || argEvals.Count == 0)
                return evaluate_variables.LoadVariable(Name);

            var args = argEvals.Select(eval => eval.Evaluate()).ToList();

            return BuiltInFunctions.LoadReturnValueMacroFunction(Name, args);
        }
    }
}