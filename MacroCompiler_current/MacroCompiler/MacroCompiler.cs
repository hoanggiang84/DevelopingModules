using HPMacroComponents;

namespace HPCompiler
{
    public partial class MacroCompiler
    {
        public MacroCompiler(string source)
        {
            Source = source;
            LexicalScan();
        }

        private void LexicalScan()
        {
            var scanner = new LexicalScanner(Source);
            var token = scanner.Scan();
            while (token.Type != TokenType.END)
            {
                if (token.Type == TokenType.UNDEFINED)
                    throw Error(string.Format("Undefined Symbol '{0}'", token.Text));
                SourceTokens.Add(token);
                token = scanner.Scan();
            }
        }

        public void DoBlock()
        {
            MacroProgram.SetSourceTokens(SourceTokens);
            MacroProgram.Compile();
        }

        public void Excecute()
        {
            MacroProgram.Execute();
        }

        public void Step()
        {
            MacroProgram.Step();
        }
    }
}
