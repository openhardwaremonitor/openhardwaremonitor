/*
  
  Version: MPL 1.1/GPL 2.0/LGPL 2.1

  The contents of this file are subject to the Mozilla Public License Version
  1.1 (the "License"); you may not use this file except in compliance with
  the License. You may obtain a copy of the License at
 
  http://www.mozilla.org/MPL/

  Software distributed under the License is distributed on an "AS IS" basis,
  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
  for the specific language governing rights and limitations under the License.

  The Original Code is the Open Hardware Monitor code.

  The Initial Developer of the Original Code is 
  Michael Möller <m.moeller@gmx.ch>.
  Portions created by the Initial Developer are Copyright (C) 2009-2010
  the Initial Developer. All Rights Reserved.

  Contributor(s):

  Alternatively, the contents of this file may be used under the terms of
  either the GNU General Public License Version 2 or later (the "GPL"), or
  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
  in which case the provisions of the GPL or the LGPL are applicable instead
  of those above. If you wish to allow use of your version of this file only
  under the terms of either the GPL or the LGPL, and not to allow others to
  use your version of this file under the terms of the MPL, indicate your
  decision by deleting the provisions above and replace them with the notice
  and other provisions required by the GPL or the LGPL. If you do not delete
  the provisions above, a recipient may use your version of this file under
  the terms of any one of the MPL, the GPL or the LGPL.
 
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.GUI {
  public partial class ParameterForm : Form {

    private IReadOnlyArray<IParameter> parameters;
    private BindingList<ParameterRow> parameterRows;

    public ParameterForm() {
      InitializeComponent();
    }
    
    public IReadOnlyArray<IParameter> Parameters {
      get {
        return parameters;
      }
      set {
        parameters = value;
        parameterRows = new BindingList<ParameterRow>();
        foreach (IParameter parameter in parameters)
          parameterRows.Add(new ParameterRow(parameter));
        bindingSource.DataSource = parameterRows;
      }
    }

    private class ParameterRow : INotifyPropertyChanged {
      public IParameter parameter;
      private float value;
      public bool isDefault;

      public event PropertyChangedEventHandler PropertyChanged;

      private void NotifyPropertyChanged(String propertyName) {
        if (PropertyChanged != null) {
          PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
      }

      public ParameterRow(IParameter parameter){
        this.parameter = parameter;
        this.value = parameter.Value;
        this.isDefault = parameter.IsDefault;
      }

      public string Name {
        get { return parameter.Name; }
      }

      public float Value {
        get { return value; }
        set {            
          this.isDefault = false;
          this.value = value;
          NotifyPropertyChanged("Default");
          NotifyPropertyChanged("Value");
        }
      }

      public bool Default {
        get { return isDefault; }
        set {
          isDefault = value;
          if (value)
            this.value = parameter.DefaultValue;
          NotifyPropertyChanged("Default");
          NotifyPropertyChanged("Value");
        }
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
        !float.TryParse(e.FormattedValue.ToString(), out value)) {
        dataGridView.Rows[e.RowIndex].Cells[0].ErrorText = 
          "Invalid value";
        e.Cancel = true;
      }
    }

    private void dataGridView_CellEndEdit(object sender,
      DataGridViewCellEventArgs e) {
      dataGridView.Rows[e.RowIndex].Cells[0].ErrorText = "";
    }

    private void okButton_Click(object sender, EventArgs e) {
      foreach (ParameterRow row in parameterRows) {
        if (row.Default) {
          row.parameter.IsDefault = true;
        } else {
          row.parameter.Value = row.Value;
        }
      }
    }

    private void dataGridView_CurrentCellDirtyStateChanged(object sender, 
      EventArgs e) {
      if (dataGridView.CurrentCell is DataGridViewCheckBoxCell ||
        dataGridView.CurrentCell is DataGridViewComboBoxCell) 
      {
        dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
      }
    }
  }
}
