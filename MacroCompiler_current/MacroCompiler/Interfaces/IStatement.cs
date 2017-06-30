namespace HPCompiler
{
    public interface IStatement
    {
        void Execute();

        void Step();
    }
}