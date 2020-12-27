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
    public partial class ComServerSettingsForm : Form
    {
        bool validBaud = true;
        bool validDataBits = true;
        MainForm parent;
        public ComServerSettingsForm(MainForm m)
        {
            InitializeComponent();
            parent = m;
        }

    private void comSaveButton_Click(object sender, EventArgs e) {
      parent.ComServer.DataBits = int.Parse(dataBitsTextBox.Text);
      parent.ComServer.BaudRate = int.Parse(baudTextBox.Text);
      parent.ComServer.PortName = portNameTextBox.Text;
      parent.ComServer.StopBits = stopBitsDropDown.SelectedIndex + 1;
      parent.ComServer.Parity = parityDropDown.SelectedIndex;
      parent.ComServer.SendInterval = (int)intervalBox.Value * 1000;
      this.Close();
    }

    private void comCancelButton_Click(object sender, EventArgs e) {
      this.Close();
    }

    private void ComServerSettingsForm_Load(object sender, EventArgs e) {
      portNameTextBox.Text = parent.ComServer.PortName;
      baudTextBox.Text = parent.ComServer.BaudRate.ToString();
      dataBitsTextBox.Text = parent.ComServer.DataBits.ToString();
      stopBitsDropDown.SelectedIndex = parent.ComServer.StopBits - 1;
      parityDropDown.SelectedIndex = parent.ComServer.Parity;
      intervalBox.Value = parent.ComServer.SendInterval / 1000;
    }

    private void dataBitsTextBox_TextChanged(object sender, EventArgs e) {
      if (!int.TryParse(dataBitsTextBox.Text, out int num)) {
        comSaveButton.Enabled = false;
        validDataBits = false;
      } else {
        validDataBits = true;
        comSaveButton.Enabled = validBaud;
      }
    }

    private void baudTextBox_TextChanged(object sender, EventArgs e) {
      if (!int.TryParse(baudTextBox.Text, out int num)) {
        comSaveButton.Enabled = false;
        validBaud = false;
      } else {
        validBaud = true;
        comSaveButton.Enabled = validDataBits;
      }
    }
  }
}
