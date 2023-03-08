namespace SampleApp
{
	partial class BackgroundExpand
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
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this._treeView = new Aga.Controls.Tree.TreeViewAdv();
			this.nodeTextBox1 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.expandingIcon1 = new Aga.Controls.Tree.NodeControls.ExpandingIcon();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(210, 3);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(119, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "Expand one by one";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(3, 3);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 3;
			this.button2.Text = "Reset";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// _treeView
			// 
			this._treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._treeView.AsyncExpanding = true;
			this._treeView.BackColor = System.Drawing.Color.White;
			this._treeView.DefaultToolTipProvider = null;
			this._treeView.DragDropMarkColor = System.Drawing.Color.Black;
			this._treeView.LineColor = System.Drawing.SystemColors.ControlDark;
			this._treeView.LoadOnDemand = true;
			this._treeView.Location = new System.Drawing.Point(3, 32);
			this._treeView.Model = null;
			this._treeView.Name = "_treeView";
			this._treeView.NodeControls.Add(this.nodeTextBox1);
			this._treeView.NodeControls.Add(this.expandingIcon1);
			this._treeView.SelectedNode = null;
			this._treeView.Size = new System.Drawing.Size(451, 369);
			this._treeView.TabIndex = 0;
			this._treeView.Text = "treeViewAdv1";
			// 
			// nodeTextBox1
			// 
			this.nodeTextBox1.DataPropertyName = "Text";
			this.nodeTextBox1.IncrementalSearchEnabled = true;
			this.nodeTextBox1.LeftMargin = 3;
			this.nodeTextBox1.ParentColumn = null;
			// 
			// expandingIcon1
			// 
			this.expandingIcon1.LeftMargin = 0;
			this.expandingIcon1.ParentColumn = null;
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(335, 3);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(119, 23);
			this.button3.TabIndex = 4;
			this.button3.Text = "Expand root nodes";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(84, 3);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(75, 23);
			this.button4.TabIndex = 5;
			this.button4.Text = "Cancel";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// BackgroundExpand
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this._treeView);
			this.Name = "BackgroundExpand";
			this.Size = new System.Drawing.Size(457, 404);
			this.ResumeLayout(false);

		}

		#endregion

		private Aga.Controls.Tree.TreeViewAdv _treeView;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private Aga.Controls.Tree.NodeControls.ExpandingIcon expandingIcon1;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
	}
}
