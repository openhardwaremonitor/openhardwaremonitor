using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Aga.Controls.Tree;
using System.Reflection;
using System.Globalization;

namespace Aga.Controls.UnitTests
{
	[TestClass]
	public class SerializationTest
	{
		[TestMethod]
		public void TestTreeNodeAdv()
		{
			PropertyInfo prop =	typeof(TreeNodeAdv).GetProperty("Nodes", BindingFlags.NonPublic | BindingFlags.Instance);

			TreeNodeAdv node = new TreeNodeAdv("root");
			node.IsExpanded = true;
			ICollection<TreeNodeAdv> nodes = (ICollection<TreeNodeAdv>)prop.GetValue(node, null);

			nodes.Add(new TreeNodeAdv("ch0"));
			nodes.Add(new TreeNodeAdv("ch2"));

			MemoryStream ms = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(ms, node);
			ms.Position = 0;

			TreeNodeAdv node2 = formatter.Deserialize(ms) as TreeNodeAdv;
			Assert.AreEqual(node.Tag, node2.Tag);
			Assert.AreEqual(node.IsExpanded, node2.IsExpanded);
			Assert.AreEqual(node.Children.Count, node2.Children.Count);
			for (int i = 0; i < node.Children.Count; i++)
				Assert.AreEqual(node.Children[i].Tag, node2.Children[i].Tag);
		}
	}
}
