namespace MacroTestProgram
{
    partial class mainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxExecuteResult = new System.Windows.Forms.TextBox();
            this.buttonStep = new System.Windows.Forms.Button();
            this.buttonCompile = new System.Windows.Forms.Button();
            this.buttonExecute = new System.Windows.Forms.Button();
            this.textBoxMacro = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxExecuteResult
            // 
            this.textBoxExecuteResult.Dock = System.Windows.Forms.DockStyle.Right;
            this.textBoxExecuteResult.Location = new System.Drawing.Point(468, 0);
            this.textBoxExecuteResult.Multiline = true;
            this.textBoxExecuteResult.Name = "textBoxExecuteResult";
            this.textBoxExecuteResult.ReadOnly = true;
            this.textBoxExecuteResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxExecuteResult.Size = new System.Drawing.Size(380, 455);
            this.textBoxExecuteResult.TabIndex = 1;
            // 
            // buttonStep
            // 
            this.buttonStep.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonStep.Location = new System.Drawing.Point(387, 3);
            this.buttonStep.Name = "buttonStep";
            this.buttonStep.Size = new System.Drawing.Size(72, 20);
            this.buttonStep.TabIndex = 2;
            this.buttonStep.Text = "Step";
            this.buttonStep.Click += new System.EventHandler(this.buttonStep_Click);
            // 
            // buttonCompile
            // 
            this.buttonCompile.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonCompile.Location = new System.Drawing.Point(387, 55);
            this.buttonCompile.Name = "buttonCompile";
            this.buttonCompile.Size = new System.Drawing.Size(72, 20);
            this.buttonCompile.TabIndex = 3;
            this.buttonCompile.Text = "Compile";
            this.buttonCompile.Click += new System.EventHandler(this.buttonCompile_Click);
            // 
            // buttonExecute
            // 
            this.buttonExecute.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonExecute.Enabled = false;
            this.buttonExecute.Location = new System.Drawing.Point(387, 29);
            this.buttonExecute.Name = "buttonExecute";
            this.buttonExecute.Size = new System.Drawing.Size(72, 20);
            this.buttonExecute.TabIndex = 4;
            this.buttonExecute.Text = "Execute";
            this.buttonExecute.Click += new System.EventHandler(this.buttonExecute_Click);
            // 
            // textBoxMacro
            // 
            this.textBoxMacro.Dock = System.Windows.Forms.DockStyle.Left;
            this.textBoxMacro.Location = new System.Drawing.Point(0, 0);
            this.textBoxMacro.Multiline = true;
            this.textBoxMacro.Name = "textBoxMacro";
            this.textBoxMacro.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxMacro.Size = new System.Drawing.Size(374, 455);
            this.textBoxMacro.TabIndex = 0;
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(848, 455);
            this.Controls.Add(this.textBoxMacro);
            this.Controls.Add(this.buttonExecute);
            this.Controls.Add(this.buttonCompile);
            this.Controls.Add(this.buttonStep);
            this.Controls.Add(this.textBoxExecuteResult);
            this.Name = "mainForm";
            this.Text = "MacroTest";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxExecuteResult;
        private System.Windows.Forms.Button buttonStep;
        private System.Windows.Forms.Button buttonCompile;
        private System.Windows.Forms.Button buttonExecute;
        private System.Windows.Forms.TextBox textBoxMacro;
    }
}

