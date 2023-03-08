using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Aga.Controls.Tree
{
	public class DrawColHeaderBgEventArgs : EventArgs
	{
		private	bool		handled		= false;
		private	Graphics	graphics	= null;
		private	Rectangle	bounds		= Rectangle.Empty;
		private	bool		pressed		= false;
		private	bool		hot			= false;

		public bool Handled
		{
			get { return this.handled; }
			set { this.handled = value; }
		}
		public Graphics Graphics
		{
			get { return this.graphics; }
		}
		public Rectangle Bounds
		{
			get { return this.bounds; }
		}
		public bool Pressed
		{
			get { return this.pressed; }
		}
		public bool Hot
		{
			get { return this.hot; }
		}

		public DrawColHeaderBgEventArgs(Graphics g, Rectangle bounds, bool pressed, bool hot)
		{
			this.graphics = g;
			this.bounds = bounds;
			this.pressed = pressed;
			this.hot = hot;
		}
	}
}
