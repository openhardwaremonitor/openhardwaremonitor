namespace SampleApp
{
	partial class PerformanceTest
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
			this._load = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this._load2 = new System.Windows.Forms.Button();
			this._treeView2 = new System.Windows.Forms.TreeView();
			this.label2 = new System.Windows.Forms.Label();
			this._expand2 = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this._expand = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this._treeView = new Aga.Controls.Tree.TreeViewAdv();
			this.nodeTextBox1 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.SuspendLayout();
			// 
			// _load
			// 
			this._load.Location = new System.Drawing.Point(3, 332);
			this._load.Name = "_load";
			this._load.Size = new System.Drawing.Size(107, 23);
			this._load.TabIndex = 1;
			this._load.Text = "Load";
			this._load.UseVisualStyleBackColor = true;
			this._load.Click += new System.EventHandler(this._load_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(71, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "TreeViewAdv";
			// 
			// _load2
			// 
			this._load2.Location = new System.Drawing.Point(273, 332);
			this._load2.Name = "_load2";
			this._load2.Size = new System.Drawing.Size(107, 23);
			this._load2.TabIndex = 3;
			this._load2.Text = "Load";
			this._load2.UseVisualStyleBackColor = true;
			this._load2.Click += new System.EventHandler(this._load2_Click);
			// 
			// _treeView2
			// 
			this._treeView2.HideSelection = false;
			this._treeView2.Location = new System.Drawing.Point(273, 32);
			this._treeView2.Name = "_treeView2";
			this._treeView2.Size = new System.Drawing.Size(269, 294);
			this._treeView2.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(275, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(130, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Windows.Forms.TreeView";
			// 
			// _expand2
			// 
			this._expand2.Location = new System.Drawing.Point(273, 361);
			this._expand2.Name = "_expand2";
			this._expand2.Size = new System.Drawing.Size(107, 23);
			this._expand2.TabIndex = 6;
			this._expand2.Text = "Expand/Collapse";
			this._expand2.UseVisualStyleBackColor = true;
			this._expand2.Click += new System.EventHandler(this._expand2_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(116, 337);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(35, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "label3";
			// 
			// _expand
			// 
			this._expand.Location = new System.Drawing.Point(3, 361);
			this._expand.Name = "_expand";
			this._expand.Size = new System.Drawing.Size(107, 23);
			this._expand.TabIndex = 9;
			this._expand.Text = "Expand/Collapse";
			this._expand.UseVisualStyleBackColor = true;
			this._expand.Click += new System.EventHandler(this._expand_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(116, 366);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(35, 13);
			this.label4.TabIndex = 10;
			this.label4.Text = "label4";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(386, 337);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(35, 13);
			this.label5.TabIndex = 11;
			this.label5.Text = "label5";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(386, 366);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(35, 13);
			this.label6.TabIndex = 12;
			this.label6.Text = "label6";
			// 
			// _treeView
			// 
			this._treeView.BackColor = System.Drawing.SystemColors.Window;
			this._treeView.Cursor = System.Windows.Forms.Cursors.Default;
			this._treeView.DefaultToolTipProvider = null;
			this._treeView.DragDropMarkColor = System.Drawing.Color.Black;
			this._treeView.LineColor = System.Drawing.SystemColors.ControlDark;
			this._treeView.LoadOnDemand = true;
			this._treeView.Location = new System.Drawing.Point(3, 32);
			this._treeView.Model = null;
			this._treeView.Name = "_treeView";
			this._treeView.NodeControls.Add(this.nodeTextBox1);
			this._treeView.SelectedNode = null;
			this._treeView.SelectionMode = Aga.Controls.Tree.TreeSelectionMode.Multi;
			this._treeView.Size = new System.Drawing.Size(269, 294);
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
			// PerformanceTest
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this._expand);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._expand2);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._treeView2);
			this.Controls.Add(this._load2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._load);
			this.Controls.Add(this._treeView);
			this.Name = "PerformanceTest";
			this.Size = new System.Drawing.Size(598, 488);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Aga.Controls.Tree.TreeViewAdv _treeView;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBox1;
		private System.Windows.Forms.Button _load;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button _load2;
		private System.Windows.Forms.TreeView _treeView2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button _expand2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button _expand;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
	}
}
