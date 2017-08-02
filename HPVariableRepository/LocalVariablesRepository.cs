using System;
using HPTypes;
using System.Collections.Generic;

namespace HPVariableRepository
{
    public class LocalVariablesRepository
    {
        private readonly Dictionary<string, HPType> initial_variables = new Dictionary<string, HPType>();

        private Dictionary<string, HPType> _variables = new Dictionary<string,HPType>();

        public LocalVariablesRepository(){}

        public LocalVariablesRepository(LocalVariablesRepository localVariablesRepository)
        {
            foreach (var v in localVariablesRepository._variables)
                initial_variables.Add(v.Key, HPType.CreateType(v.Value.Literal));

            Reset();
        }

        public void SetVariable(string name, HPType variable)
        {
            if(string.IsNullOrEmpty(name))
                throw new Exception("Empty local variable name");

            if(_variables.ContainsKey(name))
                _variables[name].SetLiteral(variable.Literal);
            else
                _variables.Add(name, HPType.CreateType(variable.Literal));
        }

        public HPType LoadVariable(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("Empty local variable name");

            if (_variables.ContainsKey(name))
                return new HPType(_variables[name]);

            throw new Exception(string.Format("Local variable '{0}' has not been assigned", name));
        }

        public void Reset()
        {
            _variables = new Dictionary<string, HPType>();
            foreach (var val in initial_variables)
                _variables.Add(val.Key, HPType.CreateType(val.Value.Literal));
        }

        public bool Contains(string name)
        {
            return _variables.ContainsKey(name);
        }
    }
}