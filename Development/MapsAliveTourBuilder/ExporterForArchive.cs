// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;

public class ExporterForArchive : Exporter
{
	private bool exportAllResources;
	private bool exportAsTemplate;
	private bool exportImagesOnly;
	private bool exportOriginalImageSizes;
	private bool exportResourcesOnly;
	private bool exportToZipFile;
	private bool exportToTempFolder;
	private string tempFolderLocation;
	private Tour tour;
	private string exportedFileLocation;
	private ZipOutputStream zipStream;

	
	public ExporterForArchive(Tour tour)
	{
		this.tour = tour;
	}

	private string CreateArchive()
	{
		string message = string.Empty;

		try
		{
			if (exportToTempFolder)
			{
				tempFolderLocation = Archive.CreateTempFolder(Utility.AccountId);
			}
			else if (exportToZipFile)
			{
				// Create the zip output stream. Set the compression level to 0 (no compression)
				// if exporting hotspot or map images because they are already compressed by virtue
				// of being jpg files. See compression high if the output is primarily XML.
				int compressionLevel = exportResourcesOnly ? 9 : 0;
				zipStream = new ZipOutputStream(File.Create(exportedFileLocation));
				zipStream.SetLevel(compressionLevel);
			}

			// Export all of the tour's map and hotspot images.
			ExportImages();

			if (!exportImagesOnly)
			{
				CreateXmlMemoryStreamAndSettings();

				// Simulataneously construct the archive.xml and write images to the archive zip or folder.
				using (xmlWriter = XmlWriter.Create(xmlMemoryStream, xmlWriterSettings))
				{
					CreateArchiveXml();
				}

				// Create the archive XML file.
				CopyXmlMemoryStreamToBytes();
				ExportFile(Archive.XmlFileName, xmlBytes);
				
				// Create the key file that the archive importer will read to verify that the XML has not changed
				// since it was exported. This way the importer is guaranteed to have good XML that a user has not
				// modified. If we ever allow users to create their own XML or edit ours, we'll have to utilize an
				// XML schema (.xsd file) plus some validation logic to ensure that we don't try to import bad data.
				string hash = Utility.Hash(xmlBytes);
				ExportFile(Archive.KeyFileName, System.Text.ASCIIEncoding.ASCII.GetBytes(hash));
				xmlMemoryStream.Close();
			}

			if (exportToZipFile)
			{
				zipStream.Finish();
				zipStream.Close();
			}
		}
		catch (Exception ex)
		{
			message = ex.Message;
		}
		finally
		{
		}

		return message;
	}

	public string CreateArchiveTempFolder(bool asTemplate)
	{
		exportToTempFolder = true;
		
		// When exporting a tour as a template, don't export any hotspots or their images.
		exportAsTemplate = asTemplate;
		
		return CreateArchive();
	}

	private void CreateArchiveXml()
	{
		xmlWriter.WriteStartDocument();
		
		EmitMapsAliveElement();
		EmitResources();

		if (!exportResourcesOnly)
		{
			EmitTourElement();

			if (!exportResourcesOnly)
			{
				xmlWriter.WriteStartElement("maps");

				foreach (TourPage tourPage in tour.TourPages)
				{
					EmitMapElement(tourPage);

					xmlWriter.WriteStartElement("hotspots");
					foreach (TourView tourView in tourPage.TourViews)
					{
						EmitHotspotElement(tourView);
					}
					xmlWriter.WriteEndElement(); // hotspots

					xmlWriter.WriteEndElement(); // map
				}

				xmlWriter.WriteEndElement(); // maps
			}

			xmlWriter.WriteEndElement(); // tour
		}
		xmlWriter.WriteEndElement(); // mapsAlive
		
		xmlWriter.WriteEndDocument();
		xmlWriter.Flush();
	}

	public string CreateArchiveZipFile(string fileLocation, bool exportOriginalImageSizes)
	{
		this.exportOriginalImageSizes = exportOriginalImageSizes;
		exportToZipFile = true;
		exportedFileLocation = fileLocation;
		return CreateArchive();
	}

	public string CreateImagesZipFile(string fileLocation)
	{
		exportImagesOnly = true;
		exportOriginalImageSizes = true;
		exportToZipFile = true;
		exportedFileLocation = fileLocation;
		return CreateArchive();
	}

	public string CreateResourcesZipFile(string fileLocation, bool exportAll)
	{
		exportResourcesOnly = true;
		exportAllResources = exportAll;
		exportToZipFile = true;
		exportedFileLocation = fileLocation;
		return CreateArchive();
	}

	private void EmitHotspotCategories(TourView tourView)
	{
		ArrayList list = tour.CategoryManager.GetCategories(tourView.Id);
		if (list.Count > 0)
		{
			xmlWriter.WriteStartElement("categories");
			string value = string.Empty;
			foreach (Category category in list)
			{
				EmitElement("category", category.Id);
			}
			xmlWriter.WriteEndElement(); // categories
		}
	}

	private void EmitHotspotElement(TourView tourView)
	{
		if (exportAsTemplate && !tourView.TourPage.IsDataSheet)
			return;

		xmlWriter.WriteStartElement("hotspot");

		EmitHotspotCategories(tourView);

		foreach (int tagId in Enum.GetValues(typeof(TourView.Tag)))
		{
			string tagName = Enum.GetName(typeof(TourView.Tag), tagId);
			EmitElement(tagName, tourView.GetTagValue(tagId));
		}
		
		xmlWriter.WriteEndElement(); // hotspot
	}

	private void EmitMapElement(TourPage tourPage)
	{
		xmlWriter.WriteStartElement("map");
		foreach (int tagId in Enum.GetValues(typeof(TourPage.Tag)))
		{
			string tagName = Enum.GetName(typeof(TourPage.Tag), tagId);
			
			if (exportAsTemplate && tagId == (int)TourPage.Tag.firstHotspotId)
			{
				// When hotspots are not being exported, there is no first hotspot.
				EmitElement(tagName, "0");
			}
			else
			{
				EmitElement(tagName, tourPage.GetTagValue(tagId));
			}
		}
	}

	private void EmitMapsAliveElement()
	{
		string comment = CreateTimeStampGmt();
		if (!App.DeveloperMode)
			comment += string.Format(" from account {0}.", Utility.AccountId);
		xmlWriter.WriteComment(comment);
		
		xmlWriter.WriteComment(string.Format("WARNING: Do not edit this file. A modified archive cannot be imported back into MapsAlive."));

		xmlWriter.WriteStartElement("mapsAlive");
		xmlWriter.WriteAttributeString("version", App.ArchiveVersion);
	}

	private void EmitResource<ResourceXmlType>(TourResourceType resourceType, string elementName)
	{
		xmlWriter.WriteStartElement(resourceType == TourResourceType.Category ? "categories" : elementName + "s");

		DataTable dataTable;
		if (exportAllResources)
		{
			// Get the Ids of all resources of this type in the account.
			string sp = string.Format("sp_{0}_Get{0}sOwnedByAccount", resourceType.ToString());
			dataTable = MapsAliveDatabase.LoadDataTable(sp, "@AccountId", MapsAliveState.Account.Id);
		}
		else
		{
			// Get the Ids of all resources of this type used by this tour.
			string sp = string.Format("sp_{0}_Get{0}sThatTourUses", resourceType.ToString());
			dataTable = MapsAliveDatabase.LoadDataTable(sp, "@TourId", tour.Id);
		}

		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			string resourceIdName = string.Format("{0}Id", resourceType.ToString());
			int resourceId = row.IntValue(resourceIdName);

			if (resourceType == TourResourceType.Symbol && resourceId == 0)
			{
				// Ignore the "No Symbol" symbol.
				continue;
			}

			TourResource resource = TourResourceManager.CreateNewResource(resourceType, resourceId);

			xmlWriter.WriteStartElement(elementName);

			foreach (int tagId in Enum.GetValues(typeof(ResourceXmlType)))
			{
				string tagName = Enum.GetName(typeof(ResourceXmlType), tagId);
				EmitElement(tagName, resource.GetTagValue(tagId));
			}

			if (resourceType == TourResourceType.Symbol)
			{
				ExportSymbolImage(row);
			}

			xmlWriter.WriteEndElement();
		}

		xmlWriter.WriteEndElement();
	}

	private void EmitResources()
	{
		xmlWriter.WriteStartElement("resources");

		EmitResource<Category.Tag>(TourResourceType.Category, "category");
		EmitResource<ColorScheme.Tag>(TourResourceType.TourStyle, "colorScheme");
		EmitResource<FontStyleResource.Tag>(TourResourceType.FontStyle, "fontStyle");
		EmitResource<MarkerStyle.Tag>(TourResourceType.MarkerStyle, "markerStyle");
		EmitResource<Symbol.Tag>(TourResourceType.Symbol, "symbol");
		EmitResource<TooltipStyle.Tag>(TourResourceType.TooltipStyle, "tooltipStyle");
		EmitResource<Marker.Tag>(TourResourceType.Marker, "marker");

		xmlWriter.WriteEndElement(); // resources
	}

	private void EmitTourElement()
	{
		xmlWriter.WriteStartElement("tour");

		foreach (int tagId in Enum.GetValues(typeof(Tour.Tag)))
		{
			string tagName = Enum.GetName(typeof(Tour.Tag), tagId);
			EmitElement(tagName, tour.GetTagValue(tagId));
		}

		xmlWriter.WriteStartElement("directory");
		foreach (int tagId in Enum.GetValues(typeof(TourDirectory.Tag)))
		{
			string tagName = Enum.GetName(typeof(TourDirectory.Tag), tagId);
			EmitElement(tagName, tour.Directory.GetTagValue(tagId));
		}
		xmlWriter.WriteEndElement();
	}

	private void ExportBannerImage(Tour tour)
	{
		if (exportResourcesOnly || !tour.HasBanner || !tour.Banner.Image.HasFile)
			return;

		byte[] bytes = tour.Banner.Image.Bytes;

		if (!exportOriginalImageSizes)
			bytes = ScaledImageBytes(ref bytes, tour.Banner.Size);

		if (bytes != null)
			ExportFile(Archive.BannerImageFileName(), bytes);
	}

	private void ExportFile(string fileName, byte[] bytes)
	{
		int length = bytes.Length;

		if (exportToZipFile)
		{
			ZipEntry entry = new ZipEntry(fileName);

			// Set the entry's size to prevent SharpZipLib from using Zip64 which can't be read on XP.
			// For more info see: http://blog.tylerholmes.com/2008/12/windows-xp-unzip-errors-with.html.
			entry.Size = bytes.Length;
			
			zipStream.PutNextEntry(entry);
			zipStream.Write(bytes, 0, length);
		}
		else if (exportToTempFolder)
		{
			string fileLocation = Path.Combine(tempFolderLocation, fileName);
			CreateFile(fileLocation, bytes);
		}
		else
		{
			CreateFile(exportedFileLocation, bytes);
		}
	}

	private void ExportHotspotImage(TourView tourView)
	{
		if (exportResourcesOnly || !tourView.HasImage)
			return;

		if (exportAsTemplate && !tourView.TourPage.IsDataSheet)
			return;

		byte[] bytes = tourView.Image.Bytes;

		if (!exportOriginalImageSizes)
		{
			Size size = tourView.GetImageContainerSize();
			if (!size.IsEmpty)
			{
				// We can only scale the image if the container has a size. One case where there will not
				// be a size is when the tour uses photo markers, but the template does not show a photo.
				bytes = ScaledImageBytes(ref bytes, size);
			}
		}

		if (bytes != null)
			ExportFile(Archive.HotspotImageFileName(tourView), bytes);
	}

	private void ExportImages()
	{
		ExportBannerImage(tour);

		foreach (TourPage tourPage in tour.TourPages)
		{
			ExportMapImage(tourPage);

			foreach (TourView tourView in tourPage.TourViews)
			{
				ExportHotspotImage(tourView);
			}
		}
	}

	private void ExportMapImage(TourPage tourPage)
	{
		if (exportResourcesOnly || tourPage.IsDataSheet || !tourPage.MapImage.HasFile)
			return;

		// Note that we have to export the full sized map image in case the map
		// has exclusive shape markers that were drawn for the original map size.
		byte[] bytes = tourPage.MapImage.Bytes;

		if (bytes != null)
		{
			string fileName = Archive.MapImageFileName(tourPage);
			ExportFile(fileName, bytes);
		}
	}

	private static byte[] ScaledImageBytes(ref byte[] bytes, Size containerSize)
	{
		if (!Utility.HasWidthAndHeight(containerSize))
			return null;
		return Utility.ScaledImageBytes(ref bytes, containerSize);
	}

	private void ExportSymbolImage(MapsAliveDataRow row)
	{
		int symbolId = row.IntValue("SymbolId");
		Symbol symbol = Account.GetCachedSymbol(symbolId);
		string fileName = Archive.SymbolImageFileName(symbol);
		ExportFile(fileName, symbol.Bytes);
	}
}
