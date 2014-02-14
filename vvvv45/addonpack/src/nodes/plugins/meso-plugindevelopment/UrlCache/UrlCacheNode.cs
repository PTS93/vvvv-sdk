#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

using System.IO;
using System.Collections.Generic;

using VVVV.Core.Logging;
using VVVV.Nodes.FileLinks;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "UrlCacheManager", Category = "Network", Help = "Basic template with one string in/out", Tags = "", Author = "woei")]
	#endregion PluginInfo
	public class UrlCacheMgrNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#pragma warning disable 649, 169
		#region pins & fields
		[Input("Update Filelist", IsSingle = true, IsBang = true)]
		IDiffSpread<bool> FUpdate;
		
		[Input("Max Concurrent Downloads", DefaultValue = 2, MinValue = 1, IsSingle = true)]
		IDiffSpread<int> FMaxConcurrecy;
		
		[Input("Delete Trash", IsBang = true, IsSingle = true)]
		IDiffSpread<bool> FDelete;
		
		[Output("Remotes")]
		ISpread<string> FRemote;
		
		[Output("Local")]
		ISpread<string> FLocal;
		
		[Output("Handled By")]
		ISpread<string> FHandled;
		
				
		[Output("Downloads Running")]
		ISpread<int> FDLR;
		
		[Output("Downloads Queued")]
		ISpread<int> FDLQ;
		
		[Output("Bytes Missing")]
		ISpread<int> FBytesMissing;
		
		[Output("Trash")]
		ISpread<string> FTrash;

		
		FileLinkManager FMgr;
		#endregion
		#pragma warning restore
		
		#region OnImportsSatisfied
		public void OnImportsSatisfied()
		{
			FMgr = FileLinkManager.Instance;
			FRemote.SliceCount = 0;
			FLocal.SliceCount = 0;
			FHandled.SliceCount = 0;
		}
		#endregion
		
		public void Evaluate(int spreadMax)
		{
			if (FMaxConcurrecy.IsChanged)
				FMgr.MaxConcurrentDLs = FMaxConcurrecy[0];
			
			if (FUpdate[0])
			{
				FRemote.SliceCount = FMgr.FileLinks.Length;
				FLocal.SliceCount = FMgr.FileLinks.Length;
				FHandled.SliceCount = FMgr.FileLinks.Length;
				for (int i=0; i<FMgr.FileLinks.Length; i++)
				{
					FRemote[i] = FMgr.FileLinks[i].RemoteFile;
					FLocal[i] = FMgr.FileLinks[i].LocalFile;
					FHandled[i] = FMgr.FileLinks[i].HandledBy;
				}
			}
			
			FDLR.SliceCount = 1;
			FDLR[0] = FMgr.DLsRunning;
			FDLQ.SliceCount = 1;
			FDLQ[0] = FMgr.DLsQueued;
			FBytesMissing.SliceCount = 1;
			FBytesMissing[0] = FMgr.BytesMissing;
			
			if (FDelete.IsChanged)
				if (FDelete[0])
					FMgr.DeleteTrash();
			FTrash.AssignFrom(FMgr.Trash);
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "UrlCache", Category = "Network", Help = "Basic template with one string in/out", Tags = "", Author = "woei")]
	#endregion PluginInfo
	public class UrlCacheNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
	{
		#pragma warning disable 649, 169
		#region pins
		[Input("Remote Root", StringType = StringType.URL)]
		IDiffSpread<string> FRemoteRoot;
		
		[Input("Remote File")]
		IDiffSpread<string> FRemotePath;
		
		[Input("Local Root", StringType = StringType.Directory)]
		ISpread<string> FLocalRoot;
		
		[Input("Local File")]
		IDiffSpread<string> FLocalPath;
		
		[Input("Download Priority")]
		IDiffSpread<float> FPriority;

		[Input("Loading Filename", DefaultString = "HaloDot.bmp", StringType = StringType.Filename)]
		IDiffSpread<string> FDefault;
		
		[Input("Force Download", IsBang = true)]
		IDiffSpread<bool> FForceDL;
		
		[Input("Enabled")]
		IDiffSpread<bool> FEnabled;


//		[Output("Output")]
//		IOutStream<string> FOutput;
		
		[Output("Is Cached")]
		IOutStream<float> FIsCached;
		
//		[Output("Status")]
//		IOutStream<string> FStatus;
		
		[Output("Status Index")]
		IOutStream<int> FStatusIndex;
		
		[Output("Status List")]
		ISpread<string> FStatusList;
		#endregion pins
		
		#region fields
		[Import()]
		IPluginHost2 FHost;

		[Import()]
		ILogger FLogger;
		
		private bool firstFrame = true;
		private bool disposed = false;
		private string FNodePath;
		private FileLinkManager FMgr;
		private List<FileLink> FLinks;
		#endregion fields
		#pragma warning restore

		#region OnImportsSatisfied
		public void OnImportsSatisfied()
		{
			FNodePath = FHost.GetNodePath(false);
			FMgr = FileLinkManager.Instance;
			FMgr.Register(FNodePath);
			FLinks = new List<FileLink>();
		}
		#endregion
		
		#region IDisposable
		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                	FMgr.UnRegister(FNodePath);
                }
                disposed = true;
            }
        }
		#endregion
		
		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			if (firstFrame)
			{
				FStatusList.AssignFrom(TFileLinkStatus.GetNames(typeof(TFileLinkStatus)).ToSpread());
				
				firstFrame = false;
			}
			
			if (FRemoteRoot.IsChanged ||
			    FRemotePath.IsChanged ||
			    FLocalRoot.IsChanged ||
			    FLocalPath.IsChanged ||
			    FPriority.IsChanged ||
			    FForceDL.IsChanged ||
			    FEnabled.IsChanged ||
			    spreadMax != FLinks.Count)
			{
//				FOutput.Length = spreadMax;
				FIsCached.Length = spreadMax;
//				FStatus.Length = spreadMax;
				FStatusIndex.Length = spreadMax;
				
				FLinks = new List<FileLink>();
				for (int f=0; f<spreadMax; f++)
				{
					//get local root
					DirectoryInfo localRoot = new DirectoryInfo(FLocalRoot[f]);
					
					//get local file
					string localPath = GetFileName(FLocalPath[f]);
	
					//get remote file
					string remoteRoot = GetFolderName(FRemoteRoot[f]);
					string remotePath = GetFileName(FRemotePath[f]);
					Uri remoteFile = new Uri("http://example.org/index.html");
					
					FileLink fl;
					try
					{
						remoteFile = new Uri(remoteRoot+remotePath);
						fl = FMgr.CreateFileLink(localRoot, localPath, remoteFile, FNodePath);
					
						if (fl.HandledBy == FNodePath)
						{
							fl.Syncronize = FEnabled[f];
							fl.Priority = FPriority[f];
							if (FForceDL[f])
								fl.ForceDownload();
						}
					}
					catch (Exception e)
					{
						fl = new FileLink(localRoot, localPath, remoteFile, string.Empty);
						FLogger.Log(e);
					}
					FLinks.Add(fl);
				}
				FMgr.RefreshList(FNodePath, FLinks);
			}
			
//			using (var oW = FOutput.GetWriter())
			using (var icW = FIsCached.GetWriter())
//			using (var sW = FStatus.GetWriter())
			using (var siW = FStatusIndex.GetWriter())
			{
				for (int i=0; i<FLinks.Count; i++)
				{
//					if (FLinks[i].IsCached==1)
//						oW.Write(FLinks[i].LocalFile);
//					else
//						oW.Write(FDefault[i]);
					icW.Write(FLinks[i].IsCached);
					siW.Write((int)FLinks[i].Status);
//					sW.Write(FLinks[i].StatusMessage);
				}
			}
		}
		
		/// <summary>
		/// trims leading slashes
		/// </summary>
		/// <param name="RawName"></param>
		/// <returns></returns>
		private string GetFileName(string RawName)
		{
			if (RawName.StartsWith("/") || RawName.StartsWith(@"\"))
				RawName = RawName.Substring(1);
			return RawName;
		}
		
		/// <summary>
		/// appends slashes
		/// </summary>
		/// <param name="RawName"></param>
		/// <returns></returns>
		private string GetFolderName(string RawName)
		{
			if (RawName.Contains("/"))
				if (!RawName.EndsWith("/"))
					RawName += "/";
			
			if (RawName.Contains(@"/"))
				if (!RawName.EndsWith(@"/"))
					RawName += @"/";
			return RawName;
		}
	}
}
