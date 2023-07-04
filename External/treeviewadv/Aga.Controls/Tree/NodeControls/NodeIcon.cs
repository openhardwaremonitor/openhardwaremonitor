using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Aga.Controls.Properties;
using System.ComponentModel;

namespace Aga.Controls.Tree.NodeControls
{
	public class NodeIcon : BindableControl
	{
		public NodeIcon()
		{
			LeftMargin = 1;
		}

		public override Size MeasureSize(TreeNodeAdv node, DrawContext context)
		{
			Image image = GetIcon(node);
			if (image != null)
				return image.Size;
			else
				return Size.Empty;
		}


		public override void Draw(TreeNodeAdv node, DrawContext context)
		{
			Image image = GetIcon(node);
			if (image != null)
			{
				Rectangle r = GetBounds(node, context);
				if ( image.Width > 0 && image.Height > 0 )
				{
					float factor = 1.0f;
					switch (_scaleMode)
					{
						case ImageScaleMode.Fit:
							factor = -1.0f;
							break;
						case ImageScaleMode.ScaleDown:
							factor = Math.Min((float)r.Width / (float)image.Width, (float)r.Height / (float)image.Height);
							factor = Math.Min(factor, 1.0f);
							break;
						case ImageScaleMode.ScaleUp:
							factor = Math.Min((float)r.Width / (float)image.Width, (float)r.Height / (float)image.Height);
							factor = Math.Max(factor, 1.0f);
							break;
						case ImageScaleMode.AlwaysScale:
							float fx = (float)r.Width / (float)image.Width;
							float fy = (float)r.Height / (float)image.Height;
							if (Math.Min(fx, fy) < 1)		factor = Math.Min(fx, fy);
							else if (Math.Max(fx, fy) > 1)	factor = Math.Max(fx, fy);
							else							factor = 1.0f;
							break;
						default: 
						case ImageScaleMode.Clip:
							factor = 1.0f;
							break;
					}

					if (DrawIconMustBeFired(node))
					{
						DrawIconEventArgs iconDrawArgs = new DrawIconEventArgs(node, this, context);
						OnDrawIcon(iconDrawArgs);

						if (iconDrawArgs.IconColorMatrix != null)
						{
							ImageAttributes attrib = new ImageAttributes();
							attrib.SetColorMatrix(iconDrawArgs.IconColorMatrix);
							Rectangle destRect;
							if (factor < 0.0f)
								destRect = r;
							else
								destRect = new Rectangle(r.X, r.Y, (int)(image.Width * factor), (int)(image.Height * factor));

							context.Graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attrib);
						}
						else
						{
							if (factor < 0.0f)
								context.Graphics.DrawImage(image, r);
							else if (factor == 1.0f)
								context.Graphics.DrawImage(image, r.X, r.Y, image.Width, image.Height);
							else
								context.Graphics.DrawImage(image, r.X, r.Y, image.Width * factor, image.Height * factor);
						}
					}
					else
					{
						if (factor < 0.0f)
							context.Graphics.DrawImage(image, r);
						else if (factor == 1.0f)
							context.Graphics.DrawImage(image, r.X, r.Y, image.Width, image.Height);
						else
							context.Graphics.DrawImage(image, r.X, r.Y, image.Width * factor, image.Height * factor);
					}
				}

			}
		}

		protected virtual Image GetIcon(TreeNodeAdv node)
		{
			return GetValue(node) as Image;
		}

        private ImageScaleMode _scaleMode = ImageScaleMode.Clip;
        [DefaultValue("Clip"), Category("Appearance")]
        public ImageScaleMode ScaleMode
        {
            get { return _scaleMode; }
            set { _scaleMode = value; }
        }

		
		/// <summary>
		/// Fires when control is going to draw the icon. Can be used to change its visual appearance
		/// </summary>
		public event EventHandler<DrawIconEventArgs> DrawIcon;
		protected virtual void OnDrawIcon(DrawIconEventArgs args)
		{
			TreeViewAdv tree = args.Node.Tree;
			if (tree != null)
				tree.FireDrawControl(args);
			if (DrawIcon != null)
				DrawIcon(this, args);
		}

		protected virtual bool DrawIconMustBeFired(TreeNodeAdv node)
		{
			return DrawIcon != null || (node.Tree != null && node.Tree.DrawControlMustBeFired());
		}
	}
}
