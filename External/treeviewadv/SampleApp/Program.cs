using System;
using System.Windows.Forms;
using Aga.Controls;

namespace SampleApp
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
			Console.WriteLine(PerformanceAnalyzer.GenerateReport("OnPaint"));
		}
	}
}