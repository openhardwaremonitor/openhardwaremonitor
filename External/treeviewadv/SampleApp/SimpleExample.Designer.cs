namespace SampleApp
{
	partial class SimpleExample
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
			this.components = new System.ComponentModel.Container();
			this._addRoot = new System.Windows.Forms.Button();
			this._clear = new System.Windows.Forms.Button();
			this._addChild = new System.Windows.Forms.Button();
			this._deleteNode = new System.Windows.Forms.Button();
			this._timer = new System.Windows.Forms.Timer(this.components);
			this._autoRowHeight = new System.Windows.Forms.CheckBox();
			this._fontSize = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this._performanceTest = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.btnExpNode = new System.Windows.Forms.Button();
			this.btnExpNodes = new System.Windows.Forms.Button();
			this.btnCollNode = new System.Windows.Forms.Button();
			this.btnCollNodes = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this._tree2 = new Aga.Controls.Tree.TreeViewAdv();
			this.nodeTextBox1 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this._tree = new Aga.Controls.Tree.TreeViewAdv();
			this._nodeCheckBox = new Aga.Controls.Tree.NodeControls.NodeCheckBox();
			this._nodeStateIcon = new Aga.Controls.Tree.NodeControls.NodeStateIcon();
			this._nodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			((System.ComponentModel.ISupportInitialize)(this._fontSize)).BeginInit();
			this.SuspendLayout();
			// 
			// _addRoot
			// 
			this._addRoot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._addRoot.Location = new System.Drawing.Point(381, 3);
			this._addRoot.Name = "_addRoot";
			this._addRoot.Size = new System.Drawing.Size(91, 23);
			this._addRoot.TabIndex = 1;
			this._addRoot.Text = "Add Root";
			this._addRoot.UseVisualStyleBackColor = true;
			this._addRoot.Click += new System.EventHandler(this.AddRootClick);
			// 
			// _clear
			// 
			this._clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._clear.Location = new System.Drawing.Point(381, 90);
			this._clear.Name = "_clear";
			this._clear.Size = new System.Drawing.Size(91, 23);
			this._clear.TabIndex = 2;
			this._clear.Text = "Clear Tree";
			this._clear.UseVisualStyleBackColor = true;
			this._clear.Click += new System.EventHandler(this.ClearClick);
			// 
			// _addChild
			// 
			this._addChild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._addChild.Location = new System.Drawing.Point(381, 32);
			this._addChild.Name = "_addChild";
			this._addChild.Size = new System.Drawing.Size(91, 23);
			this._addChild.TabIndex = 3;
			this._addChild.Text = "Add Child";
			this._addChild.UseVisualStyleBackColor = true;
			this._addChild.Click += new System.EventHandler(this.AddChildClick);
			// 
			// _deleteNode
			// 
			this._deleteNode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._deleteNode.Location = new System.Drawing.Point(381, 61);
			this._deleteNode.Name = "_deleteNode";
			this._deleteNode.Size = new System.Drawing.Size(91, 23);
			this._deleteNode.TabIndex = 5;
			this._deleteNode.Text = "Delete Node";
			this._deleteNode.UseVisualStyleBackColor = true;
			this._deleteNode.Click += new System.EventHandler(this.DeleteClick);
			// 
			// _timer
			// 
			this._timer.Interval = 1;
			this._timer.Tick += new System.EventHandler(this._timer_Tick);
			// 
			// _autoRowHeight
			// 
			this._autoRowHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._autoRowHeight.AutoSize = true;
			this._autoRowHeight.Checked = true;
			this._autoRowHeight.CheckState = System.Windows.Forms.CheckState.Checked;
			this._autoRowHeight.Location = new System.Drawing.Point(450, 336);
			this._autoRowHeight.Name = "_autoRowHeight";
			this._autoRowHeight.Size = new System.Drawing.Size(101, 17);
			this._autoRowHeight.TabIndex = 7;
			this._autoRowHeight.Text = "&AutoRowHeight";
			this._autoRowHeight.UseVisualStyleBackColor = true;
			this._autoRowHeight.Click += new System.EventHandler(this._autoRowHeight_Click);
			// 
			// _fontSize
			// 
			this._fontSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._fontSize.Location = new System.Drawing.Point(510, 310);
			this._fontSize.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
			this._fontSize.Minimum = new decimal(new int[] {
            9,
            0,
            0,
            0});
			this._fontSize.Name = "_fontSize";
			this._fontSize.Size = new System.Drawing.Size(48, 20);
			this._fontSize.TabIndex = 8;
			this._fontSize.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
			this._fontSize.ValueChanged += new System.EventHandler(this._fontSize_ValueChanged);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(450, 312);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(54, 13);
			this.label1.TabIndex = 9;
			this.label1.Text = "Font Size:";
			// 
			// _performanceTest
			// 
			this._performanceTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._performanceTest.AutoSize = true;
			this._performanceTest.Location = new System.Drawing.Point(450, 359);
			this._performanceTest.Name = "_performanceTest";
			this._performanceTest.Size = new System.Drawing.Size(109, 17);
			this._performanceTest.TabIndex = 10;
			this._performanceTest.Text = "Measure Perform.";
			this._performanceTest.UseVisualStyleBackColor = true;
			this._performanceTest.Click += new System.EventHandler(this._performanceTest_Click);
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = new System.Drawing.Point(381, 148);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(91, 23);
			this.button1.TabIndex = 12;
			this.button1.Text = "Refresh";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.Location = new System.Drawing.Point(381, 119);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(91, 23);
			this.button2.TabIndex = 13;
			this.button2.Text = "Expand/Collapse";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// btnExpNode
			// 
			this.btnExpNode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExpNode.Location = new System.Drawing.Point(478, 3);
			this.btnExpNode.Name = "btnExpNode";
			this.btnExpNode.Size = new System.Drawing.Size(91, 23);
			this.btnExpNode.TabIndex = 14;
			this.btnExpNode.Text = "Expand Node";
			this.btnExpNode.UseVisualStyleBackColor = true;
			this.btnExpNode.Click += new System.EventHandler(this.btnExpNode_Click);
			// 
			// btnExpNodes
			// 
			this.btnExpNodes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExpNodes.Location = new System.Drawing.Point(478, 32);
			this.btnExpNodes.Name = "btnExpNodes";
			this.btnExpNodes.Size = new System.Drawing.Size(91, 23);
			this.btnExpNodes.TabIndex = 15;
			this.btnExpNodes.Text = "Expand Nodes";
			this.btnExpNodes.UseVisualStyleBackColor = true;
			this.btnExpNodes.Click += new System.EventHandler(this.btnExpNodes_Click);
			// 
			// btnCollNode
			// 
			this.btnCollNode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCollNode.Location = new System.Drawing.Point(478, 61);
			this.btnCollNode.Name = "btnCollNode";
			this.btnCollNode.Size = new System.Drawing.Size(91, 23);
			this.btnCollNode.TabIndex = 18;
			this.btnCollNode.Text = "Collapse Node";
			this.btnCollNode.UseVisualStyleBackColor = true;
			this.btnCollNode.Click += new System.EventHandler(this.btnCollNode_Click);
			// 
			// btnCollNodes
			// 
			this.btnCollNodes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCollNodes.Location = new System.Drawing.Point(478, 90);
			this.btnCollNodes.Name = "btnCollNodes";
			this.btnCollNodes.Size = new System.Drawing.Size(91, 23);
			this.btnCollNodes.TabIndex = 19;
			this.btnCollNodes.Text = "Collapse Nodes";
			this.btnCollNodes.UseVisualStyleBackColor = true;
			this.btnCollNodes.Click += new System.EventHandler(this.btnCollNodes_Click);
			// 
			// button3
			// 
			this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button3.Location = new System.Drawing.Point(478, 119);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(91, 23);
			this.button3.TabIndex = 21;
			this.button3.Text = "Clear Selection";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// _tree2
			// 
			this._tree2.AllowDrop = true;
			this._tree2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._tree2.BackColor = System.Drawing.SystemColors.Window;
			this._tree2.Cursor = System.Windows.Forms.Cursors.Default;
			this._tree2.DefaultToolTipProvider = null;
			this._tree2.DisplayDraggingNodes = true;
			this._tree2.DragDropMarkColor = System.Drawing.Color.Black;
			this._tree2.LineColor = System.Drawing.SystemColors.ControlDark;
			this._tree2.LoadOnDemand = true;
			this._tree2.Location = new System.Drawing.Point(0, 229);
			this._tree2.Model = null;
			this._tree2.Name = "_tree2";
			this._tree2.NodeControls.Add(this.nodeTextBox1);
			this._tree2.SelectedNode = null;
			this._tree2.Size = new System.Drawing.Size(375, 155);
			this._tree2.TabIndex = 11;
			this._tree2.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this._tree2_ItemDrag);
			// 
			// nodeTextBox1
			// 
			this.nodeTextBox1.DataPropertyName = "Text";
			this.nodeTextBox1.IncrementalSearchEnabled = true;
			this.nodeTextBox1.LeftMargin = 3;
			this.nodeTextBox1.ParentColumn = null;
			// 
			// _tree
			// 
			this._tree.AllowDrop = true;
			this._tree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._tree.AutoRowHeight = true;
			this._tree.BackColor = System.Drawing.SystemColors.Window;
			this._tree.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this._tree.Cursor = System.Windows.Forms.Cursors.Default;
			this._tree.DefaultToolTipProvider = null;
			this._tree.DisplayDraggingNodes = true;
			this._tree.DragDropMarkColor = System.Drawing.Color.Black;
			this._tree.LineColor = System.Drawing.SystemColors.ControlDark;
			this._tree.LoadOnDemand = true;
			this._tree.Location = new System.Drawing.Point(0, 0);
			this._tree.Model = null;
			this._tree.Name = "_tree";
			this._tree.NodeControls.Add(this._nodeCheckBox);
			this._tree.NodeControls.Add(this._nodeStateIcon);
			this._tree.NodeControls.Add(this._nodeTextBox);
			this._tree.SelectedNode = null;
			this._tree.SelectionMode = Aga.Controls.Tree.TreeSelectionMode.MultiSameParent;
			this._tree.ShowNodeToolTips = true;
			this._tree.Size = new System.Drawing.Size(375, 223);
			this._tree.TabIndex = 0;
			this._tree.NodeMouseDoubleClick += new System.EventHandler<Aga.Controls.Tree.TreeNodeAdvMouseEventArgs>(this._tree_NodeMouseDoubleClick);
			this._tree.SelectionChanged += new System.EventHandler(this._tree_SelectionChanged);
			this._tree.DragOver += new System.Windows.Forms.DragEventHandler(this._tree_DragOver);
			this._tree.DragDrop += new System.Windows.Forms.DragEventHandler(this._tree_DragDrop);
			this._tree.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this._tree_ItemDrag);
			// 
			// _nodeCheckBox
			// 
			this._nodeCheckBox.DataPropertyName = "CheckState";
			this._nodeCheckBox.EditEnabled = true;
			this._nodeCheckBox.LeftMargin = 0;
			this._nodeCheckBox.ParentColumn = null;
			this._nodeCheckBox.ThreeState = true;
			// 
			// _nodeStateIcon
			// 
			this._nodeStateIcon.LeftMargin = 1;
			this._nodeStateIcon.ParentColumn = null;
			// 
			// _nodeTextBox
			// 
			this._nodeTextBox.DataPropertyName = "Text";
			this._nodeTextBox.EditEnabled = true;
			this._nodeTextBox.IncrementalSearchEnabled = true;
			this._nodeTextBox.LeftMargin = 3;
			this._nodeTextBox.ParentColumn = null;
			// 
			// SimpleExample
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.button3);
			this.Controls.Add(this.btnCollNodes);
			this.Controls.Add(this.btnCollNode);
			this.Controls.Add(this.btnExpNodes);
			this.Controls.Add(this.btnExpNode);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this._tree2);
			this.Controls.Add(this._performanceTest);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._fontSize);
			this.Controls.Add(this._autoRowHeight);
			this.Controls.Add(this._deleteNode);
			this.Controls.Add(this._addChild);
			this.Controls.Add(this._clear);
			this.Controls.Add(this._addRoot);
			this.Controls.Add(this._tree);
			this.Name = "SimpleExample";
			this.Size = new System.Drawing.Size(572, 387);
			((System.ComponentModel.ISupportInitialize)(this._fontSize)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Aga.Controls.Tree.TreeViewAdv _tree;
		private System.Windows.Forms.Button _addRoot;
		private System.Windows.Forms.Button _clear;
		private Aga.Controls.Tree.NodeControls.NodeCheckBox _nodeCheckBox;
		private System.Windows.Forms.Button _addChild;
		private System.Windows.Forms.Button _deleteNode;
		private Aga.Controls.Tree.NodeControls.NodeStateIcon _nodeStateIcon;
		private Aga.Controls.Tree.NodeControls.NodeTextBox _nodeTextBox;
		private System.Windows.Forms.Timer _timer;
		private System.Windows.Forms.CheckBox _autoRowHeight;
		private System.Windows.Forms.NumericUpDown _fontSize;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox _performanceTest;
		private Aga.Controls.Tree.TreeViewAdv _tree2;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnExpNode;
		private System.Windows.Forms.Button btnExpNodes;
        private System.Windows.Forms.Button btnCollNode;
		private System.Windows.Forms.Button btnCollNodes;
		private System.Windows.Forms.Button button3;
	}
}
