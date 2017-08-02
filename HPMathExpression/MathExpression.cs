using System;
using System.Collections.Generic;
using HPMacroCommon;
using HPTypes;
using HPVariableRepository;
using LoadIdentifierInterface;
using MacroLexScn;

namespace HPMathExpression
{
    public abstract class MathExpression : IEvaluate<HPType>
    {
        protected VariableRepository evaluate_variables;

        private static VariableRepository _variable_repository;
        public static VariableRepository Variables{
            get { return _variable_repository; }
            set
            {
                if(value == null)
                    throw new NullReferenceException("Null Variable Repository");
                _variable_repository = value;
            }
        }

        public virtual HPType Evaluate()
        {
            return HPType.CreateType("0");
        }

        public static IEvaluate<HPType> Create(string expr, VariableRepository variables_repository)
        {
            Variables = variables_repository;
            lexical_scan(expr);
            return logic_expression();
        }

        private static TokenManager _token_manager;
        public static IEvaluate<HPType> Create(List<Token> tokens, VariableRepository variables_repository)
        {
            Variables = variables_repository;
            foreach (var token in tokens)
                validate_token_MathExpressionession(token);
            _token_manager = new TokenManager(tokens);
            return logic_expression();
        }

        private static IEvaluate<HPType> logic_expression()
        {
            var xorEvals = new List<IEvaluate<HPType>> { XOR_term() };
            var nextToken = look_next_token();
            while (nextToken.Text == MacroKeywords.OR )
            {
                match(MacroKeywords.OR);
                xorEvals.Add(XOR_term());
                nextToken = look_next_token();
            }
            return new ORTerm(xorEvals);
        }

        private static IEvaluate<HPType> XOR_term()
        {
            var andEvals = new List<IEvaluate<HPType>> { AND_term() };
            var nextToken = look_next_token();
            while (nextToken.Text == MacroKeywords.XOR)
            {
                match(MacroKeywords.XOR);
                andEvals.Add(AND_term());
                nextToken = look_next_token();
            }
            return new XORTerm(andEvals);
        }

        private static IEvaluate<HPType> AND_term()
        {
            var notFactor = new List<IEvaluate<HPType>> { NOT_factor() };
            var nextToken = look_next_token();
            while (nextToken.Text == MacroKeywords.AND)
            {
                match(MacroKeywords.AND);
                notFactor.Add(NOT_factor());
                nextToken = look_next_token();
            }
            return new ANDTerm(notFactor);
        }

        private static IEvaluate<HPType> NOT_factor()
        {
            var nextToken = look_next_token();
            var notSign = string.Empty;
            if (nextToken.Text == MacroKeywords.NOT)
            {
                match(MacroKeywords.NOT);
                notSign = MacroKeywords.NOT;
            }
            return new NOTFactor(logic_factor(), notSign);
        }

        private static IEvaluate<HPType> logic_factor()
        {
            var nextToken = look_next_token();
            if (is_boolean(nextToken.Text))
            {
                match(nextToken.Text);
                return new BoolLiteral(nextToken.Text.ToUpper());
            }
            return relation_operation();
        }

        private static IEvaluate<HPType> relation_operation()
        {
            var firstArg = arithmetic_operation();
            var nextToken = look_next_token();

            if (is_relation_operation(nextToken.Text))
            {
                var relOper = nextToken.Text;
                match(relOper);
                var secondArg = arithmetic_operation();
                return new RelationEvaluate(firstArg, secondArg, relOper);
            }
            return firstArg;
        }

        private static IEvaluate<HPType> arithmetic_operation()
        {
            var addEvals = new List<IEvaluate<HPType>> { term() };
            var addOpers = new List<string>();

            var nextToken = look_next_token();
            while (is_add_operation(nextToken.Text))
            {
                var addOp = nextToken.Text;
                match(addOp);
                addEvals.Add(term());
                addOpers.Add(addOp);
                nextToken = look_next_token();
            }
            return new ArithmeticEvaluate(addEvals, addOpers);
        }

        private static IEvaluate<HPType> term()
        {
            var mulEvals = new List<IEvaluate<HPType>> { signed_factor() };
            var mulOpers = new List<string>();

            var nextToken = look_next_token();
            while (is_multiply_operation(nextToken.Text))
            {
                var mulOp = nextToken.Text;
                match(mulOp);
                mulOpers.Add(mulOp);
                mulEvals.Add(factor());
                nextToken = look_next_token();
            }
            return new TermEvaluate(mulEvals, mulOpers);
        }

        private static IEvaluate<HPType> factor()
        {
            var nextToken = look_next_token();
            if (nextToken.Text == MacroKeywords.PARANTHESE_OPEN)
            {
                match(MacroKeywords.PARANTHESE_OPEN);
                var factor = logic_expression();
                match(MacroKeywords.PARANTHESE_CLOSE);
                return factor;
            }

            if (nextToken.Type == TokenType.GLOBAL_VAR
            || nextToken.Type == TokenType.LOCAL_VAR)
            {
                var variableToken = get_next_token();
                if((variableToken.Text == "#" || variableToken.Text == "@") &&
                    look_next_token().Text == MacroKeywords.INDEX_OPEN)
                {
                    match(MacroKeywords.INDEX_OPEN);
                    var index = logic_expression();
                    match(MacroKeywords.INDEX_CLOSE);
                    return new VariableIndexer(variableToken.Text, index, Variables);
                }
                return new IdentifierEvaluate(nextToken.Text, null, Variables);
            }

            if (nextToken.Type == TokenType.IDENTIFIER)
            {
                return call_function();
            }

            if (nextToken.Type == TokenType.NUMBER)
            {
                get_next_token();
                return new ConstantEvaluate(nextToken.Text);
            }

            throw Error("Invalid Expression");
        }

        private static IEvaluate<HPType> call_function()
        {
            var functionName = get_next_token().Text;
            var argEvals = new List<IEvaluate<HPType>>();
            if (look_next_token().Text == MacroKeywords.PARANTHESE_OPEN)
            {
                match(MacroKeywords.PARANTHESE_OPEN);

                while (!(look_next_token().Text == MacroKeywords.PARANTHESE_CLOSE 
                    || look_next_token().Type == TokenType.END))
                {
                    argEvals.Add(arithmetic_operation());
                    while (look_next_token().Text == MacroKeywords.COMMA)
                    {
                        match(MacroKeywords.COMMA);
                        argEvals.Add(arithmetic_operation());
                    }
                }

                match(MacroKeywords.PARANTHESE_CLOSE);
            }
            return new IdentifierEvaluate(functionName, argEvals,Variables);
        }

        private static IEvaluate<HPType> signed_factor()
        {
            var nextToken = look_next_token();
            var addOp = MacroKeywords.ADD;
            switch (nextToken.Text)
            {
                case MacroKeywords.SUBTRACT:
                    match(MacroKeywords.SUBTRACT);
                    addOp = MacroKeywords.SUBTRACT;
                    break;
                case MacroKeywords.ADD:
                    match(MacroKeywords.ADD);
                    break;
            }
            var a_factor = factor();
            return new SignedFactorEvaluate(a_factor, addOp);
        }

        #region Lexical scan
        private static void lexical_scan(string Source)
        {
            var tokens = new List<Token>();
            var lexicalScanner = new MacroLexicalScanner(Source);
            var token = lexicalScanner.ScanNext();
            while (token.Type != TokenType.END)
            {
                validate_token_MathExpressionession(token);
                tokens.Add(token);
                token = lexicalScanner.ScanNext();
            }
            _token_manager = new TokenManager(tokens);
        }

        private static void validate_token_MathExpressionession(Token token)
        {
            if (token.Type == TokenType.UNDEFINED)
                throw Error(string.Format("Invalid symbol '{0}'", token.Text));

            if(token.Type == TokenType.IDENTIFIER 
                && MacroKeywords.IsKeyword(token.Text) 
                && !(token.Text.Equals(MacroKeywords.TRUE,StringComparison.InvariantCultureIgnoreCase)
                    || token.Text.Equals(MacroKeywords.FALSE, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw Error(string.Format("Keyword can't be used as variable '{0}'", token.Text));
            }

            if(token.Type == TokenType.SYMBOL
                && !MacroKeywords.IsMathSymbol(token.Text))
            {
                throw Error(string.Format("Invalid symbol '{0}'", token.Text));
            }
        }

        protected static int CurrentIndex;
        private static Token get_next_token()
        {
            return _token_manager.IgnoreWhiteGetNextToken();
        }

        private static Token look_next_token()
        {
            return _token_manager.IgnoreWhiteLookNextToken();
        }

        #endregion

        #region Exceptions
        protected static Exception Expected(string str)
        {
            return new Exception(string.Format("Expected: '{0}'", str));
        }

        protected static Exception Error(string str)
        {
            return new Exception(string.Format("Error: {0}", str));
        }
        #endregion

        #region Member functions
        public bool IsGlobalVariables(string expression)
        {
            return Variables.IsGlobal(expression);
        }

        public bool IsLocalVariables(string expression)
        {
            return Variables.IsLocal(expression);
        }

        private static void match(string str)
        {
            var nextToken = get_next_token();
            if (nextToken.Text != str)
                throw Expected(str);
        }

        private static bool is_add_operation(string op)
        {
            return (op == MacroKeywords.ADD || op == MacroKeywords.SUBTRACT);
        }

        private static bool is_multiply_operation(string op)
        {
            return (op == MacroKeywords.MULTIPLY 
                || op == MacroKeywords.DIVIDE 
                || op == MacroKeywords.MODULUS);
        }

        private static bool is_boolean(string boolLiteral)
        {
            var upperBool = boolLiteral.ToUpper();
            return upperBool == MacroKeywords.TRUE 
                || upperBool == MacroKeywords.FALSE;
        }

        private static readonly List<string> RelationOperators = 
            new List<string>
                {
                    MacroKeywords.EQUAL, 
                    MacroKeywords.LESS, 
                    MacroKeywords.NOT_EQUAL, 
                    MacroKeywords.LESS_EQUAL, 
                    MacroKeywords.GREATER, 
                    MacroKeywords.GREATER_EQUAL
                };
        private static bool is_relation_operation(string op)
        {
            return RelationOperators.Contains(op);
        }
        #endregion
    }
}
