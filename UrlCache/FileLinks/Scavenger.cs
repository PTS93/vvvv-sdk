using System;
using System.IO;
using System.Collections.Generic;

namespace VVVV.Nodes.FileLinks
{
	/// <summary>
	/// Looking for trash in a folder
	/// </summary>
	internal class Scavenger
	{
		private DirectoryInfo _root;
		private Dictionary<string, FileInfo> _trash;
		private FileSystemWatcher _watcher;
		
		private Dictionary<string, FileLink> _filelinks;
		
		#region constructor
		public Scavenger(DirectoryInfo root)
		{
			_root = root;
			if (!_root.Exists)
				_root.Create();
			
			_trash = new Dictionary<string, FileInfo>();
			foreach (var fi in _root.EnumerateFiles("*.*", SearchOption.AllDirectories))
				_trash.Add(fi.FullName, fi);
//			_trash = new List<FileInfo>(_root.EnumerateFiles("*.*", SearchOption.AllDirectories));
			
			_watcher = new FileSystemWatcher(_root.FullName);
			_watcher.IncludeSubdirectories = true;
			_watcher.Created += new FileSystemEventHandler(_watcher_Created);
			_watcher.Renamed += new RenamedEventHandler(_watcher_Renamed);
			_watcher.Deleted += new FileSystemEventHandler(_watcher_Deleted);
			_watcher.EnableRaisingEvents = true;
			
			_filelinks = new Dictionary<string, FileLink>();
//			_filelinks = new List<FileLink>();
		}
		#endregion
		
		#region Fields
		internal List<FileInfo> Trash
		{
			get { return new List<FileInfo>(_trash.Values); }
		}
		
		internal string Root 
		{
			get { return _root.FullName; }
		}
		#endregion
		
		internal void DeleteTrash()
		{
			foreach (FileInfo fi in _trash.Values)
				fi.Delete();
			
			_trash.Clear();
		}
		
		/// <summary>
		/// excludes the local files of a FileLink from trash
		/// </summary>
		/// <param name="fl"></param>
		internal void AddFileLink(FileLink fl)
		{
			_filelinks.Add(fl.LocalFile, fl);
			
			TryRemoveTrash(fl.LocalFileInfo.FullName);
			TryRemoveTrash(fl.TempFileInfo.FullName);
			
//			if (_trash.ContainsKey(fl.LocalFileInfo.FullName))
//				_trash.Remove(fl.LocalFileInfo.FullName);
//			if (_trash.ContainsKey(fl.TempFileInfo.FullName))
//				_trash.Remove(fl.TempFileInfo.FullName);
			
			
//			for (int i=_trash.Count-1; i>=0; i--)
//			{
//				if (_trash[i].FullName == fl.LocalFileInfo.FullName)
//					_trash.RemoveAt(i);
//				else if (_trash[i].FullName == fl.TempFileInfo.FullName)
//					_trash.RemoveAt(i);
//			}
		}
		
		/// <summary>
		/// includes the local files of a FileLink in the trash
		/// </summary>
		/// <param name="fl"></param>
		internal void RemoveFileLink(FileLink fl)
		{
			fl.LocalFileInfo.Refresh();
			if (fl.LocalFileInfo.Exists)
				_trash.Add(fl.LocalFileInfo.FullName, fl.LocalFileInfo);
			
			fl.TempFileInfo.Refresh();
			if (fl.TempFileInfo.Exists)
				_trash.Add(fl.TempFileInfo.FullName,fl.TempFileInfo);
			
			_filelinks.Remove(fl.LocalFile);
//			int id = _filelinks.IndexOf(fl);
//			if (id >= 0)
//				_filelinks.RemoveAt(id);
		}
		
		
		private void _watcher_Created(object sender, FileSystemEventArgs e)
		{
			TryAddTrash(e.FullPath);
		}
		
		private void _watcher_Renamed(object sender, RenamedEventArgs e)
		{
			TryRemoveTrash(e.OldFullPath);
			TryAddTrash(e.FullPath);
		}
		
		private void _watcher_Deleted(object sender, FileSystemEventArgs e)
		{
			TryRemoveTrash(e.FullPath);
		}
		
		/// <summary>
		/// if not excluded, add a file to the trash
		/// </summary>
		/// <param name="fullPath"></param>
		private void TryAddTrash(string fullPath)
		{
			
			if (fullPath.EndsWith(".part"))
				fullPath = fullPath.Replace(".part","");
			
			if (!_filelinks.ContainsKey(fullPath))
				_trash.Add(fullPath, new FileInfo(fullPath));
			
//			bool isTrash = true;
//			foreach (FileLink fl in _filelinks)
//			{
//				if (fullPath == fl.LocalFileInfo.FullName)
//					isTrash = false;
//				else if (fullPath == fl.TempFileInfo.FullName)
//					isTrash = false;
//			}
//			if (isTrash)
//				_trash.Add(new FileInfo(fullPath));
		}
		
		/// <summary>
		/// remove a file from trash
		/// </summary>
		/// <param name="fullPath"></param>
		private void TryRemoveTrash(string fullPath)
		{
			if (_trash.ContainsKey(fullPath))
			    _trash.Remove(fullPath);
			    
//			for (int i=_trash.Count-1; i>=0; i--)
//				if (_trash[i].FullName == fullPath)
//					_trash.RemoveAt(i);
		}
	}
}
