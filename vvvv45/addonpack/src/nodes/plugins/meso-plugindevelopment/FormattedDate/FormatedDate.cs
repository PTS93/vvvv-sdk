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
	[PluginInfo(Name = "Date",
	            Category = "Astronomy",
	            Version = "",
	            Author = "phelgma",
	            Help = "Spits out the current Date",
	            Tags = "Time, Now")]
	#endregion PluginInfo
	public class FormatedDate : IPluginEvaluate
	{
		#region fields & pins
		[Input("Formate", DefaultString="F")]
		IDiffSpread<string> FFormatIn;
		
		[Input("CultureInfo", DefaultString="en-US")]
		IDiffSpread<string> FCultureIn;
		
		[Input("Update")]
		IDiffSpread<bool> FUpdateIn;
		
		[Output("DateTime")]
		ISpread<DateTime> FOutput;
		
		[Output("Current Date")]
		ISpread<string> FCurrentDate;
		
		[Output("UTC")]
		ISpread<string> FUTC;
		
		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = FCurrentDate.SliceCount = FUTC.SliceCount = SpreadMax;

			if(FUpdateIn.IsChanged || FCultureIn.IsChanged || FFormatIn.IsChanged)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					try
					{
						DateTime Date = DateTime.Now;
						CultureInfo culture = new CultureInfo(FCultureIn[i]);
						FOutput[i] = Date;
						FCurrentDate[i] = Date.ToString(FFormatIn[i], culture);
						FUTC[i] = Date.ToUniversalTime().ToString(FFormatIn[i], culture);
					}catch(Exception ex)
					{
						FLogger.Log(LogType.Error, ex.Message);
					}
				}
			}
		}
	}
}
