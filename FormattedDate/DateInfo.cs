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
	[PluginInfo(Name = "Info",
	            Category = "Astronomy",
	            Version = "Date",
	            Author = "phelgma",
	            Help = "Infos about a given Date",
	            Tags = "Leap")]
	#endregion PluginInfo
	public class DateSplit : IPluginEvaluate
	{
		#region fields & pins
		
		[Input("Input")]
		IDiffSpread<DateTime> FInput;
		
		[Output("Leap Year")]
		ISpread<bool> FLeapOut;
		
		[Output("Day of Week")]
		ISpread<string> FDayOfWeekOut;
		
		[Output("Day of Year")]
		ISpread<int> FDayOfYearOut;
		
		[Output("Daylight Saving Time")]
		ISpread<bool> FDayLightOut;
		
		
		


		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

			if(FInput.IsChanged)
			{
				FLeapOut.SliceCount = FDayOfWeekOut.SliceCount = FDayOfYearOut.SliceCount = FDayLightOut.SliceCount = SpreadMax;

				for (int i = 0; i < SpreadMax; i++)
				{
					FLeapOut[i] = DateTime.IsLeapYear(FInput[i].Year);
					FDayOfWeekOut[i] = FInput[i].DayOfWeek.ToString();
					FDayOfYearOut[i] = FInput[i].DayOfYear;
					FDayLightOut[i] = FInput[i].IsDaylightSavingTime();
				}
			}
		}
	}
}
