using System;
using System.Drawing;
using System.IO;

namespace SampleApp
{
	public abstract class BaseItem
	{
		private string _path = "";
		public string ItemPath
		{
			get { return _path; }
			set { _path = value; }
		}

		private Image _icon;
		public Image Icon
		{
			get { return _icon; }
			set { _icon = value; }
		}

		private long _size = 0;
		public long Size
		{
			get { return _size; }
			set { _size = value; }
		}

		private DateTime _date;
		public DateTime Date
		{
			get { return _date; }
			set { _date = value; }
		}

		public abstract string Name
		{
			get;
			set;
		}

		private BaseItem _parent;
		public BaseItem Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		private bool _isChecked;
		public bool IsChecked
		{
			get { return _isChecked; }
			set 
			{ 
				_isChecked = value;
				if (Owner != null)
					Owner.OnNodesChanged(this);
			}
		}

		private FolderBrowserModel _owner;
		public FolderBrowserModel Owner
		{
			get { return _owner; }
			set { _owner = value; }
		}

		/*public override bool Equals(object obj)
		{
			if (obj is BaseItem)
				return _path.Equals((obj as BaseItem).ItemPath);
			else
				return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return _path.GetHashCode();
		}*/

		public override string ToString()
		{
			return _path;
		}
	}

	public class RootItem : BaseItem
	{
		public RootItem(string name, FolderBrowserModel owner)
		{
			ItemPath = name;
			Owner = owner;
		}

		public override string Name
		{
			get
			{
				return ItemPath;
			}
			set
			{
			}
		}
	}

	public class FolderItem : BaseItem
	{
		public override string Name
		{
			get
			{
				return Path.GetFileName(ItemPath);
			}
			set
			{
				string dir = Path.GetDirectoryName(ItemPath);
				string destination = Path.Combine(dir, value);
				Directory.Move(ItemPath, destination);
				ItemPath = destination;
			}
		}

		public FolderItem(string name, BaseItem parent, FolderBrowserModel owner)
		{
			ItemPath = name;
			Parent = parent;
			Owner = owner;
		}
	}

	public class FileItem : BaseItem
	{
		public override string Name
		{
			get
			{
				return Path.GetFileName(ItemPath);
			}
			set
			{
				string dir = Path.GetDirectoryName(ItemPath);
				string destination = Path.Combine(dir, value);
				File.Move(ItemPath, destination);
				ItemPath = destination;
			}
		}

		public FileItem(string name, BaseItem parent, FolderBrowserModel owner)
		{
			ItemPath = name;
			Parent = parent;
			Owner = owner;
		}
	}
}
