#region usings
using System;
using System.IO;
using System.Net;

using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
#endregion usings

namespace VVVV.Nodes.FileLinks
{
	public enum TFileLinkStatus { Invalid, Initializing, Duplicate, NoRemote, PeekRemote, Ready, SyncError, Aborted, Syncronizing, Cached };
	/// <summary>
	/// maps a remote file to a local file
	/// </summary>
	public class FileLink : IDisposable
	{
		
		const int BUFFER_SIZE = 1024; //TODO find good size
		private FileLinkManager mgr;
		private string handledBy;
		
		private DirectoryInfo localRoot;
		private string localPath;
		private FileInfo localFile;
		private FileInfo tempFile;
		
		private Uri remoteFile;
		
		private float priority;
		
		private TFileLinkStatus status;
		private string statusMsg;
		
		private FileSystemWatcher watcher;
		
		//sync stuff
		private bool sync;
		
		private volatile bool idle;
		private long bytesRead;
		private long contentLength;
		private volatile int bytesMissing;
		private float isCached;
		
		private bool disposed = false;
		
		#region constructor
		public FileLink(DirectoryInfo LocalRoot, string LocalPath, Uri RemoteFile, string NodePath)
		{
			mgr = FileLinkManager.Instance;
			handledBy = NodePath;
			
			status = TFileLinkStatus.Invalid;
			statusMsg = status.ToString();
			sync = false;
			idle = true;
			bytesRead = 0;
			contentLength = 0;
			bytesMissing = 0;
			isCached = 0;
			
			this.localRoot = LocalRoot;
			this.localPath = LocalPath;
			this.localFile = new FileInfo(Path.Combine(localRoot.FullName,localPath));
			this.tempFile = new FileInfo(localFile.FullName+".part");
			this.remoteFile = RemoteFile;
			
			this.priority = 0;
		}
		#endregion
		
		#region fields
		internal DirectoryInfo LocalRoot
		{
			get { return localRoot; }
		}
		
		internal FileInfo LocalFileInfo
		{
			get { return localFile; }
		}
		
		internal FileInfo TempFileInfo
		{
			get { return tempFile; }
		}
		
		public string LocalFile
		{
			get { return localFile.FullName; }
		}
		
		public string RemoteFile
		{
			get { return remoteFile.AbsoluteUri; }
		}
		
		public float Priority
		{
			get { return priority; }
			set { priority = value; }
		}
		
		public int BytesMissing
		{
			get { return bytesMissing; }
		}
		
		public float IsCached
		{
			get { return isCached; }
		}
		
		internal TFileLinkStatus Status
		{
			get { return status; }
			private set 
			{
				if (value != status)
				{
					status = value;
					statusMsg = status.ToString();
				}
			}
		}
		
		/// <summary>
		/// performance field, does the to string op only when changed
		/// </summary>
		public string StatusMessage
		{
			get { return statusMsg; }
		}
		
		public string HandledBy
		{
			get { return handledBy; }
			internal set 
			{ 
				handledBy = value;
				if (handledBy == string.Empty)
					this.Syncronize = false;
			}
		}
		
		public bool Syncronize
		{
			get { return sync; }
			set 
			{ 
				if (value != sync)
				{
					sync = value;
					if (value)
					{
						if ((int)status != (int)TFileLinkStatus.Cached)
							TryDownload();
					}
					else
					{
						idle = true;
					}
				}
			}
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
                	this.Syncronize = false;
                }
                disposed = true;
            }
        }
		#endregion
		
		public void Initialize()
		{
			Status = TFileLinkStatus.Initializing;
			localFile.Directory.Create();
			LocalFileExists();
			
			watcher = new FileSystemWatcher(localFile.DirectoryName, localFile.Name);
			watcher.Created += new FileSystemEventHandler(watcher_Created);
			watcher.Renamed += new RenamedEventHandler(watcher_Renamed);
			watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);
			watcher.Changed += new FileSystemEventHandler(watcher_Created);
			watcher.EnableRaisingEvents = true;
		}

		#region watcher raised methos
		private void watcher_Created(object sender, FileSystemEventArgs e)
		{
			localFile.Refresh();
			LocalFileExists(75);
		}
		
		private void watcher_Deleted(object sender, FileSystemEventArgs e)
		{
			localFile.Refresh();
			bytesRead = 0;
			isCached = 0;
			Status = TFileLinkStatus.Ready;
			if (sync)
				TryDownload();
		}

		private void watcher_Renamed(object sender, RenamedEventArgs e)
		{
			localFile.Refresh();
			Status = TFileLinkStatus.Ready;
			if (sync)
				TryDownload();
		}
		#endregion
		
		#region remote size
		/// <summary>
		/// sets the content size and sets the status accordingly
		/// </summary>
		private void PeekRemote()
		{
			Status = TFileLinkStatus.PeekRemote;
			switch (remoteFile.Scheme)
			{
				case "http":
				case "https":
				{
					SetContentLengthByHttp();
					break;
				}
				case "file":
				{
					SetContentLengthByFile();
					break;
				}
				default:
				{
					contentLength = 0;
					break;
				}
			}
			
			if (contentLength > 0)
			{
				Status = TFileLinkStatus.Ready;
				SetByteInfo();
			}
			else
				Status = TFileLinkStatus.NoRemote;
			
			if (localFile.Exists)
			{
				contentLength = localFile.Length;
				bytesRead = contentLength;
				SetByteInfo();
				if ((int)Status >= (int)TFileLinkStatus.Ready)
					Status = TFileLinkStatus.Cached;
			}
		}
		
		/// <summary>
		/// request remote file length by http protocol
		/// </summary>
		private void SetContentLengthByHttp()
		{
			try
			{
				HttpWebRequest existRequest = (HttpWebRequest)WebRequest.Create(remoteFile);
				existRequest.Credentials = CredentialCache.DefaultCredentials;
				existRequest.Method = "HEAD";
				HttpWebResponse existResponse = (HttpWebResponse)existRequest.GetResponse();
				if (existResponse.StatusCode == HttpStatusCode.OK)
				{
					contentLength = existResponse.ContentLength;
				}
				existResponse.Close();
			}
			catch
			{
				contentLength = 0;
			}
		}
		
		/// <summary>
		/// request remote file length by file protocol
		/// </summary>
		private void SetContentLengthByFile()
		{
			try
			{
				FileWebRequest fileRequest = (FileWebRequest)WebRequest.Create(remoteFile);
				fileRequest.Credentials = CredentialCache.DefaultCredentials;
				FileWebResponse fileResponse = (FileWebResponse)fileRequest.GetResponse();
				contentLength = fileResponse.ContentLength;
				fileResponse.Close();
			}
			catch
			{
				contentLength = 0;
			}
		}
		#endregion
		
		public void ForceDownload()
		{
			if (sync)
				TryDownload();
		}
		
		/// <summary>
		/// Issues the download or gets the remote file size
		/// </summary>
		private void TryDownload() 
		{
			if ((int)status >= (int)TFileLinkStatus.Ready) //if remote available
			{
				if (idle) //if not in already in copy process
				{
					Status = TFileLinkStatus.Ready;
					idle = false;
					try
					{
						mgr.AddDownload(this);
					}
					catch (Exception e)
					{
						Status = TFileLinkStatus.SyncError;
						System.Diagnostics.Debug.WriteLine(e);
						idle = true;
					}
				}
			}
			else if ((int)status <= (int)TFileLinkStatus.Initializing)
			{
				Task t = Task.Factory.StartNew(()=>PeekRemote());
				t.ContinueWith((x) => { if (sync) {TryDownload();} });
			}
		}
		
		/// <summary>
		/// synchronizes the local with the remote file 
		/// </summary>
		internal void Download()
		{
			Status = TFileLinkStatus.Syncronizing;
			tempFile = new FileInfo(localFile.FullName+".part");
			
			switch (remoteFile.Scheme)
			{
				case "http":
				case "https":
				{
					FileStream writeStream = OpenWriteStream(true);
					if (writeStream != null && isCached!=1)
					{
						HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(remoteFile);
						httpRequest.Credentials = CredentialCache.DefaultCredentials;
						if (bytesRead>0)
							httpRequest.AddRange(bytesRead);
						
						WebResponse httpResponse = GetResponseCancellable(httpRequest);
						
						StreamCopy(httpResponse.GetResponseStream(), writeStream);
						
						httpResponse.Close();
					}
					break;
				}
				case "file":
				{
					FileStream writeStream = OpenWriteStream(false);
					
					FileWebRequest fileRequest = (FileWebRequest)WebRequest.Create(remoteFile);
					fileRequest.Credentials = CredentialCache.DefaultCredentials;
					
					WebResponse fileResponse = GetResponseCancellable(fileRequest);
					
					StreamCopy(fileResponse.GetResponseStream(), writeStream);
					
					fileResponse.Close();
					break;
				}
				default:
				{
					break;
				}
			}
			if (isCached == 1)
			{
				tempFile.Refresh();
				if (tempFile.Length == contentLength) //maybe someone copied the local file into the folder
					tempFile.CopyTo(localFile.FullName,true);
				
				localFile.Refresh();
				if (localFile.Exists)
				{
					if (localFile.Length == contentLength) //keep temp file if localfile doesn't really match, restart download?
					{
						try
						{
							tempFile.Delete();
						}
						catch { }
					}
				}
			}
			else if ((int)Status == (int)TFileLinkStatus.Syncronizing) //cancelled -> keep temp file
				Status = TFileLinkStatus.Aborted;
				
			idle = true;
			
			if ((int)Status == (int)TFileLinkStatus.SyncError)
				TryDownload();
		}
		
		/// <summary>
		/// creates a writable stream of a temporary file
		/// </summary>
		/// <param name="Resume">bool if resume writing is desired</param>
		/// <returns></returns>
		private FileStream OpenWriteStream(bool Resume)
		{
			tempFile.Refresh();
			if (!tempFile.Directory.Exists)
				tempFile.Directory.Create();
			
			FileStream writeStream;
			if (Resume && tempFile.Exists)
			{
				try
				{
					writeStream = new FileStream(tempFile.FullName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
					bytesRead = writeStream.Length;
				}
				catch
				{
					try
					{
						tempFile = new FileInfo(tempFile.FullName+"2");
						writeStream = new FileStream(tempFile.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
						bytesRead = 0;
					}
					catch
					{
						writeStream = null;
						Status = TFileLinkStatus.SyncError;
					}
				}
			}
			else
			{
				try
				{
					if (tempFile.Exists)
						tempFile.Delete();
					writeStream = new FileStream(tempFile.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
				}
				catch
				{
					tempFile = new FileInfo(tempFile.FullName+"2");
					writeStream = new FileStream(tempFile.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
				}
				bytesRead = 0;
			}
			
			SetByteInfo();
			
			return writeStream;
		}
		
		/// <summary>
		/// Wrappermethod to be able to cancel while getting webresponse
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		private WebResponse GetResponseCancellable(WebRequest request)
		{
			var asyncResponse = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse,null);
			while (asyncResponse.Status == TaskStatus.Running && (!idle))
			{ } //empty loop while waiting for return or cancel
			return asyncResponse.Result;
		}
		
		/// <summary>
		/// copies readstream to writestream and calculates the progress
		/// </summary>
		/// <param name="readStream"></param>
		/// <param name="writeStream"></param>
		private void StreamCopy(Stream readStream, FileStream writeStream)
		{
			if (writeStream != null)
			{
	            var buffer = new byte[BUFFER_SIZE];
	            while ((!idle) && bytesRead < contentLength)
	            {
	                int readSize = readStream.Read(buffer, 0, BUFFER_SIZE);
	                writeStream.Write(buffer, 0, readSize);
	                bytesRead += readSize;
	                SetByteInfo();
	            	Thread.Sleep(5); //leave some bandwidth for sizechecks
	            }
	            
	            writeStream.Close();
			}
            readStream.Close();
		}
		
		/// <summary>
		/// calculate and set isCached and bytesMissing
		/// </summary>
		private void SetByteInfo()
		{
			isCached = bytesRead / (float)contentLength;
			bytesMissing = (int)(contentLength-bytesRead);
		}
		
		private bool LocalFileExists(int waitTime = 0)
		{
			bool exists = localFile.Exists;
			if (localFile.Exists)
			{
//				if (contentLength == localFile.Length)
//				{
					idle = true; //cancel copying
					Thread.Sleep(waitTime); //wait a bit for thread to return
					Status = TFileLinkStatus.Cached;
					contentLength = localFile.Length;
					bytesRead = contentLength;
					isCached = 1;
//				}
			}
			return exists;
		}
	}
}
