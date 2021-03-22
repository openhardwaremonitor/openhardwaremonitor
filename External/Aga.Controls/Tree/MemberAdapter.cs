using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Aga.Controls.Tree {
  internal struct MemberAdapter
	{
		private object _obj;
		private PropertyInfo _pi;
		private FieldInfo _fi;

		public static readonly MemberAdapter Empty = new MemberAdapter();

		public Type MemberType
		{
			get
			{
				if (_pi != null)
					return _pi.PropertyType;
				else if (_fi != null)
					return _fi.FieldType;
				else
					return null;
			}
		}

		public object Value
		{
			get
			{
				if (_pi != null && _pi.CanRead)
					return _pi.GetValue(_obj, null);
				else if (_fi != null)
					return _fi.GetValue(_obj);
				else
					return null;
			}
			set
			{
				if (_pi != null && _pi.CanWrite)
					_pi.SetValue(_obj, value, null);
				else if (_fi != null)
					_fi.SetValue(_obj, value);
			}
		}

		public MemberAdapter(object obj, PropertyInfo pi)
		{
			_obj = obj;
			_pi = pi;
			_fi = null;
		}

		public MemberAdapter(object obj, FieldInfo fi)
		{
			_obj = obj;
			_fi = fi;
			_pi = null;
		}
	}
}
