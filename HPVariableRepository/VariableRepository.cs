using System;
using System.Globalization;
using System.Collections.Generic;
using HPMacroFunctions;
using HPTypes;

namespace HPVariableRepository
{
    public class VariableRepository
    {
        public event EventHandler<VariableArg> VariableAssigned;

        public event EventHandler<GCodeStatementArg> GCodeGenerated;

        public void OnVariableAssigned(string name, HPType value)
        {
            if (VariableAssigned != null)
                VariableAssigned.Invoke(null, new VariableArg(name, value));
        }

        public void OnGCodeGenerated(string statemet)
        {
            if (GCodeGenerated != null)
                GCodeGenerated.Invoke(null, new GCodeStatementArg(statemet));
        }

        private Dictionary<string, HPType> GlobalVariables = new Dictionary<string, HPType>();

        private Stack<Dictionary<string, HPType>> LocalVariablesStack =
            new Stack<Dictionary<string, HPType>>();

        private Dictionary<string, HPType> CurrentLocalVariables;

        public void InitializeVariables()
        {
            InitializeGlobalVariables();
            ResetLocalVariables();
        }

        public void ResetLocalVariables()
        {
            LocalVariablesStack.Clear();
            CurrentLocalVariables = new Dictionary<string, HPType>();
            LocalVariablesStack.Push(CurrentLocalVariables);
        }

        private const string FloatInitial = ".0";
        private const string IntegerInitial = "0";
        private void InitializeGlobalVariables()
        {
            // @0~@511: float arithmetic global variables
            var index = 0;
            for (; index < 512; index++)
            {
                var varName = string.Format("@{0}", index);
                if (!(GlobalVariables.ContainsKey(varName)))
                    GlobalVariables.Add(varName, HPType.CreateType(FloatInitial));
            }

            // @512~@767: int32 R0~R255
            for (; index < 768; index++)
            {
                var varName = string.Format("@{0}", index);
                if (!(GlobalVariables.ContainsKey(varName)))
                    GlobalVariables.Add(varName, HPType.CreateType(IntegerInitial));
            }

            // @768~@2047: float variables store in ROM
            for (; index < 2048; index++)
            {
                var varName = string.Format("@{0}", index);
                if (!(GlobalVariables.ContainsKey(varName)))
                    GlobalVariables.Add(varName, HPType.CreateType(FloatInitial));
            }

            // @10000~@19999: int32 R0~R9999 
            index = 10000;
            for (; index < 20000; index++)
            {
                var varName = string.Format("@{0}", index);
                if (!(GlobalVariables.ContainsKey(varName)))
                    GlobalVariables.Add(varName, HPType.CreateType(IntegerInitial));
            }
        }

        public bool IsGlobal(string name)
        {
            return GlobalVariables.ContainsKey(name);
        }

        public bool IsLocal(string name)
        {
            return CurrentLocalVariables.ContainsKey(name);
        }

        public void SetVariable(string name, HPType value)
        {
            if (GlobalVariables.ContainsKey(name))
            {
                var newLiteral = ValidateType(GlobalVariables[name], value, name);
                GlobalVariables[name].SetLiteral(newLiteral);
            }
            else if (CurrentLocalVariables.ContainsKey(name))
            {
                var newLiteral = ValidateType(CurrentLocalVariables[name], value, name);
                CurrentLocalVariables[name].SetLiteral(newLiteral);
            }
            else if (IsIdentifier(name))
            {
                CurrentLocalVariables.Add(name, new HPType(value));
            }
            else
                throw new Exception(string.Format("Invalid Variable Name '{0}'", name));
            OnVariableAssigned(name, LoadVariable(name));
        }

        private static string ValidateType(HPType variableValue, HPType value, string name)
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

        private static bool IsIdentifier(string name)
        {
            try
            {
                if (char.IsLetter(name[0]) || name[0] == '#')
                {
                    for (var i = 1; i < name.Length; i++)
                        if (!char.IsLetterOrDigit(name[i]))
                            return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void CreateNewLocalVariablesScope()
        {
            CurrentLocalVariables = new Dictionary<string, HPType>();
            LocalVariablesStack.Push(CurrentLocalVariables);
        }

        public void ReturnPreviousLocalVariablesScope()
        {
            LocalVariablesStack.Pop();
            CurrentLocalVariables = LocalVariablesStack.Pop();
            LocalVariablesStack.Push(CurrentLocalVariables);
        }

        public HPType LoadVariable(string varName)
        {
            if (GlobalVariables.ContainsKey(varName))
                return GlobalVariables[varName];
            if (CurrentLocalVariables.ContainsKey(varName))
                return CurrentLocalVariables[varName];
            throw new Exception(string.Format("Unrecognized Identifiers '{0},", varName));
        }

        public static HPType LoadReturnValueMacroFunction(string name, List<HPType> args)
        {
            if (HPFUNC.IsReturnValueFunction(name))
                return HPFUNC.GetReturnValueFunction(name).Invoke(args);
            throw new Exception(string.Format("Unrecognized Not Void Function '{0}'", name));
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
