#region usings
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
#endregion usings

namespace VVVV.Nodes.FileLinks
{
	/// <summary>
	/// singleton class handling all urlcache nodes
	/// </summary>
	public class FileLinkManager
	{
		private static readonly FileLinkManager instance = new FileLinkManager();
		private Dispatcher mainThread; 
		
		private List<FileLink> dlQueue = new List<FileLink>();
		private PriorityComparer pc = new PriorityComparer();
		private int maxParallel = 2;
		private int dlCounter = 0;
		
		private Dictionary<string, FileLink> flDict = new Dictionary<string, FileLink>();
		private Dictionary<string, Scavenger> trashDict = new Dictionary<string, Scavenger>();
		private Dictionary<string, Dictionary<string, FileLink>> nodeDict = new Dictionary<string, Dictionary<string, FileLink>>();
		
		private FileLinkManager()
		{
			System.Net.ServicePointManager.DefaultConnectionLimit = 100; //need for speed
			mainThread = Dispatcher.CurrentDispatcher;
		}
		
		#region fields
		public static FileLinkManager Instance
		{
			get { return instance; }
		}
		
		public FileLink[] FileLinks
		{
			get { return flDict.Values.ToArray(); }
		}
		
		public int MaxConcurrentDLs
		{
			get { return maxParallel; }
			set { maxParallel = Math.Max(1,value); TryStartDownload(); }
		}
		
		public int DLsQueued
		{
			get { return dlQueue.Count; }
		}
		
		public int DLsRunning
		{
			get { return dlCounter; }
		}
		
		public int BytesMissing
		{
			get 
			{ 
				int i = 0;
				foreach (FileLink fl in flDict.Values)
					i += fl.BytesMissing;
				return i;
			}
		}
		
		public IEnumerable<string> Trash
		{
			get
			{
				foreach (Scavenger s in trashDict.Values)
					foreach (var fi in s.Trash)
						yield return fi.FullName;
			}
		}
		#endregion fields
		
		/// <summary>
		/// creates a new FileLink if not existent
		/// </summary>
		/// <param name="localRoot"></param>
		/// <param name="localPath"></param>
		/// <param name="remoteFile"></param>
		/// <param name="nodePath"></param>
		/// <returns>new or in use FileLink</returns>
		public FileLink CreateFileLink(System.IO.DirectoryInfo localRoot, string localPath, Uri remoteFile, string nodePath)
		{
			FileLink fl = new FileLink(localRoot, localPath, remoteFile, nodePath);
			string key = fl.LocalFile;
			if (!flDict.ContainsKey(key))
		    {
				fl.Initialize();
				flDict.Add(fl.LocalFile, fl);
				nodeDict[nodePath].Add(fl.LocalFile,fl);
				
				if (!trashDict.ContainsKey(fl.LocalRoot.FullName))
					trashDict.Add(fl.LocalRoot.FullName, new Scavenger(fl.LocalRoot));
				
				trashDict[fl.LocalRoot.FullName].AddFileLink(fl);
		    }
			else
			{
				fl = flDict[key];
				
				if (fl.HandledBy == string.Empty) //pick up unhandled filelinks
					fl.HandledBy = nodePath;
				
				if (!nodeDict[nodePath].ContainsKey(key))
					nodeDict[nodePath].Add(fl.LocalFile,fl);
			}
			
			return fl;
		}
		
		/// <summary>
		/// registers a node as FileLink user
		/// call this in nodes construtor
		/// </summary>
		/// <param name="NodePath"></param>
		public void Register(string NodePath)
		{
			nodeDict.Add(NodePath, new Dictionary<string, FileLink>());
		}
		
		/// <summary>
		/// removes a node as FileLink user
		/// call this on dispose of the node
		/// </summary>
		/// <param name="NodePath"></param>
		public void UnRegister(string NodePath)
		{
			foreach (FileLink fl in nodeDict[NodePath].Values)
			{
				if (fl.HandledBy == NodePath)
				{
					fl.HandledBy = string.Empty; 
					
					TryRemove(NodePath, fl);
				}
			}
			
			nodeDict.Remove(NodePath);
		}
		
		private bool TryRemove(string NodePath, FileLink fl)
		{
			bool unique = true;
			foreach(string node in nodeDict.Keys)
			{
				if (node != NodePath)
				{
					if (nodeDict[node].ContainsKey(fl.LocalFile))
					{
						nodeDict[node][fl.LocalFile].HandledBy = node;
						unique = false;
					}
				}
			}
			
			if (unique)
			{
				flDict.Remove(fl.LocalFile);
				trashDict[fl.LocalRoot.FullName].RemoveFileLink(fl);
			}
			return unique;
		}
		
		/// <summary>
		/// if unused, remove handle from filelink
		/// and remove invalid filelinks completely
		/// </summary>
		/// <param name="NodePath"></param>
		/// <param name="FileLinks"></param>
		public void RefreshList(string NodePath, List<FileLink> FileLinks)
		{
			List<string> keys = new List<string>(nodeDict[NodePath].Keys);
			for (int k=keys.Count-1; k>=0; k--)
			{
				if (FileLinks.IndexOf(flDict[keys[k]]) >= 0)
					keys.RemoveAt(k);
			}
			
			for (int i=0; i<keys.Count; i++)
			{
				FileLink fl = flDict[keys[i]];
				fl.HandledBy = string.Empty;
				if (TryRemove(NodePath, fl))
					nodeDict[NodePath].Remove(keys[i]);
//				if ((int)fl.Status <= (int)TFileLinkStatus.NoRemote) //remove invalid FileLinks from list
//				{
//					trashDict[fl.LocalRoot.FullName].RemoveFileLink(fl);
//					flDict.Remove(fl.LocalFile);
//					nodeDict[NodePath].Remove(fl.LocalFile);
//				}
			}
			
//			bool[] inUse = new bool[nodeDict[NodePath].Count];
//			foreach (FileLink fi in FileLinks)
//			{
//				int id = nodeDict[NodePath].IndexOf(fi);
//				if (id != -1)
//					inUse[id]=true;
//			}
//			for (int i=inUse.Length-1; i>=0; i--)
//			{
//				if (!inUse[i])
//				{
//					FileLink fl = nodeDict[NodePath][i];
//					fl.HandledBy = string.Empty;
//					if ((int)fl.Status <= (int)TFileLinkStatus.NoRemote) //remove invalid FileLinks from list
//					{
//						trashDict[fl.LocalRoot.FullName].RemoveFileLink(fl);
//						flDict.Remove(fl.LocalFile);
//						nodeDict[NodePath].RemoveAt(i);
//					}
//				}
//			}
		}
		
		
		internal void AddDownload(FileLink fl)
		{
			dlQueue.Add(fl);
			TryStartDownload();
		}
		
		private void TryStartDownload()
		{
			if (dlQueue.Count>0 && dlCounter<maxParallel)
			{
				dlQueue.Sort(pc);
				FileLink fl = dlQueue[0];
				dlQueue.RemoveAt(0);
				if (fl.Syncronize)
				{
					dlCounter++;
					Thread t = new Thread(delegate() 
					                      {
					                      	fl.Download(); 
					                      	mainThread.Invoke(new Action(() => FinishDownload()), DispatcherPriority.Send); 
					                      });
					t.Start();
				}
				else
				{
					TryStartDownload();
				}
			}
		}
		
		private void FinishDownload()
		{
			dlCounter--;
			dlCounter = Math.Max(0,dlCounter);
			TryStartDownload();
		}
		
		public void DeleteTrash()
		{
			foreach (Scavenger s in trashDict.Values)
				s.DeleteTrash();
		}
		
		private class PriorityComparer : IComparer<FileLink>
		{
			public int Compare(FileLink A, FileLink B)
			{
				if (A.Priority < B.Priority)
					return 1;
				else if (A.Priority > B.Priority)
					return -1;
				else //prefer smaller files
				{
					if (A.BytesMissing < B.BytesMissing)
						return -1;
					else if (A.BytesMissing > B.BytesMissing)
						return 1;
					else
						return 0;
				}
			}
		}
	}
}
