// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Configuration;
using System.Collections;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using AvantLogic.MapsAlive;
using AvantLogic.MapsAlive.Engine;

public class TourPageXmlWriter
{
	private enum MarkerInstanceFlags
	{
		IsDisabled = 0x00000001,
		IsHidden = 0x00000002,
		IsStatic = 0x00000004,
		IsRoute = 0x00000008,
		IsLocked = 0x00000010,
		MarkerZooms = 0x00000020,
		IsShapeOnly = 0x00000040,
		IsNotAnchored = 0x00000080,
		IsBound = 0x00000100,
		DoesNotShowContent = 0x00000400
	}

	public struct DirectoryRow
	{
		public int Depth;
		public int Id;
        public int PageNumber;

		public DirectoryRow(int depth, int id, int pageNumber)
		{
			Depth = depth;
			Id = id;
            PageNumber = pageNumber;
		}
	}

	public struct SlideTableEntry
	{
		public string Title;
		public string Data;

		public SlideTableEntry(string title, string data)
		{
			// Note that the title is also contained in the Data field, but we use
			// a separate title field to make it easier to sort the table entries.
			Title = title;
			Data = data;
		}
	}

	public class SlideDataComparer : IComparer
	{
		int IComparer.Compare(Object o1, Object o2)
		{
			string title1 = ((SlideTableEntry)o1).Title;
			string title2 = ((SlideTableEntry)o2).Title;
			return string.Compare(title1, title2, true);
		}
	}

	private bool allPages;
	private TourPage currentTourPage;
	private ArrayList directoryTable;
	private ArrayList imageDataTable;
	private	ArrayList markerInstanceTable;
	private ArrayList markerStylesInUse;
	private	ArrayList markerStyleTable;
	private ArrayList pageTable;
	private SlideMacroProcessor slideMacroProcessor;
	private	ArrayList stringTable;
	private ArrayList slideTable;
	private Tour tour;
	private TourBuilder tourBuilder;
	private XmlWriter xmlWriter;

	public TourPageXmlWriter(XmlWriter xmlWriter, Tour tour, TourBuilder tourBuilder)
	{
		this.xmlWriter = xmlWriter;
		this.tour = tour;
		this.tourBuilder = tourBuilder;
		slideMacroProcessor = new SlideMacroProcessor(tour);
	}

	// The data emited for runtime JavaScript tables consists only of integer values.
	// For string data we emit an integer index into the runtime string table.

	private void AddToMarkerInstanceTable(BaseMarker baseMarker)
	{
		int tourViewId = baseMarker.MarkerInstance.TargetViewId;
		TourView tourView = currentTourPage.GetTourView(tourViewId);

		if (tourView == null)
		{
			Debug.Fail("AddToMarkerInstanceTable", "No TourView found for Id " + tourViewId);
			return;
		}

		var isRoute = baseMarker.MarkerInstance.IsRoute;

		Marker marker = isRoute ? null : Account.GetCachedMarker(tourView.MarkerId);

		int markerTypeId = isRoute ? (int)MarkerType.Shape : (int)marker.MarkerType;
		int shapeTypeId = isRoute ? (int)ShapeType.Hybrid : (int)marker.ShapeType;
		int markerStyleId = isRoute ? 0 : marker.MarkerStyleId;
		int normalSymbolId = isRoute ? 0 : marker.NormalSymbolId;
		int selectedSymbolId = isRoute ? 0 : marker.SelectedSymbolId;

		int shapeCoordsIndex = AddToStringTable(isRoute ? string.Empty : marker.ShapeCoords);

		Size shapeSize = isRoute ? Size.Empty : marker.ShapeType == ShapeType.None ? marker.SymbolSize() : marker.ShapeRectangle.Size;
		Size normalSymbolSize = Size.Empty;
		Size selectedSymbolSize = Size.Empty;
		int symbolLocationX = 0;
		int symbolLocationY = 0;

		if (!isRoute)
		{
			if (marker.MarkerType == MarkerType.Photo || marker.MarkerType == MarkerType.Text)
			{
				// For photo and text markers we need to get the marker definition to determine the size.
				string markerId = string.Format("{0}_{1}", marker.Id, tourViewId);
				MarkerDefinition markerDefinition = (MarkerDefinition)(tourBuilder.MarkerDefinitions[markerId]);
				if (markerDefinition == null)
				{
					// This marker is not on the map so don't add it to the table. This can happen
					// for a photo marker that does not fit inside its gallery.
					return;
				}
				shapeSize.Width = markerDefinition.Bounds.Width;
				shapeSize.Height = markerDefinition.Bounds.Height;

				// Create a string containing the image data in Base64 format for use in the Data URI scheme.
				EmitImageDataString(tourViewId, markerDefinition);
			}

			if (marker.MarkerType == MarkerType.Symbol || marker.MarkerType == MarkerType.SymbolAndShape)
			{
				EmitImageDataString(marker);
			}

			if (marker.MarkerType == MarkerType.Symbol || marker.MarkerType == MarkerType.SymbolAndShape)
			{
				if (normalSymbolId != 0)
				{
					normalSymbolSize = Account.GetCachedSymbol(normalSymbolId).Size;
				}
				if (selectedSymbolId != 0)
				{
					selectedSymbolSize = Account.GetCachedSymbol(selectedSymbolId).Size;
				}
				symbolLocationX = marker.SymbolLocation.X;
				symbolLocationY = marker.SymbolLocation.Y;
			}
		}

		string markerClickActionTarget = string.Empty;
		if (tourView.MarkerClickAction == MarkerAction.GotoPage)
		{
			TourPage tourPage = tour.GetTourPage(int.Parse(tourView.MarkerClickActionTarget));
			markerClickActionTarget = string.Format("page{0}.htm", tourPage.PageNumber);
		}
		else if (tourView.MarkerClickAction != MarkerAction.None)
		{
			markerClickActionTarget = tourView.MarkerClickActionTarget;
		}
		int markerClickActionTargetIndex = AddToStringTable(markerClickActionTarget);
		int markerRolloverActionTargetIndex = AddToStringTable(tourView.MarkerRolloverActionTarget);
		int markerRolloutActionTargetIndex = AddToStringTable(tourView.MarkerRolloutActionTarget);

		string toolTip = tourView.ToolTip;
		if (Marker.ToolTipAllowed(tourView) && toolTip.Length == 0 && tourView.HasNoContent)
		{
			toolTip = tourView.Title;
		}

		int tooltipIndex = AddToStringTable(toolTip);

		string data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31}",
			tourViewId,
			markerTypeId,
			markerStyleId,
			normalSymbolId,
			selectedSymbolId,
			tourView.MarkerPctX,
			tourView.MarkerPctY,
			shapeTypeId,
			shapeSize.Width,
			shapeSize.Height,
			normalSymbolSize.Width,
			normalSymbolSize.Height,
			selectedSymbolSize.Width,
			selectedSymbolSize.Height,
			symbolLocationX,
			symbolLocationY,
			baseMarker.AnchorDelta.X,
			baseMarker.AnchorDelta.Y,
			tourView.MarkerRotation,
			tourView.MarkerZoomThreshold,
			MarkerInstanceFlagBits(baseMarker.MarkerInstance),
			tooltipIndex,
			(int)tourView.MarkerClickAction,
			markerClickActionTargetIndex,
			(int)tourView.MarkerRolloverAction,
			markerRolloverActionTargetIndex,
			(int)tourView.MarkerRolloutAction,
			markerRolloutActionTargetIndex,
			tourView.TouchPerformsClickAction ? 1 : 0,
			(int)tourView.ShowContentEvent,
			shapeCoordsIndex,
            isRoute ? 0 : marker.Id);

		markerInstanceTable.Add(data);

		// Add this marker's style to the list of marker styles that are in use.
		if (markerStyleId != 0 && !markerStylesInUse.Contains(markerStyleId))
			markerStylesInUse.Add(markerStyleId);
	}

	private void EmitImageDataString(Marker marker)
	{
		Symbol symbol;
		
		if (marker.NormalSymbolId != 0)
		{
			symbol = Account.GetCachedSymbol(marker.NormalSymbolId);
			imageDataTable.Add(string.Format("{{id:'S{0}N',data:'{1}'}}", symbol.Id, Convert.ToBase64String(symbol.Bytes)));
		}
		
		if (marker.SelectedSymbolId != 0)
		{
			symbol = Account.GetCachedSymbol(marker.SelectedSymbolId);
			imageDataTable.Add(string.Format("{{id:'S{0}S',data:'{1}'}}", symbol.Id, Convert.ToBase64String(symbol.Bytes)));
		}
	}

	private void EmitImageDataString(int tourViewId, MarkerDefinition markerDefinition)
	{
		Bitmap bitmap = markerDefinition.Base.NormalAppearance.SymbolBitmap;
		Byte[] bytes = Utility.ImageToByteArray(bitmap, ImageFormat.Png);
		string dataN = Convert.ToBase64String(bytes);
		
		bitmap = markerDefinition.Base.SelectedAppearance.SymbolBitmap;
		bytes = Utility.ImageToByteArray(bitmap, ImageFormat.Png);
		string dataS = Convert.ToBase64String(bytes);

		imageDataTable.Add(string.Format("{{id:'H{0}N',data:'{1}'}}", tourViewId, dataN));
		imageDataTable.Add(string.Format("{{id:'H{0}S',data:'{1}'}}", tourViewId, dataS));
	}

	private string FontSize(string pixels)
	{
        return pixels;

        // This method can be changed to convert pixels to ems using the code below.
		//return (double.Parse(pixels) / 16).ToString("0.000");
	}

	private int MarkerInstanceFlagBits(BaseMarkerInstance markerInstance)
	{
		MarkerInstanceFlags bits = 0;

		if (markerInstance.IsBound)
			bits |= MarkerInstanceFlags.IsBound;
		if (markerInstance.IsDisabled)
			bits |= MarkerInstanceFlags.IsDisabled;
		if (markerInstance.IsHidden)
			bits |= MarkerInstanceFlags.IsHidden;
		if (markerInstance.IsStatic)
			bits |= MarkerInstanceFlags.IsStatic;
		if (markerInstance.IsShapeOnly)
			bits |= MarkerInstanceFlags.IsShapeOnly;
		if (markerInstance.IsLocked)
			bits |= MarkerInstanceFlags.IsLocked;
		if (markerInstance.MarkerZooms)
			bits |= MarkerInstanceFlags.MarkerZooms;
		if (markerInstance.IsNotAnchored)
			bits |= MarkerInstanceFlags.IsNotAnchored;
		if (markerInstance.DoesNotShowContent)
			bits |= MarkerInstanceFlags.DoesNotShowContent;
		if (markerInstance.IsRoute)
		{
			bits |= MarkerInstanceFlags.IsRoute;
			bits |= MarkerInstanceFlags.MarkerZooms;
		}

		return (int)bits;
	}
	
	private void AddToMarkerStyleTable(int markerStyleId)
	{
		MarkerStyle markerStyle = Account.GetCachedMarkerStyle(markerStyleId);

		string data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
			markerStyleId,
			markerStyle.LineWidth,
			markerStyle.NormalFillColor,
			markerStyle.NormalLineColor,
			markerStyle.SelectedFillColor,
			markerStyle.SelectedLineColor,
			markerStyle.NormalFillColorOpacity,
			markerStyle.NormalLineColorOpacity,
			markerStyle.SelectedFillColorOpacity,
			markerStyle.SelectedLineColorOpacity,
			AddToStringTable(markerStyle.NormalShapeEffects),
			AddToStringTable(markerStyle.SelectedShapeEffects)
		);

		markerStyleTable.Add(data);
	}

	private void AddToPageTable(int pageIdIndex, int pageNumber)
	{
		string data = string.Format("{0},{1}", pageIdIndex, pageNumber);
		pageTable.Add(data);
	}

	private void AddToSlideTable(
		TourView tourView,
		int pageNumber,
		int slideIdIndex,
		int slideTitleIndex,
		int slideTextIndex,
		int embedTextIndex,
		int imageFileNameIndex,
		int mediaWidth,
		int mediaHeight,
		int messengerFunctionIndex,
		int dirPreviewImageUrlIndex,
		int dirPreviewTextIndex)
	{
		if (tourView.TourPage.IsGallery && !tourView.MarkerHasBeenPlacedOnMap)
		{
			// Don't add an entry for gallery markers that don't fit in the gallery.
			return;
		}

        string data;

        if (tour.V3CompatibilityEnabled)
        {
            data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
			    tourView.Id,
			    pageNumber,
			    slideIdIndex,
			    slideTitleIndex,
			    slideTextIndex,
			    imageFileNameIndex,
			    mediaWidth,
			    mediaHeight,
			    (int)tourView.MediaType,
			    embedTextIndex,
			    tourView.SlideWidthOverride,
			    tourView.SlideHeightOverride,
			    tourView.UsesLiveData ? 1 : 0,
			    messengerFunctionIndex,
			    dirPreviewImageUrlIndex,
			    dirPreviewTextIndex);

        }
        else
        {
		    // V4 passes an additional parameter to indcate if the view is on the map.
            data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
			    tourView.Id,
			    pageNumber,
			    slideIdIndex,
			    slideTitleIndex,
			    slideTextIndex,
			    imageFileNameIndex,
			    mediaWidth,
			    mediaHeight,
			    (int)tourView.MediaType,
			    embedTextIndex,
			    tourView.SlideWidthOverride,
			    tourView.SlideHeightOverride,
			    tourView.UsesLiveData ? 1 : 0,
			    messengerFunctionIndex,
			    dirPreviewImageUrlIndex,
    			dirPreviewTextIndex,
                tourView.MarkerHasBeenPlacedOnMap ? 1 : 0);
        }

		SlideTableEntry slideTableEntry = new SlideTableEntry(tourView.TitleOrSlideId, data);
		slideTable.Add(slideTableEntry);
	}

	private int AddToStringTable(string text)
	{
		int index = stringTable.IndexOf(text);
		if (index == -1)
			index = stringTable.Add(text);
		return index;
	}

	private void CreateMarkerInstanceTable()
	{
		markerInstanceTable = new ArrayList();
		markerStylesInUse = new ArrayList();
		imageDataTable = new ArrayList();

		foreach (BaseMarker baseMarker in tourBuilder.MapMarkers(currentTourPage.Id))
		{
			AddToMarkerInstanceTable(baseMarker);
		}
		
		StringBuilder symbolsData = new StringBuilder();
		bool first = true;
		foreach (string data in imageDataTable)
		{
			if (first)
				first = false;
			else
				symbolsData.Append(",\n");
			symbolsData.Append(data);
		}

		// Write the symbols data to a file.
		string fileLocation;
		string fileContent;
		if (tour.V3CompatibilityEnabled)
        {
			// Create the V3 version of the symbols file.
			fileContent = string.Format("maClient.Html5.prototype.markerImages=function(){{return [\n{0}\n];}};\n", symbolsData.ToString());
			fileLocation = FileManager.PreviewFolderLocationAbsolute(tour.Id, string.Format(TourBuilder.PatternForSymbolsFileV3, currentTourPage.PageNumber));
			FileManager.CreateTextFile(fileLocation, fileContent);
        }

		// Create the symbols file. Even when V3 compatibility is enabled, this file is needed by the Map Editor.
		fileContent = string.Format("export let page{1}Symbols=[\n{0}\n];\n", symbolsData.ToString(), currentTourPage.PageNumber);
		fileLocation = FileManager.PreviewFolderLocationAbsolute(tour.Id, string.Format(TourBuilder.PatternForSymbolsFile, currentTourPage.PageNumber, tour.BuildId));
		FileManager.CreateTextFile(fileLocation, fileContent);

		WriteMarkerInstanceTable();
	}

	private void CreateMarkerStyleTable()
	{
		markerStyleTable = new ArrayList();

		foreach (int markerStyleId in markerStylesInUse)
		{
			AddToMarkerStyleTable(markerStyleId);
		}
		
		WriteMarkerStyleTable();
	}

	private void CreatePageAndSlideTables()
	{
		pageTable = new ArrayList();
		slideTable = new ArrayList();

		int embedTextIndex;
		int imageFileNameIndex;
		int mediaHeight;
		int mediaWidth;
		int messengerFunctionIndex;
		int slideIdIndex;
		int slideTitleIndex;
		int slideTextIndex;
		int dirPreviewImageUrlIndex;
		int dirPreviewTextIndex;

		foreach (TourPage tourPage in tour.TourPages)
		{
			// Write this page to the page table.
			int pageIdIndex = AddToStringTable(tourPage.PageId);
			AddToPageTable(pageIdIndex, tourPage.PageNumber);

            // V4 only ever emits views belonging to the current page. V3 emits views for all
            // pages when the directory is enabled and the "Show All Maps" option is enabled.
			if (tourPage == currentTourPage || (allPages && tour.V3CompatibilityEnabled))
			{
				foreach (TourView tourView in tourPage.TourViews)
				{
					if (tourPage.ActiveSlideLayout.HasImageArea)
					{
						Size containerSize = tourView.GetImageContainerSize();
						if (tourView.MediaType == SlideMediaType.Photo)
						{
							Size imageSize = tourView.HasImage ? Utility.ScaledImageSize(tourView.Image.Size, containerSize) : Size.Empty;
							string fileName = tourView.HasImage ? tourView.Image.FileNameInternal : string.Empty;
							imageFileNameIndex = AddToStringTable(fileName);
							mediaWidth = imageSize.Width;
							mediaHeight = imageSize.Height;
						}
						else
						{
							imageFileNameIndex = -1;
							mediaWidth = containerSize.Width;
							mediaHeight = containerSize.Height;
							
							if (tourView.MediaType == SlideMediaType.Embed)
							{
								// The dimensions of embedded media are constrained by the layout area sizes.
								mediaWidth = Math.Min(tourView.EmbedWidth, mediaWidth);
								mediaHeight = Math.Min(tourView.EmbedHeight, mediaHeight);
							}
						}
					}
					else
					{
						mediaWidth = 0;
						mediaHeight = 0;
						imageFileNameIndex = -1;
					}

					slideIdIndex = AddToStringTable(tourView.SlideId);
					
					// Replace ampersands in title text to allow things like R & D.
					string title = tourView.TitleOrSlideId;
					title = title.Replace("&", "&amp;");
					
					// Titles that start with an underscore will be excluded from the directory.
					if (tourView.ExcludeFromDirectory && !title.StartsWith("_"))
						title = "_" + title;

					slideTitleIndex = AddToStringTable(title);

					embedTextIndex = -1;
					slideTextIndex = -1;
					messengerFunctionIndex = -1;
					dirPreviewImageUrlIndex = -1;
					dirPreviewTextIndex = -1;

					string text = PreProcessSlideText(tourView);
					slideTextIndex = AddToStringTable(text);

					if (tourView.MediaType == SlideMediaType.Embed)
						embedTextIndex = AddToStringTable(tourView.EmbedText);
						
					if (tourView.UsesLiveData)
						messengerFunctionIndex = AddToStringTable(tourView.MessengerFunction);

                    // V4 always emits directory information for use as a menu even if the directory is not turned on.
                    if (tour.HasDirectory || tour.V4)
					{
						// Provide the URL for the preview image. For data sheets we specify "0" to mean don't show
						// it. Technically we can show a data sheet preview image, but we don't so that the directory
						// preview behaves consistently for all page-level items i.e. maps, data sheets, and galleries.
						string dirPreviewImageUrl = tourPage.IsDataSheet ? "0" : tourView.DirPreviewImageUrl;
						dirPreviewImageUrlIndex = AddToStringTable(dirPreviewImageUrl);
						dirPreviewTextIndex = AddToStringTable(tourView.DirPreviewText);
					}

					AddToSlideTable(
						tourView,
						tourPage.PageNumber,
						slideIdIndex,
						slideTitleIndex,
						slideTextIndex,
						embedTextIndex,
						imageFileNameIndex,
						mediaWidth,
						mediaHeight,
						messengerFunctionIndex,
						dirPreviewImageUrlIndex,
						dirPreviewTextIndex);
				}
			}
		}

		WriteStringTable();
		WritePageTable();
		WriteSlideTable();
		WriteCategoryTable();
		WriteRoutesXml(currentTourPage);
	}

	public void CreateTourXmlForPage(int pageId)
	{
		currentTourPage = tour.GetTourPage(pageId);
		Debug.Assert(currentTourPage != null, "No page found for Id " + pageId);

        allPages = tour.HasDirectory && tour.Directory.ShowAllPages;

		xmlWriter.WriteStartElement("tour");

		WriteTourAttributes();
        WriteLayoutAttributes();
		WriteFontAttributes(tour.FontSchemeId);
		WriteColorAttributes(tour.ColorScheme);

		if (tour.V3CompatibilityEnabled)
            WriteTooltipAttributes(currentTourPage);
		
        WriteBannerXml();
		WriteFooterXml();

		stringTable = new ArrayList();

		WriteDirectoryRows();
		WriteTourPagesXml();

		if (!currentTourPage.IsDataSheet)
		{
			CreateMarkerInstanceTable();
			CreateMarkerStyleTable();
		}

		CreatePageAndSlideTables();

		xmlWriter.WriteEndElement(); // tour
	}

	private string PreProcessSlideText(TourView tourView)
	{
		// Circumvent a "behavior" of the RadEditor that wrapped the content in P tags to apply styles like
        // centering.  All the browsers except IE insert a newline above the tag in the generated tour, but
        // not in the editor control. Replacing the P with a DIV eliminates an unexpected newline. While this
        // does not occur with Tiny MCE, this filtering occurs in case the <p> tags remain in older content.
		string text = tourView.DescriptionHtml;
		if (text.StartsWith("<p") && text.EndsWith("</p>"))
		{
			text = "<div" + text.Substring(2, text.Length - 4) + "div>";
		}
		
		// Expand macros inline.
		slideMacroProcessor.ExpandMacros(tourView, ref text);
		
		return text;
	}

	private void WriteBannerXml()
	{
		if (!tour.HasBanner)
			return;

		xmlWriter.WriteAttributeString("bannerImg", tour.Banner.Image.FileNameInternal);
		string url = tour.Banner.Url;
		if (url.Length > 0)
		{
			// Make sure the URL starts with http.
			if (url.Length >= 4 && url.ToLower().Substring(0, 4) != "http")
				url = "http://" + url;

			xmlWriter.WriteAttributeString("bannerUrl", url);
			xmlWriter.WriteAttributeString("bannerUrlOpensWindow", tour.Banner.UrlOpensWindow.ToString());
			string bannerTitle = tour.Banner.UrlTitle;
			if (bannerTitle.Length > 0)
				xmlWriter.WriteAttributeString("bannerUrlTitle", bannerTitle);
		}
	}

	private void WriteColorAttributes(ColorScheme scheme)
	{
		xmlWriter.WriteAttributeString("colorTourBackground", scheme.LayoutAreaBackgroundColor);

		xmlWriter.WriteAttributeString("colorTitleText", scheme.TitleTextColor);
		xmlWriter.WriteAttributeString("colorTitleBackground", scheme.TitleBackgroundColor);

		xmlWriter.WriteAttributeString("colorHeaderStripeBackground", scheme.StripeColor);
		xmlWriter.WriteAttributeString("colorHeaderStripeTopBorder", scheme.StripeBorderColor);
		xmlWriter.WriteAttributeString("colorHeaderStripeBottomBorder", scheme.StripeBorderColor);

		xmlWriter.WriteAttributeString("colorFooterStripeBackground", scheme.StripeColor);
		xmlWriter.WriteAttributeString("colorFooterStripeTopBorder", scheme.StripeBorderColor);
		xmlWriter.WriteAttributeString("colorFooterStripeBottomBorder", scheme.StripeBorderColor);
		
		xmlWriter.WriteAttributeString("colorFooterLinkText", scheme.FooterLinkTextColor);

		xmlWriter.WriteAttributeString("colorSlideTitleText", scheme.SlideTitleTextColor);
		xmlWriter.WriteAttributeString("colorSlideTitleBackground", scheme.SlideBackgroundColor);

		xmlWriter.WriteAttributeString("colorSlideText", scheme.SlideTextColor);
		xmlWriter.WriteAttributeString("colorSlideBackground", scheme.SlideBackgroundColor);

        // Override white color scheme values for menu items since the V4 nav
        // panel has a white background which would make them appear invisible. 
        string menuNormalTextColor = scheme.MenuNormalTextColor;
        string menuSelectedTextColor = scheme.MenuSelectedTextColor;
        string menuHoverTextColor = scheme.MenuHoverTextColor;
        if (tour.V4)
        {
            if (menuNormalTextColor.ToLower() == "#ffffff")
                menuNormalTextColor = "#333333";
            if (menuSelectedTextColor.ToLower() == "#ffffff")
                menuSelectedTextColor = "#000000";
            if (menuHoverTextColor.ToLower() == "#ffffff")
                menuHoverTextColor = "#777777";
        }
        xmlWriter.WriteAttributeString("colorMenuItemNormalText", menuNormalTextColor);
		xmlWriter.WriteAttributeString("colorMenuItemSelectedText", menuSelectedTextColor);
		xmlWriter.WriteAttributeString("colorMenuItemHoverText", menuHoverTextColor);
        
        xmlWriter.WriteAttributeString("colorMenuBackground", scheme.MenuBackgroundColor);
		xmlWriter.WriteAttributeString("colorMenuItemLine", scheme.MenuLineColor);
		xmlWriter.WriteAttributeString("colorMenuItemSelectedBackground", scheme.MenuSelectedBackgroundColor);
		xmlWriter.WriteAttributeString("colorMenuItemHoverBackground", scheme.MenuHoverBackgroundColor);
		xmlWriter.WriteAttributeString("colorMenuItemNormalBackground", scheme.MenuNormalBackgroundColor);
	}

	private void WriteDirectoryRowEntry(ArrayList tourViews, int depth)
	{
		foreach (TourView tourView in tourViews)
		{
			if (!tourView.MarkerHasBeenPlacedOnMap)
			{
				// Don't add hotspots that are not on the map or gallery markers that don't fit in the gallery.
				continue;
			}
			
			directoryTable.Add(new DirectoryRow(depth, tourView.Id, tourView.TourPage.PageNumber));
		}
	}

	private void WriteDirectoryRowLevel(string title, int depth, int pageNumber)
	{
		directoryTable.Add(new DirectoryRow(depth, AddToStringTable(title), pageNumber));
	}

	private void WriteDirectoryRowPage(TourPage page, int depth)
	{
        string title = page.Title;
        if (title.Length == 0)
            title = page.Name;
        WriteDirectoryRowLevel(title, depth, page.PageNumber);
	}

	private void WriteDirectoryRows()
	{
		TourDirectory directory = tour.Directory;

        // V4 always emits this directory information for the nav panel even if the directory is not turned on.
        xmlWriter.WriteAttributeString("dirLocation", ((int)directory.Location).ToString());
        xmlWriter.WriteAttributeString("dirLocationX", directory.LocationX.ToString());
        xmlWriter.WriteAttributeString("dirLocationY", directory.LocationY.ToString());
		xmlWriter.WriteAttributeString("dirContentWidth", directory.ContentWidth.ToString());
		xmlWriter.WriteAttributeString("dirMaxHeight", directory.MaxHeight.ToString());
		xmlWriter.WriteAttributeString("dirPreviewWidth", directory.PreviewWidth.ToString());
		xmlWriter.WriteAttributeString("dirPreviewImageWidth", directory.PreviewImageWidth.ToString());

        if (!tour.HasDirectory && tour.V3CompatibilityEnabled)
            return;

		directoryTable = new ArrayList();

		if (directory.GroupByCategory)
			WriteDirectoryRowsByCategory(directory.GroupByCategoryThenPage);
		else if (directory.GroupByPage)
			WriteDirectoryRowsByPage(directory.GroupByPageThenCategory);

		StringBuilder sb = new StringBuilder();

        // Emit the depth of the view entries as the table's first value. The depth is 2 when grouping
        // by page or category. It's 3 when grouping by page then catetory, or by category then page.
        sb.Append((int)directory.EntryDepth);
		
        for (int index = 0; index < directoryTable.Count; index++)
		{
			sb.Append(",");
			DirectoryRow row = (DirectoryRow)directoryTable[index];

            if (tour.V3CompatibilityEnabled)
			    sb.Append(string.Format("{0},{1}", (int)row.Depth, row.Id));
            else
			    sb.Append(string.Format("{0},{1},{2}", (int)row.Depth, row.PageNumber, row.Id));
		}

		bool showGroupSort = (directory.GroupByCategory || directory.GroupByPage) && !MapsAliveState.Account.IsPersonalPlan;
		
		xmlWriter.WriteAttributeString("dirTable", sb.ToString());
		xmlWriter.WriteAttributeString("dirAlignContentRight", directory.AlignContentRight.ToString());
		xmlWriter.WriteAttributeString("dirAutoCollapse", directory.AutoCollapse.ToString());
		xmlWriter.WriteAttributeString("dirBackgroundColor", directory.BackgroundColor);
		xmlWriter.WriteAttributeString("dirEntryCountColor", directory.EntryCountColor);
		xmlWriter.WriteAttributeString("dirEntryTextColor", directory.EntryTextColor);
		xmlWriter.WriteAttributeString("dirEntryTextHoverColor", directory.EntryTextHoverColor);
		xmlWriter.WriteAttributeString("dirLevel1TextColor", directory.Level1TextColor);
		xmlWriter.WriteAttributeString("dirLevel2TextColor", directory.Level2TextColor);
        xmlWriter.WriteAttributeString("dirPreviewImageBorderColor", directory.PreviewImageBorderColor);
		xmlWriter.WriteAttributeString("dirPreviewOnRight", directory.PreviewOnRight.ToString());
		xmlWriter.WriteAttributeString("dirSearchResultBackgroundColor", directory.SearchResultBackgroundColor);
		xmlWriter.WriteAttributeString("dirSearchResultTextColor", directory.SearchResultTextColor);
		xmlWriter.WriteAttributeString("dirShowAllPages", directory.ShowAllPages.ToString());
		xmlWriter.WriteAttributeString("dirShowGroupSort", showGroupSort.ToString());
		xmlWriter.WriteAttributeString("dirShowImagePreview", directory.ShowImagePreview.ToString());
		xmlWriter.WriteAttributeString("dirShowSearch", directory.ShowSearch.ToString());
		xmlWriter.WriteAttributeString("dirShowTextPreview", directory.ShowTextPreview.ToString());
		xmlWriter.WriteAttributeString("dirStaysOpen", directory.StaysOpen.ToString());
		xmlWriter.WriteAttributeString("dirOpenExpanded", directory.OpenExpanded.ToString());
		xmlWriter.WriteAttributeString("dirTextAlphaSortTooltip", directory.TextAlphaSortTooltip.ToString());
		xmlWriter.WriteAttributeString("dirTextClearButtonLabel", directory.TextClearButtonLabel.ToString());
		xmlWriter.WriteAttributeString("dirTextGroupSortTooltip", directory.TextGroupSortTooltip.ToString());
		xmlWriter.WriteAttributeString("dirTextNoSearchMessage", directory.TextNoSearchMessage.ToString());
		xmlWriter.WriteAttributeString("dirTextSearchLabel", directory.TextSearchLabel.ToString());
		xmlWriter.WriteAttributeString("dirTextTitle", directory.TextTitle.ToString());
		xmlWriter.WriteAttributeString("dirTextSearchResultsMessage", directory.TextSearchResultsMessage.ToString());
		xmlWriter.WriteAttributeString("dirTitleBarWidth", directory.TitleBarWidth.ToString());

		// These colors are kept synchronized with the ColorScheme if the user has chosen that option.
		xmlWriter.WriteAttributeString("dirTitleTextColor", directory.TitleTextColor.ToString());
		xmlWriter.WriteAttributeString("dirTitleBarColor", directory.TitleBarColor.ToString());
		xmlWriter.WriteAttributeString("dirBorderColor", directory.BorderColor);
		xmlWriter.WriteAttributeString("dirPreviewBorderColor", directory.PreviewBorderColor);
		xmlWriter.WriteAttributeString("dirPreviewTextColor", directory.PreviewTextColor);
		xmlWriter.WriteAttributeString("dirPreviewBackgroundColor", directory.PreviewBackgroundColor);
		xmlWriter.WriteAttributeString("dirStatusTextColor", directory.StatusTextColor);
		xmlWriter.WriteAttributeString("dirStatusBackgroundColor", directory.StatusBackgroundColor);
	}

	private void WriteDirectoryRowsByCategory(bool thenByPage)
	{
		foreach (Category category in tour.CategoryManager.CategoryTable)
		{
			int tourPageId = allPages || tour.V4 ? -1 : currentTourPage.Id;

			// Get all the tour views that are in the current category.
			ArrayList tourViews = tour.CategoryManager.GetTourViews(category.Id, tourPageId);
			if (tourViews.Count == 0)
				continue;

			WriteDirectoryRowLevel(category.Title, 1, currentTourPage.PageNumber);

            // V4 always emits data for all pages. V3 only emits it if the Show All Maps option is enabled.
            bool writeRowsForThisPage = allPages || tour.V4;

            if (thenByPage && writeRowsForThisPage)
			{
				foreach (TourPage tourPage in tour.TourPages)
				{
					if (tourPage.ExcludeFromNavigation)
						continue;

                    // V4 always emits directory table data for all pages.
                    bool writeRowForThisPage = tourPage == currentTourPage || tour.V4;

                    if (writeRowForThisPage || allPages)
					{
						if (!tourPage.IsDataSheet)
                            WriteDirectoryRowPage(tourPage, 2);
	
						// Get only the tour views that are in the current category on this page.
						tourViews = tour.CategoryManager.GetTourViews(category.Id, tourPage.Id);
						if (tourViews.Count == 0)
							continue;
						WriteDirectoryRowEntry(tourViews, tourPage.IsDataSheet ? -2 : 3);
					}
				}
			}
			else
			{
				WriteDirectoryRowEntry(tourViews, 2);
			}
		}
	}

	private void WriteDirectoryRowsByPage(bool thenByCatgegory)
	{
		foreach (TourPage tourPage in tour.TourPages)
		{
			if (tourPage.ExcludeFromNavigation)
				continue;

            // V4 always emits data for all pages. V3 only emits it if the Show All Maps option is enabled.
            bool writeRowsForThisPage = tourPage == currentTourPage || tour.V4;

            if (writeRowsForThisPage || allPages)
			{
				// Get all the tour views that are on the current page.
				ArrayList tourViews = tour.CategoryManager.GetTourViews(-1, tourPage.Id);
				if (tourViews.Count == 0)
					continue;

				if (!tourPage.IsDataSheet)
                    WriteDirectoryRowPage(tourPage, 1);

				if (thenByCatgegory && !tourPage.IsDataSheet)
				{
					foreach (Category category in tour.CategoryManager.CategoryTable)
					{
						WriteDirectoryRowLevel(category.Title, 2, tourPage.PageNumber);

						// Get only the tour views that are on the current page in this category.
						tourViews = tour.CategoryManager.GetTourViews(category.Id, tourPage.Id);
						if (tourViews.Count == 0)
							continue;
						WriteDirectoryRowEntry(tourViews, 3);
					}
				}
				else
				{
					// Eliminate any duplicate tour views (ones that belong to more than one category).
					ArrayList uniqueTourViews = new ArrayList();
					foreach (TourView tourView in tourViews)
					{
						if (uniqueTourViews.Contains(tourView))
							continue;
						uniqueTourViews.Add(tourView);
					}

					WriteDirectoryRowEntry(uniqueTourViews, tourPage.IsDataSheet ? -1 : 2);
				}
			}
		}
	}

	private void WriteFontAttributes(int fontSchemeId)
	{
		TourFontScheme scheme = new TourFontScheme(fontSchemeId, false);
		
		xmlWriter.WriteAttributeString("fontFamilyHeading", scheme.FontFamilyHeading);
		xmlWriter.WriteAttributeString("fontSizeHeading", FontSize(scheme.FontSizeHeading));
		xmlWriter.WriteAttributeString("fontStyleHeading", scheme.FontStyleHeading);
		xmlWriter.WriteAttributeString("fontWeightHeading", scheme.FontWeightHeading);
		
		xmlWriter.WriteAttributeString("fontFamilyDescription", scheme.FontFamilyDescription);
		xmlWriter.WriteAttributeString("fontSizeDescription", FontSize(scheme.FontSizeDescription));
		xmlWriter.WriteAttributeString("fontStyleDescription", scheme.FontStyleDescription);
		xmlWriter.WriteAttributeString("fontWeightDescription", scheme.FontWeightDescription);
		
		xmlWriter.WriteAttributeString("fontFamilyTitle", scheme.FontFamilyTitle);
		xmlWriter.WriteAttributeString("fontSizeTitle", FontSize(scheme.FontSizeTitle));
		xmlWriter.WriteAttributeString("fontStyleTitle", scheme.FontStyleTitle);
		xmlWriter.WriteAttributeString("fontWeightTitle", scheme.FontWeightTitle);
		
		xmlWriter.WriteAttributeString("fontSizeFooter", FontSize(scheme.FontSizeFooter));
		xmlWriter.WriteAttributeString("fontFamilyFooter", scheme.FontFamilyFooter);
		xmlWriter.WriteAttributeString("fontStyleFooter", scheme.FontStyleFooter);
		xmlWriter.WriteAttributeString("fontWeightFooter", scheme.FontWeightFooter);
		
		xmlWriter.WriteAttributeString("fontFamilyMenuItem", scheme.FontFamilyMenuItem);
		xmlWriter.WriteAttributeString("fontSizeMenuItem", FontSize(scheme.FontSizeMenuItem));
		xmlWriter.WriteAttributeString("fontStyleMenuItem", scheme.FontStyleMenuItem);
		xmlWriter.WriteAttributeString("fontWeightMenuItem", scheme.FontWeightMenuItem);
		
		xmlWriter.WriteAttributeString("fontFamilyMenuSlideItem", scheme.FontFamilyMenuSlideItem);
		xmlWriter.WriteAttributeString("fontSizeMenuSlideItem", FontSize(scheme.FontSizeMenuSlideItem));
		xmlWriter.WriteAttributeString("fontStyleMenuSlideItem", scheme.FontStyleMenuSlideItem);
		xmlWriter.WriteAttributeString("fontWeightMenuSlideItem", scheme.FontWeightMenuSlideItem);
	}

	private void WriteFooterXml()
	{
		if (!tour.HasCustomFooter)
			return;

		string footerPrefix;
		string footerLink;
		string footerUrl;
		string footerSuffix;
		tour.GetCustomFooterComponents(out footerPrefix, out footerLink, out footerUrl, out footerSuffix);
		xmlWriter.WriteAttributeString("tourFooterText1", footerPrefix);
		xmlWriter.WriteAttributeString("tourFooterLinkText", footerLink);
		xmlWriter.WriteAttributeString("tourFooterText2", footerSuffix);
		xmlWriter.WriteAttributeString("tourFooterLinkUrl", footerUrl);
	}

	private void WriteLayoutAttributes()
	{
		string browserTitle = tour.BrowserTitle.Trim().Length == 0 ? tour.Name : tour.BrowserTitle;
		xmlWriter.WriteAttributeString("browserTitle", browserTitle);
		xmlWriter.WriteAttributeString("tourName", tour.Name);
		
		xmlWriter.WriteAttributeString("navigationId", tour.MenuLocationIdEffective.ToString());
 		xmlWriter.WriteAttributeString("menuStyleId", tour.MenuStyleId.ToString());

		xmlWriter.WriteAttributeString("hasBackgroundColor", TourLayout.HasBackgroundColor ? "True" : "False");
		xmlWriter.WriteAttributeString("hasBanner", tour.HasBanner.ToString());
		xmlWriter.WriteAttributeString("hasDirectory", tour.HasDirectory.ToString());
        xmlWriter.WriteAttributeString("hasDataSheet", tour.HasDataSheet.ToString());

        bool hasTitleBar = false;
        if (tour.V3CompatibilityEnabled)
        {
            hasTitleBar = tour.HasTitle;
        }
        else
        {
            if (tour.HasTitle || tour.Directory.Location == TourDirectoryLocation.TitleBar)
            {
                hasTitleBar = true;
            }
            else if (tour.HasDataSheet || tour.HasGallery)
            {
                // When a V4 tour has a data sheet or gallery, the menu cannot be displayed in the map. It can go into
                // the banner or above the tour, but if neither of those options apply, then it must go in the title bar.
                hasTitleBar = tour.Directory.Location == TourDirectoryLocation.MapLeft;
            }
        }
        xmlWriter.WriteAttributeString("hasTitle", hasTitleBar.ToString());
		
        xmlWriter.WriteAttributeString("hasHeaderStripe", tour.HasHeaderStripe ? "True" : "False");
		xmlWriter.WriteAttributeString("hasFooterStripe", tour.HasFooterStripe ? "True" : "False");
		xmlWriter.WriteAttributeString("bannerHeight", tour.HasBanner ? tour.Banner.OptimalHeight().ToString() : "0");
		xmlWriter.WriteAttributeString("bannerPaddingLeft", TourLayout.BannerPaddingLeft.ToString());
		xmlWriter.WriteAttributeString("bannerPaddingTop", TourLayout.BannerPaddingTop.ToString());
        xmlWriter.WriteAttributeString("canAppearUnbranded", tour.CanAppearUnbranded ? "True" : "False");

        bool hasLeftMenu = tour.MenuLocationIdEffective == (int)Tour.MenuLocation.Left;
		xmlWriter.WriteAttributeString("leftNavWidth", hasLeftMenu ? tour.MenuWidth.ToString() : "0");
		xmlWriter.WriteAttributeString("navHeight", tour.MenuHeight.ToString());
        xmlWriter.WriteAttributeString("menuScrolls", tour.MenuScrolls ? "scroll" : "hidden");

		xmlWriter.WriteAttributeString("bodyMargin", tour.BodyMargin.ToString());
		xmlWriter.WriteAttributeString("bodyBackgroundColor", tour.BodyBackgroundColor);
	    xmlWriter.WriteAttributeString("centeredInBrowser", !tour.LeftAlignedInBrowser ? "True" : "False");

		xmlWriter.WriteAttributeString("headerStripeHeight", TourLayout.HeaderStripeHeight.ToString());
		xmlWriter.WriteAttributeString("headerStripeBorderHeight", TourLayout.HeaderStripeBorderHeight.ToString());
		xmlWriter.WriteAttributeString("pageTitleHeight", TourLayout.TitleHeight.ToString());
		xmlWriter.WriteAttributeString("titleOffsetLeft", TourLayout.TitleOffsetLeft.ToString());
		xmlWriter.WriteAttributeString("titleOffsetTop", TourLayout.TitleOffsetTop.ToString());
		xmlWriter.WriteAttributeString("titleOffsetBottom", TourLayout.TitleOffsetBottom.ToString());
		
        xmlWriter.WriteAttributeString("footerStripeHeight", TourLayout.FooterStripeHeight.ToString());
        xmlWriter.WriteAttributeString("footerStripeBorderHeight", TourLayout.FooterStripeBorderHeight.ToString());
        xmlWriter.WriteAttributeString("footerHeight", TourLayout.FooterHeight.ToString());

        xmlWriter.WriteAttributeString("showMapsAliveLink", tour.HideCreatedWithMapsAlive ? "0" : "1");
        xmlWriter.WriteAttributeString("showCustomFooter", tour.HasCustomFooter ? "1" : "0");

		Size tourSize = tour.TourSize;
		xmlWriter.WriteAttributeString("tourWidth", tourSize.Width.ToString());
		xmlWriter.WriteAttributeString("tourHeight", tourSize.Height.ToString());
	
		xmlWriter.WriteAttributeString("layoutAreaWidth", tour.LayoutAreaSize.Width.ToString());
		xmlWriter.WriteAttributeString("layoutAreaHeight", tour.LayoutAreaSize.Height.ToString());
	}

	private void WriteLayoutXml(TourPage tourPage)
	{
		SlideLayout slideLayout = tourPage.ActiveSlideLayout;

        // Convert deprecated popup layouts to a supported layout.
        SlideLayoutPattern pattern = slideLayout.Pattern;
        if (tour.V4 && tourPage.SlidesPopup)
        {
            if (pattern == SlideLayoutPattern.VIITT || pattern == SlideLayoutPattern.VTTII)
                pattern = SlideLayoutPattern.HIITT;
        }

        xmlWriter.WriteAttributeString("layoutId", pattern.ToString());

		if (tour.V3CompatibilityEnabled)
        {
			Size tourSize = tour.TourSize;
			xmlWriter.WriteAttributeString("pageWidth", tourSize.Width.ToString());
			xmlWriter.WriteAttributeString("pageHeight", tourSize.Height.ToString());

			xmlWriter.WriteAttributeString("canvasWidth", tour.LayoutAreaSize.Width.ToString());
			xmlWriter.WriteAttributeString("canvasHeight", tour.LayoutAreaSize.Height.ToString());

			xmlWriter.WriteAttributeString("layoutAreaWidth", tour.LayoutAreaSize.Width.ToString());
			xmlWriter.WriteAttributeString("layoutAreaHeight", tour.LayoutAreaSize.Height.ToString());
        }

		xmlWriter.WriteAttributeString("layoutSpacingH", slideLayout.Spacing.H.ToString());
		xmlWriter.WriteAttributeString("layoutSpacingV", slideLayout.Spacing.V.ToString());

		xmlWriter.WriteAttributeString("imageAreaWidth", slideLayout.ImageArea.Width.ToString());
		xmlWriter.WriteAttributeString("imageAreaHeight", slideLayout.ImageArea.Height.ToString());

		if (tourPage.IsGallery)
		{
			xmlWriter.WriteAttributeString("mapWidth", tourPage.MapAreaSize.Width.ToString());
			xmlWriter.WriteAttributeString("mapHeight", tourPage.MapAreaSize.Height.ToString());
			xmlWriter.WriteAttributeString("mapAreaScale", "1.0");
			
            xmlWriter.WriteAttributeString("mapZoomLevel", "0");
            xmlWriter.WriteAttributeString("mapZoomPercent", "100");
            xmlWriter.WriteAttributeString("mapZoomMidLevel","0");
            xmlWriter.WriteAttributeString("mapPanX", "0");
			xmlWriter.WriteAttributeString("mapPanY", "0");
            xmlWriter.WriteAttributeString("mapPanXEditor", "0");
			xmlWriter.WriteAttributeString("mapPanYEditor", "0");
            xmlWriter.WriteAttributeString("mapFocusX", "0");
            xmlWriter.WriteAttributeString("mapFocusY", "0");
            xmlWriter.WriteAttributeString("mapFocusPercent", "0");
        }
        else
		{
            // Set a flag to support auto-zoomable maps in V4. In V3, the dimensions of the full size
            // size map image, and the map area scale and zoom percent relative to the full size image,
            // are emitted for both zoomable and non-zoomable maps. In V4, non-zoomable maps are treated
            // as though the user uploaded the scaled-size map image and those maps are made auto-zoomable
            // by the runtime which has no need to know about the full size dimensions or scale.
            bool mapCanZoomOrV3 = tourPage.MapCanZoom || tour.V3CompatibilityEnabled;

            Size mapImageSize;
            if (mapCanZoomOrV3)
                mapImageSize = tourPage.MapImage.Size;
            else
                mapImageSize = tourPage.ScaledMapSize;

            Size mapSize = tourPage.MapImage.HasFile ? mapImageSize : tourPage.MapAreaSize;
			xmlWriter.WriteAttributeString("mapWidth", mapSize.Width.ToString());
			xmlWriter.WriteAttributeString("mapHeight", mapSize.Height.ToString());

            double mapAreaScale = mapCanZoomOrV3 ? tourPage.CalculateMapAreaScale() : 1.0;
			xmlWriter.WriteAttributeString("mapAreaScale", mapAreaScale.ToString());

            double zoomPercent = tourPage.MapZoomLevel;
            
            // Make sure the percent is not negative which it is in some old V3 tours.
            if (zoomPercent < 0)
                zoomPercent = 0;

            int panX = tourPage.MapZoomX;
            int panY = tourPage.MapZoomY;
            double mapZoomMidLevel = 0.0;
            int mapZoomLevel = tourPage.MapCanZoom ? 1 : 0;

            if (tourPage.MapCanZoom && tour.V3CompatibilityEnabled)
			{
                // Determine the half way point betweeen zoomed all the way out and zoomed to 100%.
                // For example, if the map's zoomable range is 50% to 100%, half way is 75%.
                mapZoomMidLevel = Math.Round((((1.0 - mapAreaScale) / 2) + mapAreaScale) * 100);

                // If a V3 map is zoomed-in less than half way, it will display zoomed out, otherwise zoomed in.
                bool zoomedAllTheWayOut = zoomPercent <= mapZoomMidLevel;
                mapZoomLevel = zoomedAllTheWayOut ? 1 : 2;

                if (zoomedAllTheWayOut)
                {
                    panX = 0;
                    panY = 0;
                }
            }

            xmlWriter.WriteAttributeString("mapZoomLevel", mapZoomLevel.ToString());
            xmlWriter.WriteAttributeString("mapZoomMidLevel",mapZoomMidLevel.ToString());

            // Handle the case where this tour is still using the old V3 mapZoomLimit and markerZoomLimit values that
            // were once used to store zooming limits for SVG maps. In V4, mapZoomLimit is used as MapFocusZoomPercent
            // and markerZoomLimit is used as MapFocus which packs to short x and y values into the single int.
            // In V3, both propeties could have values between 1 and 10 which are not meaningful for V4 because those
            // value would mean a zoom percent of between 1% and 10% and a focus of between 0,1 and 0,10. Emit both
            // of them as zero which is the V4 default. Eventually, when the user locks the map, the values will get
            // updated in the database as valid V4 values, but until then, this code ensures that the runtime won't
            // operate on the V3 values.
            if (tour.V4 && tourPage.MapFocus >= 1 && tourPage.MapFocus <= 10)
            {
                tourPage.MapFocus = 0;
                tourPage.MapFocusPercent = 0;
            }

            // Write the map state values from the last time the user locked the map.
            xmlWriter.WriteAttributeString("mapFocusX", tourPage.MapFocusX.ToString());
            xmlWriter.WriteAttributeString("mapFocusY", tourPage.MapFocusY.ToString());
            xmlWriter.WriteAttributeString("mapFocusPercent", tourPage.MapFocusPercent.ToString());

            // Write the zoom and pan values that were last used in the Map Editor. They are different than
            // and independent of the lock values (unless the user locked the map and then immediately left
            // the editor, in which case they would be the same as the lock values).
            double mapZoomPercent = mapCanZoomOrV3 ? zoomPercent : 100;
            xmlWriter.WriteAttributeString("mapZoomPercent", mapZoomPercent.ToString());
            xmlWriter.WriteAttributeString("mapPanX", panX.ToString());
			xmlWriter.WriteAttributeString("mapPanY", panY.ToString());
      }

        xmlWriter.WriteAttributeString("mapAreaWidth", tourPage.MapAreaSize.Width.ToString());
		xmlWriter.WriteAttributeString("mapAreaHeight", tourPage.MapAreaSize.Height.ToString());

		xmlWriter.WriteAttributeString("textAreaWidth", slideLayout.TextArea.Size.Width.ToString());
		xmlWriter.WriteAttributeString("textAreaHeight", slideLayout.TextArea.Size.Height.ToString());

		SlideLayoutMargin margin = slideLayout.Margin;
		xmlWriter.WriteAttributeString("layoutMarginBottom", margin.Bottom.ToString());
		xmlWriter.WriteAttributeString("layoutMarginLeft", margin.Left.ToString());
		xmlWriter.WriteAttributeString("layoutMarginRight", margin.Right.ToString());
		xmlWriter.WriteAttributeString("layoutMarginTop", margin.Top.ToString());
	}

	private void WriteMapXml(TourPage tourPage)
	{
		string mapFileName = tourPage.MapImage.FileNameInternal;
		xmlWriter.WriteAttributeString("mapFileName", mapFileName);

		if (tourPage.MapInsetLocation != 0)
		{
			string mapInsetFileName = mapFileName;
			xmlWriter.WriteAttributeString("mapInsetFileName", "_" + mapInsetFileName);
		}

		string src;
		if (tourPage.TourViews.Count > 0 && (tourPage.IsDataSheet || !tourPage.ActiveSlideLayout.HasMapArea))
		{
			TourView firstTourView = tourPage.FirstTourView;
			src = firstTourView.HasImage ? firstTourView.Image.FileNameInternal : string.Empty;
		}
		else
		{
			// Set the image src to a blank image so that the missing image icon does not appear
			// while the first slide's image gets loaded.  Note that we used to set the src to the
			// image for the first slide so that it would load while the page loaded.  We stopped
			// doing that because in the case where currentState is being restored or when the default slide
			// is overridden by an explicit slide on the query string, the first slide image would
			// appear briefly and then get replaced by the restored or explicit image.
			src = Runtime.RuntimeFileName(RuntimeFile.Blank);
		}
		xmlWriter.WriteAttributeString("imageSrc", src); // for the map image
	}

	private void WritePageOptionsXml(TourPage tourPage)
	{
		xmlWriter.WriteAttributeString("title", tourPage.Tour.BrowserTitle);
		
		// Don't show the view title on Info pages because there is no option to turn it off
		// on the Page Options page. It's easy enough for someone to type in their own in the text area.
		bool showSlideTitle = !tourPage.IsDataSheet && tourPage.ShowSlideTitle;
		xmlWriter.WriteAttributeString("showSlideTitle", showSlideTitle.ToString());
		
		xmlWriter.WriteAttributeString("showHelp", tourPage.ShowInstructions.ToString());
		xmlWriter.WriteAttributeString("firstTourViewId", tourPage.FirstTourViewId.ToString());

        // Set the option to show slide names in the menu.  This option will only have an effect if
        // there is a menu and it is a left menu.
        bool showSlideNamesInSideMenu = tour.MenuLocationIdEffective == (int)Tour.MenuLocation.Left && tourPage.ShowSlideNamesInMenu;
        xmlWriter.WriteAttributeString("showSlideNamesInMenu", showSlideNamesInSideMenu.ToString());

		// V4 utilizes the page title even if the title bar is turned off.
        if (tour.HasTitle || tour.V4)
		{
            string title = tour.V3CompatibilityEnabled ? tourPage.TitleOrName : tourPage.Title; 
            xmlWriter.WriteAttributeString("pageTitle", title);
		}

		if (tourPage.ShowRouteList)
		{
			// If the user wants to test routes. Make sure there is a route hotspot.
			TourView routeHotspot = tourPage.GetRouteHotspot();
			if (routeHotspot != null)
				xmlWriter.WriteAttributeString("testRouteId", routeHotspot.SlideId);
		}

        xmlWriter.WriteAttributeString("excludeFromNavigation", tourPage.ExcludeFromNavigation.ToString());
    }

    private void WritePageTable()
	{
		// The page table is only used by V3 tours.
		// Create a JavaScript array from the data.
		StringBuilder sb = new StringBuilder();
		for (int index = 0; index < pageTable.Count; index++)
		{
			if (index > 0)
				sb.Append(",");
			sb.Append(string.Format("'{0}'", pageTable[index]));
		}

		xmlWriter.WriteStartElement("pageTable");
		xmlWriter.WriteString(sb.ToString());
		xmlWriter.WriteEndElement();
	}

    private void WritePopupAttributes(TourPage tourPage)
    {
		if (!tourPage.SlidesPopup)
			return;

		SlideLayout layoutAreaSlideLayout = tourPage.LayoutAreaSlideLayout;
		SlideLayout popupSlideLayout = tourPage.PopupSlideLayout;
		PopupOptions popupOptions = tourPage.PopupOptions;

		PopupDelayType delayType = popupOptions.Location == PopupLocation.FixedAlwaysVisible ? PopupDelayType.None : popupOptions.DelayType;
		xmlWriter.WriteAttributeString("popupDelayType", ((int)delayType).ToString());
		xmlWriter.WriteAttributeString("popupDelay", popupOptions.Delay.ToString());
		xmlWriter.WriteAttributeString("popupSlides", "True");
		
		// These are emitted for convenience at runtime and to make the runtime logic a bit simpler.
		xmlWriter.WriteAttributeString("popupSlidesFixed", popupOptions.LocationIsFixed ? "True" : "False");
		xmlWriter.WriteAttributeString("popupSlidesDynamic", popupOptions.LocationIsFixed ? "False" : "True");

		int x = 0;
		int y = 0;
		if (popupOptions.LocationIsFixed)
		{
			x = popupOptions.LocationPoint.X;
			y = popupOptions.LocationPoint.Y;
		}
		xmlWriter.WriteAttributeString("popupLocationX", x.ToString());
		xmlWriter.WriteAttributeString("popupLocationY", y.ToString());

		// Map the user interface popup location to the runtime popup location. The runtime
		// uses a more generic location combined with the popupAllowMouseover flag. Note that
		// these hard-coded value must be kept in sync with their counterparts in mapsalive-page.js.
		int popupLocation = 0;
		if (popupOptions.Location == PopupLocation.MarkerCenter)
			popupLocation = 1;
		else if (popupOptions.Location == PopupLocation.MarkerEdgeInside || popupOptions.Location == PopupLocation.MarkerEdgeOutside)
			popupLocation = 2;
		else if (popupOptions.Location == PopupLocation.Mouse || popupOptions.Location == PopupLocation.MouseFollow)
			popupLocation = 3;
		else if (popupOptions.Location == PopupLocation.Fixed)
			popupLocation = 4;
		else if (popupOptions.Location == PopupLocation.FixedAlwaysVisible)
			popupLocation = 5;

		xmlWriter.WriteAttributeString("popupLocation", popupLocation.ToString());
		xmlWriter.WriteAttributeString("popupBestSideSequence", popupOptions.BestSideSequence.ToString());
		xmlWriter.WriteAttributeString("popupAllowMouseover", popupOptions.LocationAllowsMouseOntoPopup ? "True" : "False");

        PopupArrowType arrowType = popupOptions.ArrowType;
        if (tour.V3CompatibilityEnabled)
        {
            // Make sure the arrow type is one supported by V3.
            if (arrowType != PopupArrowType.None && arrowType != PopupArrowType.Large && arrowType != PopupArrowType.Small)
                arrowType = PopupArrowType.Large;
        }
        xmlWriter.WriteAttributeString("popupArrowType", ((int)arrowType).ToString());

		xmlWriter.WriteAttributeString("popupPinOnClick", popupOptions.PinOnClick && popupOptions.Location != PopupLocation.FixedAlwaysVisible ? "True" : "False");
		xmlWriter.WriteAttributeString("popupPinMsg", popupOptions.PinMessage.Replace("\"", "\\\""));
		xmlWriter.WriteAttributeString("popupWidth", popupSlideLayout.OuterSize.Width.ToString());
		xmlWriter.WriteAttributeString("popupHeight", popupSlideLayout.OuterSize.Height.ToString());
		xmlWriter.WriteAttributeString("popupMinWidth", popupOptions.MinSize.Width.ToString());
		xmlWriter.WriteAttributeString("popupMinHeight", popupOptions.MinSize.Height.ToString());
		xmlWriter.WriteAttributeString("popupTextOnlyWidth", popupOptions.TextOnlyWidth.ToString());
		xmlWriter.WriteAttributeString("popupBorderWidth", popupOptions.BorderWidth.ToString());
		xmlWriter.WriteAttributeString("popupMapMarginTop", layoutAreaSlideLayout.Margin.Top.ToString());
		xmlWriter.WriteAttributeString("popupMapMarginRight", layoutAreaSlideLayout.Margin.Right.ToString());
		xmlWriter.WriteAttributeString("popupMapMarginBottom", layoutAreaSlideLayout.Margin.Bottom.ToString());
		xmlWriter.WriteAttributeString("popupMapMarginLeft", layoutAreaSlideLayout.Margin.Left.ToString());
		xmlWriter.WriteAttributeString("popupMarkerOffset", popupOptions.MarkerOffset.ToString());
		xmlWriter.WriteAttributeString("popupCornerRadius", popupOptions.PopupCornerRadius.ToString());
		xmlWriter.WriteAttributeString("popupImageCornerRadius", popupOptions.ImageCornerRadius.ToString());
		xmlWriter.WriteAttributeString("popupDropShadowDistance", popupOptions.DropShadowDistance.ToString());

        // Write color attributes for popups.
		xmlWriter.WriteAttributeString("colorPopupBackground", popupOptions.BackgroundColor);
		xmlWriter.WriteAttributeString("colorPopupBorder", popupOptions.BorderColor);
		xmlWriter.WriteAttributeString("colorPopupText", popupOptions.TextColor);
		xmlWriter.WriteAttributeString("colorPopupTitleText", popupOptions.TitleTextColor);
    }

	private void WriteRoutesXml(TourPage tourPage)
	{
		string routesXml = tourPage.RoutesXml;
		if (routesXml.Length == 0)
			return;

		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(routesXml);
			
			// Parse the DOM object and write it back out. We tried using xmlWriter.WriteRaw here
			// to simply copy the string to the output, but for very long strings (over 120k) it
			// had a bug that reported an error indicating that there was a bad character in the XML.
			XmlNodeList routeNodes = xmlDocument.SelectNodes("/routes/route");
			if (routeNodes.Count > 0)
			{
				xmlWriter.WriteStartElement("routes");
				foreach (XmlNode routeNode in routeNodes)
				{
					xmlWriter.WriteStartElement("route");
					xmlWriter.WriteAttributeString("id", routeNode.Attributes["id"].Value);
					xmlWriter.WriteString(routeNode.InnerText);
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
			}
		}
		catch (Exception ex)
		{
			Debug.Fail("Unexpected exception in WriteRoutesXml: " + ex.Message);
		}
	}

	private void WriteSlideDropdownListXml(TourPage tourPage)
	{
		SlideLayoutMargin margin = tourPage.ActiveSlideLayout.Margin;
		xmlWriter.WriteAttributeString("showDropdown", tourPage.ShowSlideList.ToString());
		xmlWriter.WriteAttributeString("slideListInstructions", tourPage.SlideListInstructions);

        if (tour.V3CompatibilityEnabled)
    		xmlWriter.WriteAttributeString("dropdownMarginRight", margin.Right.ToString());
	}

	private void WriteCategoryTable()
	{
		StringBuilder sb = new StringBuilder();

		foreach (Category category in tour.CategoryManager.CategoryTable)
		{
			// Get only the tour views that are on the current page in this category.
			ArrayList tourViews = tour.CategoryManager.GetTourViews(category.Id, currentTourPage.Id);
			if (tourViews.Count == 0)
				continue;

			// Put a separator between categories.
			if (sb.Length > 0)
				sb.Append(",");

			// Emit the code and hotspot Ids for this category.
			sb.Append("[");
			sb.Append(string.Format("'{0}'", Utility.JavascriptSingleQuotedString(category.Code.ToLower())));
			foreach (TourView tourView in tourViews)
			{
				sb.Append(",");
				sb.Append(string.Format("'{0}'", Utility.JavascriptSingleQuotedString(tourView.SlideId)));
			}
			sb.Append("]");
		}

		xmlWriter.WriteStartElement("categoryTable");
		xmlWriter.WriteString(sb.ToString());
		xmlWriter.WriteEndElement();
	}

	private void WriteMarkerInstanceTable()
	{
		// Create a JavaScript array from the data.
		StringBuilder sb = new StringBuilder();
		for (int index = 0; index < markerInstanceTable.Count; index++)
		{
			if (index > 0)
				sb.Append(",");
			string data = (string)markerInstanceTable[index];
			sb.Append(string.Format("'{0}'", data));
		}

		xmlWriter.WriteStartElement("markerInstanceTable");
		xmlWriter.WriteString(sb.ToString());
		xmlWriter.WriteEndElement();
	}

	private void WriteMarkerStyleTable()
	{
		// Create a JavaScript array from the data.
		StringBuilder sb = new StringBuilder();
		for (int index = 0; index < markerStyleTable.Count; index++)
		{
			if (index > 0)
				sb.Append(",");
			string data = (string)markerStyleTable[index];
			sb.Append(string.Format("'{0}'", data));
		}

		xmlWriter.WriteStartElement("markerStyleTable");
		xmlWriter.WriteString(sb.ToString());
		xmlWriter.WriteEndElement();
	}

	private void WriteSlideTable()
	{
		// Sort the table alphabetically.
		slideTable.Sort(new SlideDataComparer());

		// Create a JavaScript array from the data.
		StringBuilder sb = new StringBuilder();
		for (int index = 0; index < slideTable.Count; index++)
		{
			if (index > 0)
				sb.Append(",");
			SlideTableEntry slideData = (SlideTableEntry)slideTable[index];
			sb.Append(string.Format("'{0}'", slideData.Data));
		}

		xmlWriter.WriteStartElement("slideTable");
		xmlWriter.WriteString(sb.ToString());
		xmlWriter.WriteEndElement();
	}

	private void WriteStringTable()
	{
		StringBuilder sb = new StringBuilder();

		for (int index = 0; index < stringTable.Count; index++)
		{
			string s = Utility.JavascriptSingleQuotedString((string)stringTable[index]);
			
			// Make sure that a newline cannot make it's way into the string. A newline
			// within a Javascript string is not allowed. It could get there if in slide
			// content that was imported via an Excel file.
			s = s.Replace("\r", "");
			s = s.Replace("\n", "");

			// Filter out the unicode line break and paragraph break characters because if
			// they get into the string table, Firefox seems to treat them as actual line
			// breaks which results in an erroneous "unterminated string" JavaScript error.
			s = s.Replace("\u2028", "");
			s = s.Replace("\u2029", "");
			
			if (index > 0)
				sb.Append(",");

			sb.Append("'");

			foreach (char c in s)
			{
				if (Utility.IsLegalXmlChar(c))
				{
					sb.Append(c);
				}
				else
				{
					sb.Append("?");
				}
			}
			
			sb.Append("'");
		}
		xmlWriter.WriteStartElement("stringTable");
		xmlWriter.WriteString(sb.ToString());
		xmlWriter.WriteEndElement();
	}

	private void WriteTooltipAttributes(TourPage tourPage)
	{
		if (tourPage.IsDataSheet)
			return;

		TooltipStyle tooltipStyle = tourPage.TooltipStyle;
		FontStyleResource fontStyleResource = tourPage.TooltipStyle.FontStyleResource;
		
		xmlWriter.WriteAttributeString("tooltipBackgroundColor", tooltipStyle.BackgroundIsTransparent ? "transparent" : tooltipStyle.BackgroundColor);
		xmlWriter.WriteAttributeString("tooltipTextColor", tooltipStyle.TextColor);
		xmlWriter.WriteAttributeString("tooltipBorder", string.Format("solid {0}px {1}", tooltipStyle.LineWidth, tooltipStyle.LineColor));
		xmlWriter.WriteAttributeString("tooltipFontSize", fontStyleResource.FontSizePx.ToString());
		xmlWriter.WriteAttributeString("tooltipFontFamily", fontStyleResource.FontFamily);
		xmlWriter.WriteAttributeString("tooltipPadding", tooltipStyle.Padding.ToString());
 		xmlWriter.WriteAttributeString("tooltipFontWeight", fontStyleResource.Bold ? "bold" : "normal");
 		xmlWriter.WriteAttributeString("tooltipFontStyle", fontStyleResource.Italic ? "italic" : "normal");
 		xmlWriter.WriteAttributeString("tooltipUnderline", fontStyleResource.Underline ? "underline" : "none");
		if (tooltipStyle.MaxWidth > 0 || tour.V4)
			xmlWriter.WriteAttributeString("tooltipMaxWidth", tooltipStyle.MaxWidth.ToString());
	}

	private void WriteTourAttributes()
	{
		xmlWriter.WriteAttributeString("tourId", tour.Id.ToString());
		xmlWriter.WriteAttributeString("tourFolderUrl", App.TourFolderUrl);
		xmlWriter.WriteAttributeString("firstPageNumber", tour.FirstPage.PageNumber.ToString());
		xmlWriter.WriteAttributeString("buildId", tour.BuildId.ToString());
		xmlWriter.WriteAttributeString("name", tour.Name);
		xmlWriter.WriteAttributeString("accountId", MapsAliveState.Account.Id.ToString());
		xmlWriter.WriteAttributeString("appVersion", App.Version);
		xmlWriter.WriteAttributeString("fileTourLoaderJs", string.Format(Runtime.RuntimeFileName(RuntimeFile.MapsAliveLoaderJs), tour.Id));
		xmlWriter.WriteAttributeString("enableV3Compatibility", tour.V3CompatibilityEnabled ? "True" : "False");
		xmlWriter.WriteAttributeString("disableKeyboardShortcuts", tour.KeyboardShortcutsDisabled ? "True" : "False");
		xmlWriter.WriteAttributeString("selectsOnTouchStart", tour.MapSelectsOnTouchStart ? "True" : "False");
		xmlWriter.WriteAttributeString("enlargeHitTestArea", tour.MapEnlargeHitTestArea ? "True" : "False");
		xmlWriter.WriteAttributeString("disableBlendEffect", tour.MapDisableBlendEffect ? "True" : "False");
		xmlWriter.WriteAttributeString("disableSmoothPanning", tour.MapDisableSmoothPanning ? "True" : "False");
		xmlWriter.WriteAttributeString("showZoomControlOnIOs", tour.MapShowZoomControlOnIOs ? "True" : "False");
		xmlWriter.WriteAttributeString("entirePopupVisible", tour.MapEntirePopupVisible ? "True" : "False");
		xmlWriter.WriteAttributeString("enableImagePreloading", tour.MapEnableImagePreloading ? "True" : "False");
		xmlWriter.WriteAttributeString("webAppCapable", tour.TourIsWebAppCapable ? "True" : "False");
		xmlWriter.WriteAttributeString("viewPortIsDeviceWidth", tour.MapViewPortIsDeviceWidth ? "True" : "False");
		xmlWriter.WriteAttributeString("useTouchUiOnDeskop", tour.UseTouchUiOnDesktop ? "True" : "False");
		xmlWriter.WriteAttributeString("hideMenu", tour.HideMenu ? "True" : "False");
		xmlWriter.WriteAttributeString("isFlexMapTour", tour.IsFlexMapTour ? "True" : "False");

		if (tour.V3CompatibilityEnabled)
        {
			xmlWriter.WriteAttributeString("fileMapsAliveJs", Runtime.RuntimeFileName(RuntimeFile.MapsAliveJs));
			xmlWriter.WriteAttributeString("fileMapViewerJs", Runtime.RuntimeFileName(RuntimeFile.MapViewerJs));
		}

		if (tour.UseSoundManager)
			xmlWriter.WriteAttributeString("fileSoundManager", Runtime.RuntimeFileName(RuntimeFile.SoundManagerJs));

		if (tour.HasCustomHtmlCss)
			xmlWriter.WriteAttributeString("hasCustomHtmlCss", "True");
		
		if (tour.HasCustomHtmlJavaScript)
		{
			string src = tour.CustomHtmlJavaScriptIncludeSrc;
			if (src.Length > 0)
			{
				xmlWriter.WriteAttributeString("javascriptIncludeSrc", src);
			}
			xmlWriter.WriteAttributeString("hasCustomHtmlJavaScript", "True");
		}
		
		if (tour.HasCustomHtmlTop && !tour.IsFlexMapTour)
			xmlWriter.WriteAttributeString("hasCustomHtmlTop", "True");
		if (tour.HasCustomHtmlAbsolute)
			xmlWriter.WriteAttributeString("hasCustomHtmlAbsolute", "True");
		if (tour.HasCustomHtmlBottom && !tour.IsFlexMapTour)
			xmlWriter.WriteAttributeString("hasCustomHtmlBottom", "True");

        xmlWriter.WriteAttributeString("plan", MapsAliveState.Account.Plan.ToString());
	}

	private void WriteTourPageXml(TourPage tourPage)
	{
		xmlWriter.WriteStartElement("tourPage");

		xmlWriter.WriteAttributeString("tourId", tour.Id.ToString());
		xmlWriter.WriteAttributeString("pageId", tourPage.Id.ToString());
		xmlWriter.WriteAttributeString("pageNumber", tourPage.PageNumber.ToString());
		xmlWriter.WriteAttributeString("pageName", tourPage.Name);
		xmlWriter.WriteAttributeString("mapId", tourPage.PageId);

        if (tour.V4)
            WriteTooltipAttributes(tourPage);

        xmlWriter.WriteAttributeString("infoPage", tourPage.IsDataSheet ? "True" : "False");
		xmlWriter.WriteAttributeString("isGallery", tourPage.IsGallery ? "True" : "False");

		xmlWriter.WriteAttributeString("blinkCount", tourPage.SelectedMarkerBlink.ToString());
		xmlWriter.WriteAttributeString("visitedMarkerAlpha", tourPage.VisitedMarkerAlpha.ToString());

		xmlWriter.WriteAttributeString("runSlideShow", tourPage.RunSlideShow ? "True" : "False");
		xmlWriter.WriteAttributeString("slideShowInterval", tourPage.SlideShowInterval.ToString());

		int mapInsetLocation = tourPage.MapInsetLocation;

		xmlWriter.WriteAttributeString("mapInsetLocation", mapInsetLocation.ToString());
		xmlWriter.WriteAttributeString("mapInsetSize", tourPage.MapInsetSize.ToString());
		xmlWriter.WriteAttributeString("mapInsetColor", tourPage.MapInsetColor);
		xmlWriter.WriteAttributeString("mapZoomControlColor", tourPage.PanZoomControlColorOff);
		xmlWriter.WriteAttributeString("mapShowZoomControl", tourPage.MapCanZoom && tourPage.ShowPanZoomControls ? "True" : "False");
        xmlWriter.WriteAttributeString("mapImageSharpening", tourPage.MapImageSharpening.ToString());

        xmlWriter.WriteAttributeString("showInstructions", tourPage.ShowInstructions ? "True" : "False");
		if (tourPage.ShowInstructions)
		{
			string text = tourPage.InstructionsText.Replace("\r\n", "");
			text = text.Replace("\n", "");
			xmlWriter.WriteAttributeString("instructionsTitle", Utility.JavascriptDoubleQuotedString(tourPage.InstructionsTitle));
			xmlWriter.WriteAttributeString("instructionsText", Utility.JavascriptDoubleQuotedString(text));
			xmlWriter.WriteAttributeString("instructionsWidth", tourPage.InstructionsWidth.ToString());
			xmlWriter.WriteAttributeString("instructionsBgColor", tourPage.InstructionsBgColor);
			xmlWriter.WriteAttributeString("instructionsColor", tourPage.InstructionsColor);
		}

		if (tourPage.UsesLiveData)
			xmlWriter.WriteAttributeString("usesLiveData", "True");

		bool excludeDataSheet = false;
		if (tourPage.IsDataSheet)
		{
			TourView firstTourView = tourPage.FirstTourView;
			if (firstTourView == null)
			{
				Debug.Fail(string.Format("Anomally 1127: FirstTourView for page {0} is null", tourPage.Id));
			}
			else
			{
				excludeDataSheet = firstTourView.ExcludeFromDirectory;
			}
		}
		
		bool exclude = tourPage.ExcludeFromNavigation || excludeDataSheet;
		xmlWriter.WriteAttributeString("exclude", exclude ? "True" : "False");

		if (tourPage == currentTourPage)
		{
			WritePageOptionsXml(tourPage);
			WriteMapXml(tourPage);
			WriteLayoutXml(tourPage);
			if (tour.V3CompatibilityEnabled)
				WriteBannerXml();
			WriteSlideDropdownListXml(tourPage);
			WritePopupAttributes(tourPage);
			WriteTourViewXml(tourPage);
		}

		xmlWriter.WriteEndElement(); // tourPage
	}

	private void WriteTourPagesXml()
	{
		xmlWriter.WriteStartElement("tourPages");
		foreach (TourPage tourPage in tour.TourPages)
		{
			WriteTourPageXml(tourPage);
		}
		xmlWriter.WriteEndElement();
	}

	private void WriteTourViewXml(TourPage tourPage)
	{
		tourPage.TourViews.Sort(new TourViewComparer());
		foreach (TourView tourView in tourPage.TourViews)
		{
			xmlWriter.WriteStartElement("tourView");
			xmlWriter.WriteAttributeString("name", tourView.Title);
			xmlWriter.WriteAttributeString("viewId", tourView.Id.ToString());
			xmlWriter.WriteElementString("toolTip", tourView.ToolTip);
			xmlWriter.WriteEndElement(); // tourView
		}
	}
}
