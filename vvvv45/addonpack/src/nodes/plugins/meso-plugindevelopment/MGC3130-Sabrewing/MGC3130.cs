#region usings
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "MGC3130",
                Category = "Devices",
                Version = "Gestic",
                Help = "Gets data from MGC3130 based boards",
                Tags = "controller",
                Author = "Timon",
                AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class MGC3130 : IPluginEvaluate
    {
        #region fields & pins


        //Input 

        [Input("Enable Position")]
        IDiffSpread<bool> FEnablePos;

        //Output
        [Output("Attached")]
        ISpread<bool> FAttached;

        [Output("Position")]
        ISpread<Vector3D> FPosition;

        [Output("Gesture")]
        ISpread<int> FGesture;

        [Output("Air Wheel")]
        ISpread<int> FAirWheel;

        [Output("Touch")]
        ISpread<bool> FTouch;


        //Logger
        [Import()]
        ILogger FLogger;

        //private Fields

        #endregion fields & piins

        #region DLLImport
        [DllImport("gestic-bridge.dll")]
        public static extern int getGesture();

        [DllImport("gestic-bridge.dll")]
        public static extern int getPos(int axis);

        [DllImport("gestic-bridge.dll")]
        public static extern bool getTouch();

        [DllImport("gestic-bridge.dll")]
        public static extern void init_device();

        [DllImport("gestic-bridge.dll")]
        public static extern void calibrate_now();

        [DllImport("gestic-bridge.dll")]
        public static extern void switch_auto_calib();

        [DllImport("gestic-bridge.dll")]
        public static extern void init_data();

        [DllImport("gestic-bridge.dll")]
        public static extern int getAirWheel();

        [DllImport("gestic-bridge.dll")]
        public static extern int isRunning();

        [DllImport("gestic-bridge.dll")]
        public static extern int hasGestic();

        /*[DllImport("gestic-bridge.dll")]
        public static extern deviation getSignalDeviation();
        //signal deviation passing not working yet
        [StructLayout(LayoutKind.Sequential)]
        public struct deviation {
            public int source;
            public float channel;
            public int lastCalib;
        };
        */

        #endregion DLLImport

        public void OnImportsSatisfied()
        {
            //start with an empty stream output
            
        }
        bool runOnce = true;
        public void Evaluate(int SpreadMax)
        {
            //FPosition.SliceCount = SpreadMax;
            
            if (runOnce == true)
            {
                init_data();
                init_device();
                runOnce = false;
            }

            if (FEnablePos[0] == true)
            {
                FPosition[0] = new Vector3D(getPos(0), getPos(0), getPos(0));
            }
            if (FEnablePos[0] == true)
            {
                FGesture[0] = getGesture();
            }
            if (FEnablePos[0] == true)
            {
                FAirWheel[0] = getAirWheel();
            }
            if (FEnablePos[0] == true)
            {
                FTouch[0] = getTouch();
            }

        }
    }
}
