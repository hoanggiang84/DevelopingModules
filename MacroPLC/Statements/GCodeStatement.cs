using System;
using System.Collections.Generic;
using HPGCodeValidation;
using HPMacroCommon;
using HPMathExpression;
using HPTypes;
using HPVariableRepository;
using LoadIdentifierInterface;
using MacroLexScn;
using System.Reflection;
using System.IO;
using UtilitiesVS2008WinCE;

namespace MacroPLC
{
    public class GCodeStatement : MacroStatement
    {
        private readonly Dictionary<char, IEvaluate<HPType>> _parameter_evaluations =
            new Dictionary<char, IEvaluate<HPType>>();

        private string _command_code;

        public GCodeStatement(IEnumerable<Token> tokens, VariableRepository varDB)
        {
            this.varDB = varDB;
            MathExpression.VarDB = this.varDB;
            tokenManager = new TokenManager(tokens);
            get_info();
        }
   
        private void get_info()
        {
            _command_code = tokenManager.IgnoreWhiteGetNextToken().Text;
            if(_command_code.Length <= 1)
                throw new Exception(string.Format("Invalid G code '{0}'", _command_code));

            var nextToken = tokenManager.IgnoreWhiteLookNextToken();
            while (nextToken.Type != TokenType.END)
            {
                var nextWord = nextToken.Text;
                check_duplicate_parameters(nextWord);
                get_parameters_and_expressions();
                nextToken = tokenManager.IgnoreWhiteLookNextToken();
            }
        }

        private void get_parameters_and_expressions()
        {
            var nextToken = tokenManager.IgnoreWhiteGetNextToken();
            var nextWord = nextToken.Text;
            if (nextWord.Length > 1)
            {
                _parameter_evaluations.Add(nextWord[0], MathExpression.Create(nextWord.Substring(1)));

            }
            else
            {
                nextToken = tokenManager.IgnoreWhiteLookNextToken();
                if (nextToken.Text == MacroKeywords.PARANTHESE_OPEN)
                {
                    var paramTokens = MatchParantheseExpression();
                    var paramEval = MathExpression.Create(paramTokens);
                    _parameter_evaluations.Add(nextWord[0], paramEval);
                }
            }
        }

        private void check_duplicate_parameters(string nextWord)
        {
            if (_parameter_evaluations.ContainsKey(nextWord[0]))
                throw new Exception(string.Format("Duplicate parameters '{0}'", nextWord[0]));
        }

        public override void Execute()
        {
            Step();
        }

        private Dictionary<string,HPType> local_var_dict = new Dictionary<string, HPType>();
        public override void Step()
        {
            var gCodeStatement = _command_code;
            local_var_dict.Clear();
            foreach (var paramsEval in _parameter_evaluations)
            {
                var paramChar = paramsEval.Key;
                var paramValue = paramsEval.Value.Evaluate();
                var param_name = get_local_variable(paramChar);
                local_var_dict.Add(param_name, paramValue);

                var literal = paramValue.Literal;
                if (!literal.Contains("."))
                    literal += ".";
                gCodeStatement += string.Format(" {0}{1}", paramChar, literal);
            }

            if(is_gcode(gCodeStatement))
                varDB.OnGCodeGenerated(gCodeStatement);
            else
                execute_macro_file();
        }

        private void execute_macro_file()
        {
            int code_num;
            char code_char;
            var appPath = get_macro_file_name(out code_char, out code_num);
            var dir_info = new DirectoryInfo(appPath);
            var files = dir_info.GetFiles();
            foreach (var fileInfo in files)
            {
                if (fileInfo.Name.Length <= 1) continue;

                var num_str = fileInfo.Name.Substring(1);
                int num;
                if (!num_str.TryParse(out num)) continue;

                var first_char = fileInfo.Name[0];
                if (code_char == first_char && code_num == num)
                {
                    // Read file and run sub macro
                    var src_code = string.Empty;
                    using (var stream = new StreamReader(fileInfo.FullName))
                    {
                        var line = stream.ReadLine();
                        while (line != null)
                        {
                            src_code += string.Format("{0} \r\n", line);
                            line = stream.ReadLine();
                        }
                    }

                    var compiler = new MacroCompiler(src_code);
                    compiler.Compile();
                    var executor = new MacroExecutor(compiler.compiledTasks);

                    varDB.CreateNewLocalVariablesScope();
                    foreach (var local_val in local_var_dict)
                        varDB.SetVariable(local_val.Key, local_val.Value);

                    executor.Variables = varDB;
                    MathExpression.VarDB = varDB;
                    executor.Execute();
                    varDB.ReturnPreviousLocalVariablesScope();
                }
            }
        }

        private string get_macro_file_name(out char char_code , out int number)
        {
            try
            {
                char_code = _command_code[0];
                number = int.Parse(_command_code.Substring(1));
                var appName = new Uri(Assembly.GetCallingAssembly().GetName().CodeBase).LocalPath;
                return Path.GetDirectoryName(appName);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Invalid G code '{0}'. Error: {1}",
                                                  _command_code, ex.Message));
            }
        }

        private static bool is_gcode(string gCodeStatement)
        {
            try
            {
                validate_gcode(gCodeStatement);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void validate_gcode(string gCodeStatement)
        {
            new GCodeValidate(gCodeStatement).Validate();
        }

        private static string get_local_variable(char parameter_char)
        {
            var first = 'A';
            var last = 'Z';

            if(first <= parameter_char && parameter_char <= last)
            {
                var local_var_num = parameter_char - first + 1;
                return string.Format("#{0}", local_var_num);
            }

            throw new Exception(string.Format("Invalid parameter charactor '{0}'", parameter_char));
        }
    }
}