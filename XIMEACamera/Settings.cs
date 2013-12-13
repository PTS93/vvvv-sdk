using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Nodes
{
    public class XimeaSettings
    {
        private string deviceName; // Get
        public string DeviceName
        {
            get { return deviceName; }
        }

        private string deviceType; // Get
        public string DeviceType
        {
            get { return deviceType; }
        }

        private string deviceInstancePath; // Get
        public string DeviceInstancePath
        {
            get { return deviceInstancePath; }
        }

        private int deviceSN; // Get
        public int DeviceSN
        {
            get { return deviceSN; }
        }
        
        private int debugLevel; // Get, Set
        public int DebugLevel
        {
            get { return debugLevel; }
            set { debugLevel = value; }
        }

        private int autoBandwidthCalculation; // Get, Set
        public int AutoBandwidthCalculation
        {
            get { return autoBandwidthCalculation; }
            set { autoBandwidthCalculation = value; }
        }

        private int exposure; // Get, Set
        public int Exposure
        {
            get { return exposure; }
            set { exposure = value; }
        }
        
        private int exposureMIN; // Get
        public int ExposureMIN
        {
            get { return exposureMIN; }
        }
        
        private int exposureMAX; // Get
        public int ExposureMAX
        {
            get { return exposureMAX; }
        }

        private float gain;  // Get, Set
        public float Gain
        {
            get { return gain; }
            set { gain = value; }
        }
        
        private float gainMIN;  // Get
        public float GainMIN
        {
            get { return gainMIN; }
        }
        
        private float gainMAX;  // Get
        public float GainMAX
        {
            get { return gainMAX; }
        }

        private int downsamplingMode; // Get, Set
        public int DownsamplingMode
        {
            get { return downsamplingMode; }
            set { downsamplingMode = value; }
        }

        private int downsamplingMIN; // Get
        public int DownsamplingMIN
        {
            get { return downsamplingMIN; }
        }

        private int downsamplingMAX; // Get
        public int DownsamplingMAX
        {
            get { return downsamplingMAX; }
        }

        private int downsamplingType; // Get, Set, Class DOWNSAMPLING_TYPE
        public int DownsamplingType
        {
            get { return downsamplingType; }
            set { downsamplingType = value; }
        }

        private int imageDataFormat; // Get, Set Class IMG_FORMAT
        public int ImageDataFormat
        {
            get { return imageDataFormat; }
            set { imageDataFormat = value; }
        }

        private int sensorDataBitDepth; // Get, Set
        public int SensorDataBitDepth
        {
            get { return sensorDataBitDepth; }
            set { sensorDataBitDepth = value; }
        }

        private int outputDataBitDepth; // Get, Set
        public int OutputDataBitDepth
        {
            get { return outputDataBitDepth; }
            set { outputDataBitDepth = value; }
        }

        private int imageIsColor; // Get
        public int ImageIsColor
        {
            get { return imageIsColor; }
        }

        private int colorFilterArray; // Get

        public int ColorFilterArray
        {
            get { return colorFilterArray; }
        }

        private float framerate; // Get, Set
        public float Framerate
        {
            get { return framerate; }
            set { framerate = value; }
        }

        private float framerateMIN; // Get
        public float FramerateMIN
        {
            get { return framerateMIN; }
        }

        private float framerateMAX; // Get
        public float FramerateMAX
        {
            get { return framerateMAX; }
        }

        private int availableBandwidth; // Get
        public int AvailableBandwidth1
        {
            get { return availableBandwidth; }
        }

        private int availableBandwidthMIN; // Get
        public int AvailableBandwidthMIN
        {
            get { return availableBandwidthMIN; }
        }

        private int availableBandwidthMAX; // Get
        public int AvailableBandwidthMAX
        {
            get { return availableBandwidthMAX; }
        }

        private int limitBandwidth; // Get, Set
        public int LimitBandwidth
        {
            get { return limitBandwidth; }
            set { limitBandwidth = value; }
        }

        private int limitBandwidthMIN; // Get
        public int LimitBandwidthMIN
        {
            get { return limitBandwidthMIN; }
        }

        private int limitBandwidthMAX; // Get
        public int LimitBandwidthMAX
        {
            get { return limitBandwidthMAX; }
        }

        private int offset_X; // Get, Set / must be even
        public int Offset_X
        {
            get { return offset_X; }
            set { offset_X = value; }
        }

        private int offset_X_MIN; // Get
        public int Offset_X_MIN
        {
            get { return offset_X_MIN; }
        }

        private int offset_X_MAX; // Get
        public int Offset_X_MAX
        {
            get { return offset_X_MAX; }
        }

        private int offset_Y; // Get, Set / must be even
        public int Offset_Y
        {
            get { return offset_Y; }
            set { offset_Y = value; }
        }

        private int offset_Y_MIN; // Get
        public int Offset_Y_MIN
        {
            get { return offset_Y_MIN; }
        }

        private int offset_Y_MAX; // Get
        public int Offset_Y_MAX
        {
            get { return offset_Y_MAX; }
        }

        private int width; // Get, Set / must be divisible by four
        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        private int widthMIN; // Get
        public int WidthMIN
        {
            get { return widthMIN; }
        }

        private int widthMAX; // Get
        public int WidthMAX
        {
            get { return widthMAX; }
        }

        private int height; // Get, Set / must be even
        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        private int heightMIN; // Get
        public int HeightMIN
        {
            get { return heightMIN; }
        }

        private int heightMAX; // Get
        public int HeightMAX
        {
            get { return heightMAX; }
        }

        private float wB_KR; // Get, Set / default 1
        public float WB_KR
        {
            get { return wB_KR; }
            set { wB_KR = value; }
        }

        private float wB_KG; // Get, Set / default 1
        public float WB_KG
        {
            get { return wB_KG; }
            set { wB_KG = value; }
        }

        private float wB_KB; // Get, Set / default 1
        public float WB_KB
        {
            get { return wB_KB; }
            set { wB_KB = value; }
        }

        private int manualWB; // Set / default -
        public int ManualWB
        {
            get { return manualWB; }
            set { manualWB = value; }
        }

        private int autoWB; // Get, Set / default 1
        public int AutoWB
        {
            get { return autoWB; }
            set { autoWB = value; }
        }

        private float gammaY; // Get, Set / default 1.0  Luminosity Gamma
        public float GammaY
        {
            get { return gammaY; }
            set { gammaY = value; }
        }

        private float gammaC; // Get, Set / default 0  Chromaticity Gamma
        public float GammaC
        {
            get { return gammaC; }
            set { gammaC = value; }
        }

        private float sharpness; // Get, Set default 0
        public float Sharpness
        {
            get { return sharpness; }
            set { sharpness = value; }
        }

        private int aEAG; // Get, Set / automatic exposure gain / default 0 (disabled)
        public int AEAG
        {
            get { return aEAG; }
            set { aEAG = value; }
        }

        private float ExposurePriority; // 0,5 - exposure 50%, gain 50% / default 0.8
        public float ExposurePriority1
        {
            get { return ExposurePriority; }
            set { ExposurePriority = value; }
        }
        
        private int AEMaxLimit; // Get, Set default 1000000 ls
        
        private float AGMaxLimit; // Get, Set
        
        private int AEAGLevel; // Get, Set / default 400
        
        private int BPC; // Get, Set / default 0 (disabled)
        
        private int IsCooled; // Get / returns 1 if supported
        
        private int Cooling; // Get, Set / default 0
        
        private float TargetTemp; // Get, Set / default 20
        
        private float ChipTemp; // Get, Set default -
        private float HouseTemp; // Get, Set default -
        private int HDR; // Get, Set default 0 (disabled) alpha dev state
        private string APIVersion; // Get
        private string DriverVersion; // Get
        private int AvailableBandwidth; // Get

        private float bufferPolicy; // Get(float), Set(int)

        public XimeaSettings()
        {
            this.width = 640;
            this.height = 480;
            this.offset_X = 4;
            this.offset_Y = 4;
            this.framerate = 30;
            this.exposure = 30000;
            this.gain = 5;
            this.downsamplingMode = 4;
            this.downsamplingType = 1;
            this.limitBandwidth = 320;
            this.imageDataFormat = 3;
        }
    }
}
