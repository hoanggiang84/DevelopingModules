using MacroLexScn;
using MacroPLC;
using NUnit.Framework;
using System.Collections.Generic;

namespace MacroPLCTest
{
    public class IfStatementTest:StatementTest
    {
        private int lineNumber = -1;

        [Test]
        public void TestIfTrue_ExecuteAssignment()
        {
            //var src_code =  "IF (2<4) \r\n" +
            //                       "WAIT(); \r\n" +
            //                       "ENDIF; ";
            //var src_mgr = new SourceManager(src_code);
            //var line_num = 0;
            //var line_content = src_mgr.GetNextLine(out line_num);
            //var line_list = new List<List<Token>>();
            //while (line_content != null)
            //{
            //    var token_mgr = new TokenManager(line_content);
            //    var tokens = new List<Token>();
            //    while (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
            //        tokens.Add(token_mgr.IgnoreWhiteGetNextToken());

            //    line_list.Add(tokens);
            //    line_content = src_mgr.GetNextLine(out line_num);
            //}

            //var if_statement = new IfStatement(line_list, varDB);
            //if_statement.StepNotify += StepNotify;
            //if_statement.Step();
            //Assert.AreEqual(0, lineNumber);
            //if_statement.Step();
            //Assert.AreEqual(1, lineNumber);
            //if_statement.Step();
            //Assert.AreEqual(2, lineNumber);
            //AssertVariableValue("10", "@1");
        }

        private void StepNotify(object sender, StatementArg statementArg)
        {
            lineNumber = statementArg.LineNumber;
        }
    }
}