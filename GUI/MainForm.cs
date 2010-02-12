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
    private SensorSystemTray sensorSystemTray;
    private NotifyIcon notifyIcon;

    public MainForm() {      
      InitializeComponent();
      this.Font = SystemFonts.MessageBoxFont;
      treeView.Font = SystemFonts.MessageBoxFont;
      plotPanel.Font = SystemFonts.MessageBoxFont;      
      
      nodeCheckBox.IsVisibleValueNeeded += 
        new EventHandler<NodeControlValueEventArgs>(
          nodeCheckBox_IsVisibleValueNeeded);
      nodeCheckBox.CheckStateChanged += 
        new EventHandler<TreePathEventArgs>(UpdatePlotSelection);
      nodeTextBoxText.DrawText += 
        new EventHandler<DrawEventArgs>(nodeTextBoxText_DrawText);
      nodeTextBoxValue.DrawText +=
        new EventHandler<DrawEventArgs>(nodeTextBoxText_DrawText);
      nodeTextBoxMin.DrawText +=
        new EventHandler<DrawEventArgs>(nodeTextBoxText_DrawText);
      nodeTextBoxMax.DrawText +=
        new EventHandler<DrawEventArgs>(nodeTextBoxText_DrawText);
      nodeTextBoxLimit.DrawText += 
        new EventHandler<DrawEventArgs>(nodeTextBoxLimit_DrawText);

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
         
      treeModel = new TreeModel();
      root = new Node(System.Environment.MachineName);
      root.Image = Utilities.EmbeddedResources.GetImage("computer.png");
      
      treeModel.Nodes.Add(root);
      treeView.Model = treeModel;
      
      notifyIcon = new NotifyIcon();
      notifyIcon.ContextMenuStrip = this.notifyContextMenuStrip;
      notifyIcon.Icon = EmbeddedResources.GetIcon("smallicon.ico");
      notifyIcon.Text = "Open Hardware Monitor";      
      notifyIcon.DoubleClick += new EventHandler(this.restoreClick);

      sensorSystemTray = new SensorSystemTray(computer);

      computer.HardwareAdded += new HardwareEventHandler(HardwareAdded);
      computer.HardwareRemoved += new HardwareEventHandler(HardwareRemoved);
      computer.Open();

      plotColorPalette = new Color[14];
      plotColorPalette[0] = Color.Blue;
      plotColorPalette[1] = Color.OrangeRed;
      plotColorPalette[2] = Color.Green;
      plotColorPalette[3] = Color.LightSeaGreen;
      plotColorPalette[4] = Color.Goldenrod;
      plotColorPalette[5] = Color.DarkViolet;
      plotColorPalette[6] = Color.YellowGreen;
      plotColorPalette[7] = Color.SaddleBrown;
      plotColorPalette[8] = Color.Gray;
      plotColorPalette[9] = Color.RoyalBlue;
      plotColorPalette[10] = Color.DeepPink;
      plotColorPalette[11] = Color.MediumSeaGreen;
      plotColorPalette[12] = Color.Olive;
      plotColorPalette[13] = Color.Firebrick;

      plotMenuItem.Checked = Config.Get(plotMenuItem.Name, false);
      minMenuItem.Checked = Config.Get(minMenuItem.Name, false);
      maxMenuItem.Checked = Config.Get(maxMenuItem.Name, true);
      limitMenuItem.Checked = Config.Get(limitMenuItem.Name, false);

      minTrayMenuItem.Checked = Config.Get(minTrayMenuItem.Name, true);
      hddMenuItem.Checked = Config.Get(hddMenuItem.Name, true);

      voltMenuItem.Checked = Config.Get(voltMenuItem.Name, true);
      clocksMenuItem.Checked = Config.Get(clocksMenuItem.Name, true);
      loadMenuItem.Checked = Config.Get(loadMenuItem.Name, true);
      tempMenuItem.Checked = Config.Get(tempMenuItem.Name, true);
      fansMenuItem.Checked = Config.Get(fansMenuItem.Name, true);
     
      timer.Enabled = true;   
    }

    private void HardwareAdded(IHardware hardware) {
      root.Nodes.Add(new HardwareNode(hardware));
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

    private void nodeTextBoxLimit_DrawText(object sender, DrawEventArgs e) {
      SensorNode sensorNode = e.Node.Tag as SensorNode;
      if (sensorNode != null) 
        e.Text = sensorNode.ValueToString(sensorNode.Sensor.Limit);
    }

    private void nodeTextBoxText_DrawText(object sender, DrawEventArgs e) {
      if (!plotMenuItem.Checked)
        return;      

      SensorNode sensorNode = e.Node.Tag as SensorNode;
      if (sensorNode != null) {
        Color color;
        if (sensorPlotColors.TryGetValue(sensorNode.Sensor, out color)) 
          e.TextColor = color;        
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

    private void nodeCheckBox_IsVisibleValueNeeded(object sender, 
      NodeControlValueEventArgs e) {
      SensorNode node = e.Node.Tag as SensorNode;
      e.Value = (node != null) && 
        (node.Sensor.SensorType == SensorType.Temperature) && 
        plotMenuItem.Checked;
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
      Close();
    }

    private void timer_Tick(object sender, EventArgs e) {
      
      #if !DEBUG
      try {
      #endif
        computer.Update();        
      #if !DEBUG
      } catch (Exception exception) {
        CrashReport.Save(exception);
        Close();
      }
      #endif
            
      treeView.Invalidate();
      plotPanel.Invalidate();
      sensorSystemTray.Redraw();
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
            
      Config.Set(plotMenuItem.Name, plotMenuItem.Checked);
      Config.Set(minMenuItem.Name, minMenuItem.Checked);
      Config.Set(maxMenuItem.Name, maxMenuItem.Checked);
      Config.Set(limitMenuItem.Name, limitMenuItem.Checked);

      Config.Set(minTrayMenuItem.Name, minTrayMenuItem.Checked);
      Config.Set(hddMenuItem.Name, hddMenuItem.Checked);

      Config.Set(voltMenuItem.Name, voltMenuItem.Checked);
      Config.Set(clocksMenuItem.Name, clocksMenuItem.Checked);
      Config.Set(loadMenuItem.Name, loadMenuItem.Checked);
      Config.Set(tempMenuItem.Name, tempMenuItem.Checked);
      Config.Set(fansMenuItem.Name, fansMenuItem.Checked);

      if (WindowState != FormWindowState.Minimized) {
        Config.Set("mainForm.Location.X", Location.X);
        Config.Set("mainForm.Location.Y", Location.Y);
        Config.Set("mainForm.Width", Width);
        Config.Set("mainForm.Height", Height);
      }
           
      sensorSystemTray.Dispose();
      notifyIcon.Dispose();
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

    private void valueToolStripMenuItem_CheckedChanged(object sender, 
      EventArgs e) 
    {
      treeView.Columns[1].IsVisible = valueToolStripMenuItem.Checked;
    }

    private void minToolStripMenuItem_CheckedChanged(object sender, EventArgs e) 
    {
      treeView.Columns[2].IsVisible = minMenuItem.Checked;
    }

    private void maxToolStripMenuItem_CheckedChanged(object sender, EventArgs e) 
    {
      treeView.Columns[3].IsVisible = maxMenuItem.Checked;
    }

    private void limitToolStripMenuItem_CheckedChanged(object sender, 
      EventArgs e) {
      treeView.Columns[4].IsVisible = limitMenuItem.Checked;
    }

    private void treeView_Click(object sender, EventArgs e) {
      
      MouseEventArgs m = e as MouseEventArgs;
      if (m == null || m.Button != MouseButtons.Right)
        return;

      NodeControlInfo info = treeView.GetNodeControlInfoAt(new Point(m.X, m.Y));
      if (info.Control == null) {
        columnsContextMenuStrip.Show(treeView, m.X, m.Y);
      } else {
        SensorNode node = info.Node.Tag as SensorNode;
        if (node != null && node.Sensor != null) {

          sensorContextMenuStrip.Items.Clear();
          if (sensorSystemTray.Contains(node.Sensor)) {
            ToolStripMenuItem item = new ToolStripMenuItem("Remove From Tray");
            item.Click += delegate(object obj, EventArgs args) {
              sensorSystemTray.Remove(node.Sensor);
            };
            sensorContextMenuStrip.Items.Add(item);
          } else {
            ToolStripMenuItem item = new ToolStripMenuItem("Add To Tray");
            item.Click += delegate(object obj, EventArgs args) {
              sensorSystemTray.Add(node.Sensor, true);
            };
            sensorContextMenuStrip.Items.Add(item);
          }
          sensorContextMenuStrip.Show(treeView, m.X, m.Y);
        }
      }
    }

    private void saveReportToolStripMenuItem_Click(object sender, EventArgs e) {
      computer.SaveReport(new Version(Application.ProductVersion));      
    }

    private void hddsensorsToolStripMenuItem_CheckedChanged(object sender, 
      EventArgs e) 
    {
      computer.HDDEnabled = hddMenuItem.Checked;
      UpdateSensorTypeChecked(null, null);
      UpdatePlotSelection(null, null);      
    }

    private void UpdateSensorTypeChecked(object sender, EventArgs e) {
      foreach (HardwareNode node in root.Nodes) {
        node.SetVisible(SensorType.Voltage, voltMenuItem.Checked);
        node.SetVisible(SensorType.Clock, clocksMenuItem.Checked);
        node.SetVisible(SensorType.Load, loadMenuItem.Checked);
        node.SetVisible(SensorType.Temperature, tempMenuItem.Checked);
        node.SetVisible(SensorType.Fan, fansMenuItem.Checked);
      }
    }

    private void ToggleSysTray() {
      if (Visible) {
        notifyIcon.Visible = true;
        Visible = false;        
      } else {
        Visible = true;
        notifyIcon.Visible = false;
        Activate();
      }
    }

    protected override void WndProc(ref Message m) {
      const int WM_SYSCOMMAND = 0x112;
      const int SC_MINIMIZE = 0xF020;
      if (minTrayMenuItem.Checked && 
        m.Msg == WM_SYSCOMMAND && m.WParam.ToInt32() == SC_MINIMIZE) {
        ToggleSysTray();
      } else {      
        base.WndProc(ref m);
      }
    }

    private void restoreClick(object sender, EventArgs e) {
      ToggleSysTray();
    }

    private void removeToolStripMenuItem_Click(object sender, EventArgs e) {
      ToolStripMenuItem item = sender as ToolStripMenuItem;
      if (item == null)
        return;

      ISensor sensor = item.Owner.Tag as ISensor;
      if (sensor == null)
        return;

      sensorSystemTray.Remove(sensor);
    }

  }
}
