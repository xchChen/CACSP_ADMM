namespace GenerateInputdata
{
    partial class Visualization
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ADMM_Opt_Btn = new System.Windows.Forms.Button();
            this.Visualization_Btn = new System.Windows.Forms.Button();
            this.Iter_Cbox = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1190, 600);
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            // 
            // ADMM_Opt_Btn
            // 
            this.ADMM_Opt_Btn.Location = new System.Drawing.Point(13, 657);
            this.ADMM_Opt_Btn.Name = "ADMM_Opt_Btn";
            this.ADMM_Opt_Btn.Size = new System.Drawing.Size(137, 25);
            this.ADMM_Opt_Btn.TabIndex = 19;
            this.ADMM_Opt_Btn.Text = "ADMM_Opt";
            this.ADMM_Opt_Btn.UseVisualStyleBackColor = true;
            this.ADMM_Opt_Btn.Click += new System.EventHandler(this.ADMM_Opt_Btn_Click);
            // 
            // Visualization_Btn
            // 
            this.Visualization_Btn.Location = new System.Drawing.Point(930, 657);
            this.Visualization_Btn.Name = "Visualization_Btn";
            this.Visualization_Btn.Size = new System.Drawing.Size(153, 25);
            this.Visualization_Btn.TabIndex = 20;
            this.Visualization_Btn.Text = "Result_Visualization";
            this.Visualization_Btn.UseVisualStyleBackColor = true;
            this.Visualization_Btn.Click += new System.EventHandler(this.Visualization_Btn_Click);
            // 
            // Iter_Cbox
            // 
            this.Iter_Cbox.FormattingEnabled = true;
            this.Iter_Cbox.Location = new System.Drawing.Point(1101, 660);
            this.Iter_Cbox.Name = "Iter_Cbox";
            this.Iter_Cbox.Size = new System.Drawing.Size(112, 21);
            this.Iter_Cbox.TabIndex = 21;
            this.Iter_Cbox.TextChanged += new System.EventHandler(this.Iter_Cbox_TextChanged);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(13, 36);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1200, 615);
            this.panel1.TabIndex = 26;
            // 
            // Visualization
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1224, 690);
            this.Controls.Add(this.Iter_Cbox);
            this.Controls.Add(this.Visualization_Btn);
            this.Controls.Add(this.ADMM_Opt_Btn);
            this.Controls.Add(this.panel1);
            this.Name = "Visualization";
            this.Text = "Visualization";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button ADMM_Opt_Btn;
        private System.Windows.Forms.Button Visualization_Btn;
        private System.Windows.Forms.ComboBox Iter_Cbox;
        private System.Windows.Forms.Panel panel1;
    }
}