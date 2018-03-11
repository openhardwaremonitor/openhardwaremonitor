/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
	Copyright (C) 2013 Michael Möller <mmoeller@openhardwaremonitor.org>

*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Text;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.Utilities
{
	public class Log : TraceSource
	{
		public Log(string fileName) : base("FanCtrl", SourceLevels.All)
		{
			this.Listeners.Add(new TextWriterTraceListener(fileName, "FanCtrlLog"));
		}
	}

	struct Smoother
	{
		public readonly int smaPeriod;
		readonly float k1, k2;

		public Smoother(int smaPeriod)
		{
			this.smaPeriod = smaPeriod;
			this.k1 = smaPeriod > 0 ? (float)1 / (float)smaPeriod : 1;
			this.k2 = (float)1 - k1;
		}

		public void Update(ref int value, int newValue)
		{
			value = (int)( k1 * (float)newValue + k2 * (float)value);
		}

		public void Update(ref float value, float newValue)
		{
			value = newValue * k1 + value * k2;
		}
	}

	public class ControlSignal
	{
		public ISensor sensor;
		public float value;
		public int target;
		Smoother inputSmoother;

		public readonly string identifier;

		bool isLogEnabled;

		public ControlSignal(string identifier, List<Point> points, int smaPeriod, bool isLogEnabled)
		{
			this.inputSmoother = new Smoother(smaPeriod);

			this.identifier = identifier;
			this.points = points;
			this.points.Sort((p1, p2) => p1.value.CompareTo(p2.value));
			this.isLogEnabled = isLogEnabled;
		}

		public struct Point
		{
			public float value;
			public float target;
		}

		public void InitialUpdate()
		{
			float? newValue = sensor.Value;
			if (newValue.HasValue)
			{
				value = newValue.Value;
				target = (int)CalcTarget();
			}
		}

		public void Update( out bool isPresent)
		{
			isPresent = false;

			float? newValue = sensor.Value;
			if (newValue.HasValue)
			{
				isPresent = true;

				inputSmoother.Update(ref value, newValue.Value);

				target = (int)CalcTarget();

				if (isLogEnabled)
				{
					FanControllers.log.TraceInformation("Signal {0} value {1} target {2}", identifier, (int)value, (int)target);
					FanControllers.log.Flush();
				}
			}
		}

		//  target % (fan speed)
		//  100                  * -------------
		//                     - |
		//                   -   |
		//   50           *      |
		//              - |      |
		//            -   |      |
		//   25 ---*      |      |
		//		   |      |      |
		//   0 ... 40 ... 60 ... 80 ... 100 ...   <= sensor.Value %
		//		   + point(value=40, target=25)
		//                + point(value=60, target=50)
		//                       + point(value=80, target=100)

		public List<Point> points;

		static float Interpolate(Point a, Point b, float value)
		{
			if (a.value >= b.value)
			{
				return b.target;
			}

			if (value <= a.value)
			{
				return a.target;
			}

			float ratio = (value - a.value) / (b.value - a.value);
			return a.target + ratio * (b.target - a.target);
		}

		float CalcTarget()
		{
			Point prev = points[0];

			if (points.Count == 1)
			{
				return points[0].target;
			}

			foreach (Point cur in points)
			{
				if (value <= cur.value)
				{
					return Interpolate(prev, cur, value);
				}
				prev = cur;
			}

			// greater than max Point.target
			int last = points.Count-1;
			return Interpolate(points[last-1], points[last], value);

		}
	}

	// Keep fans runnnig bit longer after consistent high load
	public class SlowTargetController
	{
		int tickLength;
		int tick;

		int maxSmoothedInputInTick;
		int smoothedInput;
		public int smoothedOutput;
		int target;

		bool isLogEnabled;

		int delayIndex;
		int[] delayBuf;

		Smoother inputSmoother;
		Smoother outputSmoother;

		public SlowTargetController(int smaPeriod, int delayPeriod, int initialTarget, bool isLogEnabled)
		{
			this.isLogEnabled = isLogEnabled;

			smaPeriod = Math.Max(smaPeriod, delayPeriod/10);

			inputSmoother = new Smoother(smaPeriod);
			outputSmoother = new Smoother(smaPeriod);

			delayPeriod = Math.Max(delayPeriod, smaPeriod);
			this.tickLength = Math.Max(1, smaPeriod/10);

			delayBuf = new int[Math.Max(1, delayPeriod / this.tickLength)];

			InitialUpdate(initialTarget);
		}

		public int InitialUpdate(int curTarget)
		{
			delayBuf.Fill(curTarget);
			smoothedInput = curTarget;
			smoothedOutput = curTarget;

			return curTarget;
		}

		public int Update(int curTarget)
		{
			inputSmoother.Update(ref smoothedInput, curTarget);

			maxSmoothedInputInTick = Math.Max(maxSmoothedInputInTick, smoothedInput);

			tick++;
			if (tick >= tickLength)
			{
				tick = 0;
				delayBuf[delayIndex++] = maxSmoothedInputInTick;

				maxSmoothedInputInTick = smoothedInput;

				if (delayIndex >= delayBuf.Length)
				{
					delayIndex = 0;
				}
			}

			int delayedOutput = Math.Max(delayBuf.Max(), maxSmoothedInputInTick);

			outputSmoother.Update(ref smoothedOutput, delayedOutput);

			target = Math.Max(curTarget, smoothedOutput);

			if (isLogEnabled)
			{
				FanControllers.log.TraceInformation("SlowTargetController output {0}", smoothedOutput);
				FanControllers.log.Flush();
			}

			return target;
		}
	}


	public class FanController
	{
		private readonly IComputer computer;

		private ISensor controlledSensor;
		private string controlledSensorId;

		List<ControlSignal> signals;

		ControlSignal effectiveSignal;

		private DateTime lastTickTime = DateTime.MinValue;

		SlowTargetController slowTargetController;

		private readonly int minTarget, maxTarget;

		private int curTarget;

		private int lastEffectiveTarget;

		bool initialUpdateIsDone;
		bool isLogEnabled;


		public FanController(IComputer computer,
				string controlledSensorId,
				List<ControlSignal> signals,
				int minTarget,
				int maxTarget,
				int tickInterval,
				SlowTargetController slowTargetController,
				bool isLogEnabled)
		{
			TickInterval = System.TimeSpan.FromSeconds(tickInterval);
			this.computer = computer;
			this.computer.HardwareAdded += HardwareAdded;
			this.computer.HardwareRemoved += HardwareRemoved;

			this.controlledSensorId = controlledSensorId;

			this.slowTargetController = slowTargetController;

			this.signals = signals;

			this.minTarget = minTarget;
			this.maxTarget = maxTarget;

			lastEffectiveTarget = minTarget - 1;

			this.initialUpdateIsDone = false;
			this.isLogEnabled = isLogEnabled;
		}

		#region HardwareEventHandlers
		private void HardwareRemoved(IHardware hardware)
		{
			hardware.SensorAdded -= SensorAdded;
			hardware.SensorRemoved -= SensorRemoved;
			foreach (ISensor sensor in hardware.Sensors)
				SensorRemoved(sensor);
			foreach (IHardware subHardware in hardware.SubHardware)
				HardwareRemoved(subHardware);
		}

		private void HardwareAdded(IHardware hardware)
		{
			foreach (ISensor sensor in hardware.Sensors)
				SensorAdded(sensor);
			hardware.SensorAdded += SensorAdded;
			hardware.SensorRemoved += SensorRemoved;
			foreach (IHardware subHardware in hardware.SubHardware)
				HardwareAdded(subHardware);
		}

		private void SensorAdded(ISensor sensor)
		{
			if (sensor.Identifier.ToString() == controlledSensorId)
			{
				controlledSensor = sensor;
				InitControlSensor();
				return;
			}

			if (signals == null)
				return;

			bool allSensorsConnected = true;

			foreach (ControlSignal signal in signals)
			{
				if (signal != null && sensor.Identifier.ToString() == signal.identifier)
				{
					signal.sensor = sensor;
					//signal.InitialUpdate();
				}

				if (signal == null || signal.sensor == null)
				{
					allSensorsConnected = false;
				}
			}

			if (allSensorsConnected && !initialUpdateIsDone)
			{
				InitialUpdate();
			}
		}

		private void SensorRemoved(ISensor sensor)
		{
			if (sensor == controlledSensor)
			{
				controlledSensor = null;
				return;
			}

			if (signals == null)
				return;

			foreach (ControlSignal signal in signals)
			{
				if (signal != null && sensor == signal.sensor)
				{
					signal.sensor = null;
				}
			}
		}
		#endregion HardwareEventHandlers

		void InitialUpdate()
		{
			int maxTarget = minTarget;
			foreach (ControlSignal signal in signals)
			{
				signal.InitialUpdate();
				maxTarget = Math.Max(maxTarget, signal.target);
			}

			if (slowTargetController != null)
			{
				slowTargetController.InitialUpdate(maxTarget);
			}

			initialUpdateIsDone = true;

			if (isLogEnabled)
			{
				FanControllers.log.TraceInformation("Fan controller {0} initial update is done", controlledSensorId);
				FanControllers.log.Flush();
			}
		}

		public TimeSpan TickInterval { get; set; }

		public void Tick()
		{
			if (controlledSensor == null)
			{
				return;
			}

			var now = DateTime.Now;

			if (lastTickTime + TickInterval > now)
				return;
			lastTickTime = now;

			try
			{
				bool hasMissingSensors = false;
				ControlSignal maxSignal = null;
				{
					int newTarget = minTarget;

					foreach (ControlSignal signal in signals)
					{
						signal.Update(out bool isPresent);
						if (!isPresent)
						{
							hasMissingSensors = true;
							break;
						}
						if (signal.target > newTarget || maxSignal == null)
						{
							maxSignal = signal;
							newTarget = signal.target;
						}
						newTarget = Math.Max(newTarget, signal.target);
					}

					if (!hasMissingSensors)
					{
						if (isLogEnabled && maxSignal != effectiveSignal)
						{
							effectiveSignal = maxSignal;
							FanControllers.log.TraceInformation("Fan controller {0} max signal is {1}", controlledSensorId, maxSignal.identifier);
							FanControllers.log.Flush();
						}

						curTarget = Math.Min(newTarget, maxTarget);
					}
				}

				int effectiveTarget = slowTargetController != null ?
						slowTargetController.Update(curTarget) : curTarget;

				effectiveTarget = Math.Max(effectiveTarget, minTarget);
				effectiveTarget = Math.Min(effectiveTarget, maxTarget);

				if (hasMissingSensors)
				{
					// Enable auto control
					controlledSensor.Control.SetDefault();

					if (isLogEnabled)
					{
						FanControllers.log.TraceInformation("Fan controller {0} set to DEFAULT", controlledSensorId);
						FanControllers.log.Flush();
					}
				}
				else
				{
					if (lastEffectiveTarget != effectiveTarget)
					{
						lastEffectiveTarget = effectiveTarget;

						// Set software value
						controlledSensor.Control.SetSoftware(effectiveTarget);

						if (isLogEnabled)
						{
							FanControllers.log.TraceInformation("Fan controller {0} (max signal {2} value {3:F2} target {4} slow {5}) set to {1}",
								controlledSensorId, effectiveTarget,
								effectiveSignal.identifier, effectiveSignal.value, effectiveSignal.target,
								slowTargetController==null ? 0 : slowTargetController.smoothedOutput);
							FanControllers.log.Flush();
						}
					}
				}
			}
			catch (IOException) { }
		}

		private void InitControlSensor()
		{
			//Unused
		}

	}


	class ParseLog
	{
		public void Begin(string s)
		{
			Add("");
			Add("<" + s + ">");
			padding += "\t";
		}

		public void End(string s)
		{
			padding = padding.Substring(1);
			Add("</" + s + ">");
		}

		public void Add(string s)
		{
			parseLog.Add(padding + s);
		}

		public void AddError(string s)
		{
			parseLog.Add("\n" + padding + "ERROR: " + s + "\n");
			errorCount++;
		}

		public void AddWarning(string s)
		{
			parseLog.Add("\n" + padding + "WARNING: " + s + "\n");
		}

		public bool HasErrors()
		{
			return errorCount != 0;
		}

		public void Save(string fileName)
		{
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
			{
				file.WriteLine(String.Format("Fan controllers configuration has {0} errors\n", errorCount));

				foreach (string line in parseLog)
				{
					file.WriteLine(line);
				}
			}
		}

		List<string> parseLog = new List<string>();
		string padding = "";
		int errorCount = 0;
	}


	public class FanControllers
	{
		private readonly IComputer computer;

		List<FanController> fanControllers= new List<FanController>();

		public static Log log;

		bool isLogEnabled;


		public FanControllers(IComputer computer)
		{
			this.computer = computer;
			this.isLogEnabled = false;
		}

		public void Tick()
		{
			foreach (FanController fc in fanControllers)
			{
				fc.Tick();
			}
		}

		#region ReadXML
		bool ReadFloat(XmlAttributeCollection attrs, string name, out float value, ParseLog parseLog)
		{
			value = 0;

			XmlAttribute attr = attrs[name];
			if (attr == null)
			{
				parseLog.AddError(String.Format("attribute {0} not found", name));
				return false;
			}
			value = float.Parse(attr.Value);
			parseLog.Add(String.Format("{0}={1}", name, value));
			return true;
		}


		bool ReadInt(XmlAttributeCollection attrs, string name, out int value, ParseLog parseLog)
		{
			value = 0;

			XmlAttribute attr = attrs[name];
			if (attr == null)
			{
				parseLog.AddError( String.Format("attribute {0} not found", name) );
				return false;
			}
			value = int.Parse(attr.Value);
			parseLog.Add(String.Format("{0}={1}", name, value));
			return true;
		}


		bool ReadString(XmlAttributeCollection attrs, string name, out string value, ParseLog parseLog)
		{
			value = "";

			XmlAttribute attr = attrs[name];
			if (attr == null)
			{
				parseLog.AddError(String.Format("attribute {0} not found", name));
				return false;
			}
			value = attr.Value;
			parseLog.Add(String.Format("{0}={1}", name, value));
			return true;
		}


		string ReadOptionalString(XmlAttributeCollection attrs, string name, string defaultValue, ParseLog parseLog)
		{
			string value = defaultValue;

			XmlAttribute attr = attrs[name];
			if (attr == null)
			{
				return defaultValue;
			}
			value = attr.Value;
			parseLog.Add(String.Format("{0}={1}", name, value));
			return value;
		}
		#endregion ReadXML

		public static void Init(string logFileName)
		{
			log = new Log(logFileName);
		}

		public bool LoadConfig(string configFileName)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(configFileName);
			}
			catch
			{
				// File doesn't exist?
				return true;
			}

			bool alwaysSaveLog = false;

			ParseLog parseLog = new ParseLog();

			XmlNodeList list = doc.GetElementsByTagName("fanControllers");
			foreach (XmlNode node in list)
			{
				XmlNode parent = node.ParentNode;
				if (parent != null && parent.Name == "configuration" && parent.ParentNode is XmlDocument)
				{
					parseLog.Begin("fanControllers");

					alwaysSaveLog = ReadOptionalString(node.Attributes, "alwaysSaveParsingLog", "no", parseLog) == "yes";

					this.isLogEnabled = ReadOptionalString(node.Attributes, "enableLog", "no", parseLog) == "yes";

					if (this.isLogEnabled)
					{
						log.TraceInformation("----------------- started -----------------");
						log.TraceInformation("LoadConfig: loading {0}", configFileName);
					}

					foreach (XmlNode fcNode in node.ChildNodes)
					{
						if (fcNode.Name == "fanController")
						{
							parseLog.Begin("fanController");

							XmlAttributeCollection fcAttrs = fcNode.Attributes;

							if (!ReadString(fcAttrs, "controlledSensorId", out string controlledSensorId, parseLog) ||
								!ReadInt(fcAttrs, "minTarget", out int minTarget, parseLog) ||
								!ReadInt(fcAttrs, "maxTarget", out int maxTarget, parseLog) ||
								!ReadInt(fcAttrs, "tickIntervalSeconds", out int tickIntervalSeconds, parseLog))
							{
								continue;
							}

							bool fcLogEnabled = ReadOptionalString(fcAttrs, "enableLog", "no", parseLog) == "yes";

							SlowTargetController slowTargetController = null;

							List<ControlSignal> signals = new List<ControlSignal>();

							foreach (XmlNode signalNode in fcNode.ChildNodes)
							{
								if (signalNode.Name == "controlSignal")
								{
									parseLog.Begin("controlSignal");

									XmlAttributeCollection signalAttrs = signalNode.Attributes;

									if (!ReadString(signalAttrs, "id", out string signalId, parseLog) ||
										!ReadInt(signalAttrs, "smaPeriod", out int smaPeriod, parseLog))
									{
										continue;
									}

									bool signalLogEnabled = ReadOptionalString(signalAttrs, "enableLog", "no", parseLog) == "yes";

									List<ControlSignal.Point> points = new List<ControlSignal.Point>();

									foreach (XmlNode pointNode in signalNode.ChildNodes)
									{
										if (pointNode.Name == "point")
										{
											parseLog.Begin("point");

											XmlAttributeCollection pointAttrs = pointNode.Attributes;

											if (ReadFloat(pointAttrs, "value", out float pvalue, parseLog) &&
												ReadFloat(pointAttrs, "target", out float ptarget, parseLog))
											{
												points.Add(new ControlSignal.Point { value = pvalue, target = ptarget });
											}
											else
											{
												continue;
											}

											parseLog.End("point");
										}
									}

									if (points.Count == 0)
									{
										parseLog.AddError(@"Points are not defined (<point> elements)");
										continue;
									}
									else if (points.Count == 1)
									{
										parseLog.AddWarning(@"Only single point defined. Signal target will be fixed.");
										continue;
									}

									signals.Add(new ControlSignal(signalId, points, smaPeriod, signalLogEnabled));

									parseLog.End("controlSignal");
								}
								else if (signalNode.Name == "slowTargetController")
								{
									parseLog.Begin("slowTargetController");

									if (slowTargetController != null)
									{
										parseLog.AddError("slowTargetController already defined for current fanController.");
										continue;
									}

									XmlAttributeCollection stcAttrs = signalNode.Attributes;

									if (ReadInt(stcAttrs, "smaPeriod", out int smaPeriod, parseLog) &&
										ReadInt(stcAttrs, "delayPeriod", out int delayPeriod, parseLog))
									{
										bool logEnabled = ReadOptionalString(stcAttrs, "enableLog", "no", parseLog) == "yes";
										slowTargetController = new SlowTargetController(smaPeriod, delayPeriod, minTarget, logEnabled);
									}

									parseLog.End("slowTargetController");
								}
							}

							if (signals.Count == 0)
							{
								parseLog.AddError(@"Control signals are not defined (<controlSignal> elements).");
								continue;
							}
							fanControllers.Add(new FanController(computer, controlledSensorId, signals,
								minTarget, maxTarget, tickIntervalSeconds, slowTargetController, fcLogEnabled));

							parseLog.End("fanController");
						}
					}
					parseLog.End("fanControllers");
				}
			}

			if (parseLog.HasErrors())
			{
				log.TraceInformation("LoadConfig: configuration error");
			}

			if (parseLog.HasErrors() || alwaysSaveLog)
			{
				System.IO.Path.ChangeExtension(configFileName, "");
				parseLog.Save(configFileName + ".parsing.log");
			}

			return !parseLog.HasErrors();
		}
	}

	public static class ArrayExtensions
	{
		public static void Fill<T>(this T[] originalArray, T with)
		{
			for (int i = 0; i < originalArray.Length; i++)
			{
				originalArray[i] = with;
			}
		}
	}
}
