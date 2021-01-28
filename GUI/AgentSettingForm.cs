using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenHardwareMonitor.GUI
{
    public partial class AgentSettingForm : Form
    {
        public AgentSettingForm()
        {
            InitializeComponent();
        }

    public int Interval
    {
      get { return (int)IntervalNumericUpDn.Value; }
      set { IntervalNumericUpDn.Value = value; }

    }

    public string ActionUrl
    {
      get { return actionUrllTextBox.Text.Trim(); }
      set { actionUrllTextBox.Text = value; }
    }

    private void CancelButton_Click(object sender, EventArgs e)
        {
          Close();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
      Close();
    }
    }
}
