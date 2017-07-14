namespace MacroPLC
{
    public class SourceLine
    {
        public readonly string Text;
        public readonly int LineNumber;
        public readonly StatementType Type;

        public SourceLine(string Text, int LineNumber)
        {
            this.Text = Text;
            this.LineNumber = LineNumber;
            GetStatementType();
        }

        private void GetStatementType()
        {
            throw new System.NotImplementedException();
        }
    }
}