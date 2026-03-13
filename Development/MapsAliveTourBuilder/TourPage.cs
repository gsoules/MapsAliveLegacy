// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Web;

public partial class TourPage
{
	// These flags get written to the database as a single integer value.
	// We use this bit mask approach for tracking changes so that we can
	// add new flags without having to add new colums to the TourPage table
	// When you add a new flag, DO NOT CHANGE the hex value of existing
	// flags.  If you do, you will change the meaning of the flags in all
	// existing map pages in the database.
	[Flags]
	private enum ChangeFlags
	{
		TourHtml		= 0x00000001,
		Title			= 0x00000002,
		SlideLayout		= 0x00000004,
		MapMarker		= 0x00000008,
		MapImage		= 0x00000010,
		MapAreaSize		= 0x00000020,
		ImageAreaSize	= 0x00000040,
		TextAreaSize	= 0x00000080,
		FirstTourView	= 0x00000100,
		TourViews		= 0x00000200,
		Name			= 0x00000400,
		Instructions	= 0x00000800,
		MapSize			= 0x00001000,
		Menu			= 0x00002000,
		SlideList		= 0x00004000,
		SlideShow		= 0x00008000,
		Map				= 0x00010000,
		BannerImage		= 0x00020000,
		BannerOptions	= 0x00040000,
		PageId			= 0x00080000,
		SlideTitle		= 0x00100000,
		TooltipStyle	= 0x00200000,
		Routes			= 0x00400000
	}

	private class SavedLayout
	{
		public SlideLayout LayoutAreaSlideLayout;
		public double MapZoomLevel;
		public int MapZoomX;
		public int MapZoomY;
		public Size MaxTourSize;
		public SlideLayout PopupSlideLayout;
		public bool SlidesPopup;
		public string TourBodyBackgroundColor;
		public int TourBodyMargin;
		public bool TourHasFooterStripe;
		public bool TourHasHeaderStripe;
		public bool TourHasTitle;
		public TourSizeType TourHeightType;
		public bool TourLeftAlignedInBrowser;
		public int TourMenuLocationId;
		public bool TourMenuScrolls;
		public int TourMenuStyleId;
		public int TourMenuWidth;
		public Size TourSize;
		public int ColorSchemeId;
		public TourSizeType TourWidthType;
	}

	private int buildId;
	private ChangeFlags changed;
	private DateTime dateCreated;
	private DateTime dateModified;
	private bool markersZoom;
	private bool excludeFromNavigation;
	private int firstTourViewId;
	private Size gallerySize;
	private bool hasNeverHadMap;
	private int id;
	private bool importingArchive;
	private bool importingMarkers;
	private string instructionsBgColor;
	private string instructionsColor;
	private string instructionsFont;
	private int instructionsFontSize;
	private int instructionsWidth;
	private string instructionsText;
	private string instructionsTitle;
	private SlideLayout layoutAreaSlideLayout;
	private LayoutManager layoutManager;
	private int layoutMinNonMapWidth;
	private int layoutMinNonMapHeight;
	private MapImage mapImage;
	private int mapImageId;
	private int mapImageAltId;
	private int mapLibraryVersion;
	private string name;
	private int pageNumber;
	private bool mapCanZoom;
	private string mapInsetColor;
	private int mapInsetLocation;
	private int mapInsetSize;
	private double mapZoomLevel;
	private int mapZoomX;
	private int mapZoomY;
	private int menuPosition;
	private int mouseOverDelay;
	private string panZoomControlColorOff;
	private string panZoomControlColorOn;
	private string pageId;
	private PopupOptions popupOptions;
	private SlideLayout popupSlideLayout;
	private TourLayout tourLayout;
	private bool runSlideShow;
	private SavedLayout savedLayout;
	private bool saveMapStateChanges;
	private int selectedMarkerBlink;
	private bool showLayoutAreaInLayoutEditor;
	private bool showInstructions;
	private bool showPanZoomControls;
	private bool showRouteList;
	private bool showSlideNamesInMenu;
	private bool showSlideList;
	private bool showSlideTitle;
	private string slideListInstructions;
	private int slideShowInterval;
	private bool slidesPopup;
	private int mapZoomLimit;
	private int markerZoomLimit;
	private string routesXml;
	private Byte[] thumbnailBytes;
	private string title;
	private TooltipStyle _tooltipStyle;
	private int tooltipStyleId;
	private Tour tour;
	private ArrayList _tourViews;
	private int visitedMarkerAlpha;
	private GalleryOptions galleryOptions;

	private const string defaultSlideListInstructions = "- Choose a hotspot -";
	private const int defaultPopupTextOnlyWidth = 300;

	private const string defaultInstructionsFont = "Arial";
	private const int defaultInstructionsFontSize = 12;

	public TourPage(Tour tour, bool isDataSheet, bool slidesPopup)
	{
		this.tour = tour;

		tourLayout = new TourLayout();

		// Set the intitial splitters to -1 when using popup slides. The negative number
		// will serve as a flag to let us know that the user has never explicitly chosen
		// a fixed layout if they ever switch from popups to fixed.
		int splitterH = slidesPopup ? -1 : tour.LayoutAreaSize.Height / 2;
		int splitterV = slidesPopup ? -1 : tour.LayoutAreaSize.Width / 2;

		layoutAreaSlideLayout = new SlideLayout(
			isDataSheet ? SlideLayoutPattern.HIITT : SlideLayoutPattern.VMMIT,
			tour.LayoutAreaSize,
			new SlideLayoutSplitters(splitterH, splitterV),
			new SlideLayoutMargin(0, 0, 0, 0),
			new SlideLayoutSpacing(8, 8));

		popupOptions = new PopupOptions(
            tour,
			0,
			2,
			5,
			0,
			4,
			"#666666",
			"#ffffff",
			"#000000",
			"#000000",
			PopupLocation.MarkerEdgeInside,
			new Point(10, 10),
			new Size(36, 36),
			PopupArrowType.Large,
			true,
			PopupDelayType.None,
			0,
			true,
			string.Empty,
			defaultPopupTextOnlyWidth,
			true,
			4);
		
		Size defaultPopupSize = new Size(600, 600);

		// Set the intitial splitters to -1 when using fixed slides. The negative number
		// will serve as a flag to let us know that the user has never explicitly chosen
		// a popup layout if they ever switch from fixed to popup.
		splitterH = !slidesPopup ? -1 : defaultPopupSize.Height - (defaultPopupSize.Height / 4);
        splitterV = !slidesPopup ? -1 : defaultPopupSize.Width / 2;
		
		popupSlideLayout = new SlideLayout(
			SlideLayoutPattern.HIITT,
			defaultPopupSize,
			new SlideLayoutSplitters(splitterH, splitterV),
			new SlideLayoutMargin(4, 4, 4, 4),
			new SlideLayoutSpacing(8, 8));

		this.slidesPopup = slidesPopup;

		layoutMinNonMapWidth = 200;
		layoutMinNonMapHeight = 100;
		layoutManager = new LayoutManager(this, ref layoutAreaSlideLayout, ref popupSlideLayout);
		
		pageId = string.Empty;
		slideListInstructions = defaultSlideListInstructions;
		menuPosition = tour.TourPageCount + 1;
		mapImage = new MapImage(this, 0);
		mapImage.Width = 0;
		mapImage.Height = 0;
		SetDefaultOptions();
		instructionsText = string.Empty;
		dateCreated = DateTime.Now;
		showSlideTitle = true;

		pageNumber = (int)MapsAliveDatabase.ReadScalar("sp_Tour_IncrementNextPageId", "@TourId", tour.Id, "@IsDataSheet", isDataSheet);

		tooltipStyleId = isDataSheet ? 0 : MapsAliveState.Account.DefaultResourceId(TourResourceType.TooltipStyle);

		MapPlaceholderColor = "#ffffff";

		RoutesXml = string.Empty;
		ShowRouteList = false;

		SetDefaultMapControlColors();

		hasNeverHadMap = true;
		
		InitGalleryOptions();

        // The Markers Zoom option is always the default for new V4 pages and there is no account-level preference.
        markersZoom = tour.V3CompatibilityEnabled ? false : true;

        // See the comments at the very end of InitializeTourPageFromDataRecord.
        mapZoomLimit = 0;
		markerZoomLimit = 0;
	}

	public TourPage(Tour tour, int tourPageId)
	{
		this.tour = tour;
		
		// Get the page having the specified Id.  If the Id is bad, no record will come back.
		MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow(
			"sp_TourPage_GetTourPageByTourPageId", "@TourId", tour.Id, "@TourPageId", tourPageId, "@ThemeId", tour.ThemeId);
		if (row == null)
			return;

		this.id = tourPageId;
		this.routesXml = string.Empty;
		InitializeTourPageFromDataRecord(row);
	}

	public void InitializeTourPageFromDataRecord(MapsAliveDataRecord record)
	{
		bool isRow = record is MapsAliveDataRow;
		tourLayout = new TourLayout();

		pageId = record.StringValue(Tag.pageId);
		pageNumber = record.IntValue(Tag.pageNumber);
		name = record.StringValue(Tag.name);
		title = record.StringValue(Tag.title);
		buildId = record.IntValue("BuildId");
		
		firstTourViewId = record.IntValue("FirstTourViewId", Tag.firstHotspotId);

		menuPosition = record.IntValue(Tag.menuPosition);
		excludeFromNavigation = record.BoolValue(Tag.excludeFromNavigation);

		showSlideList = record.BoolValue("ShowSlideList", Tag.showHotspotList);
		slideListInstructions = record.StringValue("SlideListInstructions", Tag.hotspotListInstructions);

		mapLibraryVersion = isRow ? record.IntValue("MapLibraryVersion") : 0;
		
		showSlideTitle = record.BoolValue("SlideLayoutShowTitle", Tag.showHotspotTitle);
		
		showInstructions = record.BoolValue("ShowHelp", Tag.showInstructions);
		instructionsText = record.StringValue("HelpText", Tag.instructionsText);
		instructionsWidth = record.IntValue("HelpWidth", Tag.instructionsWidth);
		instructionsBgColor = record.ColorValue("HelpBgColor", Tag.instructionsBackgroundColor);
		instructionsColor = record.ColorValue("HelpColor", Tag.instructionsColor);
		instructionsFont = defaultInstructionsFont;
		instructionsFontSize = defaultInstructionsFontSize;
		instructionsTitle = record.StringValue("HelpTitle", Tag.instructionsTitle);
		
		runSlideShow = record.BoolValue("ShowSlideShow", Tag.runSlideShow);
		slideShowInterval = record.IntValue(Tag.slideShowInterval);

		showSlideNamesInMenu = record.BoolValue("ShowSlideNamesInMenu", Tag.showHotspotNamesInMenu);

		if (isRow)
		{
			// When restoring from a database row, use the mapImageId from the DB. If there is an alt image Id,
			// The map image Id is the swf file of a Ready Map. Use the alt Id instead which is the jpg file.
			mapImageAltId = record.IntValue(Tag.mapImageAltId);
			if (mapImageAltId == 0)
				mapImageId = record.IntValue(Tag.mapImageId);
			else
				mapImageId = mapImageAltId;
			mapImage = new MapImage(this, mapImageId);
		}
		else
		{
			// When restoring from XML the map image will be restored from a file in the archive.
			mapImage.ReadyMapPackageId = record.IntValue(Tag.readyMapPackageId);
		}

		// V4 maps are always zoomable so enable map zoom even if it is false in the database.
        mapCanZoom = tour.V3CompatibilityEnabled ? record.BoolValue(Tag.mapCanZoom) : true;
		
        MapPlaceholderColor = record.ColorValue(Tag.mapPlaceholderColor);
		mapZoomX = record.IntValue(Tag.mapZoomX);
		mapZoomY = record.IntValue(Tag.mapZoomY);
		mapZoomLevel = record.DoubleValue(Tag.mapZoomLevel);
	
		mapZoomLimit = record.IntValue(Tag.mapZoomLimit);
		markerZoomLimit = record.IntValue(Tag.markerZoomLimit);
		
		mapInsetLocation = record.IntValue(Tag.mapInsetLocation);
		mapInsetSize = record.IntValue(Tag.mapInsetSize);
		
		showPanZoomControls = record.BoolValue(Tag.showPanZoomControls);
		mapInsetColor = record.ColorValue(Tag.mapInsetColor);
		panZoomControlColorOff = record.ColorValue(Tag.panZoomControlColorOff);
		panZoomControlColorOn = record.ColorValue(Tag.panZoomControlColorOn);

		mouseOverDelay = record.IntValue(Tag.mouseOverDelay);
		saveMapStateChanges = record.BoolValue(Tag.saveMapStateChanges);
		selectedMarkerBlink = record.IntValue(Tag.selectedMarkerBlink);
		visitedMarkerAlpha = record.IntValue(Tag.visitedMarkerAlpha);

		if (isRow)
		{
			dateCreated = record.DateTimeValue("CreateDate");
			dateModified = record.DateTimeValue("ModifyDate");
			changed = (ChangeFlags)record.LongValue("ChangeFlags");
		}

		slidesPopup = record.BoolValue("InPopupState", Tag.popupsEnabled);

		SlideLayoutSplitters layoutAreaSplitters = new SlideLayoutSplitters(
				record.IntValue("SlideLayoutFixedSplitterH", Tag.layoutAreaSplitterH),
				record.IntValue("SlideLayoutFixedSplitterV", Tag.layoutAreaSplitterV),
				record.BoolValue("SlideLayoutFixedSplitterLockedH", Tag.layoutAreaSplitterLockedH),
				record.BoolValue("SlideLayoutFixedSplitterLockedV",Tag.layoutAreaSplitterLockedV));

		SlideLayoutMargin layoutAreaMargin = new SlideLayoutMargin(
			record.IntValue("SlideLayoutFixedMarginTop", Tag.layoutAreaMarginTop),
			record.IntValue("SlideLayoutFixedMarginRight", Tag.layoutAreaMarginRight),
			record.IntValue("SlideLayoutFixedMarginBottom",Tag.layoutAreaMarginBottom),
			record.IntValue("SlideLayoutFixedMarginLeft", Tag.layoutAreaMarginLeft));

		SlideLayoutSpacing layoutAreaSpacing = new SlideLayoutSpacing(
			record.IntValue("SlideLayoutFixedSpacingH", Tag.layoutAreaSpacingH),
			record.IntValue("SlideLayoutFixedSpacingV", Tag.layoutAreaSpacingV));

        SlideLayoutPattern pattern = (SlideLayoutPattern)record.IntValue("SlideLayoutFixedType", Tag.layoutAreaTemplateId);

        // Convert deprecated popup layouts to a supported layout.
        if (SlideLayout.IsDeprecatedLayout(pattern, this) && tour.V4)
        {
            if (pattern == SlideLayoutPattern.VIITT || pattern == SlideLayoutPattern.VTTII)
                pattern = SlideLayoutPattern.HIITT;
            else
                pattern = SlideLayoutPattern.VMMIT;
        }

        layoutAreaSlideLayout = new SlideLayout(
			pattern,
			tour.LayoutAreaSize,
			layoutAreaSplitters,
			layoutAreaMargin,
			layoutAreaSpacing);

		Size minPopupSize = new Size(record.IntValue("SlideLayoutPopupMinWidth", Tag.popupMinWidth), record.IntValue("SlideLayoutPopupMinHeight", Tag.popupMinHeight));
		if (!Utility.HasWidthAndHeight(minPopupSize))
			minPopupSize = LayoutManager.MinAllowedSize;

		int popupTextOnlyWidth = record.IntValue("SlideLayoutPopupTextOnlyWidth", Tag.popupTextOnlyWidth);
		if (popupTextOnlyWidth == 0)
			popupTextOnlyWidth = defaultPopupTextOnlyWidth;

		popupOptions = new PopupOptions(
            tour,
			record.IntValue("SlideLayoutBestSideSequence", Tag.popupBestSideSequence),
			record.IntValue("SlideLayoutPopupBorderWidth", Tag.popupBorderWidth),
			record.IntValue("SlideLayoutPopupCornerRadius", Tag.popupCornerRadius),
			record.IntValue("SlideLayoutPopupImageRadius", Tag.popupImageRadius),
			record.IntValue("SlideLayoutPopupDropShadowDistance", Tag.popupDropShadowDistance),
			record.StringValue("SlideLayoutPopupBorderColor", Tag.popupBorderColor),
			record.StringValue("SlideLayoutPopupBackgroundColor", Tag.popupBackgroundColor),
			record.StringValue("SlideLayoutPopupTextColor", Tag.popupTextColor),
			record.StringValue("SlideLayoutPopupTitleTextColor", Tag.popupTitleTextColor),
			(PopupLocation)record.IntValue("SlideLayoutPopupLocation", Tag.popupLocation),
			new Point(record.IntValue("SlideLayoutPopupLocationX", Tag.popupLocationX), record.IntValue("SlideLayoutPopupLocationY", Tag.popupLocationY)),
			minPopupSize,
			(PopupArrowType)record.IntValue("SlideLayoutPopupArrowType", Tag.popupArrowType),
			record.BoolValue("SlideLayoutPopupShowTooltipWhenNoContent", Tag.popupShowTooltipWhenNoContent),
			(PopupDelayType)record.IntValue("SlideLayoutPopupDelayType", Tag.popupDelayType),
			record.IntValue("SlideLayoutPopupDelay", Tag.popupDelay),
			record.BoolValue("SlideLayoutPopupSticks", Tag.popupPinOnClick),
			record.StringValue("SlideLayoutPopupPinMessage", Tag.popupPinMessage),
			popupTextOnlyWidth,
			record.BoolValue("SlideLayoutPopupUseTourStyleColors", Tag.popupUseTourStyleColors),
			record.IntValue("SlideLayoutPopupMarkerOffset", Tag.popupMarkerOffset));

		SlideLayoutSplitters popupSplitters = new SlideLayoutSplitters(
			record.IntValue("SlideLayoutPopupSplitterH", Tag.popupSplitterH),
			record.IntValue("SlideLayoutPopupSplitterV", Tag.popupSplitterV),
			record.BoolValue("SlideLayoutPopupSplitterLockedH", Tag.popupSplitterLockedH),
			record.BoolValue("SlideLayoutPopupSplitterLockedV", Tag.popupSplitterLockedV));

		SlideLayoutMargin popupMargin = new SlideLayoutMargin(
			record.IntValue("SlideLayoutPopupMarginTop", Tag.popupMarginTop),
			record.IntValue("SlideLayoutPopupMarginRight", Tag.popupMarginRight),
			record.IntValue("SlideLayoutPopupMarginBottom", Tag.popupMarginBottom),
			record.IntValue("SlideLayoutPopupMarginLeft", Tag.popupMarginLeft));

		SlideLayoutSpacing popupSpacing = new SlideLayoutSpacing(
			record.IntValue("SlideLayoutPopupSpacingH", Tag.popupSpacingH),
			record.IntValue("SlideLayoutPopupSpacingV", Tag.popupSpacingV));

		popupSlideLayout = new SlideLayout(
			(SlideLayoutPattern)record.IntValue("SlideLayoutPopupType", Tag.popupTemplateId),
			new Size(record.IntValue("SlideLayoutPopupWidth", Tag.popupWidth), record.IntValue("SlideLayoutPopupHeight", Tag.popupHeight)),
			popupSplitters,
			popupMargin,
			popupSpacing);

		layoutMinNonMapWidth = record.IntValue("SlideLayoutMinNonMapWidth", Tag.layoutMinNonMapWidth);
		layoutMinNonMapHeight = record.IntValue("SlideLayoutMinNonMapHeight", Tag.layoutMinNonMapHeight);

		layoutManager = new LayoutManager(this, ref layoutAreaSlideLayout, ref popupSlideLayout);

		tooltipStyleId = record.IntValue(Tag.tooltipStyleId);

		RoutesXml = record.StringValue(Tag.routesXml);
		ShowRouteList = record.BoolValue(Tag.showRouteList);

		ReadyMapGroupId = record.IntValue(Tag.readyMapGroupId);

		galleryOptions = new GalleryOptions(
			record.BoolValue(Tag.isGallery),
			record.IntValue(Tag.gallerySpacingRow),
			record.IntValue(Tag.gallerySpacingColumn),
			record.BoolValue(Tag.galleryAutoSpacingRow),
			record.BoolValue(Tag.galleryAutoSpacingColumn),
			record.IntValue(Tag.galleryMarginTop),
			record.IntValue(Tag.galleryMarginLeft),
			(GalleryCellAlignH)record.IntValue(Tag.galleryCellAlignH),
			(GalleryCellAlignV)record.IntValue(Tag.galleryCellAlignV),
			record.BoolValue(Tag.galleryUseFixedRowHeight),
			record.BoolValue(Tag.galleryUseFixedColumnWidth),
			(ImageExpansionType)record.IntValue(Tag.galleryBackgroundType));

		markersZoom = record.BoolValue(Tag.markersZoom);
    }

	public enum Tag
	{
		id,
		isDataSheet,
		pageId,
		pageNumber,
		name,
		title,
		firstHotspotId,
		menuPosition,
		excludeFromNavigation,
		showHotspotList,
		hotspotListInstructions,
		showHotspotTitle,
		showInstructions,
		instructionsText,
		instructionsWidth,
		instructionsBackgroundColor,
		instructionsColor,
		instructionsTitle,
		runSlideShow,
		slideShowInterval,
		showHotspotNamesInMenu,
		mapImageId,
		mapImageAltId,
		mapPlaceholderColor,
		mapCanZoom,
		mapZoomX,
		mapZoomY,
		mapZoomLevel,
		mapZoomLimit,
		markerZoomLimit,
		mapInsetLocation,
		mapInsetSize,
		mapInsetColor,
		showPanZoomControls,
		panZoomControlColorOff,
		panZoomControlColorOn,
		mouseOverDelay,
		saveMapStateChanges,
		selectedMarkerBlink,
		visitedMarkerAlpha,
		popupsEnabled,
		layoutAreaSplitterH,
		layoutAreaSplitterV,
		layoutAreaSplitterLockedH,
		layoutAreaSplitterLockedV,
		layoutAreaMarginTop,
		layoutAreaMarginRight,
		layoutAreaMarginBottom,
		layoutAreaMarginLeft,
		layoutAreaSpacingH,
		layoutAreaSpacingV,
		layoutAreaTemplateId,
		popupBestSideSequence,
		popupMinWidth,
		popupMinHeight,
		popupTextOnlyWidth,
		popupBorderWidth,
		popupBorderColor,
		popupBackgroundColor,
		popupTextColor,
		popupTitleTextColor,
		popupLocation,
		popupLocationX,
		popupLocationY,
		popupArrowType,
		popupShowTooltipWhenNoContent,
		popupDelayType,
		popupDelay,
		popupPinOnClick,
		popupPinMessage,
		popupUseTourStyleColors,
		popupMarkerOffset,
		popupSplitterH,
		popupSplitterV,
		popupSplitterLockedH,
		popupSplitterLockedV,
		popupMarginTop,
		popupMarginRight,
		popupMarginBottom,
		popupMarginLeft,
		popupSpacingH,
		popupSpacingV,
		popupWidth,
		popupHeight,
		popupTemplateId,
		popupCornerRadius,
		popupImageRadius,
		popupDropShadowDistance,
		layoutMinNonMapWidth,
		layoutMinNonMapHeight,
		tooltipStyleId,
		readyMapPackageId,
		readyMapGroupId,
		routesXml,
		showRouteList,
		isGallery,
		gallerySpacingRow,
		gallerySpacingColumn,
		galleryAutoSpacingRow,
		galleryAutoSpacingColumn,
		galleryMarginTop,
		galleryMarginLeft,
		galleryCellAlignV,
		galleryCellAlignH,
		galleryUseFixedRowHeight,
		galleryUseFixedColumnWidth,
		galleryBackgroundType,
		markersZoom
	}

	public string GetTagValue(int tagId)
	{
		Tag tag = (Tag)tagId;

		switch (tag)
		{
			case Tag.id:
				return Id.ToString();

			case Tag.isDataSheet:
				return IsDataSheet.ToString();

			case Tag.pageId:
				return PageId;

			case Tag.pageNumber:
				return PageNumber.ToString();

			case Tag.name:
				return Name;

			case Tag.title:
				return Title;

			case Tag.firstHotspotId:
				return FirstTourViewId.ToString();

			case Tag.menuPosition:
				return ((int)MenuPosition).ToString();

			case Tag.excludeFromNavigation:
				return ExcludeFromNavigation.ToString();

			case Tag.showHotspotList:
				return ShowSlideList.ToString();

			case Tag.hotspotListInstructions:
				return SlideListInstructions;

			case Tag.showHotspotTitle:
				return ShowSlideTitle.ToString();

			case Tag.showInstructions:
				return ShowInstructions.ToString();

			case Tag.instructionsText:
				return InstructionsText;

			case Tag.instructionsWidth:
				return InstructionsWidth.ToString();

			case Tag.instructionsBackgroundColor:
				return InstructionsBgColor;

			case Tag.instructionsColor:
				return InstructionsColor;

			case Tag.instructionsTitle:
				return InstructionsTitle;

			case Tag.runSlideShow:
				return RunSlideShow.ToString();

			case Tag.slideShowInterval:
				return SlideShowInterval.ToString();

			case Tag.mapImageId:
				return mapImageId.ToString();

			case Tag.mapImageAltId:
				return mapImageAltId.ToString();

			case Tag.showHotspotNamesInMenu:
				return ShowSlideNamesInMenu.ToString();

			case Tag.mapPlaceholderColor:
				return MapPlaceholderColor;

			case Tag.mapCanZoom:
				return MapCanZoom.ToString();

			case Tag.mapZoomX:
				return MapZoomX.ToString();

			case Tag.mapZoomY:
				return  MapZoomY.ToString();

			case Tag.mapZoomLevel:
				return (mapCanZoom ? MapZoomLevel : 100).ToString();

			case Tag.mapInsetLocation:
				return ((int)MapInsetLocation).ToString();

			case Tag.mapInsetSize:
				return MapInsetSize.ToString();

			case Tag.mapInsetColor:
				return MapInsetColor;
				
			case Tag.mapZoomLimit:
				return (mapCanZoom ? mapZoomLimit : 1).ToString();

			case Tag.markerZoomLimit:
				return (mapCanZoom ? MarkerZoomLimit : 1).ToString();

			case Tag.showPanZoomControls:
				return ShowPanZoomControls.ToString();

			case Tag.panZoomControlColorOff:
				return PanZoomControlColorOff;

			case Tag.panZoomControlColorOn:
				return PanZoomControlColorOn;

			case Tag.mouseOverDelay:
				return MapImageSharpening.ToString();

			case Tag.saveMapStateChanges:
				return SaveMapStateChanges.ToString();

			case Tag.selectedMarkerBlink:
				return SelectedMarkerBlink.ToString();

			case Tag.visitedMarkerAlpha:
				return VisitedMarkerAlpha.ToString();

			case Tag.popupsEnabled:
				return SlidesPopup.ToString();

			case Tag.layoutAreaSplitterH:
				return LayoutAreaSlideLayout.Splitters.H.ToString();

			case Tag.layoutAreaSplitterV:
				return LayoutAreaSlideLayout.Splitters.V.ToString();

			case Tag.layoutAreaSplitterLockedH:
				return LayoutAreaSlideLayout.Splitters.LockedH.ToString();

			case Tag.layoutAreaSplitterLockedV:
				return LayoutAreaSlideLayout.Splitters.LockedV.ToString();

			case Tag.layoutAreaMarginTop:
				return LayoutAreaSlideLayout.Margin.Top.ToString();

			case Tag.layoutAreaMarginRight:
				return LayoutAreaSlideLayout.Margin.Right.ToString();

			case Tag.layoutAreaMarginBottom:
				return LayoutAreaSlideLayout.Margin.Bottom.ToString();

			case Tag.layoutAreaMarginLeft:
				return LayoutAreaSlideLayout.Margin.Left.ToString();

			case Tag.layoutAreaSpacingH:
				return LayoutAreaSlideLayout.Spacing.H.ToString();

			case Tag.layoutAreaSpacingV:
				return LayoutAreaSlideLayout.Spacing.V.ToString();

			case Tag.layoutAreaTemplateId:
				return ((int)LayoutAreaSlideLayout.Pattern).ToString();

			case Tag.popupBestSideSequence:
				return PopupOptions.BestSideSequence.ToString();

			case Tag.popupMinWidth:
				return PopupOptions.MinSize.Width.ToString();

			case Tag.popupMinHeight:
				return PopupOptions.MinSize.Height.ToString();

			case Tag.popupTextOnlyWidth:
				return PopupOptions.TextOnlyWidth.ToString();

			case Tag.popupBorderWidth:
				return PopupOptions.BorderWidth.ToString();
				
			case Tag.popupCornerRadius:
				return popupOptions.PopupCornerRadius.ToString();
				
			case Tag.popupImageRadius:
				return popupOptions.ImageCornerRadius.ToString();

			case Tag.popupDropShadowDistance:
				return popupOptions.DropShadowDistance.ToString();

			case Tag.popupBorderColor:
				return PopupOptions.BorderColor;

			case Tag.popupBackgroundColor:
				return PopupOptions.BackgroundColor;

			case Tag.popupTextColor:
				return PopupOptions.TextColor;

			case Tag.popupTitleTextColor:
				return PopupOptions.TitleTextColor;

			case Tag.popupLocation:
				return ((int)PopupOptions.Location).ToString();

			case Tag.popupLocationX:
				return PopupOptions.LocationPoint.X.ToString();

			case Tag.popupLocationY:
				return PopupOptions.LocationPoint.Y.ToString();

			case Tag.popupArrowType:
				return ((int)PopupOptions.ArrowType).ToString();

			case Tag.popupShowTooltipWhenNoContent:
				return PopupOptions.ShowTooltipWhenNoContent.ToString();

			case Tag.popupDelayType:
				return ((int)PopupOptions.DelayType).ToString();

			case Tag.popupDelay:
				return PopupOptions.Delay.ToString();

			case Tag.popupPinOnClick:
				return PopupOptions.PinOnClick.ToString();

			case Tag.popupPinMessage:
				return PopupOptions.PinMessage;

			case Tag.popupUseTourStyleColors:
				return PopupOptions.UseColorSchemeColors.ToString();

			case Tag.popupMarkerOffset:
				return PopupOptions.MarkerOffset.ToString();

			case Tag.popupSplitterH:
				return PopupSlideLayout.Splitters.H.ToString();

			case Tag.popupSplitterV:
				return PopupSlideLayout.Splitters.V.ToString();

			case Tag.popupSplitterLockedH:
				return PopupSlideLayout.Splitters.LockedH.ToString();

			case Tag.popupSplitterLockedV:
				return PopupSlideLayout.Splitters.LockedV.ToString();

			case Tag.popupMarginTop:
				return PopupSlideLayout.Margin.Top.ToString();

			case Tag.popupMarginRight:
				return PopupSlideLayout.Margin.Right.ToString();

			case Tag.popupMarginBottom:
				return PopupSlideLayout.Margin.Bottom.ToString();

			case Tag.popupMarginLeft:
				return PopupSlideLayout.Margin.Left.ToString();

			case Tag.popupSpacingH:
				return PopupSlideLayout.Spacing.H.ToString();

			case Tag.popupSpacingV:
				return PopupSlideLayout.Spacing.V.ToString();

			case Tag.popupWidth:
				return PopupSlideLayout.OuterSize.Width.ToString();

			case Tag.popupHeight:
				return PopupSlideLayout.OuterSize.Height.ToString();

			case Tag.popupTemplateId:
				return ((int)PopupSlideLayout.Pattern).ToString();

			case Tag.layoutMinNonMapWidth:
				return LayoutMinNonMapWidth.ToString();

			case Tag.layoutMinNonMapHeight:
				return LayoutMinNonMapHeight.ToString();

			case Tag.tooltipStyleId:
				return tooltipStyleId.ToString();

			case Tag.readyMapPackageId:
				return mapImage.ReadyMapPackageId.ToString();

			case Tag.readyMapGroupId:
				return ReadyMapGroupId.ToString();

			case Tag.routesXml:
				return RoutesXml;

			case Tag.showRouteList:
				return ShowRouteList.ToString();

			case Tag.isGallery:
				return galleryOptions.IsGallery.ToString();

			case Tag.gallerySpacingRow:
				return galleryOptions.SpacingRow.ToString();

			case Tag.gallerySpacingColumn:
				return galleryOptions.SpacingColumn.ToString();

			case Tag.galleryAutoSpacingRow:
				return galleryOptions.AutoSpacingRow.ToString();

			case Tag.galleryAutoSpacingColumn:
				return galleryOptions.AutoSpacingColumn.ToString();

			case Tag.galleryMarginTop:
				return galleryOptions.MarginTop.ToString();

			case Tag.galleryMarginLeft:
				return galleryOptions.MarginLeft.ToString();

			case Tag.galleryCellAlignH:
				return ((int)galleryOptions.CellAlignH).ToString();

			case Tag.galleryCellAlignV:
				return ((int)galleryOptions.CellAlignV).ToString();

			case Tag.galleryUseFixedRowHeight:
				return galleryOptions.UseFixedRowHeight.ToString();

			case Tag.galleryUseFixedColumnWidth:
				return galleryOptions.UseFixedColumnWidth.ToString();

			case Tag.galleryBackgroundType:
				return ((int)galleryOptions.BackgroundType).ToString();

			case Tag.markersZoom:
				return markersZoom.ToString();

			default:
				Debug.Fail("Unsupported TourPage XML tag requested " + tag);
				return "???";
		}
	}

	#region ===== Properties ========================================================

	public string MapPlaceholderColor { get; set; }
	public int ReadyMapGroupId { get; set; }

	public SlideLayout ActiveSlideLayout
	{
		get { return slidesPopup ? popupSlideLayout : layoutAreaSlideLayout; }
	}

	public string ActiveFileNameDisabled
	{
		get { return id.ToString(); }
	}

	public string ActiveFileNameEnabled(TourState oldState)
	{
		// Prior to version 1.57 (TourState.ExpiredPre_1_57) we used to rename the page1.htm file,
		// but then we discovered that since embedded tours don't use the htm files, they could
		// still work after expired. So now we rename the js file instead.
		return string.Format("page{0}.{1}", pageNumber, oldState == TourState.ExpiredPre_1_57 ? "htm" : "js");
	}

	public DateTime DateCreated
	{
		get { return dateCreated; }
	}

	public string DateCreatedShort
	{
		get { return Utility.DateShort(dateCreated); }
	}

	public DateTime DateModified
	{
		get { return dateModified; }
	}

	public bool ExcludeFromNavigation
	{
		get { return excludeFromNavigation; }
		set
		{
			if (excludeFromNavigation != value)
			{
				FlagAsChanged(ChangeFlags.Menu);
				excludeFromNavigation = value;
			}
		}
	}

	public string NameForPageCssFileV3
	{
		get { return string.Format(TourBuilder.PatternForPageCssFileV3, pageNumber); }
	}

	public string NameForPageHtmlPublishedFile
	{
		get { return string.Format(TourBuilder.PatternForPageHtmlPublishedFile, pageNumber); }
	}

	public string NameForPageHtmlPublishedFileV3
	{
		get { return string.Format(TourBuilder.PatternForPageHtmlPublishedFileV3, pageNumber); }
	}

	public string NameForPageHtmlPreviewFile
	{
		get { return string.Format(TourBuilder.PatternForPageHtmlPreviewFile, Id, tour.BuildId); }
	}

	public string NameForPageHtmlPreviewFileV3
	{
		get { return string.Format(TourBuilder.PatternForPageHtmlPreviewFileV3, Id); }
	}

	public string NameForPageHtmlUnbrandedPreviewFile
	{
		get { return string.Format(TourBuilder.PatternForPageHtmlUnbrandedPreviewFile, Id, tour.BuildId); }
	}

	public string NameForPageHtmlUnbrandedPreviewFileV3
	{
		get { return string.Format(TourBuilder.PatternForPageHtmlUnbrandedPreviewFileV3, Id, tour.BuildId); }
	}

	public string NameForPageHtmlUnbrandedPublishedFile
	{
		get { return string.Format(TourBuilder.PatternForPageHtmlUnbrandedPublishedFile, PageNumber, tour.BuildId); }
	}

	public string NameForPageHtmlUnbrandedPublishedFileV3
	{
		get { return string.Format(TourBuilder.PatternForPageHtmlUnbrandedPublishedFileV3, PageNumber); }
	}

	public string NameForPageJsFile
	{
		get { return string.Format(TourBuilder.PatternForPageJsFile, pageNumber, tour.BuildId); }
	}

	public string NameForPageJsFileV3
	{
		get { return string.Format(TourBuilder.PatternForPageJsFileV3, pageNumber); }
	}

	public TourView FirstTourView
	{
		get
		{
			int id = FirstTourViewId;
			if (id == 0)
			{
				// This can only happen if the tour has no hotspots.
				return null;
			}

            return GetTourView(id);
		}
	}

	public int FirstTourViewId
	{
		get	{ return firstTourViewId; }
		set { firstTourViewId = value; }
	}

	public Size GallerySize
	{
		get { return gallerySize; }
		set { gallerySize = value; }
	}

	public bool HasBeenBuilt
	{
		get { return buildId != 0; }
	}

	public bool HasNeverHadMap
	{
		// This flag is set true when a new TourPage is first created and set false
		// when a map has been loaded. It is not preserved in the DB and therefore
		// is false when the page is loaded from the DB.
		get { return hasNeverHadMap; }
		set { hasNeverHadMap = value; }
	}

	public bool HtmlChanged
	{
		get
		{
			bool htmlChanged = Changed(
				ChangeFlags.SlideLayout |
				ChangeFlags.SlideList |
				ChangeFlags.SlideTitle |
				ChangeFlags.MapImage |
				ChangeFlags.MapAreaSize |
				ChangeFlags.ImageAreaSize |
				ChangeFlags.TextAreaSize |
				ChangeFlags.Name |
				ChangeFlags.Menu |
				ChangeFlags.Title |
				ChangeFlags.TourViews |
				ChangeFlags.FirstTourView |
				ChangeFlags.TourHtml |
				ChangeFlags.BannerImage |
				ChangeFlags.BannerOptions |
				ChangeFlags.PageId |
				ChangeFlags.TooltipStyle |
				ChangeFlags.Routes
			);

			return htmlChanged;
		}
	}

	public int Id
	{
		get { return id; }
	}

	public bool ImageAreaSizeChanged
	{
		get { return Changed(ChangeFlags.ImageAreaSize); }
	}

	public bool ImportingArchive
	{
		get { return importingArchive; }
		set { importingArchive = value; }
	}

	public bool ImportingMarkers
	{
		get { return importingMarkers; }
		set { importingMarkers = value; }
	}

	public string InfoPageNotice
	{
		get { return "<span style='color:#808080;font-weight:normal;'>&nbsp;&nbsp;(Info page)</span>"; }
	}

	public string InstructionsBgColor
	{
		get { return instructionsBgColor; }
		set
		{
			FlagInstructionsAsChangedIf(instructionsBgColor != value);
			instructionsBgColor = value;
		}
	}

	public string InstructionsColor
	{
		get { return instructionsColor; }
		set
		{
			FlagInstructionsAsChangedIf(instructionsColor != value);
			instructionsColor = value;
		}
	}

	public string InstructionsFont
	{
		get { return instructionsFont; }
		set
		{
			FlagInstructionsAsChangedIf(instructionsFont != value);
			instructionsFont = value;
		}
	}

	public int InstructionsFontSize
	{
		get { return instructionsFontSize; }
		set
		{
			FlagInstructionsAsChangedIf(instructionsFontSize != value);
			instructionsFontSize = value;
		}
	}

	public string InstructionsText
	{
		get { return instructionsText; }
		set
		{
			FlagInstructionsAsChangedIf(instructionsText != value);
			instructionsText = value;
		}
	}

	public string InstructionsTitle
	{
		get { return instructionsTitle; }
		set
		{
			FlagInstructionsAsChangedIf(instructionsTitle != value);
			instructionsTitle = value;
		}
	}

	public int InstructionsWidth
	{
		get { return instructionsWidth; }
		set
		{
			FlagInstructionsAsChangedIf(instructionsWidth != value);
			instructionsWidth = value;
		}
	}

	public bool IsDataSheet
	{
		get { return mapImageId == 0; }
	}

	public bool IsGallery
	{
		get { return galleryOptions.IsGallery; }
		set { galleryOptions.IsGallery = true; }
	}

	public bool IsNewTourPage
	{
		get { return id == 0; }
	}

	public SlideLayout LayoutAreaSlideLayout
	{
		get { return layoutAreaSlideLayout; }
		set { layoutAreaSlideLayout = value; }
	}

	public LayoutManager LayoutManager
	{
		get { return layoutManager; }
	}

	public int LayoutMinNonMapWidth
	{
		get { return layoutMinNonMapWidth; }
		set { layoutMinNonMapWidth = value; }
	}

	public int LayoutMinNonMapHeight
	{
		get { return layoutMinNonMapHeight; }
		set { layoutMinNonMapHeight = value; }
	}

	public Size MapAreaSize
	{
		get { return slidesPopup ? layoutAreaSlideLayout.InnerSize : layoutAreaSlideLayout.MapArea.Size; }
	}

	public bool MapAreaSizeChanged
	{
		get { return Changed(ChangeFlags.MapAreaSize); }
	}

	public bool MapCanZoom
	{
		get
        {
            if (IsGallery)
                return false;

            // All V4 maps are zoomable when they are larger than their container.
            return tour.V3CompatibilityEnabled ? mapCanZoom : true; 
        }
		set
		{
			if (mapCanZoom != value)
			{
				FlagMapAsChangedIf(true);
				
				// Changing the map zoom setting has the same effect as if a larger
				// or smaller map image had been uploaded.
				FlagAsChanged(ChangeFlags.MapImage);
				
				mapCanZoom = value;
			}
		}
	}

	public bool MapChanged
	{
		get
		{
			bool mapChanged = Changed(
				ChangeFlags.MapImage |
				ChangeFlags.MapSize |
				ChangeFlags.MapAreaSize |
				ChangeFlags.TourViews |
				ChangeFlags.FirstTourView |
				ChangeFlags.Instructions |
				ChangeFlags.SlideShow |
				ChangeFlags.Map
			);

			return mapChanged;
		}
	}

	public MapImage MapImage
	{
		get { return mapImage; }
	}

	public bool MapImageChanged
	{
		get { return Changed(ChangeFlags.MapImage); }
	}

	public int MapInsetLocation
	{
		get { return mapInsetLocation; }
		set
		{
			FlagMapAsChangedIf(mapInsetLocation != value);
			mapInsetLocation = value;
		}
	}

	public string MapInsetColor
	{
		get { return mapInsetColor; }
		set
		{
			FlagMapAsChangedIf(mapInsetColor != value);
			mapInsetColor = value;
		}
	}

	public int MapInsetSize
	{
		get { return mapInsetSize; }
		set
		{
			FlagMapAsChangedIf(mapInsetSize != value);
			mapInsetSize = value;

			// Flag that the map image changed in order to force the map inset image to get regenerated at the new size.
			SetMapImageChanged();
		}
	}

    // V4 uses the no longer used mapZoomLimit value to store both the x and y values for the map focus.
    // The x value is stored in the first two bytes and the y value is stored in the second two bytes. MapZoomLimit
    // was repurposed this way in V4 to avoid having to change the database schema to add columns for x and y.
    public int MapFocus
	{
		get { return markerZoomLimit; }
		set
		{
			FlagMapAsChangedIf(markerZoomLimit != value);
            markerZoomLimit = value;
		}
	}

    public short MapFocusX
    {
        // Return the first word (two byte)s of MapFocus.
        get { return (short)(MapFocus >> 16); }
    }

    public short MapFocusY
    {
        // Return the second word (two bytes) of MapFocus.
        get { return (short)(MapFocus & 0xffff); }
    }

    // V4 uses the no longer used mapZoomLimit value to store the locked map zoom percent.
    public int MapFocusPercent
    {
        get { return mapZoomLimit; }
        set
        {
            FlagMapAsChangedIf(mapZoomLimit != value);
            mapZoomLimit = value;
        }
    }

	public int MapZoomLimit
	{
		get { return mapZoomLimit; }
		set
		{
			FlagMapAsChangedIf(mapZoomLimit != value);
			mapZoomLimit = value;
		}
	}

    public int MarkerZoomLimit
    {
        get { return markerZoomLimit; }
        set
        {
            FlagMapAsChangedIf(markerZoomLimit != value);
            markerZoomLimit = value;
        }
    }

    public int MapZoomX
	{
		get { return mapZoomX; }
		set
		{
			FlagMapAsChangedIf(mapZoomX != value);
			mapZoomX = value;
		}
	}

	public int MapZoomY
	{
		get { return mapZoomY; }
		set
		{
			FlagMapAsChangedIf(mapZoomY != value);
			mapZoomY = value;
		}
	}

	public double MapZoomLevel
	{
		get { return mapZoomLevel; }
		set
		{
			FlagMapAsChangedIf(mapZoomLevel != value);
			mapZoomLevel = value;
		}
	}

	public bool MarkersZoom
	{
		get { return markersZoom; }
		set { markersZoom = value; }
	}

	public int MarkersNotPlacedOnMapCount
	{
		get
		{
			int count = 0;
			foreach (TourView tourView in TourViews)
			{
				if (!tourView.MarkerHasBeenPlacedOnMap)
					count++;
			}
			return count;
		}
	}

	public int MarkersOnMap
	{
		get { return TourViews.Count - MarkersNotPlacedOnMapCount; }
	}

	public int MenuPosition
	{
		get
		{
			return menuPosition;
		}
		set
		{
			if (menuPosition != value)
			{
				FlagAsChanged(ChangeFlags.Menu);
				menuPosition = value;
				tour.SetMenuItemChanged();
			}
		}
	}

    // V4 uses the old V3 mouseOverDelay property as the map image sharpening setting. The delay option
    // was never implemented in the JavaScript runtime and is no longer a Tour Builder option.
    public int MapImageSharpening
	{
		get { return mouseOverDelay; }
		set
		{
			if (mouseOverDelay != value)
			{
				FlagAsChanged(ChangeFlags.Map);
				mouseOverDelay = value;
			}
		}
	}

	public string Name
	{
		get { return name; }
		set
		{
			if (name != value)
			{
				FlagAsChanged(ChangeFlags.Name);
				name = value;
				tour.SetMenuItemChanged();
			}
		}
	}

	public string PageId
	{
		get
		{
			if (pageId == string.Empty)
				return "page" + pageNumber.ToString();
			else
				return pageId;
		}
		set
		{
			string id = value.Trim();
			if (pageId != id)
			{
				FlagAsChanged(ChangeFlags.PageId);
				pageId = id;
			}
		}
	}

	public int PageNumber
	{
		get { return pageNumber; }
		set { pageNumber = value; }
	}

	public string PanZoomControlColorOff
	{
		get { return panZoomControlColorOff; }
		set
		{
			FlagMapAsChangedIf(panZoomControlColorOff != value);
			panZoomControlColorOff = value;
		}
	}

	public string PanZoomControlColorOn
	{
		get { return panZoomControlColorOn; }
		set
		{
			FlagMapAsChangedIf(panZoomControlColorOn != value);
			panZoomControlColorOn = value;
		}
	}

	public PopupOptions PopupOptions
	{
		get { return popupOptions; }
		set { popupOptions = value; }
	}

	public SlideLayout PopupSlideLayout
	{
		get { return popupSlideLayout; }
		set { popupSlideLayout = value; }
	}

    public string RoutesXml
	{
		get { return routesXml; }
		set
		{
			FlagAsChangedIf(routesXml != value, ChangeFlags.Routes);
			routesXml = value;
		}
	}

	public bool RunSlideShow
	{
		get { return runSlideShow; }
		set
		{
			FlagAsChangedIf(runSlideShow != value, ChangeFlags.SlideShow);
			runSlideShow = value;
		}
	}

	public bool SaveMapStateChanges
	{
		get { return saveMapStateChanges; }
		set
		{
			FlagMapAsChangedIf(saveMapStateChanges != value);
			saveMapStateChanges = value;
		}
	}

	public int SelectedMarkerBlink
	{
		get { return selectedMarkerBlink; }
		set
		{
			FlagMapAsChangedIf(selectedMarkerBlink != value);
			selectedMarkerBlink = value;
		}
	}

	public Size ScaledMapSize
	{
		get
		{
			Size size = mapImage.HasFile && !IsGallery ? mapImage.Size : MapAreaSize;
			return Utility.ScaledImageSize(size, MapAreaSize);
		}
	}

	public bool ShowAllPagesInDirectory
	{
		get { return true; }
	}

	public bool ShowInstructions
	{
		get { return showInstructions; }
		set
		{
			FlagInstructionsAsChangedIf(showInstructions != value);
			showInstructions = value;
		}
	}

	public bool ShowLayoutAreaInLayoutEditor
	{
		get { return showLayoutAreaInLayoutEditor; }
		set { showLayoutAreaInLayoutEditor = value; }
	}

	public bool ShowPanZoomControls
	{
		get
        {
            // Hide the zoom controls for a non-zoom V3 map in case the user had switched
            // to V4, turned the controls on, and then switched back to V3.
            if (IsGallery || tour.V3CompatibilityEnabled && !MapCanZoom)
                return false;
            
            return showPanZoomControls;
        }
		set
		{
			FlagMapAsChangedIf(showPanZoomControls != value);
			showPanZoomControls = value;
		}
	}

	public bool ShowRouteList
	{
		get { return showRouteList; }
		set
		{
			FlagAsChangedIf(showRouteList != value, ChangeFlags.Routes);
			showRouteList = value;
		}
	}

	public bool ShowSlideList
	{
		get { return showSlideList; }
		set
		{
			FlagAsChangedIf(showSlideList != value, ChangeFlags.SlideList);
			showSlideList = value;
		}
	}

	public bool ShowSlideTitle
	{
		get { return showSlideTitle; }
		set
		{
			FlagAsChangedIf(showSlideTitle != value, ChangeFlags.SlideTitle);
			showSlideTitle = value;
		}
	}

	public bool ShowSlideNamesInMenu
	{
		get { return showSlideNamesInMenu; }
		set
		{
			FlagAsChangedIf(showSlideNamesInMenu != value, ChangeFlags.Menu);
			showSlideNamesInMenu = value;
		}
	}

	public string SlideListInstructions
	{
		get
		{
			return slideListInstructions.Length == 0 ? defaultSlideListInstructions : slideListInstructions;
		}
		set
		{
			FlagAsChangedIf(slideListInstructions != value, ChangeFlags.Title);
			slideListInstructions = value;
		}
	}

	public int SlideShowInterval
	{
		get { return slideShowInterval; }
		set
		{
			FlagAsChangedIf(slideShowInterval != value, ChangeFlags.SlideShow);
			slideShowInterval = value;
		}
	}

	public bool SlidesPopup
	{
		get { return slidesPopup; }
		set { slidesPopup = value; }
	}

	public Byte[] ThumbnailBytes
	{
		get
		{
			if (thumbnailBytes == null)
			{
				object bytes = MapsAliveDatabase.ReadScalar("sp_TourPage_GetThumbnail", "@TourPageId", id);
				if (bytes is DBNull)
					return null;
				else
				{
					thumbnailBytes = (Byte[])bytes;
					if (thumbnailBytes.Length == 0)
						thumbnailBytes = null;
					return thumbnailBytes;
				}
			}
			return thumbnailBytes;
		}
		set
		{
			if (thumbnailBytes == null)
			{
				thumbnailBytes = value;
				MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_UpdateThumbnail",
					"@TourPageId", id,
					"@Thumbnail", thumbnailBytes
				);
			}
		}
	}

	public string Title
	{
		get { return title; }
		set
		{
			FlagAsChangedIf(title != value, ChangeFlags.Title);
			title = value;
		}
	}

	public string TitleOrName
	{
		get { return title.Length > 0 ? title : tour.Name; }
	}

	public TooltipStyle TooltipStyle
	{
		get
		{
			if (_tooltipStyle == null)
				_tooltipStyle = Account.GetCachedTooltipStyle(tooltipStyleId);
			return _tooltipStyle;
		}
		set
		{
			if (_tooltipStyle != value)
			{
				FlagAsChanged(ChangeFlags.TooltipStyle);
				_tooltipStyle = value;
				tooltipStyleId = _tooltipStyle.Id;
			}
		}
	}

	public int TooltipStyleId
	{
		get { return tooltipStyleId; }
		set { tooltipStyleId = value; }
	}

	public Tour Tour
	{
		get { return tour; }
	}

	public ArrayList TourViews
	{
		get
		{
			// We load tour views on demand so that we don't pay the price to fetch tour view
			// information from the database unless a request has actually been made for it.
			if (_tourViews == null)
				LoadViews();
			return _tourViews;
		}
	}

	public ArrayList TourViewsBySequence
	{
		get
		{
			// Create a sequenced version of the TourViews array. This property is called infrequently enough
			// that it's simpler and safer to always create and sort the array than to do the bookkeeping to
			// keep two arrays, one sequenced and one non-sequenced, in sync all the time. This way adds and
			// deletes are only performed on the non-sequenced array, and when the sequence changes, we don't
			// even have to update the sequenced array.
		
			ArrayList _tourViewsBySequence = new ArrayList();
			foreach (TourView tourView in TourViews)
			{
				_tourViewsBySequence.Add(tourView);
			}
			
			_tourViewsBySequence.Sort(new TourViewSequenceNumberComparer());

			return _tourViewsBySequence;
		}
	}

	public bool UsesLiveData
	{
		get
		{
			foreach (TourView tourView in TourViews)
			{
				if (tourView.UsesLiveData)
					return true;
			}
			return false;
		}
	}

	public int VisitedMarkerAlpha
	{
		get { return visitedMarkerAlpha; }
		set
		{
			FlagMapAsChangedIf(visitedMarkerAlpha != value);
			visitedMarkerAlpha = value;
		}
	}

	public GalleryOptions GalleryOptions
	{
		get { return galleryOptions; }
	}
	#endregion

	#region ===== Public ============================================================

	public void AcceptLayoutChanges()
	{
		savedLayout = new SavedLayout();
		
		savedLayout.LayoutAreaSlideLayout = new SlideLayout(layoutAreaSlideLayout);
		savedLayout.MapZoomLevel = mapZoomLevel;
		savedLayout.MapZoomX = mapZoomX;
		savedLayout.MapZoomY = mapZoomY;
		savedLayout.MaxTourSize = tour.MaxTourSize;
		savedLayout.PopupSlideLayout = new SlideLayout(popupSlideLayout);
		savedLayout.SlidesPopup = slidesPopup;
		savedLayout.TourBodyBackgroundColor = tour.BodyBackgroundColor;
		savedLayout.TourBodyMargin = tour.BodyMargin;
		savedLayout.TourHasFooterStripe = tour.HasFooterStripe;
		savedLayout.TourHasHeaderStripe = tour.HasHeaderStripe;
		savedLayout.TourHasTitle = tour.HasTitle;
		savedLayout.TourHeightType = tour.HeightType;
		savedLayout.TourLeftAlignedInBrowser = tour.LeftAlignedInBrowser;
		savedLayout.TourMenuLocationId = tour.MenuLocationId;
		savedLayout.TourMenuScrolls = tour.MenuScrolls;
		savedLayout.TourMenuStyleId = tour.MenuStyleId;
		savedLayout.TourMenuWidth = tour.MenuWidth;
		savedLayout.TourSize = tour.TourSize;
		savedLayout.ColorSchemeId = tour.ColorScheme.Id;
		savedLayout.TourWidthType = tour.WidthType;
	}

	public string GetHotspotCoordinates(int tourViewId)
	{
		TourView hotspot = GetTourView(tourViewId);
		if (hotspot == null || hotspot.MarkerIsRoute)
			return null;
		else
//			return string.Format("{0},{1}", hotspot.MarkerX, hotspot.MarkerY);
			return tourViewId.ToString();
	}

	public bool LayoutChanged
	{
		get
		{
			bool same = true;

			if (savedLayout != null)
			{
				same =
					savedLayout.LayoutAreaSlideLayout == layoutAreaSlideLayout &&
					savedLayout.MaxTourSize == tour.MaxTourSize &&
					savedLayout.PopupSlideLayout == popupSlideLayout &&
					savedLayout.SlidesPopup == slidesPopup &&
					savedLayout.TourBodyBackgroundColor == tour.BodyBackgroundColor &&
					savedLayout.TourBodyMargin == tour.BodyMargin &&
					savedLayout.TourHasFooterStripe == tour.HasFooterStripe &&
					savedLayout.TourHasHeaderStripe == tour.HasHeaderStripe &&
					savedLayout.TourHasTitle == tour.HasTitle &&
					savedLayout.TourHeightType == tour.HeightType &&
					savedLayout.TourLeftAlignedInBrowser == tour.LeftAlignedInBrowser &&
					savedLayout.TourMenuLocationId == tour.MenuLocationId &&
					savedLayout.TourMenuScrolls == tour.MenuScrolls &&
					savedLayout.TourMenuStyleId == tour.MenuStyleId &&
					savedLayout.TourMenuWidth == tour.MenuWidth &&
					savedLayout.TourSize == tour.TourSize &&
					savedLayout.ColorSchemeId == tour.ColorScheme.Id &&
					savedLayout.TourWidthType == tour.WidthType;
			}

			return !same;
		}
	}

	public void RestoreLayout()
	{
		if (savedLayout != null)
		{
			layoutAreaSlideLayout = savedLayout.LayoutAreaSlideLayout;
			popupSlideLayout = savedLayout.PopupSlideLayout;
			slidesPopup = savedLayout.SlidesPopup;
			mapZoomLevel = savedLayout.MapZoomLevel;
			mapZoomX = savedLayout.MapZoomX;
			mapZoomY = savedLayout.MapZoomY;
			
			UpdateDatabase();

			tour.SetTourAndLayoutAreaSizes(savedLayout.TourSize, layoutAreaSlideLayout.OuterSize);

			tour.BodyBackgroundColor = savedLayout.TourBodyBackgroundColor;
			tour.BodyMargin = savedLayout.TourBodyMargin;
			tour.HasFooterStripe = savedLayout.TourHasFooterStripe;
			tour.HasHeaderStripe = savedLayout.TourHasHeaderStripe;
			tour.HasTitle = savedLayout.TourHasTitle;
			tour.HeightType = savedLayout.TourHeightType;
			tour.LeftAlignedInBrowser = savedLayout.TourLeftAlignedInBrowser;
			tour.MaxTourSize = savedLayout.MaxTourSize;
			tour.MenuLocationId = savedLayout.TourMenuLocationId;
			tour.MenuScrolls = savedLayout.TourMenuScrolls;
			tour.MenuStyleId = savedLayout.TourMenuStyleId;
			tour.MenuWidth = savedLayout.TourMenuWidth;
			tour.ColorScheme = Account.GetCachedColorScheme(savedLayout.ColorSchemeId);
			tour.WidthType = savedLayout.TourWidthType;
			
			tour.UpdateDatabase();

			AcceptLayoutChanges();

			// Create a new layout manager that uses the restored layouts. If we don't
			// do this, the manager will still be using the old SlideLayout object and
			// will be out of sync with the ones in this tour page. Note that we pass
			// the layouts by reference so that the layout manager operates on the same
			// object as this tour page.
			layoutManager = new LayoutManager(this, ref layoutAreaSlideLayout, ref popupSlideLayout);

			// Force a rebuild the next time the tour is previewed so that the restored
			// layout will go back into effect.
			tour.RequireRebuild();
		}
	}

	public void AddTourView(TourView tourView)
	{
		AddTourView(tourView, false);
	}

	public void AddTourView(TourView tourView, bool importingSlides)
	{
		TourViews.Add(tourView);
		tourView.InsertTourViewIntoDatabase();

		if (!importingSlides)
		{
			TourViewChanged();
			tour.RebuildTourTreeXml();
		}

		if (firstTourViewId == 0)
		{
			SetFirstTourView(tourView.Id);
			UpdateDatabaseFirstTourView();
		}

		// Tell the account that a hotspot was added so that it can check to see if the user
		// is over their limit. We don't make the call when importing an archive because
		// performance profiling showed us that this causes an expensive database lookup.
		// Instead we wait until the archive is imported to let the account determine hotspot status.
		if (!importingArchive)
			MapsAliveState.Account.HotspotAdded(tour);
	}

	public void Built()
	{
		// We know that in order to build this page, its views had to be loaded.
		// Because we don't want to keep all views for all pages in memory all the
		// time, we unload views unless this is the currently selected page.
		if (this != tour.SelectedTourPage)
			UnloadTourViews();
		
		// Determine if anything changed.  If not, we don't need to update the database.
		if (changed == 0 && !mapImage.VersionChanged)
			return;

		buildId = tour.BuildId;

		// Clear this map page's change flags.
		changed = 0;

		// Mark this map page's image as built.
		mapImage.Built();
		
		// Update the database to clear the change flags and/or update the map image version.
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_Built",
			"@BuildId", buildId,
			"@TourPageId", id,
			"@ImageId", mapImage.Id,
			"@ThemeId", tour.ThemeId,
			"@MapImageVersionBuilt", mapImage.VersionBuilt,
			"@MapLibraryVersion", mapLibraryVersion
		);
	}

	public double CalculateMapAreaScale()
	{
		Size mapImageSize = MapImage.HasFile ? MapImage.Size : MapAreaSize;
		double mapAreaScale;

		// Determine the scale factor needed to make the map fit within the map area fully zoomed out.
		// Normally it's less than 1. We calculate the factor based on the shorter side of the map area
		// so that the entire map will be visible.
		if (MapAreaSize.Width < MapAreaSize.Height)
		{
			mapAreaScale = ((double)ScaledMapSize.Width / (double)mapImageSize.Width);
		}
		else
		{
			mapAreaScale = ((double)ScaledMapSize.Height / (double)mapImageSize.Height);
		}

		return mapAreaScale;
	}

    public void ChangeId(int newId)
	{
		id = newId;
	}

	public void ChangePageNumber(int newNumber)
	{
		pageNumber = newNumber;
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_UpdatePageNumber", "@TourPageId", id, "@PageNumber", pageNumber);
	}

	public int CountNonExclusiveMarkers()
	{
		int count = 0;
		foreach (TourView tourView in TourViews)
		{
			if (tourView.TourPage.IsDataSheet || tourView.MarkerIsRoute)
				continue;

			Marker marker = Account.GetCachedMarker(tourView.MarkerId);
			if (!marker.IsExclusive)
				count++;
		}
		return count;
	}

	public void Delete()
	{
		// Remove this page's hotspot images from the database and from the preview folder.
		// Also remove photo, text, and symbol marker images from the preview folder.
		foreach (TourView tourView in TourViews)
		{
			tourView.RemoveImage();
		}
		
		// Remove the map image from the database and the map image files from the preview folder.
		RemoveImage();

		// Delete this page's htm, js, css, and xml files from the preview folder
		DeletePageFiles();
		
		DeleteExclusiveMarkers();
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_Delete", "@TourPageId", id);
		tour.RemoveTourPage(this);
		tour.RebuildTourTreeXml();
		tour.SetNothingSelected();
		MapsAliveState.Account.UpdateHotspotStatus();
	}

	public void DeleteExclusiveMarkers()
	{
		// We used to loop over each tour view in TourView calling tourView.DeleteExclusiveMarker()
		// For each one. Performance profiling showed that to be very expensive. A tour with
		// hundreds of markers -- exclusive or not -- took several seconds to delete because of the
		// time it took to read and initialize all of its tour views from the database. This stored 
		// procedure instantly deletes all the markers in a single operation.
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_DeleteExclusiveMarkers", "@TourPageId", id);
	}

	private void DeletePageFiles()
	{
		ArrayList files = new ArrayList();

		files.Add(FileManager.PreviewFolderLocationAbsolute(tour.Id, string.Format(TourBuilder.PatternForPageHtmlPreviewFile, id, tour.BuildId)));
		files.Add(FileManager.PreviewFolderLocationAbsolute(tour.Id, string.Format(TourBuilder.PatternForPageHtmlPreviewFileV3, id, tour.BuildId)));

		files.Add(FileManager.PreviewFolderLocationAbsolute(tour.Id, string.Format(TourBuilder.PatternForPageJsFile, this.pageNumber, tour.BuildId)));
		files.Add(FileManager.PreviewFolderLocationAbsolute(tour.Id, string.Format(TourBuilder.PatternForPageJsFileV3, this.pageNumber)));

		files.Add(FileManager.PreviewFolderLocationAbsolute(tour.Id, string.Format(TourBuilder.PatternForPageCssFile, this.pageNumber, tour.BuildId)));
		files.Add(FileManager.PreviewFolderLocationAbsolute(tour.Id, string.Format(TourBuilder.PatternForPageCssFileV3, this.pageNumber)));

		files.Add(FileManager.PreviewFolderLocationAbsolute(tour.Id, string.Format(TourBuilder.PatternForSymbolsFile, this.pageNumber, tour.BuildId)));
		files.Add(FileManager.PreviewFolderLocationAbsolute(tour.Id, string.Format(TourBuilder.PatternForSymbolsFileV3, this.pageNumber)));

		// The V3 map tiles file gets deleted by TourImage::DeleteMapImagesFromPreviewFolder. It uses a wildcard to delete the map image, inset image, and tiles.

		files.Add(FileManager.PreviewFolderLocationAbsolute(tour.Id, string.Format(TourBuilder.PatternForMapJsFile, this.pageNumber, tour.BuildId)));
		// There is no V3 map JS file.

		foreach (string fileLocation in files)
			FileManager.DeleteFile(fileLocation);
	}

	public TourView GetRouteHotspot()
	{
		foreach (TourView tourView in TourViews)
		{
			if (tourView.MarkerIsRoute)
				return tourView;
		}
		return null;
	}

	public TourView GetTourView(int tourViewId)
	{
		foreach (TourView tourView in TourViews)
		{
			if (tourViewId == tourView.Id)
				return tourView;
		}

		return null;
	}

	public TourView GetTourViewBySlideId(string slideId)
	{
		foreach (TourView tourView in TourViews)
		{
			if (slideId.ToLower() == tourView.SlideId.ToLower())
				return tourView;
		}

		return null;
	}

	public TourView GetTourViewTemp(int tourViewId)
	{
		// This method allows us to get just one view without loading all views.
		// However, if the views for this page are already loaded, lookup the
		// requested view and return it.
		if (_tourViews != null)
			return FirstTourView;

		// Fetch just the requested view from the database.  
		return new TourView(tour, this, tourViewId);
	}

	public bool HasChangedSinceLastBuilt()
	{
		if (changed != 0)
			return true;

		foreach (TourView tourView in TourViews)
		{
			if (tourView.HasChangedSinceLastBuilt())
				return true;
		}
		return false;
	}

	public void ImageUploaded(string fileName, Size sizeFile, Byte[] bytesFile, bool importedFromArchive)
	{
		Size oldMapSize = Size.Empty;

		if (sizeFile != Size.Empty)
		{
			oldMapSize = ScaledMapSize;
			mapImage.Uploaded(tour.Id, fileName, sizeFile, bytesFile);
		}

		if (!IsGallery)
		{
			// The map image has changed (or has been uploaded for the first time). 
			// Adjust the layout based on the new map's size and aspect ratio.
			layoutManager.PerformAutoLayoutForNewMapImage();

			if (!importedFromArchive)
			{
				// Set the zoom defaults for a new image. If importing the image from an archive,
				// leave the current settings alone.
				SetMapZoomDefaults();
			}
		}

		FlagAsChangedIf(ScaledMapSize != oldMapSize, ChangeFlags.MapSize);
		FlagAsChanged(ChangeFlags.MapImage);

        if (sizeFile != Size.Empty)
        {
            mapImage.BumpVersionAndUpdateDatabase();
            mapImage.KeepUploadedFile(tour.Id);
        }

		RebuildMap();
	}

	private void InitGalleryOptions()
	{
		galleryOptions = new GalleryOptions(
			false,
			8,
			8,
			false,
			true,
			8,
			8,
			GalleryCellAlignH.Center,
			GalleryCellAlignV.Center,
			false,
			false,
			ImageExpansionType.Center);
	}

	public void InvalidateThumbnail()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_SetThumbnailNull", "@TourPageId", id);
		thumbnailBytes = null;
	}

	public void InsertTourPageIntoDatabase(int pageNumber, bool isDataSheet)
	{
		this.pageNumber = pageNumber;

		// If the user did not choose a map image, we provide one for them.
		if (IsNewTourPage)
		{
			mapImageId = isDataSheet ? 0 : TourImage.GetNextIdForTour(tour.Id);
			MapImage.Id = mapImageId;
			FlagAsChangedIf(!IsDataSheet, ChangeFlags.MapImage);
		}

		id = (int)MapsAliveDatabase.ReadScalar("sp_TourPage_CreateTourPage",
			"@TourId", tour.Id,
			"@ThemeId", tour.ThemeId,
			"@PageNumber", pageNumber,
			"@MapImageId", mapImageId);
		
		mapImage.InsertImageIntoDatabase();
		
		UpdateDatabase();
	}

	public void LoadViews()
	{
		Debug.Assert(_tourViews == null, "LoadViews called when views already loaded");
	//	Debug.WriteLine(">>> LoadViews()"); 

		_tourViews = new ArrayList();
		DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_TourView_GetTourViewIdsByTourPageId", "@TourPageId", id);
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int tourViewId = row.IntValue("TourViewId");
			
			TourView tourView;

			// If the row represents the selected view, use the TourView object we
			// already have.  Otherwise we'll have two instances of the view.  Because
			// TourView objects (and their corresponding database records) can get updated
			// during a build, we have to be sure that everything stays in sync and that
			// won't happen if the selected view is not included in the view list.
			TourView selectedTourView = tour.SelectedTourView;
			if (selectedTourView != null && selectedTourView.Id == tourViewId)
				tourView = selectedTourView;
			else
				tourView = new TourView(tour, this, tourViewId);

			_tourViews.Add(tourView);
		}
	}

	public void MapMarkerChanged()
	{
		FlagAsChanged(ChangeFlags.MapMarker);
	}

	public bool QualifiesForMapZoom(Size mapAreaSize)
	{
		// Determine if map zoom should be turned on or off. The determination is made based
		// on how many pixels you could pan the map if map zoom were turned on. We figure that
		// anything less than a few inches isn't worth zooming.
		const int mapZoomThreshold = 200;
		return
			mapImage.Size.Width - mapAreaSize.Width > mapZoomThreshold ||
			mapImage.Size.Height - mapAreaSize.Height > mapZoomThreshold;
	}

	public void RebuildMap()
	{
		// This method does not actually rebuild the map, but simply flags it as changed.
		// This way, RebuildMap can be called any number of times without incurring the overhead
		// of a rebuild on each call.
		FlagAsChanged(ChangeFlags.Map);
	}

	public static void RebuildMap(DataTable tourPageIdDataTable)
	{
		// The map for each tour page in the data table needs to be rebuilt.
		
		Tour tour = null;

		foreach (DataRow dataRow in tourPageIdDataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int tourId = row.IntValue("TourId");
			int tourPageId = row.IntValue("TourPageId");

			// Get the in-memory tour for this page instead of creating a temp Tour object.
			if (tour == null || tour.Id != tourId)
				tour = Tour.GetSelectedTourOrCreateFromDatabase(tourId);

			if (tour.Id == 0)
				continue;

			// Get the page. Check to see if this page is already in memory 
			// and if so use it instead of a temp TourPage object.
			TourPage tourPage = tour.GetInMemoryTourPageOrCreateFromDatabase(tour, tourPageId);

			tourPage.UpdateDatabase();

			Utility.Trace(string.Format("RebuildMap for {0} : {1}", tour.Name, tourPage.Name));
		}
	}

    public void RemoveImage()
	{
		// Remove map image files from the preview folder.
		mapImage.DeleteMapImagesFromPreviewFolder(tour.Id);

		// Remove the map image from the database.
		mapImage.Remove();
		
		SetNoImage();
	}

	public void RemoveTourView(TourView tourView)
	{
		TourViews.Remove(tourView);

		if (firstTourViewId == tourView.Id)
		{
			// If we just removed the only view, set the first tour view Id to 0.
			// Otherwise we arbitrarily make the first view in the list the first view.
			SetFirstTourView(TourViews.Count == 0 ? 0 : ((TourView)TourViews[0]).Id);
		}
	}

	public void SetBannerImageChanged()
	{
		FlagAsChanged(ChangeFlags.BannerImage);
		InvalidateThumbnail();
	}

	public void SetBannerOptionsChanged()
	{
		FlagAsChanged(ChangeFlags.BannerOptions);
	}

	public void SetFirstTourView(int tourViewId)
	{
		bool firstTourViewChanged = firstTourViewId != tourViewId;
		if (firstTourViewChanged)
		{
			FlagAsChanged(ChangeFlags.FirstTourView);
			firstTourViewId = tourViewId;
			MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_UpdateTourPageFirstTourView",
				"@TourPageId", id, "@FirstTourViewId", firstTourViewId, "@ChangeFlags", changed);
			
			// Remove the thumbnail that contained the previous first slide's image.
			InvalidateThumbnail();
		}
	}

	public void SetImageAreaSizeChanged()
	{
		FlagAsChanged(ChangeFlags.ImageAreaSize);
	}

	public void SetMapImageChanged()
	{
		FlagAsChanged(ChangeFlags.MapImage);
	}

	public void SetMapImageSizeChanged()
	{
        FlagAsChanged(ChangeFlags.MapSize);
        RebuildMap();
	}

	private void SetMapZoomDefaults()
	{
		// Set map zoom defaults so that the map is all the way zoomed out. Note that 100% means the
		// map is zoomed in all the way to its full size. In V4, 0 means unlocked and all the way zoomed
        // out. In V3, all the way zoomed out is the map scale when the map is all the way zoomed out, but
        // that doesn't work in V4 because the scale of responsive maps depends on the container size.
		
		if (mapImage.HasFile)
			mapZoomLevel = (tour.V3CompatibilityEnabled ? CalculateMapAreaScale() : 0) * 100;
		else
			mapZoomLevel = 100;
		
		mapZoomX = 0;
		mapZoomY = 0;
		
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_ResetMapZoomDefaults", "@TourPageId", id);
	}

	public void SetLayoutChanged()
	{
		FlagAsChanged(ChangeFlags.SlideLayout);
	}

	public void SetNoImage()
	{
		mapImage = null;
		mapImageId = 0;
	}

	public void SetReadyMapGroupId(int groupId)
	{
		ReadyMapGroupId = groupId;
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_UpdateReadyMapGroupId", "@TourPageId", id, "ReadyMapGroupId", groupId);
	}

	public void SetTooltipStyleChanged()
	{
		FlagAsChanged(ChangeFlags.TooltipStyle);
		_tooltipStyle = null;
	}

	public static bool TourPageNameInUse(Tour tour, int tourPageId, string name)
	{
		return MapsAliveDatabase.GetCount("sp_TourPage_GetTourPageExistsByTourPageName", "@TourId", tour.Id, "TourPageId", tourPageId, "@Name", name) != 0;
	}

	public static bool TourPagePageIdInUse(Tour tour, int tourPageId, string pageId)
	{
		return MapsAliveDatabase.GetCount("sp_TourPage_GetTourPageExistsByPageId", "@TourId", tour.Id, "TourPageId", tourPageId, "@PageId", pageId) != 0;
	}

	public void TourChanged()
	{
		FlagAsChanged(ChangeFlags.TourHtml);
	}

	public void UnloadTourViews()
	{
	//	Debug.WriteLine(">>> UnloadTourViews() for " + name);
		_tourViews = null;
	}

	public void UnlockSplitters()
	{
		ActiveSlideLayout.Splitters = new SlideLayoutSplitters(ActiveSlideLayout.Splitters.H, ActiveSlideLayout.Splitters.V, false, false);
	}

	public void UpdateDatabase()
	{
		// We make the broad assumption that whenever the page is updated, its thumbnail
		// becomes invalid.  It will be regenerated the next time it is needed.
		Byte[] nullThumbnail = new Byte[0];
		thumbnailBytes = null;

		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_UpdateTourPage",
			"@TourPageId", id,
			"@ThemeId", tour.ThemeId,
			"@Name", name,
			"@Title", title,
			"@PageId", pageId,
			"@SlideListInstructions", slideListInstructions,
			"@InPopupState", slidesPopup,
			"@SlideLayoutFixedType", layoutAreaSlideLayout.Pattern,
			"@SlideLayoutFixedSplitterH", layoutAreaSlideLayout.Splitters.H,
			"@SlideLayoutFixedSplitterV", layoutAreaSlideLayout.Splitters.V,
			"@SlideLayoutFixedSplitterLockedH", layoutAreaSlideLayout.Splitters.LockedH,
			"@SlideLayoutFixedSplitterLockedV", layoutAreaSlideLayout.Splitters.LockedV,
			"@SlideLayoutPopupType", popupSlideLayout.Pattern,
			"@SlideLayoutPopupWidth", popupSlideLayout.OuterSize.Width,
			"@SlideLayoutPopupHeight", popupSlideLayout.OuterSize.Height,
			
			"@SlideLayoutPopupDelayType", (int)popupOptions.DelayType,
			"@SlideLayoutPopupDelay", popupOptions.Delay,
			"@SlideLayoutPopupMinWidth", popupOptions.MinSize.Width,
			"@SlideLayoutPopupMinHeight", popupOptions.MinSize.Height,
			"@SlideLayoutPopupTextOnlyWidth", popupOptions.TextOnlyWidth,
			"@SlideLayoutPopupShowArrow", popupOptions.ArrowType == PopupArrowType.None ? false : true,
			"@SlideLayoutPopupArrowType", (int)popupOptions.ArrowType,
			"@SlideLayoutPopupShowTooltipWhenNoContent", popupOptions.ShowTooltipWhenNoContent,
			"@SlideLayoutPopupSticks", popupOptions.PinOnClick,
			"@SlideLayoutPopupPinMessage", popupOptions.PinMessage == MapsAliveTourBuilder.Text.DefaultPinPopupMessage ? string.Empty : popupOptions.PinMessage,
			"@SlideLayoutBestSideSequence", popupOptions.BestSideSequence,
			"@SlideLayoutPopupBorderWidth", popupOptions.BorderWidth,
			"@SlideLayoutPopupCornerRadius", popupOptions.PopupCornerRadius,
			"@SlideLayoutPopupImageRadius", popupOptions.ImageCornerRadius,
			"@SlideLayoutPopupDropShadowDistance", popupOptions.DropShadowDistance,
			"@SlideLayoutPopupBorderColor", popupOptions.BorderColor,
			"@SlideLayoutPopupBackgroundColor", popupOptions.BackgroundColor,
			"@SlideLayoutPopupTextColor", popupOptions.TextColor,
			"@SlideLayoutPopupTitleTextColor", popupOptions.TitleTextColor,
			"@SlideLayoutPopupLocation", (int)popupOptions.Location,
			"@SlideLayoutPopupLocationX", popupOptions.LocationPoint.X,
			"@SlideLayoutPopupLocationY", popupOptions.LocationPoint.Y,
			"@SlideLayoutPopupMarkerOffset", popupOptions.MarkerOffset,
			"@SlideLayoutPopupUseTourStyleColors", popupOptions.UseColorSchemeColors,
			
			"@SlideLayoutPopupSplitterH", popupSlideLayout.Splitters.H,
			"@SlideLayoutPopupSplitterV", popupSlideLayout.Splitters.V,
			"@SlideLayoutPopupSplitterLockedH", popupSlideLayout.Splitters.LockedH,
			"@SlideLayoutPopupSplitterLockedV", popupSlideLayout.Splitters.LockedV,
			"@SlideLayoutPopupMarginTop", popupSlideLayout.Margin.Top,
			"@SlideLayoutPopupMarginRight", popupSlideLayout.Margin.Right,
			"@SlideLayoutPopupMarginBottom", popupSlideLayout.Margin.Bottom,
			"@SlideLayoutPopupMarginLeft", popupSlideLayout.Margin.Left,
			"@SlideLayoutPopupSpacingH", popupSlideLayout.Spacing.H,
			"@SlideLayoutPopupSpacingV", popupSlideLayout.Spacing.V,
			"@SlideLayoutFixedMarginTop", layoutAreaSlideLayout.Margin.Top,
			"@SlideLayoutFixedMarginRight", layoutAreaSlideLayout.Margin.Right,
			"@SlideLayoutFixedMarginBottom", layoutAreaSlideLayout.Margin.Bottom,
			"@SlideLayoutFixedMarginLeft", layoutAreaSlideLayout.Margin.Left,
			"@SlideLayoutFixedSpacingH", layoutAreaSlideLayout.Spacing.H,
			"@SlideLayoutFixedSpacingV", layoutAreaSlideLayout.Spacing.V,
			"@SlideLayoutMinNonMapWidth", layoutMinNonMapWidth,
			"@SlideLayoutMinNonMapHeight", layoutMinNonMapHeight,
			"@SlideLayoutShowTitle", showSlideTitle,
			"@ShowSlideList", showSlideList,
			"@ShowSlideNamesInMenu", showSlideNamesInMenu,
			"@ShowHelp", showInstructions,
			"@HelpText", instructionsText,
			"@HelpTitle", instructionsTitle,
			"@HelpBgColor", instructionsBgColor,
			"@HelpColor", instructionsColor,
			"@HelpFont", instructionsFont,
			"@HelpFontSize", instructionsFontSize,
			"@HelpWidth", instructionsWidth,
			"@MapInsetLocation", mapInsetLocation,
			"@MapInsetSize", mapInsetSize,
			"@MapCanZoom", mapCanZoom,
			"@MapZoomX", mapZoomX,
			"@MapZoomY", mapZoomY,
			"@MapZoomLevel", mapZoomLevel,
			"@MouseOverDelay", mouseOverDelay,
			"@SaveMapStateChanges", saveMapStateChanges,
			"@SelectedMarkerBlink", selectedMarkerBlink,
			"@ShowPanZoomControls", showPanZoomControls,
			"@VisitedMarkerAlpha", visitedMarkerAlpha,
			"@ShowSlideShow", runSlideShow,
			"@SlideShowInterval", slideShowInterval,
			"@Thumbnail", nullThumbnail,
			"@MenuPosition", menuPosition.ToString(),
			"@TooltipStyleId", tooltipStyleId,
			"@MapPlaceholderColor", MapPlaceholderColor,
			"@RoutesXml", RoutesXml,
			"@ShowRouteList", ShowRouteList,
			"@MapInsetColor", mapInsetColor,
			"@PanZoomControlColorOff", PanZoomControlColorOff,
			"@PanZoomControlColorOn", PanZoomControlColorOn,
			"@ExcludeFromNavigation", ExcludeFromNavigation,
			"@IsGallery", galleryOptions.IsGallery,
			"@GallerySpacingRow", galleryOptions.SpacingRow,
			"@GallerySpacingColumn", galleryOptions.SpacingColumn,
			"@GalleryAutoSpacingRow", galleryOptions.AutoSpacingRow,
			"@GalleryAutoSpacingColumn", galleryOptions.AutoSpacingColumn,
			"@GalleryMarginTop", galleryOptions.MarginTop,
			"@GalleryMarginLeft", galleryOptions.MarginLeft,
			"@GalleryCellAlignH", galleryOptions.CellAlignH,
			"@GalleryCellAlignV", galleryOptions.CellAlignV,
			"@GalleryUseFixedRowHeight", galleryOptions.UseFixedRowHeight,
			"@GalleryUseFixedColumnWidth", galleryOptions.UseFixedColumnWidth,
			"@GalleryBackgroundType", galleryOptions.BackgroundType,
			"@MarkersZoom", markersZoom,
			"@MapZoomLimit", mapZoomLimit,
			"@MarkerZoomLimit", markerZoomLimit,
			"@ChangeFlags", (int)changed
		);
	}

	public void UpdateDatabaseFirstTourView()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_UpdateTourPageFirstTourView",
			"@TourPageId", id,
			"@FirstTourViewId", firstTourViewId,
			"@ChangeFlags", (int)changed
		);
	}

	public void UpdateDatabaseTooltipStyle()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_UpdateTourPageTooltipStyle",
			"@TourPageId", id,
			"@TooltipStyleId", TooltipStyle.Id,
			"@ChangeFlags", (int)changed
		);
	}

    // The HelpState column in the database is no longer being used. This method is
    // preserved in case the column should be used for another purpose in the future.
    public void updateHelpState(int helpState)
    {
        MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_UpdateHelpState",
            "@TourPageId", id, "HelpState", helpState);
    }

    public void UpdateMarkerCoords(string coords)
	{
		try
		{
			if (coords.Length == 0)
				return;

			string[] markerCoordSets = coords.Split(';');
            int[] coordsSequence = new int[markerCoordSets.Length];
			Hashtable hashTable = new Hashtable();
            int index = 0;
			
            foreach (string markerCoordSet in markerCoordSets)
			{
				string[] coordSet = markerCoordSet.Split(',');
                int tourViewId = int.Parse(coordSet[0]);
                hashTable.Add(tourViewId, coordSet);
                coordsSequence[index] = tourViewId;
                index += 1;
            }

			bool viewChanged = false;

            foreach (TourView tourView in TourViews)
			{
				string[] coordSet = (string[])hashTable[tourView.Id];
				if (coordSet == null)
					continue;

				double pctX = double.Parse(coordSet[1]);
				if (double.IsNaN(pctX))
				{
					// This should never happen, but it has and until we figure out how, we trap it.
					Debug.Fail(string.Format("X Coordinate is NaN. Leaving as {0} : {1}", tourView.MarkerPctX, coords));
				}
				else
				{
					tourView.MarkerPctX = pctX;
				}

				double pctY = double.Parse(coordSet[2]);
				if (double.IsNaN(pctY))
				{
					// This should never happen, but it has and until we figure out how, we trap it.
					Debug.Fail(string.Format("Y Coordinate is NaN. Leaving as {0} : {1}", tourView.MarkerPctY, coords));
				}
				else
				{
					tourView.MarkerPctY = pctY;
				}

				tourView.MarkerRotation = int.Parse(coordSet[3]);
				tourView.MarkerIsLocked = int.Parse(coordSet[4]) == 1;

				if (tourView.MarkerChanged)
				{
					const bool notifyTourPage = false;
					tourView.UpdateDatabase(notifyTourPage);
					viewChanged = true;
				}

                // Get this tour view's position within the coords. If it's different than the view's sequence
                // number, the user changed the stacking order of at least one hotspot which could affect the
                // sequence number of some or all of the other view's. Update each view as necessary.
                int sequenceNumber = Array.IndexOf(coordsSequence, tourView.Id) + 1;
                if (tourView.SequenceNumber != sequenceNumber)
                {
                    tourView.SetSequenceNumber(sequenceNumber);
                    viewChanged = true;
                }
            }

            if (viewChanged)
				TourViewChanged();

		}
		catch (Exception ex)
		{
			Debug.Fail(string.Format("Exception in UpdateMarkerCoords: '{0}', {1}", coords, ex.Message));
		}
	}

	public string Url(HttpRequest request)
	{
		return string.Format("{0}/{1}", tour.Url, NameForPageHtmlPublishedFile);
	}

	public void TourViewChanged()
	{
		TourViewChanged(true);
	}

	public void TourViewChanged(bool rebuildMap)
	{
		FlagAsChanged(ChangeFlags.TourViews);
		if (rebuildMap)
			RebuildMap();
		WriteChangeFlagsToDatabase();
	}
	#endregion

	#region ===== Protected =========================================================
	#endregion

	#region ===== Private ===========================================================

	private bool Changed(ChangeFlags flags)
	{
		return (changed & flags) != 0;
	}

	private void FlagAsChanged(ChangeFlags flag)
	{
		FlagAsChangedIf(true, flag);
	}

	private void FlagAsChangedIf(bool condition, ChangeFlags flag)
	{
		if (condition)
			changed |= flag;
	}

	private void FlagMapAsChangedIf(bool condition)
	{
		if (condition)
		{
			changed |= ChangeFlags.Map;
		}
	}

	private void FlagInstructionsAsChangedIf(bool condition)
	{
		if (condition)
		{
			changed |= ChangeFlags.Instructions;
		}
	}

	private void SetDefaultMapControlColors()
	{
		mapInsetColor = "#dbdbff";
		panZoomControlColorOff = "#ffffff";
		panZoomControlColorOn = "#eeeeee";
	}

	private void SetDefaultOptions()
	{
		mapInsetLocation = tour.V3CompatibilityEnabled ? 4 : 2;
		mapInsetSize = 112;
		mapCanZoom = tour.V3CompatibilityEnabled ? false : true;
		mapZoomX = 0;
		mapZoomY = 0;
		mapZoomLevel = 100;
		saveMapStateChanges = false;
		showPanZoomControls = true;

        // V4 uses the old V3 mouseOverDelay property as the Map Image Sharpening property.
        mouseOverDelay = 3;
		
		instructionsBgColor = "#FFFFFF";
		instructionsColor = "#000000";
		instructionsFont = defaultInstructionsFont;
		instructionsFontSize = defaultInstructionsFontSize;
		instructionsTitle = "";
		instructionsWidth = 400;
		
		slideShowInterval = 2000;
		selectedMarkerBlink = 6;
		visitedMarkerAlpha = 100;
	}
	private void WriteChangeFlagsToDatabase()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourPage_SetChangeFlags", "@TourPageId", this.id, "@ChangeFlags", (int)changed);
	}
	#endregion
}
