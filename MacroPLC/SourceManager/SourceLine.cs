using HPGCodeValidation;
using HPMacroCommon;
using HPMacroFunctions;
using MacroLexScn;
using System.Collections.Generic;
using UtilitiesVS2008WinCE;

namespace MacroPLC
{
    public class SourceLine
    {
        public readonly string Text;
        public readonly int LineNumber;

        private List<Token> _tokens;
        public List<Token> Tokens
        {
            get { return _tokens; }
        } 

        private Keyword _type;
        public Keyword Type { get { return _type; } }
    
        public SourceLine(string Text, int LineNumber)
        {
            this.Text = Text;
            this.LineNumber = LineNumber;
            GetStatementType();
        }

        private void GetStatementType()
        {
            if (Text.IsNullOrWhite())
            {
                _type = Keyword.WHITE_SPACE;
                return;
            }

            var lex_scn = new MacroLexicalScanner(Text);
            var next_tkn = lex_scn.ScanNext();

            _tokens = new List<Token>();
            while (next_tkn.Type == TokenType.WHITE_SPACE)
            {
                _tokens.Add(next_tkn);
                next_tkn = lex_scn.ScanNext();
            }

            if(next_tkn.Type == TokenType.LOCAL_VAR
                || next_tkn.Type == TokenType.GLOBAL_VAR)
            {
                _type = Keyword.VAR;
            }
            else if (MacroKeywords.IsKeyword(next_tkn.Text))
            {
                _type = MacroKeywords.GetKeywordType(next_tkn.Text);
            }
            else if (GCodeValidate.ValidateGCodeCommand(next_tkn.Text))
            {
                _type = Keyword.GCODE;
            }
            else if (HPFUNC.IsMacroFuntion(next_tkn.Text))
            {
                _type = Keyword.FUNCTION;
            }
            else if(next_tkn.Type == TokenType.IDENTIFIER)
            {   
                _tokens.Add(next_tkn);
                next_tkn = lex_scn.ScanNext();
                if(next_tkn.Text == MacroKeywords.COLON)
                {
                    _type = Keyword.LABEL;
                    while (next_tkn.Type != TokenType.END)
                    {
                        _tokens.Add(next_tkn);
                        next_tkn = lex_scn.ScanNext();
                    }
                    return;
                }
            }

            while(next_tkn.Type != TokenType.END)
            {
                _tokens.Add(next_tkn);
                next_tkn = lex_scn.ScanNext();
            }
        }
    }
}