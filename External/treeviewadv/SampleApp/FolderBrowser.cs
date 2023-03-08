using System;
using System.ComponentModel;
using System.Windows.Forms;

using Aga.Controls.Tree.NodeControls;
using Aga.Controls.Tree;

namespace SampleApp
{
	public partial class FolderBrowser : UserControl
	{
		private class ToolTipProvider: IToolTipProvider
		{
			public string GetToolTip(TreeNodeAdv node, NodeControl nodeControl)
			{
				if (node.Tag is RootItem)
					return null;
				else
					return "Second click to rename node";
			}
		}

		public FolderBrowser()
		{
			InitializeComponent();

            cboxGrid.DataSource = System.Enum.GetValues(typeof(GridLineStyle));
			cboxGrid.SelectedItem = GridLineStyle.HorizontalAndVertical;

            cbLines.Checked = _treeView.ShowLines;

			_name.ToolTipProvider = new ToolTipProvider();
			_name.EditorShowing += new CancelEventHandler(_name_EditorShowing);

			_treeView.Model = new SortedTreeModel(new FolderBrowserModel());

		}

		void _name_EditorShowing(object sender, CancelEventArgs e)
		{
			if (_treeView.CurrentNode.Tag is RootItem)
				e.Cancel = true;
		}

		private void _treeView_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				NodeControlInfo info = _treeView.GetNodeControlInfoAt(e.Location);
				if (info.Control != null)
				{
					Console.WriteLine(info.Bounds);
				}
			}
		}

		private void _treeView_ColumnClicked(object sender, TreeColumnEventArgs e)
		{
            TreeColumn clicked = e.Column;
            if (clicked.SortOrder == SortOrder.Ascending)
                clicked.SortOrder = SortOrder.Descending;
            else
                clicked.SortOrder = SortOrder.Ascending;

            (_treeView.Model as SortedTreeModel).Comparer = new FolderItemSorter(clicked.Header, clicked.SortOrder);
		}

        private void cboxGrid_SelectedIndexChanged(object sender, EventArgs e)
        {
            _treeView.GridLineStyle = (GridLineStyle)cboxGrid.SelectedItem;
        }

        private void cbLines_CheckedChanged(object sender, EventArgs e)
        {
            _treeView.ShowLines = cbLines.Checked;
        }

		private void _treeView_NodeMouseDoubleClick(object sender, TreeNodeAdvMouseEventArgs e)
		{
			Console.WriteLine(e.Node);
		}
	}
}
