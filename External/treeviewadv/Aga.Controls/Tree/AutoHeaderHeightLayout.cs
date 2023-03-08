using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Aga.Controls.Tree
{
	using System.Drawing;

	public class AutoHeaderHeightLayout : IHeaderLayout
	{
		DrawContext _measureContext;
		TreeViewAdv _treeView;

		public AutoHeaderHeightLayout(TreeViewAdv treeView, int headerHeight)
		{
			_treeView = treeView;
			PreferredHeaderHeight = headerHeight;
			_measureContext = new DrawContext();
			_measureContext.Graphics = Graphics.FromImage(new Bitmap(1, 1));
		}

		int? _headerHeight;

		bool _computed;

		#region Implementation of IHeaderLayout

		public int PreferredHeaderHeight
		{
			get { return GetHeaderHeight(); }
			set
			{
				_headerHeight = value;
				_computed = false;
			}
		}

		public void ClearCache()
		{
			_computed = false;
		}

		int GetHeaderHeight()
		{
			if (!_computed)
			{
				int res = 0;
				_measureContext.Font = _treeView.Font;
				foreach (TreeColumn nc in _treeView.Columns)
				{
					int h = nc.GetActualSize(_measureContext).Height;
					if (h > res) res = h;
				}
				_headerHeight = res;

				_computed = true;
			}

			return _headerHeight.Value;
		}

		#endregion
	}
}
