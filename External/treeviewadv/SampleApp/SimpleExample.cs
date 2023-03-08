using System;
using System.Drawing;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using Aga.Controls;

namespace SampleApp
{
	public partial class SimpleExample : UserControl
	{
		private class ToolTipProvider : IToolTipProvider
		{
			public string GetToolTip(TreeNodeAdv node, NodeControl nodeControl)
			{
				return "Drag&Drop nodes to move them";
			}
		}

		private TreeModel _model;
		private Font _childFont;

		public SimpleExample()
		{
			InitializeComponent();

			_tree.NodeMouseClick += new EventHandler<TreeNodeAdvMouseEventArgs>(_tree_NodeMouseClick);

			_nodeTextBox.ToolTipProvider = new ToolTipProvider();
            _nodeTextBox.DrawText += new EventHandler<DrawTextEventArgs>(_nodeTextBox_DrawText);
			_model = new TreeModel();
			_childFont = new Font(_tree.Font.FontFamily, 18, FontStyle.Bold);
			_tree.Model = _model;
			ChangeButtons();

			_tree.BeginUpdate();
			for (int i = 0; i < 10; i++)
			{
				Node node = AddRoot();
				for (int n = 0; n < 500; n++)
				{
					Node child = AddChild(node);
					for (int k = 0; k < 5; k++)
						AddChild(child);
				}
			}
			_tree.EndUpdate();

			TreeModel model2 = new TreeModel();
			_tree2.Model = model2;
			for (int i = 0; i < 10; i++)
				model2.Nodes.Add(new MyNode("Node" + i.ToString()));
		}

		void _tree_NodeMouseClick(object sender, TreeNodeAdvMouseEventArgs e)
		{
			Console.WriteLine("NodeMouseClick at " + e.Node.Index.ToString());
		}

		void _nodeTextBox_DrawText(object sender, DrawTextEventArgs e)
		{
            if ((e.Node.Tag as MyNode).Text.StartsWith("Child"))
            {
                e.TextColor = Color.Red;
                e.Font = _childFont;
            }
		}

		private Node AddRoot()
		{
			Node node = new MyNode("Long Root Node Text" + _model.Nodes.Count.ToString());
			_model.Nodes.Add(node);
			return node;
		}

		private Node AddChild(Node parent)
		{
			Node node = new MyNode("Child Node " + parent.Nodes.Count.ToString());
            parent.Nodes.Add(node);
			return node;
		}

		private void ClearClick(object sender, EventArgs e)
		{
			_tree.BeginUpdate();
			_model.Nodes.Clear();
			_tree.EndUpdate();
		}

		private void AddRootClick(object sender, EventArgs e)
		{
			AddRoot();
		}

		private void AddChildClick(object sender, EventArgs e)
		{
			if (_tree.SelectedNode != null)
			{
				Node parent = _tree.SelectedNode.Tag as Node;
				AddChild(parent);
				_tree.SelectedNode.IsExpanded = true;
			}
		}

		private void DeleteClick(object sender, EventArgs e)
		{
			if (_tree.SelectedNode != null)
				(_tree.SelectedNode.Tag as Node).Parent = null;
		}

		private void _tree_SelectionChanged(object sender, EventArgs e)
		{
			ChangeButtons();
		}

		private void ChangeButtons()
		{
            _addChild.Enabled = (_tree.SelectedNode != null);
            _deleteNode.Enabled = (_tree.SelectedNode != null);
            
            btnExpNode.Enabled = (_tree.SelectedNode != null);
            btnExpNodes.Enabled = (_tree.SelectedNode != null);

            btnCollNode.Enabled = (_tree.SelectedNode != null);
            btnCollNodes.Enabled = (_tree.SelectedNode != null);
		}

		private void _tree_ItemDrag(object sender, ItemDragEventArgs e)
		{
			_tree.DoDragDropSelectedNodes(DragDropEffects.Move);
		}

		private void _tree_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(TreeNodeAdv[])) && _tree.DropPosition.Node != null)
			{
				TreeNodeAdv[] nodes = e.Data.GetData(typeof(TreeNodeAdv[])) as TreeNodeAdv[];
				TreeNodeAdv parent = _tree.DropPosition.Node;
				if (_tree.DropPosition.Position != NodePosition.Inside)
					parent = parent.Parent;

				foreach (TreeNodeAdv node in nodes)
					if (!CheckNodeParent(parent, node))
					{
						e.Effect = DragDropEffects.None;
						return;
					}

				e.Effect = e.AllowedEffect;
			}
		}

		private bool CheckNodeParent(TreeNodeAdv parent, TreeNodeAdv node)
		{
			while (parent != null)
			{
				if (node == parent)
					return false;
				else
					parent = parent.Parent;
			}
			return true;
		}

		private void _tree_DragDrop(object sender, DragEventArgs e)
		{
			_tree.BeginUpdate();

			TreeNodeAdv[] nodes = (TreeNodeAdv[])e.Data.GetData(typeof(TreeNodeAdv[]));
			Node dropNode = _tree.DropPosition.Node.Tag as Node;
			if (_tree.DropPosition.Position == NodePosition.Inside)
			{
				foreach (TreeNodeAdv n in nodes)
				{
					(n.Tag as Node).Parent = dropNode;
				}
				_tree.DropPosition.Node.IsExpanded = true;
			}
			else
			{
				Node parent = dropNode.Parent;
				Node nextItem = dropNode;
				if (_tree.DropPosition.Position == NodePosition.After)
					nextItem = dropNode.NextNode;

				foreach(TreeNodeAdv node in nodes)
					(node.Tag as Node).Parent = null;

				int index = -1;
				index = parent.Nodes.IndexOf(nextItem);
				foreach (TreeNodeAdv node in nodes)
				{
					Node item = node.Tag as Node;
					if (index == -1)
						parent.Nodes.Add(item);
					else
					{
						parent.Nodes.Insert(index, item);
						index++;
					}
				}
			}

			_tree.EndUpdate();
		}

		private void _timer_Tick(object sender, EventArgs e)
		{
			_tree.Refresh();
		}

		private void _performanceTest_Click(object sender, EventArgs e)
		{
			_timer.Enabled = _performanceTest.Checked;
		}

		private void _autoRowHeight_Click(object sender, EventArgs e)
		{
			_tree.AutoRowHeight = _autoRowHeight.Checked;
		}

		private void _fontSize_ValueChanged(object sender, EventArgs e)
		{
			_tree.Font = new Font(_tree.Font.FontFamily, (float)_fontSize.Value);
		}

		private void _tree_NodeMouseDoubleClick(object sender, TreeNodeAdvMouseEventArgs e)
		{
			if (e.Control is NodeTextBox)
				MessageBox.Show(e.Node.Tag.ToString());
		}

		private void _tree2_ItemDrag(object sender, ItemDragEventArgs e)
		{
			_tree2.DoDragDropSelectedNodes(DragDropEffects.Move);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			TimeCounter.Start();
			_tree.FullUpdate();
			//_model.Root.Nodes[0].Text = "Child";
			button1.Text = TimeCounter.Finish().ToString();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (_tree.Root.Children.Count > 0)
			{
				if (_tree.Root.Children[0].IsExpanded)
					_tree.CollapseAll();
				else
					_tree.ExpandAll();
			}
		}

        private void btnExpNode_Click(object sender, EventArgs e)
        {
            _tree.SelectedNode.Expand();
        }

        private void btnExpNodes_Click(object sender, EventArgs e)
        {
            _tree.SelectedNode.ExpandAll();
        }

        private void btnCollNode_Click(object sender, EventArgs e)
        {
            _tree.SelectedNode.Collapse();
        }

        private void btnCollNodes_Click(object sender, EventArgs e)
        {
            _tree.SelectedNode.CollapseAll();
        }

		private void button3_Click(object sender, EventArgs e)
		{
			_tree.ClearSelection();
		}
	}
}
