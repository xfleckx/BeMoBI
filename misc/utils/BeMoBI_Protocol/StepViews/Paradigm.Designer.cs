namespace BeMoBI_Protocol.StepViews
{
    partial class Paradigm
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
            this.exeLookUpButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxExePath = new System.Windows.Forms.TextBox();
            this.textBoxArgs = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // exeLookUpButton
            // 
            this.exeLookUpButton.Location = new System.Drawing.Point(361, 38);
            this.exeLookUpButton.Name = "exeLookUpButton";
            this.exeLookUpButton.Size = new System.Drawing.Size(36, 23);
            this.exeLookUpButton.TabIndex = 1;
            this.exeLookUpButton.Text = "...";
            this.exeLookUpButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Executable";
            // 
            // textBoxExePath
            // 
            this.textBoxExePath.Location = new System.Drawing.Point(6, 41);
            this.textBoxExePath.Name = "textBoxExePath";
            this.textBoxExePath.Size = new System.Drawing.Size(349, 20);
            this.textBoxExePath.TabIndex = 3;
            // 
            // textBoxArgs
            // 
            this.textBoxArgs.Location = new System.Drawing.Point(6, 80);
            this.textBoxArgs.Name = "textBoxArgs";
            this.textBoxArgs.Size = new System.Drawing.Size(391, 20);
            this.textBoxArgs.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Startup arguments";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(255, 106);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(142, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Insert a filepath at cursor";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // Paradigm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBoxArgs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxExePath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.exeLookUpButton);
            this.Name = "Paradigm";
            this.Size = new System.Drawing.Size(401, 150);
            this.Controls.SetChildIndex(this.exeLookUpButton, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.textBoxExePath, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.textBoxArgs, 0);
            this.Controls.SetChildIndex(this.button1, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button exeLookUpButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxExePath;
        private System.Windows.Forms.TextBox textBoxArgs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
    }
}
