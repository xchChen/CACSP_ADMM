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
            this.Generate_Input_File_Btn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Arrival_Gate_Num_TBox = new System.Windows.Forms.TextBox();
            this.Departure_Gate_Num_TBox = new System.Windows.Forms.TextBox();
            this.Container_Bay_Num_TBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Crane_Num_TBox = new System.Windows.Forms.TextBox();
            this.TimeHorizion_TBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ADMM_Opt_Btn = new System.Windows.Forms.Button();
            this.Visualization_Btn = new System.Windows.Forms.Button();
            this.Iter_Cbox = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Generate_Input_File_Btn
            // 
            this.Generate_Input_File_Btn.Location = new System.Drawing.Point(1065, 10);
            this.Generate_Input_File_Btn.Name = "Generate_Input_File_Btn";
            this.Generate_Input_File_Btn.Size = new System.Drawing.Size(148, 25);
            this.Generate_Input_File_Btn.TabIndex = 0;
            this.Generate_Input_File_Btn.Text = "Generate Input File";
            this.Generate_Input_File_Btn.UseVisualStyleBackColor = true;
            this.Generate_Input_File_Btn.Click += new System.EventHandler(this.Generate_Input_File_Btn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Arrival Gate #";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(226, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Departure Gate #";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(448, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Container Slot #";
            // 
            // Arrival_Gate_Num_TBox
            // 
            this.Arrival_Gate_Num_TBox.Location = new System.Drawing.Point(119, 16);
            this.Arrival_Gate_Num_TBox.Name = "Arrival_Gate_Num_TBox";
            this.Arrival_Gate_Num_TBox.Size = new System.Drawing.Size(100, 20);
            this.Arrival_Gate_Num_TBox.TabIndex = 4;
            // 
            // Departure_Gate_Num_TBox
            // 
            this.Departure_Gate_Num_TBox.Location = new System.Drawing.Point(342, 16);
            this.Departure_Gate_Num_TBox.Name = "Departure_Gate_Num_TBox";
            this.Departure_Gate_Num_TBox.Size = new System.Drawing.Size(100, 20);
            this.Departure_Gate_Num_TBox.TabIndex = 5;
            // 
            // Container_Bay_Num_TBox
            // 
            this.Container_Bay_Num_TBox.Location = new System.Drawing.Point(561, 13);
            this.Container_Bay_Num_TBox.Name = "Container_Bay_Num_TBox";
            this.Container_Bay_Num_TBox.Size = new System.Drawing.Size(100, 20);
            this.Container_Bay_Num_TBox.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(667, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Crane #";
            // 
            // Crane_Num_TBox
            // 
            this.Crane_Num_TBox.Location = new System.Drawing.Point(746, 13);
            this.Crane_Num_TBox.Name = "Crane_Num_TBox";
            this.Crane_Num_TBox.Size = new System.Drawing.Size(100, 20);
            this.Crane_Num_TBox.TabIndex = 8;
            // 
            // TimeHorizion_TBox
            // 
            this.TimeHorizion_TBox.Location = new System.Drawing.Point(981, 13);
            this.TimeHorizion_TBox.Name = "TimeHorizion_TBox";
            this.TimeHorizion_TBox.Size = new System.Drawing.Size(69, 20);
            this.TimeHorizion_TBox.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(852, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(123, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Planning Horizon Length";
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
            this.Controls.Add(this.TimeHorizion_TBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.Crane_Num_TBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Container_Bay_Num_TBox);
            this.Controls.Add(this.Departure_Gate_Num_TBox);
            this.Controls.Add(this.Arrival_Gate_Num_TBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Generate_Input_File_Btn);
            this.Controls.Add(this.panel1);
            this.Name = "Visualization";
            this.Text = "Visualization";
            this.Load += new System.EventHandler(this.STN_Visulization_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Generate_Input_File_Btn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox Arrival_Gate_Num_TBox;
        private System.Windows.Forms.TextBox Departure_Gate_Num_TBox;
        private System.Windows.Forms.TextBox Container_Bay_Num_TBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox Crane_Num_TBox;
        private System.Windows.Forms.TextBox TimeHorizion_TBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button ADMM_Opt_Btn;
        private System.Windows.Forms.Button Visualization_Btn;
        private System.Windows.Forms.ComboBox Iter_Cbox;
        private System.Windows.Forms.Panel panel1;
    }
}