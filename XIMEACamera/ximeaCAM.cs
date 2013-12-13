#region usings
using System;
using System.Drawing;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;
using SlimDX.Direct3D9;
using xiApi.NET;
using VVVV.Core.Logging;
using VertexType = VVVV.Utils.SlimDX.TexturedVertex;

#endregion usings


namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "XIMEA Camera", Category = "Devices", Version = "1.0", Help = "XIMEA camera", Tags = "")]
	#endregion PluginInfo

    public class ximeaCAM : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
        //little helper class used to store information for each
        //texture resource
        public class Info
        {
            public int Slice;
            public int Width;
            public int Height;
            public double WaveCount;
        }

		#region fields & pins

        private XimeaSettings xs = new XimeaSettings();

        [Input("Cam Serial", IsSingle = true)]
        IDiffSpread<string> FInCamSerial;

        [Input("Set Params", IsSingle=true, IsBang=true, DefaultBoolean=false)]
        IDiffSpread<bool> FInSetParams;

        [Input("Settings")]
        IDiffSpread<XimeaSettings> FInSettings;

        [Input("Enable Image Processing", IsSingle = true, IsToggle = true)]
        IDiffSpread<bool> FInEnableProcessImage;

        [Output("Camera Found (Serial Valid)")]
        ISpread<bool> FOutCamFoundSerialValid;

        [Output("Texture Out")]
        public ISpread<TextureResource<Info>> FTextureOut;

        
        [Output("xiCam")]
        public ISpread<xiCam> FOutXiCam;

        [Output("On Settings Change")]
        ISpread<bool> FOutSettingsChanged;

        private xiCam cam = new xiCam();
        private bool FParamsChanged = false;
        
        private string FCamSerial = null;
        private bool FCamSerialValid = false;
        private bool FDoImageProcessing = false;

        private bool FCamRunning = false;
        private int FCurrentWidth;
        private int FCurrentHeight;

		[Import()]
		ILogger FLogger;

        [Import]
        IHDEHost FHost;


		#endregion fields & pins

        public void OnImportsSatisfied()
        {
            //spreads have a length of one by default, change it
            //to zero so ResizeAndDispose works properly.
            FTextureOut.SliceCount = 0;
        }

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
            FOutSettingsChanged[0] = false;
            if (FInCamSerial.IsChanged || FCamSerial == null)
            {
                try
                {
                    if (FCamRunning)
                    {
                        cam.StopAcquisition();
                        cam.CloseDevice();
                        FCamRunning = false;
                    }
                    FCamSerial = FInCamSerial[0];
                    cam.OpenDevice(xiCam.OpenDevBy.SerialNumber, FCamSerial);
                    cam.StartAcquisition();
                    // set deafult settings:
                    SetParams(cam, FCamSerial, xs);

                    FCamSerialValid = true;
                    FCamRunning = true;
                    FOutXiCam[0] = cam;
                    cam.GetParam(PRM.WIDTH, out FCurrentWidth);
                    cam.GetParam(PRM.HEIGHT, out FCurrentHeight);
                }
                catch (Exception e)
                {
                    FCamSerial = "";
                    FCamSerialValid = false;
                    FCamRunning = false;
                    FOutXiCam[0] = null;
                    FLogger.Log(LogType.Debug, "cam init failed (for sn " + FInCamSerial[0] + "): " + e.Message);
                }
            }
            if (FInEnableProcessImage.IsChanged)
            {
                FDoImageProcessing = FInEnableProcessImage[0];
                if (!FDoImageProcessing)
                {
                    FTextureOut.SliceCount = 0;
                }
            }
            if (FCamRunning)
            {
                if (FInEnableProcessImage.IsChanged)
                {
                    FDoImageProcessing = FInEnableProcessImage[0];
                }
                if (FInSetParams.IsChanged && FInSetParams[0])
                {
                    SetParams(cam, FCamSerial, FInSettings[0]);
                    cam.GetParam(PRM.WIDTH, out FCurrentWidth);
                    cam.GetParam(PRM.HEIGHT, out FCurrentHeight);
                    FOutXiCam[0] = cam;
                }
                if (FInSettings.IsChanged && FInSettings != null)
                {
                    FParamsChanged = true;
                }
                if (FParamsChanged)
                {
                    SetParams(cam, FCamSerial, FInSettings[0]);
                    cam.GetParam(PRM.WIDTH, out FCurrentWidth);
                    cam.GetParam(PRM.HEIGHT, out FCurrentHeight);
                    FParamsChanged = false;
                    FOutXiCam[0] = cam;
                    FOutSettingsChanged[0] = true;
                }
                if (FDoImageProcessing)
                {
                    FTextureOut.ResizeAndDispose(spreadMax, CreateTextureResource);

                    for (int i = 0; i < spreadMax; i++)
                    {
                        var textureResource = FTextureOut[i];
                        var info = textureResource.Metadata;
                        //recreate textures if resolution was changed
                        if (info.Width != FCurrentWidth || info.Height != FCurrentHeight)
                        {
                            textureResource.Dispose();
                            textureResource = CreateTextureResource(i);
                            info = textureResource.Metadata;
                        }
                        FTextureOut[i] = textureResource;
                    }
                }
            }
            FOutCamFoundSerialValid[0] = FCamSerialValid;
		}

        TextureResource<Info> CreateTextureResource(int slice)
        {
            var info = new Info() { Slice = slice, Width = FCurrentWidth, Height = FCurrentHeight};
            return TextureResource.Create(info, CreateTexture, UpdateTexture);
        }


        //this method gets called, when Reinitialize() was called in evaluate, or a graphics device asks for its data
        Texture CreateTexture(Info info, Device device)
        {
            FLogger.Log(LogType.Debug, "Creating new texture at slice: " + info.Slice);
            Texture t = TextureUtils.CreateTextureNoAlpha(device, Math.Max(info.Width, 1), Math.Max(info.Height, 1));
            return t;
        }

        //this method gets called, when Update() was called in evaluate,
        //or a graphics device asks for its texture, here you fill the texture with the actual data
        //this is called for each renderer, careful here with multiscreen setups, in that case
        //calculate the pixels in evaluate and just copy the data to the device texture here
        unsafe void UpdateTexture(Info info, Texture texture)
        {
            Bitmap Bitmap;
            int timeout = 1000;
            try
            {
                cam.GetImage(out Bitmap, timeout);
            }
            catch (Exception e) 
            {
                Bitmap = new Bitmap(FCurrentWidth, FCurrentHeight);
            }
            TextureUtils.CopyBitmapToTexture(Bitmap, texture);
        }


        private void SetParams(xiCam theCam, string serial, XimeaSettings xis)
        {
            try
            {
                cam.SetParam(PRM.DOWNSAMPLING, xis.DownsamplingMode);
                cam.SetParam(PRM.IMAGE_DATA_FORMAT, xis.ImageDataFormat);
                cam.SetParam(PRM.WIDTH, xis.Width);
                cam.SetParam(PRM.HEIGHT, xis.Height);
                cam.SetParam(PRM.OFFSET_X, xis.Offset_X);
                cam.SetParam(PRM.OFFSET_Y, xis.Offset_Y);
                cam.SetParam(PRM.EXPOSURE, xis.Exposure);
                cam.SetParam(PRM.GAIN, xis.Gain);
                cam.SetParam(PRM.FRAMERATE, xis.Framerate);
                cam.SetParam(PRM.DOWNSAMPLING_TYPE, xis.DownsamplingType);
                cam.SetParam(PRM.LIMIT_BANDWIDTH, xis.LimitBandwidth);
                FLogger.Log(LogType.Debug, "Cam " + serial + ": parameters set successfully");
            }
            catch (Exception e)
            {
                FLogger.Log(LogType.Debug, "Cam " + serial + ": error setting device properties: " + e.Message);
            }
        }
	}
}
