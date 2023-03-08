using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;

namespace SampleApp
{
	public partial class AdvancedExample : UserControl
	{
		public AdvancedExample()
		{
			InitializeComponent();
			_nodeTextBox.IsVisibleValueNeeded += CheckIndex;
			_nodeCheckBox.IsEditEnabledValueNeeded += CheckIndex;

			TreeModel _model = new TreeModel();
			for (int i = 0; i < 20; i++)
			{
				_model.Root.Nodes.Add(new MyNode("node" + i.ToString()));
			}
			_treeView.Model = _model;
		}

		void CheckIndex(object sender, NodeControlValueEventArgs e)
		{
			e.Value = (e.Node.Index % 2 == 0);
		}
	}
}
