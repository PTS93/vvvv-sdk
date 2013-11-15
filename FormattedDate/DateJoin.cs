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
	[PluginInfo(Name = "Join",
	            Category = "Astronomy",
	            Version = "Date",
	            Author = "phelgma",
	            Help = "Create a Date by the given values",
	            Tags = "")]
	#endregion PluginInfo
	public class DateCompare : IPluginEvaluate
	{
		#region fields & pins
		[Input("Milliseconds", MinValue=0, MaxValue=999)]
		ISpread<int> FMillisecondsIn;
		
		[Input("Seconds", MinValue=0,MaxValue=59)]
		ISpread<int> FSecondsIn;
		
		[Input("Minutes", MinValue=0, MaxValue=59)]
		ISpread<int> FMinutesIn;
		
		[Input("Hours", MinValue=0,MaxValue=23)]
		ISpread<int> FHoursIn;
		
		[Input("Days", DefaultValue=1, MinValue=1, MaxValue=31)]
		ISpread<int> FDaysIn;
		
		[Input("Months", DefaultValue=1, MinValue=1, MaxValue=12)]
		ISpread<int> FMonthsIn;
		
		[Input("Years", DefaultValue=1, MinValue=1, MaxValue=double.MaxValue)]
		ISpread<int> FYearsIn;
		
		[Input("Type")]
		ISpread<DateTimeKind> FDateTimeKindIn;
		
		[Output("Output")]
		ISpread<DateTime> FOutput;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;

			for (int i = 0; i < SpreadMax; i++)
			{
				try
				{
					DateTime MyDate = new DateTime(FYearsIn[i],FMonthsIn[i],FDaysIn[i],FHoursIn[i],FMinutesIn[i],FSecondsIn[i],FMillisecondsIn[i],FDateTimeKindIn[i]);
					FOutput[i] = MyDate;
				}catch(Exception ex)
				{
					FLogger.Log(LogType.Error, ex.Message);
				}
			}
			

			
		}
	}
}
