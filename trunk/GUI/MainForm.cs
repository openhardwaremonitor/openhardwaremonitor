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

namespace OpenHardwareMonitor.GUI {
  public partial class MainForm : Form {

    private Node root;
    private List<IGroup> groupList = new List<IGroup>();
    private TreeModel treeModel;
    private IDictionary<ISensor, Color> sensorPlotColors = 
      new Dictionary<ISensor, Color>();
    private Color[] plotColorPalette;

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

      AddGroup(new Hardware.SMBIOS.SMBIOSGroup());
      AddGroup(new Hardware.LPC.LPCGroup());
      AddGroup(new Hardware.CPU.CPUGroup());
      AddGroup(new Hardware.ATI.ATIGroup());
      AddGroup(new Hardware.Nvidia.NvidiaGroup());
      AddGroup(new Hardware.TBalancer.TBalancerGroup());
      
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

      plotMenuItem.Checked = Utilities.Config.Get(plotMenuItem.Name, false);
      minMenuItem.Checked = Utilities.Config.Get(minMenuItem.Name, false);
      maxMenuItem.Checked = Utilities.Config.Get(maxMenuItem.Name, true);
      limitMenuItem.Checked = Utilities.Config.Get(limitMenuItem.Name, false);
      hddMenuItem.Checked = Utilities.Config.Get(hddMenuItem.Name, true);

      voltMenuItem.Checked = Utilities.Config.Get(voltMenuItem.Name, true);
      clocksMenuItem.Checked = Utilities.Config.Get(clocksMenuItem.Name, true);
      tempMenuItem.Checked = Utilities.Config.Get(tempMenuItem.Name, true);
      fansMenuItem.Checked = Utilities.Config.Get(fansMenuItem.Name, true);

      timer.Enabled = true;
    }

    private void AddGroup(IGroup group) {
      groupList.Add(group);
      foreach (IHardware hardware in group.Hardware)
        root.Nodes.Add(new HardwareNode(hardware));
    }

    private void RemoveGroup(IGroup group) {
      List<Node> nodesToRemove = new List<Node>();
      foreach (IHardware hardware in group.Hardware)
        foreach (Node node in root.Nodes) {
          HardwareNode hardwareNode = node as HardwareNode;
          if (hardwareNode != null && hardwareNode.Hardware == hardware)
            nodesToRemove.Add(node);
        }
      foreach (Node node in nodesToRemove)
        root.Nodes.Remove(node);
      groupList.Remove(group);
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
        foreach (IGroup group in groupList)
          foreach (IHardware hardware in group.Hardware)
            hardware.Update();
      #if !DEBUG
      } catch (Exception exception) {
        Utilities.CrashReport.Save(exception);
        Close();
      }
      #endif
            
      treeView.Invalidate();
      plotPanel.Invalidate();
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
      Utilities.Config.Set(plotMenuItem.Name, plotMenuItem.Checked);
      Utilities.Config.Set(minMenuItem.Name, minMenuItem.Checked);
      Utilities.Config.Set(maxMenuItem.Name, maxMenuItem.Checked);
      Utilities.Config.Set(limitMenuItem.Name, limitMenuItem.Checked);
      Utilities.Config.Set(hddMenuItem.Name, hddMenuItem.Checked);

      Utilities.Config.Set(voltMenuItem.Name, voltMenuItem.Checked);
      Utilities.Config.Set(clocksMenuItem.Name, clocksMenuItem.Checked);
      Utilities.Config.Set(tempMenuItem.Name, tempMenuItem.Checked);
      Utilities.Config.Set(fansMenuItem.Name, fansMenuItem.Checked);

      Utilities.Config.Set("mainForm.Location.X", Location.X);
      Utilities.Config.Set("mainForm.Location.Y", Location.Y);
      Utilities.Config.Set("mainForm.Width", Width);
      Utilities.Config.Set("mainForm.Height", Height);

      foreach (IGroup group in groupList)
        group.Close();
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
      if (info.Control == null)
        columnsContextMenuStrip.Show(treeView, m.X, m.Y);
    }

    private void saveReportToolStripMenuItem_Click(object sender, EventArgs e) {
      ReportWriter.Save(groupList, new Version(Application.ProductVersion));
    }

    private void hddsensorsToolStripMenuItem_CheckedChanged(object sender, 
      EventArgs e) 
    {
      if (hddMenuItem.Checked) {
        AddGroup(new Hardware.HDD.HDDGroup());
        UpdateSensorTypeChecked(null, null);
      } else {
        List<IGroup> groupsToRemove = new List<IGroup>();
        foreach (IGroup group in groupList) 
          if (group is Hardware.HDD.HDDGroup)
            groupsToRemove.Add(group);
        foreach (IGroup group in groupsToRemove) {
          group.Close();
          RemoveGroup(group);
        }
        UpdatePlotSelection(null, null);        
      }
    }

    private void UpdateSensorTypeChecked(object sender, EventArgs e) {
      foreach (HardwareNode node in root.Nodes) {
        node.SetVisible(SensorType.Voltage, voltMenuItem.Checked);
        node.SetVisible(SensorType.Clock, clocksMenuItem.Checked);
        node.SetVisible(SensorType.Temperature, tempMenuItem.Checked);
        node.SetVisible(SensorType.Fan, fansMenuItem.Checked);
      }
    }  
  }
}
