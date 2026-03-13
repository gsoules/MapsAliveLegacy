// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using ICSharpCode.SharpZipLib.Zip;

public class ImporterForArchiveXml : Importer
{
	private Account account;
	private bool autoLayoutEnabled;
	private bool duplicatingTour;
	private bool importedNewTour;
	private bool importingResourcesForNewAccount;
	private int numberOfImportStepsCompleted;
	private Hashtable restoredCategories;
	private Hashtable restoredColorSchemes;
	private Hashtable restoredFontStyles;
	private Hashtable restoredMarkerStyles;
	private Hashtable restoredMarkers;
	private Hashtable restoredSymbols;
	private Hashtable restoredTooltipStyles;
	private Hashtable restoredTourPages;
	private Hashtable restoredTourViews;
	private bool schemaValidationFailed;
	private int totalImportSteps;
	private string tempFolderLocation;

	public ImporterForArchiveXml(Stream stream, string reportTitle)
		: base(null, null, stream, reportTitle)
	{
		restoredCategories = new Hashtable();
		restoredColorSchemes = new Hashtable();
		restoredFontStyles = new Hashtable();
		restoredMarkers = new Hashtable();
		restoredMarkerStyles = new Hashtable();
		restoredSymbols = new Hashtable();
		restoredTooltipStyles = new Hashtable();
		restoredTourPages = new Hashtable();
		restoredTourViews = new Hashtable();
	}

	private XmlReaderSettings CreateXmlReaderSettings()
	{
		XmlReaderSettings settings = new XmlReaderSettings();
		settings.IgnoreComments = true;
		settings.IgnoreWhitespace = true;
		settings.IgnoreProcessingInstructions = true;
		SetSchema(settings);
		return settings;
	}

	private void ExtractArchiveZipIntoTempFolder(ZipInputStream zipStream, string fileLocation)
	{
		using (FileStream fileStream = new FileStream(fileLocation, FileMode.Create, FileAccess.Write))
		{
			byte[] buffer = new byte[2048];
			int bytesRead = 0;

			// Read the data a block at a time and write it to a memory stream.
			while ((bytesRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
			{
				fileStream.Write(buffer, 0, bytesRead);
			}
		}
	}

	private void FixupReferencesToOtherResources(TourResource resource)
	{
		// Find other resources that this resource references. Locate the referenced resource in this
		// account and update this resource to use the referenced resources Id. Until we do this fixup,
		// this resource uses an Id in the account that this resource was archived from. We are able to
		// do this fixup because we always import non-dependent resources like font styles before
		// importing dependent resources like tooltip styles.

		if (resource.ResourceType == TourResourceType.TooltipStyle)
		{
			TooltipStyle tooltipStyle = (TooltipStyle)resource;
			FontStyleResource fontStyle = (FontStyleResource)restoredFontStyles[tooltipStyle.FontStyleResourceId];
			report.Trace(string.Format("{0} (TooltipStyle {1}): fixed up FontStyle {2} with {3}", tooltipStyle.Name, tooltipStyle.Id, tooltipStyle.FontStyleResourceId, fontStyle.Id));
			tooltipStyle.FontStyleResourceId = fontStyle.Id;
		}
		else if (resource.ResourceType == TourResourceType.Marker)
		{
			Marker marker = (Marker)resource;

			bool hasSymbol = marker.MarkerType == MarkerType.Symbol || marker.MarkerType == MarkerType.SymbolAndShape;
			bool hasShape = marker.MarkerType != MarkerType.Symbol;
			bool isTextMarker = marker.MarkerType == MarkerType.Text;
			bool isPhotoMarkerWithText = marker.MarkerType == MarkerType.Photo && marker.PhotoCaptionPosition != PhotoCaptionPositionType.None;

			if (hasSymbol)
			{
				if (marker.NormalSymbolId > 0)
				{
					Symbol normalSymbol = (Symbol)restoredSymbols[marker.NormalSymbolId];
					report.Trace(string.Format("{0} (Marker {1}): fixed up normal Symbol {2} with {3}", marker.Name, marker.Id, marker.NormalSymbolId, normalSymbol.Id));
					marker.NormalSymbolId = normalSymbol.Id;
				}
				if (marker.SelectedSymbolId > 0)
				{
					Symbol selectedSymbol = (Symbol)restoredSymbols[marker.SelectedSymbolId];
					report.Trace(string.Format("{0} (Marker {1}): fixed up selected Symbol {2} with {3}", marker.Name, marker.Id, marker.SelectedSymbolId, selectedSymbol.Id));
					marker.SelectedSymbolId = selectedSymbol.Id;
				}
			}

			if (hasShape)
			{
				MarkerStyle markerStyle = (MarkerStyle)restoredMarkerStyles[marker.MarkerStyleId];
				report.Trace(string.Format("{0} (Marker {1}): fixed up MarkerStyle {2} with {3}", marker.Name, marker.Id, marker.MarkerStyleId, markerStyle.Id));
				marker.MarkerStyleId = markerStyle.Id;
			}

			if (isTextMarker || isPhotoMarkerWithText)
			{
				FontStyleResource fontStyle = (FontStyleResource)restoredFontStyles[marker.FontStyleResourceId];
				report.Trace(string.Format("{0} (Marker {1}): fixed up FontStyle {2} with {3}", marker.Name, marker.Id, marker.FontStyleResourceId, fontStyle.Id));
				marker.FontStyleResourceId = fontStyle.Id;
			}
		}
	}

	private void FixupRoutesXml(TourPage tourPage)
	{
		if (tourPage.RoutesXml.Length > 0)
		{
			XmlDocument xmlDocument = Routes.FixupRoutesXml(tourPage.RoutesXml, GetRestoredTourViewId);
			tourPage.RoutesXml = xmlDocument.OuterXml;
		}
	}

	private void FixupTourViewClickActionTarget(TourView tourView)
	{
		if (tourView.MarkerClickAction == MarkerAction.GotoPage)
		{
			int originalPageId = int.Parse(tourView.MarkerClickActionTarget);
			TourPage targetPage = (TourPage)restoredTourPages[originalPageId];
			tourView.MarkerClickActionTarget = targetPage.Id.ToString();
		}
	}

	private void FixupTourViewMarkerId(TourView tourView)
	{
		// Fixup the tour view's reference to its marker.
		int originalMarkerId = tourView.MarkerId;
		if (originalMarkerId > 0)
		{
			Marker marker = (Marker)restoredMarkers[originalMarkerId];
			tourView.MarkerId = marker.Id;

			if (marker.IsExclusive)
			{
				marker.MakeExclusive(tourView);
			}
		}
	}

	public string GetRestoredTourViewId(int tourViewId)
	{
		TourView hotspot = (TourView)restoredTourViews[tourViewId];
		if (hotspot == null || hotspot.MarkerIsRoute)
			return null;
		else
			return hotspot.Id.ToString();
	}

	public void ImportArchiveFromStream(Account account)
	{
		// Get the account being imported into. We have to pass the account in so that we can import
		// into a newly created account that is not yet accessible via MapsAliveState.Account.
		this.account = account;

		ZipEntry zipEntry;
		ZipInputStream zipStream = null;
		
		try
		{
			tempFolderLocation = Archive.CreateTempFolder(account.Id);
			
			zipStream = new ZipInputStream(stream);
			while ((zipEntry = zipStream.GetNextEntry()) != null)
			{
				if (!OkToKeepImporting)
					break;

				if (!zipEntry.IsFile)
					return;

				// Extract the file into the temp folder.
				ExtractArchiveZipIntoTempFolder(zipStream, Path.Combine(tempFolderLocation, zipEntry.Name));
			}

			if (!importFailed)
			{
				ImportArchiveFromTempFolder(account, false);
			}
		}
		catch (Exception ex)
		{
			string error = ex.Message;
			if (error == "No password set.")
				error = "The zip file is password protected";
			else
				error = "MapsAlive does not recognize the file as an archive";
			message = string.Format("Import failed: {0}", error);
			importFailed = true;
			Archive.DeleteTempFolder(account.Id);
		}
		finally
		{
			zipStream.Close();
		}
	}

	public void ImportArchiveFromTempFolder(Account account, bool duplicatingTour)
	{
		this.duplicatingTour = duplicatingTour;

		try
		{
			// We set the account and temp folder here even though they are set in ImportArchiveFromStream
			// because this method can be called directly as is the case when a tour is duplicated.
			this.account = account;
			tempFolderLocation = Archive.TempFolderLocation(account.Id);

			XmlReaderSettings settings = CreateXmlReaderSettings();
			string archiveFileLocation = Path.Combine(tempFolderLocation, Archive.XmlFileName);

			// Read the key file to verify that the XML being imported has not been changed since it was exported.
			if (!ValidateArchive(archiveFileLocation))
			{
				importFailed = true;
				return;
			}

			using (XmlReader reader = XmlReader.Create(archiveFileLocation, settings))
			{
				// Read the archive XML and validate it against the schema.
				XPathDocument xPathDocument = new XPathDocument(reader);
				if (schemaValidationFailed)
				{
					message = "The imported XML does not conform to the schema for a MapsAlive archive file";
					importFailed = true;
					return;
				}

				// Create a navigator and move to the root of the archive XML.
				XPathNavigator navigator = xPathDocument.CreateNavigator();
				navigator.MoveToRoot();

				// Determine if this archive contains a tour (a resources-only archive has no tour).
				XPathNavigator tourNavigator = navigator.SelectSingleNode("/mapsAlive/tour");
				if (tourNavigator != null)
				{
					// Get a count of the hotspots in the tour to use as a measure of progress.
					// Use double the count for the number of import steps because each hotspot
					// is imported twice, once in pass 2 and again in pass 3. Note that we don't
					// include pass 1 in the progress only because it's simpler not to and
					// because most of the work is in importing the hot spots.
					totalImportSteps = tourNavigator.Select("maps/map/hotspots/hotspot").Count;
					totalImportSteps *= 2;
				}
				
				// Count the number of resource restore steps (add in 1 for the color scheme).
				totalImportSteps += 1;
				totalImportSteps += navigator.Select("/mapsAlive/resources/fontStyles/fontStyle").Count;
				totalImportSteps += navigator.Select("/mapsAlive/resources/markerStyles/markerStyle").Count;
				totalImportSteps += navigator.Select("/mapsAlive/resources/symbols/symbol").Count;
				totalImportSteps += navigator.Select("/mapsAlive/resources/tooltipStyles/tooltipStyle").Count;
				totalImportSteps += navigator.Select("/mapsAlive/resources/markers/marker").Count;

				// Restore resources.
				XPathNavigator resourcesNode = navigator.SelectSingleNode("/mapsAlive/resources");
				try
				{
					RestoreTourFromArchivePass1(resourcesNode);
				}
				catch (Exception ex)
				{
					message = ex.Message;
					importFailed = true;
					return;
				} 

				if (tourNavigator != null)
				{
					// Import the hotspots.
					RestoreTourFromArchivePass2(tourNavigator);
					RestoreTourFromArchivePass3(tourNavigator);
					importedNewTour = true;
				}
			}
		}
		catch (Exception ex)
		{
			message = (duplicatingTour ? "Duplication" : "Import") + " failed. Please report this problem to support@mapsalive.com";
			importFailed = true;
			
			string msg = string.Format("An exception occurred while importing tour #{0}.\n\nException: {1}", tour.Id, ex.Message);
			string subject = "Import Failed";
			Utility.ReportError(subject, msg);
			
			if (tour != null)
				tour.Delete();
		}
		finally
		{
			Archive.DeleteTempFolder(account.Id);
		}
	}

	public bool ImportedNewTour
	{
		get { return importedNewTour; }
	}

	private void InitializeTourPagesAndTourViews(XPathNavigator navigator)
	{
		// Restore all TourPage and TourView fields from the archive XML.
		foreach (TourPage tourPage in tour.TourPages)
		{
			XPathNavigator mapNode = navigator.SelectSingleNode(string.Format("maps/map[pageId='{0}']", tourPage.PageId));
			tourPage.InitializeTourPageFromDataRecord(new MapsAliveDataRecordXml(mapNode));

			// Fixup the tour page's tooltip.
			if (tourPage.IsDataSheet)
			{
				tourPage.TooltipStyleId = 0;
			}
			else
			{
				TooltipStyle tooltipStyle = (TooltipStyle)restoredTooltipStyles[tourPage.TooltipStyleId];
				tourPage.TooltipStyleId = tooltipStyle.Id;
			}

			// Restore the map.
			RestoreMapImage(tourPage);

			// Fixup the tourPage's reference to its first tour view. The first
			// tour view should only be null if the tour page has no tour views.
			// Note that a data sheet's only tour view is also its first tour view.
			if (tourPage.IsDataSheet)
			{
				tourPage.SetFirstTourView(((TourView)tourPage.TourViews[0]).Id);
			}
			TourView firstTourView = (TourView)restoredTourViews[tourPage.FirstTourViewId];
			if (firstTourView != null)
			{
				tourPage.FirstTourViewId = firstTourView.Id;
				tourPage.UpdateDatabaseFirstTourView();
			}

			// Fixup the routes XML so that the waypoint hotspots refer to the restored hotspots.
			FixupRoutesXml(tourPage);

			// Save the fixups.
			tourPage.UpdateDatabase();

			foreach (TourView tourView in tourPage.TourViews)
			{
				// Get the node for this tour view. Note that we use double quotes around the Id value in
				// case the slide Id contains a single quote (double quotes are not allowed in slide Ids).
				string xPath = string.Format("hotspots/hotspot[hotspotId=\"{0}\"]", tourView.SlideId);
				XPathNavigator hotspotNode = mapNode.SelectSingleNode(xPath);
				
				tourView.InitializeTourViewFromDataRecord(new MapsAliveDataRecordXml(hotspotNode));
				report.EmitRow(ImportReport.Topic.SlideImported, tourView.Title);

				FixupTourViewMarkerId(tourView);
				FixupTourViewClickActionTarget(tourView);

				tourView.UpdateDatabase();

				ProgressMonitor.Update(tourView.Title, totalImportSteps, ++numberOfImportStepsCompleted);
			}
		}
	}

	private string OptionalNodeStringValue(XPathNavigator parentNode, string xPath)
	{
		// Call this method when the archive is not required to contain the requested node.
		// This would be the case for new tags that won't exist in older archives.
		XPathNavigator node = parentNode.SelectSingleNode(xPath);
		return node == null ? string.Empty : node.Value;
	}

	private void RestoreBannerImage()
	{
		string fileName = Archive.BannerImageFileName();
		string fileLocation = Path.Combine(tempFolderLocation, fileName);
		if (FileManager.FileExists(fileLocation))
		{
			Size size;
			Byte[] imageBytes = Utility.ImageFileToByteArray(fileLocation, out size);
			tour.Banner.ImageUploaded(fileName, size, imageBytes);
		}
	}

	private void RestoreDataSheetPlaceholder(TourPage tourPage, int originalTourViewId)
	{
		TourView tourView = tour.CreateNewTourViewForDataSheet(tourPage);
		tour.AddTourView(tourView, true);
		restoredTourViews.Add(originalTourViewId, tourView);
		
		// Get the data sheet's image.
		RestoreHotspotImage(tourView);
	}

	public bool ImportingResourcesForNewAccount
	{
		set { importingResourcesForNewAccount = value; }
	}

	private void RestoreHotspotImage(TourView tourView)
	{
		string fileName = Archive.HotspotImageFileName(tourView);
		string fileLocation = Path.Combine(tempFolderLocation, fileName);
		if (FileManager.FileExists(fileLocation))
		{
			Size size;
			Byte[] imageBytes = Utility.ImageFileToByteArray(fileLocation, out size);

			if (imageBytes.Length == 0)
			{
				Debug.Fail("Imported image has zero bytes: " + fileLocation);
				return;
			}
			
			tourView.ImageUploaded(fileName, size, imageBytes);
			tourView.UpdateDatabase(false);
		}
	}

	private void RestoreMapImage(TourPage tourPage)
	{
		string fileName = Archive.MapImageFileName(tourPage);
		string fileLocation = Path.Combine(tempFolderLocation, fileName);
		if (FileManager.FileExists(fileLocation))
		{
			Size sizeFile;
			Byte[] bytesFile;
			bytesFile = Utility.ImageFileToByteArray(fileLocation, out sizeFile);
			tourPage.ImageUploaded(fileName, sizeFile, bytesFile, true);
		}
	}

	private void RestoreResourceFromArchive(XPathNavigator resourcesNode, TourResourceType resourceType, string elementName, Hashtable restoredList)
	{
		int restoreCount = 0;
		string xPath = string.Format("{0}/{1}", resourceType == TourResourceType.Category ? "categories" : elementName + "s", elementName);

		string title = TourResourceManager.GetTitlePlural(resourceType);
		XPathNodeIterator resourceNodes = resourcesNode.Select(xPath);
		foreach (XPathNavigator resourceNode in resourceNodes)
		{
			bool restored = RestoreResourceFromArchive(resourceType, resourceNode, restoredList);
			if (restored)
				restoreCount++;

			ProgressMonitor.Update(title, totalImportSteps, ++numberOfImportStepsCompleted);
		}
		
		if (restoreCount == 0)
			report.EmitRow(ImportReport.Topic.ResourceImported, string.Format("No {0}s were imported", TourResourceManager.GetTitle(resourceType)));
	}

	private bool RestoreResourceFromArchive(TourResourceType resourceType, XPathNavigator resourceNode, Hashtable restoredList)
	{
		// This method creates a new resource object from a resource in the archive XML.
		// If no resource with the same name exists in the account, the archived resource gets added.
		// Otherwise, we compare the archived resource with the account resource of the same name.
		// The archived resource gets added to the account only if it does not match the existing resource.

		// Create a resource from the archive.
		TourResource resourceFromArchive = TourResourceManager.CreateNewResource(resourceType);
		RestoreResourcePropertiesFromArchive(resourceNode, resourceFromArchive);
		int resourceFromArchiveId = resourceFromArchive.Id;
		
		// Determine if this resource is an exclusive marker.
		bool isExlusiveMarker = resourceFromArchive.ResourceType == TourResourceType.Marker && ((Marker)resourceFromArchive).IsExclusive;

		// Update the Ids of resources that this resource references. We do this even before we know if
		// this resource will get imported because the fixup is necessary in order to compare two resources
		// that reference other resources. If we don't do this, HasSameAppearanceAs will return false.
		FixupReferencesToOtherResources(resourceFromArchive);

		// Get the resource's name.
		string resourceName = resourceType == TourResourceType.Category ? ((Category)resourceFromArchive).Code : resourceFromArchive.Name;

		// Determine if a resource with this name exists in this account. Note that when the resource is a
		// marker, the stored procedure sp_Marker_GetMarkerByName only returns non-exclusive marker. That
		// allows us to import non-exclusive markers that have the same names as existing exclusive markers.
		// If the resource from the archive is an exclusive marker, we don't bother to look for an existing
		// resource with the name since we always add an exclusive marker as a new resource. Note that name
		// overloading is allowed for exclusive markers because the name's scope is only the owner map.
		MapsAliveDataRow row = null;
		if (!isExlusiveMarker)
		{
			row = MapsAliveDatabase.LoadDataRow(
				string.Format("sp_{0}_Get{0}ByName", resourceType.ToString()), "@AccountId", account.Id, "@Name", resourceName);
		}

		// Decide if we should import this archived resource or use an existing resource that is identical.
		bool import = false;
		if (row == null || isExlusiveMarker)
		{
			// No existing resource has this name or the resource is an exclusive marker.
			import = true;
			report.Trace(string.Format("{0} ({1}): did not exist", resourceName, TourResourceManager.GetTitle(resourceType)));
		}
		else
		{
			// There is already a resource with this name. See if it is identical to the one from the archive.
			int resourceId = row.IntValue(string.Format("{0}Id", resourceType.ToString()));
			TourResource existingResource = TourResourceManager.CreateNewResource(resourceType, resourceId);
			if (existingResource.HasSameAppearanceAs(resourceFromArchive))
			{
				// The archived resource matches an existing resource -- don't import.
				restoredList.Add(resourceFromArchiveId, existingResource);
				report.Trace(string.Format("{0} ({1}): skipped because identical resource exists.", resourceFromArchive.Name, TourResourceManager.GetTitle(resourceType)));
			}
			else
			{
				// The archived resource is different than an existing resource with the same name.
				// Import the archived resource, but give it a different name.
				import = true;
				resourceName = TourResource.CreateUniqueResourceName(resourceType, resourceName);
				report.Trace(string.Format("{0} ({1}): exists, but imported version is different.", resourceFromArchive.Name, TourResourceManager.GetTitle(resourceType)));

				if (resourceType == TourResourceType.Category)
					((Category)resourceFromArchive).Code = resourceName;
				else
					resourceFromArchive.Name = resourceName;
			}
		}

		if (import)
		{
			// Add the unarchived resource to this account.
			resourceFromArchive.InsertIntoDatabase(account.Id);
			
			// Make sure there is a resource image file for this resource. If the archive was
			// created in another account, this account might not have an image for the resource.
			// Note that exclusive marker's don't get resource image files and must be marked so.
			if (resourceType != TourResourceType.Category)
			{
				if (isExlusiveMarker)
				{
					resourceFromArchive.ResourceImageId = TourResource.NoImageResourceImageId;
					resourceFromArchive.UpdateResourceImageIdInDatabase();
				}
				else
				{
					resourceFromArchive.CreateResourceImageId();
					resourceFromArchive.UpdateResourceImageIdInDatabase();
					TourResource.CreateResourceImageFile(resourceType, resourceFromArchive.Id, resourceFromArchive.ResourceImageId, ResourceImageFileAction.CreateFileIfMissing);
				}
			}
			
			// Keep track of this resource and it's orgiginal Id in case it needs to be used in
			// a fixup for another resource that refers to this resource. This resource now has
			// an Id in this account, but for a fixup we can find it using its original Id.
			restoredList.Add(resourceFromArchiveId, resourceFromArchive);
			
			// Determine if this is the default resource for its resource type.
			if (importingResourcesForNewAccount)
				SetDefaultResource(resourceFromArchive);
			
			report.EmitRow(ImportReport.Topic.ResourceImported, string.Format("{0}: {1}", TourResourceManager.GetTitle(resourceType), resourceName));
		}

		return import;
	}

	private void RestoreResourcePropertiesFromArchive(XPathNavigator navigator, TourResource resource)
	{
		// Set the resource's Id and name directly from the XML -- these are common to all resources.
		resource.Id = navigator.SelectSingleNode("id").ValueAsInt;
		if (resource.ResourceType != TourResourceType.Category)
			resource.Name = navigator.SelectSingleNode("name").Value;
		
		// Call the resource's initialize method to set all the resource-specific properties.
		resource.InitializeResourceFromDataRecord(new MapsAliveDataRecordXml(navigator));

		// Special case handling for symbol reources.
		if (resource.ResourceType == TourResourceType.Symbol)
		{
			// A symbol's image bytes are not in the archive XML.
			// Restore them from the image file contained in the archive.
			Symbol symbol = (Symbol)resource;
			string fileName = Archive.SymbolImageFileName(symbol);
			string fileLocation = Path.Combine(tempFolderLocation, fileName);
			if (FileManager.FileExists(fileLocation))
			{
				byte[] imageBytes = FileManager.ReadFileBytes(fileLocation);

				// Verify that the bytes from the file match those that were exported. We want to
				// make sure that the user did not modify the image file within the archive zip.
				if (Utility.Hash(imageBytes) != navigator.SelectSingleNode("key").Value)
				{
					report.Trace(string.Format("File for symbol {0} was modified.", symbol.Id));
					throw new Exception("Import was terminated because a symbol image in the archive was modified");
				}

				// Put the image bytes into the symbol as though an image file had been uploaded.
				symbol.ImageUploaded(fileName, symbol.Size, imageBytes);
			}
			else
			{
				report.Trace(string.Format("File for symbol {0} is not in the archive.", symbol.Id));
				throw new Exception("Import was terminated because a symbol image is missing from the archive");
			}
		}
	}

	private void RestoreTourFromArchivePass1(XPathNavigator resourcesNode)
	{
		RestoreResourceFromArchive(resourcesNode, TourResourceType.Category, "category", restoredCategories);
		RestoreResourceFromArchive(resourcesNode, TourResourceType.TourStyle, "colorScheme", restoredColorSchemes);
		RestoreResourceFromArchive(resourcesNode, TourResourceType.FontStyle, "fontStyle", restoredFontStyles);
		RestoreResourceFromArchive(resourcesNode, TourResourceType.MarkerStyle, "markerStyle", restoredMarkerStyles);
		RestoreResourceFromArchive(resourcesNode, TourResourceType.Symbol, "symbol", restoredSymbols);
		RestoreResourceFromArchive(resourcesNode, TourResourceType.TooltipStyle, "tooltipStyle", restoredTooltipStyles);
		RestoreResourceFromArchive(resourcesNode, TourResourceType.Marker, "marker", restoredMarkers);

		if (importingResourcesForNewAccount)
		{
			// Set the default resources for the new account.
			account.UpdateAccountResourceSettings();
		}
	}

	private void RestoreTourFromArchivePass2(XPathNavigator navigator)
	{
		MapsAliveDataRecord dataRecord = new MapsAliveDataRecordXml(navigator);

		string originalTourName = dataRecord.StringValue(Tour.Tag.name);
		bool nameInUse = Tour.TourNameInUse(originalTourName);

		// Create a new tour in this account and initialize it from the archive XML.
		tour = Tour.CreateNewTour(null, false, false);
		
		tour.InitializeTourFromDataRecord(dataRecord, true);
		report.Trace(string.Format("IMPORTING ARCHIVED XML INTO TOUR {0} (#{1})", tour.Name, tour.Id));

		// Turn off autolayout while importing.
		autoLayoutEnabled = tour.AutoLayoutEnabled;
		tour.AutoLayoutEnabled = false;

		// Give the tour a new name if necessary.
		if (duplicatingTour)
		{
			tour.Name = Tour.CreateCopyOfTourName(originalTourName);
		}
		else
		{
			if (nameInUse)
				tour.Name = Tour.CreateUniqueTourName(originalTourName, 1, true);
		}

		// Restore the tour's banner image if it has one.
		RestoreBannerImage();

		// Restore the tour's maps, map images, hotspots, and hotspot images.
		// In this first pass over the tour's pages, the TourPage and TourView objects
		// get default values only. Later we'll restore them completely from the archive XML.
		// The purpose of the first pass is to get everything in the database so that we
		// can fixup references between objects.
		XPathNodeIterator mapNodes = navigator.Select("maps/map");
		foreach (XPathNavigator mapNode in mapNodes)
		{
			RestoreTourPagePlaceholder(tour, mapNode);
		}

		// Fixup the tour's reference to its first page. The first
		// page should only be null if the tour has no pages.
		TourPage firstPage = (TourPage)restoredTourPages[tour.FirstPageId];
		if (firstPage != null)
			tour.FirstPageId = firstPage.Id;

		// Fixup the tour's reference to its color scheme.
		ColorScheme colorScheme = (ColorScheme)restoredColorSchemes[tour.ColorSchemeId];
		tour.ColorSchemeId = colorScheme.Id;

		// Restore the directory.
		XPathNavigator directoryNode = navigator.SelectSingleNode("directory");
		tour.Directory.InitializeDirectoryFromDataRecord(new MapsAliveDataRecordXml(directoryNode));
		tour.Directory.UpdateDatabase();

		// Save the updates made to the tour.
		tour.UpdateNextPageId();
		tour.UpdateDatabase();
		
		// Let the account figure out if this import put the user over their hotspot limit.
		account.HotspotAdded(tour);
	}

	private void RestoreTourFromArchivePass3(XPathNavigator navigator)
	{
		// At this point the structure of the tour, its tour pages, and tour views, has been
		// recreated. Now fully initialize each TourPage and TourView from the archive XML.
		InitializeTourPagesAndTourViews(navigator);

		// Set the tour's first page as selected so that one will be highlighted in the Tour Navigator.
		tour.SetSelectedTourPage(tour.FirstPageId);

		// Restore the tour's autolayout option.
		tour.AutoLayoutEnabled = autoLayoutEnabled;
		tour.UpdateDatabase();

		// Build the TourBuilder representation of the tour tree XML.
		tour.RebuildTourTreeXml();
	}

	private void RestoreTourPagePlaceholder(Tour tour, XPathNavigator mapNode)
	{
		int tourPageId = mapNode.SelectSingleNode(TourPage.Tag.id.ToString()).ValueAsInt;
		bool isDataSheet = mapNode.SelectSingleNode(TourPage.Tag.isDataSheet.ToString()).Value.ToLower() == "true";
		bool isGallery = OptionalNodeStringValue(mapNode, TourPage.Tag.isGallery.ToString()).ToLower() == "true";
		string pageName = mapNode.SelectSingleNode(TourPage.Tag.name.ToString()).Value;
		string pageId = mapNode.SelectSingleNode(TourPage.Tag.pageId.ToString()).Value;
		int pageNumber = mapNode.SelectSingleNode(TourPage.Tag.pageNumber.ToString()).ValueAsInt;

		// Create the tour page for this map or data sheet.
		TourPage tourPage = tour.CreateNewTourPage(isGallery, isDataSheet, pageName, false);
		tourPage.ImportingArchive = true;
		tourPage.PageId = pageId;
		tourPage.PageNumber = pageNumber;
		tour.AddTourPage(tourPage, isDataSheet);
		restoredTourPages.Add(tourPageId, tourPage);
		report.EmitRow(ImportReport.Topic.MapImported, tourPage.Name);

		if (isDataSheet)
		{
			XPathNavigator hotspotNode = mapNode.SelectSingleNode("hotspots/hotspot");
			if (hotspotNode == null)
			{
				// This should never happen, but there was a bug where a datasheet's tour view could
				// get deleted so we protect against it here in case a tour like that ever got archived.
				return;
			}
			int originalTourViewId = hotspotNode.SelectSingleNode("id").ValueAsInt;
			RestoreDataSheetPlaceholder(tourPage, originalTourViewId);
		}
		else
		{
			// Restore the map's hotspots.
			XPathNodeIterator hotspots = mapNode.Select("hotspots/hotspot");
			foreach (XPathNavigator hotspotNode in hotspots)
			{
				RestoreTourViewPlaceholder(tourPage, hotspotNode);
			}
		}
	}

	private void RestoreTourViewPlaceholder(TourPage tourPage, XPathNavigator hotspotNode)
	{
		int originalTourViewId = hotspotNode.SelectSingleNode(TourView.Tag.id.ToString()).ValueAsInt;
		string hotspotId = hotspotNode.SelectSingleNode(TourView.Tag.hotspotId.ToString()).Value;

		// Create a new tour view using an empty title. It will get replaced with the actual
		// title in a later pass. By using an empty title now we save the cost of having to
		// generate unique title names which gets expensive when restoring a large tour.
		TourView tourView = tour.CreateNewTourView(string.Empty, tourPage);
		
		tourView.SlideId = hotspotId;
		tour.AddTourView(tourView, true);
		restoredTourViews.Add(originalTourViewId, tourView);
		report.Trace("Restore placeholder for hotspot " + hotspotId);

		// Restore the hotspot's categories.
		CategoryManager categoryManager = tour.CategoryManager;
		XPathNodeIterator categories = hotspotNode.Select("categories/category");
		foreach (XPathNavigator categoryNode in categories)
		{
			int categoryId = categoryNode.ValueAsInt;
			Category category = (Category)restoredCategories[categoryId];
			categoryManager.AddTourViewCategory(tourView, category.Code);
		}

		// Get the hotspot's image.
		RestoreHotspotImage(tourView);

		ProgressMonitor.Update("Hotspots", totalImportSteps, ++numberOfImportStepsCompleted);
	}

	private void SetDefaultResource(TourResource resource)
	{
		// This method is called when resource are being imported for a new account. It determines
		// if the passed-in resource should be used as the account default. This logic is designed
		// so that a default will get set for each resource even if the master resources XML names
		// don't match names in the switch statement below. If there's no match, the first resource
		// that this method gets called with becomes the default. This way a default still gets set
		// even if we edit names in the master XML and forget to make a corresponding changes here.
		// Ideally the names here should be in an external file, but this is good enough for now.

		bool isDefault = false;
		string name = resource.Name.ToLower();
		
		switch (resource.ResourceType)
		{
			case TourResourceType.FontStyle:
				isDefault = name == "arial 18";
				break;

			case TourResourceType.Marker:
				isDefault = name == "arrow 32 blue red";
				break;

			case TourResourceType.MarkerStyle:
				isDefault = name == "blend - gold/red";
				break;

			case TourResourceType.Symbol:
				isDefault = name == "arrow 32 blue";
				break;

			case TourResourceType.TourStyle:
				isDefault = name == "metal";
				break;

			case TourResourceType.TooltipStyle:
				isDefault = name == "arial 24 - black on white";
				break;
		}

		bool defaultHasBeenSet = account.DefaultResourceId(resource.ResourceType) != 0;
		if (!isDefault && defaultHasBeenSet)
		{
			// The current resource is not the default, but the default has already
			// been set. If no default was set yet, we'd fall through and set it.
			return;
		}
		
		int resourceId = resource.Id;
		switch (resource.ResourceType)
		{
			case TourResourceType.FontStyle:
				account.DefaultFontStyleId = resourceId;
				break;
			
			case TourResourceType.Marker:
				account.DefaultMarkerId = resourceId;
				break;
			
			case TourResourceType.MarkerStyle:
				account.DefaultMarkerStyleId = resourceId;
				break;
			
			case TourResourceType.Symbol:
				account.DefaultSymbolId = resourceId;
				break;
			
			case TourResourceType.TourStyle:
				account.DefaultColorSchemeId = resourceId;
				break;
			
			case TourResourceType.TooltipStyle:
				account.DefaultTooltipStyleId = resourceId;
				break;
		}
	}

	private void SetSchema(XmlReaderSettings settings)
	{
		// We are not using a schema to validate the imported XML because we currently do not allow
		// a user to create their own or modify ours. If we decide to start using a schema, the code
		// below will detect and report schema errors and prevent an import from using bad XML.
		bool usingSchema = false;
		if (!usingSchema)
			return;

		string schemaFileLocation = FileManager.WebAppFileLocationAbsolute("App_Data", Archive.XmlSchemaFileName);
		settings.Schemas.Add(null, schemaFileLocation);
		settings.ValidationType = ValidationType.Schema;
		settings.ValidationEventHandler += new ValidationEventHandler(ValidationEventHandler);
	}

	private bool ValidateArchive(string archiveFileLocation)
	{
		try
		{
			string keyFileLocation = Path.Combine(tempFolderLocation, Archive.KeyFileName);
			if (!FileManager.FileExists(archiveFileLocation))
			{
				message = "The archive cannot be imported because its XML is missing.";
				return false;
			}
			if (!FileManager.FileExists(keyFileLocation))
			{
				message = "The archive cannot be imported because its key is missing.";
				return false;
			}

			string key;
			string hash;
			try
			{
				// Read the exported file's hash from the key file.
				key = FileManager.ReadFileContents(keyFileLocation);

				// Read and hash the bytes for the archive XML.
				byte[] bytes = FileManager.ReadFileBytes(archiveFileLocation);
				hash = Utility.Hash(bytes);
			}
			catch 
			{
				message = "The archive cannot be imported because its key is not valid.";
				return false;
			}

			// Make sure the two hash values match.
			if (hash != key)
			{
				message = "The archive cannot be imported because it has been modified.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			message = "Import failed: " + ex.Message;
			return false;
		}
	}

	private void ValidationEventHandler(object sender, ValidationEventArgs e)
	{
		string message = e.Message;
		int lineNumber = e.Exception.LineNumber;

		switch (e.Severity)
		{
			case XmlSeverityType.Error:
				report.Trace(message, lineNumber);
				break;

			case XmlSeverityType.Warning:
				report.Trace(message, lineNumber);
				break;
		}

		schemaValidationFailed = true;
	}
}
