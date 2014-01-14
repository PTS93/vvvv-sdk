#region usings
using System;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using SCIP_library;
using System.Collections.Generic;
#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Decode",
                Category = "Devices",
                Version = "SCIP2.0",
                Help = "",
                Tags = "")]
    #endregion PluginInfo
    public class Decode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input", IsSingle=true)]
        public IDiffSpread<string> FInput;

        [Output("Distances")]
        public ISpread<int> FDistances;

        [Output("TimeStamp")]
        public ISpread<int> FTimestamp;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {

            if (FInput.IsChanged)
            {
                FTimestamp.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    try
                    {
                        List<long> distances = new List<long>();
                        long time_stamp = 0;
                        SCIP_Reader.MD(FInput[i], ref time_stamp, ref distances);
                        FDistances.SliceCount = distances.Count;
                        for (int j = 0; j < distances.Count; j++)
                        {
                            FDistances[j] = (int)distances[j];
                        }
                        FTimestamp[i] = (int)time_stamp;
                    }
                    catch (Exception ex)
                    {
                        FLogger.Log(LogType.Error, ex.Message);
                        throw;
                    }
                }
            }


        }

        public static int LongToD32Int(long d)
        {

            return Convert.ToInt32(d);
        }
    }
}
