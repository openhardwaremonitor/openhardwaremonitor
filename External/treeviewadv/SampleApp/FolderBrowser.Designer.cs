namespace SampleApp
{
    partial class FolderBrowser
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
			this.cboxGrid = new System.Windows.Forms.ComboBox();
			this.cbLines = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this._treeView = new Aga.Controls.Tree.TreeViewAdv();
			this.treeColumn1 = new Aga.Controls.Tree.TreeColumn();
			this.treeColumn2 = new Aga.Controls.Tree.TreeColumn();
			this.treeColumn3 = new Aga.Controls.Tree.TreeColumn();
			this.nodeCheckBox1 = new Aga.Controls.Tree.NodeControls.NodeCheckBox();
			this._icon = new Aga.Controls.Tree.NodeControls.NodeStateIcon();
			this._name = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this._size = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this._date = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.SuspendLayout();
			// 
			// cboxGrid
			// 
			this.cboxGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cboxGrid.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboxGrid.FormattingEnabled = true;
			this.cboxGrid.Location = new System.Drawing.Point(338, 303);
			this.cboxGrid.Name = "cboxGrid";
			this.cboxGrid.Size = new System.Drawing.Size(192, 21);
			this.cboxGrid.TabIndex = 1;
			this.cboxGrid.SelectedIndexChanged += new System.EventHandler(this.cboxGrid_SelectedIndexChanged);
			// 
			// cbLines
			// 
			this.cbLines.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cbLines.AutoSize = true;
			this.cbLines.Location = new System.Drawing.Point(3, 305);
			this.cbLines.Name = "cbLines";
			this.cbLines.Size = new System.Drawing.Size(81, 17);
			this.cbLines.TabIndex = 3;
			this.cbLines.Text = "Show Lines";
			this.cbLines.UseVisualStyleBackColor = true;
			this.cbLines.CheckedChanged += new System.EventHandler(this.cbLines_CheckedChanged);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(257, 306);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Grid Line Style";
			// 
			// _treeView
			// 
			this._treeView.AllowColumnReorder = true;
			this._treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._treeView.AutoRowHeight = true;
			this._treeView.BackColor = System.Drawing.SystemColors.Window;
			this._treeView.ColumnHeaderHeight = 21;
			this._treeView.Columns.Add(this.treeColumn1);
			this._treeView.Columns.Add(this.treeColumn2);
			this._treeView.Columns.Add(this.treeColumn3);
			this._treeView.Cursor = System.Windows.Forms.Cursors.Default;
			this._treeView.DefaultToolTipProvider = null;
			this._treeView.DragDropMarkColor = System.Drawing.Color.Black;
			this._treeView.FullRowSelect = true;
			this._treeView.GridLineStyle = ((Aga.Controls.Tree.GridLineStyle)((Aga.Controls.Tree.GridLineStyle.Horizontal | Aga.Controls.Tree.GridLineStyle.Vertical)));
			this._treeView.LineColor = System.Drawing.SystemColors.ControlDark;
			this._treeView.LoadOnDemand = true;
			this._treeView.Location = new System.Drawing.Point(0, 0);
			this._treeView.Model = null;
			this._treeView.Name = "_treeView";
			this._treeView.NodeControls.Add(this.nodeCheckBox1);
			this._treeView.NodeControls.Add(this._icon);
			this._treeView.NodeControls.Add(this._name);
			this._treeView.NodeControls.Add(this._size);
			this._treeView.NodeControls.Add(this._date);
			this._treeView.SelectedNode = null;
			this._treeView.ShowNodeToolTips = true;
			this._treeView.Size = new System.Drawing.Size(533, 298);
			this._treeView.TabIndex = 0;
			this._treeView.UseColumns = true;
			this._treeView.NodeMouseDoubleClick += new System.EventHandler<Aga.Controls.Tree.TreeNodeAdvMouseEventArgs>(this._treeView_NodeMouseDoubleClick);
			this._treeView.ColumnClicked += new System.EventHandler<Aga.Controls.Tree.TreeColumnEventArgs>(this._treeView_ColumnClicked);
			this._treeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this._treeView_MouseClick);
			// 
			// treeColumn1
			// 
			this.treeColumn1.Header = "Name";
			this.treeColumn1.SortOrder = System.Windows.Forms.SortOrder.None;
			this.treeColumn1.TooltipText = "File name";
			this.treeColumn1.Width = 250;
			// 
			// treeColumn2
			// 
			this.treeColumn2.Header = "Size";
			this.treeColumn2.SortOrder = System.Windows.Forms.SortOrder.None;
			this.treeColumn2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.treeColumn2.TooltipText = "File size";
			this.treeColumn2.Width = 100;
			// 
			// treeColumn3
			// 
			this.treeColumn3.Header = "Date";
			this.treeColumn3.SortOrder = System.Windows.Forms.SortOrder.None;
			this.treeColumn3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.treeColumn3.TooltipText = "File date";
			this.treeColumn3.Width = 150;
			// 
			// nodeCheckBox1
			// 
			this.nodeCheckBox1.DataPropertyName = "IsChecked";
			this.nodeCheckBox1.LeftMargin = 0;
			this.nodeCheckBox1.ParentColumn = this.treeColumn1;
			// 
			// _icon
			// 
			this._icon.DataPropertyName = "Icon";
			this._icon.LeftMargin = 1;
			this._icon.ParentColumn = this.treeColumn1;
			this._icon.ScaleMode = Aga.Controls.Tree.ImageScaleMode.Clip;
			// 
			// _name
			// 
			this._name.DataPropertyName = "Name";
			this._name.IncrementalSearchEnabled = true;
			this._name.LeftMargin = 3;
			this._name.ParentColumn = this.treeColumn1;
			this._name.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			this._name.UseCompatibleTextRendering = true;
			// 
			// _size
			// 
			this._size.DataPropertyName = "Size";
			this._size.IncrementalSearchEnabled = true;
			this._size.LeftMargin = 3;
			this._size.ParentColumn = this.treeColumn2;
			this._size.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// _date
			// 
			this._date.DataPropertyName = "Date";
			this._date.IncrementalSearchEnabled = true;
			this._date.LeftMargin = 3;
			this._date.ParentColumn = this.treeColumn3;
			this._date.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// FolderBrowser
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cbLines);
			this.Controls.Add(this.cboxGrid);
			this.Controls.Add(this._treeView);
			this.Name = "FolderBrowser";
			this.Size = new System.Drawing.Size(533, 327);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Aga.Controls.Tree.TreeViewAdv _treeView;
        private Aga.Controls.Tree.NodeControls.NodeStateIcon _icon;
        private Aga.Controls.Tree.NodeControls.NodeTextBox _name;
        private Aga.Controls.Tree.NodeControls.NodeTextBox _size;
        private Aga.Controls.Tree.NodeControls.NodeTextBox _date;
        private Aga.Controls.Tree.NodeControls.NodeCheckBox nodeCheckBox1;
        private Aga.Controls.Tree.TreeColumn treeColumn1;
        private Aga.Controls.Tree.TreeColumn treeColumn2;
        private Aga.Controls.Tree.TreeColumn treeColumn3;
		private System.Windows.Forms.ComboBox cboxGrid;
        private System.Windows.Forms.CheckBox cbLines;
		private System.Windows.Forms.Label label1;
    }
}
