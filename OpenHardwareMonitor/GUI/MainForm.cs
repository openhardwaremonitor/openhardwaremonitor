/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2020 Michael Möller <mmoeller@openhardwaremonitor.org>
	Copyright (C) 2010 Paul Werelds <paul@werelds.net>
	Copyright (C) 2012 Prince Samuel <prince.samuel@gmail.com>

*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;
using OpenHardwareMonitorLib;

namespace OpenHardwareMonitor.GUI
{
    public partial class MainForm : Form
    {

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
        private readonly UserOption showGadgetTopmost;
        private Form plotForm;
        private PlotPanel plotPanel;

        private UserOption showHiddenSensors;
        private UserOption showPlot;
        private UserOption showValue;
        private UserOption showMin;
        private UserOption showMax;
        private UserOption startMinimized;
        private UserOption minimizeToTray;
        private UserOption minimizeOnClose;
        private UserOption autoStart;
        private UserOption autoStartAsService;

        private UserOption readMainboardSensors;
        private UserOption readCpuSensors;
        private UserOption readRamSensors;
        private UserOption readGpuSensors;
        private UserOption readFanControllersSensors;
        private UserOption readHddSensors;
        private UserOption readHddSensorsRemovable;
        private UserOption readNetworkSensors;

        private UserOption showGadget;
        private UserRadioGroup plotLocation;

        private UserOption runWebServer;
        private UserOption allowWebServerRemoteAccess;
        private GrapevineServer server;

        private UserOption logSensors;
        private UserRadioGroup loggingInterval;
        private Logger m_valueLogger;
        private readonly SecondInstanceService secondInstanceService = new SecondInstanceService();

        private bool selectionDragging = false;

        public MainForm()
        {
            InitializeComponent();

            // check if the OpenHardwareMonitorLib assembly has the correct version
            if (Assembly.GetAssembly(typeof(Computer)).GetName().Version !=
              Assembly.GetExecutingAssembly().GetName().Version)
            {
                MessageBox.Show(
                  "The version of the file OpenHardwareMonitorLib.dll is incompatible.",
                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            this.settings = new PersistentSettings();
            if (!Program.Arguments.DoNotLoadConfiguration)
            {
                this.settings.Load(Path.ChangeExtension(
                  Application.ExecutablePath, ".config"));
            }

            this.unitManager = new UnitManager(settings);

            // make sure the buffers used for double buffering are not disposed 
            // after each draw call
            BufferedGraphicsManager.Current.MaximumBuffer =
              Screen.PrimaryScreen.Bounds.Size;

            // set the DockStyle here, to avoid conflicts with the MainMenu
            this.splitContainer.Dock = DockStyle.Fill;

            this.Font = SystemFonts.MessageBoxFont;
            treeView.Font = SystemFonts.MessageBoxFont;

            plotPanel = new PlotPanel(settings, unitManager);
            plotPanel.Font = SystemFonts.MessageBoxFont;
            plotPanel.Dock = DockStyle.Fill;

            nodeCheckBox.IsVisibleValueNeeded += nodeCheckBox_IsVisibleValueNeeded;
            nodeTextBoxText.DrawText += nodeTextBoxText_DrawText;
            nodeTextBoxValue.DrawText += nodeTextBoxText_DrawText;
            nodeTextBoxMin.DrawText += nodeTextBoxText_DrawText;
            nodeTextBoxMax.DrawText += nodeTextBoxText_DrawText;
            nodeTextBoxText.EditorShowing += nodeTextBoxText_EditorShowing;

            this.sensor.Width = DpiHelper.LogicalToDeviceUnits(250);
            this.value.Width = DpiHelper.LogicalToDeviceUnits(100);
            this.min.Width = DpiHelper.LogicalToDeviceUnits(100);
            this.max.Width = DpiHelper.LogicalToDeviceUnits(100);

            foreach (TreeColumn column in treeView.Columns)
                column.Width = Math.Max(DpiHelper.LogicalToDeviceUnits(20), Math.Min(
                  DpiHelper.LogicalToDeviceUnits(400),
                  settings.GetValue("treeView.Columns." + column.Header + ".Width",
                  column.Width)));

            treeModel = new TreeModel();
            root = new Node(System.Environment.MachineName);
            root.Image = Utilities.EmbeddedResources.GetImage("computer.png");

            treeModel.Nodes.Add(root);
            treeView.Model = treeModel;

            this.computer = new Computer(settings);

            systemTray = new SystemTray(computer, settings, unitManager);
            systemTray.HideShowCommand += hideShowClick;
            systemTray.ExitCommand += exitClick;

            if (Hardware.OperatingSystem.IsUnix)
            { // Unix
                treeView.RowHeight = Math.Max(treeView.RowHeight,
                  DpiHelper.LogicalToDeviceUnits(18));
                splitContainer.BorderStyle = BorderStyle.None;
                splitContainer.Border3DStyle = Border3DStyle.Adjust;
                splitContainer.SplitterWidth = 4;
                treeView.BorderStyle = BorderStyle.Fixed3D;
                plotPanel.BorderStyle = BorderStyle.Fixed3D;
                gadgetMenuItem.Visible = false;
                minCloseMenuItem.Visible = false;
                minTrayMenuItem.Visible = false;
                startMinMenuItem.Visible = false;
            }
            else
            { // Windows
                treeView.RowHeight = Math.Max(treeView.Font.Height +
                  DpiHelper.LogicalToDeviceUnits(1),
                  DpiHelper.LogicalToDeviceUnits(18));

                gadget = new SensorGadget(computer, settings, unitManager);
                gadget.HideShowCommand += hideShowClick;

            }

            m_valueLogger = new Logger(computer);

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

            computer.HardwareAdded += new HardwareEventHandler(HardwareAdded);
            computer.HardwareRemoved += new HardwareEventHandler(HardwareRemoved);

            computer.Open();

            Microsoft.Win32.SystemEvents.PowerModeChanged += PowerModeChanged;

            timer.Enabled = true;

            showHiddenSensors = new UserOption("hiddenMenuItem", false,
              hiddenMenuItem, settings);
            showHiddenSensors.Changed += delegate (object sender, EventArgs e)
            {
                treeModel.ForceVisible = showHiddenSensors.Value;
            };

            showValue = new UserOption("valueMenuItem", true, valueMenuItem,
              settings);
            showValue.Changed += delegate (object sender, EventArgs e)
            {
                treeView.Columns[1].IsVisible = showValue.Value;
            };

            showMin = new UserOption("minMenuItem", false, minMenuItem, settings);
            showMin.Changed += delegate (object sender, EventArgs e)
            {
                treeView.Columns[2].IsVisible = showMin.Value;
            };

            showMax = new UserOption("maxMenuItem", true, maxMenuItem, settings);
            showMax.Changed += delegate (object sender, EventArgs e)
            {
                treeView.Columns[3].IsVisible = showMax.Value;
            };

            startMinimized = new UserOption("startMinMenuItem", false,
              startMinMenuItem, settings, () => Program.Arguments.StartMinimized ? true : null);

            minimizeToTray = new UserOption("minTrayMenuItem", true,
              minTrayMenuItem, settings, () => Program.Arguments.MinimizeToTray ? true : null);

            // Force a refresh of the icon state
            systemTray.IsMainIconEnabled = minimizeToTray.Value;

            minimizeToTray.Changed += delegate (object sender, EventArgs e)
            {
                systemTray.IsMainIconEnabled = minimizeToTray.Value;
            };

            minimizeOnClose = new UserOption("minCloseMenuItem", false,
              minCloseMenuItem, settings);

            autoStart = new UserOption(null, startupManager.IsAutoStartupEnabled && !startupManager.IsStartupAsService,
              startupMenuItem, settings, () =>
              {
                  if (Program.Arguments.AutoStartupMode == AutoStartupMode.NoChange)
                  {
                      return null;
                  }

                  if (Program.Arguments.AutoStartupMode == AutoStartupMode.Logon)
                  {
                      return true;
                  }

                  return false;
              });

            autoStartAsService = new UserOption(null, startupManager.IsStartupAsService, runAsServiceMenuItem, settings, () =>
            {
                if (Program.Arguments.AutoStartupMode == AutoStartupMode.NoChange)
                {
                    return null;
                }

                if (Program.Arguments.AutoStartupMode == AutoStartupMode.Bootup)
                {
                    return true;
                }

                return false;
            });

            autoStart.Changed += delegate (object sender, EventArgs e)
            {
                try
                {
                    if (autoStartAsService.Value == false)
                    {
                        startupManager.ConfigureAutoStartup(autoStart.Value, false);
                    }
                    else if (autoStart.Value)
                    {
                        // Only either of them can be enabled
                        autoStart.Value = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Updating the auto-startup option failed.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    autoStart.Value = startupManager.IsAutoStartupEnabled;
                }
            };

            autoStartAsService.Changed += (sender, e) =>
            {
                try
                {
                    if (autoStart.Value == false)
                    {
                        startupManager.ConfigureAutoStartup(autoStartAsService.Value, true);
                    }
                    else if (autoStartAsService.Value)
                    {
                        autoStartAsService.Value = false;
                    }

                }
                catch (Exception x) when (x is InvalidOperationException || x is UnauthorizedAccessException || x is IOException)
                {
                    MessageBox.Show("Updating the auto-startup option failed: " + x.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    autoStart.Value = startupManager.IsAutoStartupEnabled;
                }
            };

            readMainboardSensors = new UserOption("mainboardMenuItem", true,
              mainboardMenuItem, settings, () => Program.Arguments.IgnoreMonitorMainboard ? false : null);
            readMainboardSensors.Changed += delegate (object sender, EventArgs e)
            {
                computer.MainboardEnabled = readMainboardSensors.Value;
            };

            readCpuSensors = new UserOption("cpuMenuItem", true,
              cpuMenuItem, settings, () => Program.Arguments.IgnoreMonitorCPU ? false : null);
            readCpuSensors.Changed += delegate (object sender, EventArgs e)
            {
                computer.CPUEnabled = readCpuSensors.Value;
            };

            readRamSensors = new UserOption("ramMenuItem", true,
              ramMenuItem, settings, () => Program.Arguments.IgnoreMonitorRAM ? false : null);
            readRamSensors.Changed += delegate (object sender, EventArgs e)
            {
                computer.RAMEnabled = readRamSensors.Value;
            };

            readGpuSensors = new UserOption("gpuMenuItem", true,
              gpuMenuItem, settings, () => Program.Arguments.IgnoreMonitorGPU ? false : null);
            readGpuSensors.Changed += delegate (object sender, EventArgs e)
            {
                computer.GPUEnabled = readGpuSensors.Value;
            };

            readFanControllersSensors = new UserOption("fanControllerMenuItem", true,
              fanControllerMenuItem, settings, () => Program.Arguments.IgnoreMonitorFanController ? false : null);
            readFanControllersSensors.Changed += delegate (object sender, EventArgs e)
            {
                computer.FanControllerEnabled = readFanControllersSensors.Value;
            };

            readHddSensorsRemovable = new UserOption("hddMenuItemRemovable", true, hddMenuItemRemovable, settings,
              () => Program.Arguments.IgnoreRemovableDrives ? false : null);
            readHddSensors = new UserOption("hddMenuItem", true, hddMenuItem,
              settings, () => Program.Arguments.IgnoreMonitorHDD ? false : null);
            readHddSensors.Changed += delegate (object sender, EventArgs e)
            {
                computer.HDDEnabled = readHddSensors.Value;
            };
            readHddSensorsRemovable.Changed += delegate (object sender, EventArgs e)
            {
                // Refresh the Hdd settings
                computer.HDDEnabled = false;
                computer.HDDEnabled = readHddSensors.Value;
            };

            readNetworkSensors = new UserOption("networkMenuItem", true, networkMenuItem,
              settings, () => Program.Arguments.IgnoreMonitorNetwork ? false : null);
            readNetworkSensors.Changed += delegate (object sender, EventArgs e)
            {
                computer.NetworkEnabled = readNetworkSensors.Value;
            };

            showGadgetTopmost = new UserOption("showGadgetTopmost", false, showGadgetWindowTopmostMenuItem,
                settings);
            showGadgetTopmost.Changed += (sender, e) =>
            {
                if (gadget != null)
                {
                    gadget.AlwaysOnTop = showGadgetTopmost.Value;
                }
            };

            showGadget = new UserOption("gadgetMenuItem", false, gadgetMenuItem,
              settings);
            showGadget.Changed += delegate (object sender, EventArgs e)
            {
                if (gadget != null)
                {
                    gadget.Visible = showGadget.Value;
                    gadget.AlwaysOnTop = showGadgetTopmost.Value;
                }
            };

            celsiusMenuItem.Checked =
              unitManager.TemperatureUnit == TemperatureUnit.Celsius;
            fahrenheitMenuItem.Checked = !celsiusMenuItem.Checked;

            HttpServerPort = this.settings.GetValue("listenerPort", 8086);
            if (Program.Arguments.WebServerPort.HasValue)
            {
                HttpServerPort = Program.Arguments.WebServerPort.Value;
            }

            allowWebServerRemoteAccess = new UserOption("allowRemoteAccessMenuItem", false, allowRemoteAccessToolStripMenuItem,
                settings, () => Program.Arguments.AllowRemoteAccess ? true : null);

            allowWebServerRemoteAccess.Changed += (sender, e) =>
            {
                if (server != null)
                {
                    server.AllowRemoteAccess = allowWebServerRemoteAccess.Value;
                }
            };

            server = new GrapevineServer(root, computer, HttpServerPort, allowWebServerRemoteAccess.Value);
            runWebServer = new UserOption("runWebServerMenuItem", false,
              runWebServerMenuItem, settings, () => Program.Arguments.RunWebServer ? true : null);
            runWebServer.Changed += delegate (object sender, EventArgs e)
            {
                if (runWebServer.Value)
                {
                    server.Stop();
                    server.Dispose();
                    server = new GrapevineServer(root, computer, HttpServerPort, allowWebServerRemoteAccess.Value);
                    server.Start();
                }
                else
                    server.Stop();
            };

            logSensors = new UserOption("logSensorsMenuItem", false, logSensorsMenuItem,
              settings);

            loggingInterval = new UserRadioGroup("loggingInterval", 0,
              new[] { log1sMenuItem, log2sMenuItem, log5sMenuItem, log10sMenuItem,
        log30sMenuItem, log1minMenuItem, log2minMenuItem, log5minMenuItem,
        log10minMenuItem, log30minMenuItem, log1hMenuItem, log2hMenuItem,
        log6hMenuItem},
              settings);
            loggingInterval.Changed += (sender, e) =>
            {
                switch (loggingInterval.Value)
                {
                    case 0: m_valueLogger.LoggingInterval = new TimeSpan(0, 0, 1); break;
                    case 1: m_valueLogger.LoggingInterval = new TimeSpan(0, 0, 2); break;
                    case 2: m_valueLogger.LoggingInterval = new TimeSpan(0, 0, 5); break;
                    case 3: m_valueLogger.LoggingInterval = new TimeSpan(0, 0, 10); break;
                    case 4: m_valueLogger.LoggingInterval = new TimeSpan(0, 0, 30); break;
                    case 5: m_valueLogger.LoggingInterval = new TimeSpan(0, 1, 0); break;
                    case 6: m_valueLogger.LoggingInterval = new TimeSpan(0, 2, 0); break;
                    case 7: m_valueLogger.LoggingInterval = new TimeSpan(0, 5, 0); break;
                    case 8: m_valueLogger.LoggingInterval = new TimeSpan(0, 10, 0); break;
                    case 9: m_valueLogger.LoggingInterval = new TimeSpan(0, 30, 0); break;
                    case 10: m_valueLogger.LoggingInterval = new TimeSpan(1, 0, 0); break;
                    case 11: m_valueLogger.LoggingInterval = new TimeSpan(2, 0, 0); break;
                    case 12: m_valueLogger.LoggingInterval = new TimeSpan(6, 0, 0); break;
                }
            };

            InitializePlotForm();

            startupMenuItem.Visible = startupManager.IsAvailable;

            if (startMinMenuItem.Checked && !Program.Arguments.StartNormal)
            {
                if (!minTrayMenuItem.Checked)
                {
                    WindowState = FormWindowState.Minimized;
                    Show();
                }
            }
            else
            {
                Show();
            }

            // Create a handle, otherwise calling Close() does not fire FormClosed     
            IntPtr handle = Handle;

            // Make sure the settings are saved when the user logs off
            Microsoft.Win32.SystemEvents.SessionEnded += delegate
            {
                computer.Close();
                SaveConfiguration();
                if (runWebServer.Value)
                    server.Stop();
            };

            treeView.ExpandAll();
            secondInstanceService.Run();
            secondInstanceService.OnSecondInstanceRequest += HandleSecondInstanceRequest;
        }

        private void HandleSecondInstanceRequest(SecondInstanceService.SecondInstanceRequest requestType)
        {
            base.BeginInvoke(() =>
            {
                switch (requestType)
                {
                    case SecondInstanceService.SecondInstanceRequest.MaximizeWindow:
                        {
                            Logging.LogInfo($"Received MaximizeWindow request. Visible = {Visible}");
                            if (Visible == false)
                            {
                                SysTrayHideShow();
                            }
                        }
                        break;
                }
            });
        }

        public int HttpServerPort
        {
            get;
            set;
        } = 8086;

        private void PowerModeChanged(object sender,
      Microsoft.Win32.PowerModeChangedEventArgs e)
        {

            if (e.Mode == Microsoft.Win32.PowerModes.Resume)
            {
                computer.Reset();
            }
        }

        private void InitializePlotForm()
        {
            plotForm = new Form();
            plotForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            plotForm.ShowInTaskbar = false;
            plotForm.StartPosition = FormStartPosition.Manual;
            this.AddOwnedForm(plotForm);
            plotForm.Bounds = new Rectangle
            {
                X = settings.GetValue("plotForm.Location.X", -100000),
                Y = settings.GetValue("plotForm.Location.Y", 100),
                Width = settings.GetValue("plotForm.Width", 600),
                Height = settings.GetValue("plotForm.Height", 400)
            };

            showPlot = new UserOption("plotMenuItem", false, plotMenuItem, settings);
            plotLocation = new UserRadioGroup("plotLocation", 0,
              new[] { plotWindowMenuItem, plotBottomMenuItem, plotRightMenuItem },
              settings);

            showPlot.Changed += delegate (object sender, EventArgs e)
            {
                if (plotLocation.Value == 0)
                {
                    if (showPlot.Value && this.Visible)
                        plotForm.Show();
                    else
                        plotForm.Hide();
                }
                else
                {
                    splitContainer.Panel2Collapsed = !showPlot.Value;
                }
                treeView.Invalidate();
            };
            plotLocation.Changed += delegate (object sender, EventArgs e)
            {
                switch (plotLocation.Value)
                {
                    case 0:
                        splitContainer.Panel2.Controls.Clear();
                        splitContainer.Panel2Collapsed = true;
                        plotForm.Controls.Add(plotPanel);
                        if (showPlot.Value && this.Visible)
                            plotForm.Show();
                        break;
                    case 1:
                        plotForm.Controls.Clear();
                        plotForm.Hide();
                        splitContainer.Orientation = Orientation.Horizontal;
                        splitContainer.Panel2.Controls.Add(plotPanel);
                        splitContainer.Panel2Collapsed = !showPlot.Value;
                        break;
                    case 2:
                        plotForm.Controls.Clear();
                        plotForm.Hide();
                        splitContainer.Orientation = Orientation.Vertical;
                        splitContainer.Panel2.Controls.Add(plotPanel);
                        splitContainer.Panel2Collapsed = !showPlot.Value;
                        break;
                }
            };

            plotForm.FormClosing += delegate (object sender, FormClosingEventArgs e)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    // just switch off the plotting when the user closes the form
                    if (plotLocation.Value == 0)
                    {
                        showPlot.Value = false;
                    }
                    e.Cancel = true;
                }
            };

            EventHandler moveOrResizePlotForm = delegate (object sender, EventArgs e)
            {
                if (plotForm.WindowState != FormWindowState.Minimized)
                {
                    settings.SetValue("plotForm.Location.X", plotForm.Bounds.X);
                    settings.SetValue("plotForm.Location.Y", plotForm.Bounds.Y);
                    settings.SetValue("plotForm.Width", plotForm.Bounds.Width);
                    settings.SetValue("plotForm.Height", plotForm.Bounds.Height);
                }
            };
            plotForm.Move += moveOrResizePlotForm;
            plotForm.Resize += moveOrResizePlotForm;

            plotForm.VisibleChanged += delegate (object sender, EventArgs e)
            {
                Rectangle bounds = new Rectangle(plotForm.Location, plotForm.Size);
                Screen screen = Screen.FromRectangle(bounds);
                Rectangle intersection =
                  Rectangle.Intersect(screen.WorkingArea, bounds);
                if (intersection.Width < Math.Min(16, bounds.Width) ||
                    intersection.Height < Math.Min(16, bounds.Height))
                {
                    plotForm.Location = new Point(
                      screen.WorkingArea.Width / 2 - bounds.Width / 2,
                      screen.WorkingArea.Height / 2 - bounds.Height / 2);
                }
            };

            this.VisibleChanged += delegate (object sender, EventArgs e)
            {
                if (this.Visible && showPlot.Value && plotLocation.Value == 0)
                    plotForm.Show();
                else
                    plotForm.Hide();
            };
        }

        private void InsertSorted(Collection<Node> nodes, HardwareNode node)
        {
            int i = 0;
            while (i < nodes.Count && nodes[i] is HardwareNode &&
              ((HardwareNode)nodes[i]).Hardware.HardwareType <=
                node.Hardware.HardwareType)
                i++;
            nodes.Insert(i, node);
        }

        private void SubHardwareAdded(IHardware hardware, Node node)
        {
            HardwareNode hardwareNode =
              new HardwareNode(hardware, settings, unitManager);
            hardwareNode.PlotSelectionChanged += PlotSelectionChanged;

            InsertSorted(node.Nodes, hardwareNode);

            foreach (IHardware subHardware in hardware.SubHardware)
                SubHardwareAdded(subHardware, hardwareNode);
        }

        private void HardwareAdded(IHardware hardware)
        {
            SubHardwareAdded(hardware, root);
            PlotSelectionChanged(this, null);
        }

        private void HardwareRemoved(IHardware hardware)
        {
            List<HardwareNode> nodesToRemove = new List<HardwareNode>();
            foreach (Node node in root.Nodes)
            {
                HardwareNode hardwareNode = node as HardwareNode;
                if (hardwareNode != null && hardwareNode.Hardware == hardware)
                    nodesToRemove.Add(hardwareNode);
            }
            foreach (HardwareNode hardwareNode in nodesToRemove)
            {
                root.Nodes.Remove(hardwareNode);
                hardwareNode.PlotSelectionChanged -= PlotSelectionChanged;
            }
            PlotSelectionChanged(this, null);
        }

        private void nodeTextBoxText_DrawText(object sender, DrawTextEventArgs e)
        {
            Node node = e.Node.Tag as Node;
            if (node != null)
            {
                Color color;
                if (node.IsVisible)
                {
                    SensorNode sensorNode = node as SensorNode;
                    if (plotMenuItem.Checked && sensorNode != null &&
                        sensorPlotColors.TryGetValue(sensorNode.Sensor, out color))
                    {
                        e.TextColor = color;
                    }

                }
                else
                {
                    e.TextColor = Color.DarkGray;
                }
            }
        }

        private void PlotSelectionChanged(object sender, EventArgs e)
        {
            List<ISensor> selected = new List<ISensor>();
            IDictionary<ISensor, Color> colors = new Dictionary<ISensor, Color>();
            int colorIndex = 0;
            foreach (TreeNodeAdv node in treeView.AllNodes)
            {
                SensorNode sensorNode = node.Tag as SensorNode;
                if (sensorNode != null)
                {
                    if (sensorNode.Plot)
                    {
                        if (!sensorNode.PenColor.HasValue)
                        {
                            colors.Add(sensorNode.Sensor,
                              plotColorPalette[colorIndex % plotColorPalette.Length]);
                        }
                        selected.Add(sensorNode.Sensor);
                    }
                    colorIndex++;
                }
            }

            // if a sensor is assigned a color that's already being used by another 
            // sensor, try to assign it a new color. This is done only after the 
            // previous loop sets an unchanging default color for all sensors, so that 
            // colors jump around as little as possible as sensors get added/removed 
            // from the plot
            var usedColors = new List<Color>();
            foreach (var curSelectedSensor in selected)
            {
                if (!colors.ContainsKey(curSelectedSensor)) continue;
                var curColor = colors[curSelectedSensor];
                if (usedColors.Contains(curColor))
                {
                    foreach (var potentialNewColor in plotColorPalette)
                    {
                        if (!colors.Values.Contains(potentialNewColor))
                        {
                            colors[curSelectedSensor] = potentialNewColor;
                            usedColors.Add(potentialNewColor);
                            break;
                        }
                    }
                }
                else
                {
                    usedColors.Add(curColor);
                }
            }

            foreach (TreeNodeAdv node in treeView.AllNodes)
            {
                SensorNode sensorNode = node.Tag as SensorNode;
                if (sensorNode != null && sensorNode.Plot && sensorNode.PenColor.HasValue)
                    colors.Add(sensorNode.Sensor, sensorNode.PenColor.Value);
            }

            sensorPlotColors = colors;
            plotPanel.SetSensors(selected, colors);
        }

        private void nodeTextBoxText_EditorShowing(object sender,
          CancelEventArgs e)
        {
            e.Cancel = !(treeView.CurrentNode != null &&
              (treeView.CurrentNode.Tag is SensorNode ||
               treeView.CurrentNode.Tag is HardwareNode));
        }

        private void nodeCheckBox_IsVisibleValueNeeded(object sender,
          NodeControlValueEventArgs e)
        {
            SensorNode node = e.Node.Tag as SensorNode;
            e.Value = (node != null) && plotMenuItem.Checked;
        }

        private void exitClick(object sender, EventArgs e)
        {
            Close();
        }

        private int delayCount = 0;

        private void timer_Tick(object sender, EventArgs e)
        {
            computer.Accept(updateVisitor);
            treeView.Invalidate();
            plotPanel.InvalidatePlot();
            systemTray.Redraw();
            if (gadget != null)
                gadget.Redraw();


            if (logSensors != null && logSensors.Value && delayCount >= 4)
                m_valueLogger.Log();

            if (delayCount < 4)
                delayCount++;
        }

        private void SaveConfiguration()
        {
            if (settings == null)
                return;

            if (plotPanel != null)
            {
                plotPanel.SetCurrentSettings();
                foreach (TreeColumn column in treeView.Columns)
                    settings.SetValue("treeView.Columns." + column.Header + ".Width",
                      column.Width);
            }

            if (server != null)
            {
                this.settings.SetValue("listenerPort", server.ListenerPort);
            }

            string fileName = Path.ChangeExtension(
                System.Windows.Forms.Application.ExecutablePath, ".config");
            try
            {
                settings.Save(fileName);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access to the path '" + fileName + "' is denied. " +
                  "The current settings could not be saved.",
                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException)
            {
                MessageBox.Show("The path '" + fileName + "' is not writeable. " +
                  "The current settings could not be saved.",
                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Rectangle newBounds = new Rectangle
            {
                X = settings.GetValue("mainForm.Location.X", Location.X),
                Y = settings.GetValue("mainForm.Location.Y", Location.Y),
                Width = settings.GetValue("mainForm.Width",
                DpiHelper.LogicalToDeviceUnits(470)),
                Height = settings.GetValue("mainForm.Height",
                DpiHelper.LogicalToDeviceUnits(640))
            };

            Rectangle fullWorkingArea = new Rectangle(int.MaxValue, int.MaxValue,
              int.MinValue, int.MinValue);

            foreach (Screen screen in Screen.AllScreens)
                fullWorkingArea = Rectangle.Union(fullWorkingArea, screen.Bounds);

            Rectangle intersection = Rectangle.Intersect(fullWorkingArea, newBounds);
            if (intersection.Width < 20 || intersection.Height < 20 ||
              !settings.Contains("mainForm.Location.X")
            )
            {
                newBounds.X = (Screen.PrimaryScreen.WorkingArea.Width / 2) -
                              (newBounds.Width / 2);

                newBounds.Y = (Screen.PrimaryScreen.WorkingArea.Height / 2) -
                              (newBounds.Height / 2);
            }

            this.Bounds = newBounds;

            // Register for receiving hardware change information (trough WndProc)
            DeviceNotification.RegisterDeviceNotification(Handle);

            // This setting needs to be applied after the form load since it otherwise triggers
            // a relayout.
            if (this.settings.Contains("splitContainer.SplitterDistance"))
            {
                this.splitContainer.SplitterDistance = this.settings.GetValue("splitContainer.SplitterDistance", 0);
            }

            // This event needs to be attached after the form load since it otherwise
            // triggers a relayout.
            this.splitContainer.SplitterMoved += delegate (object splitterSender,
                    SplitterEventArgs splitterEvent)
            {
                if (this.settings.GetValue("plotLocation", 0) == 1)
                {
                    this.settings.SetValue("splitContainer.SplitterDistance", splitterEvent.SplitY);
                }
                else if (this.settings.GetValue("plotLocation", 0) == 2)
                {
                    this.settings.SetValue("splitContainer.SplitterDistance", splitterEvent.SplitX);
                }
            };

            if (Program.Arguments.CloseAll)
            {
                BeginInvoke(ForceClose);
            }
        }

        internal void ForceClose()
        {
            if (!Visible)
            {
                Activate();
            }
            CloseAllProcesses();
            Close();
        }

        private void CloseAllProcesses()
        {
            var currentProcess = Process.GetCurrentProcess();
            int currentPid = currentProcess.Id;
            var processes = Process.GetProcesses().Where(x => x.ProcessName == currentProcess.ProcessName && x.Id != currentPid);
            foreach (var process in processes)
            {
                try
                {
                    process.CloseMainWindow();
                    // Wait until main window closure took effect
                    if (!process.WaitForExit(5000))
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Not much we can do here (but normally, our process would run as admin, so this shouldn't happen)
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Visible = false;
            systemTray.IsMainIconEnabled = false;
            timer.Enabled = false;
            computer.Close();
            SaveConfiguration();
            if (runWebServer.Value)
                server.Stop();
            systemTray.Dispose();
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        private void treeView_Click(object sender, EventArgs e)
        {

            MouseEventArgs m = e as MouseEventArgs;
            if (m == null || m.Button != MouseButtons.Right)
                return;

            NodeControlInfo info = treeView.GetNodeControlInfoAt(
              new Point(m.X, m.Y)
            );
            treeView.SelectedNode = info.Node;
            if (info.Node != null)
            {
                SensorNode node = info.Node.Tag as SensorNode;
                if (node != null && node.Sensor != null)
                {
                    treeContextMenu.Items.Clear();
                    if (node.Sensor.Parameters.Length > 0)
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem("Parameters...");
                        item.Click += delegate (object obj, EventArgs args)
                        {
                            ShowParameterForm(node.Sensor);
                        };
                        treeContextMenu.Items.Add(item);
                    }
                    if (nodeTextBoxText.EditEnabled)
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem("Rename");
                        item.Click += delegate (object obj, EventArgs args)
                        {
                            nodeTextBoxText.BeginEdit();
                        };
                        treeContextMenu.Items.Add(item);
                    }
                    if (node.IsVisible)
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem("Hide");
                        item.Click += delegate (object obj, EventArgs args)
                        {
                            node.IsVisible = false;
                        };
                        treeContextMenu.Items.Add(item);
                    }
                    else
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem("Unhide");
                        item.Click += delegate (object obj, EventArgs args)
                        {
                            node.IsVisible = true;
                        };
                        treeContextMenu.Items.Add(item);
                    }
                    treeContextMenu.Items.Add(new ToolStripSeparator());
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem("Pen Color...");
                        item.Click += delegate (object obj, EventArgs args)
                        {
                            ColorDialog dialog = new ColorDialog();
                            dialog.Color = node.PenColor.GetValueOrDefault();
                            if (dialog.ShowDialog() == DialogResult.OK)
                                node.PenColor = dialog.Color;
                        };
                        treeContextMenu.Items.Add(item);
                    }
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem("Reset Pen Color");
                        item.Click += delegate (object obj, EventArgs args)
                        {
                            node.PenColor = null;
                        };
                        treeContextMenu.Items.Add(item);
                    }
                    treeContextMenu.Items.Add(new ToolStripSeparator());
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem("Show in Tray");
                        item.Checked = systemTray.Contains(node.Sensor);
                        item.Click += delegate (object obj, EventArgs args)
                        {
                            if (item.Checked)
                                systemTray.Remove(node.Sensor);
                            else
                                systemTray.Add(node.Sensor, true);
                        };
                        treeContextMenu.Items.Add(item);
                    }
                    if (gadget != null)
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem("Show in Gadget");
                        item.Checked = gadget.Contains(node.Sensor);
                        item.Click += delegate (object obj, EventArgs args)
                        {
                            if (item.Checked)
                            {
                                gadget.Remove(node.Sensor);
                            }
                            else
                            {
                                gadget.Add(node.Sensor);
                            }
                        };
                        treeContextMenu.Items.Add(item);
                    }
                    if (node.Sensor.Control != null)
                    {
                        treeContextMenu.Items.Add(new ToolStripSeparator());
                        IControl control = node.Sensor.Control;
                        ToolStripMenuItem controlItem = new ToolStripMenuItem("Control");
                        ToolStripMenuItem defaultItem = new ToolStripMenuItem("Default");
                        defaultItem.Checked = control.ControlMode == ControlMode.Default;
                        controlItem.DropDownItems.Add(defaultItem);
                        defaultItem.Click += delegate (object obj, EventArgs args)
                        {
                            control.SetDefault();
                        };
                        ToolStripMenuItem manualItem = new ToolStripMenuItem("Manual");
                        controlItem.DropDownItems.Add(manualItem);
                        manualItem.Checked = control.ControlMode == ControlMode.Software;
                        for (int i = 0; i <= 100; i += 5)
                        {
                            if (i <= control.MaxSoftwareValue &&
                                i >= control.MinSoftwareValue)
                            {
                                ToolStripMenuItem item = new ToolStripMenuItem(i + " %");
                                manualItem.DropDownItems.Add(item);
                                item.Checked = control.ControlMode == ControlMode.Software &&
                                  Math.Round(control.SoftwareValue) == i;
                                int softwareValue = i;
                                item.Click += delegate (object obj, EventArgs args)
                                {
                                    control.SetSoftware(softwareValue);
                                };
                            }
                        }
                        treeContextMenu.Items.Add(controlItem);
                    }

                    treeContextMenu.Show(treeView, new Point(m.X, m.Y));
                }

                HardwareNode hardwareNode = info.Node.Tag as HardwareNode;
                if (hardwareNode != null && hardwareNode.Hardware != null)
                {
                    treeContextMenu.Items.Clear();

                    if (nodeTextBoxText.EditEnabled)
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem("Rename");
                        item.Click += delegate (object obj, EventArgs args)
                        {
                            nodeTextBoxText.BeginEdit();
                        };
                        treeContextMenu.Items.Add(item);
                    }

                    treeContextMenu.Show(treeView, new Point(m.X, m.Y));
                }
            }
        }

        private void saveReportMenuItem_Click(object sender, EventArgs e)
        {
            string report = computer.GetReport();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (TextWriter w = new StreamWriter(saveFileDialog.FileName))
                {
                    w.Write(report);
                }
            }
        }

        private void SysTrayHideShow()
        {
            Visible = !Visible;
            if (Visible)
                Activate();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x112;
            const int SC_MINIMIZE = 0xF020;
            const int SC_CLOSE = 0xF060;
            if (m.Msg == DeviceNotification.WmDevicechange)
            {
                switch ((int)m.WParam)
                {
                    case DeviceNotification.DbtDeviceArrival:
                        BeginInvoke(new Action<bool>(DeviceChanged), true); // this is where you do your magic
                        break;
                    case DeviceNotification.DbtDeviceRemoveComplete:
                        BeginInvoke(new Action<bool>(DeviceChanged), false); // this is where you do your magic
                        break;
                }
            }
            if (minimizeToTray.Value &&
              m.Msg == WM_SYSCOMMAND && m.WParam.ToInt64() == SC_MINIMIZE)
            {
                SysTrayHideShow();
            }
            else if (minimizeOnClose.Value &&
            m.Msg == WM_SYSCOMMAND && m.WParam.ToInt64() == SC_CLOSE)
            {
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
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void DeviceChanged(bool added)
        {
            // Swapping these will re-query the list of devices
            if (computer != null)
            {
                if (computer.NetworkEnabled)
                {
                    computer.NetworkEnabled = false;
                    computer.NetworkEnabled = true;
                }

                if (computer.HDDEnabled)
                {
                    computer.HDDEnabled = false;
                    computer.HDDEnabled = true;
                }
            }
        }

        private void hideShowClick(object sender, EventArgs e)
        {
            SysTrayHideShow();
        }

        private void ShowParameterForm(ISensor sensor)
        {
            ParameterForm form = new ParameterForm();
            form.Parameters = sensor.Parameters;
            form.captionLabel.Text = sensor.Name;
            form.ShowDialog();
        }

        private void treeView_NodeMouseDoubleClick(object sender,
          TreeNodeAdvMouseEventArgs e)
        {
            SensorNode node = e.Node.Tag as SensorNode;
            if (node != null && node.Sensor != null &&
              node.Sensor.Parameters.Length > 0)
            {
                ShowParameterForm(node.Sensor);
            }
        }

        private void celsiusMenuItem_Click(object sender, EventArgs e)
        {
            celsiusMenuItem.Checked = true;
            fahrenheitMenuItem.Checked = false;
            unitManager.TemperatureUnit = TemperatureUnit.Celsius;
        }

        private void fahrenheitMenuItem_Click(object sender, EventArgs e)
        {
            celsiusMenuItem.Checked = false;
            fahrenheitMenuItem.Checked = true;
            unitManager.TemperatureUnit = TemperatureUnit.Fahrenheit;
        }

        private void resetMinMaxMenuItem_Click(object sender, EventArgs e)
        {
            computer.Accept(new SensorVisitor(delegate (ISensor sensor)
            {
                sensor.ResetMin();
                sensor.ResetMax();
            }));
        }

        private void MainForm_MoveOrResize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                settings.SetValue("mainForm.Location.X", Bounds.X);
                settings.SetValue("mainForm.Location.Y", Bounds.Y);
                settings.SetValue("mainForm.Width", Bounds.Width);
                settings.SetValue("mainForm.Height", Bounds.Height);
            }
        }

        private void resetClick(object sender, EventArgs e)
        {
            // disable the fallback MainIcon during reset, otherwise icon visibility
            // might be lost 
            systemTray.IsMainIconEnabled = false;
            computer.Reset();
            // restore the MainIcon setting
            systemTray.IsMainIconEnabled = minimizeToTray.Value;
        }

        private void treeView_MouseMove(object sender, MouseEventArgs e)
        {
            selectionDragging = selectionDragging &
              (e.Button & (MouseButtons.Left | MouseButtons.Right)) > 0;

            if (selectionDragging)
                treeView.SelectedNode = treeView.GetNodeAt(e.Location);
        }

        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            selectionDragging = true;
        }

        private void treeView_MouseUp(object sender, MouseEventArgs e)
        {
            selectionDragging = false;
        }

        private void serverPortMenuItem_Click(object sender, EventArgs e)
        {
            new PortForm(this).ShowDialog();
        }

        protected override void OnClosed(EventArgs e)
        {
            secondInstanceService.OnSecondInstanceRequest -= HandleSecondInstanceRequest;
            secondInstanceService.Dispose();
            base.OnClosed(e);
        }
    }
}
