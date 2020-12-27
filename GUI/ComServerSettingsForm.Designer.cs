namespace OpenHardwareMonitor.GUI
{
    partial class ComServerSettingsForm
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
            this.components = new System.ComponentModel.Container();
            this.portNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.baudTextBox = new System.Windows.Forms.TextBox();
            this.dataBitsTextBox = new System.Windows.Forms.TextBox();
            this.parityDropDown = new System.Windows.Forms.ComboBox();
            this.stopBitsDropDown = new System.Windows.Forms.ComboBox();
            this.comSaveButton = new System.Windows.Forms.Button();
            this.comCancelButton = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.intervalBox = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.intervalBox)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // portNameTextBox
            // 
            this.portNameTextBox.Location = new System.Drawing.Point(155, 49);
            this.portNameTextBox.Name = "portNameTextBox";
            this.portNameTextBox.Size = new System.Drawing.Size(153, 31);
            this.portNameTextBox.TabIndex = 0;
            this.portNameTextBox.Text = "COM1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 97);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "Baud Rate";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(39, 145);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 25);
            this.label3.TabIndex = 3;
            this.label3.Text = "Data Bits";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(40, 192);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 25);
            this.label4.TabIndex = 4;
            this.label4.Text = "Stop Bits";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(71, 240);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 25);
            this.label5.TabIndex = 5;
            this.label5.Text = "Parity";
            // 
            // baudTextBox
            // 
            this.baudTextBox.Location = new System.Drawing.Point(155, 94);
            this.baudTextBox.Name = "baudTextBox";
            this.baudTextBox.Size = new System.Drawing.Size(153, 31);
            this.baudTextBox.TabIndex = 6;
            this.baudTextBox.Text = "9600";
            this.baudTextBox.TextChanged += new System.EventHandler(this.baudTextBox_TextChanged);
            // 
            // dataBitsTextBox
            // 
            this.dataBitsTextBox.Location = new System.Drawing.Point(155, 139);
            this.dataBitsTextBox.Name = "dataBitsTextBox";
            this.dataBitsTextBox.Size = new System.Drawing.Size(153, 31);
            this.dataBitsTextBox.TabIndex = 7;
            this.dataBitsTextBox.Text = "8";
            this.dataBitsTextBox.TextChanged += new System.EventHandler(this.dataBitsTextBox_TextChanged);
            // 
            // parityDropDown
            // 
            this.parityDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.parityDropDown.FormattingEnabled = true;
            this.parityDropDown.Items.AddRange(new object[] {
            "None",
            "Odd",
            "Even",
            "Mark",
            "Space"});
            this.parityDropDown.Location = new System.Drawing.Point(143, 225);
            this.parityDropDown.Name = "parityDropDown";
            this.parityDropDown.Size = new System.Drawing.Size(153, 33);
            this.parityDropDown.TabIndex = 8;
            // 
            // stopBitsDropDown
            // 
            this.stopBitsDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.stopBitsDropDown.FormattingEnabled = true;
            this.stopBitsDropDown.Items.AddRange(new object[] {
            "1",
            "2",
            "1.5"});
            this.stopBitsDropDown.Location = new System.Drawing.Point(143, 177);
            this.stopBitsDropDown.Name = "stopBitsDropDown";
            this.stopBitsDropDown.Size = new System.Drawing.Size(153, 33);
            this.stopBitsDropDown.TabIndex = 9;
            // 
            // comSaveButton
            // 
            this.comSaveButton.Location = new System.Drawing.Point(189, 426);
            this.comSaveButton.Name = "comSaveButton";
            this.comSaveButton.Size = new System.Drawing.Size(119, 54);
            this.comSaveButton.TabIndex = 10;
            this.comSaveButton.Text = "Save";
            this.comSaveButton.UseVisualStyleBackColor = true;
            this.comSaveButton.Click += new System.EventHandler(this.comSaveButton_Click);
            // 
            // comCancelButton
            // 
            this.comCancelButton.Location = new System.Drawing.Point(30, 426);
            this.comCancelButton.Name = "comCancelButton";
            this.comCancelButton.Size = new System.Drawing.Size(119, 54);
            this.comCancelButton.TabIndex = 11;
            this.comCancelButton.Text = "Cancel";
            this.comCancelButton.UseVisualStyleBackColor = true;
            this.comCancelButton.Click += new System.EventHandler(this.comCancelButton_Click);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // intervalBox
            // 
            this.intervalBox.Location = new System.Drawing.Point(143, 46);
            this.intervalBox.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.intervalBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.intervalBox.Name = "intervalBox";
            this.intervalBox.Size = new System.Drawing.Size(153, 31);
            this.intervalBox.TabIndex = 12;
            this.intervalBox.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.parityDropDown);
            this.groupBox1.Controls.Add(this.stopBitsDropDown);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(316, 282);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Serial Port Settings";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(33, 36);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(105, 50);
            this.label6.TabIndex = 14;
            this.label6.Text = "Update\r\nPeriod (s)";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.intervalBox);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Location = new System.Drawing.Point(12, 310);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(316, 100);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Server Settings";
            // 
            // ComServerSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 492);
            this.Controls.Add(this.comCancelButton);
            this.Controls.Add(this.comSaveButton);
            this.Controls.Add(this.dataBitsTextBox);
            this.Controls.Add(this.baudTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.portNameTextBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Name = "ComServerSettingsForm";
            this.Text = "COM Settings";
            this.Load += new System.EventHandler(this.ComServerSettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.intervalBox)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox portNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox baudTextBox;
        private System.Windows.Forms.TextBox dataBitsTextBox;
        private System.Windows.Forms.ComboBox parityDropDown;
        private System.Windows.Forms.ComboBox stopBitsDropDown;
    private System.Windows.Forms.Button comSaveButton;
    private System.Windows.Forms.Button comCancelButton;
    private System.Windows.Forms.ErrorProvider errorProvider1;
    private System.Windows.Forms.NumericUpDown intervalBox;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Label label6;
  }
}
