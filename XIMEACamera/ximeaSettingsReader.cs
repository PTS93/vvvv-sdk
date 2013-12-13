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
    [PluginInfo(Name = "XIMEA Settings Reader", Category = "Devices", Version = "1.0", Help = "Read settings of a XIMEA camera", Tags = "XIMEA, Camera, XI")]
    #endregion PluginInfo

    public class ximeaSettingsReader : IPluginEvaluate
    {
        #region fields & pins


        #region allAPIParams
        //private string FDeviceName; // Get
        //private string FDeviceType; // Get
        //private string FDeviceInstancePath; // Get
        //private int FDeviceSN; // Get
        //private int FDebugLevel; // Get, Set
        
        //private int FAutoBandwidthCalculation; // Get, Set
        
        //private int FExposure; // Get, Set
        //private int FExposureMIN; // Get
        //private int FExposureMAX; // Get
        
        //private float FGain;  // Get, Set
        //private float FGainMIN;  // Get
        //private float FGainMAX;  // Get
        
        //private int FDopwnsampling; // Get, Set
        //private int FDopwnsamplingMIN; // Get
        //private int FDopwnsamplingMAX; // Get
        //private int FDownsamplingType; // Get, Set, Class DOWNSAMPLING_TYPE
        
        //private int FImageDataFormat; // Get, Set Class IMG_FORMAT
        //private int FSensorDataBitDepth; // Get, Set
        //private int FOutputDataBitDepth; // Get, Set
        //private int FImageIsColor; // Get
        //private int FColorFilterArray; // Get
        //private float FFramerate; // Get
        //private float FFramerateMIN; // Get
        //private float FFramerateMAX; // Get
        //private int FAvailableBandwidth; // Get
        //private int FAvailableBandwidthMIN; // Get
        //private int FAvailableBandwidthMAX; // Get
        //private int FLimitBandwidth; // Get, Set
        //private int FLimitBandwidthMIN; // Get
        //private int FLimitBandwidthMAX; // Get
        //private float FBufferPolicy; // Get(float), Set(int)
        //private int FOffset_X; // Get, Set / must be even
        //private int FOffset_X_MIN; // Get
        //private int FOffset_X_MAX; // Get
        //private int FOffset_Y; // Get, Set / must be even
        //private int FOffset_Y_MIN; // Get
        //private int FOffset_Y_MAX; // Get
        //private int FWidth; // Get, Set / must be divisible by four
        //private int FWidthMIN; // Get
        //private int FWidthMAX; // Get
        //private int FHeight; // Get, Set / must be even
        //private int FHeightMIN; // Get
        //private int FHeightMAX; // Get

        //private float FWB_KR; // Get, Set / default 1
        //private float FWB_KG; // Get, Set / default 1
        //private float FWB_KB; // Get, Set / default 1
        //private int FManualWB; // Set / default -
        //private int FAutoWB; // Get, Set / default 1
        //private float FGammaY; // Get, Set / default 1.0  Luminosity Gamma
        //private float FGammaC; // Get, Set / default 0  Chromaticity Gamma
        //private float FSharpness; // Get, Set default 0
        //private int FAEAG; // Get, Set / automatic exposure gain / default 0 (disabled)
        //private float FExposurePriority; // 0,5 - exposure 50%, gain 50% / default 0.8
        //private int FAEMaxLimit; // Get, Set default 1000000 ls
        //private float FAGMaxLimit; // Get, Set
        
        //private int FAEAGLevel; // Get, Set / default 400
        //private int FBPC; // Get, Set / default 0 (disabled)
        //private int FIsCooled; // Get / returns 1 if supported
        //private int FCooling; // Get, Set / default 0
        //private float FTargetTemp; // Get, Set / default 20
        //private float FChipTemp; // Get, Set default -
        //private float FHouseTemp; // Get, Set default -
        //private int FHDR; // Get, Set default 0 (disabled) alpha dev state
        //private string FAPIVersion; // Get
        //private string FDriverVersion; // Get
        #endregion

        private xiCam FCam = null;

        private List<string> FParameterNamesSingleList;
        private List<string> FParameterNamesTriplesList;
        private List<string> FParameterValuesSingleList;
        private List<float> FParameterValuesTripleList;

        private List<string> FImageDataformats;
        private List<string> FDSTypes;

        [Input("xiCam")]
        IDiffSpread<xiCam> FInXiCam;

        [Input("Update")]
        IDiffSpread<bool> FInUpdate;

        // List Outputs
        [Output("Single Parameter Names")]
        ISpread<string> FOutParameterNamesSingle;

        [Output("Single Parameter Values")]
        ISpread<string> FOutParameterValuesSingle;

        [Output("Triple Parameter Names")]
        ISpread<string> FOutParameterNamesTriple;

        [Output("Triple Parameter Values")]
        ISpread<float> FOutParameterValuesTriple;




        [Import()]
        ILogger FLogger;

        #endregion


        public ximeaSettingsReader()
        {
            FParameterNamesSingleList = new List<string>();
            FParameterValuesSingleList = new List<string>();

            FParameterValuesTripleList = new List<float>();
            FParameterNamesTriplesList = new List<string>();

            FImageDataformats = new List<string>();
            FDSTypes = new List<string>();

            FParameterNamesSingleList.Add("device name");
            FParameterNamesSingleList.Add("device type");
            FParameterNamesSingleList.Add("device serial");
            FParameterNamesSingleList.Add("api version");
            FParameterNamesSingleList.Add("driver version");

            FParameterNamesSingleList.Add("image data format");
            FParameterNamesSingleList.Add("downsampling type");
            FParameterNamesSingleList.Add("sensordata bitdepth");
            FParameterNamesSingleList.Add("outputdata bitdepth");
            FParameterNamesSingleList.Add("auto white balance");

            FParameterNamesTriplesList.Add("width min");
            FParameterNamesTriplesList.Add("width max");
            FParameterNamesTriplesList.Add("width current");
            FParameterNamesTriplesList.Add("height min");
            FParameterNamesTriplesList.Add("height max");
            FParameterNamesTriplesList.Add("height current");
            FParameterNamesTriplesList.Add("offset X min");
            FParameterNamesTriplesList.Add("offset x max");
            FParameterNamesTriplesList.Add("offset X current");
            FParameterNamesTriplesList.Add("offset Y min");
            FParameterNamesTriplesList.Add("offset Y max");
            FParameterNamesTriplesList.Add("offset Y current");
            FParameterNamesTriplesList.Add("exposure min");
            FParameterNamesTriplesList.Add("exposure max");
            FParameterNamesTriplesList.Add("exposure current");
            FParameterNamesTriplesList.Add("gain min");
            FParameterNamesTriplesList.Add("gain max");
            FParameterNamesTriplesList.Add("gain current");
            FParameterNamesTriplesList.Add("framerate min");
            FParameterNamesTriplesList.Add("framerate max");
            FParameterNamesTriplesList.Add("framerate current");
            FParameterNamesTriplesList.Add("downsampling min");
            FParameterNamesTriplesList.Add("downsampling max");
            FParameterNamesTriplesList.Add("downsampling current");
            FParameterNamesTriplesList.Add("limit bandwith min");
            FParameterNamesTriplesList.Add("limit bandwith max");
            FParameterNamesTriplesList.Add("limit bandwith current");
            FParameterNamesTriplesList.Add("avail bandwidth min");
            FParameterNamesTriplesList.Add("avail bandwidth max");
            FParameterNamesTriplesList.Add("avail bandwidth current");

            FImageDataformats.Add("XI_MONO8");
            FImageDataformats.Add("XI_MONO16");
            FImageDataformats.Add("XI_RGB24");
            FImageDataformats.Add("XI_RGB32");
            FImageDataformats.Add("XI_RGB_PLANAR");
            FImageDataformats.Add("XI_RAW8");
            FImageDataformats.Add("XI_RAW16");

            FDSTypes.Add("Binning (pixels interpoled - better image)");
            FDSTypes.Add("Skipping (pixels skipped - higher framerate)");


        }

        public void Evaluate(int spreadMax)
        {
            if (FInXiCam.IsChanged)
            {
                if (FInXiCam != null)
                {
                    SetSliceCounts(1);
                    // begin read settings
                    try
                    {
                        FCam = FInXiCam[0];
                        ReadSettingsToOutputs(FCam);


                        // copy list content to outputs

                        // singles
                        FOutParameterNamesSingle.SliceCount = FParameterNamesSingleList.Count;
                        for (int i = 0; i < FParameterNamesSingleList.Count; i++)
                        {
                            FOutParameterNamesSingle[i] = FParameterNamesSingleList.ElementAt<string>(i);
                        }
                        
                        FOutParameterValuesSingle.SliceCount = FParameterValuesSingleList.Count;
                        for (int j = 0; j < FParameterNamesSingleList.Count; j++)
                        {
                            FOutParameterValuesSingle[j] = FParameterValuesSingleList.ElementAt<string>(j);
                        }

                        // triples
                        FOutParameterNamesTriple.SliceCount = FParameterNamesTriplesList.Count;
                        for (int k = 0; k < FParameterNamesTriplesList.Count; k++)
                        {
                            FOutParameterNamesTriple[k] = FParameterNamesTriplesList.ElementAt<string>(k);
                        }

                        FOutParameterValuesTriple.SliceCount = FParameterValuesTripleList.Count;
                        for (int m = 0; m < FParameterValuesTripleList.Count; m++)
                        {
                            FOutParameterValuesTriple[m] = FParameterValuesTripleList.ElementAt<float>(m); ;
                        }

                    }
                    catch (Exception e)
                    {
                        FCam = null;
                        SetSliceCounts(0);
                    }
                }
                else
                {
                    // if cam null, set all slicecounts to zero / delete params at outputs
                    SetSliceCounts(0);
                    FCam = null;
                }

            }
            if (FCam != null && FInUpdate.IsChanged && FInUpdate[0])
                ReadSettingsToOutputs(FCam);
                
        }

        private void ReadSettingsToOutputs(xiCam cam) 
        {

            // TRIPLES

            #region triples

            FParameterValuesTripleList.Clear();

            //FOutWidth
            float widthMin;
            float widthMax;
            float width;
            cam.GetParam(PRM.WIDTH, out width);
            cam.GetParam(PRM.WIDTH_MIN, out widthMin);
            cam.GetParam(PRM.WIDTH_MAX, out widthMax);
            FParameterValuesTripleList.Add(widthMin);
            FParameterValuesTripleList.Add(widthMax);
            FParameterValuesTripleList.Add(width);
            

            //FOutHeight
            float heightMin;
            float heightMax;
            float height;
            cam.GetParam(PRM.HEIGHT, out height);
            cam.GetParam(PRM.HEIGHT_MIN, out heightMin);
            cam.GetParam(PRM.HEIGHT_MAX, out heightMax);
            FParameterValuesTripleList.Add(heightMin);
            FParameterValuesTripleList.Add(heightMax);
            FParameterValuesTripleList.Add(height);

            //FOutOffsetX
            float osX;
            float osXMin;
            float osXMax;
            cam.GetParam(PRM.OFFSET_X, out osX);
            cam.GetParam(PRM.OFFSET_X_MIN, out osXMin);
            cam.GetParam(PRM.OFFSET_X_MAX, out osXMax);
            FParameterValuesTripleList.Add(osXMin);
            FParameterValuesTripleList.Add(osXMax);
            FParameterValuesTripleList.Add(osX);

            //FOutOffsetY
            float osY;
            float osYMin;
            float osYMax;
            cam.GetParam(PRM.OFFSET_Y, out osY);
            cam.GetParam(PRM.OFFSET_Y_MIN, out osYMin);
            cam.GetParam(PRM.OFFSET_Y_MAX, out osYMax);
            FParameterValuesTripleList.Add(osYMin);
            FParameterValuesTripleList.Add(osYMax);
            FParameterValuesTripleList.Add(osY);

            //FOutExposure
            float exp;
            float expMin;
            float expMax;
            cam.GetParam(PRM.EXPOSURE, out exp);
            cam.GetParam(PRM.EXPOSURE_MIN, out expMin);
            cam.GetParam(PRM.EXPOSURE_MAX, out expMax);
            FParameterValuesTripleList.Add(expMin);
            FParameterValuesTripleList.Add(expMax);
            FParameterValuesTripleList.Add(exp);

            //FOutGain
            float gain;
            float gainMin;
            float gainMax;
            cam.GetParam(PRM.GAIN, out gain);
            cam.GetParam(PRM.GAIN_MIN, out gainMin);
            cam.GetParam(PRM.GAIN_MAX, out gainMax);
            FParameterValuesTripleList.Add(gainMin);
            FParameterValuesTripleList.Add(gainMax);
            FParameterValuesTripleList.Add(gain);

            //FOutFramerate
            float fr;
            float frMin;
            float frMax;
            cam.GetParam(PRM.FRAMERATE, out fr);
            cam.GetParam(PRM.FRAMERATE_MIN, out frMin);
            cam.GetParam(PRM.FRAMERATE_MAX, out frMax);
            FParameterValuesTripleList.Add(frMin);
            FParameterValuesTripleList.Add(frMax);
            FParameterValuesTripleList.Add(fr);

            //FOutDownsampling
            float dS;
            float dSMin;
            float dSMax;
            cam.GetParam(PRM.DOWNSAMPLING, out dS);
            cam.GetParam(PRM.DOWNSAMPLING_MIN, out dSMin);
            cam.GetParam(PRM.DOWNSAMPLING_MAX, out dSMax);
            FParameterValuesTripleList.Add(dSMin);
            FParameterValuesTripleList.Add(dSMax);
            FParameterValuesTripleList.Add(dS);

            //FOutLimitBandwidth
            float lmtBW;
            float lmtBWMin;
            float lmtBWMax;
            cam.GetParam(PRM.LIMIT_BANDWIDTH, out lmtBW);
            cam.GetParam(PRM.LIMIT_BANDWIDTH_MIN, out lmtBWMin);
            cam.GetParam(PRM.LIMIT_BANDWIDTH_MAX, out lmtBWMax);
            FParameterValuesTripleList.Add(lmtBWMin);
            FParameterValuesTripleList.Add(lmtBWMax);
            FParameterValuesTripleList.Add(lmtBW);

            //FOutAvailableBandwidth
            float avlbBaWi;
            float avlbBaWiMin;
            float avlbBaWiMax;
            cam.GetParam(PRM.AVAILABLE_BANDWIDTH, out avlbBaWi);
            cam.GetParam(PRM.AVAILABLE_BANDWIDTH_MIN, out avlbBaWiMin);
            cam.GetParam(PRM.AVAILABLE_BANDWIDTH_MAX, out avlbBaWiMax);
            FParameterValuesTripleList.Add(avlbBaWiMin);
            FParameterValuesTripleList.Add(avlbBaWiMax);
            FParameterValuesTripleList.Add(avlbBaWi);

            #endregion triples

            // SINGLES

            #region singles

            FParameterValuesSingleList.Clear();

            //FOutDeviceName
            string devName;
            cam.GetParam(PRM.DEVICE_NAME, out devName);
            FParameterValuesSingleList.Add(devName);

            //FOutDeviceType
            string dt;
            cam.GetParam(PRM.DEVICE_TYPE, out dt);
            FParameterValuesSingleList.Add(dt);

            //FOutDeviceSN
            string devSN;
            cam.GetParam(PRM.DEVICE_SN, out devSN);
            FParameterValuesSingleList.Add(devSN);

            // api version
            string apiV;
            cam.GetParam(PRM.API_VERSION, out apiV);
            FParameterValuesSingleList.Add(apiV);

            // driver version
            string driverV;
            cam.GetParam(PRM.DRV_VERSION, out driverV);
            FParameterValuesSingleList.Add(driverV);

            //FOutUImageDataFormat
            int imgFormat;
            cam.GetParam(PRM.IMAGE_DATA_FORMAT, out imgFormat);
            string form = FImageDataformats.ElementAt<string>(imgFormat);
            FParameterValuesSingleList.Add(form);

            // FOutDownsamplingType
            int dSType;
            cam.GetParam(PRM.DOWNSAMPLING_TYPE, out dSType);
            string dsT = FDSTypes.ElementAt<string>(dSType);
            FParameterValuesSingleList.Add(dsT);

            // FOutSensorDataBitDepth
            string sensDataBD;
            cam.GetParam(PRM.SENSOR_DATA_BIT_DEPTH, out sensDataBD);
            FParameterValuesSingleList.Add(sensDataBD);

            //FOutOutputDataBitDepth
            string outDataBD;
            cam.GetParam(PRM.OUTPUT_DATA_BIT_DEPTH, out outDataBD);
            FParameterValuesSingleList.Add(outDataBD);

            // Auto WhiteBalance
            string autoWB;
            cam.GetParam(PRM.AUTO_WB, out autoWB);
            FParameterValuesSingleList.Add(autoWB);

            #endregion
        }

        private void SetSliceCounts(int count)
        {
            FOutParameterNamesSingle.SliceCount = count;
            FOutParameterNamesTriple.SliceCount = count;
            FOutParameterValuesSingle.SliceCount = count;
            FOutParameterValuesTriple.SliceCount = count;
        }
    }
}
