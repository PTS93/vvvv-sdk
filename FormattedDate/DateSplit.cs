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
	[PluginInfo(Name = "Split",
	            Category = "Astronomy",
	            Version = "Date",
	            Author = "phelgma",
	            Help = "Splits the DateTime Object into Values",
	            Tags = "")]
	#endregion PluginInfo
	public class DatePlite : IPluginEvaluate
	{
		#region fields & pins
		
		[Input("Input")]
		IDiffSpread<DateTime> FInput;
		
		[Output("Ticks")]
		ISpread<int> FTicksOut;
		
		[Output("Milliseconds")]
		ISpread<int> FMillisecondsOut;
		
		[Output("Seconds")]
		ISpread<int> FSecondsOut;
		
		[Output("Minutes")]
		ISpread<int> FMinutesOut;
		
		[Output("Hours")]
		ISpread<int> FHoursOut;
		
		[Output("Days")]
		ISpread<int> FDaysOut;
		
		[Output("Months")]
		ISpread<int> FMonthsOut;
		
		[Output("Years")]
		ISpread<int> FYearsOut;
		


		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

			if(FInput.IsChanged)
			{
				FTicksOut.SliceCount =  FMillisecondsOut.SliceCount = FSecondsOut.SliceCount = FMinutesOut.SliceCount = FHoursOut.SliceCount = FDaysOut.SliceCount = FMonthsOut.SliceCount = FYearsOut.SliceCount = SpreadMax;

				for (int i = 0; i < SpreadMax; i++)
				{
					FTicksOut[i] = (int) FInput[i].Ticks;
					FMillisecondsOut[i] = FInput[i].Millisecond;
					FSecondsOut[i] = FInput[i].Second;
					FMinutesOut[i] = FInput[i].Minute;
					FHoursOut[i] = FInput[i].Hour;
					FDaysOut[i] = FInput[i].Day;
					FMonthsOut[i] = FInput[i].Month;
					FYearsOut[i] = FInput[i].Year;
				}
			}
		}
	}
}
