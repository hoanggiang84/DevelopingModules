using System.Collections.Generic;
using System;
using HPMacroComponents;

namespace HPCompiler
{
    public partial class MacroCompiler
    {
        private string Source;
        private readonly List<Token> SourceTokens = new List<Token>();

        private Exception Error(string str)
        {
            return new Exception(string.Format("Error: {0}", str));
        }
    }
}