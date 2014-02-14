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
	[PluginInfo(Name = "Compare",
	            Category = "Astronomy",
	            Version = "Date",
	            Author = "phelgma",
	            Help = "Compares the Daytome of two Dates",
	            Tags = "")]
	#endregion PluginInfo
	public class CompareDayTime : IPluginEvaluate
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
		
		[Output("Date")]
		ISpread<int> FOutput;
		
		[Output("Time of Day")]
		ISpread<int> FTimeOfDayOut;

		
		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FDate1In.IsChanged || FDate2In.IsChanged)
			{
				FOutput.SliceCount = FTimeOfDayOut.SliceCount = SpreadMax;
				
				for (int i = 0; i < SpreadMax; i++)
				{
					FOutput[i] = TimeSpan.Compare(new TimeSpan(FDate2In[i].Ticks),new TimeSpan(FDate1In[i].Ticks));
					FTimeOfDayOut[i] = TimeSpan.Compare(FDate2In[i].TimeOfDay,FDate1In[i].TimeOfDay);                                                        
				}
			}
		}
	}
}