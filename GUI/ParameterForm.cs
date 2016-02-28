/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/


using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenHardwareMonitor.Collections;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.GUI
{
    public partial class ParameterForm : Form
    {
        private BindingList<ParameterRow> parameterRows;

        private IReadOnlyArray<IParameter> parameters;

        public ParameterForm()
        {
            InitializeComponent();
        }

        public IReadOnlyArray<IParameter> Parameters
        {
            get { return parameters; }
            set
            {
                parameters = value;
                parameterRows = new BindingList<ParameterRow>();
                foreach (var parameter in parameters)
                    parameterRows.Add(new ParameterRow(parameter));
                bindingSource.DataSource = parameterRows;
            }
        }

        private void dataGridView_RowEnter(object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < parameters.Length)
                descriptionLabel.Text = parameters[e.RowIndex].Description;
            else
                descriptionLabel.Text = "";
        }

        private void dataGridView_CellValidating(object sender,
            DataGridViewCellValidatingEventArgs e)
        {
            float value;
            if (e.ColumnIndex == 2 &&
                !float.TryParse(e.FormattedValue.ToString(), out value))
            {
                dataGridView.Rows[e.RowIndex].Cells[0].ErrorText =
                    "Invalid value";
                e.Cancel = true;
            }
        }

        private void dataGridView_CellEndEdit(object sender,
            DataGridViewCellEventArgs e)
        {
            dataGridView.Rows[e.RowIndex].Cells[0].ErrorText = "";
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            foreach (var row in parameterRows)
            {
                if (row.Default)
                {
                    row.parameter.IsDefault = true;
                }
                else
                {
                    row.parameter.Value = row.Value;
                }
            }
        }

        private void dataGridView_CurrentCellDirtyStateChanged(object sender,
            EventArgs e)
        {
            if (dataGridView.CurrentCell is DataGridViewCheckBoxCell ||
                dataGridView.CurrentCell is DataGridViewComboBoxCell)
            {
                dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private class ParameterRow : INotifyPropertyChanged
        {
            public bool isDefault;
            public readonly IParameter parameter;
            private float value;

            public ParameterRow(IParameter parameter)
            {
                this.parameter = parameter;
                value = parameter.Value;
                isDefault = parameter.IsDefault;
            }

            public string Name
            {
                get { return parameter.Name; }
            }

            public float Value
            {
                get { return value; }
                set
                {
                    isDefault = false;
                    this.value = value;
                    NotifyPropertyChanged("Default");
                    NotifyPropertyChanged("Value");
                }
            }

            public bool Default
            {
                get { return isDefault; }
                set
                {
                    isDefault = value;
                    if (value)
                        this.value = parameter.DefaultValue;
                    NotifyPropertyChanged("Default");
                    NotifyPropertyChanged("Value");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}