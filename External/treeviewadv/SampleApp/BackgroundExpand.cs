using System;
using System.Windows.Forms;
using Aga.Controls.Tree;

namespace SampleApp
{
	public partial class BackgroundExpand : UserControl
	{
		public BackgroundExpand()
		{
			InitializeComponent();
			_treeView.Model = new SlowModel();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			_treeView.ExpandAll();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			_treeView.Model = new SlowModel();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			foreach (TreeNodeAdv node in _treeView.Root.Children)
				node.Expand();
		}

		private void button4_Click(object sender, EventArgs e)
		{
			_treeView.AbortBackgroundExpandingThreads();
		}
	}
}
