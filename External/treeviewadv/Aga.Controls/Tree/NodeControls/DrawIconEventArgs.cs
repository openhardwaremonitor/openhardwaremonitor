using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Aga.Controls.Tree.NodeControls
{
	public class DrawIconEventArgs : DrawEventArgs
	{
		private ColorMatrix _iconMatrix;
		public ColorMatrix IconColorMatrix
		{
			get { return _iconMatrix; }
			set { _iconMatrix = value; }
		}

		public DrawIconEventArgs(TreeNodeAdv node, NodeControl control, DrawContext context) : base(node, control, context) {}
	}
}
