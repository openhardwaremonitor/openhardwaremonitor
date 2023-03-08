using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
namespace SampleApp
{
    public partial class DataTableTreeExample : UserControl
    {

        private DataTableTreeModel m_dataTableModel;
        public DataTableTreeExample()
        {
            InitializeComponent();
            DataTable table = CreateSampleDataTable();

            this.dataGridView1.Columns.Clear();
            this.dataGridView1.DataSource = table;

            treeViewAdv1.LoadOnDemand = true;
            treeViewAdv1.SelectionMode = TreeSelectionMode.Multi;
            treeViewAdv1.NodeControls.Clear();

            NodeStateIcon ni = new NodeStateIcon();
            ni.DataPropertyName = "Icon";
            treeViewAdv1.NodeControls.Add(ni);

            NodeTextBox tb = new NodeTextBox();
            tb.DataPropertyName = "Text";
            treeViewAdv1.NodeControls.Add(tb);

            
            m_dataTableModel = new DataTableTreeModel(table,"id");
            this.treeViewAdv1.Model = m_dataTableModel;
            this.treeViewAdv1.SelectionChanged += new EventHandler(treeViewAdv1_SelectionChanged);
        }

        void treeViewAdv1_SelectionChanged(object sender, EventArgs e)
        {
            Enabling();                        
            //treeViewAdv1.SelectedNodes
        }

        private void Enabling()
        {
            if (treeViewAdv1.SelectedNodes.Count == 0)
                addNodeToolStripMenuItem.Enabled = false;
            else
            addNodeToolStripMenuItem.Enabled = !treeViewAdv1.SelectedNode.IsLeaf;
        }


        private static DataTable CreateSampleDataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("ParentID", typeof(int));
            table.Columns.Add("IsFolder", typeof(bool));
            table.Columns.Add("Name");
            table.Columns.Add("Tag");

            table.Rows.Add(1, 1, true, "Columbia River ", "set ID = ParentID for root");
            table.Rows.Add(2, 1, false, "John Day", " ");
            table.Rows.Add(3, 1, true, "Snake River", "");
            table.Rows.Add(4, 3, false, "Payette River", "");
            table.Rows.Add(5, 3, false, "Boise River", "");

            return table;
        }

        private void addNode_Click(object sender, 
            EventArgs e)
        {
            if (treeViewAdv1.SelectedNode != null)
            {
                 DataRowNode n = treeViewAdv1.SelectedNode.Tag as DataRowNode;
                 if (!Convert.ToBoolean(n.Row["IsFolder"]))
                 {
                     return;
                 }

             TreePath parent = treeViewAdv1.GetPath(treeViewAdv1.SelectedNode);

             m_dataTableModel.AddChild(parent, "Hi "+DateTime.Today.ToShortDateString());
                
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            Console.WriteLine("Row added");
        }
    }
}
