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
	[PluginInfo(Name = "AsString",
	            Category = "Astronomy",
	            Version = "Date",
	            Author = "phelgma",
	            Help = "Node to create Date Object",
	            Tags = "")]
	#endregion PluginInfo
	public class DateToString : IPluginEvaluate
	{
		#region fields & pins
		[Input("DateTime")]
		IDiffSpread<DateTime> FDateTimeIn;
		
		[Input("Format", DefaultString="F")]
		IDiffSpread<string> FFormatIn;
		
		[Input("CultureInfo", DefaultString="en-US")]
		IDiffSpread<string> FCultureIn;
		
		[Output("Output")]
		ISpread<string> FOutput;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

			if(FDateTimeIn.IsChanged || FCultureIn.IsChanged || FFormatIn.IsChanged)
			{
				FOutput.SliceCount = SpreadMax;
				
				for (int i = 0; i < SpreadMax; i++)
				{
					try
					{
						CultureInfo culture = new CultureInfo(FCultureIn[i]);
						FOutput[i] = FDateTimeIn[i].ToString(FFormatIn[i], culture);
					}catch(Exception ex)
					{
						FLogger.Log(LogType.Error, ex.Message);
					}
				}
			}
		}
	}
}
