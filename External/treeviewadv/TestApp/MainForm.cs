using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Aga.Controls.Tree;

namespace TestApp
{
	public partial class MainForm : Form
	{
		private TreeModel model;

		public MainForm()
		{
			InitializeComponent();

			this.model = new TreeModel();
			this.model.Nodes.Add(new Node("Root0"));
			this.model.Nodes.Add(new Node("Root1"));
			this.model.Nodes[1].Nodes.Add(new Node("Child0"));
			this.model.Nodes[1].Nodes.Add(new Node("Child1"));
			this.model.Nodes[1].Nodes.Add(new Node("Child2"));
			this.model.Nodes[1].Nodes.Add(new Node("Child3"));
			this.model.Nodes[1].Nodes.Add(new Node("Child4"));
			this.model.Nodes[1].Nodes.Add(new Node("Child5"));
			this.model.Nodes.Add(new Node("Root2"));
			this.model.Nodes.Add(new Node("Root3"));
			this.model.Nodes.Add(new Node("Root4"));
			this.model.Nodes.Add(new Node("Root5"));
			this.model.Nodes[5].Nodes.Add(new Node("Child0"));
			this.model.Nodes[5].Nodes.Add(new Node("Child1"));
			this.model.Nodes[5].Nodes.Add(new Node("Child2"));
			this.model.Nodes.Add(new Node("Root6"));
			this.model.Nodes.Add(new Node("Root7"));
			this.model.Nodes.Add(new Node("Root8"));
			this.model.Nodes.Add(new Node("Root9"));
			this.model.Nodes.Add(new Node("Root10"));
			this.model.Nodes.Add(new Node("Root11"));
			this.model.Nodes.Add(new Node("Root12"));
			this.model.Nodes.Add(new Node("Root13"));
			this.model.Nodes.Add(new Node("Root14"));

			this.treeViewAdv1.Model = this.model;
			this.treeViewAdv1.NodeFilter = filter;

			this.model.Nodes[1].Nodes[1].IsHidden = true;
			this.model.Nodes[1].Nodes[2].IsHidden = true;
			this.model.Nodes[1].Nodes[3].IsHidden = true;
			this.model.Nodes[5].IsHidden = true;
			this.model.Nodes[6].IsHidden = true;
			this.model.Nodes[7].IsHidden = true;
			this.model.Nodes[8].IsHidden = true;
			this.model.Nodes.First().IsHidden = true;
			this.model.Nodes.Last().IsHidden = true;
		}

		private bool filter(object obj)
		{
			TreeNodeAdv viewNode = obj as TreeNodeAdv;
			Node n = viewNode != null ? viewNode.Tag as Node : obj as Node;
			return n == null || n.Text.ToUpper().Contains(this.textBox1.Text.ToUpper()) || n.Nodes.Any(filter);
		}
		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			this.treeViewAdv1.UpdateNodeFilter();
		}
		private void button1_Click(object sender, EventArgs e)
		{
			Node n = (this.treeViewAdv1.SelectedNode.Tag as Node);
			//n.Parent.Nodes.Remove(n);
			n.IsHidden = !n.IsHidden;
		}

		private void treeViewAdv1_SelectionChanged(object sender, EventArgs e)
		{
			Console.WriteLine("---");
			foreach (TreeNodeAdv viewNode in this.treeViewAdv1.SelectedNodes)
			{
				Console.WriteLine((viewNode.Tag as Node).Text);
			}
		}
	}
}
