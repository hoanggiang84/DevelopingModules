using MacroPLC;
using NUnit.Framework;

namespace MacroPLCTest
{
    public class IfStatementTest:StatementTest
    {
        private int lineNumber = -1;

        [Test]
        public void TestIfTrue_ExecuteAssignment()
        {
            var src_code = "IF (2<4) \r\n" +
                                   "WAIT(); \r\n" +
                                   "ENDIF; ";
            var compiler = new MacroCompiler(src_code);
            compiler.Compile();
            var executor = new MacroExecutor(compiler.compiledTasks);
            executor.NotifyStep += StepNotify;

            var line_num = executor.StepExecute();
            while (line_num != MacroExecutor.INVALID_LINE_NUMBER)
            {
                line_num = executor.StepExecute();
            }
        }

        private void StepNotify(object sender, StepExecuteArg statementArg)
        {
            lineNumber = statementArg.LineNumber;
        }
    }
}