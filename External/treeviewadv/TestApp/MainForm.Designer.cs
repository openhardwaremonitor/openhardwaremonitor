namespace TestApp
{
	partial class MainForm
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.treeViewAdv1 = new Aga.Controls.Tree.TreeViewAdv();
			this.nodeStateIcon1 = new Aga.Controls.Tree.NodeControls.NodeStateIcon();
			this.nodeTextBox1 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// treeViewAdv1
			// 
			this.treeViewAdv1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeViewAdv1.AutoRowHeight = true;
			this.treeViewAdv1.BackColor = System.Drawing.SystemColors.Window;
			this.treeViewAdv1.DefaultToolTipProvider = null;
			this.treeViewAdv1.DragDropMarkColor = System.Drawing.Color.Black;
			this.treeViewAdv1.LineColor = System.Drawing.SystemColors.ControlDark;
			this.treeViewAdv1.Location = new System.Drawing.Point(12, 12);
			this.treeViewAdv1.Model = null;
			this.treeViewAdv1.Name = "treeViewAdv1";
			this.treeViewAdv1.NodeControls.Add(this.nodeStateIcon1);
			this.treeViewAdv1.NodeControls.Add(this.nodeTextBox1);
			this.treeViewAdv1.SelectedNode = null;
			this.treeViewAdv1.SelectionMode = Aga.Controls.Tree.TreeSelectionMode.Multi;
			this.treeViewAdv1.Size = new System.Drawing.Size(301, 205);
			this.treeViewAdv1.TabIndex = 0;
			this.treeViewAdv1.Text = "treeViewAdv1";
			this.treeViewAdv1.SelectionChanged += new System.EventHandler(this.treeViewAdv1_SelectionChanged);
			// 
			// nodeStateIcon1
			// 
			this.nodeStateIcon1.DataPropertyName = "Icon";
			this.nodeStateIcon1.LeftMargin = 1;
			this.nodeStateIcon1.ParentColumn = null;
			this.nodeStateIcon1.ScaleMode = Aga.Controls.Tree.ImageScaleMode.Clip;
			// 
			// nodeTextBox1
			// 
			this.nodeTextBox1.DataPropertyName = "Text";
			this.nodeTextBox1.IncrementalSearchEnabled = true;
			this.nodeTextBox1.LeftMargin = 3;
			this.nodeTextBox1.ParentColumn = null;
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.Location = new System.Drawing.Point(12, 223);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(220, 20);
			this.textBox1.TabIndex = 1;
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = new System.Drawing.Point(238, 223);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(325, 255);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.treeViewAdv1);
			this.Name = "MainForm";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Aga.Controls.Tree.TreeViewAdv treeViewAdv1;
		private System.Windows.Forms.TextBox textBox1;
		private Aga.Controls.Tree.NodeControls.NodeStateIcon nodeStateIcon1;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBox1;
		private System.Windows.Forms.Button button1;
	}
}

