#region usings
using System;
using System.Globalization;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Add",
	            Category = "Astronomy",
	            Version = "Date",
	            Author = "phelgma",
	            Help = "Add the given values to the given Date",
	            Tags = "")]
	#endregion PluginInfo
	public class DateAdd : IPluginEvaluate
	{
		#region fields & pins
		[Input("DateTime")]
		ISpread<DateTime> FDateTimeIn;

		[Input("Ticks")]
		ISpread<int> FTicksIn;
		
		[Input("Milliseconds")]
		ISpread<int> FMillisecondsIn;
		
		[Input("Seconds")]
		ISpread<int> FSecondsIn;
		
		[Input("Minutes")]
		ISpread<int> FMinutesIn;
		
		[Input("Hours")]
		ISpread<int> FHoursIn;
		
		[Input("Days")]
		ISpread<int> FDaysIn;
		
		[Input("Months")]
		ISpread<int> FMonthsIn;
		
		[Input("Years")]
		ISpread<int> FYearsIn;
		
		[Output("Output")]
		ISpread<DateTime> FOutput;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FTicksIn.IsChanged || FDateTimeIn.IsChanged || FMillisecondsIn.IsChanged || FSecondsIn.IsChanged || FMinutesIn.IsChanged || FHoursIn.IsChanged || FDaysIn.IsChanged || FMonthsIn.IsChanged || FYearsIn.IsChanged)
			{
				FOutput.SliceCount = SpreadMax;

				for (int i = 0; i < SpreadMax; i++)
				{
					DateTime Date = FDateTimeIn[i];
					Date = Date.AddTicks((long)FTicksIn[i]);
					Date = Date.AddMilliseconds(FMillisecondsIn[i]);
					Date = Date.AddSeconds(FSecondsIn[i]);
					Date = Date.AddMinutes(FMinutesIn[i]);
					Date = Date.AddHours(FHoursIn[i]);
					Date = Date.AddDays(FDaysIn[i]);
					Date = Date.AddMonths(FMonthsIn[i]);
					Date = Date.AddYears(FYearsIn[i]);
					FOutput[i] = Date;
				}
			}
		}
	}
}
