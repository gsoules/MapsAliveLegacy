// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Diagnostics;
using AvantLogic.MapsAlive.Engine;

public class FileManager : BaseFileManager
{
	public FileManager()
	{
	}

	public static string AppRuntimeFolderLocationAbsolute
	{
		get { return App.AppRuntimeFolderLocationAbsolute; }
	}

	public new static bool CopyFile(string sourceFileLocation, string targetFileLocation)
	{
		try
		{
			BaseFileManager.CopyFile(sourceFileLocation, targetFileLocation);
			return true;
		}
		catch (BaseFileManagerException ex)
		{
			Utility.ReportException("CopyFile", string.Format("Copy {0} to {1}{2}{3}", sourceFileLocation, targetFileLocation, DumpFileInfo(sourceFileLocation), DumpFileInfo(targetFileLocation)), ex);
			return false;
		}
	}

	public new static bool CopyFolder(string sourceDir, string targetDir)
	{
		try
		{
			return BaseFileManager.CopyFolder(sourceDir, targetDir);
		}
		catch (BaseFileManagerException ex)
		{
			Utility.ReportException("CopyFolder " + sourceDir, ex);
			return false;
		}
	}

	public new static bool CreateFolder(string folderLocation)
	{
		try
		{
			return BaseFileManager.CreateFolder(folderLocation);
		}
		catch (BaseFileManagerException ex)
		{
			Utility.ReportException("CreateFolder " + folderLocation, ex);
			return false;
		}
	}

	public static bool CreateTextFile(string fileLocation, string contents)
	{
		return CreateHtmlFile(fileLocation, contents, false);
	}

	public new static bool CreateHtmlFile(string fileLocation, string contents, bool includeBom)
	{
		try
		{
			return BaseFileManager.CreateHtmlFile(fileLocation, contents, includeBom);
		}
		catch (BaseFileManagerException ex)
		{
			Utility.ReportException("CreateHtmlFile " + fileLocation, ex);
			return false;
		}
	}

	public new static bool DeleteFile(string fileLocation)
	{
		try
		{
			return BaseFileManager.DeleteFile(fileLocation);
		}
		catch (BaseFileManagerException ex)
		{
			Utility.ReportException("DeleteFile", DumpFileInfo(fileLocation), ex);
			return false;
		}
	}

	public new static bool DeleteFolder(string folderName)
	{
		try
		{
			return BaseFileManager.DeleteFolder(folderName);
		}
		catch (BaseFileManagerException ex)
		{
			Utility.ReportException("DeleteFolder " + folderName, ex);
			return false;
		}
	}

	public static bool DeleteFolderContents(string folderName)
	{
		try
		{
			string[] Files = FolderEntries(folderName);

			foreach (string file in Files)
			{
				if (File.Exists(file))
				{
					DeleteFile(file);
				}
			}

			return true;
		}
		catch (BaseFileManagerException ex)
		{
			Utility.ReportException("DeleteFolderContents " + folderName, ex);
			return false;
		}
	}

	public static string DumpFileInfo(string fileLocation)
	{
		string dump = "\n\nFILE INFO DUMP:";

		FileInfo fileInfo = new FileInfo(fileLocation);
		if (fileInfo.Exists)
		{
			dump += string.Format("\nFullName:{0}\nLength:{1}\nAttributes:{2}\nCreationTime:{3}\nLastAccessTime:{4}\nLastWriteTime:{5}\n",
				fileInfo.FullName,
				fileInfo.Length,
				fileInfo.Attributes,
				fileInfo.CreationTime,
				fileInfo.LastAccessTime,
				fileInfo.LastWriteTime
				);
		}
		else
		{
			dump += string.Format("\n{0} does not exist\n", fileLocation);
		}

		return dump;
	}

	public static bool FileExists(string fileLocation)
	{
		return File.Exists(fileLocation);
	}

	public static string FileNamePrefix(string fileLocation)
	{
		if (fileLocation == null || fileLocation == string.Empty)
			return string.Empty;
		FileInfo fileInfo = new FileInfo(fileLocation);
		string name = fileInfo.Name;
		string ext = fileInfo.Extension;
		int extLength = ext.Length;
		return name.Substring(0, name.Length - extLength);
	}

	public new static string[] FolderEntries(string folderLocation)
	{
		try
		{
			return BaseFileManager.FolderEntries(folderLocation);
		}
		catch (BaseFileManagerException ex)
		{
			Utility.ReportException("FolderEntries " + folderLocation, ex);
			return new string[0];
		}
	}

	public new static string[] FolderEntries(string folderLocation, string pattern)
	{
		try
		{
			return BaseFileManager.FolderEntries(folderLocation, pattern);
		}
		catch (BaseFileManagerException ex)
		{
			Utility.ReportException("FolderEntries " + folderLocation, ex);
			return new string[0];
		}
	}

	public static bool FolderExists(string folderLocation)
	{
		return Directory.Exists(folderLocation);
	}

	public static string PreviewFolderLocationAbsolute(int tourId)
	{
		return PublishedFolderLocationAbsolute(tourId) + "_";
	}

	public static string PreviewFolderLocationAbsolute(int tourId, string fileName)
	{
		return Path.Combine(PreviewFolderLocationAbsolute(tourId), fileName);
	}

	public static string PublishedFolderLocationAbsolute(int tourId)
	{
		return Path.Combine(App.TourFolderLocationAbsolute, tourId.ToString());
	}

	public static string PublishedFolderLocationAbsolute(int tourId, string fileName)
	{
		return Path.Combine(PublishedFolderLocationAbsolute(tourId), fileName);
	}

	public static byte[] ReadFileBytes(string fileLocation)
	{
		try
		{
			byte[] bytes = File.ReadAllBytes(fileLocation);
			return bytes;
		}
		catch (Exception ex)
		{
			Utility.ReportException("ReadFileBytes " + fileLocation, ex);
			return null;
		}
	}

	public static string ReadFileContents(string fileLocation)
	{
		try
		{
			FileStream fileStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			StreamReader streamReader = new StreamReader(fileStream);
			string content = streamReader.ReadToEnd();
			fileStream.Close();
			return content;
		}
		catch (Exception ex)
		{
			Utility.ReportException("ReadFileContents " + fileLocation, ex);
			return null;
		}
	}

	public new static bool RenameFile(string oldName, string newName)
	{
		try
		{
			return BaseFileManager.RenameFile(oldName, newName);
		}
		catch (BaseFileManagerException ex)
		{
			Utility.ReportException("RenameFile " + oldName, ex);
			return false;
		}
	}

	public new static bool RenameFolder(string oldName, string newName)
	{
		try
		{
			return BaseFileManager.RenameFolder(oldName, newName);
		}
		catch (BaseFileManagerException ex)
		{
			Utility.ReportException("RenameFolder " + oldName, ex);
			return false;
		}
	}

	public static void TouchFile(string fileLocation)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(fileLocation);
			DateTime curDate = DateTime.Now;
			fileInfo.LastWriteTime = curDate;
		}
		catch (BaseFileManagerException ex)
		{
			Utility.ReportException("TouchFile " + fileLocation, ex);
		}
	}

	public static string WebAppFileLocationAbsolute(string folderLocationRelative, string fileName)
	{
		try
		{
			return Path.Combine(WebAppFolderLocationAbsolute(folderLocationRelative), fileName);
		}
		catch
		{
			// Handle the case where the file name contains illegal characters like ';#!--"<>=[]:{()}&.
			// We have only seen this happen with TrustWave scans where they pass junk on the query string.
			return string.Empty;
		}
	}

	public static string WebAppFolderLocationAbsolute(string folderLocationRelative)
	{
		string appPath = HttpContext.Current.Server.MapPath("~");
		return Path.Combine(appPath, folderLocationRelative);
	}
}
