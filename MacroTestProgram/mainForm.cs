using System;
using System.Windows.Forms;
using HPVariableRepository;
using MacroPLC;

namespace MacroTestProgram
{
    public partial class mainForm : Form
    {
        private MacroCompiler compiler;
        private VariableRepository varDB = new VariableRepository();

        public mainForm()
        {
            InitializeComponent();
            compiler = new MacroCompiler("END;");
            varDB.InitializeVariables();
            varDB.VariableAssigned += VariableDb_OnVariableAssigned;
            varDB.GCodeGenerated += VariableDb_OnGCodeGenerated;
        }

        private string _statement_string;
        private void VariableDb_OnGCodeGenerated(object sender, GCodeStatementArg e)
        {
            _statement_string += e.Statement;
        }

        private void VariableDb_OnVariableAssigned(object sender, VariableArg e)
        {
            var name = e.Name;
            var value = e.Value.Literal;
            _statement_string += string.Format("{0} = {1}", name, value);
        }

        private void buttonCompile_Click(object sender, EventArgs e)
        {
            try
            {
                textBoxExecuteResult.Text = string.Empty;

                var source = textBoxMacro.Text;
                if(string.IsNullOrEmpty(source)) return;

                compiler = new MacroCompiler(source);
                compiler.Compile();

                _executor = new MacroExecutor(compiler.compiledTasks);
                _executor.NotifyStep += executor_step_notify;
                _executor.Variables = varDB;
                
                textBoxExecuteResult.Text = "Compile Succeeded!\r\n";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                textBoxExecuteResult.Text = "Compile Failed!\r\n";
            }
        }

        private void executor_step_notify(object sender, StepExecuteArg stepExecuteArg)
        {
            _step_string += stepExecuteArg.Task;
        }

        private MacroExecutor _executor;
        private int _current_line;
        private string _step_string;
        private void buttonStep_Click(object sender, EventArgs e)
        {
            try
            {
                var step_line = step_execute();
                if(step_line == MacroExecutor.INVALID_LINE_NUMBER)
                {
                    _step_string = "END PROGRAM \r\n";
                    return;
                }

                while (_current_line == step_line)
                    step_line = step_execute();
                _current_line = step_line;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int step_execute()
        {
            _step_string = string.Empty;
            _statement_string = string.Empty;

            var line_num = _executor.StepExecute();
            textBoxExecuteResult.Text += string.Format(" \r\n Line number: {0} {1} {2}",
                                                       line_num, _step_string, _statement_string);
            textBoxExecuteResult.SelectionStart = textBoxExecuteResult.TextLength;
            textBoxExecuteResult.ScrollToCaret();
            return line_num;
        }

        private void buttonExecute_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}