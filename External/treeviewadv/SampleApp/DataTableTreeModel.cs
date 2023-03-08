using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Aga.Controls.Tree;
using System.Drawing;
namespace SampleApp
{
    /// <summary>
    /// Using a System.Data.DataTable to represent a tree structure 
    /// </summary>
	public class DataTableTreeModel :  TreeModelBase
	{
        private DataRowNode m_root;

        DataTable m_table;
        string m_IDColumnName;

		public DataTableTreeModel(DataTable table, string idColumnName)
		{
            m_table = table;
            m_IDColumnName = idColumnName;
            DataRow[] rows = table.Select(m_IDColumnName+" = ParentID");
            if( rows.Length ==0 )
            {
                throw new Exception("DataTableModel Requires a root Node");
            }
            m_root = new DataRowNode(rows[0],rows[0]["Name"].ToString());
            m_root.Row = rows[0];
		}

        public override System.Collections.IEnumerable GetChildren(TreePath treePath)
        {
            List<DataRowNode> items = new List<DataRowNode>();

            if (treePath.IsEmpty() )
            {
                items.Add(m_root);
            }
            else
            {
                DataRowNode n = treePath.LastNode as DataRowNode;

                DataRow row = n.Row;
                int id = Convert.ToInt32(row[m_IDColumnName]);

                DataRow[] rows = m_table.Select("ParentID = " + id+" and "+m_IDColumnName+" <> "+id);
                foreach (DataRow r in rows)
                {
                    DataRowNode node = new DataRowNode(r,r["Name"].ToString());
                    node.Row = r;
                    //SampleApp.Properties.Resources.ResourceManager.
                    //node.Icon = new Bitmap(SampleApp.Properties.Resources.Records,new Size(15,15));
                    items.Add(node);
                }
            }
            return items;
        }

        public override bool IsLeaf(TreePath treePath)
        {
            DataRowNode n = treePath.LastNode as DataRowNode;
            if (n.Row["IsFolder"] == DBNull.Value)
                return false;
            return !Convert.ToBoolean(n.Row["IsFolder"]);
        }


        //public event EventHandler<TreeModelEventArgs> NodesChanged;

         //public event EventHandler<TreeModelEventArgs> NodesInserted;

        //public event EventHandler<TreeModelEventArgs> NodesRemoved;

        //public event EventHandler<TreePathEventArgs> StructureChanged;


        public void AddChild(TreePath parent, string text)
        {
            DataRowNode n = parent.LastNode as DataRowNode;
             
           DataRow r =   m_table.NewRow();
           r["ID"] = GetNextID();
           r["ParentID"] = n.Row["ID"];
           r["IsFolder"] = false;
           r["Name"] = text;
           r["Tag"] = "";
           m_table.Rows.Add(r);
           DataRowNode child = new DataRowNode(r, text);
           OnStructureChanged(new TreePathEventArgs(parent));
        }

        private int GetNextID()
        {
            int max = 1;
            for (int i = 0; i < m_table.Rows.Count; i++)
            {
                int id = Convert.ToInt32(m_table.Rows[i]["ID"]);
                if (id > max)
                    max = id;
            }

            return max + 1;
        }
    }
}
