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

    private PersistentSettings settings;
    private UnitManager unitManager;
    private Computer computer;
    private Node root;
    private TreeModel treeModel;
    private IDictionary<ISensor, Color> sensorPlotColors = 
      new Dictionary<ISensor, Color>();
    private Color[] plotColorPalette;
    private SystemTray systemTray;    
    private StartupManager startupManager = new StartupManager();
    private UpdateVisitor updateVisitor = new UpdateVisitor();
    private SensorGadget gadget;

    private UserOption showHiddenSensors;
    private UserOption showPlot;
    private UserOption showValue;
    private UserOption showMin;
    private UserOption showMax;
    private UserOption startMinimized;
    private UserOption minimizeToTray;
    private UserOption minimizeOnClose;
    private UserOption autoStart;
    private UserOption readHddSensors;
    private UserOption showGadget;

    public MainForm() {      
      InitializeComponent();

      this.settings = new PersistentSettings();      
      this.settings.Load(Path.ChangeExtension(
        Application.ExecutablePath, ".config"));

      this.unitManager = new UnitManager(settings);

      // set the DockStyle here, to avoid conflicts with the MainMenu
      this.splitContainer.Dock = DockStyle.Fill;
            
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

      if (settings.Contains("mainForm.Location.X")) {
        int x = settings.GetValue("mainForm.Location.X", Location.X);
        x = x < 0 ? 0 : x;
        int y = settings.GetValue("mainForm.Location.Y", Location.Y);
        y = y < 0 ? 0 : y;
        this.Location = new Point(x, y);
      } else {
        StartPosition = FormStartPosition.CenterScreen;
      }

      ClientSize = new Size(
        settings.GetValue("mainForm.Width", 470),
        settings.GetValue("mainForm.Height", 640));

      foreach (TreeColumn column in treeView.Columns) 
        column.Width = Math.Max(20, Math.Min(400,
          settings.GetValue("treeView.Columns." + column.Header + ".Width",
          column.Width)));

      treeModel = new TreeModel();
      root = new Node(System.Environment.MachineName);
      root.Image = Utilities.EmbeddedResources.GetImage("computer.png");
      
      treeModel.Nodes.Add(root);
      treeView.Model = treeModel;

      this.computer = new Computer(settings);

      systemTray = new SystemTray(computer, settings);
      systemTray.HideShowCommand += hideShowClick;
      systemTray.ExitCommand += exitClick;

      int p = (int)Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128)) { // Unix
        splitContainer.BorderStyle = BorderStyle.None;
        splitContainer.Border3DStyle = Border3DStyle.Adjust;
        splitContainer.SplitterWidth = 4;
        treeView.BorderStyle = BorderStyle.Fixed3D;
        plotPanel.BorderStyle = BorderStyle.Fixed3D;
        gadgetMenuItem.Visible = false;
        minCloseMenuItem.Visible = false;
      } else { // Windows
        gadget = new SensorGadget(computer, settings, unitManager);
      }          

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

      showHiddenSensors = new UserOption("hiddenMenuItem", false, hiddenMenuItem, settings);
      showHiddenSensors.Changed += delegate(object sender, EventArgs e) {
        treeModel.ForceVisible = showHiddenSensors.Value;
      };

      showPlot = new UserOption("plotMenuItem", false, plotMenuItem, settings);
      showPlot.Changed += delegate(object sender, EventArgs e) {
        splitContainer.Panel2Collapsed = !showPlot.Value;
        treeView.Invalidate();
      };

      showValue = new UserOption("valueMenuItem", true, valueMenuItem, settings);
      showValue.Changed += delegate(object sender, EventArgs e) {
        treeView.Columns[1].IsVisible = showValue.Value;
      };

      showMin = new UserOption("minMenuItem", false, minMenuItem, settings);
      showMin.Changed += delegate(object sender, EventArgs e) {
        treeView.Columns[2].IsVisible = showMin.Value;
      };

      showMax = new UserOption("maxMenuItem", true, maxMenuItem, settings);
      showMax.Changed += delegate(object sender, EventArgs e) {
        treeView.Columns[3].IsVisible = showMax.Value;
      };

      startMinimized = new UserOption("startMinMenuItem", false, startMinMenuItem, settings);

      minimizeToTray = new UserOption("minTrayMenuItem", true, minTrayMenuItem, settings);
      minimizeToTray.Changed += delegate(object sender, EventArgs e) {
        systemTray.IsMainIconEnabled = minimizeToTray.Value;
      };

      minimizeOnClose = new UserOption("minCloseMenuItem", false, minCloseMenuItem, settings);

      autoStart = new UserOption(null, startupManager.Startup, startupMenuItem, settings);
      autoStart.Changed += delegate(object sender, EventArgs e) {
        try {
          startupManager.Startup = autoStart.Value;
        } catch (InvalidOperationException) {
          MessageBox.Show("Updating the auto-startup option failed.", "Error", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
          autoStart.Value = startupManager.Startup;
        }
      };

      readHddSensors = new UserOption("hddMenuItem", true, hddMenuItem, settings);
      readHddSensors.Changed += delegate(object sender, EventArgs e) {
        computer.HDDEnabled = readHddSensors.Value;
        UpdatePlotSelection(null, null);
      };

      showGadget = new UserOption("gadgetMenuItem", false, gadgetMenuItem, settings);
      showGadget.Changed += delegate(object sender, EventArgs e) {
        if (gadget != null) 
          gadget.Visible = showGadget.Value;
      };

      celciusMenuItem.Checked = 
        unitManager.TemperatureUnit == TemperatureUnit.Celcius;
      fahrenheitMenuItem.Checked = !celciusMenuItem.Checked;

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
      Node hardwareNode = new HardwareNode(hardware, settings, unitManager);
      node.Nodes.Add(hardwareNode);
      foreach (IHardware subHardware in hardware.SubHardware)
        SubHardwareAdded(subHardware, hardwareNode);  
    }

    private void HardwareAdded(IHardware hardware) {
      Node hardwareNode = new HardwareNode(hardware, settings, unitManager);
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
      if (gadget != null)
        gadget.Redraw();
    }

    private void SaveConfiguration() {
      if (WindowState != FormWindowState.Minimized) {
        settings.SetValue("mainForm.Location.X", Location.X);
        settings.SetValue("mainForm.Location.Y", Location.Y);
        settings.SetValue("mainForm.Width", ClientSize.Width);
        settings.SetValue("mainForm.Height", ClientSize.Height);
      }

      foreach (TreeColumn column in treeView.Columns)
        settings.SetValue("treeView.Columns." + column.Header + ".Width",
          column.Width);

      settings.Save(Path.ChangeExtension(
        System.Windows.Forms.Application.ExecutablePath, ".config"));
    }

   private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
      Visible = false;
      SaveConfiguration();

      timer.Enabled = false;
      systemTray.Dispose();      
      computer.Close();
    }

    private void aboutMenuItem_Click(object sender, EventArgs e) {
      new AboutBox().ShowDialog();
    }

    private void treeView_Click(object sender, EventArgs e) {
      
      MouseEventArgs m = e as MouseEventArgs;
      if (m == null || m.Button != MouseButtons.Right)
        return;

      NodeControlInfo info = treeView.GetNodeControlInfoAt(new Point(m.X, m.Y));
      treeView.SelectedNode = info.Node;
      if (info.Node != null) {
        SensorNode node = info.Node.Tag as SensorNode;
        if (node != null && node.Sensor != null) {
          sensorContextMenu.MenuItems.Clear();
          if (node.Sensor.Parameters.Length > 0) {
            MenuItem item = new MenuItem("Parameters...");
            item.Click += delegate(object obj, EventArgs args) {
              ShowParameterForm(node.Sensor);
            };
            sensorContextMenu.MenuItems.Add(item);
          }
          if (nodeTextBoxText.EditEnabled) {
            MenuItem item = new MenuItem("Rename");
            item.Click += delegate(object obj, EventArgs args) {
              nodeTextBoxText.BeginEdit();
            };
            sensorContextMenu.MenuItems.Add(item);
          }
          if (node.IsVisible) {
            MenuItem item = new MenuItem("Hide");
            item.Click += delegate(object obj, EventArgs args) {
              node.IsVisible = false;
            };
            sensorContextMenu.MenuItems.Add(item);
          } else {
            MenuItem item = new MenuItem("Unhide");
            item.Click += delegate(object obj, EventArgs args) {
              node.IsVisible = true;
            };
            sensorContextMenu.MenuItems.Add(item);
          }
          sensorContextMenu.MenuItems.Add(new MenuItem("-"));
          {
            MenuItem item = new MenuItem("Show in Tray");
            item.Checked = systemTray.Contains(node.Sensor);
            item.Click += delegate(object obj, EventArgs args) {
              if (item.Checked)
                systemTray.Remove(node.Sensor);
              else
                systemTray.Add(node.Sensor, true);
            };
            sensorContextMenu.MenuItems.Add(item);
          }
          if (gadget != null) {
            MenuItem item = new MenuItem("Show in Gadget");
            item.Checked = gadget.Contains(node.Sensor);
            item.Click += delegate(object obj, EventArgs args) {
              if (item.Checked) {
                gadget.Remove(node.Sensor);
              } else {
                gadget.Add(node.Sensor);
              }
            };
            sensorContextMenu.MenuItems.Add(item);
          }

          sensorContextMenu.Show(treeView, new Point(m.X, m.Y));
        }
      }
    }

    private void saveReportMenuItem_Click(object sender, EventArgs e) {
      string report = computer.GetReport();
      if (saveFileDialog.ShowDialog() == DialogResult.OK) {
        using (TextWriter w = new StreamWriter(saveFileDialog.FileName)) {
          w.Write(report);
        }
      }
    }

    private void SysTrayHideShow() {
      Visible = !Visible;
      if (Visible)
        Activate();    
    }

    protected override void WndProc(ref Message m) {
      const int WM_SYSCOMMAND = 0x112;
      const int SC_MINIMIZE = 0xF020;
      const int SC_CLOSE = 0xF060;

      if (minimizeToTray.Value && 
        m.Msg == WM_SYSCOMMAND && m.WParam.ToInt32() == SC_MINIMIZE) {
        SysTrayHideShow();
      } else if(minimizeOnClose.Value && 
        m.Msg == WM_SYSCOMMAND && m.WParam.ToInt32() == SC_CLOSE) {
        /*
         * Apparently the user wants to minimize rather than close
         * Now we still need to check if we're going to the tray or not
         * 
         * Note: the correct way to do this would be to send out SC_MINIMIZE,
         * but since the code here is so simple,
         * that would just be a waste of time.
         */
        if (minimizeToTray.Value)
          SysTrayHideShow();
        else
          WindowState = FormWindowState.Minimized;
      } else {      
        base.WndProc(ref m);
      }
    }

    private void hideShowClick(object sender, EventArgs e) {
      SysTrayHideShow();
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

    private void celciusMenuItem_Click(object sender, EventArgs e) {
      celciusMenuItem.Checked = true;
      fahrenheitMenuItem.Checked = false;
      unitManager.TemperatureUnit = TemperatureUnit.Celcius;
    }

    private void fahrenheitMenuItem_Click(object sender, EventArgs e) {
      celciusMenuItem.Checked = false;
      fahrenheitMenuItem.Checked = true;
      unitManager.TemperatureUnit = TemperatureUnit.Fahrenheit;
    }

    private void sumbitReportMenuItem_Click(object sender, EventArgs e) 
    {
      ReportForm form = new ReportForm();
      form.Report = computer.GetReport();
      form.ShowDialog();      
    }

    private void resetMinMaxMenuItem_Click(object sender, EventArgs e) {
      computer.Accept(new SensorVisitor(delegate(ISensor sensor) {
        sensor.ResetMin();
        sensor.ResetMax();
      }));
    }
  }
}
