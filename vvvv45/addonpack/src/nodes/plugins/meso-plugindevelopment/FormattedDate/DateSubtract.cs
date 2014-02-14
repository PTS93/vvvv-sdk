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
	[PluginInfo(Name = "Subtract",
	            Category = "Astronomy",
	            Version = "Date",
	            Author = "phelgma",
	            Help = "Calculate the differnce between two Dates",
	            Tags = "")]
	#endregion PluginInfo
	public class DateSubtract : IPluginEvaluate
	{
		public enum DateOutputUnit
		{
			Milliseconds,
			Seconds,
			Minutes,
			Hours,
			Days,
		}
		
		#region fields & pins
		[Input("Date 1")]
		ISpread<DateTime> FDate1In;
		
		[Input("Date 2")]
		ISpread<DateTime> FDate2In;
		
		[Input("Output Unit")]
		ISpread<DateOutputUnit> FDateOutputUnitIn;
		
		[Input("Switch Input")]
		ISpread<bool> FSwitchIn;
		
		[Output("Date")]
		ISpread<double> FOutput;
		
		[Output("Time of Day")]
		ISpread<double> FTimeOFDayOut;

		
		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FDate1In.IsChanged || FDate2In.IsChanged || FDateOutputUnitIn.IsChanged || FSwitchIn.IsChanged)
			{
				FOutput.SliceCount = FTimeOFDayOut.SliceCount = SpreadMax;
				
				for (int i = 0; i < SpreadMax; i++)
				{
					TimeSpan DiffernceTotal;
					if(!FSwitchIn[i])
						DiffernceTotal = FDate1In[i].Subtract(FDate2In[i]);
					else
						DiffernceTotal = ((FDate1In[i].Subtract(FDate2In[i])).Negate());
					
					TimeSpan DiffernceDay;
					if(!FSwitchIn[i])
						DiffernceDay = FDate1In[i].TimeOfDay.Subtract(FDate2In[i].TimeOfDay);
					else
						DiffernceDay = FDate1In[i].TimeOfDay.Subtract(FDate2In[i].TimeOfDay).Negate();
					
					
					switch (FDateOutputUnitIn[i])
					{
						case DateSubtract.DateOutputUnit.Milliseconds:
							FOutput[i] = DiffernceTotal.TotalMilliseconds;
							FTimeOFDayOut[i] = DiffernceDay.TotalMilliseconds;
							break;
						case DateSubtract.DateOutputUnit.Seconds:
							FOutput[i] = DiffernceTotal.TotalSeconds;
							FTimeOFDayOut[i] = DiffernceDay.TotalSeconds;
							break;
						case DateSubtract.DateOutputUnit.Minutes:
							FOutput[i] = DiffernceTotal.TotalMinutes;
							FTimeOFDayOut[i] = DiffernceDay.TotalMinutes;
							break;
						case DateSubtract.DateOutputUnit.Hours:
							FOutput[i] = DiffernceTotal.TotalHours;
							FTimeOFDayOut[i] = DiffernceDay.TotalHours;
							break;
						case DateSubtract.DateOutputUnit.Days:
							FOutput[i] = DiffernceTotal.TotalDays;
							FTimeOFDayOut[i] = DiffernceDay.TotalDays;
							break;
						default:
							throw new Exception("Invalid value for DateOutputUnit");
					}
					
					
				}
			}
		}
	}
}