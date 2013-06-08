// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSeries.cs" company="OxyPlot">
//   The MIT License (MIT)
//
//   Copyright (c) 2012 Oystein Bjorke
//
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary>
//   DataPointProvider interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace OxyPlot
{
    /// <summary>
    /// DataPointProvider interface.
    /// </summary>
    public interface IDataPointProvider
    {
        /// <summary>
        /// Gets the data point.
        /// </summary>
        /// <returns></returns>
        DataPoint GetDataPoint();
    }

    public abstract class DataSeries : PlotSeriesBase
    {
        protected IList<DataPoint> points;

        protected DataSeries()
        {
            points = new Collection<DataPoint>();
            DataFieldX = "X";
            DataFieldY = "Y";
            CanTrackerInterpolatePoints = false;
        }

        /// <summary>
        /// Gets or sets the items source.
        /// </summary>
        /// <value>The items source.</value>
        public IEnumerable ItemsSource { get; set; }

        /// <summary>
        /// Gets or sets the data field X.
        /// </summary>
        /// <value>The data field X.</value>
        public string DataFieldX { get; set; }

        /// <summary>
        /// Gets or sets the data field Y.
        /// </summary>
        /// <value>The data field Y.</value>
        public string DataFieldY { get; set; }

        /// <summary>
        /// Gets or sets the mapping deleagte.
        /// Example: series1.Mapping = item => new DataPoint(((MyType)item).Time,((MyType)item).Value);
        /// </summary>
        /// <value>The mapping.</value>
        public Func<object, DataPoint> Mapping { get; set; }

        /// <summary>
        /// Gets or sets the points.
        /// </summary>
        /// <value>The points.</value>
        [Browsable(false)]
        public IList<DataPoint> Points
        {
            get { return points; }
            set { points = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref = "DataSeries" /> is smooth.
        /// </summary>
        /// <value><c>true</c> if smooth; otherwise, <c>false</c>.</value>
        public bool Smooth { get; set; }

        public override void UpdateData()
        {
            if (ItemsSource == null)
            {
                return;
            }

            points.Clear();

            // Use the mapping to generate the points
            if (Mapping != null)
            {
                foreach (var item in ItemsSource)
                {
                    points.Add(Mapping(item));
                }
            }

            // Get DataPoints from the items in ItemsSource
            // if they implement IDataPointProvider
            // If DataFields are set, this is not used
            if (DataFieldX == null || DataFieldY == null)
            {
                foreach (var item in ItemsSource)
                {
                    var idpp = item as IDataPointProvider;
                    if (idpp == null)
                    {
                        continue;
                    }

                    points.Add(idpp.GetDataPoint());
                }

                return;
            }

            // TODO: is there a better way to do this?
            // http://msdn.microsoft.com/en-us/library/bb613546.aspx

            // Using reflection on DataFieldX and DataFieldY
            AddDataPoints(points, ItemsSource, DataFieldX, DataFieldY);
        }

        /// <summary>
        /// Converts the value of the specified object to a double precision floating point number.
        /// DateTime objects are converted using DateTimeAxis.ToDouble
        /// TimeSpan objects are converted using TimeSpanAxis.ToDouble
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected virtual double ToDouble(object value)
        {
            if (value is DateTime)
            {
                return DateTimeAxis.ToDouble((DateTime)value);
            }

            if (value is TimeSpan)
            {
                return ((TimeSpan)value).TotalSeconds;
            }

            return Convert.ToDouble(value);
        }

        /// <summary>
        /// Updates the max/min from the datapoints.
        /// </summary>
        public override void UpdateMaxMin()
        {
            base.UpdateMaxMin();
            InternalUpdateMaxMin(points);
        }

        /// <summary>
        /// Gets the point in the dataset that is nearest the specified point.
        /// </summary>
        /// <param name = "point">The point.</param>
        /// <param name = "dpn">The nearest point (data coordinates).</param>
        /// <param name = "spn">The nearest point (screen coordinates).</param>
        /// <returns></returns>
        public override bool GetNearestPoint(ScreenPoint point, out DataPoint dpn, out ScreenPoint spn)
        {
            spn = default(ScreenPoint);
            dpn = default(DataPoint);

            double minimumDistance = double.MaxValue;
            foreach (var p in points)
            {
                var sp = AxisBase.Transform(p, XAxis, YAxis);
                double dx = sp.x - point.x;
                double dy = sp.y - point.y;
                double d2 = dx * dx + dy * dy;

                if (d2 < minimumDistance)
                {
                    dpn = p;
                    spn = sp;
                    minimumDistance = d2;
                }
            }

            return minimumDistance < double.MaxValue;
        }

        /// <summary>
        /// Gets the point on the curve that is nearest the specified point.
        /// </summary>
        /// <param name = "point">The point.</param>
        /// <param name = "dpn">The nearest point (data coordinates).</param>
        /// <param name = "spn">The nearest point (screen coordinates).</param>
        /// <returns></returns>
        public override bool GetNearestInterpolatedPoint(ScreenPoint point, out DataPoint dpn, out ScreenPoint spn)
        {
            spn = default(ScreenPoint);
            dpn = default(DataPoint);

            // http://local.wasp.uwa.edu.au/~pbourke/geometry/pointline/
            double minimumDistance = double.MaxValue;

            for (int i = 0; i + 1 < points.Count; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];
                var sp1 = AxisBase.Transform(p1, XAxis, YAxis);
                var sp2 = AxisBase.Transform(p2, XAxis, YAxis);

                double sp21X = sp2.x - sp1.x;
                double sp21Y = sp2.y - sp1.y;
                double u1 = (point.x - sp1.x) * sp21X + (point.y - sp1.y) * sp21Y;
                double u2 = sp21X * sp21X + sp21Y * sp21Y;
                double ds = sp21X * sp21X + sp21Y * sp21Y;

                if (ds < 4)
                {
                    // if the points are very close, we can get numerical problems, just use the first point...
                    u1 = 0; u2 = 1;
                }

                if (u2 == 0)
                {
                    continue; // P1 && P2 coincident
                }

                double u = u1 / u2;
                if (u < 0 || u > 1)
                {
                    continue; // outside line
                }

                double sx = sp1.x + u * sp21X;
                double sy = sp1.y + u * sp21Y;

                double dx = point.x - sx;
                double dy = point.y - sy;
                double distance = dx * dx + dy * dy;

                if (distance < minimumDistance)
                {
                    double px = p1.x + u * (p2.x - p1.x);
                    double py = p1.y + u * (p2.y - p1.y);
                    dpn = new DataPoint(px, py);
                    spn = new ScreenPoint(sx, sy);
                    minimumDistance = distance;
                }
            }

            return minimumDistance < double.MaxValue;
        }

        protected void AddDataPoints(ICollection<DataPoint> points, IEnumerable itemsSource, string dataFieldX, string dataFieldY)
        {
            PropertyInfo pix = null;
            PropertyInfo piy = null;
            Type t = null;

            foreach (var o in itemsSource)
            {
                if (pix == null || o.GetType() != t)
                {
                    t = o.GetType();
                    pix = t.GetProperty(dataFieldX);
                    piy = t.GetProperty(dataFieldY);
                    if (pix == null)
                    {
                        throw new InvalidOperationException(string.Format("Could not find data field {0} on type {1}",
                                                                          DataFieldX, t));
                    }

                    if (piy == null)
                    {
                        throw new InvalidOperationException(string.Format("Could not find data field {0} on type {1}",
                                                                          DataFieldY, t));
                    }
                }

                var x = ToDouble(pix.GetValue(o, null));
                var y = ToDouble(piy.GetValue(o, null));

                var pp = new DataPoint(x, y);
                points.Add(pp);
            }
        }

        /// <summary>
        /// Updates the Max/Min limits from the specified point list.
        /// </summary>
        /// <param name="pts">The PTS.</param>
        protected void InternalUpdateMaxMin(IList<DataPoint> pts)
        {
            if (pts == null || pts.Count == 0)
            {
                return;
            }

            double minx = MinX;
            double miny = MinY;
            double maxx = MaxX;
            double maxy = MaxY;

            foreach (var pt in pts)
            {
                if (!IsValidPoint(pt,XAxis,YAxis))
                    continue;
                if (pt.x < minx || double.IsNaN(minx)) minx = pt.x;
                if (pt.x > maxx || double.IsNaN(maxx)) maxx = pt.x;
                if (pt.y < miny || double.IsNaN(miny)) miny = pt.y;
                if (pt.y > maxy || double.IsNaN(maxy)) maxy = pt.y;
            }

            MinX = minx;
            MinY = miny;
            MaxX = maxx;
            MaxY = maxy;

            XAxis.Include(MinX);
            XAxis.Include(MaxX);
            YAxis.Include(MinY);
            YAxis.Include(MaxY);
        }

        /// <summary>
        /// Gets the value from the specified X.
        /// </summary>
        /// <param name = "x">The x.</param>
        /// <returns></returns>
        public double? GetValueFromX(double x)
        {
            for (int i = 0; i + 1 < points.Count; i++)
            {
                if (IsBetween(x, points[i].x, points[i + 1].x))
                {
                    return points[i].y +
                           (points[i + 1].y - points[i].y) /
                           (points[i + 1].x - points[i].x) * (x - points[i].x);
                }
            }

            return null;
        }

        private static bool IsBetween(double x, double x0, double x1)
        {
            if (x >= x0 && x <= x1)
            {
                return true;
            }

            if (x >= x1 && x <= x0)
            {
                return true;
            }

            return false;
        }

        public virtual bool IsValidPoint(DataPoint pt, IAxis xAxis, IAxis yAxis)
        {
            return !double.IsNaN(pt.X) && !double.IsInfinity(pt.X)
                   && !double.IsNaN(pt.Y) && !double.IsInfinity(pt.Y)
                   && (xAxis!=null && xAxis.IsValidValue(pt.X))
                   && (yAxis!=null && yAxis.IsValidValue(pt.Y));
        }
    }
}