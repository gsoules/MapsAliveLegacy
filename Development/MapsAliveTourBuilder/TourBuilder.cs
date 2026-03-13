// Copyright (C) 2003-2010 AvantLogic Corporation
using AvantLogic.MapsAlive.Engine;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

public class TourBuilder
{
	private StringBuilder cssStringBuilder;
	private FileManager fileManager;
	private bool forceRebuild;
	private Hashtable mapMarkersTable;
	private Hashtable markerDefinitions;
	private int markerDefinitionId;
	private DataTable markersTable;
	private	XmlWriterSettings outputSettings;
	private MarkerDefinition routeMarkerDefinition;
	private string runtimeFolderLocationAbsolute;
	private Tour tour;
	private string previewFolderLocationAbsolute;
	private string publishedFolderLocationAbsolute;
	private XslCompiledTransform xslt;

	public TourBuilder()
	{
		runtimeFolderLocationAbsolute = FileManager.WebAppFolderLocationAbsolute("Runtime");
	}

	public TourBuilder(Tour tour) : this()
	{
		Debug.WriteLine("BUILD: " + "TourBuilder " + tour.Name);
		
		this.tour = tour;
		previewFolderLocationAbsolute = FileManager.PreviewFolderLocationAbsolute(tour.Id);
		publishedFolderLocationAbsolute = FileManager.PublishedFolderLocationAbsolute(tour.Id);

		fileManager = new FileManager();
		mapMarkersTable = new Hashtable();
	}

	#region ===== Properties ========================================================

	public static string PatternForMapJsFile
	{
		get { return "{1}_map_{0}.js"; }
	}

	public static string PatternForMapTilesFileV3
	{
		get { return "{0}.js"; }
	}

	public static string PatternForPageCssFile
	{
		get { return "{1}_page_{0}.css"; }
	}

	public static string PatternForPageCssFileV3
	{
		get { return "page{0}.css"; }
	}

	public static string PatternForPageHtmlPreviewFile
	{
		get { return "{1}_{0}"; }
	}

	public static string PatternForPageHtmlPreviewFileV3
	{
		get { return "{0}"; }
	}

	public static string PatternForPageHtmlPublishedFile
	{
		// Both V3 and newer published tours use the same HTML file name. This preserves backward
		// compatibiilty for users who had links to V3 tours e.g. tour.mapsalive.com/1234/page1.htm.
		get { return PatternForPageHtmlPublishedFileV3; }
	}

	public static string PatternForPageHtmlPublishedFileV3
	{
		get { return "page{0}.htm"; }
	}

	public static string PatternForPageHtmlUnbrandedPreviewFile
	{
		get { return "{1}_{0}_"; }
	}

	public static string PatternForPageHtmlUnbrandedPreviewFileV3
	{
		get { return "{0}_"; }
	}

	public static string PatternForPageHtmlUnbrandedPublishedFile
	{
		// Both V3 and newer published tours use the same HTML file name. This preserves backward
		// compatibiilty for users who had links to V3 tours e.g. tour.mapsalive.com/1234/page1_.htm.
		get { return PatternForPageHtmlUnbrandedPublishedFileV3; }
	}

	public static string PatternForPageHtmlUnbrandedPublishedFileV3
	{
		get { return "page{0}_.htm"; }
	}

	public static string PatternForPageJsFile
	{
		get { return "{1}_page_{0}.js"; }
	}

	public static string PatternForPageJsFileV3
	{
		get { return "page{0}.js"; }
	}

	public static string PatternForSymbolsFile
	{
		get { return "{1}_symbols_{0}.js"; }
	}

	public static string PatternForSymbolsFileV3
	{
		get { return "symbols{0}.js"; }
	}

	public static string PatternForTourCssJsFile
	{
		get { return "{0}_css.js"; }
	}

	public static string PatternForTourCustomJsFile
	{
		get { return "{0}_custom.js"; }
	}

	public static string PatternForTourCustomJsFileV3
	{
		get { return "custom.js"; }
	}

	public static string PatternForTourHtmlJsFile
	{
		get { return "{0}_html.js"; }
	}

	public static string PatternForTourindexPreviewFile
	{
		get { return "{0}_{1}"; }
	}

	public static string PatternForTourindexUnbrandedPreviewFile
	{
		get { return "{0}_{1}_"; }
	}

	public static string PatternForTourIndexPublishedFile
	{
		get { return "index.htm"; }
	}

	public static string PatternForTourIndexUnbrandedPublishedFile
	{
		get { return "index_.htm"; }
	}

	public static string PatternForTourLoaderJsFile
	{
		get { return "mapsalive-module.js"; }
	}

	public static string PatternForTourLoaderDeactivatedJsFile
	{
		get { return "mapsalive-module.js.deactivated"; }
	}

	public static string PatternForTourPropertiesJsFile
	{
		get { return "{0}_tour.js"; }
	}

	#endregion

	#region ===== Public ============================================================

	private void BuildDefaultPage()
	{
		if (tour.V4)
			return;

		Debug.WriteLine("BUILD: " + "BuildDefaultPage");

		// Build the default.htm page that that will redirect to the starting
		// tour page when just the tour folder's name is used as the tour's URL.

		string tourPageLocation = Path.Combine(previewFolderLocationAbsolute, "default.htm");
		string head = string.Empty;
		string body = string.Empty;

		int firstPageId = tour.FirstPageId;

		if (firstPageId == 0)
		{
			body = string.Format(MapsAliveTourBuilder.Html.HtmlForDefaultPage, tour.Name);
		}
		else
		{
			int pageNumber = (int)MapsAliveDatabase.ReadScalar("sp_TourPage_GetTourPageNumberByTourPageId", "@TourPageId", firstPageId);
			head = string.Format("<meta http-equiv=\"refresh\" content=\"0;url=page{0}.htm\">", pageNumber);
		}

		string html = string.Format("<html><head>{0}</head><body>{1}</body></html><!--{2}-->", head, body, DateTime.Now.ToString());

		FileManager.CreateHtmlFile(tourPageLocation, html, false);
		
		// Make another copy of the file, but call it index.htm for web servers that
		// prefer that as the default when hosting a downloaded tour.
		FileManager.CopyFile(tourPageLocation, tourPageLocation.Replace("default.htm", "index.htm"));
		
		// Make another copy that can be used when embedding a map into a Facebook app. 
		// If you supply Facebook with index.htm it reports a 405 error.
		FileManager.CopyFile(tourPageLocation, tourPageLocation.Replace("default.htm", "index.aspx"));
	}

	private void BuildLoaderJavaScriptForMapEditor()
	{
		// Create a single Map Editor loader JS file for each page in the tour.
		// The loader for a page gets executed when that page's map gets edited.

		foreach (TourPage tourPage in tour.TourPages)
        {
            if (tourPage.IsDataSheet)
                continue;

			StringBuilder script = new StringBuilder();
			bool isLoaderForMapEditor = true;

			BuildLoaderSectionForAddEventListener(script, isLoaderForMapEditor);
			BuildLoaderSectionForImportTourClasses(script, isLoaderForMapEditor);
			BuildLoaderSectionForImportTourPageData(script, tourPage, isLoaderForMapEditor);
			BuildLoaderSectionForInit(script, isLoaderForMapEditor);
			BuildLoaderSectionForAddTourPage(script, tourPage, isLoaderForMapEditor);

			string mapEditorLoaderLocation = string.Format(Path.Combine(previewFolderLocationAbsolute, PatternForMapJsFile), tourPage.PageNumber, tour.BuildId);
			FileManager.CreateTextFile(mapEditorLoaderLocation, script.ToString());
        }
    }

    private void BuildLoaderJavaScriptForTour()
    {
        // Create a single loader JS file for the entire tour./ The loader gets
		// executed once when the tour is run and it loads data for all the pages. 

        StringBuilder script = new StringBuilder();
		bool isLoaderForMapEditor = false;

		if (tour.TourPages.Count >= 1)
			BuildLoaderSectionForAddEventListener(script, isLoaderForMapEditor);
        
		BuildLoaderSectionForImportTourClasses(script, isLoaderForMapEditor);

        foreach (TourPage tourPage in tour.TourPages)
        {
			BuildLoaderSectionForImportTourPageData(script, tourPage, isLoaderForMapEditor);
        }

        BuildLoaderSectionForInit(script, isLoaderForMapEditor);

        foreach (TourPage tourPage in tour.TourPages)
        {
            BuildLoaderSectionForAddTourPage(script, tourPage, isLoaderForMapEditor);
        }

		string tourLoaderLocation = string.Format(Path.Combine(previewFolderLocationAbsolute, PatternForTourLoaderJsFile), tour.Id);
        FileManager.CreateTextFile(tourLoaderLocation, script.ToString());
    }

    private void BuildLoaderSectionForAddEventListener( StringBuilder script, bool isLoaderForMapEditor)
    {
		string dirProperties = !isLoaderForMapEditor ? "directoryProperties" : "null";
		string js = string.Format("window.addEventListener('load',(event)=>{{{{MapsAliveRuntime{1}.onWindowLoadEvent(tourProperties, {0}, pagesData", dirProperties, tour.Id);
		
		if (!isLoaderForMapEditor)
        {
			// Add parameters that are used for tours, but not by the Map Editor.
			js += ", css, html";
			js += tour.HasCustomHtmlJavaScript ? ", js" : "";
        }

		js += ");}});";
		script.AppendLine(js);
	}

	private void BuildLoaderSectionForAddTourPage(StringBuilder script, TourPage tourPage, bool isLoaderForMapEditor)
    {
        if (!tourPage.IsDataSheet && (tourPage.SlidesPopup || tourPage.ActiveSlideLayout.HasMapArea))
			script.AppendLine(string.Format("pagesData.push({{page:page{0}Properties, map:page{0}MapProperties, popup:page{0}PopupProperties, symbols:page{0}Symbols}});", tourPage.PageNumber));
		else
            script.AppendLine(string.Format("pagesData.push({{page:page{0}Properties, map:null, popup:null, tiles:null, symbols:null}});", tourPage.PageNumber));
    }

    private void BuildLoaderSectionForImportTourClasses(StringBuilder script, bool isLoaderForMapEditor)
    {
        script.AppendLine();
        script.AppendLine(string.Format("import {{ MapsAliveRuntime as MapsAliveRuntime{0}}} from './mapsalive-runtime.js';", tour.Id));
		script.AppendLine(string.Format("import {{ tourProperties }} from './{0}'", tour.NameForTourPropertiesJsFile));

		if (!isLoaderForMapEditor)
			script.AppendLine(string.Format("import {{ directoryProperties }} from './{0}'", tour.NameForTourPropertiesJsFile));

		if (!isLoaderForMapEditor)
        {
			script.AppendLine(string.Format("import {{ css }} from './{0}'", tour.NameForTourCssJsFile));
			script.AppendLine(string.Format("import {{ html }} from './{0}'", tour.NameForTourHtmlJsFile));
			
			if (tour.HasCustomHtmlJavaScript)
				script.AppendLine(string.Format("import {{ js }} from './{0}'", tour.NameForTourCustomJsFile));
		}
	}

	private void BuildLoaderSectionForImportTourPageData(StringBuilder script, TourPage tourPage, bool isLoaderForMapEditor)
	{
		string symbolsFileName = String.Format(PatternForSymbolsFile, tourPage.PageNumber, tour.BuildId);
		string pageFileName = String.Format(PatternForPageJsFile, tourPage.PageNumber, tour.BuildId);
		script.AppendLine();
		script.AppendLine(string.Format("import {{ page{0}Properties }} from './{1}';", tourPage.PageNumber, pageFileName));

        if (!tourPage.IsDataSheet && (tourPage.SlidesPopup || tourPage.ActiveSlideLayout.HasMapArea))
        {
            script.AppendLine(string.Format("import {{ page{0}MapProperties }} from './{1}';", tourPage.PageNumber, pageFileName));
			script.AppendLine(string.Format("import {{ page{0}PopupProperties }} from './{1}';", tourPage.PageNumber, pageFileName));
			script.AppendLine(string.Format("import {{ page{0}Symbols }} from './{1}';", tourPage.PageNumber, symbolsFileName));
        }

		if (isLoaderForMapEditor)
			return;
	}

	private void BuildLoaderSectionForInit(StringBuilder script, bool isLoaderForMapEditor)
	{
		script.AppendLine();
		script.AppendLine("let pagesData = [];");
	}

	private void BuildMap(TourPage tourPage, BaseMap map)
	{
		Debug.WriteLine("BUILD: " + "BuildMap " + tourPage.Name);

		const int layerId = 1;
		BaseLayer layer = new BaseLayer(map, "< map >", layerId, null);
		layer.ZIndex = 1;

		// Create an array that will be used to hold each instance of a map marker. The list includes
        // Gallery markers that don't fit in a gallery so that the runtime knows which markers don't fit.
		int count = 0;
		foreach (TourView tourView in tourPage.TourViews)
			count++;
		ArrayList mapMarkers = new ArrayList(count);
		for (int i = 0; i < count; i++)
			mapMarkers.Add(null);

		// Create a marker instance for each hotspot.
		mapMarkersTable.Add(tourPage.Id, mapMarkers);
		CreateMarkerInstances(tourPage, layer, mapMarkers);
	}

	public void BuildMapForTourPage(TourPage tourPage, bool createMapImageFile)
	{
		Debug.WriteLine("BUILD: " + "BuildMapForTourPage " + tourPage.Name);
		
		TourImage mapImage = tourPage.MapImage;

		// Create a file for the map's image.
		if (createMapImageFile)
		{
			bool isMapImage = true;
			bool isMapInsetImage = false;

			bool useImageSize = mapImage.HasFile && !tourPage.IsGallery;
			int mapImageWidth = useImageSize ? mapImage.Width : tourPage.MapAreaSize.Width;
			int mapImageHeight = useImageSize ? mapImage.Height : tourPage.MapAreaSize.Height;
			
			mapImage.BumpVersionAndUpdateDatabase();
			Size containerSize;
			if (tourPage.MapCanZoom)
				containerSize = new Size(mapImageWidth, mapImageHeight);
			else
				containerSize = tourPage.MapAreaSize;
			
			// Create the full size map image file.
            if (tour.V3CompatibilityEnabled)
            {
			    ImageExpansionType imageExpansionType = tourPage.IsGallery ? tourPage.GalleryOptions.BackgroundType : ImageExpansionType.None;
			    mapImage.CreateFile(tour.Id, tourPage, containerSize, isMapImage, isMapInsetImage, tourPage.MapPlaceholderColor, imageExpansionType);
            }
            else
            {
                // Create a single V4 gallery image instead of multiple versions of the entire map image at different resolutions.
                if (tourPage.IsGallery)
                    mapImage.CreateFile(tour.Id, tourPage, containerSize, isMapImage, isMapInsetImage, tourPage.MapPlaceholderColor, tourPage.GalleryOptions.BackgroundType);
            }

            if (!tourPage.IsGallery)
            {
                // Create multiple versions of the entire map image at different resolutions. This is the V4 approach vs V3's use
                // of map tiles. The runtime chooses the appropriate size to use based on how far in our out the map is zooomed.
                // The files get created in the preview folder for both V3 and V4 for use by the Map Editor.
                mapImage.CreateFilesForMapImage(tour.Id, containerSize, tourPage.MapPlaceholderColor);
            }

			// Create the map image inset file that's displayed on zoomable maps,
            // or in V4, any map that has a non-zero map inset location.
			if ((tourPage.MapCanZoom) && tourPage.MapInsetLocation != 0)
			{
				// Calculate the scale of the inset based on the long dimension.
				int longDimension = mapImageWidth > mapImageHeight ? mapImageWidth : mapImageHeight;
				float mapInsetScale = (float)tourPage.MapInsetSize / (float)longDimension;

				// Determine the size of the map inset.
				int insetWidth = (int)Math.Round((float)mapImageWidth * mapInsetScale);
				int insetHeight = (int)Math.Round((float)mapImageHeight * mapInsetScale);
				Size mapInsetSize = new Size(insetWidth, insetHeight);

				// Create the map inset image.  It will have the same file name as
				// the full size map image, but will be prefixed with an underscore.
				isMapInsetImage = true;
				TourImage tourImage = mapImage;
				tourImage.CreateFile(tour.Id, mapInsetSize, isMapImage, isMapInsetImage, tourPage.MapPlaceholderColor);
			}
		}

		// Create the marker definitions needed by this page.
		BuildMarkerDefinitions(tourPage);
		
		// Arrange the photo markers.
		if (tourPage.IsGallery)
		{
			new Gallery(tourPage, markerDefinitions).ArrangeMarkers();
		}

		// Build the map.
		BaseMap map = new BaseMap(tourPage.Id, Size.Empty);
		BuildMap(tourPage, map);
	}

	private void BuildMarkerDefinitions(TourPage tourPage)
	{
		Debug.WriteLine("BUILD: " + "BuildMarkerDefinitions " + tourPage.Name);

		if (markersTable == null)
		{
			// There's are no marker definitions yet because this is the first or the only
			// page being built. Get a list of unique Ids of the markers used in this tour.
			// We do the query just once for all pages that are being built.
			markersTable = MapsAliveDatabase.LoadDataTable("sp_Marker_GetDistinctMarkersInTour", "@TourId", tour.Id);
			markerDefinitions = new Hashtable();
			markerDefinitionId = 1;
		}
		
		// Create a marker definition for each unique marker.
		foreach (DataRow dataRow in markersTable.Rows)
		{
			// Get the marker's Id.
			MapsAliveDataRow distinctMarkerIdDataRow = new MapsAliveDataRow(dataRow);
			int markerId = distinctMarkerIdDataRow.IntValue("MarkerId");

			if (markerDefinitions.Contains(markerId.ToString()))
			{
				// This marker's definition was created for an earlier page.
				continue;
			}

			// Get the marker object.
			Marker marker = Account.GetCachedMarker(markerId);

			if (marker.MarkerType == MarkerType.Photo)
			{
				// This is a photo marker. Create one definition for each instance.
				foreach (TourView tourView in tourPage.TourViews)
				{
					if (tourView.MarkerId == marker.Id)
					{
						// This view uses this photo marker. Create an instance using the view's image.
						MarkerDefinition markerDefinition = marker.CreateMarkerDefinition(markerDefinitionId, tourPage.Tour, tourView);
						string galleryMarkerId = string.Format("{0}_{1}", markerId, tourView.Id);
						markerDefinitions.Add(galleryMarkerId, markerDefinition);
						markerDefinitionId++;
					}
				}
			}
			else if (marker.MarkerType == MarkerType.Text)
			{
				// This is a text marker. Create one definition for each instance.
				foreach (TourView tourView in tourPage.TourViews)
				{
					if (tourView.MarkerId == marker.Id)
					{
						// This view uses this text marker. Create an instance using the view's title text.
						MarkerDefinition markerDefinition = marker.CreateMarkerDefinition(markerDefinitionId, tourPage.Tour, tourView);
						string textMarkerId = string.Format("{0}_{1}", markerId, tourView.Id);
						markerDefinitions.Add(textMarkerId, markerDefinition);
						markerDefinitionId++;
					}
				}
			}
			else
			{
				// This is a standard marker -- create a single definition for it.
				// All tour views that use this marker will share this definition.
				MarkerDefinition markerDefinition = marker.CreateMarkerDefinition(markerDefinitionId, tourPage.Tour, null);
				markerDefinitions.Add(markerId.ToString(), markerDefinition);
				markerDefinitionId++;
			}
		}
	}

	public bool BuildTour()
	{
		Utility.RecordAction(MemberPageActionId.BuildTour);

		Debug.WriteLine("BUILD: " + "BuildTour");

		// For now, force a tour to be fully rebuilt, including its map tiles and symbols, even if those things have not changed.
		// Ultimately we should either a) remove this flag so that a full rebuild always happens or b) restore some of the V3
		// optimization logic where the preview and/or published tour folders were not completely cleaned out on a rebuild and
		// instead kept files that did not need to be updated. Note that the most expensive operation by far is to create map
		// tiles for a very large zoomable map. Just avoiding that operation might be a sufficient performance improvement.
		forceRebuild = true;

		tour.BuildStarted();
		tour.ReloadCategories();

		if (!BuildTourPreviewFolderRuntimeFiles())
			return false;

		if (tour.HasBanner)
        {
			if (tour.BannerImageChanged || tour.TourSizeChanged)
				tour.Banner.Image.BumpVersionAndUpdateDatabase();

			// Create the banner image file.
			tour.Banner.Image.CreateFile(tour.Id, tour.Banner.Size, false);
        }

		BuildTourPreviewFolderTourFiles();
		
		tour.BuildCompleted();

		return true;
	}

	private void BuildTourPageFiles(TourPage tourPage, bool firstPage, bool buildingV3Files)
	{
		Debug.WriteLine("BUILD: " + "BuildTourPageFiles " + tourPage.Name);

		try
		{
			// Put the XML in memory.
			MemoryStream tourXmlMemoryStream = tour.XmlForPage(tourPage.Id, this);

			if (buildingV3Files)
			{
				EmitFile("css", tourPage.Id, tourPage.NameForPageCssFileV3, tourXmlMemoryStream, true);
			}
			else if (tourPage.Id == tour.FirstPageId)
            {
				// Only build one of each of these files for the tour. This code is in tour page
				// loop because the XML is always created for a specfic page, not for just the tour.
				// It's simpler to do it this way than add totour-only XML logic.
				EmitFile("css-tour", 0, string.Empty, tourXmlMemoryStream, false);
				EmitFile("html-tour", 0, tour.NameForTourHtmlJsFile, tourXmlMemoryStream, false);
					
				// Create the preview version of what will be index.htm in the published tour.
				// In the preview folder the file name is numeric so that it cannot be run in the preview folder.
				EmitFile("html", tourPage.Id, tour.NameForTourIndexPreviewFile, tourXmlMemoryStream, false);

				if (tour.HasBanner && tour.CanAppearUnbranded)
					EmitFile("html_", tourPage.Id, tour.NameForTourIndexUnbrandedPreviewFile, tourXmlMemoryStream, false);

				EmitFile("css-page", tourPage.Id, string.Empty, tourXmlMemoryStream, buildingV3Files);
            }

			string fileName = buildingV3Files ? tourPage.NameForPageJsFileV3 : tourPage.NameForPageJsFile;
			EmitFile("js", tourPage.Id, fileName, tourXmlMemoryStream, buildingV3Files);

			if (buildingV3Files)
			{
				// Create the files that will be page1.htm, page2.htm in the published tour, but in the preview folder
				// have internal. These V3 files contain the HTML for a tour page.
				EmitFile("html", tourPage.Id, tourPage.NameForPageHtmlPreviewFileV3, tourXmlMemoryStream, true);

				if (tour.HasBanner && tour.CanAppearUnbranded)
				{
					EmitFile("html_", tourPage.Id, tourPage.NameForPageHtmlUnbrandedPreviewFileV3, tourXmlMemoryStream, true);
				}
			}
			else
			{
				if (tour.V4)
                {
					// Create the files named page1.htm, page2.htm etc. that simply redirect to index.htm. If someone attempts to run
					// one of them from the preview folder they'll get a 404 error because index.htm does not exist there.
					string head = string.Format("<meta http-equiv=\"refresh\" content=\"0;url=index.htm?page={0}\">", tourPage.PageNumber);
					string html = string.Format("<html><head>{0}</head><body></body></html>", head);
					string fileLocation = Path.Combine(previewFolderLocationAbsolute, string.Format("page{0}.htm", tourPage.PageNumber));
					FileManager.CreateTextFile(fileLocation, html);
                }
			}

			if (buildingV3Files)
				return;

			if (tourPage.Id == tour.FirstPage.Id)
			{
				// Build the main JavaScript module for the tour.
				EmitFile("tour", tourPage.Id, tour.NameForTourPropertiesJsFile, tourXmlMemoryStream, buildingV3Files);
			}
		}
		catch (Exception ex)
		{
			// If something goes wrong, report the exception to support, but let this method return
			// normally so that the build can continue. Chances are that the resulting tour will not
			// work correctly, but the user won't get a fatal error while trying to go to Tour Preview.
			// The only case we know of right now is null reference exception that occurs when somehow
			// the user was able to delete an Info page's slide. Without a slide, tourView is null in
			// the logic that emits XML for an info page (that logic originates from tour.XmlForPage
			// which is called from this method). 
			Utility.ReportException("BuildHtmlFile", ex);
		}
	}

	public bool BuildTourPreviewFolderRuntimeFiles()
	{
		Debug.WriteLine("BUILD: " + "BuildTourPreviewFolderRuntimeFiles");

		try
		{
			if (FileManager.FolderExists(previewFolderLocationAbsolute))
			{
				if (!DeleteTourFolderContents(previewFolderLocationAbsolute))
					return false;
			}
			else
			{
				FileManager.CreateFolder(previewFolderLocationAbsolute);
			}

			CopyRuntimeFilesToPreviewFolder();
			BuildDefaultPage();

			tour.CreateCustomHtmlFiles();

		}
		catch (Exception ex)
		{
			Utility.ReportException("BuildTourFolder", ex);
			return false;
		}
		
		return true;
	}

	private void BuildTourPreviewFolderTourFiles()
	{
		Debug.WriteLine("BUILD: " + "BuildTourPreviewFolderTourFiles");

		// If page renumbering is requested. Change each page's number to correspond to its menu position.
		// This is a user option to deal with pages that have been deleted leaving gaps in the numbering.
		if (tour.RenumberPages)
			RenumberPages();

		// Determine if a change to the tour itself made the HTML for all pages become
		// stale.  An example would be a change to the tour layout or color scheme.
		bool buildAllPages = forceRebuild || tour.HtmlForAllPagesChanged;

		// Build the pages in two passes. The first pass builds image files and constructs unique,
		// versioned names for files that are being created or recreated. The second pass builds the HTML
		// pages. Separate passes are required so that all file names for all pages are constructed before
		// those names are referenced in the HTML. We used to only have one pass until the [image-macro]
		// was introduced which allowed the HTML in e.g. Page 1 to reference a slide name in Page 2. With
		// only one, pass, the HTML for Page 1 would get generated before the Page 2 slide's file name was
		// constructed. If the user updated the Page 2 slide and a rebuild occurred, the original slide's
		// name would get used into Page 1 and then the name would change when Page 2 was built. This caused
		// a red X (missing image) to appear in Page 1.
		//
		BuildTourPagesPass1(buildAllPages);
		BuildTourPagesPass2();

		// Page renumbering is a one-shot request, so turn it off (if it was on).
		tour.RenumberPages = false;

		// If there is a new start page for this tour, build a new default.htm file that
		// will load the start page when the tour is referenced via its folder name.  If
		// the tour has no pages, also rebuild the default page in case the tour's name
		// changed since the default page was last built.
		if (tour.StartPageChanged || tour.TourPages.Count == 0)
			BuildDefaultPage();

		// Create the JavaScript that serves as the main entry point to a tour or the map editor.
		BuildLoaderJavaScriptForTour();
		BuildLoaderJavaScriptForMapEditor();
	}

	private void BuildTourPagesPass1(bool buildAllPages)
	{
		Debug.WriteLine("BUILD: " + "BuildTourPagesPass1 " + buildAllPages);

		// Examine each map page to determine what if anything needs to be built.
		foreach (TourPage tourPage in tour.TourPages)
		{
			if (buildAllPages)
				tourPage.TourChanged();

			// Build the page's map if needed
			bool buildMap = forceRebuild || tourPage.MapChanged || tour.TourSizeChanged || tourPage.HtmlChanged;
			if (buildMap && !tourPage.IsDataSheet)
			{
				bool createMapImageFile = forceRebuild || tourPage.MapImageChanged || tourPage.MapAreaSizeChanged;
				BuildMapForTourPage(tourPage, createMapImageFile);
			}

			// Tour views are not built per se, but we do need to create a new or resized image
			// file whenever a view's image changes or its page's image area size changes.  We
			// then also need to delete the old image.  When we create a new image, we bump its
			// version number in order to cause its file name to change and thus thwart browser
			// attempts to display a cached version of the image.
			foreach (TourView tourView in tourPage.TourViews)
			{
				if (tourView.HasImage)
				{
					// Determine if we need a new version of the image file. 
					if (tourPage.ImageAreaSizeChanged || tour.TourSizeChanged || tourView.ImageChanged)
						tourView.Image.BumpVersionAndUpdateDatabase();

					// If necessary, create an image file that is scaled to fit within the image
					// container.  If the right file already exists, we'll simply keep using it.
					Size containerSize = tourView.GetImageContainerSize();
					if (Utility.HasWidthAndHeight(containerSize))
						tourView.Image.CreateFile(tour.Id, containerSize, false);
				}

				// Update the database to indicate that this view has been built.
				tourView.Built();
			}
		}
	}

	private void BuildTourPagesPass2()
	{
		Debug.WriteLine("BUILD: " + "BuildTourPagesPass2");

		// Load the xslt file that will transform the input xml to the output html.
		// This is expensive so we only do it once, the first time a user does a build.
		// We then save the result in a transform object and save that object in the
		// application cache so that it will be there for everyone.

		if (tour.V3CompatibilityEnabled)
        {
			// Build V3 versions of the files.
			xslt = (XslCompiledTransform)MapsAliveState.Retrieve(MapsAliveObjectType.XslV3);
			if (xslt == null)
			{
				xslt = new XslCompiledTransform();
				xslt.Load(FileManager.WebAppFolderLocationAbsolute("XSL") + "\\TourHtmlV3.xsl");
				MapsAliveState.Persist(MapsAliveObjectType.XslV3, xslt);
			}

			outputSettings = xslt.OutputSettings;

			foreach (TourPage tourPage in tour.TourPages)
			{
				BuildTourPageFiles(tourPage, false, true);
			}
		}

		// Build the files. Even when V3 compatibility is enabled, the files are needed by the Map Editor.
		xslt = (XslCompiledTransform)MapsAliveState.Retrieve(MapsAliveObjectType.Xsl);
		if (xslt == null)
		{
			xslt = new XslCompiledTransform();
			xslt.Load(FileManager.WebAppFolderLocationAbsolute("XSL") + "\\TourHtml.xsl");
			MapsAliveState.Persist(MapsAliveObjectType.Xsl, xslt);
		}

		outputSettings = xslt.OutputSettings;

		// Create an empty string builder that will accumulate the CSS for the tour and for each page.
		cssStringBuilder = new StringBuilder();

		bool firstPage = true;
		foreach (TourPage tourPage in tour.TourPages)
		{
			BuildTourPageFiles(tourPage, firstPage, false);
			tourPage.Built();
			firstPage = false;
		}

		// Append the user's CSS to the tour's CSS.
		if (tour.HasCustomHtmlCss)
        {
            // Rename CSS IDs to be unique for this tour.
            string content = tour.CustomHtmlCss;
            content = content.Replace("#ma", string.Format("#ma-1-{0}-", tour.Id));

            cssStringBuilder.Append(content);
        }

		// Create the JavaScript that will make the CSS available to the tour loader.
		string fileContent = string.Format("export let css =\n`{0}`;", cssStringBuilder.ToString());
		
		// Create the CSS loader JS file.
		string cssFileLocationAbsolute = previewFolderLocationAbsolute + "\\" + tour.NameForTourCssJsFile;
		FileManager.CreateTextFile(cssFileLocationAbsolute, fileContent);
	}

	public void CreateDownloadFile()
	{
		string folderLocationAbsolute = tour.IsPrivate ? previewFolderLocationAbsolute : publishedFolderLocationAbsolute;

		CopyRuntimeFile(RuntimeFile.DownloadReadMe, folderLocationAbsolute);

		// Delete the previous download file if there is one.
		string downloadFileLocation = tour.DownloadFileLocation;
		if (FileManager.FileExists(downloadFileLocation))
			FileManager.DeleteFile(downloadFileLocation);
		
		// Get a list of the files in the folder. Do this before creating the new
		// download file so that it won't appear in the list.
		string[] fileLocations = FileManager.FolderEntries(folderLocationAbsolute);

		ArrayList publishedFilesExclusionList = CreatePublishedFilesExclusionList();

		// Create the download zip file.
		ZipOutputStream zipStream = new ZipOutputStream(File.Create(downloadFileLocation));

		// Set zip compression level: 0 [none] - 9 [highest]
		zipStream.SetLevel(0);

		// Add an entry to the zip file for each file in the folder.
		for (int i = 0; i < fileLocations.Length; i++)
		{
			string fileLocation = fileLocations[i];
			string fileName = new FileInfo(fileLocation).Name;

			fileName = ConvertUnpublishedFileName(fileName);

			if (publishedFilesExclusionList.Contains(fileName))
			{
				// The tour does not need this file.
				continue;
			}

			FileStream fileStream = File.OpenRead(fileLocation);
			byte[] buffer = new byte[fileStream.Length];
			fileStream.Read(buffer, 0, buffer.Length);
			ZipEntry entry = new ZipEntry(fileName);
			
			// Set the entry's size to prevent SharpZipLib from using Zip64 which can't be read on XP.
			// For more info see: http://blog.tylerholmes.com/2008/12/windows-xp-unzip-errors-with.html.
			entry.Size = new FileInfo(fileLocation).Length;
			
			zipStream.PutNextEntry(entry);
			zipStream.Write(buffer, 0, buffer.Length);
			fileStream.Close();
		}
		zipStream.Finish();
		zipStream.Close();

		tour.DownloadFileCreated();
	}

	private string ConvertUnpublishedFileName(string fileName)
	{
		if (tour.V3CompatibilityEnabled)
        {
			// Determine if this is one of the page htm files and if so, convert it to its published
			// file name page1.htm, page2.htm etc. (or page1_.htm, page2_.htm etc. if unbranded).
			foreach (TourPage tourPage in tour.TourPages)
			{
				if (fileName == tourPage.NameForPageHtmlPreviewFileV3)
					return tourPage.NameForPageHtmlPublishedFileV3;

				if (fileName == tourPage.NameForPageHtmlUnbrandedPreviewFileV3)
					return tourPage.NameForPageHtmlUnbrandedPublishedFileV3;
			}
        }
		else
        {
			// Determine if this is the index file and if so, convert it to index.htm (or index_.htm if unbranded).
			if (fileName == tour.NameForTourIndexPreviewFile)
				return tour.NameForTourIndexPublishedFile;

			if (fileName == tour.NameForTourIndexUnbrandedPreviewFile)
				return tour.NameForTourIndexUnbrandedPublishedFile;
		}

		return fileName;
	}

	private bool PostProcessMapsAliveJavaScriptFile(RuntimeFile runtimeFile, string sourceLocation, string targetLocation, bool minify)
	{
		Debug.WriteLine("BUILD: " + "PostProcessMapsAliveJavaScriptFile " + sourceLocation + " : " + targetLocation);

		// Strip comments and console.log() calls from the file to reduce its size.
		try
		{
			FileStream fsIn = new FileStream(sourceLocation, FileMode.Open, FileAccess.Read);
			FileStream fsOut = new FileStream(targetLocation, FileMode.Create);
			StreamReader rd = new StreamReader(fsIn);
			StreamWriter sw = new StreamWriter(fsOut);
			string line = rd.ReadLine();

            // Emit a copyright and build time as the first line of a minified JavaScript file.
           if (minify)
            {
                DateTime now = DateTime.Now;
                string copyright = string.Format("// Copyright (C) 2006-{0} AvantLogic Corporation https://www.mapsalive.com", now.Year.ToString());
                string buildInfo = string.Format("{0} {1}", now.ToShortDateString(), now.ToLongTimeString());
                
                // When emitting files for tour, add the tour number and build Id. When building
                // one of the AppRuntime files like MemberPage.js, there will be no tour.
                if (tour != null)
                    buildInfo = string.Format("#{0} [{1}] {2}", tour.Id, tour.BuildId, buildInfo);
                
                sw.WriteLine(string.Format("{0} {1}", copyright, buildInfo));
            }

			while (line != null)
			{
				bool keep = true;

                if (minify)
                {
				    string trimmedLine = line.Trim();
				    if (trimmedLine.Length == 0 ||
                        trimmedLine.StartsWith("//") ||
                        trimmedLine.StartsWith("console.") ||
                        trimmedLine.StartsWith("MapsAliveRuntime.assert") ||
                        trimmedLine.StartsWith("debugger"))
				    {
					    // Remove blank lines, comments, asserts, and calls to the console or to invoke the debugger.
					    keep = false;
				    }
                }

				if (keep)
				{
					if (minify)
                    {
                        // Strip off mid-line comments. There must be a space after the // so that we don't strip things like "http://".
					    int index = line.IndexOf("// ");
					    if (index > 0)
						    line = line.Substring(0, index);
                    }

                    if (Runtime.RuntimeJavaScriptFilesV4().Contains(runtimeFile))
                        line = performMacroSubstitution(line, tour.Id);

					sw.WriteLine(line);
				}
				line = rd.ReadLine();
			}

			rd.Close();
			sw.Close();
			fsIn.Dispose();
			fsOut.Dispose();
		}
		catch (Exception ex)
		{
			Utility.ReportException("PostProcessMapsAliveJavaScriptFile " + sourceLocation, ex);
			return false;
		}
		
		return true;
	}

    private string performMacroSubstitution(string text, int tourId)
    {
        // Replace occurrences of "_$$"" with the tour Id. This special purpose macro feature is
        // used primarily to make V4 JavaScript class names unique for each runtime instance of a tour.
        // For example, "SomeName__$$" for tour 12345 gets converted to "SomeName_12345". Note the
        // use of the extra underscore which is not part of the macro which has only a single underscore.
        // This way the macro can also be used in a few special cases where it's necessary to hard-code
        // the tour Id into the runtime source, for example as a parameter to a callback function.
        //
        // Making class names unique is necessary so that when more than one tour is running in the same
        // container web page, each tour has its own unique set of class objects created from that tour's
        // runtime sources. Without the unique names, the class objects for every tour on the page would
        // get created using the runtime sources for the first tour in the page due to the fact that the
        // JavaScript module import mechanism ignores duplicate imports for a class name even if the module
        // is imported from a different tour folder. This would not be a problem if all tours on the page
        // were built using the exact same versions of the runtime files, but each tour could have been
        // built at a different time using different runtime files, with some of the older tours containing
        // bugs that were fixed, or functionality that changed, in newer runtime files. This scheme ensures
        // that each tour runs as it was built and is not side-affected by the runtime sources of other tours.

        return text.Replace("_$$", tourId.ToString());
    }

	public void CopyAppRuntimeFilesToAppRuntimeFolder()
	{
		Debug.WriteLine("BUILD: " + "CopyAppRuntimeFilesToAppRuntimeFolder");

		string runtimeFolder = FileManager.AppRuntimeFolderLocationAbsolute + "\\" + App.Version + "\\";
		
		if (!FileManager.FolderExists(runtimeFolder))
			FileManager.CreateFolder(runtimeFolder);

		// Copy the V3 runtime files to support compatibility
		foreach (RuntimeFile runtimeFile in Runtime.RuntimeJavaScriptFilesV3())
			CopyRuntimeFile(runtimeFile, runtimeFolder);

        foreach (RuntimeFile runtimeFile in Runtime.AppRuntimeJavaScriptFiles())
            CopyRuntimeFile(runtimeFile, runtimeFolder);

        // Copy the folder of HTML editor files to the app runtime folder.
        string htmlEditor = "HtmlEditor";
        CopyRuntimeFolder(htmlEditor, runtimeFolder + htmlEditor);
	}

    private void CopyRuntimeFolder(string folderName, string targetFolder)
    {
        string sourceFolder = runtimeFolderLocationAbsolute + "\\" + folderName;
        bool ok = FileManager.CopyFolder(sourceFolder, targetFolder);
        if (!ok)
            throw new Exception("CopyRuntimeFolder failed to copy " + folderName + " to " + targetFolder);
    }

    private void CopyRuntimeFile(RuntimeFile runtimeFile, string targetFolder)
	{
		Debug.WriteLine("BUILD: " + "CopyRuntimeFile " + runtimeFile + " : " + targetFolder);

		string sourceLocation = runtimeFolderLocationAbsolute + "\\" + Runtime.ProjectFileName(runtimeFile);
		string targetLocation = targetFolder + "\\" + Runtime.RuntimeFileName(runtimeFile);

		bool isRuntimeFile = 
            Runtime.RuntimeJavaScriptFilesV3().Contains(runtimeFile) ||
            Runtime.RuntimeJavaScriptFilesV4().Contains(runtimeFile) ||
            Runtime.AppRuntimeJavaScriptFiles().Contains(runtimeFile);
		
		// Determine if the file should be minified to strip out comments, console.log calls, and blank lines.
        bool minify = isRuntimeFile && !App.DeveloperMode;
		
        bool ok;
        if (isRuntimeFile)
            ok = PostProcessMapsAliveJavaScriptFile(runtimeFile, sourceLocation, targetLocation, minify);
        else
            ok = FileManager.CopyFile(sourceLocation, targetLocation);

        if (!ok)
			throw new Exception("CopyRuntimeFile failed to copy " + runtimeFile + " to " + targetFolder);
	}

	private void CopyRuntimeFilesToPreviewFolder()
	{
		Debug.WriteLine("BUILD: " + "CopyRuntimeFilesToPreviewFolder");

		// This is the complete set of runtime files that a tour could use. When the tour is published,
		// any of these files that the tour is not using are not copied to the published folder.

		foreach (RuntimeFile runtimeFile in Runtime.RuntimeJavaScriptFilesV3())
			CopyRuntimeFile(runtimeFile, previewFolderLocationAbsolute);
		
		foreach (RuntimeFile runtimeFile in Runtime.RuntimeJavaScriptFilesV4())
			CopyRuntimeFile(runtimeFile, previewFolderLocationAbsolute);
		
		foreach (RuntimeFile runtimeFile in Runtime.RuntimeJavaScriptFiles3rdParty())
			CopyRuntimeFile(runtimeFile, previewFolderLocationAbsolute);

		CopyRuntimeFile(RuntimeFile.LiveDataJs, previewFolderLocationAbsolute);
		CopyRuntimeFile(RuntimeFile.SoundManagerJs, previewFolderLocationAbsolute);
		CopyRuntimeFile(RuntimeFile.Blank, previewFolderLocationAbsolute);
		CopyRuntimeFile(RuntimeFile.LoadingLiveData, previewFolderLocationAbsolute);
 		CopyRuntimeFile(RuntimeFile.CloseX, previewFolderLocationAbsolute);
        CopyRuntimeFile(RuntimeFile.ZoomIn, previewFolderLocationAbsolute);
        CopyRuntimeFile(RuntimeFile.ZoomOut, previewFolderLocationAbsolute);

 		if (tour.V3CompatibilityEnabled)
        {
            CopyRuntimeFile(RuntimeFile.CloseTouchX, previewFolderLocationAbsolute);
        }
        else
        {
            CopyRuntimeFile(RuntimeFile.PopupCloseX, previewFolderLocationAbsolute);
            CopyRuntimeFile(RuntimeFile.ContentContract, previewFolderLocationAbsolute);
            CopyRuntimeFile(RuntimeFile.ContentExpand, previewFolderLocationAbsolute);
            CopyRuntimeFile(RuntimeFile.DirContract, previewFolderLocationAbsolute);
            CopyRuntimeFile(RuntimeFile.DirExpand, previewFolderLocationAbsolute);
            CopyRuntimeFile(RuntimeFile.DirSearch, previewFolderLocationAbsolute);
            CopyRuntimeFile(RuntimeFile.NavButton, previewFolderLocationAbsolute);
            CopyRuntimeFile(RuntimeFile.HelpButton, previewFolderLocationAbsolute);
            CopyRuntimeFile(RuntimeFile.MobileCloseX, previewFolderLocationAbsolute);
            CopyRuntimeFile(RuntimeFile.CurrentPage, previewFolderLocationAbsolute);
            CopyRuntimeFile(RuntimeFile.Offline, previewFolderLocationAbsolute);
        }

        CopyRuntimeFile(RuntimeFile.CloseHelpX, previewFolderLocationAbsolute);
 		CopyRuntimeFile(RuntimeFile.Pin1, previewFolderLocationAbsolute);
 		CopyRuntimeFile(RuntimeFile.Pin2, previewFolderLocationAbsolute);
		CopyRuntimeFile(RuntimeFile.Pin1Animated, previewFolderLocationAbsolute);

        if (tour.V3CompatibilityEnabled)
        {
 		    CopyRuntimeFile(RuntimeFile.ArrowLeft1, previewFolderLocationAbsolute);
 		    CopyRuntimeFile(RuntimeFile.ArrowLeft2, previewFolderLocationAbsolute);
 		    CopyRuntimeFile(RuntimeFile.ArrowRight1, previewFolderLocationAbsolute);
 		    CopyRuntimeFile(RuntimeFile.ArrowRight2, previewFolderLocationAbsolute);
 		    CopyRuntimeFile(RuntimeFile.ArrowUp1, previewFolderLocationAbsolute);
 		    CopyRuntimeFile(RuntimeFile.ArrowUp2, previewFolderLocationAbsolute);
 		    CopyRuntimeFile(RuntimeFile.ArrowDown1, previewFolderLocationAbsolute);
 		    CopyRuntimeFile(RuntimeFile.ArrowDown2, previewFolderLocationAbsolute);
        }

		CopyRuntimeFile(RuntimeFile.DirGroup, previewFolderLocationAbsolute);
		CopyRuntimeFile(RuntimeFile.DirSortAZ, previewFolderLocationAbsolute);
	}

	private bool CopyPreviewFilesToPublishedFolder(ArrayList previewFiles, ArrayList publishedFilesExclusionList)
	{
		bool ok = true;

		// Create a table of file names that have to be renamed in the published folder.
		Hashtable fileRenameTable = CreateFileRenameTable();

		// Copy files from preview folder to the published folder that are needed by the published tour.
		foreach (FileInfo previewFileInfo in previewFiles)
		{
			// Get the name of the file to be copied from the preview folder.
			string fileName = previewFileInfo.Name;

			string sourceFileLocation = FileManager.PreviewFolderLocationAbsolute(tour.Id, fileName);
			string targetFileLocation = FileManager.PublishedFolderLocationAbsolute(tour.Id, fileName);

			// Determine the name to use for the file in the published folder. Most files retain their orginal
			// names, but the page html file names in the preview folder are the page's Id number e.g. 3409.
			// THe numberic names are a security measure against someone trying to browse the tour's page files
			string newFileName = (string)fileRenameTable[fileName];
			if (newFileName != null)
			{
				if (newFileName == "index.htm" && tour.V4)
                {
					// Create the other flavors of the default page: default.htm and index.aspx so that
					// every kind of web server will be happy. The .aspx flavor got added for the Facebook
					// app which returned a 405 error when using index.htm.

					targetFileLocation = FileManager.PublishedFolderLocationAbsolute(tour.Id, "index.aspx");
					FileManager.CopyFile(sourceFileLocation, targetFileLocation);
					targetFileLocation = FileManager.PublishedFolderLocationAbsolute(tour.Id, "default.htm");
					FileManager.CopyFile(sourceFileLocation, targetFileLocation);
				}

				fileName = newFileName;
				targetFileLocation = FileManager.PublishedFolderLocationAbsolute(tour.Id, fileName);
			}

			if (publishedFilesExclusionList.Contains(fileName))
			{
				// The tour does not need this file.
				continue;
			}

            if (tour.V3CompatibilityEnabled && IsV4MapFileName(fileName))
            {
                // V3 tours use maps tiles. This is one of the V4 map image files.
                continue;
            }

			Debug.WriteLine("BUILD: Copy " + sourceFileLocation + " TO " + targetFileLocation);
			bool copied = FileManager.CopyFile(sourceFileLocation, targetFileLocation);
			if (!copied)
				ok = false;
		}

		return ok;
	}

    private bool IsV4MapFileName(string fileName)
    {
        return fileName.EndsWith("_25.jpg") || fileName.EndsWith("_50.jpg") || fileName.EndsWith("_100.jpg");
    }

	private Hashtable CreateFileRenameTable()
	{
		Hashtable list = new Hashtable();

		string oldName;
		string newName;

		// Rename the tour's index files (index.htm in V4 and page1.htm etc. in V3) so that they can be
		// accessed via the tour's URL e.g. tour.mapsalive.com/12345/index.htm. The rename is needed
		// because in the preview folder, index file names are simply a numeric pattern e.g. 2595.82991
		// that cannot be used via the tour's URL. This is security measure to prevent someone from
		// running a tour from the preview folder.

		if (tour.V3CompatibilityEnabled)
		{
			// Rename the tour's page files to be page1.htm, page2.htm etc. (or page1_.htm, page2_.htm etc. if unbranded).
			foreach (TourPage tourPage in tour.TourPages)
			{
				oldName = tourPage.NameForPageHtmlPreviewFileV3;
				newName = tourPage.NameForPageHtmlPublishedFileV3;
				list.Add(oldName, newName);

				if (tour.HasBanner && tour.CanAppearUnbranded)
				{
					oldName = tourPage.NameForPageHtmlUnbrandedPreviewFileV3;
					newName = tourPage.NameForPageHtmlUnbrandedPublishedFileV3;
					list.Add(oldName, newName);
				}
			}
		}
		else
		{
			// Rename the tour's default file to be index.htm (or index_.htm if unbranded).
			oldName = tour.NameForTourIndexPreviewFile;
			newName = tour.NameForTourIndexPublishedFile;
			list.Add(oldName, newName);
		}

		return list;
	}

	private void CreateMarkerInstance(BaseLayer layer, ArrayList mapMarkers, int zIndex, TourView tourView)
	{
		MarkerDefinition markerDefinition;

		if (tourView.MarkerIsRoute)
		{
			if (routeMarkerDefinition == null)
			{
				// This is the first time a route marker instance is being created. Create a definition
				// for it. We do this because route marker's do not have a marker style and so we have
				// to make one up on the fly here.
				routeMarkerDefinition = MarkerDefinition.CreateMarkerDefinitionForRoute();
			}
			markerDefinition = routeMarkerDefinition;
		}
		else
		{
			string id;
			Marker marker = Account.GetCachedMarker(tourView.MarkerId);
			if (marker.MarkerType == MarkerType.Photo || marker.MarkerType == MarkerType.Text)
			{
				id = string.Format("{0}_{1}", tourView.MarkerId, tourView.Id);
			}
			else
			{
				id = tourView.MarkerId.ToString();
			}
			markerDefinition = (MarkerDefinition)markerDefinitions[id];
		}
		
		BaseMarker baseMarker = Marker.CreateMarkerInstance(markerDefinition, tourView, layer, zIndex);
		int mapMarkerIndex = GetMapMarkersSequenceIndex(tourView);
		if (mapMarkerIndex < mapMarkers.Count)
		{
			mapMarkers[mapMarkerIndex] = baseMarker;
		}
		else
		{
			// This should never happen, but it does every so often and we don't know the sequence that
			// causes it. Somehow a user can delete a hotspot from a map, but it's marker still exists.
			// This doesn't appear to cause any harm, so we silently report the problem and continue building.
			Utility.ReportEvent("CreateMarkerInstance", "Ghost marker index is out of range -- ignored");
		}
	}
	
	private void CreateMarkerInstances(TourPage tourPage, BaseLayer layer, ArrayList mapMarkers)
	{
		Debug.WriteLine("BUILD: " + "CreateMarkerInstances " + tourPage.Name);

		int zIndex = 0;

		ArrayList hybrids = new ArrayList();
		ArrayList polygons = new ArrayList();
		ArrayList lines = new ArrayList();
		ArrayList circlesAndRectangles = new ArrayList();
		ArrayList symbols = new ArrayList();
		ArrayList notAnchored = new ArrayList();

		// Examine the tour view to determine what kind of marker it has.
		// Divide the markers into groups according to how they should be stacked: 
		// polygons at the bottom, then lines, circles and rectangles, and symbols on top.
		foreach (TourView tourView in tourPage.TourViewsBySequence)
		{
			if (tourView.MarkerIsRoute)
			{
				lines.Add(tourView);
			}
			else if (tourView.MarkerIsNotAnchored)
			{
				notAnchored.Add(tourView);
			}
			else
			{
				Marker marker = Account.GetCachedMarker(tourView.MarkerId);
				ShapeType shapeType = (ShapeType)marker.ShapeType;

				if (marker.ShapeType == 0)
				{
					symbols.Add(tourView);
				}
				else
				{
					switch (shapeType)
					{
						case ShapeType.Circle:
						case ShapeType.Rectangle:
							circlesAndRectangles.Add(tourView);
							break;

						case ShapeType.Hybrid:
							hybrids.Add(tourView);
							break;

						case ShapeType.Line:
							lines.Add(tourView);
							break;

						case ShapeType.Polygon:
							polygons.Add(tourView);
							break;

						default:
							Debug.Fail("Unsupported shape type " + shapeType);
							break;
					}
				}
			}
		}

		// Create the marker instances starting at the lowest layer and moving up.
		CreateMarkerInstanceLayer(layer, mapMarkers, ref zIndex, hybrids);
		CreateMarkerInstanceLayer(layer, mapMarkers, ref zIndex, polygons);
		CreateMarkerInstanceLayer(layer, mapMarkers, ref zIndex, lines);
		CreateMarkerInstanceLayer(layer, mapMarkers, ref zIndex, circlesAndRectangles);
		CreateMarkerInstanceLayer(layer, mapMarkers, ref zIndex, symbols);
		CreateMarkerInstanceLayer(layer, mapMarkers, ref zIndex, notAnchored);
	}

	private void CreateMarkerInstanceLayer(BaseLayer layer, ArrayList mapMarkers, ref int zIndex, ArrayList group)
	{
		foreach (TourView tourView in group)
		{
			zIndex++;
			CreateMarkerInstance(layer, mapMarkers, zIndex, tourView);
		}
	}

	private ArrayList CreatePublishedFilesExclusionList()
	{
		ArrayList list = new ArrayList();

		if (tour.V3CompatibilityEnabled)
        {
			// Exclude the tour loader and tour properties JS files that is not used by V3 tours.
			list.Add(tour.NameForTourLoaderJsFile);
			list.Add(tour.NameForTourPropertiesJsFile);

			// Exclude the files that become index.htm and index_.htm.
			list.Add(tour.NameForTourIndexPreviewFile);
			list.Add(tour.NameForTourIndexUnbrandedPreviewFile);

			// Exclude runtime files not used by V3.
			foreach (RuntimeFile runtimeFile in Runtime.RuntimeJavaScriptFilesV4())
				list.Add(Runtime.RuntimeFileName(runtimeFile));
			foreach (RuntimeFile runtimeFile in Runtime.RuntimeJavaScriptFiles3rdParty())
				list.Add(Runtime.RuntimeFileName(runtimeFile));

            // Exclude the new images used by V4.
            list.Add(Runtime.RuntimeFileName(RuntimeFile.PopupCloseX));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.ContentContract));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.ContentExpand));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.DirContract));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.DirExpand));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.DirSearch));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.NavButton));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.HelpButton));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.MobileCloseX));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.CurrentPage));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.ZoomIn));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.ZoomOut));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.Offline));
        }
        else
        {
			foreach (RuntimeFile runtimeFile in Runtime.RuntimeJavaScriptFilesV3())
				list.Add(Runtime.RuntimeFileName(runtimeFile));

            // Exclude the V3 popup arrow graphics which are replaced by SVG in V4.
            list.Add(Runtime.RuntimeFileName(RuntimeFile.ArrowDown1));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.ArrowDown2));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.ArrowLeft1));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.ArrowLeft2));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.ArrowRight1));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.ArrowRight2));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.ArrowUp1));
            list.Add(Runtime.RuntimeFileName(RuntimeFile.ArrowUp2));
           
            // Exclude the V3 style popup close X.
            list.Add(Runtime.RuntimeFileName(RuntimeFile.CloseTouchX));
		}

		if (!tour.UsesLiveData)
		{
			list.Add(Runtime.RuntimeFileName(RuntimeFile.LiveDataJs));
		}

		if (!tour.UseSoundManager)
		{
			list.Add(Runtime.RuntimeFileName(RuntimeFile.SoundManagerJs));
		}

		foreach (TourPage tourPage in tour.TourPages)
		{
			// Exclude the map JS file that is only used by the Map Editor in the Tour Builder.
			list.Add(string.Format(PatternForMapJsFile, tourPage.PageNumber, tour.BuildId));

			// Exclude the preview HTML files that become page1.htm, page2.htm etc. (or page1_.htm, page2_.htm etc. for branded tours).
			list.Add(string.Format(PatternForPageHtmlPreviewFile, tourPage.Id, tour.BuildId));
			list.Add(string.Format(PatternForPageHtmlPreviewFileV3, tourPage.Id));
			list.Add(string.Format(PatternForPageHtmlUnbrandedPreviewFile, tourPage.Id, tour.BuildId));
			list.Add(string.Format(PatternForPageHtmlUnbrandedPreviewFileV3, tourPage.Id));

			// Exclude the tour page XML dump files that are only used by developers.
			list.Add(string.Format("_dump{0}.xml", tourPage.Id));

			if (tour.V3CompatibilityEnabled)
            {
				// Exclude the page JS, CSS, symbols, and map tiles files that are only used by the Map Editor.
				list.Add(string.Format(PatternForPageJsFile, tourPage.PageNumber, tour.BuildId));
				list.Add(string.Format(PatternForPageCssFile, tourPage.PageNumber, tour.BuildId));
				list.Add(string.Format(PatternForSymbolsFile, tourPage.PageNumber, tour.BuildId));
				list.Add(string.Format(PatternForTourCssJsFile, tour.BuildId));
				list.Add(string.Format(PatternForTourHtmlJsFile, tour.BuildId));
				list.Add(string.Format(PatternForTourCustomJsFile, tour.BuildId));
            }
		}

		return list;
	}

	public void DeletePublishedTour()
	{
		Debug.WriteLine("BUILD: " + "DeletePublishedTour");
		
		if (FileManager.FolderExists(publishedFolderLocationAbsolute))
			FileManager.DeleteFolder(publishedFolderLocationAbsolute);
	}

	public void DeleteTourFolder()
	{
		Debug.WriteLine("BUILD: " + "DeleteTourFolder");
		
		FileManager.DeleteFolder(previewFolderLocationAbsolute);
		FileManager.DeleteFolder(publishedFolderLocationAbsolute);
	}
	
	private bool DeleteTourFolderContents(string folderName)
	{
		FileManager.DeleteFolderContents(folderName);

		string[] fileLocations = FileManager.FolderEntries(folderName);

		if (fileLocations.Length > 0)
		{
			// One or more files were not deleted. In theory this should never happen but we occasionally
			// see an Access Denied error on a file like mapsalive.js or mapviewer.js. Try to delete the
			// folder contents a second time.
			Utility.ReportEvent("BuildTourFolder", "2nd attempt to delete files in " + folderName);
			FileManager.DeleteFolderContents(folderName);
			fileLocations = FileManager.FolderEntries(folderName);

			if (fileLocations.Length > 0)
			{
				// There are still undeleted files. Find out what the files are and report an error to
				// ourselves and tell the user there was a problem and ask them to try again.
				string fileNames = string.Empty;
				for (int i = 0; i < fileLocations.Length; i++)
				{
					string fileLocation = fileLocations[i];
					fileNames += "\n" + fileLocation;
				}
				Utility.ReportEvent("BuildTourFolder", "Could not delete these files:" + fileNames);
				return false;
			}
		}

		return true;
	}

	private void EmitFile(string contentId, int pageId, string fileName, MemoryStream tourXmlMemoryStream, bool buildingV3Files)
    {
		Debug.WriteLine("BUILD: " + "EmitFile " + fileName);

		Exception exception;
		MemoryStream outputStream = null;
		StreamReader streamReader = null;
		XmlReader xmlReader = null;
		XmlWriter xmlWriter = null;

		string fileLocationAbsolute = previewFolderLocationAbsolute + "\\" + fileName;
			
		try
		{
			// Set up parameters to pass to the xslt.
			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("contentId", "", contentId);
			arguments.AddParam("pageId", "", pageId);
			arguments.AddParam("customHtmlTop", "", RemoveLineBreaksAndTabs(tour.CustomHtmlTop));
			arguments.AddParam("customHtmlBottom", "", RemoveLineBreaksAndTabs(tour.CustomHtmlBottom));
			arguments.AddParam("customHtmlAbsolute", "", RemoveLineBreaksAndTabs(tour.CustomHtmlAbsolute));
			
			// Position to the start of the XML memory stream.
			tourXmlMemoryStream.Position = 0;
			xmlReader = XmlReader.Create(tourXmlMemoryStream);

			if (contentId == "html" || contentId == "html_")
			{
				// For HTML data, the output of the XSLT transform is written directly to the file.  We pass
				//the output settings from the XSLT file so that they will be emitted into the output.
				xmlWriter = XmlWriter.Create(fileLocationAbsolute, outputSettings);
				xslt.Transform(xmlReader, arguments, xmlWriter);
			}
			else
			{
				// All other data must first be transformed and post processesed. Instead of writing
				// the XSLT output to a file, we write it to a stream and then create a file from that.
				// Note that contentId "css" is only for V3 whereas "css-tour" and "css-page" are only for V4.
				outputStream = new MemoryStream();
				xslt.Transform(xmlReader, arguments, outputStream);
				outputStream.Position = 0;
				streamReader = new StreamReader(outputStream);

				// Read the output stream line by line.  Process each line and append the
				// result to the string that will be written to the file.
				string fileContent = string.Empty;
				string line;
				while ((line = streamReader.ReadLine()) != null)
				{
					string trimmedLine = line.Trim();
					if (trimmedLine.Length == 0)
						continue;

					if (contentId == "css-tour" || contentId == "css-page" || contentId == "css")
					{
						// For CSS we remove leading and trailing spaces.
						fileContent += trimmedLine + "\n";
					}
					else if (contentId == "js" || contentId == "tour" || contentId == "html-tour")
                    {
                        // Strip off the DOCTYPE tag that XSLT emits automatically for HTML.
                        if (line.StartsWith("<!DOCTYPE"))
                        {
                            line = line.Substring(line.IndexOf('>') + 1);
                        }

						if (buildingV3Files)
							line = EmitFileContentForV3(buildingV3Files, line);

                        // A carriage return at the end of the line.
                        fileContent += line + "\n";
                    }
                }

				if (contentId == "css-tour" || contentId == "css-page")
                {
					// Give each CSS Id style a unique name so that this tour's CSS can coexist on a webpage with the CSS of other
                    // tours. Rename Id styles that starts with "#ma" to include an instance number and tour Id. The instance number
                    // is always 0, but will get changed to another number by the runtime JavaScript if there is more than one
                    // instance of the same tour in the same HTML container page. Class styles that start with '.ma' are left alone
                    // so that class styles can be applied to all tours.
					fileContent = fileContent.Replace("#ma", string.Format("#ma-1-{0}-", tour.Id));

					// Append the contents to a string instead of creating a file. The string will accumulate
					// the CSS for all the pages and tour itself and later get put into a single file.
					cssStringBuilder.Append(fileContent);
                }
				else
                {
					if (contentId == "html-tour")
					{
						// Create unique Ids in the tour HTML e.g. change id="maPageTitle" to id="ma-1-02345-PageTitle"
						// where 0 is the zero-based instance number and 12345 is the tour Id.
						fileContent = fileContent.Replace("id=\"ma", string.Format("id=\"ma-1-{0}-", tour.Id));
					}

					// Create the actual file.
					FileManager.CreateHtmlFile(fileLocationAbsolute, fileContent, false);
                }
			}

			return;
		}
		catch (Exception ex)
		{
			exception = ex;
		}
		finally
		{
			if (streamReader != null)
				streamReader.Close();
			if (outputStream != null)
				outputStream.Close();
			if (xmlReader != null)
				xmlReader.Close();
			if (xmlWriter != null)
				xmlWriter.Close();
		}

		Utility.ReportException("EmitFile " + fileName, exception);
	}

    private string EmitFileContentForV3(bool buildingV3Files, string line)
    {
        // Insert user HTML.
        const string tagTop = "<customHtmlTop></customHtmlTop>";
        const string tagBottom = "<customHtmlBottom></customHtmlBottom>";
        const string tagFloating = "<customHtmlAbsolute></customHtmlAbsolute>";

        if (line.Contains(tagTop))
        {
            string customHtmlTop = RemoveLineBreaksAndTabs(tour.CustomHtmlTop);
            if (customHtmlTop.Length > 0)
                customHtmlTop = WrapCustomHtml("maCustomHtmlTop", customHtmlTop);
            line = line.Replace(tagTop, customHtmlTop);
        }

        if (line.Contains(tagBottom))
        {
            string customHtmlBottom = RemoveLineBreaksAndTabs(tour.CustomHtmlBottom);
            if (customHtmlBottom.Length > 0)
                customHtmlBottom = WrapCustomHtml("maCustomHtmlBottom", customHtmlBottom);
            line = line.Replace(tagBottom, customHtmlBottom);
        }

        if (line.Contains(tagFloating))
        {
            string customHtmlAbsolute = RemoveLineBreaksAndTabs(tour.CustomHtmlAbsolute);
            if (customHtmlAbsolute.Length > 0)
                customHtmlAbsolute = WrapCustomHtml("maCustomHtmlAbsolute", customHtmlAbsolute);
            line = line.Replace(tagFloating, customHtmlAbsolute);
        }

        // Prepare the line to be added to the file. If the line itself starts with document.writeln(),
        // we simply emit the line, otherwise we wrap the line in the function call. This way the XSLT has
        // the option of emitting JavaScript logic that will be written here as-is instead of as the
        // string parameter to the document.writeln function.
        if (line.TrimStart().StartsWith("document.writeln("))
        {
            line = string.Format("{0}", line.Trim());
        }
        else
        {
            // Escape characters that are not allowed in JavaScript.
            line = EscapeSlashesAndQuotes(line);
            line = string.Format("document.writeln('{0}');", line);
        }

        return line;
    }

    private static string EscapeSlashesAndQuotes(string line)
	{
		// Escape back slashes and single quotes.
		line = line.Replace("\\", "\\\\");
		line = line.Replace("'", "\\'");
		line = line.Replace("\t", "");
		return line;
	}

	private ArrayList GetFileInfoList(string folderLocationAbsolute)
	{
		ArrayList list = new ArrayList();
		string[] fileLocations = FileManager.FolderEntries(folderLocationAbsolute);

		for (int i = 0; i < fileLocations.Length; i++)
		{
			string fileLocation = fileLocations[i];
			FileInfo fileInfo = new FileInfo(fileLocation);
			list.Add(fileInfo);
		}

		return list;
	}

	private int GetMapMarkersSequenceIndex(TourView markerTourView)
	{
		// This method returns an index indicating which position markerTourView should be placed
		// in the mapMarkers array. We can't use the tourView's sequence number because there can
		// be holes in the sequence caused by hotspots that have been deleted. For example, if a
		// map originally had four hotspots with sequence 1, 2, 3, 4, the corresponding index values
		// would be 0, 1, 2, 3. However if hotspot 3 got deleted, then the hotspot sequence would
		// become 1, 2, 4 and the index sequence would need to be 0, 1, 2.
		int index = 0;
		foreach (TourView tourView in markerTourView.TourPage.TourViewsBySequence)
		{
			if (tourView.Id == markerTourView.Id)
			{
				break;
			}
			index++;
		}
		return index;
	}

	public ArrayList MapMarkers(int tourPageId)
	{
		return (ArrayList)mapMarkersTable[tourPageId];
	}

	public Hashtable MarkerDefinitions
	{
		get { return markerDefinitions; }
	}

	public bool PublishTour()
	{
		Debug.WriteLine("BUILD: " + "PublishTour");

		Utility.RecordAction(MemberPageActionId.PublishTour);

		// Make sure the preview folder exists (in case this is the first publish).
		if (!FileManager.FolderExists(publishedFolderLocationAbsolute))
		{
			FileManager.CreateFolder(publishedFolderLocationAbsolute);
		}

		// Create lists of files needed for synchronization.
		ArrayList previewFiles = GetFileInfoList(previewFolderLocationAbsolute);
		ArrayList publishedFilesExclusionList = CreatePublishedFilesExclusionList();

		if (!DeleteTourFolderContents(publishedFolderLocationAbsolute))
			return false;

		bool ok = CopyPreviewFilesToPublishedFolder(previewFiles, publishedFilesExclusionList);

		// Update the database with the publish date.
		tour.PublishCompleted();

		return ok;
	}

	private static string RemoveLineBreaksAndTabs(string customHtml)
	{
		customHtml = customHtml.Replace(Utility.CrLf, "");
		customHtml = customHtml.Replace("\n", "");
		customHtml = customHtml.Replace("\t", "");

		// Escape backtick to be a character in the custom HTML instead of a JavaScript backtick.
		customHtml = customHtml.Replace("`", "\\`");
		
		return customHtml;
	}

	private void RenumberPages()
	{
		int pageNumber = 0;
		foreach (TourPage tourPage in tour.TourPages)
		{
			pageNumber++;
			tourPage.ChangePageNumber(pageNumber);
		}
	}

	public void SetTourState(TourState newState, TourState oldState)
	{
		try
		{
			string oldName;
			string newName;

			if (tour.V3CompatibilityEnabled)
			{

				foreach (TourPage tourPage in tour.TourPages)
				{
					switch (newState)
					{
						case TourState.Active:
							// Activate a deactivated tour.  A deactivated tour has a normal page name e.g.
							// page1.htm (versions < 1.57 == TourState.ExpiredPre_1_57) or page1.js for each page,
							// but those pages simply display a message saying the tour is deactivated.  We
							// need to delete those pages and rename the actual page files from their disabled
							// name (just the page Id) to their real name.
							oldName = publishedFolderLocationAbsolute + "\\" + tourPage.ActiveFileNameDisabled;
							newName = publishedFolderLocationAbsolute + "\\" + tourPage.ActiveFileNameEnabled(oldState);

							// Check to see if their is a file for this page.  There might not be if the page
							// was added, but never built.
							if (FileManager.FileExists(oldName) && FileManager.FileExists(newName))
							{
								// Delete the file that displays the deactivated message.
								FileManager.DeleteFile(newName);

								// Restore the actual page file by giving it an official page name.
								FileManager.RenameFile(oldName, newName);

								// Update the file's timestamp.  If we don't do this, the browser will think 
								// the file is unchanged and render the cached version of the deactivated file.
								FileManager.TouchFile(newName);
							}
							break;

						case TourState.Expired:
							// Deactivate a tour by renaming all of its page Javascript files to be just the page's
							// Id and then creating a new set of files that display a message saying the tour
							// is not active.  By doing this we preserve the original files so that we can
							// reactivate the tour by simply renaming them back to their official names.
							oldName = publishedFolderLocationAbsolute + "\\" + tourPage.NameForPageJsFile;
							if (FileManager.FileExists(oldName))
							{
								string script = string.Format(Utility.DeactivatedPageJavascript, App.WebSiteUrl, tour.Id, tour.TourSize.Width, tour.TourSize.Height);
								newName = publishedFolderLocationAbsolute + "\\" + tourPage.ActiveFileNameDisabled;
								FileManager.RenameFile(oldName, newName);
								FileManager.CreateHtmlFile(oldName, script, false);
							}
							break;

						default:
							Debug.Fail("Unexpected state " + newState);
							break;
					}
				}
			}
			else
            {
				switch (newState)
				{
					case TourState.Active:
						oldName = publishedFolderLocationAbsolute + "\\" + tour.NameForTourLoaderDeactivatedJsFile;
						newName = publishedFolderLocationAbsolute + "\\" + tour.NameForTourLoaderJsFile;

						if (FileManager.FileExists(oldName) && FileManager.FileExists(newName))
						{
							// Delete the file that displays the deactivated message and restore the original file.
							FileManager.DeleteFile(newName);
							FileManager.RenameFile(oldName, newName);

							// Update the file's timestamp.  If we don't do this, the browser may think 
							// the file is unchanged and render the cached version of the deactivated file.
							FileManager.TouchFile(newName);
						}
						break;

					case TourState.Expired:
						oldName = publishedFolderLocationAbsolute + "\\" + tour.NameForTourLoaderJsFile;
						newName = publishedFolderLocationAbsolute + "\\" + tour.NameForTourLoaderDeactivatedJsFile;
						if (FileManager.FileExists(oldName))
						{
							string url = string.Format("{1}ExpiredTour.ashx?maTourId={0}", tour.Id, App.WebSiteUrl);
							string script = string.Format("window.location.href='{0}';", url);
							FileManager.RenameFile(oldName, newName);
							FileManager.CreateHtmlFile(oldName, script, false);
						}
						break;

					default:
						Debug.Fail("Unexpected state " + newState);
						break;
				}
			}
		}
		catch (Exception ex)
		{
			string msg = string.Format("Tour:{0} OldState:{1} NewState:{2} Exception: {3}", tour.Id, oldState.ToString(), newState.ToString(), ex.Message);
			Utility.ReportError("TourBuilder::SetTourState", msg);
		}
	}
		private static string WrapCustomHtml(string id, string content)
	{
		// Wrap the user's HTML in a div with an Id having a name corresponding to a MapsAlive-generated CSS class.
		return string.Format("<div id=\"{0}\">{1}</div>", id, content);
	}

	#endregion
}