// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;

public class ImporterForHotspotPhotos : ImporterForHotspots
{
	private Bitmap bitmap;
	private string fileName;
	private string ext;
	private byte[] imageBytes;
	private string rejectedMessage;
	private int templateMarkerId;
	private double totalBytes;
	private double totalBytesRead;
	private ZipEntry zipEntry;
	private ZipInputStream zipStream;

	public ImporterForHotspotPhotos(TourPage tourPage, Stream stream, string reportTitle)
		: base(tourPage.Tour, tourPage, stream, reportTitle)
	{
	}

	public void ImportImageFiles(int templateMarkerId)
	{
		this.templateMarkerId = templateMarkerId;

		try
		{
			zipStream = new ZipInputStream(stream);
			totalBytes = stream.Length;
			totalBytesRead = 0;

			while ((zipEntry = zipStream.GetNextEntry()) != null)
			{
				if (!OkToKeepImporting)
					break;

				string fileNamePrefix = GetFileNamePrefix();

				if (!IsImportableFile(ext))
					continue;

				if (!GetImageBytes())
					continue;

				CreateOrUpdateTourView(fileName, fileNamePrefix);

				ProgressMonitor.Update(zipEntry.Name, totalBytes, totalBytesRead);
			}

			tourPage.RebuildMap();
		}
		catch (Exception ex)
		{
			string error = ex.Message;
			if (error == "No password set.")
				error = "The zip file is password protected";
			message = string.Format("Import failed: {0}", error);
			importFailed = true;
		}
		finally
		{
			zipStream.Close();
		}
	}

	private byte[] ConvertFileToImageBytes()
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] buffer = new byte[2048];
		int bytesRead = 0;

		// Read the data a block at a time and write it to a memory stream.
		while ((bytesRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
		{
			memoryStream.Write(buffer, 0, bytesRead);
			totalBytesRead += bytesRead;
		}

		memoryStream.Position = 0;
		return memoryStream.GetBuffer();
	}

	private void CreateOrUpdateTourView(string fileName, string fileNamePrefix)
	{
		// Determine if an existing slide has the file's name as its slide Id.
		TourView tourView = GetTourViewBySlideId(tourPage, fileNamePrefix, string.Empty);

		if (tourView == null)
		{
			// No slide has this file as its key. Create a new slide.
			tourView = CreateSlide(fileNamePrefix, fileNamePrefix, templateMarkerId);
			tourView.ImageUploaded(fileName, bitmap.Size, imageBytes);
			tourView.UpdateDatabase(false);
			report.EmitRow(ImportReport.Topic.SlideImported, tourView.Title);
		}
		else
		{
			// An existing slide's key matches the file's name.
			if (tourView.HasImage && UploadedImageIsSameAsTourViewImage(tourView, imageBytes))
			{
				// The image file is identical to the existing slide's image.
				report.EmitRow(ImportReport.Topic.SlideUnchanged, tourView.Title);
			}
			else
			{
				// The image file is different than the existing slide's image.
				tourView.ImageUploaded(fileName, bitmap.Size, imageBytes);
				tourView.UpdateDatabase(false);
				report.EmitRow(ImportReport.Topic.SlideUpdated, tourView.Title);
			}
		}
	}

	private string GetFileNamePrefix()
	{
		// Extract just the file's name ignoring its path and extension.
		FileInfo fileInfo = new FileInfo(zipEntry.Name);
		fileName = fileInfo.Name;
		ext = fileInfo.Extension;
		int extLength = ext.Length;
		string fileNamePrefix = fileName.Substring(0, fileName.Length - extLength);
		return fileNamePrefix;
	}

	private bool GetImageBytes()
	{
		// Get the image data as an array of bytes and attempt to convert them into an image.
		imageBytes = ConvertFileToImageBytes();
		bitmap = Utility.BitmapFromBytes(imageBytes, out rejectedMessage);
		if (bitmap == null)
		{
			// The data is not an image.
			report.EmitRow(ImportReport.Topic.ImageFileRejected, fileName, rejectedMessage);
			imageBytes = null;
			return false;
		}

		// Check the image size and reduce it if necessary.
		Size maxSize = Utility.MaxImageSizeForMapPage;
		Size imageSize = bitmap.Size;
		bool imageSizeChanged = false;
		if (imageSize.Width > maxSize.Width || imageSize.Height > maxSize.Height)
		{
			imageSize = maxSize;
			Bitmap scaledBitmap = Utility.ScaledBitmap(bitmap, imageSize, true);
			bitmap = scaledBitmap;
			imageSizeChanged = true;
		}

		// Convert scaled and non-jpeg bitmaps back into bytes.
		if (imageSizeChanged || Utility.BitmapFormat(bitmap) != "jpeg")
			imageBytes = Utility.BytesFromUploadedBitmap(bitmap, ImageFormat.Jpeg, ref imageSize);

		return true;
	}

	private bool IsImportableFile(string ext)
	{
		if (!zipEntry.IsFile)
			return false;

		// Ignore certain types of files that we know for certain cannot be imported.
		ext = ext.ToLower();
		if (ext == ".ds_store" || ext == ".plist" || ext == ".db")
			return false;

		return true;
	}

	private static bool UploadedImageIsSameAsTourViewImage(TourView tourView, byte[] imageBytes)
	{
		bool identical = false;
		byte[] originalBytes = tourView.Image.Bytes;

		if (imageBytes.Length == originalBytes.Length)
		{
			identical = true;
			for (int i = 0; i < imageBytes.Length; i++)
			{
				if (imageBytes[i] != originalBytes[i])
				{
					identical = false;
					break;
				}
			}
		}
		return identical;
	}
}
