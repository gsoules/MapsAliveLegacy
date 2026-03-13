// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseFileManagerException : ApplicationException
	{
		public BaseFileManagerException(string message, Exception ex) : base(message, ex) { }
	}

	public class BaseFileManager
	{
		protected BaseFileDescriptors fileDescriptors;

		public BaseFileManager()
		{
			fileDescriptors = new BaseFileDescriptors();
		}

		public virtual void Release()
		{
			fileDescriptors.Release();
			fileDescriptors = null;
		}

		#region ===== Properties ========================================================
		#endregion

		#region ===== Public ============================================================

		public BaseFileDescriptor AddFileDescriptor(string fileLocation)
		{
			BaseFileDescriptor fileDescriptor = new BaseFileDescriptor(NewId(), fileLocation);
			fileDescriptors.Add(fileDescriptor);
			return fileDescriptor;
		}

		public static bool CopyFile(string sourceFileLocation, string targetFileLocation)
		{
			Exception exception;
			try
			{
				Debug.Assert(sourceFileLocation != null & sourceFileLocation != string.Empty, "Null or empty source file location passed to CopyFile");
				Debug.Assert(targetFileLocation != null && targetFileLocation != string.Empty, "Null or empty target file location passed to CopyFile");

				FileInfo fileInfo = new FileInfo(sourceFileLocation);
				fileInfo.CopyTo(targetFileLocation, true);
				fileInfo = new FileInfo(targetFileLocation);

				// Make sure that files copied to the directory are not read-only.
				fileInfo.Attributes &= ~FileAttributes.ReadOnly;

				return true;
			}
			catch (IOException ex)
			{
				exception = ex;
			}
			catch (UnauthorizedAccessException ex)
			{
				exception = ex;
			}
			catch (NotSupportedException ex)
			{
				exception = ex;
			}
			throw new BaseFileManagerException(exception.Message, exception);
		}

		public static bool CopyFolder(string sourceDir, string targetDir)
		{
			Exception exception;
			try
			{
				if (!Directory.Exists(sourceDir))
					return false;

				if (targetDir[targetDir.Length - 1] != Path.DirectorySeparatorChar)
					targetDir += Path.DirectorySeparatorChar;

				if (!Directory.Exists(targetDir))
					Directory.CreateDirectory(targetDir);

				string[] Files = FolderEntries(sourceDir);

				foreach (string element in Files)
				{
					if (Directory.Exists(element))
					{
						// The element is a directory.  Recursively copy it.
						if (!CopyFolder(element, targetDir + Path.GetFileName(element)))
							return false;
					}
					else
					{
						// The element is a file.  Copy it.
						File.Copy(element, targetDir + Path.GetFileName(element), true);
					}
				}
				return true;
			}
			catch (IOException ex)
			{
				exception = ex;
			}
			catch (UnauthorizedAccessException ex)
			{
				exception = ex;
			}
			catch (System.NotSupportedException ex)
			{
				exception = ex;
			}
			throw new BaseFileManagerException(exception.Message, exception);
		}

		public static bool CreateFolder(string folderLocation)
		{
			Exception exception;
			try
			{
				DirectoryInfo dirInfo = new DirectoryInfo(folderLocation);
				if (!dirInfo.Exists)
					dirInfo.Create();
				return true;
			}
			catch (IOException ex)
			{
				exception = ex;
			}
			catch (UnauthorizedAccessException ex)
			{
				exception = ex;
			}
			throw new BaseFileManagerException(exception.Message, exception);
		}

		public static bool CreateHtmlFile(string fileLocation, string contents, bool includeBom)
		{
			// This method writes HTML file in UTF-8 format.  We use UTF-8 to preserve any unicode
			// characters that might be in the page.  Normally .NET writes a 3 byte BOM (Byte Order Mark)
			// at the beginning of a UTF-8 file, but this causes problems for some browsers.  Browsers
			// that are not BOM-aware display an extra line at the top of the page containing a unicode FEFF character.
			// See this link for more info: http://www.w3.org/International/questions/qa-utf8-bom.
			// Through trial and error we discovered that we avoid browser problems by having MapsAlive
			// emit its HTML files without the BOM; however, we also discovered that we need the BOM
			// in the project design file to help the MapsAlive embedded browser know to deal with
			// unicode characters if they exist.
			//
			// Without the BOM, a design file that does not specify its charset as utf-8, but contains
			// unicode, causes the embedded browser to display each unicode character to two funny
			// characters; however, within the outer html, these funny characters are apparently unicode.
			// When we when save the outer html as the project design file in UTF-8 format, each of the
			// two funny characters gets written as two unicode bytes.  The next time the file is loaded
			// into the browser, we then see four funny character for each of the original unicode characters.
			// This extension of the original unicode bytes into funny characters continues each time the
			// file is written again and later loaded back into the browser.  Although this is an obscure
			// case (because an HTML file's Meta tag charset attribute should say if the file contains unicode)
			// we still have to deal with it.  We do so by always writing the BOM to the project design file.
			// That way, the browser always interprets unicode byte pairs as unicode characters.
			// 
			// This method provides the includeBom parameter to let the caller decide if they want the
			// BOM written to the output file location.  This way we can use the BOM internally to tell
			// the embedded browser that the project design file can contain unicode, and not write the
			// BOM to files read from (the design file) or written to (site files) the outside world.
			
			Exception exception;
			try
			{
				UTF8Encoding utf8Encoding = new UTF8Encoding(includeBom);
				StreamWriter streamWriter = new StreamWriter(fileLocation, false, utf8Encoding);
				streamWriter.Write(contents);
				streamWriter.Close();
				return true;
			}
			catch (IOException ex)
			{
				exception = ex;
			}
			catch (UnauthorizedAccessException ex)
			{
				exception = ex;
			}
			catch (System.NotSupportedException ex)
			{
				exception = ex;
			}
			throw new BaseFileManagerException(exception.Message, exception);
		}

		public static bool DeleteFile(string fileLocation)
		{
			Exception exception;
			try
			{
				FileInfo fileInfo = new FileInfo(fileLocation);
				if (fileInfo.Exists)
				{
					fileInfo.Delete();
				}
				return true;
			}
			catch (IOException ex)
			{
				exception = ex;
			}
			catch (UnauthorizedAccessException ex)
			{
				exception = ex;
			}
			catch (System.NotSupportedException ex)
			{
				exception = ex;
			}
			throw new BaseFileManagerException(exception.Message, exception);
		}

		public static bool DeleteFolder(string folderName)
		{
			Exception exception;
			try
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(folderName);
				if (directoryInfo.Exists)
				{
					directoryInfo.Delete(true);
				}
				return true;
			}
			catch (IOException ex)
			{
				exception = ex;
			}
			catch (UnauthorizedAccessException ex)
			{
				exception = ex;
			}
			catch (System.NotSupportedException ex)
			{
				exception = ex;
			}
			throw new BaseFileManagerException(exception.Message, exception);
		}

		public static long FileBytes(string fileLocation)
		{
			try
			{
				return new FileInfo(fileLocation).Length;
			}
			catch
			{
				return 0;
			}
		}

		public static string[] FolderEntries(string folderLocation)
		{
			Exception exception;
			string[] fileLocations;
			try
			{
				fileLocations = Directory.GetFileSystemEntries(folderLocation);
				return fileLocations;
			}
			catch (SystemException ex)
			{
				exception = ex;
			}
			throw new BaseFileManagerException(exception.Message, exception);
		}

		public static string[] FolderEntries(string folderLocation, string pattern)
		{
			Exception exception;
			string[] fileLocations;
			try
			{
				fileLocations = Directory.GetFileSystemEntries(folderLocation, pattern);
				return fileLocations;
			}
			catch (SystemException ex)
			{
				exception = ex;
			}
			throw new BaseFileManagerException(exception.Message, exception);
		}

		public BaseFileDescriptor GetFileDescriptorByFileLocation(string fileLocation)
		{
			return fileDescriptors.GetFileDescriptorByFileLocation(fileLocation);
		}

		public BaseFileDescriptor GetFileDescriptorById(int id)
		{
			return fileDescriptors.GetFileDescriptorById(id);
		}

		public static long LastWriteTimeInTicks(string fileLocation)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(fileLocation);
				return fileInfo.LastWriteTime.Ticks;
			}
			catch
			{
				return 0;
			}
		}

		public int NewId()
		{
			return fileDescriptors.NewId();
		}

		public static bool RenameFile(string oldName, string newName)
		{
			Exception exception;
			try
			{
				FileInfo fileInfo = new FileInfo(oldName);
				fileInfo.MoveTo(newName);
				return true;
			}
			catch (IOException ex)
			{
				exception = ex;
			}
			catch (UnauthorizedAccessException ex)
			{
				exception = ex;
			}
			catch (System.NotSupportedException ex)
			{
				exception = ex;
			}
			throw new BaseFileManagerException(exception.Message, exception);
		}

		public static bool RenameFolder(string oldName, string newName)
		{
			Exception exception;
			try
			{
				DirectoryInfo dirInfo = new DirectoryInfo(oldName);
				dirInfo.MoveTo(newName);
				return true;
			}
			catch (IOException ex)
			{
				exception = ex;
			}
			catch (UnauthorizedAccessException ex)
			{
				exception = ex;
			}
			catch (System.NotSupportedException ex)
			{
				exception = ex;
			}
			throw new BaseFileManagerException(exception.Message, exception);
		}
		#endregion
	}
}
