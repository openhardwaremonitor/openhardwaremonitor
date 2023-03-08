namespace SampleApp
{
	partial class AdvancedExample
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
			this.textBox1 = new System.Windows.Forms.TextBox();
			this._treeView = new Aga.Controls.Tree.TreeViewAdv();
			this._nodeCheckBox = new Aga.Controls.Tree.NodeControls.NodeCheckBox();
			this._nodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.textBox1.Location = new System.Drawing.Point(0, 0);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(424, 66);
			this.textBox1.TabIndex = 2;
			this.textBox1.Text = "This example demonstrates how to control the visibility of the nodes, and how to " +
				"enable/disable node editing";
			// 
			// _treeView
			// 
			this._treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._treeView.BackColor = System.Drawing.SystemColors.Window;
			this._treeView.Cursor = System.Windows.Forms.Cursors.Default;
			this._treeView.DefaultToolTipProvider = null;
			this._treeView.DragDropMarkColor = System.Drawing.Color.Black;
			this._treeView.LineColor = System.Drawing.SystemColors.ControlDark;
			this._treeView.Location = new System.Drawing.Point(3, 72);
			this._treeView.Model = null;
			this._treeView.Name = "_treeView";
			this._treeView.NodeControls.Add(this._nodeCheckBox);
			this._treeView.NodeControls.Add(this._nodeTextBox);
			this._treeView.SelectedNode = null;
			this._treeView.Size = new System.Drawing.Size(418, 313);
			this._treeView.TabIndex = 0;
			this._treeView.Text = "treeViewAdv1";
			// 
			// _nodeCheckBox
			// 
			this._nodeCheckBox.DataPropertyName = "Checked";
			this._nodeCheckBox.EditEnabled = true;
			this._nodeCheckBox.LeftMargin = 0;
			this._nodeCheckBox.ParentColumn = null;
			// 
			// _nodeTextBox
			// 
			this._nodeTextBox.DataPropertyName = "Text";
			this._nodeTextBox.EditEnabled = true;
			this._nodeTextBox.IncrementalSearchEnabled = true;
			this._nodeTextBox.LeftMargin = 3;
			this._nodeTextBox.ParentColumn = null;
			// 
			// AdvancedExample
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this._treeView);
			this.Name = "AdvancedExample";
			this.Size = new System.Drawing.Size(424, 388);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Aga.Controls.Tree.TreeViewAdv _treeView;
		private System.Windows.Forms.TextBox textBox1;
		private Aga.Controls.Tree.NodeControls.NodeCheckBox _nodeCheckBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox _nodeTextBox;
	}
}
