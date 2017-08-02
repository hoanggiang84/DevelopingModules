using System;
using System.Globalization;
using System.Collections.Generic;
using HPMacroFunctions;
using HPTypes;

namespace HPVariableRepository
{
    public class VariableRepository
    {
        public VariableRepository()
        {
            _current_local_variables = new LocalVariablesRepository();
            _local_variables_stack.Push(_current_local_variables);
        }

        public event EventHandler<VariableArg> VariableAssigned;

        public event EventHandler<GCodeStatementArg> GCodeGenerated;

        public void OnVariableAssigned(string name, HPType value)
        {
            if (VariableAssigned != null)
                VariableAssigned.Invoke(this, new VariableArg(name, value));
        }

        public void OnGCodeGenerated(string statement)
        {
            if (GCodeGenerated != null)
                GCodeGenerated.Invoke(this, new GCodeStatementArg(statement));
        }

        private Dictionary<string, HPType> _global_variables =
            new Dictionary<string, HPType>();

        private Stack<LocalVariablesRepository> _local_variables_stack = 
            new Stack<LocalVariablesRepository>();

        private LocalVariablesRepository _current_local_variables = 
            new LocalVariablesRepository();

        public void InitializeVariables()
        {
            initialize_global_variables();
            ResetLocalVariables();
        }

        public void ResetLocalVariables()
        {
            _current_local_variables.Reset();
        }

        private const string FLOAT_INITIAL = ".0";
        private const string INTEGER_INITIAL = "0";
        private void initialize_global_variables()
        {
            // @0~@511: float arithmetic global variables
            var index = 0;
            for (; index < 512; index++)
            {
                var varName = string.Format("@{0}", index);
                if (!(_global_variables.ContainsKey(varName)))
                    _global_variables.Add(varName, HPType.CreateType(FLOAT_INITIAL));
            }

            // @512~@767: int32 R0~R255
            for (; index < 768; index++)
            {
                var varName = string.Format("@{0}", index);
                if (!(_global_variables.ContainsKey(varName)))
                    _global_variables.Add(varName, HPType.CreateType(INTEGER_INITIAL));
            }

            // @768~@2047: float variables store in ROM
            for (; index < 2048; index++)
            {
                var varName = string.Format("@{0}", index);
                if (!(_global_variables.ContainsKey(varName)))
                    _global_variables.Add(varName, HPType.CreateType(FLOAT_INITIAL));
            }

            // @10000~@19999: int32 R0~R9999 
            index = 10000;
            for (; index < 20000; index++)
            {
                var varName = string.Format("@{0}", index);
                if (!(_global_variables.ContainsKey(varName)))
                    _global_variables.Add(varName, HPType.CreateType(INTEGER_INITIAL));
            }
        }

        public bool IsGlobal(string name)
        {
            return _global_variables.ContainsKey(name);
        }

        public bool IsLocal(string name)
        {
            return _current_local_variables.Contains(name);
        }

        public void SetVariable(string name, HPType value)
        {
            if (_global_variables.ContainsKey(name))
            {
                var new_literal = validate_type(_global_variables[name], value, name);
                _global_variables[name].SetLiteral(new_literal);
            }
            else if (_current_local_variables.Contains(name))
            {
                var current_val = _current_local_variables.LoadVariable(name);
                var new_literal = validate_type(current_val, value, name);
                _current_local_variables.SetVariable(name, HPType.CreateType(new_literal));
            }
            else if (is_identifier(name))
            {
                _current_local_variables.SetVariable(name, value);
            }
            else
                throw new Exception(string.Format("Invalid variable name '{0}'", name));

            OnVariableAssigned(name, LoadVariable(name));
        }

        private static string validate_type(HPType variableValue, HPType value, string name)
        {
            if (variableValue.Type == value.Type)
                return value.Literal;

            try
            {
                switch (variableValue.Type)
                {
                    case VariableType.FLOAT:
                        return float.Parse(value.Literal).ToString(CultureInfo.InvariantCulture);
                    case VariableType.INT:
                        return int.Parse(value.Literal).ToString(CultureInfo.InvariantCulture);
                    default:
                        return bool.Parse(value.Literal).ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Cannot assign variable '{0}'", name), ex);
            }
        }

        private static bool is_identifier(string name)
        {
            try
            {
                if (char.IsLetter(name[0]) || name[0] == '#')
                    for (var i = 1; i < name.Length; i++)
                        if (!char.IsLetterOrDigit(name[i]))
                            return false;
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void CreateNewLocalVariablesScope()
        {
            var newLocalVarDB = new LocalVariablesRepository();
            _local_variables_stack.Push(newLocalVarDB);
            _current_local_variables = newLocalVarDB;
        }

        public void CreateNewLocalVariablesScope(LocalVariablesRepository variables)
        {
            var newLocalVarDB =new LocalVariablesRepository(variables);
            _local_variables_stack.Push(newLocalVarDB);
            _current_local_variables = newLocalVarDB;
        }

        public void ReturnPreviousLocalVariablesScope()
        {
            _local_variables_stack.Pop();
            _current_local_variables = _local_variables_stack.Pop();
            _local_variables_stack.Push(_current_local_variables);
        }

        public HPType LoadVariable(string varName)
        {
            if (_global_variables.ContainsKey(varName))
                return _global_variables[varName];

            if (_current_local_variables.Contains(varName))
                return _current_local_variables.LoadVariable(varName);

            throw new Exception(string.Format("Invalid identifiers '{0}'", varName));
        }

        public static HPType LoadReturnValueMacroFunction(string name, List<HPType> args)
        {
            if (HPFUNC.IsReturnValueFunction(name))
                return HPFUNC.GetReturnValueFunction(name).Invoke(args);

            throw new Exception(string.Format("Invalid function '{0}'", name));
        }
    }

    public class GCodeStatementArg : EventArgs
    {
        public readonly string Statement;
        public GCodeStatementArg(string statement)
        {
            Statement = statement;
        }
    }

    public class VariableArg : EventArgs
    {
        public readonly string Name;
        public readonly HPType Value;
        public VariableArg(string varName, HPType value)
        {
            Name = varName;
            Value = new HPType(value);
        }
    }
}
