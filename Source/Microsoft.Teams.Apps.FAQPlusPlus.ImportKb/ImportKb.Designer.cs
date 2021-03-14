namespace Microsoft.Teams.Apps.FAQPlusPlus.ImportKb
{
    partial class ImportKb
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
            this.btnImport = new System.Windows.Forms.Button();
            this.ofdTsv = new System.Windows.Forms.OpenFileDialog();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.btnSelectKb = new System.Windows.Forms.Button();
            this.tbQuestions = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(591, 58);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(112, 32);
            this.btnImport.TabIndex = 0;
            this.btnImport.Text = "Import KB";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // ofdTsv
            // 
            this.ofdTsv.FileName = "ofdFile";
            // 
            // tbFileName
            // 
            this.tbFileName.Location = new System.Drawing.Point(80, 68);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(341, 22);
            this.tbFileName.TabIndex = 1;
            // 
            // btnSelectKb
            // 
            this.btnSelectKb.Location = new System.Drawing.Point(453, 58);
            this.btnSelectKb.Name = "btnSelectKb";
            this.btnSelectKb.Size = new System.Drawing.Size(106, 32);
            this.btnSelectKb.TabIndex = 2;
            this.btnSelectKb.Text = "Select Kb";
            this.btnSelectKb.UseVisualStyleBackColor = true;
            this.btnSelectKb.Click += new System.EventHandler(this.btnSelectKb_Click);
            // 
            // tbQuestions
            // 
            this.tbQuestions.Location = new System.Drawing.Point(80, 131);
            this.tbQuestions.Multiline = true;
            this.tbQuestions.Name = "tbQuestions";
            this.tbQuestions.Size = new System.Drawing.Size(623, 272);
            this.tbQuestions.TabIndex = 3;
            // 
            // ImportKb
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tbQuestions);
            this.Controls.Add(this.btnSelectKb);
            this.Controls.Add(this.tbFileName);
            this.Controls.Add(this.btnImport);
            this.Name = "ImportKb";
            this.Text = "ImportKb";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.OpenFileDialog ofdTsv;
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.Button btnSelectKb;
        private System.Windows.Forms.TextBox tbQuestions;
    }
}

