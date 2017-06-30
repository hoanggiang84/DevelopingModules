using System;
using HPMathExpression;
using HPTypes;
using MacroVariableDB;
using NUnit.Framework;

namespace UnitTestMacroCompiler
{
    
    [TestFixture]
    public class UnitTestExpressions
    {
        [SetUp]
        public void Setup()
        {
            VariableDB.InitializeVariables();
        }

        [TearDown]
        public void CleanUp()
        {
            VariableDB.ResetLocalVariables();
        }

        [Test]
        public void TestConstantExpression()
        {
            var exprStr = "1234";
            var eval = MathExpression.Create(exprStr).Evaluate();
            Assert.AreEqual(VariableType.INT, eval.Type);
            Assert.AreEqual(1234, int.Parse(eval.Literal));
        }

        [Test]
        public void TestVariableExpression()
        {
            var exprStr = "#1";
            VariableDB.SetVariable("#1", HPType.CreateType("10"));
            var varExpr = MathExpression.Create(exprStr).Evaluate();
            Assert.AreEqual(10, int.Parse(varExpr.Literal));
        }

        [Test]
        public void TestCallFunctionExpression()
        {
            var exprStr = "ABS(-10)";
            var varExpr = MathExpression.Create(exprStr).Evaluate();
            Assert.AreEqual(10f, float.Parse(varExpr.Literal));
        }

        [Test]
        public void TestCallFunctionExpressionWithVariable()
        {
            VariableDB.SetVariable("#1", HPType.CreateType("34.12"));
            var exprStr = "FTOI(#1)";
            var varExpr = MathExpression.Create(exprStr).Evaluate();
            Assert.AreEqual(34, float.Parse(varExpr.Literal));
        }

        [Test]
        public void TestCallFunctionExpressionWithTwoVariable()
        {
            VariableDB.SetVariable("#1", HPType.CreateType("34"));
            var exprStr = "MAX(#1, 30)";
            var varExpr = MathExpression.Create(exprStr).Evaluate();
            Assert.AreEqual(34f, float.Parse(varExpr.Literal));
        }

        [Test]
        public void TestGlobalVariableExpression()
        {
            var exprStr = "(@12 + 1)";
            VariableDB.SetVariable("@12", HPType.CreateType("10.1"));
            var varExpr = MathExpression.Create(exprStr).Evaluate();
            Assert.AreEqual(VariableType.FLOAT, varExpr.Type);
            Assert.AreEqual(11.1f, float.Parse(varExpr.Literal));
        }

        [Test]
        public void TestSignedNumberExpression()
        {
            var exprStr = "-1234";
            var eval = MathExpression.Create(exprStr).Evaluate();
            Assert.AreEqual(VariableType.INT, eval.Type);
            Assert.AreEqual(-1234, int.Parse(eval.Literal));
        }

        [Test]
        public void TestMulOpExpression()
        {
            var exprStr = "-1234 * 10 / 100";
            var eval = MathExpression.Create(exprStr).Evaluate();
            Assert.AreEqual(VariableType.INT, eval.Type);
            Assert.AreEqual(-123, int.Parse(eval.Literal));
        }

        [Test]
        public void TestInvalidDivide()
        {
            var exprStr = "-1234 / 0";
            var expression = MathExpression.Create(exprStr);
            Assert.Throws<Exception>(() => expression.Evaluate());

            exprStr = "-1234 % 0";
            expression = MathExpression.Create(exprStr);
            Assert.Throws<Exception>(() => expression.Evaluate());
        }

        [Test]
        public void TestArithmeticExpression()
        {
            var exprStr = "1 - 1234 * 10 / 100 + 122";
            var expression = MathExpression.Create(exprStr).Evaluate();
            Assert.AreEqual(0, int.Parse(expression.Literal));
        }

        [Test]
        public void TestRelationExpression()
        {
            var exprStr = "1-1234<0";
            var expression = MathExpression.Create(exprStr).Evaluate();
            Assert.IsTrue(bool.Parse(expression.Literal));
        }

        [Test]
        public void TestBoolLiteralExpression()
        {
            var exprStr = "true";
            var expression = MathExpression.Create(exprStr).Evaluate();
            Assert.IsTrue(bool.Parse(expression.Literal));

            exprStr = "false";
            expression = MathExpression.Create(exprStr).Evaluate();
            Assert.IsFalse(bool.Parse(expression.Literal));
        }

        [Test]
        public void TestNotFactorExpression()
        {
            var exprStr = "!true";
            var expression = MathExpression.Create(exprStr).Evaluate();
            Assert.IsFalse(bool.Parse(expression.Literal));
        }

        [Test]
        public void TestNotBitwiseExpression()
        {
            var exprStr = "!1";
            var expression = MathExpression.Create(exprStr).Evaluate();
            var expectValue = ~1;
            Assert.AreEqual(expectValue, int.Parse(expression.Literal));
        }

        [Test]
        public void TestAndLogicExpression()
        {
            var exprStr = "TRUE & FALSE";
            var expression = MathExpression.Create(exprStr).Evaluate();
            Assert.IsFalse(bool.Parse(expression.Literal));
        }

        [Test]
        public void TestAndBitwiseExpression()
        {
            var exprStr = "1 & 3";
            var expression = MathExpression.Create(exprStr).Evaluate();
            var expectValue = 1;
            Assert.AreEqual(expectValue, int.Parse(expression.Literal));
        }

        [Test]
        public void TestORLogicExpression()
        {
            var exprStr = "TRUE | FALSE";
            var expression = MathExpression.Create(exprStr).Evaluate();
            Assert.IsTrue(bool.Parse(expression.Literal));
        }

        [Test]
        public void TestBitwiseExpression()
        {
            var exprStr = "0 | 1 & 3 ";
            var expression = MathExpression.Create(exprStr).Evaluate();
            var expectValue = 1;
            Assert.AreEqual(expectValue, int.Parse(expression.Literal));
        }

        [Test]
        public void TestParanthesisExpression()
        {
            var exprStr = "(2=(1+1))";
            var expression = MathExpression.Create(exprStr).Evaluate();
            Assert.IsTrue(bool.Parse(expression.Literal));
        }

        [Test]
        public void TestFloatExpression()
        {
            var exprStr = "((1.4+1.5))";
            var expression = MathExpression.Create(exprStr).Evaluate();
            Assert.AreEqual(VariableType.FLOAT,expression.Type);
            Assert.AreEqual(2.9f, float.Parse(expression.Literal));
        }

        [Test]
        public void TestCsharpArithmetic()
        {
            var numberf = 11/2;
            Assert.IsInstanceOf(typeof(int), numberf & -1);
            Assert.AreEqual(numberf, numberf & -1);
        }
    }
}