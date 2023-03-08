using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Windows.Forms;

namespace Aga.Controls.UnitTests
{
	[TestClass]
	public class PerformanceTest
	{
		public PerformanceTest()
		{
		}

		[TestMethod]
		public void TestMethod()
		{
			Bitmap b = new Bitmap(500, 50);
			Graphics gr = Graphics.FromImage(b);
			int num = 5000;
			string text = "Some Text String";
			Rectangle rect = new Rectangle(0, 0, 500, 50);

			TimeCounter.Start();
			for (int i = 0; i < num; i++)
			{
				TextRenderer.MeasureText(text, Control.DefaultFont);
				TextRenderer.DrawText(gr, text, Control.DefaultFont, rect, Color.Black);
			}
			Console.WriteLine("TextRenderer {0}", TimeCounter.Finish());

			TimeCounter.Start();
			for (int i = 0; i < num; i++)
			{
				gr.MeasureString(text, Control.DefaultFont);
				gr.DrawString(text, Control.DefaultFont, Brushes.Black, rect);
			}
			Console.WriteLine("Graphics {0}", TimeCounter.Finish());
		}
	}
}
