namespace GenerateInputdata
{
    partial class ADMM_OPT_Frm
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
            this.ADMM_OPT_Btn = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // ADMM_OPT_Btn
            // 
            this.ADMM_OPT_Btn.Location = new System.Drawing.Point(13, 14);
            this.ADMM_OPT_Btn.Name = "ADMM_OPT_Btn";
            this.ADMM_OPT_Btn.Size = new System.Drawing.Size(118, 25);
            this.ADMM_OPT_Btn.TabIndex = 0;
            this.ADMM_OPT_Btn.Text = "ADMM-based Opt";
            this.ADMM_OPT_Btn.UseVisualStyleBackColor = true;
            this.ADMM_OPT_Btn.Click += new System.EventHandler(this.ADMM_OPT_Btn_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(13, 46);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(713, 352);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = " ";
            // 
            // ADMM_OPT_Frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(738, 411);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.ADMM_OPT_Btn);
            this.Name = "ADMM_OPT_Frm";
            this.Text = "ADMM_OPT_Frm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ADMM_OPT_Btn;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}