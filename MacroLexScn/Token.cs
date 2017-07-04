namespace MacroLexScn
{
    public class Token
    {
        public readonly string Text;
        public readonly TokenType Type;

        public Token(string text, TokenType type)
        {
            Text = text;
            Type = type;
        }
    }

    public enum TokenType
    {
        UNDEFINED,
        WHITE_SPACE,
        IDENTIFIER,
        NUMBER,
        SYMBOL,
        GLOBAL_VAR,
        LOCAL_VAR,
        END
    } 
}