using MacroVariableDB;
using NUnit.Framework;
using HPCompiler;
using HPTypes;
using System;

namespace UnitTestMacroCompiler
{
    [TestFixture]
    public class UnitTestMacroCompiler
    {
        private string GeneratedGcode = string.Empty;

        [SetUp]
        public void Setup()
        {
            VariableDB.InitializeVariables();
            VariableDB.GCodeGenerated += VariableDbOnGCodeGenerated;
        }

        private void VariableDbOnGCodeGenerated(object sender, GCodeStatementArg e)
        {
            GeneratedGcode = e.Statement;
        }

        [TearDown]
        public void CleanUp()
        {
            GeneratedGcode = string.Empty;
        }

        [Test]
        public void TestGetAssignment()
        {
            var compiler = new MacroCompiler("@10 = 31.0 + 10.5;");
            compiler.DoBlock();
            compiler.Excecute();
            var variable = VariableDB.LoadVariable("@10");
            Assert.AreEqual(VariableType.FLOAT, variable.Type);
            Assert.AreEqual(41.5f, float.Parse(variable.Literal));
        }

        [Test]
        public void TestGetAssignmentWithCallfunction()
        {
            var compiler = new MacroCompiler("@10 = ABS(-23);");
            compiler.DoBlock();
            compiler.Excecute();
            var variable = VariableDB.LoadVariable("@10");
            Assert.AreEqual(VariableType.FLOAT, variable.Type);
            Assert.AreEqual(23f, float.Parse(variable.Literal));
        }

        [Test]
        public void TestIfClause()
        {
            var compiler = new MacroCompiler("IF (4<2) #1 = 1; ELSE #1 = 2; ENDIF;");
            compiler.DoBlock();
            compiler.Excecute();
            var variable = VariableDB.LoadVariable("#1");
            Assert.AreEqual(2, int.Parse(variable.Literal));

            compiler = new MacroCompiler("IF (1=1) #2 = 1; ENDIF;");
            compiler.DoBlock();
            compiler.Excecute();
            variable = VariableDB.LoadVariable("#2");
            Assert.AreEqual(1, int.Parse(variable.Literal));

            compiler = new MacroCompiler("IF (1=2) #3 = 1; ENDIF;");
            compiler.DoBlock();
            compiler.Excecute();
            Assert.Throws<Exception>(() => VariableDB.LoadVariable("#3"));
        }

        [Test]
        public void TestInvalidIfCondition()
        {
            var compiler = new MacroCompiler("IF (4) #4 = 1; ELSE @1 = 2; ENDIF;");
            compiler.DoBlock();
            Assert.Throws<Exception>(compiler.Excecute);
        }

        [Test]
        public void TestIfBlockClause()
        {
            var compiler = new MacroCompiler("IF ((2+(1-1))=(1+1)) #5 = 5; #6 = 15; ELSE @2 = 2; ENDIF;");
            compiler.DoBlock();
            compiler.Excecute();
            var var1 = VariableDB.LoadVariable("#5");
            var var2 = VariableDB.LoadVariable("#6");
            Assert.AreEqual(5, int.Parse(var1.Literal));
            Assert.AreEqual(15, int.Parse(var2.Literal));
        }

        [Test]
        public void TestBlock()
        {
            var compiler = new MacroCompiler("@3 = 2; IF (@3=2) #7 = 1; #8 = 15; ELSE @4 = 2; ENDIF;");
            compiler.DoBlock();
            compiler.Excecute();
            var var1 = VariableDB.LoadVariable("#7");
            var var2 = VariableDB.LoadVariable("#8");
            Assert.AreEqual(1, int.Parse(var1.Literal));
            Assert.AreEqual(15, int.Parse(var2.Literal));
        }

        [Test]
        public void TestNestedIf()
        {
            var compiler = new MacroCompiler("@3 = 3; IF (@3=3) IF (1=1) #9 = 1; #10 = 15; ENDIF; ELSE @3 = 2; ENDIF;");
            compiler.DoBlock(); 
            compiler.Excecute();
            var var1 = VariableDB.LoadVariable("#9");
            var var2 = VariableDB.LoadVariable("#10");
            Assert.AreEqual(1, int.Parse(var1.Literal));
            Assert.AreEqual(15, int.Parse(var2.Literal));
        }

        [Test]
        public void TestWhile()
        {
            var compiler = new MacroCompiler("@3 = 1; WHILE (@3<3) @3 = @3 + 1; ENDWHILE;");
            compiler.DoBlock();
            compiler.Excecute();
            var variable = VariableDB.LoadVariable("@3");
            Assert.AreEqual(3, int.Parse(variable.Literal));
        }

        [Test]
        public void TestIfInWhile()
        {
            var compiler = new MacroCompiler("@4 = 1; WHILE (@4<3) @4 = @4 + 1; IF (@4 = 2) #11 = @4; ENDIF; ENDWHILE;");
            compiler.DoBlock(); 
            compiler.Excecute();
            var var1 = VariableDB.LoadVariable("@4");
            Assert.AreEqual(3, int.Parse(var1.Literal));
            var var2 = VariableDB.LoadVariable("#11");
            Assert.AreEqual(2, int.Parse(var2.Literal));
        }

        [Test]
        public void TestRepeatUntil()
        {
            var compiler = new MacroCompiler("@5 = 1; REPEAT @5 = @5 + 1; UNTIL (@5 > 3)");
            compiler.DoBlock(); 
            compiler.Excecute();
            var variable = VariableDB.LoadVariable("@5");
            Assert.AreEqual(4, int.Parse(variable.Literal));
        }

        [Test]
        public void TestForToBy()
        {
            var compiler = new MacroCompiler("FOR #12 = 1 TO 10 #13 = #12; ENDFOR; ");
            compiler.DoBlock(); compiler.Excecute();
            var var1 = VariableDB.LoadVariable("#12");
            var var2 = VariableDB.LoadVariable("#13");
            Assert.AreEqual(11, int.Parse(var1.Literal));
            Assert.AreEqual(10, int.Parse(var2.Literal));

            compiler = new MacroCompiler("#14 = 0; FOR #15 = 1 TO 3 BY 2 #14 = #14 + 1; ENDFOR; ");
            compiler.DoBlock(); compiler.Excecute();
            var1 = VariableDB.LoadVariable("#14");
            var2 = VariableDB.LoadVariable("#15");
            Assert.AreEqual(2, int.Parse(var1.Literal));
            Assert.AreEqual(5, int.Parse(var2.Literal));
        }

        [Test]
        public void TestSwitchCase()
        {
            var compiler = new MacroCompiler("X1 = 3; SWITCH (X1) CASE 1,2,3: X2 = 10; CASE 2: X2 = 5; DEFAULT: X2 = 1; ENDSWITCH;");
            Assert.Throws<Exception>(() => compiler.DoBlock());

            compiler = new MacroCompiler("#16 = 3; SWITCH (#16) CASE 1,2,3: #17 = 10; DEFAULT: #17 = 1; ENDSWITCH;");
            compiler.DoBlock(); compiler.Excecute();
            var var1 = VariableDB.LoadVariable("#16");
            var var2 = VariableDB.LoadVariable("#17");
            Assert.AreEqual(3, int.Parse(var1.Literal));
            Assert.AreEqual(10, int.Parse(var2.Literal));

            compiler = new MacroCompiler("#16 = 3; SWITCH (#16) CASE 1,2,3: #17 = 10; CASE 4: #17 = 5; DEFAULT: #17 = 1; ENDSWITCH;");
            compiler.DoBlock(); compiler.Excecute();
            var1 = VariableDB.LoadVariable("#16");
            var2 = VariableDB.LoadVariable("#17");
            Assert.AreEqual(3, int.Parse(var1.Literal));
            Assert.AreEqual(10, int.Parse(var2.Literal));
        }

        [Test]
        public void TestGCode()
        {
            VariableDB.SetVariable("#12", HPType.CreateType(10));
            VariableDB.SetVariable("@15", HPType.CreateType(11));
            var compiler = new MacroCompiler("G01 X1 Y#12 Z14 F@15;");
            compiler.DoBlock(); compiler.Excecute();
            Assert.AreEqual("G01 X1 Y10 Z14 F11", GeneratedGcode);


            compiler = new MacroCompiler("@1 = 10; #3 = 20; @2 = @1; IF (@1 = 10) G10 X10 Y@1 Z(@1+10) F#3; ENDIF;");
            compiler.DoBlock(); compiler.Excecute();
            Assert.AreEqual("G10 X10 Y10 Z20 F20", GeneratedGcode);
        }

        [Test]
        public void TestGCodeWithFloatParam()
        {
            VariableDB.SetVariable("#12", HPType.CreateType(10));
            VariableDB.SetVariable("@15", HPType.CreateType(11));
            var compiler = new MacroCompiler("G01 X1 Y345.34 Z14 F@15;");
            compiler.DoBlock(); compiler.Excecute();
            Assert.AreEqual("G01 X1 Y345.34 Z14 F11", GeneratedGcode);
        }

        [Test]
        public void TestInvalidGcode()
        {
            var compiler = new MacroCompiler("G01 X1 Y345.34 Z14 F-15;");
            compiler.DoBlock(); 
            Assert.Throws<Exception>(compiler.Excecute);
        }

        [Test]
        public void TestGoto()
        {
            var compiler = new MacroCompiler("@24 = 1; GOTO L1; #20 = 2; L1: @10 = 0;");
            compiler.DoBlock();
            compiler.Excecute();
            var var1 = VariableDB.LoadVariable("@24");
            var var2 = VariableDB.LoadVariable("@10");
            Assert.AreEqual(1, int.Parse(var1.Literal));
            Assert.AreEqual(0, int.Parse(var2.Literal));
            Assert.Throws<Exception>(() => VariableDB.LoadVariable("#20"));
        }

        [Test]
        public void TestBreak()
        {
            var compiler = new MacroCompiler("#18 = 1; #19 = 0; WHILE(1=1) #18 = #18 + 1; #19 = #19 + 1; IF (#18>3) BREAK;ENDIF; ENDWHILE;");
            compiler.DoBlock(); compiler.Excecute();
            var var1 = VariableDB.LoadVariable("#18");
            var var2 = VariableDB.LoadVariable("#19");
            Assert.AreEqual(4, int.Parse(var1.Literal));
            Assert.AreEqual(3, int.Parse(var2.Literal));
        }

        [Test]
        public void TestVoidFunction()
        {
            var compiler = new MacroCompiler("WAIT();");
            compiler.DoBlock();
            compiler.Excecute();
            Assert.Fail();
        }

        [Test]
        public void TestMCodeWithMacroFile()
        {
            var compiler = new MacroCompiler("M13 X3 A14 B34;");
            compiler.DoBlock();
            compiler.Excecute();
            Assert.Fail();
        }

        [Test]
        public void TestIndexer()
        {
            var compiler = new MacroCompiler("@[#1 + 1] = 10;");
            compiler.DoBlock();
            compiler.Excecute();
            Assert.Fail();
        }

    }
}
