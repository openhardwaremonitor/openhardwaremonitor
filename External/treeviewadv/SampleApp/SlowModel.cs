#pragma warning disable 67  // Event never used

using System;
using System.Collections.Generic;
using System.Text;
using Aga.Controls.Tree;
using System.Threading;

namespace SampleApp
{
	public class SlowModel: ITreeModel
	{
		#region ITreeModel Members

		public System.Collections.IEnumerable GetChildren(TreePath treePath)
		{
			if (treePath.FullPath.Length < 3)
				for (int i = 0; i < 5; i++)
				{
					if (treePath.FirstNode != null)
						Thread.Sleep(1000);
					yield return new Node("item" + i.ToString());
				}
			else
				yield break;
		}

		public bool IsLeaf(TreePath treePath)
		{
			return false;
		}

		public event EventHandler<TreeModelEventArgs> NodesChanged;

		public event EventHandler<TreeModelEventArgs> NodesInserted;

		public event EventHandler<TreeModelEventArgs> NodesRemoved;

		public event EventHandler<TreePathEventArgs> StructureChanged;

		#endregion
	}
}
