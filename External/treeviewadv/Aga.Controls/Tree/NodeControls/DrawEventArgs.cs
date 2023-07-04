using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Aga.Controls.Tree.NodeControls
{
	public class DrawEventArgs : NodeEventArgs
	{
		private DrawContext _context;
		public DrawContext Context
		{
			get { return _context; }
		}

		private NodeControl _control;
		public NodeControl Control
		{
			get { return _control; }
		}
		public EditableControl EditableControl
		{
			get { return _control as EditableControl; }
		}

		public DrawEventArgs(TreeNodeAdv node, NodeControl control, DrawContext context) : base(node)
		{
			_control = control;
			_context = context;
		}
	}
}
