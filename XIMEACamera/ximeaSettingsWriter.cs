using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using xiApi.NET;

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "XIMEA Settings", Category = "Devices", Version = "1.0", Help = "Edit settings of a XIMEA camera", Tags = "XIMEA, camera, XI")]
    #endregion PluginInfo

    public class ximeaSettingsWriter : IPluginEvaluate
    {
        #region fields & pins

        [Input("Width", DefaultValue = 640)]
        IDiffSpread<int> FInWidth;

        [Input("Height", DefaultValue = 480)]
        IDiffSpread<int> FInHeight;

        [Input("Offset X", DefaultValue = 4)]
        IDiffSpread<int> FInOffsetX;

        [Input("Offset Y", DefaultValue = 4)]
        IDiffSpread<int> FInOffsetY;

        [Input("Framerate", DefaultValue = 30)]
        IDiffSpread<float> FInFrameRate;

        [Input("Exposure", DefaultValue = 30000)]
        IDiffSpread<int> FInExposure;

        [Input("Gain", DefaultValue = 5)]
        IDiffSpread<float> FInGain;

        [Input("Downsampling Mode", DefaultValue = 4)]
        IDiffSpread<int> FInDSMode;

        [Input("DS Type (Binning / Skipping)", DefaultValue = 1, MinValue = 0, MaxValue = 1)]
        IDiffSpread<int> FInDSType;

        [Input("Imaga Format", DefaultValue = 3)]
        IDiffSpread<int> FInIMGFormat;

        [Input("Limit Bandwidth", DefaultValue = 200)]
        IDiffSpread<int> FInLimitBandwidth;

        [Output("Ximea Settings")]
        ISpread<XimeaSettings> FOutSettings;

        [Import()]
        ILogger FLogger;

        private bool FUpdate = false;

        #endregion

        /* currently supported settings:
         * Width
         * Height
         * Offset X
         * Offset Y
         * Framerate
         * Exposure
         * Gain
         * Downsampling Mode
         * Downsampling Type (Binning / Skipping)
         * Limit Bandwidth
         * */

        public ximeaSettingsWriter()
        {

        }

        public void Evaluate(int spreadMax)
        {
            FUpdate = false;
            FOutSettings.SliceCount = spreadMax;

            if (FInWidth.IsChanged || FInHeight.IsChanged || FInOffsetX.IsChanged || FInOffsetY.IsChanged || FInFrameRate.IsChanged || FInExposure.IsChanged || FInGain.IsChanged || FInDSMode.IsChanged || FInDSType.IsChanged || FInLimitBandwidth.IsChanged || FInIMGFormat.IsChanged) 
            {
                FUpdate = true;
            }

            for (int i = 0; i < spreadMax; i++)
            {
                if (FUpdate)
                {
                    XimeaSettings xs = new XimeaSettings();
                    xs.Width = FInWidth[i];
                    xs.Height = FInHeight[i];
                    xs.Offset_X = FInOffsetX[i];
                    xs.Offset_Y = FInOffsetY[i];
                    xs.Framerate = FInFrameRate[i];
                    xs.Exposure = FInExposure[i];
                    xs.Gain = FInGain[i];
                    xs.DownsamplingMode = FInDSMode[i];
                    xs.DownsamplingType = FInDSType[i];
                    xs.LimitBandwidth = FInLimitBandwidth[i];
                    xs.ImageDataFormat = FInIMGFormat[i];
                    FOutSettings[i] = xs;
                }
            }
            FUpdate = false;
        }
    }
}
