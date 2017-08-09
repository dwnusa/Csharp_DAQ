namespace _10_Csharp_DAQ
{
    partial class Form4
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.ep00wire13_0 = new System.Windows.Forms.NumericUpDown();
            this.ep00wire21_14 = new System.Windows.Forms.NumericUpDown();
            this.ep00wire22 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.ep00wire23 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.ep00wire24 = new System.Windows.Forms.CheckBox();
            this.ep00wire25 = new System.Windows.Forms.CheckBox();
            this.ep00wire26 = new System.Windows.Forms.CheckBox();
            this.ep00wire27 = new System.Windows.Forms.CheckBox();
            this.ep00wire28 = new System.Windows.Forms.CheckBox();
            this.ep00wire30 = new System.Windows.Forms.CheckBox();
            this.ep00wire29 = new System.Windows.Forms.CheckBox();
            this.ep00wire31 = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ep00wire13_0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ep00wire21_14)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.ep00wire13_0);
            this.panel1.Controls.Add(this.ep00wire21_14);
            this.panel1.Controls.Add(this.ep00wire22);
            this.panel1.Controls.Add(this.checkBox4);
            this.panel1.Controls.Add(this.checkBox2);
            this.panel1.Controls.Add(this.checkBox3);
            this.panel1.Controls.Add(this.ep00wire23);
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Controls.Add(this.ep00wire24);
            this.panel1.Controls.Add(this.ep00wire25);
            this.panel1.Controls.Add(this.ep00wire26);
            this.panel1.Controls.Add(this.ep00wire27);
            this.panel1.Controls.Add(this.ep00wire28);
            this.panel1.Controls.Add(this.ep00wire30);
            this.panel1.Controls.Add(this.ep00wire29);
            this.panel1.Controls.Add(this.ep00wire31);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(184, 444);
            this.panel1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(75, 263);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Threshold";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(75, 236);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "PeakStretch";
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(13, 318);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(146, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Capture";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(13, 289);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(146, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Apply";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ep00wire13_0
            // 
            this.ep00wire13_0.Location = new System.Drawing.Point(13, 261);
            this.ep00wire13_0.Maximum = new decimal(new int[] {
            8191,
            0,
            0,
            0});
            this.ep00wire13_0.Minimum = new decimal(new int[] {
            8191,
            0,
            0,
            -2147483648});
            this.ep00wire13_0.Name = "ep00wire13_0";
            this.ep00wire13_0.Size = new System.Drawing.Size(59, 21);
            this.ep00wire13_0.TabIndex = 1;
            this.ep00wire13_0.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // ep00wire21_14
            // 
            this.ep00wire21_14.Location = new System.Drawing.Point(13, 234);
            this.ep00wire21_14.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ep00wire21_14.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ep00wire21_14.Name = "ep00wire21_14";
            this.ep00wire21_14.Size = new System.Drawing.Size(59, 21);
            this.ep00wire21_14.TabIndex = 1;
            this.ep00wire21_14.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // ep00wire22
            // 
            this.ep00wire22.AutoSize = true;
            this.ep00wire22.Location = new System.Drawing.Point(13, 211);
            this.ep00wire22.Name = "ep00wire22";
            this.ep00wire22.Size = new System.Drawing.Size(59, 16);
            this.ep00wire22.TabIndex = 0;
            this.ep00wire22.Text = "bit(22)";
            this.ep00wire22.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(98, 369);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(79, 16);
            this.checkBox4.TabIndex = 0;
            this.checkBox4.Text = "ChannelD";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(98, 347);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(79, 16);
            this.checkBox2.TabIndex = 0;
            this.checkBox2.Text = "ChannelB";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(13, 369);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(80, 16);
            this.checkBox3.TabIndex = 0;
            this.checkBox3.Text = "ChannelC";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // ep00wire23
            // 
            this.ep00wire23.AutoSize = true;
            this.ep00wire23.Location = new System.Drawing.Point(13, 189);
            this.ep00wire23.Name = "ep00wire23";
            this.ep00wire23.Size = new System.Drawing.Size(59, 16);
            this.ep00wire23.TabIndex = 0;
            this.ep00wire23.Text = "bit(23)";
            this.ep00wire23.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(13, 347);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(79, 16);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "ChannelA";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // ep00wire24
            // 
            this.ep00wire24.AutoSize = true;
            this.ep00wire24.Location = new System.Drawing.Point(13, 167);
            this.ep00wire24.Name = "ep00wire24";
            this.ep00wire24.Size = new System.Drawing.Size(59, 16);
            this.ep00wire24.TabIndex = 0;
            this.ep00wire24.Text = "bit(24)";
            this.ep00wire24.UseVisualStyleBackColor = true;
            // 
            // ep00wire25
            // 
            this.ep00wire25.AutoSize = true;
            this.ep00wire25.Location = new System.Drawing.Point(13, 145);
            this.ep00wire25.Name = "ep00wire25";
            this.ep00wire25.Size = new System.Drawing.Size(59, 16);
            this.ep00wire25.TabIndex = 0;
            this.ep00wire25.Text = "bit(25)";
            this.ep00wire25.UseVisualStyleBackColor = true;
            // 
            // ep00wire26
            // 
            this.ep00wire26.AutoSize = true;
            this.ep00wire26.Location = new System.Drawing.Point(13, 123);
            this.ep00wire26.Name = "ep00wire26";
            this.ep00wire26.Size = new System.Drawing.Size(59, 16);
            this.ep00wire26.TabIndex = 0;
            this.ep00wire26.Text = "bit(26)";
            this.ep00wire26.UseVisualStyleBackColor = true;
            // 
            // ep00wire27
            // 
            this.ep00wire27.AutoSize = true;
            this.ep00wire27.Location = new System.Drawing.Point(13, 101);
            this.ep00wire27.Name = "ep00wire27";
            this.ep00wire27.Size = new System.Drawing.Size(59, 16);
            this.ep00wire27.TabIndex = 0;
            this.ep00wire27.Text = "bit(27)";
            this.ep00wire27.UseVisualStyleBackColor = true;
            // 
            // ep00wire28
            // 
            this.ep00wire28.AutoSize = true;
            this.ep00wire28.Location = new System.Drawing.Point(13, 79);
            this.ep00wire28.Name = "ep00wire28";
            this.ep00wire28.Size = new System.Drawing.Size(108, 16);
            this.ep00wire28.TabIndex = 0;
            this.ep00wire28.Text = "Pulse Only(28)";
            this.ep00wire28.UseVisualStyleBackColor = true;
            // 
            // ep00wire30
            // 
            this.ep00wire30.AutoSize = true;
            this.ep00wire30.Location = new System.Drawing.Point(13, 35);
            this.ep00wire30.Name = "ep00wire30";
            this.ep00wire30.Size = new System.Drawing.Size(139, 16);
            this.ep00wire30.TabIndex = 0;
            this.ep00wire30.Text = "AutoOffset Reset(30)";
            this.ep00wire30.UseVisualStyleBackColor = true;
            // 
            // ep00wire29
            // 
            this.ep00wire29.AutoSize = true;
            this.ep00wire29.Checked = true;
            this.ep00wire29.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ep00wire29.Location = new System.Drawing.Point(13, 57);
            this.ep00wire29.Name = "ep00wire29";
            this.ep00wire29.Size = new System.Drawing.Size(146, 16);
            this.ep00wire29.TabIndex = 0;
            this.ep00wire29.Text = "AutoOffset Enable(29)";
            this.ep00wire29.UseVisualStyleBackColor = true;
            // 
            // ep00wire31
            // 
            this.ep00wire31.AutoSize = true;
            this.ep00wire31.Checked = true;
            this.ep00wire31.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ep00wire31.Location = new System.Drawing.Point(13, 13);
            this.ep00wire31.Name = "ep00wire31";
            this.ep00wire31.Size = new System.Drawing.Size(134, 16);
            this.ep00wire31.TabIndex = 0;
            this.ep00wire31.Text = "Simulation Test(31)";
            this.ep00wire31.UseVisualStyleBackColor = true;
            // 
            // Form4
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(208, 468);
            this.Controls.Add(this.panel1);
            this.Name = "Form4";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form4";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form4_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ep00wire13_0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ep00wire21_14)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.NumericUpDown ep00wire13_0;
        private System.Windows.Forms.NumericUpDown ep00wire21_14;
        private System.Windows.Forms.CheckBox ep00wire22;
        private System.Windows.Forms.CheckBox ep00wire23;
        private System.Windows.Forms.CheckBox ep00wire24;
        private System.Windows.Forms.CheckBox ep00wire25;
        private System.Windows.Forms.CheckBox ep00wire26;
        private System.Windows.Forms.CheckBox ep00wire27;
        private System.Windows.Forms.CheckBox ep00wire28;
        private System.Windows.Forms.CheckBox ep00wire30;
        private System.Windows.Forms.CheckBox ep00wire29;
        private System.Windows.Forms.CheckBox ep00wire31;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}