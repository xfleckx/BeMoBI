namespace BeMoBI_Protocol.StepViews
{
    partial class Base
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.isReadyCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // isReadyCheckBox
            // 
            this.isReadyCheckBox.AutoSize = true;
            this.isReadyCheckBox.Location = new System.Drawing.Point(4, 4);
            this.isReadyCheckBox.Name = "isReadyCheckBox";
            this.isReadyCheckBox.Size = new System.Drawing.Size(46, 17);
            this.isReadyCheckBox.TabIndex = 0;
            this.isReadyCheckBox.Text = "Title";
            this.isReadyCheckBox.UseVisualStyleBackColor = true;
            // 
            // Base
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.isReadyCheckBox);
            this.Name = "Base";
            this.Size = new System.Drawing.Size(400, 150);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected internal System.Windows.Forms.CheckBox isReadyCheckBox;
    }
}
