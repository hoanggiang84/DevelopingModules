using System;
using System.Linq;
using System.Collections.Generic;
using MacroLexScn;
using UtilitiesVS2008WinCE;

namespace MacroPLC
{
    public class GMCodeExtension
    {
        public readonly List<Token> Tokens;
        public GMCodeExtension(string code)
        {
            Tokens = new List<Token>();
            var lex_scn = new MacroLexicalScanner(code);
            var next_token = lex_scn.ScanNext();
            while (next_token.Type != TokenType.END)
            {
                if(next_token.Type != TokenType.WHITE_SPACE)
                    Tokens.Add(next_token);
                next_token = lex_scn.ScanNext();
            }
        }

        public GMCodeExtension(IEnumerable<Token> tokens)
        {
            Tokens = new List<Token>();
            var token_mgr = new TokenManager(tokens);
            var next_token = tryGetNextToken(token_mgr);
            while (next_token != null && next_token.Type != TokenType.END)
            {
                if(next_token.Type != TokenType.WHITE_SPACE)
                    Tokens.Add(next_token);
                next_token = tryGetNextToken(token_mgr);
            }
        }

        private static Token tryGetNextToken(TokenManager token_manager)
        {
            try
            {
                return token_manager.IgnoreWhiteGetNextToken();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool Validate()
        {
            var cmd_token = Tokens.First();
            if(!isGMExtensionCommand(cmd_token))
                throw new Exception(string.Format("Invalid G code extension command '{0}'", 
                    cmd_token.Text));

            for (var i = 1; i < Tokens.Count; i++)
            {
                if(Tokens[i].Type != TokenType.NUMBER 
                    && Tokens[i].Type != TokenType.GLOBAL_VAR
                    && Tokens[i].Type != TokenType.LOCAL_VAR)
                    throw new Exception(
                        string.Format("G code extension parameter must be a number or variable '{0}'", 
                        Tokens[i].Text));
            }
            return true;
        }

        /// <summary>
        /// M or G[0000~9999]
        /// </summary>
        private static bool isGMExtensionCommand(Token token)
        {
            var cmd = token.Text;
            if (token.Type != TokenType.IDENTIFIER && cmd[0] != 'M' && cmd[0] != 'G')
                return false;

            var gm_num_str = cmd.Substring(1);
            int gm_num;
            if (!gm_num_str.TryParseInt32(out gm_num))
                return false;

            if (0 > gm_num || gm_num > 9999)
                return false;

            return true;
        }
    }
}
