// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.IO;

class Archive
{
	public static string BannerImageFileName()
	{
		return string.Format("banner");
	}

	public static string CreateTempFolder(int accountId)
	{
		// Create a temporary folder on disk to extract the zip contents into.
		// Ideally we could simply read and process the files in memory, but
		// we can't do it that way because we must process the archive.xml file
		// first and then the images. Since the files could be in any sequence
		// within the zip, we need to unzip all the files first. Note that we
		// could loop over the stream to find the archive.xml file while skipping
		// the images, but we can't start again at the beginning to get the images
		// because seek is not supported on the stream.
		string folderLocation = TempFolderLocation(accountId);
		if (FileManager.FolderExists(folderLocation))
			FileManager.DeleteFolderContents(folderLocation);
		else
			FileManager.CreateFolder(folderLocation);

		return folderLocation;
	}

	public static void DeleteTempFolder(int accountId)
	{
		FileManager.DeleteFolder(TempFolderLocation(accountId));
	}

	public static string HotspotImageFileName(TourView tourView)
	{
		return string.Format("{0}_{1}.jpg", tourView.TourPage.PageId, tourView.SlideId);
	}

	public static string KeyFileName
	{
		get { return "key"; }
	}

	public static string MapImageFileName(TourPage tourPage)
	{
		return string.Format("{0}.jpg", tourPage.PageId);
	}

	public static string SymbolImageFileName(Symbol symbol)
	{
		return string.Format("{0}", symbol.Id);
	}

	public static string TempFolderLocation(int accountId)
	{
		return Path.Combine(App.TourFolderLocationAbsolute, string.Format("TEMP{0}", accountId));
	}

	public static string XmlFileName
	{
		get { return "archive.xml"; }
	}
	
	public static string XmlSchemaFileName
	{
		get { return "archive.xsd"; }
	}
}
