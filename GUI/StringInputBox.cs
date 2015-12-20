using System;
using System.Windows.Forms;

namespace OpenHardwareMonitor.GUI
{
    public partial class StringInputBox : Form
    {
        public StringInputBox()
        {
            InitializeComponent();

            Activated += (sender, args) => inputBox.Focus();
        }

        public string Title
        {
            get
            {
                return Text;
            }

            set
            {
                Text = value;
            }
        }

        public string PromptMessage
        {
            get
            {
                return promptMessageLabel.Text;
            }

            set
            {
                promptMessageLabel.Text = value;
            }
        }

        public string UserInput
        {
            get
            {
                return inputBox.Text;
            }
        }

        public Func<string, bool> IsValidInput { get; set; }

        public bool Accepted { get; private set; }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (IsValidInput != null && !IsValidInput(UserInput))
            {
                MessageBox.Show("Invalid input.", "Invalid input.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Accepted = true;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Accepted = false;
            Close();
        }
    }
}
