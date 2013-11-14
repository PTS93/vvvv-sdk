#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;

using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "AppendChild", Category = "Xml", Help = "Reads a File as XElement", Tags = "xml", Author = "woei")]
	#endregion PluginInfo
	public class XElementAppendChildNode : IPluginEvaluate
	{
		#pragma warning disable 169, 649
		#region fields & pins
		[Input("Element")]
		IDiffSpread<XElement> FInput;

		[Input("Child")]
		IDiffSpread<ISpread<XElement>> FChild;
		
		[Output("Element")]
		ISpread<XElement> FOutput;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins
		#pragma warning restore

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			if (FInput.IsChanged || FChild.IsChanged)
			{
				FOutput.SliceCount = FInput.SliceCount;
				for (int i=0; i<FInput.SliceCount; i++)
				{
					FOutput[i] = new XElement(FInput[i]);
					for (int j=0; j<FChild[i].SliceCount; j++)
						FOutput[i].Add(FChild[i][j]);
				}
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Cons", Category = "XElement", Help = "", Tags = "xml", Author = "woei")]
	#endregion PluginInfo
	public class ConsXElementNode : IPluginEvaluate
	{
		#pragma warning disable 169, 649
		#region fields & pins
		[Input("Element", IsPinGroup = true)]
		IDiffSpread<ISpread<XElement>> FInput;
		
		[Output("Element")]
		ISpread<ISpread<XElement>> FOutput;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins
		#pragma warning restore

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			if (FInput.IsChanged)
				FOutput.AssignFrom(FInput);

		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Queue", Category = "XElement", Help = "", Tags = "xml", Author = "woei")]
	#endregion PluginInfo
	public class QueueXElementNode : IPluginEvaluate
	{
		#pragma warning disable 169, 649
		#region fields & pins
		[Input("Input")]
		ISpread<XElement> FInput;
		
		[Input("Insert", IsSingle = true)]
		ISpread<bool> FDoInsert;
		
		[Input("Frame Count", IsSingle = true, MinValue = -1, DefaultValue = 1)]
		ISpread<int> FFrameCount;

        [Input("Reset", IsSingle = true, IsBang = true)]
        ISpread<bool> FReset;

		[Output("Output")]
		ISpread<ISpread<XElement>> FOutput;

		List<ISpread<XElement>> FBuffer = new List<ISpread<XElement>>();
		#endregion fields & pins
		#pragma warning restore
		
		public void Evaluate(int SpreadMax)
		{
			//return null if one of the control inputs is null
            if(FDoInsert.IsAnyEmpty(FFrameCount, FReset))
            {
            	FOutput.SliceCount = 0;
            	return;
            }
			
            if (FReset[0])
                FBuffer.Clear();

        	if (FDoInsert[0])
        		FBuffer.Insert(0, FInput.Clone() as ISpread<XElement>);
			
        	var frameCount = FFrameCount[0];
        	if (frameCount >= 0 && FBuffer.Count > frameCount)
        		FBuffer.RemoveRange(frameCount, FBuffer.Count - frameCount);
			
			FOutput.AssignFrom(FBuffer);
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Change", Category = "XElement", Help = "", Tags = "xml", Author = "woei")]
	#endregion PluginInfo
	public class ChangeXElementNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#pragma warning disable 169, 649
		#region fields & pins
		[Input("Input")]
		IDiffSpread<XElement> FInput;
		
		[Output("OnChange")]
		ISpread<bool> FOutput;

		[Import()]
		ILogger FLogger;
		
		private Spread<XElement> FBuffer;
		private bool wasChanged;
		#endregion fields & pins
		#pragma warning restore
		
		public void OnImportsSatisfied()
		{
			FBuffer = new Spread<XElement>(0);
			FBuffer.AssignFrom(FInput);
			wasChanged = false;
		}

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			if (wasChanged)
			{
				FOutput.SliceCount = 0;
				FOutput.SliceCount = FInput.SliceCount;
				wasChanged = false;
			}
			if (FInput.IsChanged)
			{
				FOutput.SliceCount = FInput.SliceCount;
				for (int i=0; i<FInput.SliceCount; i++)
				{
					if (i < FBuffer.SliceCount)
						FOutput[i] = FBuffer[i]!=FInput[i];
					else
						FOutput[i] = true;
				}
				FBuffer.AssignFrom(FInput);
				wasChanged = true;
			}

		}
	}
}
