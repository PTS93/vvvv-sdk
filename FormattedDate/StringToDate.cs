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
	[PluginInfo(Name = "AsDate",
	            Category = "String",
	            Version = "",
	            Author = "phelgma",
	            Help = "Converts a String to DateTime Object",
	            Tags = "Convert")]
	#endregion PluginInfo
	public class StringToDate : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		IDiffSpread<string> FInputIn;
	
		[Input("CultureInfo", DefaultString="en-US")]
		IDiffSpread<string> FCultureIn;
		
		[Input("DateTime Styles")]
		IDiffSpread<DateTimeStyles> FDateTimeStyles;
		
		[Input("Update", IsBang = true)]
		IDiffSpread<bool> FUpdate;
		
		[Output("DateTime")]
		ISpread<DateTime> FOutput;
		

		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
			
			if(FInputIn.IsChanged || FCultureIn.IsChanged || FDateTimeStyles.IsChanged || FUpdate.IsChanged)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					try
					{
						IFormatProvider Provider = new CultureInfo(FCultureIn[i]);
						FOutput[i] = DateTime.Parse(FInputIn[i],Provider,FDateTimeStyles[i]);
					}catch(Exception ex)
					{
						FLogger.Log(LogType.Error, ex.Message);
					}
				}
			}
		}
	}
}
