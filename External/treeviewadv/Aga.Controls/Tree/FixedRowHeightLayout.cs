using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Aga.Controls.Tree.NodeControls;

namespace Aga.Controls.Tree
{
	internal class FixedRowHeightLayout : IRowLayout
	{
		private TreeViewAdv _treeView;
		private List<Rectangle> _rowCache;

		public FixedRowHeightLayout(TreeViewAdv treeView, int rowHeight)
		{
			_rowCache = new List<Rectangle>();
			_treeView = treeView;
			PreferredRowHeight = rowHeight;
		}

		private int _rowHeight;
		public int PreferredRowHeight
		{
			get { return _rowHeight; }
			set { _rowHeight = value; }
		}


		public int VisiblePageRowCount
		{
			get 
			{
				if (_treeView.RowCount == 0)
					return 0;
				else
				{
					int pageHeight = _treeView.DisplayRectangle.Height - _treeView.ColumnHeaderHeight;
					int y = 0;
					int visibleCount = 0;
					for (int i = _treeView.RowCount - 1; i >= 0; i--)
					{
						if (_treeView.RowMap[i].IsHidden)
							continue;
						y += GetRowHeight(i);
						if (y > pageHeight)
							return visibleCount;
						visibleCount++;
					}
					return _treeView.RowCount;
				}
			}
		}

		public int CurrentPageSize
		{
			get
			{
				if (_treeView.RowCount == 0)
					return 0;
				else
				{
					int pageHeight = _treeView.DisplayRectangle.Height - _treeView.ColumnHeaderHeight;
					int y = 0;
					for (int i = _treeView.FirstVisibleRow; i < _treeView.RowCount; i++)
					{
						y += GetRowHeight(i);
						if (y > pageHeight)
							return Math.Max(0, i - _treeView.FirstVisibleRow);
					}
					return Math.Max(0, _treeView.RowCount - _treeView.FirstVisibleRow);
				}
			}
		}

		public Rectangle GetRowBounds(int rowNo)
		{
			if (rowNo >= _rowCache.Count)
			{
				int count = _rowCache.Count;
				int y = count > 0 ? _rowCache[count - 1].Bottom : 0;
				for (int i = count; i <= rowNo; i++)
				{
					int height = GetRowHeight(i);
					_rowCache.Add(new Rectangle(0, y, 0, height));
					y += height;
				}
				if (rowNo < _rowCache.Count - 1)
					return Rectangle.Empty;
			}
			if (rowNo >= 0 && rowNo < _rowCache.Count)
				return _rowCache[rowNo];
			else
				return Rectangle.Empty;
		}

		private int GetRowHeight(int rowNo)
		{
			if (rowNo < _treeView.RowMap.Count)
			{
				TreeNodeAdv node = _treeView.RowMap[rowNo];
				return node.IsHidden ? 0 : _rowHeight;
			}
			else
				return 0;
		}

		public int GetRowAt(Point point)
		{
			int py = point.Y - _treeView.ColumnHeaderHeight;
			int y = 0;
			for (int i = _treeView.FirstVisibleRow; i < _treeView.RowCount; i++)
			{
				int h = GetRowHeight(i);
				if (py >= y && py < y + h)
					return i;
				else
					y += h;
			}
			return -1;
		}

		public int GetFirstRow(int lastPageRow)
		{
			int pageHeight = _treeView.DisplayRectangle.Height - _treeView.ColumnHeaderHeight;
			int y = 0;
			for (int i = lastPageRow; i >= 0; i--)
			{
				y += GetRowHeight(i);
				if (y > pageHeight)
					return Math.Max(0, i + 1);
			}
			return 0;
		}

		public void ClearCache()
		{
			_rowCache.Clear();
		}
	}
}
