using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Aga.Controls.Tree
{
	/// <summary>
	/// Provides a simple ready to use implementation of <see cref="ITreeModel"/>. Warning: this class is not optimized 
	/// to work with big amount of data. In this case create you own implementation of <c>ITreeModel</c>, and pay attention
	/// on GetChildren and IsLeaf methods.
	/// </summary>
	public class TreeModel : ITreeModel
	{
		private Node _root;

		public event EventHandler<TreePathEventArgs> StructureChanged;
		public event EventHandler<TreeModelEventArgs> NodesInserted;
		public event EventHandler<TreeModelEventArgs> NodesRemoved;
		public event EventHandler<TreeModelEventArgs> NodesChanged;

		public Node Root
		{
			get { return _root; }
		}
		public Collection<Node> Nodes
		{
			get { return _root.Nodes; }
		}

		public TreeModel()
		{
			_root = new Node();
			_root.Model = this;
		}

		public TreePath GetPath(Node node)
		{
			if (node == _root)
				return TreePath.Empty;
			else
			{
				Stack<object> stack = new Stack<object>();
				while (node != _root)
				{
					stack.Push(node);
					node = node.Parent;
				}
				return new TreePath(stack.ToArray());
			}
		}
		public Node FindNode(TreePath path)
		{
			if (path.IsEmpty())
				return _root;
			else
				return FindNode(_root, path, 0);
		}
		private Node FindNode(Node root, TreePath path, int level)
		{
			foreach (Node node in root.Nodes)
			{
				if (node == path.FullPath[level])
				{
					if (level == path.FullPath.Length - 1)
						return node;
					else
						return FindNode(node, path, level + 1);
				}
			}
			return null;
		}

		public virtual System.Collections.IEnumerable GetChildren(TreePath treePath)
		{
			Node node = FindNode(treePath);
			if (node != null)
				foreach (Node n in node.Nodes)
					yield return n;
			else
				yield break;
		}
		public virtual bool IsLeaf(TreePath treePath)
		{
			Node node = FindNode(treePath);
			if (node != null)
				return node.IsLeaf;
			else
				throw new ArgumentException("treePath");
		}

		public virtual void OnStructureChanged(TreePathEventArgs args)
		{
			if (StructureChanged != null)
				StructureChanged(this, args);
		}
		internal protected virtual void OnNodesChanged(Node parent, int index, Node node)
		{
			if (NodesChanged != null)
			{
				TreePath path = GetPath(parent);
				if (path == null) return;
				TreeModelEventArgs args = new TreeModelEventArgs(path, new int[] { index }, new object[] { node });
				NodesChanged(this, args);
			}
		}
		internal protected virtual void OnNodeInserted(Node parent, int index, Node node)
		{
			if (NodesInserted != null)
			{
				TreeModelEventArgs args = new TreeModelEventArgs(GetPath(parent), new int[] { index }, new object[] { node });
				NodesInserted(this, args);
			}

		}
		internal protected virtual void OnNodeRemoved(Node parent, int index, Node node)
		{
			if (NodesRemoved != null)
			{
				TreeModelEventArgs args = new TreeModelEventArgs(GetPath(parent), new int[] { index }, new object[] { node });
				NodesRemoved(this, args);
			}
		}
	}
}
