namespace MacroPLC
{
    public enum StatementType
    {
        NULL,
        ASSIGNMENT,
        GCODE,
        FUNCTION,
        IF,
        ELSE,
        END_IF,
        WHILE,
        END_WHILE,
        REPEAT,
        UNTIL,
        FOR,
        END_FOR,
        LOOP,
        END_LOOP,
        GOTO,
        SWITCH,
        END_SWITCH,
        BREAK
    }
}