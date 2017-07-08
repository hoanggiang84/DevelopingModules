namespace HPMacroTask
{
    public enum TaskType
    {
        NULL,
        BOOLEAN_EVALUATE,
        ARITHMETIC_EVALUATE,
        BRANCH_FALSE,
        BRANCH_TRUE,
        BRANCH,
        BRANCH_GREATER,
        BRANCH_EQUAL,
        LABEL,
        ASSIGNMENT,
        GCODE,
        BUILT_IN_FUNCTION
    }
}