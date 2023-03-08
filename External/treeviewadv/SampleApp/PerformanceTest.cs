using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls;

namespace SampleApp
{
	public partial class PerformanceTest : UserControl
	{
		private const int Num = 25000;
		private TreeModel _model;

		public PerformanceTest()
		{
			InitializeComponent();
		}

		private void _load_Click(object sender, EventArgs e)
		{
			label3.Text = "Working";
			Application.DoEvents();

			_treeView.Model = null;
			_model = null;
			GC.Collect(3);

			TimeCounter.Start();

			_model = new TreeModel();
			for (int i = 0; i < 10; i++)
			{
				_model.Root.Nodes.Add(new Node(i.ToString()));
				for (int n = 0; n < 500; n++)
				{
					_model.Root.Nodes[i].Nodes.Add(new Node(n.ToString()));
					for (int k = 0; k < 5; k++)
						_model.Root.Nodes[i].Nodes[n].Nodes.Add(new Node(k.ToString()));
				}
			}

			_treeView.Model = _model;

			label3.Text = TimeCounter.Finish().ToString();
		}

		private void _expand_Click(object sender, EventArgs e)
		{
			if (_treeView.Root.Children.Count > 0)
			{
				label4.Text = "Working";
				Application.DoEvents();

				TimeCounter.Start();

				if (_treeView.Root.Children[0].IsExpanded)
					_treeView.CollapseAll();
				else
					_treeView.ExpandAll();

				label4.Text = TimeCounter.Finish().ToString();
			}
		}

		private void _load2_Click(object sender, EventArgs e)
		{
			label5.Text = "Working";
			Application.DoEvents();

			_treeView2.Nodes.Clear();

			TimeCounter.Start();
			_treeView2.BeginUpdate();

			List<TreeNode> list = new List<TreeNode>();
			for (int i = 0; i < 10; i++)
			{
				list.Add(new TreeNode(i.ToString()));
				for (int n = 0; n < 500; n++)
				{
					list[i].Nodes.Add(n.ToString());
					for (int k = 0; k < 5; k++)
						list[i].Nodes[n].Nodes.Add(k.ToString());
				}
			}
			_treeView2.Nodes.AddRange(list.ToArray());

			_treeView2.EndUpdate();
			label5.Text = TimeCounter.Finish().ToString();
		}

		private void _expand2_Click(object sender, EventArgs e)
		{
			if (_treeView2.Nodes.Count > 0)
			{
				label6.Text = "Working";
				Application.DoEvents();

				TimeCounter.Start();

				//treeView1.BeginUpdate();
				if (_treeView2.Nodes[0].IsExpanded)
					_treeView2.CollapseAll();
				else
					_treeView2.ExpandAll();
				//treeView1.EndUpdate();

				label6.Text = TimeCounter.Finish().ToString();
			}
		}
	}
}
