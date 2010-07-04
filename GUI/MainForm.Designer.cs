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

namespace OpenHardwareMonitor.GUI {
  partial class MainForm {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.treeView = new Aga.Controls.Tree.TreeViewAdv();
      this.sensor = new Aga.Controls.Tree.TreeColumn();
      this.value = new Aga.Controls.Tree.TreeColumn();
      this.min = new Aga.Controls.Tree.TreeColumn();
      this.max = new Aga.Controls.Tree.TreeColumn();
      this.nodeImage = new Aga.Controls.Tree.NodeControls.NodeIcon();
      this.nodeCheckBox = new Aga.Controls.Tree.NodeControls.NodeCheckBox();
      this.nodeTextBoxText = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.nodeTextBoxValue = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.nodeTextBoxMin = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.nodeTextBoxMax = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.menuStrip = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.saveReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.sumbitReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
      this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.hiddenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.plotMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.columnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.valueMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.minMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.maxMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.startMinMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.minTrayMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.startupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.separatorMenuItem = new System.Windows.Forms.ToolStripSeparator();
      this.temperatureUnitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.celciusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.fahrenheitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
      this.hddMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.splitContainer = new System.Windows.Forms.SplitContainer();
      this.plotPanel = new OpenHardwareMonitor.GUI.PlotPanel();
      this.sensorContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
      this.timer = new System.Windows.Forms.Timer(this.components);
      this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
      this.resetMinMaxMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.menuStrip.SuspendLayout();
      this.splitContainer.Panel1.SuspendLayout();
      this.splitContainer.Panel2.SuspendLayout();
      this.splitContainer.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeView
      // 
      this.treeView.BackColor = System.Drawing.SystemColors.Window;
      this.treeView.Columns.Add(this.sensor);
      this.treeView.Columns.Add(this.value);
      this.treeView.Columns.Add(this.min);
      this.treeView.Columns.Add(this.max);
      this.treeView.DefaultToolTipProvider = null;
      this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeView.DragDropMarkColor = System.Drawing.Color.Black;
      this.treeView.FullRowSelect = true;
      this.treeView.GridLineStyle = Aga.Controls.Tree.GridLineStyle.Horizontal;
      this.treeView.LineColor = System.Drawing.SystemColors.ControlDark;
      this.treeView.Location = new System.Drawing.Point(0, 0);
      this.treeView.Model = null;
      this.treeView.Name = "treeView";
      this.treeView.NodeControls.Add(this.nodeImage);
      this.treeView.NodeControls.Add(this.nodeCheckBox);
      this.treeView.NodeControls.Add(this.nodeTextBoxText);
      this.treeView.NodeControls.Add(this.nodeTextBoxValue);
      this.treeView.NodeControls.Add(this.nodeTextBoxMin);
      this.treeView.NodeControls.Add(this.nodeTextBoxMax);
      this.treeView.RowHeight = 18;
      this.treeView.SelectedNode = null;
      this.treeView.Size = new System.Drawing.Size(410, 488);
      this.treeView.TabIndex = 0;
      this.treeView.Text = "treeView";
      this.treeView.UseColumns = true;
      this.treeView.NodeMouseDoubleClick += new System.EventHandler<Aga.Controls.Tree.TreeNodeAdvMouseEventArgs>(this.treeView_NodeMouseDoubleClick);
      this.treeView.Click += new System.EventHandler(this.treeView_Click);
      // 
      // sensor
      // 
      this.sensor.Header = "Sensor";
      this.sensor.SortOrder = System.Windows.Forms.SortOrder.None;
      this.sensor.TooltipText = null;
      this.sensor.Width = 250;
      // 
      // value
      // 
      this.value.Header = "Value";
      this.value.SortOrder = System.Windows.Forms.SortOrder.None;
      this.value.TooltipText = null;
      this.value.Width = 100;
      // 
      // min
      // 
      this.min.Header = "Min";
      this.min.SortOrder = System.Windows.Forms.SortOrder.None;
      this.min.TooltipText = null;
      this.min.Width = 100;
      // 
      // max
      // 
      this.max.Header = "Max";
      this.max.SortOrder = System.Windows.Forms.SortOrder.None;
      this.max.TooltipText = null;
      this.max.Width = 100;
      // 
      // nodeImage
      // 
      this.nodeImage.DataPropertyName = "Image";
      this.nodeImage.LeftMargin = 1;
      this.nodeImage.ParentColumn = this.sensor;
      this.nodeImage.ScaleMode = Aga.Controls.Tree.ImageScaleMode.Fit;
      // 
      // nodeCheckBox
      // 
      this.nodeCheckBox.DataPropertyName = "Plot";
      this.nodeCheckBox.EditEnabled = true;
      this.nodeCheckBox.LeftMargin = 3;
      this.nodeCheckBox.ParentColumn = this.sensor;
      // 
      // nodeTextBoxText
      // 
      this.nodeTextBoxText.DataPropertyName = "Text";
      this.nodeTextBoxText.EditEnabled = true;
      this.nodeTextBoxText.IncrementalSearchEnabled = true;
      this.nodeTextBoxText.LeftMargin = 3;
      this.nodeTextBoxText.ParentColumn = this.sensor;
      // 
      // nodeTextBoxValue
      // 
      this.nodeTextBoxValue.DataPropertyName = "Value";
      this.nodeTextBoxValue.IncrementalSearchEnabled = true;
      this.nodeTextBoxValue.LeftMargin = 3;
      this.nodeTextBoxValue.ParentColumn = this.value;
      // 
      // nodeTextBoxMin
      // 
      this.nodeTextBoxMin.DataPropertyName = "Min";
      this.nodeTextBoxMin.IncrementalSearchEnabled = true;
      this.nodeTextBoxMin.LeftMargin = 3;
      this.nodeTextBoxMin.ParentColumn = this.min;
      // 
      // nodeTextBoxMax
      // 
      this.nodeTextBoxMax.DataPropertyName = "Max";
      this.nodeTextBoxMax.IncrementalSearchEnabled = true;
      this.nodeTextBoxMax.LeftMargin = 3;
      this.nodeTextBoxMax.ParentColumn = this.max;
      // 
      // menuStrip
      // 
      this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
      this.menuStrip.Location = new System.Drawing.Point(0, 0);
      this.menuStrip.Name = "menuStrip";
      this.menuStrip.Size = new System.Drawing.Size(410, 24);
      this.menuStrip.TabIndex = 1;
      this.menuStrip.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveReportToolStripMenuItem,
            this.sumbitReportToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exitToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
      this.fileToolStripMenuItem.Text = "File";
      // 
      // saveReportToolStripMenuItem
      // 
      this.saveReportToolStripMenuItem.Name = "saveReportToolStripMenuItem";
      this.saveReportToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
      this.saveReportToolStripMenuItem.Text = "Save Report...";
      this.saveReportToolStripMenuItem.Click += new System.EventHandler(this.saveReportToolStripMenuItem_Click);
      // 
      // sumbitReportToolStripMenuItem
      // 
      this.sumbitReportToolStripMenuItem.Name = "sumbitReportToolStripMenuItem";
      this.sumbitReportToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
      this.sumbitReportToolStripMenuItem.Text = "Submit Report...";
      this.sumbitReportToolStripMenuItem.Click += new System.EventHandler(this.sumbitReportToolStripMenuItem_Click);
      // 
      // toolStripMenuItem2
      // 
      this.toolStripMenuItem2.Name = "toolStripMenuItem2";
      this.toolStripMenuItem2.Size = new System.Drawing.Size(156, 6);
      // 
      // exitToolStripMenuItem
      // 
      this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
      this.exitToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
      this.exitToolStripMenuItem.Text = "Exit";
      this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitClick);
      // 
      // viewToolStripMenuItem
      // 
      this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetMinMaxMenuItem,
            this.toolStripMenuItem3,
            this.hiddenMenuItem,
            this.plotMenuItem,
            this.toolStripMenuItem1,
            this.columnsToolStripMenuItem});
      this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
      this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
      this.viewToolStripMenuItem.Text = "View";
      // 
      // hiddenMenuItem
      // 
      this.hiddenMenuItem.CheckOnClick = true;
      this.hiddenMenuItem.Name = "hiddenMenuItem";
      this.hiddenMenuItem.Size = new System.Drawing.Size(188, 22);
      this.hiddenMenuItem.Text = "Show Hidden Sensors";
      this.hiddenMenuItem.CheckedChanged += new System.EventHandler(this.hiddenSensorsMenuItem_CheckedChanged);
      // 
      // plotMenuItem
      // 
      this.plotMenuItem.Checked = true;
      this.plotMenuItem.CheckOnClick = true;
      this.plotMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
      this.plotMenuItem.Name = "plotMenuItem";
      this.plotMenuItem.Size = new System.Drawing.Size(188, 22);
      this.plotMenuItem.Text = "Show Plot";
      this.plotMenuItem.CheckedChanged += new System.EventHandler(this.plotToolStripMenuItem_CheckedChanged);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(185, 6);
      // 
      // columnsToolStripMenuItem
      // 
      this.columnsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.valueMenuItem,
            this.minMenuItem,
            this.maxMenuItem});
      this.columnsToolStripMenuItem.Name = "columnsToolStripMenuItem";
      this.columnsToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
      this.columnsToolStripMenuItem.Text = "Columns";
      // 
      // valueMenuItem
      // 
      this.valueMenuItem.Checked = true;
      this.valueMenuItem.CheckOnClick = true;
      this.valueMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
      this.valueMenuItem.Name = "valueMenuItem";
      this.valueMenuItem.Size = new System.Drawing.Size(103, 22);
      this.valueMenuItem.Text = "Value";
      this.valueMenuItem.CheckedChanged += new System.EventHandler(this.valueMenuItem_CheckedChanged);
      // 
      // minMenuItem
      // 
      this.minMenuItem.Checked = true;
      this.minMenuItem.CheckOnClick = true;
      this.minMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
      this.minMenuItem.Name = "minMenuItem";
      this.minMenuItem.Size = new System.Drawing.Size(103, 22);
      this.minMenuItem.Text = "Min";
      this.minMenuItem.CheckedChanged += new System.EventHandler(this.minMenuItem_CheckedChanged);
      // 
      // maxMenuItem
      // 
      this.maxMenuItem.Checked = true;
      this.maxMenuItem.CheckOnClick = true;
      this.maxMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
      this.maxMenuItem.Name = "maxMenuItem";
      this.maxMenuItem.Size = new System.Drawing.Size(103, 22);
      this.maxMenuItem.Text = "Max";
      this.maxMenuItem.CheckedChanged += new System.EventHandler(this.maxMenuItem_CheckedChanged);
      // 
      // optionsToolStripMenuItem
      // 
      this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startMinMenuItem,
            this.minTrayMenuItem,
            this.startupMenuItem,
            this.separatorMenuItem,
            this.temperatureUnitsToolStripMenuItem,
            this.toolStripMenuItem4,
            this.hddMenuItem});
      this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
      this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
      this.optionsToolStripMenuItem.Text = "Options";
      // 
      // startMinMenuItem
      // 
      this.startMinMenuItem.CheckOnClick = true;
      this.startMinMenuItem.Name = "startMinMenuItem";
      this.startMinMenuItem.Size = new System.Drawing.Size(207, 22);
      this.startMinMenuItem.Text = "Start Minimized";
      // 
      // minTrayMenuItem
      // 
      this.minTrayMenuItem.CheckOnClick = true;
      this.minTrayMenuItem.Name = "minTrayMenuItem";
      this.minTrayMenuItem.Size = new System.Drawing.Size(207, 22);
      this.minTrayMenuItem.Text = "Minimize To Tray";
      this.minTrayMenuItem.CheckedChanged += new System.EventHandler(this.minTrayMenuItem_CheckedChanged);
      // 
      // startupMenuItem
      // 
      this.startupMenuItem.CheckOnClick = true;
      this.startupMenuItem.Name = "startupMenuItem";
      this.startupMenuItem.Size = new System.Drawing.Size(207, 22);
      this.startupMenuItem.Text = "Run On Windows Startup";
      this.startupMenuItem.CheckedChanged += new System.EventHandler(this.runOnWindowsStartupToolStripMenuItem_CheckedChanged);
      // 
      // separatorMenuItem
      // 
      this.separatorMenuItem.Name = "separatorMenuItem";
      this.separatorMenuItem.Size = new System.Drawing.Size(204, 6);
      // 
      // temperatureUnitsToolStripMenuItem
      // 
      this.temperatureUnitsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.celciusToolStripMenuItem,
            this.fahrenheitToolStripMenuItem});
      this.temperatureUnitsToolStripMenuItem.Name = "temperatureUnitsToolStripMenuItem";
      this.temperatureUnitsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
      this.temperatureUnitsToolStripMenuItem.Text = "Temperature Unit";
      // 
      // celciusToolStripMenuItem
      // 
      this.celciusToolStripMenuItem.Name = "celciusToolStripMenuItem";
      this.celciusToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
      this.celciusToolStripMenuItem.Text = "Celcius";
      this.celciusToolStripMenuItem.Click += new System.EventHandler(this.celciusToolStripMenuItem_Click);
      // 
      // fahrenheitToolStripMenuItem
      // 
      this.fahrenheitToolStripMenuItem.Name = "fahrenheitToolStripMenuItem";
      this.fahrenheitToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
      this.fahrenheitToolStripMenuItem.Text = "Fahrenheit";
      this.fahrenheitToolStripMenuItem.Click += new System.EventHandler(this.fahrenheitToolStripMenuItem_Click);
      // 
      // toolStripMenuItem4
      // 
      this.toolStripMenuItem4.Name = "toolStripMenuItem4";
      this.toolStripMenuItem4.Size = new System.Drawing.Size(204, 6);
      // 
      // hddMenuItem
      // 
      this.hddMenuItem.CheckOnClick = true;
      this.hddMenuItem.Name = "hddMenuItem";
      this.hddMenuItem.Size = new System.Drawing.Size(207, 22);
      this.hddMenuItem.Text = "Read HDD sensors";
      this.hddMenuItem.CheckedChanged += new System.EventHandler(this.hddsensorsToolStripMenuItem_CheckedChanged);
      // 
      // helpToolStripMenuItem
      // 
      this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
      this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
      this.helpToolStripMenuItem.Text = "Help";
      // 
      // aboutToolStripMenuItem
      // 
      this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
      this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
      this.aboutToolStripMenuItem.Text = "About";
      this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
      // 
      // splitContainer
      // 
      this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer.Location = new System.Drawing.Point(0, 24);
      this.splitContainer.Name = "splitContainer";
      this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer.Panel1
      // 
      this.splitContainer.Panel1.Controls.Add(this.treeView);
      // 
      // splitContainer.Panel2
      // 
      this.splitContainer.Panel2.Controls.Add(this.plotPanel);
      this.splitContainer.Panel2.Cursor = System.Windows.Forms.Cursors.Default;
      this.splitContainer.Size = new System.Drawing.Size(410, 662);
      this.splitContainer.SplitterDistance = 488;
      this.splitContainer.SplitterWidth = 3;
      this.splitContainer.TabIndex = 3;
      // 
      // plotPanel
      // 
      this.plotPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.plotPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.plotPanel.Location = new System.Drawing.Point(0, 0);
      this.plotPanel.Name = "plotPanel";
      this.plotPanel.Size = new System.Drawing.Size(410, 171);
      this.plotPanel.TabIndex = 0;
      // 
      // sensorContextMenuStrip
      // 
      this.sensorContextMenuStrip.Name = "sensorContextMenuStrip";
      this.sensorContextMenuStrip.Size = new System.Drawing.Size(61, 4);
      // 
      // saveFileDialog
      // 
      this.saveFileDialog.DefaultExt = "txt";
      this.saveFileDialog.FileName = "OpenHardwareMonitor.Report.txt";
      this.saveFileDialog.Filter = "Text Documents|*.txt|All Files|*.*";
      this.saveFileDialog.RestoreDirectory = true;
      this.saveFileDialog.Title = "Save Report As";
      // 
      // timer
      // 
      this.timer.Interval = 1000;
      this.timer.Tick += new System.EventHandler(this.timer_Tick);
      // 
      // toolStripMenuItem3
      // 
      this.toolStripMenuItem3.Name = "toolStripMenuItem3";
      this.toolStripMenuItem3.Size = new System.Drawing.Size(185, 6);
      // 
      // resetMinMaxMenuItem
      // 
      this.resetMinMaxMenuItem.Name = "resetMinMaxMenuItem";
      this.resetMinMaxMenuItem.Size = new System.Drawing.Size(188, 22);
      this.resetMinMaxMenuItem.Text = "Reset Min/Max";
      this.resetMinMaxMenuItem.Click += new System.EventHandler(this.resetMinMaxMenuItem_Click);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(410, 686);
      this.Controls.Add(this.splitContainer);
      this.Controls.Add(this.menuStrip);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MainMenuStrip = this.menuStrip;
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Open Hardware Monitor";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
      this.menuStrip.ResumeLayout(false);
      this.menuStrip.PerformLayout();
      this.splitContainer.Panel1.ResumeLayout(false);
      this.splitContainer.Panel2.ResumeLayout(false);
      this.splitContainer.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private Aga.Controls.Tree.TreeViewAdv treeView;
    private System.Windows.Forms.MenuStrip menuStrip;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    private Aga.Controls.Tree.TreeColumn sensor;
    private Aga.Controls.Tree.TreeColumn value;
    private Aga.Controls.Tree.TreeColumn min;
    private Aga.Controls.Tree.TreeColumn max;
    private Aga.Controls.Tree.NodeControls.NodeIcon nodeImage;
    private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxText;
    private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxValue;
    private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxMin;
    private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxMax;
    private System.Windows.Forms.SplitContainer splitContainer;
    private PlotPanel plotPanel;
    private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem plotMenuItem;
    private Aga.Controls.Tree.NodeControls.NodeCheckBox nodeCheckBox;
    private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveReportToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem hddMenuItem;
    private System.Windows.Forms.ToolStripMenuItem minTrayMenuItem;
    private System.Windows.Forms.ToolStripSeparator separatorMenuItem;
    private System.Windows.Forms.ContextMenuStrip sensorContextMenuStrip;
    private System.Windows.Forms.ToolStripMenuItem startMinMenuItem;
    private System.Windows.Forms.ToolStripMenuItem startupMenuItem;
    private System.Windows.Forms.SaveFileDialog saveFileDialog;
    private System.Windows.Forms.Timer timer;
    private System.Windows.Forms.ToolStripMenuItem hiddenMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem columnsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem valueMenuItem;
    private System.Windows.Forms.ToolStripMenuItem minMenuItem;
    private System.Windows.Forms.ToolStripMenuItem maxMenuItem;
    private System.Windows.Forms.ToolStripMenuItem temperatureUnitsToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
    private System.Windows.Forms.ToolStripMenuItem celciusToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem fahrenheitToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem sumbitReportToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
    private System.Windows.Forms.ToolStripMenuItem resetMinMaxMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
  }
}

