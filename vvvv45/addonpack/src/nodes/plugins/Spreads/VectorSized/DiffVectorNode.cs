#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Differential", Category = "Spreads", Version = "Vector", Help = "Differential (Spreads) with vector size", Author = "woei")]
	#endregion PluginInfo
	public class DifferentialVectorNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649
		[Input("Input")]
		IInStream<double> FInput;

		[Input("Vector Size", MinValue = 1, DefaultValue = 1, IsSingle = true)]
		IInStream<int> FVec;
		
		[Input("Bin Size", DefaultValue = -1)]
		IInStream<int> FBin;
		
		[Output("Output")]
		IOutStream<double> FOutput;
		
		[Output("Output Bin Size")]
		IOutStream<int> FOutBin;
		
		[Output("Offset")]
		IOutStream<double> FOffset;
		#pragma warning restore
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FVec.Length>0)
			{
				int vecSize = Math.Max(1,FVec.GetReader().Read());
				VecBinSpread<double> spread = new VecBinSpread<double>(FInput,vecSize,FBin);
				
				FOutput.Length = spread.ItemCount-(spread.Count*vecSize);
				FOutBin.Length = spread.Count;
				FOffset.Length = spread.Count * vecSize;
				using (var offWriter = FOffset.GetWriter())
				using (var binWriter = FOutBin.GetWriter())
				using (var dataWriter = FOutput.GetWriter())
				{
					int incr = 0;
					for (int b = 0; b < spread.Count; b++)
					{
						for (int v = 0; v < vecSize; v++)
						{
							dataWriter.Position = incr+v;
							double[] column = spread.GetBinColumn(b,v).ToArray();
							for (int s=0; s<column.Length-1;s++)
							{
								dataWriter.Write(column[s+1]-column[s],vecSize);
							}
						}
						incr+=spread[b].Length-vecSize;
						binWriter.Write((spread[b].Length/vecSize)-1,1);
						
						offWriter.Write(spread.GetBinRow(b,0).ToArray(),0,vecSize);
					}
				}
			}
			else
				FOutput.Length = FOutBin.Length = FOffset.Length;
		}
	}
}
