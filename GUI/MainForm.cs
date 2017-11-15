/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2013 Michael Möller <mmoeller@openhardwaremonitor.org>
	Copyright (C) 2010 Paul Werelds <paul@werelds.net>
	Copyright (C) 2012 Prince Samuel <prince.samuel@gmail.com>

*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using Microsoft.Win32;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;
using OpenHardwareMonitor.WMI;

namespace OpenHardwareMonitor.GUI
{
    public partial class MainForm : Form
    {
        private readonly UserOption autoStart;
        private readonly Computer computer;
        private readonly SensorGadget gadget;
        private readonly Logger logger;
        private readonly UserRadioGroup loggingInterval;

        private readonly UserOption logSensors;
        private readonly UserOption minimizeOnClose;
        private readonly UserOption minimizeToTray;
        private readonly Color[] plotColorPalette;
        private readonly PlotPanel plotPanel;
        private readonly UserOption readCpuSensors;
        private readonly UserOption readFanControllersSensors;
        private readonly UserOption readGpuSensors;
        private readonly UserOption readHddSensors;

        private readonly UserOption readMainboardSensors;
        private readonly UserOption readRamSensors;
        private readonly Node root;

        private readonly UserOption runWebServer;

        private readonly PersistentSettings settings;

        private readonly UserOption showGadget;

        private readonly UserOption showHiddenSensors;
        private readonly UserOption showMax;
        private readonly UserOption showMin;
        private readonly UserOption showValue;
        private readonly StartupManager startupManager = new StartupManager();
        private readonly SystemTray systemTray;
        private readonly TreeModel treeModel;
        private readonly UnitManager unitManager;
        private readonly UpdateVisitor updateVisitor = new UpdateVisitor();
        private readonly WmiProvider wmiProvider;

        private int delayCount;
        private Form plotForm;
        private UserRadioGroup plotLocation;

        private bool selectionDragging;

        private IDictionary<ISensor, Color> sensorPlotColors =
            new Dictionary<ISensor, Color>();

        private UserOption showPlot;
        private UserOption startMinimized;

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

            settings = new PersistentSettings();
            settings.Load(Path.ChangeExtension(
                Application.ExecutablePath, ".config"));

            unitManager = new UnitManager(settings);

            // make sure the buffers used for double buffering are not disposed 
            // after each draw call
            BufferedGraphicsManager.Current.MaximumBuffer =
                Screen.PrimaryScreen.Bounds.Size;

            // set the DockStyle here, to avoid conflicts with the MainMenu
            splitContainer.Dock = DockStyle.Fill;

            Font = SystemFonts.MessageBoxFont;
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

            foreach (var column in treeView.Columns)
                column.Width = Math.Max(20, Math.Min(400,
                    settings.GetValue("treeView.Columns." + column.Header + ".Width",
                        column.Width)));

            treeModel = new TreeModel();
            root = new Node(Environment.MachineName);
            root.Image = EmbeddedResources.GetImage("computer.png");

            treeModel.Nodes.Add(root);
            treeView.Model = treeModel;

            computer = new Computer(settings);

            systemTray = new SystemTray(computer, settings, unitManager);
            systemTray.HideShowCommand += hideShowClick;
            systemTray.ExitCommand += exitClick;

            var p = (int) Environment.OSVersion.Platform;
            if (p == 4 || p == 128)
            {
                // Unix
                treeView.RowHeight = Math.Max(treeView.RowHeight, 18);
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
            {
                // Windows
                treeView.RowHeight = Math.Max(treeView.Font.Height + 1, 18);

                gadget = new SensorGadget(computer, settings, unitManager);
                gadget.HideShowCommand += hideShowClick;

                wmiProvider = new WmiProvider(computer);
            }

            logger = new Logger(computer);

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

            computer.HardwareAdded += HardwareAdded;
            computer.HardwareRemoved += HardwareRemoved;

            computer.Open();

            timer.Enabled = true;

            showHiddenSensors = new UserOption("hiddenMenuItem", false,
                hiddenMenuItem, settings);
            showHiddenSensors.Changed += delegate { treeModel.ForceVisible = showHiddenSensors.Value; };

            showValue = new UserOption("valueMenuItem", true, valueMenuItem,
                settings);
            showValue.Changed += delegate { treeView.Columns[1].IsVisible = showValue.Value; };

            showMin = new UserOption("minMenuItem", false, minMenuItem, settings);
            showMin.Changed += delegate { treeView.Columns[2].IsVisible = showMin.Value; };

            showMax = new UserOption("maxMenuItem", true, maxMenuItem, settings);
            showMax.Changed += delegate { treeView.Columns[3].IsVisible = showMax.Value; };

            startMinimized = new UserOption("startMinMenuItem", false,
                startMinMenuItem, settings);

            minimizeToTray = new UserOption("minTrayMenuItem", true,
                minTrayMenuItem, settings);
            minimizeToTray.Changed += delegate { systemTray.IsMainIconEnabled = minimizeToTray.Value; };

            minimizeOnClose = new UserOption("minCloseMenuItem", false,
                minCloseMenuItem, settings);

            autoStart = new UserOption(null, startupManager.Startup,
                startupMenuItem, settings);
            autoStart.Changed += delegate
            {
                try
                {
                    startupManager.Startup = autoStart.Value;
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Updating the auto-startup option failed.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    autoStart.Value = startupManager.Startup;
                }
            };

            readMainboardSensors = new UserOption("mainboardMenuItem", true,
                mainboardMenuItem, settings);
            readMainboardSensors.Changed += delegate { computer.MainboardEnabled = readMainboardSensors.Value; };

            readCpuSensors = new UserOption("cpuMenuItem", true,
                cpuMenuItem, settings);
            readCpuSensors.Changed += delegate { computer.CPUEnabled = readCpuSensors.Value; };

            readRamSensors = new UserOption("ramMenuItem", true,
                ramMenuItem, settings);
            readRamSensors.Changed += delegate { computer.RAMEnabled = readRamSensors.Value; };

            readGpuSensors = new UserOption("gpuMenuItem", true,
                gpuMenuItem, settings);
            readGpuSensors.Changed += delegate { computer.GPUEnabled = readGpuSensors.Value; };

            readFanControllersSensors = new UserOption("fanControllerMenuItem", true,
                fanControllerMenuItem, settings);
            readFanControllersSensors.Changed += delegate
            {
                computer.FanControllerEnabled = readFanControllersSensors.Value;
            };

            readHddSensors = new UserOption("hddMenuItem", true, hddMenuItem,
                settings);
            readHddSensors.Changed += delegate { computer.HDDEnabled = readHddSensors.Value; };

            showGadget = new UserOption("gadgetMenuItem", false, gadgetMenuItem,
                settings);
            showGadget.Changed += delegate
            {
                if (gadget != null)
                    gadget.Visible = showGadget.Value;
            };

            celsiusMenuItem.Checked =
                unitManager.TemperatureUnit == TemperatureUnit.Celsius;
            fahrenheitMenuItem.Checked = !celsiusMenuItem.Checked;

            Server = new HttpServer(root, settings.GetValue("listenerPort", 8085));
            if (Server.PlatformNotSupported)
            {
                webMenuItemSeparator.Visible = false;
                webMenuItem.Visible = false;
            }

            runWebServer = new UserOption("runWebServerMenuItem", false,
                runWebServerMenuItem, settings);
            runWebServer.Changed += delegate
            {
                if (runWebServer.Value)
                    Server.StartHTTPListener();
                else
                    Server.StopHTTPListener();
            };

            logSensors = new UserOption("logSensorsMenuItem", false, logSensorsMenuItem,
                settings);

            loggingInterval = new UserRadioGroup("loggingInterval", 0,
                new[]
                {
                    log1sMenuItem, log2sMenuItem, log5sMenuItem, log10sMenuItem,
                    log30sMenuItem, log1minMenuItem, log2minMenuItem, log5minMenuItem,
                    log10minMenuItem, log30minMenuItem, log1hMenuItem, log2hMenuItem,
                    log6hMenuItem
                },
                settings);
            loggingInterval.Changed += (sender, e) =>
            {
                switch (loggingInterval.Value)
                {
                    case 0:
                        logger.LoggingInterval = new TimeSpan(0, 0, 1);
                        break;
                    case 1:
                        logger.LoggingInterval = new TimeSpan(0, 0, 2);
                        break;
                    case 2:
                        logger.LoggingInterval = new TimeSpan(0, 0, 5);
                        break;
                    case 3:
                        logger.LoggingInterval = new TimeSpan(0, 0, 10);
                        break;
                    case 4:
                        logger.LoggingInterval = new TimeSpan(0, 0, 30);
                        break;
                    case 5:
                        logger.LoggingInterval = new TimeSpan(0, 1, 0);
                        break;
                    case 6:
                        logger.LoggingInterval = new TimeSpan(0, 2, 0);
                        break;
                    case 7:
                        logger.LoggingInterval = new TimeSpan(0, 5, 0);
                        break;
                    case 8:
                        logger.LoggingInterval = new TimeSpan(0, 10, 0);
                        break;
                    case 9:
                        logger.LoggingInterval = new TimeSpan(0, 30, 0);
                        break;
                    case 10:
                        logger.LoggingInterval = new TimeSpan(1, 0, 0);
                        break;
                    case 11:
                        logger.LoggingInterval = new TimeSpan(2, 0, 0);
                        break;
                    case 12:
                        logger.LoggingInterval = new TimeSpan(6, 0, 0);
                        break;
                }
            };

            InitializePlotForm();

            startupMenuItem.Visible = startupManager.IsAvailable;

            if (startMinMenuItem.Checked)
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
            var handle = Handle;

            // Make sure the settings are saved when the user logs off
            SystemEvents.SessionEnded += delegate
            {
                computer.Close();
                SaveConfiguration();
                if (runWebServer.Value)
                    Server.Quit();
            };
        }

        public HttpServer Server { get; }

        private void InitializePlotForm()
        {
            plotForm = new Form();
            plotForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            plotForm.ShowInTaskbar = false;
            plotForm.StartPosition = FormStartPosition.Manual;
            AddOwnedForm(plotForm);
            plotForm.Bounds = new Rectangle
            {
                X = settings.GetValue("plotForm.Location.X", -100000),
                Y = settings.GetValue("plotForm.Location.Y", 100),
                Width = settings.GetValue("plotForm.Width", 600),
                Height = settings.GetValue("plotForm.Height", 400)
            };

            showPlot = new UserOption("plotMenuItem", false, plotMenuItem, settings);
            plotLocation = new UserRadioGroup("plotLocation", 0,
                new[] {plotWindowMenuItem, plotBottomMenuItem, plotRightMenuItem},
                settings);

            showPlot.Changed += delegate
            {
                if (plotLocation.Value == 0)
                    if (showPlot.Value && Visible)
                        plotForm.Show();
                    else
                        plotForm.Hide();
                else splitContainer.Panel2Collapsed = !showPlot.Value;
                treeView.Invalidate();
            };
            plotLocation.Changed += delegate
            {
                switch (plotLocation.Value)
                {
                    case 0:
                        splitContainer.Panel2.Controls.Clear();
                        splitContainer.Panel2Collapsed = true;
                        plotForm.Controls.Add(plotPanel);
                        if (showPlot.Value && Visible)
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

            plotForm.FormClosing += delegate(object sender, FormClosingEventArgs e)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    // just switch off the plotting when the user closes the form
                    if (plotLocation.Value == 0) showPlot.Value = false;
                    e.Cancel = true;
                }
            };

            EventHandler moveOrResizePlotForm = delegate
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

            plotForm.VisibleChanged += delegate
            {
                var bounds = new Rectangle(plotForm.Location, plotForm.Size);
                var screen = Screen.FromRectangle(bounds);
                var intersection =
                    Rectangle.Intersect(screen.WorkingArea, bounds);
                if (intersection.Width < Math.Min(16, bounds.Width) ||
                    intersection.Height < Math.Min(16, bounds.Height))
                    plotForm.Location = new Point(
                        screen.WorkingArea.Width / 2 - bounds.Width / 2,
                        screen.WorkingArea.Height / 2 - bounds.Height / 2);
            };

            VisibleChanged += delegate
            {
                if (Visible && showPlot.Value && plotLocation.Value == 0)
                    plotForm.Show();
                else
                    plotForm.Hide();
            };
        }

        private void InsertSorted(Collection<Node> nodes, HardwareNode node)
        {
            var i = 0;
            while (i < nodes.Count && nodes[i] is HardwareNode &&
                   ((HardwareNode) nodes[i]).Hardware.HardwareType <
                   node.Hardware.HardwareType)
                i++;
            nodes.Insert(i, node);
        }

        private void SubHardwareAdded(IHardware hardware, Node node)
        {
            var hardwareNode =
                new HardwareNode(hardware, settings, unitManager);
            hardwareNode.PlotSelectionChanged += PlotSelectionChanged;

            InsertSorted(node.Nodes, hardwareNode);

            foreach (var subHardware in hardware.SubHardware)
                SubHardwareAdded(subHardware, hardwareNode);
        }

        private void HardwareAdded(IHardware hardware)
        {
            SubHardwareAdded(hardware, root);
            PlotSelectionChanged(this, null);
        }

        private void HardwareRemoved(IHardware hardware)
        {
            var nodesToRemove = new List<HardwareNode>();
            foreach (var node in root.Nodes)
            {
                var hardwareNode = node as HardwareNode;
                if (hardwareNode != null && hardwareNode.Hardware == hardware)
                    nodesToRemove.Add(hardwareNode);
            }
            foreach (var hardwareNode in nodesToRemove)
            {
                root.Nodes.Remove(hardwareNode);
                hardwareNode.PlotSelectionChanged -= PlotSelectionChanged;
            }
            PlotSelectionChanged(this, null);
        }

        private void nodeTextBoxText_DrawText(object sender, DrawEventArgs e)
        {
            var node = e.Node.Tag as Node;
            if (node != null)
            {
                Color color;
                if (node.IsVisible)
                {
                    var sensorNode = node as SensorNode;
                    if (plotMenuItem.Checked && sensorNode != null &&
                        sensorPlotColors.TryGetValue(sensorNode.Sensor, out color))
                        e.TextColor = color;
                }
                else
                {
                    e.TextColor = Color.DarkGray;
                }
            }
        }

        private void PlotSelectionChanged(object sender, EventArgs e)
        {
            var selected = new List<ISensor>();
            IDictionary<ISensor, Color> colors = new Dictionary<ISensor, Color>();
            var colorIndex = 0;
            foreach (var node in treeView.AllNodes)
            {
                var sensorNode = node.Tag as SensorNode;
                if (sensorNode != null)
                {
                    if (sensorNode.Plot)
                    {
                        if (!sensorNode.PenColor.HasValue)
                            colors.Add(sensorNode.Sensor,
                                plotColorPalette[colorIndex % plotColorPalette.Length]);
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
                    foreach (var potentialNewColor in plotColorPalette)
                        if (!colors.Values.Contains(potentialNewColor))
                        {
                            colors[curSelectedSensor] = potentialNewColor;
                            usedColors.Add(potentialNewColor);
                            break;
                        }
                        else
                        {
                            usedColors.Add(curColor);
                        }
            }

            foreach (var node in treeView.AllNodes)
            {
                var sensorNode = node.Tag as SensorNode;
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
            var node = e.Node.Tag as SensorNode;
            e.Value = node != null && plotMenuItem.Checked;
        }

        private void exitClick(object sender, EventArgs e)
        {
            Close();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            computer.Accept(updateVisitor);
            treeView.Invalidate();
            plotPanel.InvalidatePlot();
            systemTray.Redraw();
            if (gadget != null)
                gadget.Redraw();

            if (wmiProvider != null)
                wmiProvider.Update();


            if (logSensors != null && logSensors.Value && delayCount >= 4)
                logger.Log();

            if (delayCount < 4)
                delayCount++;
        }

        private void SaveConfiguration()
        {
            plotPanel.SetCurrentSettings();
            foreach (var column in treeView.Columns)
                settings.SetValue("treeView.Columns." + column.Header + ".Width",
                    column.Width);

            settings.SetValue("listenerPort", Server.ListenerPort);

            var fileName = Path.ChangeExtension(
                Application.ExecutablePath, ".config");
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
            var newBounds = new Rectangle
            {
                X = settings.GetValue("mainForm.Location.X", Location.X),
                Y = settings.GetValue("mainForm.Location.Y", Location.Y),
                Width = settings.GetValue("mainForm.Width", 470),
                Height = settings.GetValue("mainForm.Height", 640)
            };

            var fullWorkingArea = new Rectangle(int.MaxValue, int.MaxValue,
                int.MinValue, int.MinValue);

            foreach (var screen in Screen.AllScreens)
                fullWorkingArea = Rectangle.Union(fullWorkingArea, screen.Bounds);

            var intersection = Rectangle.Intersect(fullWorkingArea, newBounds);
            if (intersection.Width < 20 || intersection.Height < 20 ||
                !settings.Contains("mainForm.Location.X")
            )
            {
                newBounds.X = Screen.PrimaryScreen.WorkingArea.Width / 2 -
                              newBounds.Width / 2;

                newBounds.Y = Screen.PrimaryScreen.WorkingArea.Height / 2 -
                              newBounds.Height / 2;
            }

            Bounds = newBounds;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Visible = false;
            systemTray.IsMainIconEnabled = false;
            timer.Enabled = false;
            computer.Close();
            SaveConfiguration();
            if (runWebServer.Value)
                Server.Quit();
            systemTray.Dispose();
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        private void treeView_Click(object sender, EventArgs e)
        {
            var m = e as MouseEventArgs;
            if (m == null || m.Button != MouseButtons.Right)
                return;

            var info = treeView.GetNodeControlInfoAt(
                new Point(m.X, m.Y)
            );
            treeView.SelectedNode = info.Node;
            if (info.Node != null)
            {
                var node = info.Node.Tag as SensorNode;
                if (node != null && node.Sensor != null)
                {
                    treeContextMenu.MenuItems.Clear();
                    if (node.Sensor.Parameters.Length > 0)
                    {
                        var item = new MenuItem("Parameters...");
                        item.Click += delegate { ShowParameterForm(node.Sensor); };
                        treeContextMenu.MenuItems.Add(item);
                    }
                    if (nodeTextBoxText.EditEnabled)
                    {
                        var item = new MenuItem("Rename");
                        item.Click += delegate { nodeTextBoxText.BeginEdit(); };
                        treeContextMenu.MenuItems.Add(item);
                    }
                    if (node.IsVisible)
                    {
                        var item = new MenuItem("Hide");
                        item.Click += delegate { node.IsVisible = false; };
                        treeContextMenu.MenuItems.Add(item);
                    }
                    else
                    {
                        var item = new MenuItem("Unhide");
                        item.Click += delegate { node.IsVisible = true; };
                        treeContextMenu.MenuItems.Add(item);
                    }
                    treeContextMenu.MenuItems.Add(new MenuItem("-"));
                    {
                        var item = new MenuItem("Pen Color...");
                        item.Click += delegate
                        {
                            var dialog = new ColorDialog();
                            dialog.Color = node.PenColor.GetValueOrDefault();
                            if (dialog.ShowDialog() == DialogResult.OK)
                                node.PenColor = dialog.Color;
                        };
                        treeContextMenu.MenuItems.Add(item);
                    }
                    {
                        var item = new MenuItem("Reset Pen Color");
                        item.Click += delegate { node.PenColor = null; };
                        treeContextMenu.MenuItems.Add(item);
                    }
                    treeContextMenu.MenuItems.Add(new MenuItem("-"));
                    {
                        var item = new MenuItem("Show in Tray");
                        item.Checked = systemTray.Contains(node.Sensor);
                        item.Click += delegate
                        {
                            if (item.Checked)
                                systemTray.Remove(node.Sensor);
                            else
                                systemTray.Add(node.Sensor, true);
                        };
                        treeContextMenu.MenuItems.Add(item);
                    }
                    if (gadget != null)
                    {
                        var item = new MenuItem("Show in Gadget");
                        item.Checked = gadget.Contains(node.Sensor);
                        item.Click += delegate
                        {
                            if (item.Checked) gadget.Remove(node.Sensor);
                            else gadget.Add(node.Sensor);
                        };
                        treeContextMenu.MenuItems.Add(item);
                    }
                    if (node.Sensor.Control != null)
                    {
                        treeContextMenu.MenuItems.Add(new MenuItem("-"));
                        var control = node.Sensor.Control;
                        var controlItem = new MenuItem("Control");
                        var defaultItem = new MenuItem("Default");
                        defaultItem.Checked = control.ControlMode == ControlMode.Default;
                        controlItem.MenuItems.Add(defaultItem);
                        defaultItem.Click += delegate { control.SetDefault(); };
                        var manualItem = new MenuItem("Manual");
                        controlItem.MenuItems.Add(manualItem);
                        manualItem.Checked = control.ControlMode == ControlMode.Software;
                        for (var i = 0; i <= 100; i += 5)
                            if (i <= control.MaxSoftwareValue &&
                                i >= control.MinSoftwareValue)
                            {
                                var item = new MenuItem(i + " %");
                                item.RadioCheck = true;
                                manualItem.MenuItems.Add(item);
                                item.Checked = control.ControlMode == ControlMode.Software &&
                                               Math.Round(control.SoftwareValue) == i;
                                var softwareValue = i;
                                item.Click += delegate { control.SetSoftware(softwareValue); };
                            }
                        treeContextMenu.MenuItems.Add(controlItem);
                    }

                    treeContextMenu.Show(treeView, new Point(m.X, m.Y));
                }

                var hardwareNode = info.Node.Tag as HardwareNode;
                if (hardwareNode != null && hardwareNode.Hardware != null)
                {
                    treeContextMenu.MenuItems.Clear();

                    if (nodeTextBoxText.EditEnabled)
                    {
                        var item = new MenuItem("Rename");
                        item.Click += delegate { nodeTextBoxText.BeginEdit(); };
                        treeContextMenu.MenuItems.Add(item);
                    }

                    treeContextMenu.Show(treeView, new Point(m.X, m.Y));
                }
            }
        }

        private void saveReportMenuItem_Click(object sender, EventArgs e)
        {
            var report = computer.GetReport();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                using (TextWriter w = new StreamWriter(saveFileDialog.FileName))
                {
                    w.Write(report);
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

            if (minimizeToTray.Value &&
                m.Msg == WM_SYSCOMMAND && m.WParam.ToInt64() == SC_MINIMIZE) SysTrayHideShow();
            else if (minimizeOnClose.Value &&
                     m.Msg == WM_SYSCOMMAND && m.WParam.ToInt64() == SC_CLOSE)
                if (minimizeToTray.Value)
                    SysTrayHideShow();
                else
                    WindowState = FormWindowState.Minimized;
            else base.WndProc(ref m);
        }

        private void hideShowClick(object sender, EventArgs e)
        {
            SysTrayHideShow();
        }

        private void ShowParameterForm(ISensor sensor)
        {
            var form = new ParameterForm();
            form.Parameters = sensor.Parameters;
            form.captionLabel.Text = sensor.Name;
            form.ShowDialog();
        }

        private void treeView_NodeMouseDoubleClick(object sender,
            TreeNodeAdvMouseEventArgs e)
        {
            var node = e.Node.Tag as SensorNode;
            if (node != null && node.Sensor != null &&
                node.Sensor.Parameters.Length > 0) ShowParameterForm(node.Sensor);
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

        private void sumbitReportMenuItem_Click(object sender, EventArgs e)
        {
            var form = new ReportForm();
            form.Report = computer.GetReport();
            form.ShowDialog();
        }

        private void resetMinMaxMenuItem_Click(object sender, EventArgs e)
        {
            computer.Accept(new SensorVisitor(delegate(ISensor sensor)
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
            computer.Close();
            computer.Open();
            // restore the MainIcon setting
            systemTray.IsMainIconEnabled = minimizeToTray.Value;
        }

        private void treeView_MouseMove(object sender, MouseEventArgs e)
        {
            selectionDragging = selectionDragging &
                                ((e.Button & (MouseButtons.Left | MouseButtons.Right)) > 0);

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
    }
}