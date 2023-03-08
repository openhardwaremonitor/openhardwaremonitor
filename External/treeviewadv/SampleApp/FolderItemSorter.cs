using System;
using System.Collections;
using System.Windows.Forms;

namespace SampleApp
{
	public class FolderItemSorter : IComparer
	{
		private string _mode;
		private SortOrder _order;

		public FolderItemSorter(string mode, SortOrder order)
		{
			_mode = mode;
			_order = order;
		}

		public int Compare(object x, object y)
		{
			BaseItem a = x as BaseItem;
			BaseItem b = y as BaseItem;
			int res = 0;

			if (_mode == "Date")
				res = DateTime.Compare(a.Date, b.Date);
			else if (_mode == "Size")
			{
				if (a.Size < b.Size)
					res = -1;
				else if (a.Size > b.Size)
					res = 1;
			}
			else
				res = string.Compare(a.Name, b.Name);

			if (_order == SortOrder.Ascending)
				return -res;
			else
				return res;
		}

		private string GetData(object x)
		{
			return (x as BaseItem).Name;
		}
	}
}
