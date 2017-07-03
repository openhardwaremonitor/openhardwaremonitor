using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.WindowsForms;
using OxyPlot.Series;
using System.Linq;
using OxyPlot.Annotations;
using System.Globalization;

namespace OpenHardwareMonitor.GUI
{
    public partial class SensorControlForm : Form
    {
        private readonly Plot mPlot;
        private readonly PlotModel mModel;
        private LineSeries lineSerie;
        private LinearAxis controlAxis;
        private LinearAxis sensorAxis;

        private readonly ISensor control;
        private readonly ISensor sensor;

        private float MinimumSensor;
        private float MaximumSensor;

        private TextAnnotation annotation;
        private Dictionary<SensorType, string> units;
        private string controlTypename = "X";
        private string sensorTypename = "Y";

        public SensorControlForm(ISensor control, ISensor sensor, List<ISoftwareCurvePoint> points)
        {
            this.control = control;
            this.sensor = sensor;

            this.InitializeComponent();

            // Add plot
            this.mPlot = new Plot();
            mModel = new PlotModel();
            mPlot.Height = panel1.Height;
            mPlot.Width = panel1.Width;
            mPlot.Padding = new Padding(0);
            mModel.Padding = new OxyThickness(0);
            mPlot.Model = mModel;
            this.panel1.Controls.Add(mPlot);

            //Line
            lineSerie = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 4,
                MarkerStroke = OxyColors.DarkGray,
                MarkerFill = OxyColors.DarkGray,
                MarkerType = MarkerType.Circle,
                Color = OxyColors.Gray,
                CanTrackerInterpolatePoints = false,
                Smooth = false,
            };
            mModel.Series.Add(lineSerie);

            // new or edit curve
            if (points != null)
            {
                MinimumSensor = points[0].SensorValue;
                MaximumSensor = points[points.Count - 1].SensorValue;

                foreach (var point in points)
                {
                    lineSerie.Points.Add(new DataPoint(point.SensorValue, point.ControlValue));
                }
            }
            else
            {
                if (sensor.SensorType == SensorType.Temperature)
                {
                    MinimumSensor = 20;
                    MaximumSensor = 105;
                }
                else
                {
                    MinimumSensor = sensor.Min.GetValueOrDefault();
                    MaximumSensor = sensor.Max.HasValue ? sensor.Max.Value : MinimumSensor + 100;
                }
                lineSerie.Points.Add(new DataPoint(MinimumSensor, control.Control.MinSoftwareValue));
                lineSerie.Points.Add(new DataPoint(MaximumSensor, control.Control.MaxSoftwareValue));
            }

            // Axes
            UpdateAxes();

            //Annotaion
            annotation = new TextAnnotation();
            annotation.StrokeThickness = 0;
            annotation.TextColor = OxyColors.Red;
            annotation.FontSize = 16;
            annotation.HorizontalAlignment = OxyPlot.HorizontalAlignment.Center;
            annotation.VerticalAlignment = VerticalAlignment.Top;
            annotation.Position = new DataPoint((MinimumSensor + MaximumSensor) / 2, control.Control.MaxSoftwareValue);

            units = new Dictionary<SensorType, string>();
            units.Add(SensorType.Voltage, "V");
            units.Add(SensorType.Clock, "MHz");
            units.Add(SensorType.Temperature, "°C");
            units.Add(SensorType.Load, "%");
            units.Add(SensorType.Fan, "RPM");
            units.Add(SensorType.Flow, "L/h");
            units.Add(SensorType.Control, "%");
            units.Add(SensorType.Level, "%");
            units.Add(SensorType.Factor, "1");
            units.Add(SensorType.Power, "W");
            units.Add(SensorType.Data, "GB");

            if (units.ContainsKey(control.SensorType))
                controlTypename = units[control.SensorType];

            if (units.ContainsKey(sensor.SensorType))
                sensorTypename = units[sensor.SensorType];

            textBox1.Text = Convert.ToString(MinimumSensor);
            textBox2.Text = Convert.ToString(MaximumSensor);
            textBox1.TextChanged += textBox1_TextChanged;
            textBox2.TextChanged += textBox2_TextChanged;

            mPlot.MouseDown += MPlot_MouseDown;
        }
        private void UpdateAxes()
        {
            mModel.Axes.Clear();

            controlAxis = new LinearAxis(AxisPosition.Left, 0) { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Control" };
            controlAxis.IsZoomEnabled = false;
            controlAxis.IsPanEnabled = false;
            controlAxis.Maximum = control.Control.MaxSoftwareValue;
            controlAxis.Minimum = control.Control.MinSoftwareValue;
            controlAxis.Title = control.Name;
            mModel.Axes.Add(controlAxis);

            sensorAxis = new LinearAxis(AxisPosition.Bottom, 0) { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Sensor" };
            sensorAxis.IsZoomEnabled = false;
            sensorAxis.IsPanEnabled = false;
            sensorAxis.Maximum = MaximumSensor;
            sensorAxis.Minimum = MinimumSensor;
            sensorAxis.Title = sensor.Name;
            mModel.Axes.Add(sensorAxis);

            // update first and last points
            var firstpoint = lineSerie.Points.ElementAt(0);
            firstpoint.X = MinimumSensor;

            var lastpoint = lineSerie.Points.ElementAt(lineSerie.Points.Count - 1);
            lastpoint.X = MaximumSensor;

            // remove too small or large points
            for (int i = lineSerie.Points.Count - 2; i > 0; i--)
            {
                var el = lineSerie.Points.ElementAt(i);
                if (el.X > sensorAxis.Maximum ||
                    el.X < sensorAxis.Minimum ||
                    el.Y > controlAxis.Maximum ||
                    el.Y < controlAxis.Minimum)
                {
                    lineSerie.Points.RemoveAt(i);
                }
            }

            mModel.Update(true);
            mPlot.RefreshPlot(true);
            mPlot.Refresh();
        }

        private IDataPoint before_foundPoint;
        private IDataPoint foundPoint;
        private bool foundPointIsEndPoint;
        private IDataPoint after_foundPoint;

        private void MPlot_MouseDown(object sender, MouseEventArgs e)
        {
            // right click deletes the point at the mouse location
            if (e.Button == MouseButtons.Right)
            {
                DeletePoint(new ScreenPoint(e.X, e.Y));
            }
            // left button moves en existing point, or creates a new one if no points at location
            else if (e.Button == MouseButtons.Left)
            {
                foundPoint = Search(new ScreenPoint(e.X, e.Y));

                // move point
                if (foundPoint != null)
                {
                    // can move first and last point but keep Y/ControlValue value 
                    var idx = lineSerie.Points.IndexOf(foundPoint);
                    if (idx > 0 && idx < lineSerie.Points.Count - 1)
                    {
                        foundPointIsEndPoint = false;
                        before_foundPoint = lineSerie.Points.ElementAt(idx - 1);
                        after_foundPoint = lineSerie.Points.ElementAt(idx + 1);
                    }
                    else
                        foundPointIsEndPoint = true;

                    AttachMouseMove();
                }
                // create new point
                else
                {
                    var point = Axis.InverseTransform(new ScreenPoint(e.X, e.Y), sensorAxis, controlAxis);
                    var newpoint = new DataPoint(point.X, point.Y);

                    // prevent creating points outside of range
                    if (newpoint.X < sensorAxis.Maximum &&
                        newpoint.X > sensorAxis.Minimum &&
                        newpoint.Y < controlAxis.Maximum &&
                        newpoint.Y > controlAxis.Minimum)
                        for (var i = 0; i < lineSerie.Points.Count; i++)
                        {
                            if (newpoint.X >= lineSerie.Points.ElementAt(i).X &&
                                newpoint.X <= lineSerie.Points.ElementAt(i + 1).X)
                            {
                                var idx = i + 1;
                                lineSerie.Points.Insert(idx, newpoint);
                                mPlot.RefreshPlot(true);
                                before_foundPoint = lineSerie.Points.ElementAt(idx - 1);
                                foundPoint = lineSerie.Points.ElementAt(idx);
                                after_foundPoint = lineSerie.Points.ElementAt(idx + 1);
                                AttachMouseMove();
                                return;
                            }
                        }
                }
            }
            mPlot.Refresh();
        }

        private void AttachMouseMove()
        {
            MouseEventHandler mouseUpListener = null;
            mouseUpListener = (curveselect_sender, curveselect_e) =>
            {
                mPlot.MouseUp -= mouseUpListener;
                mPlot.MouseMove -= MPlot_MouseMove;
                before_foundPoint = null;
                foundPoint = null;
                after_foundPoint = null;
                mModel.Annotations.Remove(annotation);
                mModel.Update(true);
                mPlot.RefreshPlot(true);
                mPlot.Refresh();
            };

            mModel.Annotations.Add(annotation);
            mPlot.MouseUp += mouseUpListener;
            mPlot.MouseMove += MPlot_MouseMove;
        }

        private void MPlot_MouseMove(object sender, MouseEventArgs e)
        {
            DataPoint mousepos = lineSerie.InverseTransform(new ScreenPoint(e.X, e.Y));

            if (!foundPointIsEndPoint)
                if (mousepos.X <= before_foundPoint.X)
                    foundPoint.X = before_foundPoint.X;
                else if (mousepos.X >= after_foundPoint.X)
                    foundPoint.X = after_foundPoint.X;
                else
                    foundPoint.X = mousepos.X;

            if (mousepos.Y >= control.Control.MaxSoftwareValue)
                foundPoint.Y = control.Control.MaxSoftwareValue;
            else if (mousepos.Y <= control.Control.MinSoftwareValue)
                foundPoint.Y = control.Control.MinSoftwareValue;
            else
                foundPoint.Y = mousepos.Y;


            double sensorvalue;

            if (sensor.SensorType == SensorType.Voltage)
                sensorvalue = Math.Round(foundPoint.X, 4);
            else if (sensor.SensorType == SensorType.Power)
                sensorvalue = Math.Round(foundPoint.X, 1);
            else if (sensor.SensorType == SensorType.Flow)
                sensorvalue = Math.Round(foundPoint.X, 1);
            else
                sensorvalue = Math.Round(foundPoint.X, 0);

            annotation.Text = Math.Round(foundPoint.Y, 0) + " " + controlTypename + " - " + sensorvalue + " " + sensorTypename;

            mPlot.Refresh();
        }

        private IDataPoint Search(ScreenPoint point)
        {
            var mousepos = lineSerie.InverseTransform(point);
            return lineSerie.Points.FirstOrDefault(p => Math.Abs(p.X - mousepos.X) < 2 && Math.Abs(p.Y - mousepos.Y) < 2);
        }

        private void DeletePoint(ScreenPoint point)
        {
            var mousepos = lineSerie.InverseTransform(point);
            var deletepoint = lineSerie.Points.FirstOrDefault(p => Math.Abs(p.X - mousepos.X) < 2 && Math.Abs(p.Y - mousepos.Y) < 2);
            if (deletepoint != null)
            {
                var idx = lineSerie.Points.IndexOf(deletepoint);
                if (idx != 0 && idx != lineSerie.Points.Count - 1)
                    lineSerie.Points.Remove(deletepoint);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            float input1;
            var isnum = float.TryParse(textBox1.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out input1);

            if (isnum)
            {
                MinimumSensor = input1;
                UpdateAxes();
            }
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            float input2;
            var isnum = float.TryParse(textBox2.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out input2);

            if (isnum)
            {
                MaximumSensor = input2;
                UpdateAxes();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<ISoftwareCurvePoint> softwareCurvePoints = new List<ISoftwareCurvePoint>();

            foreach (var point in lineSerie.Points)
            {
                softwareCurvePoints.Add(new SoftwareCurvePoint
                {
                    SensorValue = (float)point.X,
                    ControlValue = (float)point.Y
                });
            }

            if (softwareCurvePoints.Count < 2)
            {
                MessageBox.Show("There are less than the required 2 points", "Error", MessageBoxButtons.OK);
                return;
            }

            control.Control.SetSoftwareCurve(softwareCurvePoints, sensor);
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

    public class SoftwareCurvePoint : ISoftwareCurvePoint
    {
        public float SensorValue { get; set; }
        public float ControlValue { get; set; }
    }
}
