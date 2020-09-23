using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace Aga.Controls
{
	/// <summary>
	/// High resolution timer, used to test performance
	/// </summary>
	public static class TimeCounter
	{
		private static Int64 _start;

		/// <summary>
		/// Start time counting
		/// </summary>
		public static void Start()
		{
			_start = Stopwatch.GetTimestamp();
		}

		public static Int64 GetStartValue()
		{
			return Stopwatch.GetTimestamp();
		}

		/// <summary>
		/// Finish time counting
		/// </summary>
		/// <returns>time in seconds elapsed from Start till Finish	</returns>
		public static double Finish()
		{
			return Finish(_start);
		}

		public static double Finish(Int64 start)
		{
			Int64 finish = Stopwatch.GetTimestamp();
			return (finish - start) / (double)Stopwatch.Frequency;
		}

	}
}
