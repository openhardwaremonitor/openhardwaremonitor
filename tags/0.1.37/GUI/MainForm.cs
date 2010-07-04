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
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.GUI {
  public partial class MainForm : Form {

    private Computer computer = new Computer();
    private Node root;
    private TreeModel treeModel;
    private IDictionary<ISensor, Color> sensorPlotColors = 
      new Dictionary<ISensor, Color>();
    private Color[] plotColorPalette;
    private SystemTray systemTray;    
    private StartupManager startupManager = new StartupManager();
    private UpdateVisitor updateVisitor = new UpdateVisitor();

    public MainForm() {      
      InitializeComponent();
      this.Font = SystemFonts.MessageBoxFont;
      treeView.Font = SystemFonts.MessageBoxFont;
      plotPanel.Font = SystemFonts.MessageBoxFont;
      
      nodeCheckBox.IsVisibleValueNeeded += nodeCheckBox_IsVisibleValueNeeded;
      nodeCheckBox.CheckStateChanged += UpdatePlotSelection;
      nodeTextBoxText.DrawText += nodeTextBoxText_DrawText;
      nodeTextBoxValue.DrawText += nodeTextBoxText_DrawText;
      nodeTextBoxMin.DrawText += nodeTextBoxText_DrawText;
      nodeTextBoxMax.DrawText += nodeTextBoxText_DrawText;
      nodeTextBoxText.EditorShowing += nodeTextBoxText_EditorShowing;

      if (Utilities.Config.Contains("mainForm.Location.X")) {
        int x = Utilities.Config.Get("mainForm.Location.X", Location.X);
        x = x < 0 ? 0 : x;
        int y = Utilities.Config.Get("mainForm.Location.Y", Location.Y);
        y = y < 0 ? 0 : y;
        this.Location = new Point(x, y);
      } else {
        StartPosition = FormStartPosition.CenterScreen;
      }

      Width = Utilities.Config.Get("mainForm.Width", Width);
      Height = Utilities.Config.Get("mainForm.Height", Height);

      foreach (TreeColumn column in treeView.Columns) 
        column.Width = Math.Max(20, Math.Min(400, 
          Config.Get("treeView.Columns." + column.Header + ".Width",
          column.Width)));

      treeModel = new TreeModel();
      root = new Node(System.Environment.MachineName);
      root.Image = Utilities.EmbeddedResources.GetImage("computer.png");
      
      treeModel.Nodes.Add(root);
      treeView.Model = treeModel;     

      systemTray = new SystemTray(computer);
      systemTray.HideShowCommand += hideShowClick;
      systemTray.ExitCommand += exitClick;

      computer.HardwareAdded += new HardwareEventHandler(HardwareAdded);
      computer.HardwareRemoved += new HardwareEventHandler(HardwareRemoved);
      computer.Open();

      timer.Enabled = true;

      plotColorPalette = new Color[13];
      plotColorPalette[0] = Color.Blue;
      plotColorPalette[1] = Color.OrangeRed;
      plotColorPalette[2] = Color.Green;
      plotColorPalette[3] = Color.LightSeaGreen;
      plotColorPalette[4] = Color.Goldenrod;
      plotColorPalette[5] = Color.DarkViolet;
      plotColorPalette[6] = Color.YellowGreen;
      plotColorPalette[7] = Color.SaddleBrown;
      plotColorPalette[8] = Color.RoyalBlue;
      plotColorPalette[9] = Color.DeepPink;
      plotColorPalette[10] = Color.MediumSeaGreen;
      plotColorPalette[11] = Color.Olive;
      plotColorPalette[12] = Color.Firebrick;

      hiddenMenuItem.Checked = Config.Get(hiddenMenuItem.Name, false);
      plotMenuItem.Checked = Config.Get(plotMenuItem.Name, false);

      valueMenuItem.Checked = Config.Get(valueMenuItem.Name, true);
      minMenuItem.Checked = Config.Get(minMenuItem.Name, false);
      maxMenuItem.Checked = Config.Get(maxMenuItem.Name, true);

      startMinMenuItem.Checked = Config.Get(startMinMenuItem.Name, false); 
      minTrayMenuItem.Checked = Config.Get(minTrayMenuItem.Name, true);
      startupMenuItem.Checked = startupManager.Startup;
      hddMenuItem.Checked = Config.Get(hddMenuItem.Name, true);

      celciusToolStripMenuItem.Checked = 
        UnitManager.TemperatureUnit == TemperatureUnit.Celcius;
      fahrenheitToolStripMenuItem.Checked = !celciusToolStripMenuItem.Checked;

      startupMenuItem.Visible = startupManager.IsAvailable;
      
      if (startMinMenuItem.Checked) {
        if (!minTrayMenuItem.Checked) {
          WindowState = FormWindowState.Minimized;
          Show();
        }
      } else {
        Show();
      }

      // Create a handle, otherwise calling Close() does not fire FormClosed     
      IntPtr handle = Handle;

      // Make sure the settings are saved when the user logs off
      Microsoft.Win32.SystemEvents.SessionEnded +=
        delegate(object sender, Microsoft.Win32.SessionEndedEventArgs e) {
          SaveConfiguration();
        };
    }
    
    private void SubHardwareAdded(IHardware hardware, Node node) {
      Node hardwareNode = new HardwareNode(hardware);
      node.Nodes.Add(hardwareNode);
      foreach (IHardware subHardware in hardware.SubHardware)
        SubHardwareAdded(subHardware, hardwareNode);  
    }

    private void HardwareAdded(IHardware hardware) {
      Node hardwareNode = new HardwareNode(hardware);
      root.Nodes.Add(hardwareNode);
      foreach (IHardware subHardware in hardware.SubHardware)
        SubHardwareAdded(subHardware, hardwareNode);     
    }

    private void HardwareRemoved(IHardware hardware) {      
      List<Node> nodesToRemove = new List<Node>();
      foreach (Node node in root.Nodes) {
        HardwareNode hardwareNode = node as HardwareNode;
        if (hardwareNode != null && hardwareNode.Hardware == hardware)
          nodesToRemove.Add(node);
      }
      foreach (Node node in nodesToRemove)
        root.Nodes.Remove(node);
    }

    private void nodeTextBoxText_DrawText(object sender, DrawEventArgs e) {       
      Node node = e.Node.Tag as Node;
      if (node != null) {
        Color color;
        if (node.IsVisible) {
          SensorNode sensorNode = node as SensorNode;
          if (plotMenuItem.Checked && sensorNode != null &&
            sensorPlotColors.TryGetValue(sensorNode.Sensor, out color))
            e.TextColor = color;
        } else {
          e.TextColor = Color.DarkGray;
        }
      }
    }

    private void UpdatePlotSelection(object sender, 
      TreePathEventArgs e) 
    {
      List<ISensor> selected = new List<ISensor>();
      IDictionary<ISensor, Color> colors = new Dictionary<ISensor, Color>();
      int colorIndex = 0;
      foreach (TreeNodeAdv node in treeView.AllNodes) {
        SensorNode sensorNode = node.Tag as SensorNode;
        if (sensorNode != null && 
          sensorNode.Sensor.SensorType == SensorType.Temperature) {
          if (sensorNode.Plot) {
            colors.Add(sensorNode.Sensor,
              plotColorPalette[colorIndex % plotColorPalette.Length]);
            selected.Add(sensorNode.Sensor);
          }
          colorIndex++;
        }
      }
      sensorPlotColors = colors;
      plotPanel.SetSensors(selected, colors);
    }

    private void nodeTextBoxText_EditorShowing(object sender, CancelEventArgs e) 
    {
      e.Cancel = !(treeView.CurrentNode != null &&
        treeView.CurrentNode.Tag is SensorNode);
    }

    private void nodeCheckBox_IsVisibleValueNeeded(object sender, 
      NodeControlValueEventArgs e) {
      SensorNode node = e.Node.Tag as SensorNode;
      e.Value = (node != null) && 
        (node.Sensor.SensorType == SensorType.Temperature) && 
        plotMenuItem.Checked;
    }

    private void exitClick(object sender, EventArgs e) {
      Close();      
    }

    private void timer_Tick(object sender, EventArgs e) {
      computer.Accept(updateVisitor);
      treeView.Invalidate();
      plotPanel.Invalidate();
      systemTray.Redraw();
    }

    private void SaveConfiguration() {
      Config.Set(hiddenMenuItem.Name, hiddenMenuItem.Checked);
      Config.Set(plotMenuItem.Name, plotMenuItem.Checked);

      Config.Set(valueMenuItem.Name, valueMenuItem.Checked);
      Config.Set(minMenuItem.Name, minMenuItem.Checked);
      Config.Set(maxMenuItem.Name, maxMenuItem.Checked);

      Config.Set(startMinMenuItem.Name, startMinMenuItem.Checked);
      Config.Set(minTrayMenuItem.Name, minTrayMenuItem.Checked);
      Config.Set(hddMenuItem.Name, hddMenuItem.Checked);

      if (WindowState != FormWindowState.Minimized) {
        Config.Set("mainForm.Location.X", Location.X);
        Config.Set("mainForm.Location.Y", Location.Y);
        Config.Set("mainForm.Width", Width);
        Config.Set("mainForm.Height", Height);
      }

      foreach (TreeColumn column in treeView.Columns)
        Config.Set("treeView.Columns." + column.Header + ".Width",
          column.Width);

      Config.Save();
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
      SaveConfiguration();

      timer.Enabled = false;
      systemTray.Dispose();      
      computer.Close();
    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
      new AboutBox().ShowDialog();
    }

    private void plotToolStripMenuItem_CheckedChanged(object sender, 
      EventArgs e) 
    {
      splitContainer.Panel2Collapsed = !plotMenuItem.Checked;
      treeView.Invalidate();
    }

    private void treeView_Click(object sender, EventArgs e) {
      
      MouseEventArgs m = e as MouseEventArgs;
      if (m == null || m.Button != MouseButtons.Right)
        return;

      NodeControlInfo info = treeView.GetNodeControlInfoAt(new Point(m.X, m.Y));
      if (info.Control != null) {
        SensorNode node = info.Node.Tag as SensorNode;
        if (node != null && node.Sensor != null) {

          sensorContextMenuStrip.Items.Clear();
          if (node.Sensor.Parameters.Length > 0) {
            ToolStripMenuItem item = new ToolStripMenuItem("Parameters...");
            item.Click += delegate(object obj, EventArgs args) {
              ShowParameterForm(node.Sensor);
            };
            sensorContextMenuStrip.Items.Add(item);
          }
          EditableControl control = info.Control as EditableControl;
          if (control != null) {
            ToolStripMenuItem item = new ToolStripMenuItem("Rename");
            item.Click += delegate(object obj, EventArgs args) {
              control.BeginEdit();
            };
            sensorContextMenuStrip.Items.Add(item);
          }          
          if (node.IsVisible) {
            ToolStripMenuItem item = new ToolStripMenuItem("Hide");
            item.Click += delegate(object obj, EventArgs args) {
              node.IsVisible = false;
            };
            sensorContextMenuStrip.Items.Add(item);
          } else {
            ToolStripMenuItem item = new ToolStripMenuItem("Unhide");
            item.Click += delegate(object obj, EventArgs args) {
              node.IsVisible = true;
            };
            sensorContextMenuStrip.Items.Add(item);
          }         
          if (systemTray.Contains(node.Sensor)) {
            ToolStripMenuItem item = new ToolStripMenuItem("Remove From Tray");
            item.Click += delegate(object obj, EventArgs args) {
              systemTray.Remove(node.Sensor);
            };
            sensorContextMenuStrip.Items.Add(item);
          } else {
            ToolStripMenuItem item = new ToolStripMenuItem("Add To Tray");
            item.Click += delegate(object obj, EventArgs args) {
              systemTray.Add(node.Sensor, true);
            };
            sensorContextMenuStrip.Items.Add(item);
          }
          sensorContextMenuStrip.Show(treeView, m.X, m.Y);
        }
      }
    }

    private void saveReportToolStripMenuItem_Click(object sender, EventArgs e) {
      string report = computer.GetReport();
      if (saveFileDialog.ShowDialog() == DialogResult.OK) {
        using (TextWriter w = new StreamWriter(saveFileDialog.FileName)) {
          w.Write(report);
        }
      }
    }

    private void hddsensorsToolStripMenuItem_CheckedChanged(object sender, 
      EventArgs e) 
    {
      computer.HDDEnabled = hddMenuItem.Checked;
      UpdatePlotSelection(null, null);      
    }

    private void SysTrayHideShow() {
      Visible = !Visible;
      if (Visible)
        Activate();    
    }

    protected override void WndProc(ref Message m) {
      const int WM_SYSCOMMAND = 0x112;
      const int SC_MINIMIZE = 0xF020;
      if (minTrayMenuItem.Checked && 
        m.Msg == WM_SYSCOMMAND && m.WParam.ToInt32() == SC_MINIMIZE) {
        SysTrayHideShow();
      } else {      
        base.WndProc(ref m);
      }
    }

    private void hideShowClick(object sender, EventArgs e) {
      SysTrayHideShow();
    }

    private void removeToolStripMenuItem_Click(object sender, EventArgs e) {
      ToolStripMenuItem item = sender as ToolStripMenuItem;
      if (item == null)
        return;

      ISensor sensor = item.Owner.Tag as ISensor;
      if (sensor == null)
        return;

      systemTray.Remove(sensor);
    }

    private void ShowParameterForm(ISensor sensor) {
      ParameterForm form = new ParameterForm();
      form.Parameters = sensor.Parameters;
      form.captionLabel.Text = sensor.Name;
      form.ShowDialog();
    }

    private void treeView_NodeMouseDoubleClick(object sender, 
      TreeNodeAdvMouseEventArgs e) {
      SensorNode node = e.Node.Tag as SensorNode;
      if (node != null && node.Sensor != null && 
        node.Sensor.Parameters.Length > 0) {
        ShowParameterForm(node.Sensor);
      }
    }

    private void runOnWindowsStartupToolStripMenuItem_CheckedChanged(
      object sender, EventArgs e) 
    {
      startupManager.Startup = startupMenuItem.Checked;
    }

    private void minTrayMenuItem_CheckedChanged(object sender, EventArgs e) {
      systemTray.IsMainIconEnabled = minTrayMenuItem.Checked;
    }

    private void hiddenSensorsMenuItem_CheckedChanged(object sender, 
      EventArgs e) {
      treeModel.ForceVisible = hiddenMenuItem.Checked;
    }

    private void valueMenuItem_CheckedChanged(object sender, EventArgs e) {
      treeView.Columns[1].IsVisible = valueMenuItem.Checked;
    }

    private void minMenuItem_CheckedChanged(object sender, EventArgs e) {
      treeView.Columns[2].IsVisible = minMenuItem.Checked;
    }

    private void maxMenuItem_CheckedChanged(object sender, EventArgs e) {
      treeView.Columns[3].IsVisible = maxMenuItem.Checked;
    }

    private void celciusToolStripMenuItem_Click(object sender, EventArgs e) {
      celciusToolStripMenuItem.Checked = true;
      fahrenheitToolStripMenuItem.Checked = false;
      UnitManager.TemperatureUnit = TemperatureUnit.Celcius;
    }

    private void fahrenheitToolStripMenuItem_Click(object sender, EventArgs e) {
      celciusToolStripMenuItem.Checked = false;
      fahrenheitToolStripMenuItem.Checked = true;
      UnitManager.TemperatureUnit = TemperatureUnit.Fahrenheit;
    }

    private void sumbitReportToolStripMenuItem_Click(object sender, EventArgs e) 
    {
      ReportForm form = new ReportForm();
      form.Report = computer.GetReport();
      form.ShowDialog();      
    }

    private void resetMinMaxMenuItem_Click(object sender, EventArgs e) {
      IVisitor visitor = new ResetMinMaxVisitor();
      computer.Accept(visitor);
    }
  }
}
