using HPMacroTask;
using HPTypes;
using HPVariableRepository;
using MacroPLC;
using NUnit.Framework;
using System;

namespace MacroPLCTest
{
    public class GCodeStatementTest:StatementTest
{
        [Test]
        public void GCodeStatement_WithNumberParameters()
        {
            var tokens = GetTokens("G01 X1 Y10 Z14 F11;");
            new GCodeStatement(tokens, varDB).Step();
        }

        [Test]
        public void GCodeStatement_InvalidGCode()
        {
            var tokens = GetTokens("G92.15");
            try
            {
                new GCodeStatement(tokens, varDB).Step();
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message.Contains("Invalid")
                    && ex.Message.Contains("G92.15"));
            }
        }

        [Test]
        public void GCodeStatement_WithVariableParameters()
        {
            varDB.GCodeGenerated += VarDbOnGCodeGenerated;
            varDB.SetVariable("#12", HPType.CreateType(10));
            varDB.SetVariable("@15", HPType.CreateType(11));
            var tokens = GetTokens("G01 X1 Y345.34 Z(#12 + 1) F(@15);");
            var statement = new GCodeStatement(tokens,varDB);
            statement.Step();
            Assert.IsNotEmpty(gcode_statement);
        }

        private string gcode_statement;
        private void VarDbOnGCodeGenerated(object sender, GCodeStatementArg gCodeStatementArg)
        {
            gcode_statement = gCodeStatementArg.Statement;
        }

        [Test]
        public void GCodeStatement_WithNestedParantheses()
        {
            varDB.SetVariable("#12", HPType.CreateType(10));
            varDB.SetVariable("@15", HPType.CreateType(11));
            var tokens = GetTokens("G01 X1 Y345.34 Z(#12+(2*2)) F(@15);");
            var statement = MacroStatement.CreateStatement(TaskType.GCODE, tokens, varDB);
            statement.Step();
        }

        [Test]
        public void GCodeExtensionStatement_withoutArguments()
        {
            var tokens = GetTokens("M100;");
            var statement = new GCodeStatement(tokens, varDB);
            statement.Step();

            // Code in M100 file: 
            //   @1 = 10;
            //WHILE (@1 < 12)
	        //  @1 = @1 + 1;
            //ENDWHILE;
            Assert.AreEqual("12",varDB.LoadVariable("@1").Literal);
        }

        [Test]
        public void GCodeExtensionStatement_withArguments()
        {
            varDB.SetVariable("#12", HPType.CreateType(10));
            varDB.SetVariable("@15", HPType.CreateType(11));
            var tokens = GetTokens("M100 A1 B345.34 C(#12+(2*2)) D(@15);");
            var statement = new GCodeStatement(tokens, varDB);
            statement.Step();

            // Code in M100 file: 
            //   @1 = 10;
            //WHILE (@1 < 12)
            //  @1 = @1 + 1;
            //ENDWHILE;
            // @1 = #1 + #2;
            Assert.AreEqual("27", varDB.LoadVariable("@1").Literal);
        }
    }
}