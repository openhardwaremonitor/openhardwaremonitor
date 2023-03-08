namespace SampleApp
{
    partial class DataTableTreeExample
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataTableTreeExample));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeViewAdv1 = new Aga.Controls.Tree.TreeViewAdv();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nodeTextBox1 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
            this.nodeStateIcon1 = new Aga.Controls.Tree.NodeControls.NodeStateIcon();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ParentID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Label = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Data = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeViewAdv1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2.Controls.Add(this.dataGridView1);
            this.splitContainer1.Size = new System.Drawing.Size(830, 528);
            this.splitContainer1.SplitterDistance = 298;
            this.splitContainer1.TabIndex = 0;
            // 
            // treeViewAdv1
            // 
            this.treeViewAdv1.BackColor = System.Drawing.SystemColors.Window;
            this.treeViewAdv1.ContextMenuStrip = this.contextMenuStrip1;
            this.treeViewAdv1.Cursor = System.Windows.Forms.Cursors.Default;
            this.treeViewAdv1.DefaultToolTipProvider = null;
            this.treeViewAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewAdv1.DragDropMarkColor = System.Drawing.Color.Black;
            this.treeViewAdv1.LineColor = System.Drawing.SystemColors.ControlDark;
            this.treeViewAdv1.Location = new System.Drawing.Point(0, 0);
            this.treeViewAdv1.Model = null;
            this.treeViewAdv1.Name = "treeViewAdv1";
            this.treeViewAdv1.NodeControls.Add(this.nodeTextBox1);
            this.treeViewAdv1.NodeControls.Add(this.nodeStateIcon1);
            this.treeViewAdv1.SelectedNode = null;
            this.treeViewAdv1.Size = new System.Drawing.Size(298, 528);
            this.treeViewAdv1.TabIndex = 0;
            this.treeViewAdv1.Text = "treeViewAdv1";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNodeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 48);
            // 
            // addNodeToolStripMenuItem
            // 
            this.addNodeToolStripMenuItem.Name = "addNodeToolStripMenuItem";
            this.addNodeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.addNodeToolStripMenuItem.Text = "Add Node";
            this.addNodeToolStripMenuItem.Click += new System.EventHandler(this.addNode_Click);
            // 
            // nodeTextBox1
            // 
            this.nodeTextBox1.IncrementalSearchEnabled = true;
            this.nodeTextBox1.LeftMargin = 3;
            this.nodeTextBox1.ParentColumn = null;
            // 
            // nodeStateIcon1
            // 
            this.nodeStateIcon1.LeftMargin = 1;
            this.nodeStateIcon1.ParentColumn = null;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 428);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(528, 100);
            this.panel1.TabIndex = 1;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.ParentID,
            this.Label,
            this.Data});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(528, 528);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.dataGridView1_RowsAdded);
            // 
            // ID
            // 
            this.ID.HeaderText = "ID";
            this.ID.Name = "ID";
            // 
            // ParentID
            // 
            this.ParentID.HeaderText = "ParentID";
            this.ParentID.Name = "ParentID";
            // 
            // Label
            // 
            this.Label.HeaderText = "Label";
            this.Label.Name = "Label";
            // 
            // Data
            // 
            this.Data.HeaderText = "Data";
            this.Data.Name = "Data";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "");
            this.imageList1.Images.SetKeyName(2, "");
            this.imageList1.Images.SetKeyName(3, "");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(59, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(174, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "right click on a folder to add a node";
            // 
            // DataTableTreeExample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "DataTableTreeExample";
            this.Size = new System.Drawing.Size(830, 528);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private Aga.Controls.Tree.TreeViewAdv treeViewAdv1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParentID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Label;
        private System.Windows.Forms.DataGridViewTextBoxColumn Data;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem addNodeToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList1;
        private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBox1;
        private Aga.Controls.Tree.NodeControls.NodeStateIcon nodeStateIcon1;
        private System.Windows.Forms.Label label1;


    }
}
