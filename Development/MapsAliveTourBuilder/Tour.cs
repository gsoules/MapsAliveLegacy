// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Web;
using System.Xml;
using System.Xml.XPath;

// These values are known in the DB -- Do not change.
// Note: ExpiredPre_1_57 is the value for tours expired prior to version 1.57.
// We have to distinguish because we know use a different deactivation scheme.
public enum TourState
{
	Active = 1,
	ExpiredPre_1_57 = 2,
	Expired = 3,
}

// These values are known in the DB -- Do not change.
public enum TourSizeType
{
	Unknown = 0,
	Exact = 1,
	Max = 2,
	LayoutArea = 3
}

public partial class Tour
{
	// These values are known in the database -- don't change them.
	public enum MenuLocation
	{
		None = 1,
		Left = 2,
		Top = 3,
		AutoTop = 4
	}

	private enum TourOption
	{
		Navigation = 1,
		ColorScheme = 2,
		MenuStyle = 3,
		FontScheme = 4
	}

	// These flags get written to the database as a single integer value.
	// We use this bit mask approach for tracking changes so that we can
	// add new flags without having to add new colums to the Tour table
	// When you add a new flag, DO NOT CHANGE the hex value of existing
	// flags.  If you do, you will change the meaning of the flags in all
	// existing tours in the database.
	[Flags]
	private enum ChangeFlags
	{
		BrowserTitle	= 0x00000001,
		HasBanner		= 0x00000002,
		HasTitle		= 0x00000004,
		MenuLocation	= 0x00000008,
		ColorScheme		= 0x00000010,
		MenuStyle		= 0x00000020,
		StartPage		= 0x00000040,
		PageAdded		= 0x00000080,
		TourSize		= 0x00000100,
		FontScheme		= 0x00000200,
		PageDeleted		= 0x00000400,
		CustomFooter	= 0x00000800,
		BannerImage		= 0x00001000,
		BannerOptions	= 0x00002000,
		Runtime			= 0x00004000,
		TourName		= 0x00008000,
		Directory		= 0x00010000,
		Unused			= 0x00020000,
		HasHeaderStripe	= 0x00040000,
		HasFooterStripe	= 0x00080000,
		CustomHtml		= 0x00100000
    }

	[Flags]
	public enum MapViewerFlags
	{
		EnableMobileInternetOptions = 0x00000001,
		Deprecated1            		= 0x00000002,
        Deprecated2                 = 0x00000004,
		UseTouchUiOnDesktop			= 0x00000008,
		SelectsOnTouchStart			= 0x00000010,
		EnlargeHitTestArea			= 0x00000020,
		DisableBlendEffect			= 0x00000040,
		EnableImagePreloading		= 0x00000080,
		ViewPortIsDeviceWidth		= 0x00000100,
		WebAppCapable				= 0x00000200,
		DisableSmoothPanning		= 0x00000400,
		ShowZoomControlOnIOs		= 0x00000800,
		EntirePopupVisible			= 0x00001000,
		Deprecated3					= 0x00002000,
		EnableV3Compatibility		= 0x00004000,
		HideMenu             		= 0x00008000,           
        IsFlexMapTour               = 0x00010000,
        DisableKeyboardShortcuts    = 0x00020000
    }

    private TourAdvisor advisor;
	private bool autoLayoutEnabled;
	private Banner banner;
	private string bodyBackgroundColor;
	private int bodyMargin;
	private string browserTitle;
	private int buildId;
	private bool canAppearUnbranded;
	private Size layoutAreaSize;
	private CategoryManager categoryManager;
	private ChangeFlags changed;
	private bool createXmlFileForTour;
	private string customFooter;
	private string customHtmlAbsolute;
	private string customHtmlBottom;
	private string customHtmlCss;
	private string customHtmlJavaScript;
	private string customHtmlTop;
	private DateTime dateArchiveFileCreated;
	private DateTime dateBuilt;
	private DateTime dateCreated;
	private DateTime dateDownloadFileCreated;
	private DateTime dateModified;
	private DateTime datePublished;
    private bool exceedsSlideLimit;
	private int firstPageId;
	private int fontSchemeId;
	private bool hasTitle;
	private bool hasBanner;
	private bool hasDirectory;
	private bool hasFooterStripe;
	private bool hasHeaderStripe;
	private int id;
	private bool isPrivate;
	private bool leftAlignedInBrowser;
	private int majorVersion;
	private Size maxTourSize;
	private int menuStyleId;
	private int minorVersion;
	private string name;
	private int menuHeight;
	private int menuLocationId;
	private bool menuScrolls;
	private int menuWidth;
	private Size tourSize;
	private bool renumberPages;
	private string remoteImportUrl;
	private MapViewerFlags runtimeTarget;
	private TourPage selectedTourPage;
	private TourView selectedTourView;
	private TourState state;
	private TourDirectory tourDirectory;
	private ArrayList _tourPages;
	private int colorSchemeId;
	private ColorScheme colorScheme;
	private string tourTreeXml;
	private TourSizeType widthType;
	private TourSizeType heightType;
	private bool useSoundManager;

	public Tour()
	{
		Account account = MapsAliveState.Account;

		autoLayoutEnabled = false;
		hasTitle = false;
		hasFooterStripe = false;
		HasHeaderStripe = false;
		colorSchemeId = account.DefaultResourceId(TourResourceType.TourStyle);
		
		// The default for the body margin is 0 rather than a more pleasing value so that
		// someone creating a tour for use in an iframe won't be confused by the extra
		// space between the left and top edge of the tour and the iframe.
		bodyMargin = 0;
		
		bodyBackgroundColor = "#ffffff";

		hasDirectory = true;
		
		customFooter = string.Empty;
		fontSchemeId = DefaultFontSchemeId;
		menuLocationId = (int)DefaultMenuLocationId;
		menuStyleId = DefaultMenuStyleId;
		menuHeight = 20;
		menuWidth = 125;
		menuScrolls = false;
		tourSize = new Size(600, 600);
		maxTourSize = tourSize;
		widthType = TourSizeType.LayoutArea;
		heightType = TourSizeType.LayoutArea;
		
		majorVersion = App.MajorVersion;
		minorVersion = App.MinorVersion;
				
		// Determine how much room is available for the slide area based on the settings above.
		layoutAreaSize = TourLayout.CalculateLayoutAreaSizeFromTourSize(this, tourSize);

		dateCreated = DateTime.Now;
		state = TourState.Active;
		advisor = new TourAdvisor(this);
		remoteImportUrl = string.Empty;

		customHtmlAbsolute = string.Empty;
		customHtmlBottom = string.Empty;
		customHtmlCss = string.Empty;
		customHtmlJavaScript = string.Empty;
		customHtmlTop = string.Empty;

		runtimeTarget = MapViewerFlags.EnableMobileInternetOptions;
        runtimeTarget |= Tour.MapViewerFlags.ShowZoomControlOnIOs;
        runtimeTarget |= Tour.MapViewerFlags.EnlargeHitTestArea;
	}

	public Tour(int tourId) : this(tourId, MapsAliveState.Account.Id)
	{
	}

	public Tour(int tourId, int accountId)
	{
		MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow("sp_Tour_GetTourByTourId", "@TourId", tourId, "@ThemeId", ThemeId);
		if (row == null)
			return;

		if (row.IntValue("AccountId") != accountId)
		{
			// The requested tour does not belong to this account. This can happen if you logout of one
			// account and into another. The second login checks the cookie which contains the tour Id
			// of the last tour accessed in the previous account.
			return;
		}

		id = tourId;

		InitializeTourFromDataRecord(row, false);
	}

	public void InitializeTourFromDataRecord(MapsAliveDataRecord record, bool restoreFromArchive)
	{
		bool isRow = record is MapsAliveDataRow;
		
		Name = record.StringValue(Tag.name);
		tourSize = new Size(record.IntValue("PageWidth", Tag.tourWidth), record.IntValue("PageHeight", Tag.tourHeight));

		// Tours created prior to release 2.4 did not have user-settable max tour height and width options.
		maxTourSize = new Size(record.IntValue("AutoWidth", Tag.maxTourWidth), record.IntValue("AutoHeight", Tag.maxTourHeight));
		if (maxTourSize == Size.Empty)
			maxTourSize = tourSize;

		widthType = (TourSizeType)record.IntValue("WidthType", Tag.tourWidthType);
		heightType = (TourSizeType)record.IntValue("HeightType", Tag.tourHeightType);

        // Auto layout is no longer an option in V4. It is always enabled, but only to adjust the tour
        // height when an option is changed such as adding or removing the banner or tour title. It no
        // longer switches or modifies the tour layout as it did in V3.
        autoLayoutEnabled = V3CompatibilityEnabled ? record.BoolValue(Tag.autoLayoutEnabled) : true;
	
		browserTitle = record.StringValue(Tag.browserTitle);
		canAppearUnbranded = record.BoolValue("EmitUnbrandedPages", Tag.canAppearUnbranded);
		createXmlFileForTour = record.BoolValue("EmitTourXml", Tag.exportTourData);
		customFooter = record.StringValue(Tag.customFooter);
		hasBanner = record.BoolValue(Tag.hasBanner);
		banner = new Banner(this, record);
        hasDirectory = V3CompatibilityEnabled && MapsAliveState.Account.IsPersonalPlan ? false : record.BoolValue(Tag.hasDirectory);
		hasTitle = record.BoolValue("HasPageTitle", Tag.hasTitle);
		hasHeaderStripe = record.BoolValue(Tag.hasHeaderStripe);
		hasFooterStripe = record.BoolValue(Tag.hasFooterStripe);
		menuLocationId = record.IntValue("TourNavigationId", Tag.menuLocationId);
		fontSchemeId = record.IntValue("TourFontSchemeId", Tag.fontSchemeId);
		colorSchemeId = record.IntValue("TourStyleId", Tag.colorSchemeId);
		bodyBackgroundColor = record.StringValue(Tag.bodyBackgroundColor);
		bodyMargin = record.IntValue(Tag.bodyMargin);
		menuStyleId = record.IntValue("TourMenuStyleId", Tag.menuStyleId);
		menuScrolls = record.BoolValue(Tag.menuScrolls);
		menuWidth = record.IntValue(Tag.menuWidth);
		menuHeight = record.IntValue(Tag.menuHeight);
		tourTreeXml = record.StringValue("TreeXml");
		firstPageId = record.IntValue("StartPageId", Tag.firstPageId);
		buildId = record.IntValue("BuildId");
		majorVersion = record.IntValue(Tag.majorVersion);
		minorVersion = record.IntValue(Tag.minorVersion);

		if (isRow)
		{
			dateBuilt = record.DateTimeValue("BuildDate");
			dateCreated = record.DateTimeValue("CreateDate");
			dateModified = record.DateTimeValue("ModifyDate");
			datePublished = record.DateTimeValue("PublishDate");
			dateArchiveFileCreated = record.DateTimeValue("ArchiveFileDate");
			dateDownloadFileCreated = record.DateTimeValue("DownloadFileDate");
			changed = (ChangeFlags)record.LongValue("ChangeFlags");
			state = (TourState)record.IntValue("State");
		}
		
		isPrivate = record.BoolValue(Tag.isPrivate);
		useSoundManager = record.BoolValue(Tag.useSoundManager);
		remoteImportUrl = record.StringValue(Tag.remoteImportUrl);
		
		// Note that new lines in the StringValue returned from the record are represented as \n even
		// if they were represented as \r\n in the original tour. The XML actually contains the \r\n
		// but for some reason the XPathNavigator.Value property strips off the \r characters. We can
		// probably figure out how to restore the original bytes if this ever becomes a problem.
		customHtmlAbsolute = record.StringValue(Tag.customHtmlAbsolute);
		customHtmlBottom = record.StringValue(Tag.customHtmlBottom);
		customHtmlCss = record.StringValue(Tag.customHtmlCss);
		customHtmlJavaScript = record.StringValue(Tag.customHtmlJavaScript);
		customHtmlTop = record.StringValue(Tag.customHtmlTop);
		
		leftAlignedInBrowser = record.BoolValue(Tag.leftAlignedInBrowser);
		exceedsSlideLimit = record.BoolValue("ExceedsSlideLimit");

		if (isRow)
		{
			layoutAreaSize = TourLayout.CalculateLayoutAreaSizeFromTourSize(this, tourSize);
		}
		else
		{
			// When initializing from archive XML use the actual layout area height of the archived tour
			// because it takes into consideration how many pages the tour has. The page count affects
			// the height when MenuLocation is set to AutoTop which is the default. If we were to let
			// the size get calculated right now when the tour has no pages, a multi-page tour would
			// end up being too short because the calculation would think the tour had no top menu.
			layoutAreaSize = new Size(record.IntValue(Tag.layoutAreaWidth), record.IntValue(Tag.layoutAreaHeight));
		}

		runtimeTarget = (MapViewerFlags)record.IntValue(Tag.runtimeTarget);

        bool fixupRequired = false;

		if (majorVersion == 3 && V4)
        {
			// This tour was built with V3 and is being built in V4 for the first time.
            // Update the database to set V3 compatibility mode.
            runtimeTarget |= Tour.MapViewerFlags.EnableV3Compatibility;
            fixupRequired = true;
        }

        if (V4 && !restoreFromArchive)
        {
            // Force the directory to get read from the database (in V3 it gets read only on demand).
            // Reading the directory will cause it to perform any needed V3 to V4 fixups.
            tourDirectory = new TourDirectory(this, true);

            // Check for conflicting navigation options from V3.
            if (!hasTitle && Directory.Location == TourDirectoryLocation.TitleBar)
            {
                hasTitle = true;
                fixupRequired = true;
            }

            // Convert non-qualifying Flex Map tours that were created during development to Classic tours.
            if (IsFlexMapTour && !CanBeFlexMapTour)
            {
                runtimeTarget &= ~Tour.MapViewerFlags.IsFlexMapTour;
                fixupRequired = true;
            }
        }

        if (fixupRequired)
            UpdateDatabase();

		advisor = new TourAdvisor(this);
	}

	public enum Tag
	{
		id,
		name,
		layoutAreaWidth,
		layoutAreaHeight,
		tourWidth,
		tourHeight,
		maxTourWidth,
		maxTourHeight,
		tourWidthType,
		tourHeightType,
		autoLayoutEnabled,
		browserTitle,
		canAppearUnbranded,
		exportTourData,
		customFooter,
		hasBanner,
		hasDirectory,
		hasTitle,
		hasHeaderStripe,
		hasFooterStripe,
		menuLocationId,
		fontSchemeId,
		colorSchemeId,
		bodyBackgroundColor,
		bodyMargin,
		menuStyleId,
		menuScrolls,
		menuWidth,
		menuHeight,
		firstPageId,
		majorVersion,
		minorVersion,
		isPrivate,
		useSoundManager,
		remoteImportUrl,
		customHtmlAbsolute,
		customHtmlBottom,
		customHtmlCss,
		customHtmlJavaScript,
		customHtmlTop,
		leftAlignedInBrowser,
		runtimeTarget
	}

    public string GetTagValue(int tagId)
	{
		Tag tag = (Tag)tagId;

		switch (tag)
		{
			case Tag.id:
				return Id.ToString();
			
			case Tag.name:
				return Name;

			case Tag.layoutAreaWidth:
				return layoutAreaSize.Width.ToString();

			case Tag.layoutAreaHeight:
				return layoutAreaSize.Height.ToString();
			
			case Tag.tourWidth:
				return TourSize.Width.ToString();
			
			case Tag.tourHeight:
				return TourSize.Height.ToString();
			
			case Tag.maxTourWidth:
				return MaxTourSize.Width.ToString();
			
			case Tag.maxTourHeight:
				return MaxTourSize.Height.ToString();
			
			case Tag.tourWidthType:
				return ((int)WidthType).ToString();
			
			case Tag.tourHeightType:
				return ((int)HeightType).ToString();
			
			case Tag.autoLayoutEnabled:
				return AutoLayoutEnabled.ToString();
			
			case Tag.browserTitle:
				return BrowserTitle;
			
			case Tag.canAppearUnbranded:
				return CanAppearUnbranded.ToString();
			
			case Tag.exportTourData:
				return ExportTourData.ToString();
			
			case Tag.customFooter:
				return CustomFooter;
			
			case Tag.hasBanner:
				return HasBanner.ToString();
			
			case Tag.hasDirectory:
				return HasDirectory.ToString();
			
			case Tag.hasTitle:
				return HasTitle.ToString();
			
			case Tag.hasHeaderStripe:
				return HasHeaderStripe.ToString();
			
			case Tag.hasFooterStripe:
				return HasFooterStripe.ToString();
			
			case Tag.menuLocationId:
				return ((int)MenuLocationId).ToString();
			
			case Tag.fontSchemeId:
				return FontSchemeId.ToString();
			
			case Tag.colorSchemeId:
				return colorSchemeId.ToString();
			
			case Tag.bodyBackgroundColor:
				return BodyBackgroundColor;
			
			case Tag.bodyMargin:
				return BodyMargin.ToString();
			
			case Tag.menuStyleId:
				return MenuStyleId.ToString();
			
			case Tag.menuScrolls:
				return MenuScrolls.ToString();
			
			case Tag.menuWidth:
				return MenuWidth.ToString();
			
			case Tag.menuHeight:
				return MenuHeight.ToString();
			
			case Tag.firstPageId:
				return FirstPageId.ToString();
			
			case Tag.majorVersion:
				return MajorVersion.ToString();
			
			case Tag.minorVersion:
				return MinorVersion.ToString();
			
			case Tag.isPrivate:
				return IsPrivate.ToString();
			
			case Tag.useSoundManager:
				return UseSoundManager.ToString();
			
			case Tag.remoteImportUrl:
				return RemoteImportUrl;
			
			case Tag.customHtmlAbsolute:
				return CustomHtmlAbsolute;
			
			case Tag.customHtmlBottom:
				return CustomHtmlBottom;
			
			case Tag.customHtmlCss:
				return CustomHtmlCss;
			
			case Tag.customHtmlJavaScript:
				return CustomHtmlJavaScript;
			
			case Tag.customHtmlTop:
				return CustomHtmlTop;
			
			case Tag.leftAlignedInBrowser:
				return LeftAlignedInBrowser.ToString();

			case Tag.runtimeTarget:
				return ((int)runtimeTarget).ToString();
				
			default:
				Debug.Fail("Unsupported Tour XML tag requested " + tag);
				return "???";
		}
	}

	#region ===== Properties ========================================================

	public TourAdvisor Advisor
	{
		get { return advisor; }
	}

	public bool AutoLayoutEnabled
	{
		get { return autoLayoutEnabled; }
		set { autoLayoutEnabled = value; }
	}

	public TourSizeType WidthType
	{
		get	{ return widthType; }
		set { widthType = value; }
	}
	
	public TourSizeType HeightType
	{
		get { return heightType; }
		set { heightType = value; }
	}

	public Banner Banner
	{
		get { return banner; }
	}

	public string BodyBackgroundColor
	{
		get { return bodyBackgroundColor; }
		set
		{
			FlagAsChangedIf(bodyBackgroundColor != value, ChangeFlags.ColorScheme);
			bodyBackgroundColor = value;
		}
	}

	public int BodyMargin
	{
		get { return bodyMargin; }
		set
		{
			FlagAsChangedIf(bodyMargin != value, ChangeFlags.ColorScheme);
			bodyMargin = value;
		}
	}

	public string BrowserTitle
	{
		get { return browserTitle; }
		set
		{
			FlagAsChangedIf(browserTitle != value, ChangeFlags.BrowserTitle);
			browserTitle = value;
		}
	}

	public bool BannerImageChanged
	{
		get { return Changed(ChangeFlags.BannerImage); }
	}

	public int BuildId
	{
		get { return buildId; }
	}

	public bool CanAppearUnbranded
	{
		get { return canAppearUnbranded; }
		set
		{
			FlagAsChangedIf(canAppearUnbranded != value, ChangeFlags.BannerOptions);
			canAppearUnbranded = value;
		}
	}

	public CategoryManager CategoryManager
	{
		get
		{
			if (categoryManager == null)
			{
				categoryManager = new CategoryManager(this);
			}
			return categoryManager;
		}
	}

	public int ColorSchemeId
	{
		get { return colorSchemeId; }
		set { colorSchemeId = value; }
	}

	public string CustomFooter
	{
		get { return customFooter; }
		set
		{
			FlagAsChangedIf(customFooter != value, ChangeFlags.CustomFooter);
			customFooter = value;
		}
	}

	public string CustomHtmlAbsolute
	{
		get { return customHtmlAbsolute; }
		set
		{
			FlagAsChangedIf(customHtmlAbsolute != value, ChangeFlags.CustomHtml);
			customHtmlAbsolute = value;
		}
	}

	public string CustomHtmlBottom
	{
		get { return customHtmlBottom; }
		set
		{
			FlagAsChangedIf(customHtmlBottom != value, ChangeFlags.CustomHtml);
			customHtmlBottom = value;
		}
	}

	public string CustomHtmlCss
	{
		get { return customHtmlCss; }
		set
		{
			FlagAsChangedIf(customHtmlCss != value, ChangeFlags.CustomHtml);
			customHtmlCss = value;
		}
	}

	public string CustomHtmlJavaScript
	{
		get { return customHtmlJavaScript; }
		set
		{
			FlagAsChangedIf(customHtmlJavaScript != value, ChangeFlags.CustomHtml);
			customHtmlJavaScript = value;
		}
	}

	public string CustomHtmlJavaScriptIncludeSrc
	{
		get
		{
			string js = CustomHtmlJavaScript;
			string src = string.Empty;
			if (js.StartsWith("//#include "))
			{
				src = js.Substring(11);
				int end = src.IndexOf(Utility.CrLf);
				if (end >= 0)
				{
					src = src.Substring(0, end).Trim();
				}
			}
			return src;
		}
	}

	public string CustomHtmlTop
	{
		get { return customHtmlTop; }
		set
		{
			FlagAsChangedIf(customHtmlTop != value, ChangeFlags.CustomHtml);
			customHtmlTop = value;
		}
	}

	public DateTime DateArchiveFileCreated
	{
		get { return dateArchiveFileCreated; }
	}

	public DateTime DateBuilt
	{
		get { return dateBuilt; }
	}

	public DateTime DateCreated
	{
		get { return dateCreated; }
		set { dateCreated = value; }
	}

	public string DateCreatedShort
	{
		get { return Utility.DateShort(dateCreated); }
	}

	public DateTime DateDownloadFileCreated
	{
		get { return dateDownloadFileCreated; }
	}
	
	public DateTime DateModified
	{
		get { return dateModified; }
	}

	public DateTime DatePublished
	{
		get { return datePublished; }
	}

	public static int DefaultFontSchemeId
	{
		get { return 1; }
	}

	public static MenuLocation DefaultMenuLocationId
	{
		get { return MenuLocation.AutoTop; }
	}

	public static int DefaultMenuStyleId
	{
		get { return 3; }
	}

	public static Size DefaultTourSizeMin
	{
		get { return new Size(100, 100); }
	}

	public TourDirectory Directory
	{
		get
		{
			if (tourDirectory == null)
				tourDirectory = new TourDirectory(this, true);
			return tourDirectory;
		}
	}

	public string DownloadFileLocation
	{
		get
		{
			string previewFolderLocationAbsolute = FileManager.PreviewFolderLocationAbsolute(id);
			string fileLocation = string.Format("{0}\\{1}", previewFolderLocationAbsolute, DownloadFileName);
			return fileLocation;
		}
	}
	
	public string DownloadFileName
	{
		get { return string.Format("mapsalivetour{0}.zip", id); }

	}

	public bool ExceedsSlideLimit
	{
		get { return exceedsSlideLimit; }
		set
		{
			if (exceedsSlideLimit != value)
			{
				exceedsSlideLimit = value;
				
				if (exceedsSlideLimit)
					MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateExceedsSlideLimit", "@TourId", Id);
				
				// Flush the tour list table so that it will contain the updated exceeds slide limit flag.
				MapsAliveState.Flush(MapsAliveObjectType.TourList);
			}
		}
	}

	public bool ExportTourData
	{
		get { return createXmlFileForTour; }
		set { createXmlFileForTour = value; }
	}

	public TourPage FirstPage
	{
		get
		{
			if (TourPageCount == 0)
			{
				Debug.Fail("FirstPage called for a tour with no pages");
				return null;
			}
			return GetTourPage(FirstPageId);
		}
	}

	public int FirstPageId
	{
		get
		{
			if (firstPageId == 0 && TourPages.Count > 0)
			{
				// This should never happen, but we have seen it occur so we are repairing the database
				// until we figure out the cause of the problem and how to prevent it from happening.
				SetFirstPage(((TourPage)TourPages[0]).Id);
				Utility.ReportEvent("Repaired FirstPageId", firstPageId.ToString());
			}
			return firstPageId;
		}
		set { firstPageId = value; }
	}

	public string FolderLocationRelative
	{
		get { return "Tour/" + Id.ToString(); }
	}

	public int FontSchemeId
	{
		get { return fontSchemeId; }
		set
		{
			FlagAsChangedIf(fontSchemeId != value, ChangeFlags.FontScheme);
			fontSchemeId = value;
		}
	}

	public bool HasBanner
	{
		get { return hasBanner; }
		set
		{
			if (hasBanner != value)
			{
				FlagAsChanged(ChangeFlags.HasBanner);
				hasBanner = value;
			}
		}
	}

	public bool HasBeenBuilt
	{
		get { return buildId != 0; }
	}

	public bool HasBeenPublished
	{
		get { return datePublished != DateTime.MinValue; }
	}

	public bool HasChangedSinceLastPublished
	{
		get {return HasBeenPublished && (DateBuilt > DatePublished || HasChangedSinceLastBuilt()); }
	}

	static public string HasChangedSinceLastPublishedConfirm(int tourId)
	{
        string warningMessage = 
			"<p class=\"confirmWarning\">This tour has changed since it was last published.</p>" +
			"<p>Because you changed the tour, the published version might not look " +
			"the same as how the changed tour looks in Tour Preview.</p>";
		return string.Format("maRunStalePublishedTour('{0}','{1}');", warningMessage, App.TourUrl(tourId));
	}

	static public string HasChangedSinceLastPublishedDeny
	{
		get
		{
			string warningMessage =
                "<p class=\"confirmWarning\">This tour has changed since it was last published.</p>" +
                "<p>To see the unpublished version, edit the tour and click the Tour Preview button.</p>";
			return string.Format("maAlert('{0}');return false;", warningMessage);
		}
	}

	public bool HasCustomFooter
	{
		get { return customFooter.Trim().Length > 0; }
	}

	public bool HasCustomHtmlAbsolute
	{
		get { return CustomHtmlAbsolute.Length > 0; }
	}

	public bool HasCustomHtmlBottom
	{
		get { return CustomHtmlBottom.Length > 0; }
	}

	public bool HasCustomHtmlCss
	{
		get { return CustomHtmlCss.Length > 0; }
	}

	public bool HasCustomHtmlJavaScript
	{
		get { return CustomHtmlJavaScript.Length > 0; }
	}

	public bool HasCustomHtmlTop
	{
		get { return CustomHtmlTop.Length > 0; }
	}

	public bool HasDataSheet
	{
		get
        {
            bool hasDataSheet = false;
            foreach (TourPage tourPage in TourPages)
            {
                if (tourPage.IsDataSheet)
                {
                    hasDataSheet = true;
                    break;
                }
            }
            return hasDataSheet;
        }
	}

	public bool HasDirectory
	{
		get { return hasDirectory; }
		set
		{
			FlagAsChangedIf(hasDirectory != value, ChangeFlags.Directory);
			hasDirectory = value;
		}
	}

	public bool HasHeaderStripe
	{
		get { return hasHeaderStripe; }
		set
		{
			if (hasHeaderStripe != value)
			{
				FlagAsChanged(ChangeFlags.HasHeaderStripe);
				hasHeaderStripe = value;
			}
		}
	}

	public bool HasFooterStripe
	{
		get { return hasFooterStripe; }
		set
		{
			if (hasFooterStripe != value)
			{
				FlagAsChanged(ChangeFlags.HasFooterStripe);
				hasFooterStripe = value;
			}
		}
	}

    public bool HasGallery
    {
        get
        {
            bool hasGallery = false;
            foreach (TourPage tourPage in TourPages)
            {
                if (tourPage.IsGallery)
                {
                    hasGallery = true;
                    break;
                }
            }
            return hasGallery;
        }
    }

    public bool HasMoreThanOnePage
	{
		get { return TourPages.Count > 1; }
	}

    public bool HasTitle
	{
		get { return hasTitle; }
		set
		{
			if (hasTitle != value)
			{
				FlagAsChanged(ChangeFlags.HasTitle);
				hasTitle = value;
			}
		}
	}

	public bool HideCreatedWithMapsAlive
	{
		get { return !MapsAliveState.Account.IsTrial; }
	}

    public bool HideMenu
    {
        get { return (runtimeTarget & Tour.MapViewerFlags.HideMenu) != 0; }
    }

    public bool HtmlForAllPagesChanged
	{
		get
		{
			bool htmlChanged = Changed(
				ChangeFlags.BrowserTitle |
				ChangeFlags.CustomFooter |
				ChangeFlags.ColorScheme |
				ChangeFlags.FontScheme |
				ChangeFlags.MenuLocation |
				ChangeFlags.MenuStyle |
				ChangeFlags.PageAdded |
				ChangeFlags.PageDeleted |
				ChangeFlags.TourName |
				ChangeFlags.Directory |
				ChangeFlags.CustomHtml
			);
			
			return htmlChanged || LayoutChanged; 
		}
	}

	public int Id
	{
		get { return id; }
		set { id = value; }
	}

	public string ImagesFolder
	{
		get { return App.TourUrl(id); }
	}

    public bool CanBeFlexMapTour
    {
        get
        {
            if (HasBanner)
                return false;
            if (HasTitle)
                return false;
            if (HasCustomFooter)
                return false;
            if (HasHeaderStripe)
                return false;
            if (HasFooterStripe)
                return false;
            if (HasCustomHtmlTop)
                return false;
            if (HasCustomHtmlBottom)
                return false;
            if (BodyMargin != 0)
                return false;
            
            foreach (TourPage tourPage in TourPages)
            {
                if (!tourPage.SlidesPopup)
                    return false;
                if (tourPage.IsDataSheet)
                    return false;
                if (tourPage.IsGallery)
                    return false;

                SlideLayoutMargin margin = tourPage.LayoutAreaSlideLayout.Margin;
                if (margin.Top + margin.Right + margin.Bottom + margin.Left != 0)
                    return false;
             }

            return true;
        }
    }

    public ArrayList FlexMapTourDisqualifiers()
    {
        ArrayList list = new ArrayList();
        if (HasBanner)
            list.Add("Tour has a banner");

        if (HasTitle)
            list.Add("Tour has a title bar");

        if (HasCustomFooter)
            list.Add("Tour has a footer");

        if (HasHeaderStripe)
            list.Add("Tour has a header stripe");

        if (HasFooterStripe)
            list.Add("Tour has a footer stripe");

        if (HasCustomHtmlTop)
            list.Add("Tour has custom HTML Top");

        if (HasCustomHtmlBottom)
            list.Add("Tour has custom HTML Bottom");

        if (BodyMargin != 0)
            list.Add("Tour has a margin");

        foreach (TourPage tourPage in TourPages)
        {
            string pageName = tourPage.Name;

            if (tourPage.IsDataSheet)
                list.Add(string.Format("Page {0} is a data sheet ", pageName));
            else if (!tourPage.SlidesPopup)
                list.Add(string.Format("Page {0} uses a tiled layout", pageName));

            if (tourPage.IsGallery)
                list.Add(string.Format("Page {0} is a gallery", pageName));

            SlideLayoutMargin margin = tourPage.LayoutAreaSlideLayout.Margin;
            if (margin.Top + margin.Right + margin.Bottom + margin.Left != 0)
                list.Add(string.Format("Page {0} has a non-zero map margin", pageName));
        }

        return list;
    }

    public bool IsFlexMapTour
    {
        get { return (runtimeTarget & Tour.MapViewerFlags.IsFlexMapTour) != 0; }
    }

    public bool IsPrivate
	{
		get { return isPrivate; }
		set { isPrivate = value; }
	}
    public bool KeyboardShortcutsDisabled
    {
        get { return (runtimeTarget & Tour.MapViewerFlags.DisableKeyboardShortcuts) != 0; }
    }

    public Size LayoutAreaSize
	{
		get { return layoutAreaSize; }
	}

	public bool LayoutChanged
	{
		get
		{
			bool layoutChanged = Changed(
				ChangeFlags.HasBanner |
				ChangeFlags.HasTitle |
				ChangeFlags.HasHeaderStripe |
				ChangeFlags.HasFooterStripe |
				ChangeFlags.MenuLocation |
				ChangeFlags.TourSize
			);

			return layoutChanged; 
		}
	}

	public bool LeftAlignedInBrowser
	{
		get { return leftAlignedInBrowser; }
		set
		{
			FlagAsChangedIf(leftAlignedInBrowser != value, ChangeFlags.ColorScheme);
			leftAlignedInBrowser = value;
		}
	}

	public int MajorVersion
	{
		get { return majorVersion; }
		set { majorVersion = value; }
	}

	public bool MapDisableBlendEffect
	{
		get { return (runtimeTarget & Tour.MapViewerFlags.DisableBlendEffect) != 0; }
	}

	public bool MapDisableSmoothPanning
	{
		get { return (runtimeTarget & Tour.MapViewerFlags.DisableSmoothPanning) != 0; }
	}

	public bool MapEnableImagePreloading
	{
		get { return (runtimeTarget & Tour.MapViewerFlags.EnableImagePreloading) != 0; }
	}

	public bool MapEnlargeHitTestArea
	{
		get { return (runtimeTarget & Tour.MapViewerFlags.EnlargeHitTestArea) != 0; }
	}

	public bool MapEntirePopupVisible
	{
		get { return (runtimeTarget & Tour.MapViewerFlags.EntirePopupVisible) != 0; }
	}

	public bool MapSelectsOnTouchStart
	{
		get { return (runtimeTarget & Tour.MapViewerFlags.SelectsOnTouchStart) != 0; }
	}

	public bool MapShowZoomControlOnIOs
	{
		// This option now means show zoom controls on touch devices.
        get { return (runtimeTarget & Tour.MapViewerFlags.ShowZoomControlOnIOs) != 0; }
	}

	public bool MapViewPortIsDeviceWidth
	{
		get { return (runtimeTarget & Tour.MapViewerFlags.ViewPortIsDeviceWidth) != 0; }
	}

	public Size MaxTourSize
	{
		get { return maxTourSize; }
		set { maxTourSize = value; }
	}

	public bool MeetsMinimumRequirements
	{
		get
		{
			return advisor.TourMeetsMinimumRequirements;
		}
	}

	public int MenuHeight
	{
		get { return menuHeight; }
		set
		{
			FlagAsChangedIf(menuHeight != value, ChangeFlags.MenuStyle);
			menuHeight = value;
		}
	}

	public int MenuLocationId
	{
		get { return menuLocationId; }
		set
		{
			if (menuLocationId != value)
			{
				FlagAsChanged(ChangeFlags.MenuLocation);
				menuLocationId = value;
			}
		}
	}

	public int MenuLocationIdEffective
	{
		get
		{
			int idEffective;
            if (menuLocationId == (int)Tour.MenuLocation.AutoTop && V3CompatibilityEnabled)
			{
				// Get the number of pages in this tour by quering the database rather than calling
				// TourPages.Count because it counts by loading and creating each page. Normally that's
				// okay because the pages stay in memory, but this MenuLocationIdEffective property may
				// be called too early for us to want to pay the price of loading all pages.
				int pageCount = TourPageCount;
				idEffective = pageCount >= 2 ? (int)Tour.MenuLocation.Top : (int)Tour.MenuLocation.None;
			}
			else
			{
				idEffective = menuLocationId;
			}
			return idEffective;
		}
	}

    public bool MenuScrolls
    {
        get { return menuScrolls; }
		set
		{
			FlagAsChangedIf(menuScrolls != value, ChangeFlags.MenuStyle);
			menuScrolls = value;
		}
	}

	public int MenuStyleId
	{
		get { return menuStyleId; }
		set
		{
			FlagAsChangedIf(menuStyleId != value, ChangeFlags.MenuStyle);
			menuStyleId = value;
		}
	}

	public int MenuWidth
	{
		get { return menuWidth; }
		set
		{
			FlagAsChangedIf(menuWidth != value, ChangeFlags.MenuStyle);
			menuWidth = value;
		}
	}

	public int MinorVersion
	{
		get { return minorVersion; }
		set { minorVersion = value; }
	}

	public string Name
	{
		// Don't allow double quotes in tour names.
        get { return name.Replace("\"", ""); }
		set
		{
            string tourName = value.Replace("\"", "");
            FlagAsChangedIf(name != null && name != tourName, ChangeFlags.TourName);
			name = tourName;
		}
	}

	public string NameForTourCssJsFile
	{
		get { return string.Format(TourBuilder.PatternForTourCssJsFile, BuildId); }
	}

	public string NameForTourCustomJsFile
	{
		get { return string.Format(TourBuilder.PatternForTourCustomJsFile, BuildId); }
	}

	public string NameForTourCustomJsFileV3
	{
		get { return string.Format(TourBuilder.PatternForTourCustomJsFileV3); }
	}

	public string NameForTourHtmlJsFile
	{
		get { return string.Format(TourBuilder.PatternForTourHtmlJsFile, BuildId); }
	}

	public string NameForTourIndexPreviewFile
	{
		get { return string.Format(TourBuilder.PatternForTourindexPreviewFile, BuildId, Id); }
	}

	public string NameForTourIndexUnbrandedPreviewFile
	{
		get { return string.Format(TourBuilder.PatternForTourindexUnbrandedPreviewFile, BuildId, Id); }
	}

	public string NameForTourIndexPublishedFile
	{
		get { return TourBuilder.PatternForTourIndexPublishedFile; }
	}

	public string NameForTourIndexUnbrandedPublishedFile
	{
		get { return TourBuilder.PatternForTourIndexUnbrandedPublishedFile; }
	}

	public string NameForTourLoaderJsFile
	{
		get { return TourBuilder.PatternForTourLoaderJsFile; }
	}

	public string NameForTourLoaderDeactivatedJsFile
	{
		get { return TourBuilder.PatternForTourLoaderDeactivatedJsFile; }
	}

	public string NameForTourPropertiesJsFile
	{
		get { return string.Format(TourBuilder.PatternForTourPropertiesJsFile, BuildId); }
	}

	public int NextDataSheetId
	{
		get	{ return (int)MapsAliveDatabase.ReadScalar("sp_Tour_GetNextDataSheetId", "@TourId", Id); }
	}
	
	public int NextMapId
	{
		get { return (int)MapsAliveDatabase.ReadScalar("sp_Tour_GetNextMapId", "@TourId", Id); }
	}

    public static DataTable OptionsForColorScheme
	{
		get { return TourOptionDataTable(TourOption.ColorScheme); }
	}

	public static DataTable OptionsForFontScheme
	{
		get { return TourOptionDataTable(TourOption.FontScheme); }
	}

	public static DataTable OptionsForNavigation
	{
		get { return TourOptionDataTable(TourOption.Navigation); }
	}

	public static DataTable OptionsForMenuStyle
	{
		get { return TourOptionDataTable(TourOption.MenuStyle); }
	}

	public Size TourSize
	{
		get { return tourSize; }
	}

	public bool TourSizeChanged
	{
		get { return Changed(ChangeFlags.TourSize); }
	}

	public bool TourWidthLocked
	{
		get { return widthType == TourSizeType.Exact; }
	}

	public bool TourHeightLocked
	{
		get { return heightType == TourSizeType.Exact; }
	}

	public bool RenumberPages
	{
		get { return renumberPages; }
		set { renumberPages = value; }
	}

	public bool RequiresRebuild
	{
		get
		{
			if (VersionLessThan(App.MajorVersion, App.MinorVersion))
			{
				// The tour needs to get rebuilt with the latest runtime.
				return true;
			}

			if (Changed(ChangeFlags.Runtime))
			{
				// Runtime releated tour options changed.
				return true;
			}

			if (!FileManager.FolderExists(FileManager.PreviewFolderLocationAbsolute(id)))
			{
				// There is no preview folder. This can happen in the development environment where
				// different developers have their own tour folders, but share the same database.
				return true;
			}

			return false;
		}
	}

	public string RemoteImportUrl
	{
		get { return remoteImportUrl; }
		set { remoteImportUrl = value; }
	}
	
	public MapViewerFlags RuntimeTarget
	{
		get { return runtimeTarget; }
		set
		{
			FlagAsChangedIf(runtimeTarget != value, ChangeFlags.Runtime);
			runtimeTarget = value;
		}
	}

	public TourPage SelectedTourPage
	{
		get { return selectedTourPage; }
	}

	public TourView SelectedTourView
	{
		get { return selectedTourView; }
	}

	public static string SessionKey
	{
		get { return "TourObject"; }
	}

	public bool StartPageChanged
	{
		get	{ return Changed(ChangeFlags.StartPage); }
	}

	public TourState State
	{
		get { return state; }
	}

	public int ThemeId
	{
		get { return 1; }
	}

	public static Size ThumbnailSize
	{
		get { return new Size(100, 100); }
	}

	public int TourPageCount
	{
		get { return MapsAliveDatabase.GetCount("sp_TourPage_GetTourPageCountByTourId", "@TourId", id); }
	}

	public ArrayList TourPages
	{
		get
		{
			// We load tour pages on demand so that we don't pay the price to fetch tour page
			// information from the database unless a request has actually been made for it.
			if (_tourPages == null)
				LoadPages();
			return _tourPages;
		}
	}

	public string TourTreeXml
	{
		get
		{
			if (tourTreeXml == null || tourTreeXml.Length == 0)
				RebuildTourTreeXml();
			return tourTreeXml;
		}
	}

	public string Url
	{
		get { return App.TourUrl(id); }
	}

	public string UrlPlain
	{
		// Strip off the leading http://
		get { return Url.Substring(7); }
	}

	public string UrlWithBuildId
	{
		get
		{
			string url = App.TourUrl(id);
			
			// Append a do-nothing query string with the latest build Id so that
			// the browser won't display the old cached default.htm page.
			url += "/default.htm?build=" + buildId;
			
			return url;
		}
	}

	public bool UsesPopupArrow
	{
		get
		{
			bool usesPopupArrow = false;

			foreach (TourPage tourPage in TourPages)
			{
				if (tourPage.IsDataSheet)
					continue;
				if (!tourPage.SlidesPopup)
					continue;
				if (tourPage.PopupOptions.ArrowType == PopupArrowType.None)
					continue;
				usesPopupArrow = true;
			}
			return usesPopupArrow;
		}
	}

	public bool UsesLiveData
	{
		get
		{
			foreach (TourPage tourPage in TourPages)
			{
				if (tourPage.UsesLiveData)
					return true;
			}
			return false;
		}
	}

	public bool UseSoundManager
	{
		get { return useSoundManager; }
		set
		{
			FlagAsChangedIf(useSoundManager != value, ChangeFlags.Runtime);
			useSoundManager = value;
		}
	}

	public bool V3CompatibilityEnabled
	{
		get { return (runtimeTarget & Tour.MapViewerFlags.EnableV3Compatibility) != 0; }
	}

	public bool V4
	{
		get { return !V3CompatibilityEnabled; }
	}
	#endregion

	#region ===== Public ============================================================

	public void AddTourPage(TourPage tourPage, bool isDataSheet)
	{
		TourPages.Add(tourPage);

		selectedTourPage = tourPage;
		tourPage.InsertTourPageIntoDatabase(tourPage.PageNumber, isDataSheet);
		MapsAliveState.SetSelectedTourPage(tourPage);

		if (TourPages.Count == 1)
		{
			// Thi is the first page of the tour.
			SetFirstPage(tourPage.Id);
			UpdateDatabase();
		}
	
		FlagAsChanged(ChangeFlags.PageAdded);
		RebuildTourTreeXml();

		PageCountChanged(1);
	}

	public void AddTourView(TourView tourView)
	{
		AddTourView(tourView, false);
	}

	public void AddTourView(TourView tourView, bool importingSlides)
	{
		selectedTourView = tourView;
		tourView.TourPage.AddTourView(tourView, importingSlides);
		if (!importingSlides)
			MapsAliveState.SetSelectedTourView(selectedTourView);
	}

	public void AdjustBannerToFitLayout()
	{
		int optimalHeight = Banner.OptimalHeight();

		const bool runAutoLayout = false;
		const bool imageChanged = true;
		SetBannerOptions(runAutoLayout, imageChanged, Banner.Url, Banner.UrlTitle, Banner.UrlOpensWindow);
		
		UpdateDatabase();
	}

	public void ArchiveFileCreated()
	{
		dateArchiveFileCreated = DateTime.Now;
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_ArchiveFileCreated", "@TourId", id);
	}

	public void BuildCompleted()
	{
		// Update the version in this Tour object...
		majorVersion = App.MajorVersion;
		minorVersion = App.MinorVersion;

		// ...and in the database.
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_Built",
			"@TourId", id,
			"@BuildId", buildId,
			"@MajorVersion", majorVersion,
			"@MinorVersion", minorVersion
		);
		
		// Clear change flags and set the date in this Tour object to match
		// what the stored procedure call above just did.
		changed = 0;
		dateBuilt = DateTime.Now;

		if (App.DeveloperMode)
			DumpBuildInfo();
	}

	public void BuildStarted()
	{
		buildId++;
	}

	public void CategoryFilterChanged()
	{
		// Force the category manager to get rebuilt using the new filter.
		categoryManager = null;
	}

    public void ConvertToLatestVersion()
	{
		if (VersionLessThan(3, 0) || RequiresRebuild)
		{
			TourBuilder tourBuilder = new TourBuilder(this);
			tourBuilder.BuildTour();
		}
	}

	public void CreateCustomHtmlFiles()
	{
		string previewFolderLocationAbsolute = FileManager.PreviewFolderLocationAbsolute(Id);
		string fileLocation;

		if (V3CompatibilityEnabled)
        {
			if (HasCustomHtmlCss)
            {
				fileLocation = Path.Combine(previewFolderLocationAbsolute, "custom.css");
				FileManager.CreateTextFile(fileLocation, CustomHtmlCss);
            }
			
			if (CustomHtmlJavaScript.Trim().Length > 0)
            {
				fileLocation = Path.Combine(previewFolderLocationAbsolute, NameForTourCustomJsFileV3);
				FileManager.CreateTextFile(fileLocation, CustomHtmlJavaScript);
            }
		}
		else
        {
			if (CustomHtmlJavaScript.Trim().Length > 0)
            {
				fileLocation = Path.Combine(previewFolderLocationAbsolute, NameForTourCustomJsFile);
				
				// Escape backtiks and $ so that the JavaScript text can be be emitted as Template literal
				// enclosed in backtiks. If the $ are not escaped, they'll get intrepret as expressions.
				string js = CustomHtmlJavaScript;
				js = js.Replace("`", "\\`");
				js = js.Replace("$", "\\$");
				
				// Create the custom JS file which exports the JavaScript as a string variable named js.
				js = string.Format("export let js =\n`{0}`;", js);
				FileManager.CreateTextFile(fileLocation, js);
            }
        }
	}

	public static Tour CreateNewTour(string tourName, bool noExtras, bool noDirectory)
	{
		// Create a new Tour object that will initially be used as a place to hold
		// default option values, and later as the tour that will be written to the database.
		Tour tour = new Tour();
		tour.Name = tourName == null ? CreateNewTourName() : tourName;

		// We'll use the tour name for the title if the user does not set an explicit title.
		tour.BrowserTitle = string.Empty;

		// Get the default for each dropdown list option.
		tour.MenuLocationId = (int)DefaultMenuLocationId;
		tour.FontSchemeId = DefaultFontSchemeId;
		tour.MenuStyleId = DefaultMenuStyleId;

		if (noExtras)
		{
			tour.HasTitle = false;
			tour.HasHeaderStripe = false;
			tour.HasFooterStripe = false;
		}

		if (noDirectory)
		{
			tour.HasDirectory = false;
		}

		MapsAliveState.Account.TourCountChanged();

		tour.InsertIntoDatabase();
		
		MapsAliveState.SetSelectedTour(tour);

		TourBuilder tourBuilder = new TourBuilder(tour);
		tourBuilder.BuildTourPreviewFolderRuntimeFiles();

		// When creating a new tour, make sure that no filters are set, otherwise
		// the user won't have any markers or categories to select from when adding a slide.
		Account account = MapsAliveState.Account;
		account.ClearResourceFilters();

		return tour;
	}

	public static string CreateCopyOfTourName(string oldName)
	{
		string newName = "Copy of " + oldName;

		// Generate names like "Copy of Foo", "Copy 2 of Foo", "Copy 3 of Foo" ...
		if (Tour.TourNameInUse(newName))
		{
			int index = newName.IndexOf(" of ");
			if (index > 0 && newName.Length > index + 5)
			{
				newName = newName.Substring(index + 4);
			}
		}

		if (Tour.TourNameInUse(newName))
		{
			int copyNumber = 2;
			do
			{
				newName = string.Format("Copy {0} of {1}", copyNumber, oldName);
				copyNumber++;
			} while (Tour.TourNameInUse(newName));
		}

		return newName;
	}

	public static string CreateNewTourName()
	{
		return CreateUniqueTourName(MapsAliveTourBuilder.Text.KindName, MapsAliveState.Account.TourCount, false);
	}

	public static string CreateNewTourName(string name)
	{
		if (Tour.TourNameInUse(name))
			return CreateUniqueTourName(name, 1, true);
		else
			return name;
	}

	public static string CreateUniqueTourName(string prefix, int suffix, bool useParens)
	{
		string name;
		do
		{
			suffix++;
            
            // Tour names can only be 50 characters long so truncate a long name to leave room for the suffix.
            if (prefix.Length > 44)
                prefix = prefix.Substring(0, 43);
			
            if (useParens)
				name = string.Format("{0} ({1})", prefix, suffix);
			else
				name = string.Format("{0} {1}", prefix, suffix);
		} while (Tour.TourNameInUse(name));

		return name;
	}

	public TourPage CreateNewTourPage(bool isGallery, bool isDataSheet, string pageName, bool slidesPopup)
	{
		TourPage tourPage = new TourPage(this, isDataSheet, slidesPopup);
		
		int nextId = isDataSheet ? NextDataSheetId : NextMapId;
		
		tourPage.Name = pageName == null ? CreateNewTourPageName(tourPage, isGallery, isDataSheet, nextId) : pageName;
		tourPage.Title = string.Empty;
		
		// Get a unique Id for the new page. Note that we pass isDataSheet
		// because it's too early to use the tourPage's IsDataSheet property.
		tourPage.PageId = CreateNewTourPageId(tourPage, isDataSheet, isGallery, nextId);

        // When adding a gallery or datasheet, make sure the tour has a title and that its nav button is not located in the map.
        if (V4 && (isGallery || isDataSheet))
        {
            bool updateTour = false;
            if (!HasTitle)
            {
                HasTitle = true;
                updateTour = true;
            }
            if (Directory.Location == TourDirectoryLocation.MapLeft || Directory.Location == TourDirectoryLocation.MapRight)
            {
                Directory.Location = TourDirectoryLocation.TitleBar;
                updateTour = true;
            }
            if (updateTour)
                UpdateDatabase();
        }

		return tourPage;
	}

	public static string CreateNewTourPageName(TourPage tourPage, bool isGallery, bool isDataSheet, int id)
	{
		string name = string.Format("{0} {1}", isDataSheet ? "Data Sheet" : (isGallery ? "Gallery" : "Map"), id);

		if (tourPage == null)
		{
			// The caller is requesting a candidate name to present to the user before
			// creating a new map. Don't attempt to determine if the name is already in use.
			return name;
		}

		if (TourPage.TourPageNameInUse(tourPage.Tour, tourPage.Id, name))
		{
			string prefix = name;
			int suffix = 0;
			do
			{
				suffix++;
				name = string.Format("{0}_{1}", prefix, suffix);
			} while (TourPage.TourPageNameInUse(tourPage.Tour, tourPage.Id, name));
		}
		return name;
	}

	private static string CreateNewTourPageId(TourPage tourPage, bool isDataSheet, bool isGallery, int id)
	{
		string prefix = isDataSheet ? "datasheet" : isGallery ? "gallery" : "map";
		string pageId = prefix + id;
		if (!TourPage.TourPagePageIdInUse(tourPage.Tour, 0, pageId))
			return pageId;

		prefix = pageId;
		
		// The default Id is in use (the user must have explicitly changed the Id
		// of another page to be the Id we would normally use for this page.
		// Generate a default that is not already in use.
		int suffix = 0;
		do
		{
			suffix++;
			pageId = string.Format("{0}_{1}", prefix, suffix);
		} while (TourPage.TourPagePageIdInUse(tourPage.Tour, 0, pageId));

		return pageId;
	}

	public TourView CreateNewTourView()
	{
		return CreateNewTourView(MapsAliveTourBuilder.Text.KindNameSlide);
	}

	public TourView CreateNewTourView(string titlePrefix)
	{
		return CreateNewTourView(titlePrefix, selectedTourPage);
	}

	public TourView CreateNewTourView(string titlePrefix, TourPage tourPage)
	{
		string title = titlePrefix;
		string slideId = string.Empty;
		int suffix = 0;

		// Create the view.
		TourView tourView = new TourView(this, tourPage);

		// Create a suffix to use to make the view name unique if necessary.
		if (titlePrefix == MapsAliveTourBuilder.Text.KindNameSlide)
		{
			// We are using the standard default name. Set the suffix to
			// be 1 greater than the number of tour views already on this page.
			suffix = tourPage.TourViews.Count + 1;
		}
		else
		{
			if (titlePrefix.Length > 0 && TourView.TourViewTitleInUse(tourPage, titlePrefix))
			{
				// A non-standard default name was provided, but it's already
				// in use. Set the suffix to 2 meaning the 2nd instance of the name.
				suffix = 2;
			}
		}

		if (suffix > 0)
		{
			// See if the default slide Id + suffix is in use. If it is, keep
			// trying by increasing the suffix until a unique slide Id is found.
			do
			{
				title = titlePrefix + " " + suffix;
				slideId = "H" + suffix;
				suffix++;
			} while (TourView.TourViewTitleInUse(tourPage, title) || TourView.TourViewSlideIdInUse(tourPage, slideId));
		}
		
		tourView.Title = title;
		tourView.SlideId = slideId;

		return tourView;
	}

	public TourView CreateNewTourViewForDataSheet(TourPage tourPage)
	{
		TourView tourView = new TourView(this, tourPage);
		tourView.Title = tourPage.Name;
		tourView.SlideId = tourPage.PageId;
		return tourView;
	}

	public void Delete()
	{
		Account account = MapsAliveState.Account;
		
		if (banner.HasImage)
			banner.Image.Remove();

		foreach (TourPage tourPage in TourPages)
			tourPage.DeleteExclusiveMarkers();

		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_Delete", "@TourId", id);
		
		TourBuilder tourBuilder = new TourBuilder(this);
		tourBuilder.DeleteTourFolder();

		Tour currentTour = (Tour)MapsAliveState.Retrieve(MapsAliveObjectType.Tour);
		if (currentTour != null && this.Id == currentTour.Id)
		{
			MapsAliveState.SetSelectedTour(null);
			SetNothingSelected();
		}
		
		MapsAliveState.Flush(MapsAliveObjectType.TourList);

		account.TourCountChanged();
		account.UpdateHotspotStatus();
		account.ClearResourceFilters();
	}

	public static void DenyTourAccess()
	{
		HttpContext context = HttpContext.Current;
		context.Response.Redirect("~/Members/TourExplorer.aspx");
	}

	public void DownloadFileCreated()
	{
		dateDownloadFileCreated = DateTime.Now;
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_DownloadFileCreated", "@TourId", id);
	}

	private void DumpBuildInfo()
	{
		Debug.WriteLine("\n:: Built tour " + name);
		DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_Tour_GetBuiltTourPagesAndViews", "@TourId", id, "@BuildId", buildId);
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			string tourPageName = row.StringValue("TourPageName");
			string tourViewId = row.IntValue("TourViewId").ToString();
			if (tourViewId.Length > 0)
				tourViewId = " : " + tourViewId;
			if (tourPageName.Length > 0)
			{
				string s = string.Format("::  {0}{1}", tourPageName, tourViewId);
				Debug.WriteLine(s);
			}
		}
		Debug.WriteLine("");
	}

	public void RequireRebuild()
	{
		FlagAsChanged(ChangeFlags.Runtime);
	}

	public static int GetAccountId(int tourId)
	{
		return MapsAliveDatabase.ReadInt("sp_Tour_GetAccountId", "@TourId", tourId);
	}

	public void GetCustomFooterComponents(out string prefix, out string link, out string url, out string suffix)
	{
		string footerString = customFooter.Replace("\\~", "\n");
		string[] component = footerString.Split('~');
		for (int i = 0; i < component.Length; i++)
			component[i] = component[i].Replace("\n", "~");
		int count = component.Length;
		prefix = string.Empty;
		link = string.Empty;
		url = string.Empty;
		suffix = string.Empty;
		switch (count)
		{
			case 1:
				prefix = component[0];
				break;

			case 2:
				link = component[0];
				url = component[1];
				break;

			case 3:
				prefix = component[0] + " ";
				link = component[1];
				url = component[2];
				break;

			case 4:
				prefix = component[0] + " ";
				link = component[1];
				suffix = " " + component[2];
				url = component[3];
				break;
		}
	}

	public static Tour GetSelectedTourOrCreateFromDatabase(int tourId)
	{
		Tour selectedTour = MapsAliveState.SelectedTourOrNull;
		if (selectedTour != null && selectedTour.Id == tourId)
			return selectedTour;
		else
			return new Tour(tourId);
	}

	public TourPage GetSelectedTourPageOrCreateFromDatabase(Tour tour, int tourPageId)
	{
		TourPage selectedTourPage = tour.SelectedTourPage;
		if (selectedTourPage != null && selectedTourPage.Id == tourPageId)
			return selectedTourPage;
		else
			return new TourPage(tour, tourPageId);
	}

	public TourPage GetInMemoryTourPageOrCreateFromDatabase(Tour tour, int tourPageId)
	{
		if (_tourPages != null)
		{
			foreach (TourPage tourPage in TourPages)
			{
				if (tourPage.Id == tourPageId)
					return tourPage;
			}
		}
		return new TourPage(tour, tourPageId);
	}

	public TourPage GetTourPage(int tourPageId)
	{
		foreach (TourPage tourPage in TourPages)
		{
			if (tourPageId == tourPage.Id)
				return tourPage;
		}

		return null;
	}

	public TourPage GetTourPage(string name)
	{
		foreach (TourPage tourPage in TourPages)
		{
			if (name.ToLower() == tourPage.Name.ToLower())
				return tourPage;
		}

		return null;
	}

	public TourPage GetTourPageByPageId(string pageId)
	{
		foreach (TourPage tourPage in TourPages)
		{
			if (pageId.ToLower() == tourPage.PageId.ToLower())
				return tourPage;
		}

		return null;
	}

	public TourView GetTourView(int tourViewId)
	{
		foreach (TourPage tourPage in TourPages)
		{
			foreach (TourView tourView in tourPage.TourViews)
			{
				if (tourViewId == tourView.Id)
					return tourView;
			}
		}

		return null;
	}

	public bool HasChangedSinceLastBuilt()
	{
		if (changed != 0)
			return true;

		foreach (TourPage tourPage in TourPages)
		{
			if (tourPage.HasChangedSinceLastBuilt())
				return true;
		}
		return false;
	}

	public bool IsUsingSampleMap(int sampleId)
	{
		foreach (TourPage tourPage in TourPages)
		{
			if (tourPage.MapImage.ReadyMapPackageId == sampleId)
				return true;
		}
		return false;
	}

	public void LoadPages()
	{
		Debug.Assert(_tourPages == null, "LoadPages called when pages already loaded");

		_tourPages = new ArrayList();

		DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_TourPage_GetTourPageIdsByTourId", "@TourId", id);
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int pageId = row.IntValue("TourPageId");

			// If the row represents the selected map page, use the TourPage object we
			// already have.  Otherwise we'll have two instances of the page.  Because
			// TourPage objects (and their corresponding database records) can get updated
			// during a build, we have to be sure that everything stays in sync and that
			// won't happen if the selected page is not included in the page list.
			TourPage tourPage = GetSelectedTourPageOrCreateFromDatabase(this, pageId);
			_tourPages.Add(tourPage);
		}
	}

	public void PublishCompleted()
	{
		datePublished = DateTime.Now;
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_Published", "@TourId", id);
	}

	public void UnpublishCompleted()
	{
		datePublished = DateTime.MinValue;
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_Unpublished", "@TourId", id);
	}

	public void ReloadCategories()
	{
		// Force the category manager to get rebuilt the first time it is accessed.
		// We do this so that it will get updated with the latest category information.
		// In particular, any slides that have been added that don't have categories
		// will get added to the in-memory category table with the "Other" category
		// even though they have no catgory in the database.
		categoryManager = null;

		if (HasDirectory)
		{
			// Don't flag the directory changed if there is no directory because doing so
			// will cause all pages in the tour to get rebuilt on the next tour preview.
			SetDirectoryChanged();
		}
	}

	public void ReloadTourPages()
	{
		_tourPages = null;
	}

	public void RemoveTourPage(TourPage tourPage)
	{
		TourPages.Remove(tourPage);
		FlagAsChanged(ChangeFlags.PageDeleted);

		if (FirstPageId == tourPage.Id)
		{
			// If we just removed the only page, set the start page Id to 0.
			// Otherwise we arbitrarily make the start page be the first page in the list..
			SetFirstPage(TourPages.Count == 0 ? 0 : ((TourPage)TourPages[0]).Id);
		}

		// See if any markers on other pages have the deleted page as a Go To Page target.
		TourPage otherTourPage = null;
		DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_TourView_GetTourViewIdsByTargetTourPageId", "@TargetTourPageId", tourPage.Id.ToString());
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int tourPageId = row.IntValue("TourPageId");
			int tourViewId = row.IntValue("TourViewId");
			
			if (otherTourPage == null || otherTourPage.Id != tourPageId)
				otherTourPage = new TourPage(this, tourPageId);

			TourView tourView = new TourView(this, otherTourPage, tourViewId);
			tourView.MarkerClickAction = MarkerAction.None;
			tourView.MarkerClickActionTarget = string.Empty;
			tourView.UpdateDatabase();
		}

		PageCountChanged(-1);
	}

	public void SetBannerImageChanged()
	{
		FlagAsChanged(ChangeFlags.BannerImage);
	}

	public void SetBannerOptions(bool runAutoLayout, bool imageChanged, string url, string title, bool opensWindow)
	{
		banner.SetOptions(runAutoLayout, (int)changed, imageChanged, url, title, opensWindow);
		FlagAsChanged(ChangeFlags.BannerOptions);
	}

	public void SetDirectoryChanged()
	{
		FlagAsChanged(ChangeFlags.Directory);
	}

	public void SetFirstPage(int tourPageId)
	{
		bool firstPageChanged = firstPageId != tourPageId;
		if (firstPageChanged)
		{
			FlagAsChanged(ChangeFlags.StartPage);
			firstPageId = tourPageId;
			MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateTourStartPage",
				"@TourId", id,
				"@StartPageId", firstPageId,
				"@ChangeFlags", (int)changed);
		}
	}

	public void SetMenuItemChanged()
	{
		FlagAsChanged(ChangeFlags.MenuLocation);
		UpdateChangeFlagsInDatabase();
	}

	public void SetNothingSelected()
	{
		selectedTourPage = null;
		MapsAliveState.SetSelectedTourPage(null);
		
		selectedTourView = null;
		MapsAliveState.SetSelectedTourView(null);
	}

	public void SetNoTourViewSelected()
	{
		selectedTourView = null;
		MapsAliveState.SetSelectedTourView(null);
	}

	public void SetRequiresRebuild()
	{
		FlagAsChanged(ChangeFlags.Runtime);
	}

	public void SetTourAndLayoutAreaSizes(Size newTourSize, Size newLayoutAreaSize)
	{
		FlagAsChanged(ChangeFlags.TourSize);

		tourSize = newTourSize;
		layoutAreaSize = newLayoutAreaSize;

		foreach (TourPage tourPage in TourPages)
		{
			tourPage.LayoutManager.SetLayoutAreaOuterSize(newLayoutAreaSize);
		}

		UpdateDatabase();
	}

	public void SetTourSizeAndAdjustLayouts(Size newTourSize)
	{
		if (tourSize == newTourSize)
			return;

		FlagAsChanged(ChangeFlags.TourSize);
		tourSize = newTourSize;

		// Calculate the new layout area based on the full page size minus menu area, title
		// bar, etc. that use up space that is not available for the layout.
		layoutAreaSize = TourLayout.CalculateLayoutAreaSizeFromTourSize(this, tourSize);

		// Tell each page that the layout area size changed so that the size of the individual
		// layout elements (map, photo, text) will get adjusted accordingly.
		foreach (TourPage tourPage in TourPages)
		{
			tourPage.LayoutManager.LayoutAreaSlideLayoutSizeChanged(layoutAreaSize);
			tourPage.UpdateDatabase();
		}
	}

	public void SetColorSchemeChanged()
	{
		FlagAsChanged(ChangeFlags.ColorScheme);
		colorScheme = null;
	}

	public TourPage SetSelectedTourPage(int tourPageId)
	{
		if (selectedTourPage != null && selectedTourPage.Id == tourPageId)
			return selectedTourPage;

		// When switching to a new page, free the memory for the old page's views.
		if (selectedTourPage != null)
			selectedTourPage.UnloadTourViews();

		selectedTourPage = GetTourPage(tourPageId);
		MapsAliveState.SetSelectedTourPage(selectedTourPage);
		
		selectedTourView = null;
		MapsAliveState.SetSelectedTourView(null);
		
		return selectedTourPage;
	}

	public TourView SetSelectedTourView(int tourViewId)
	{
		bool getViewFromDatabase = selectedTourPage == null;
		
		if (!getViewFromDatabase)
		{
			selectedTourView = selectedTourPage.GetTourView(tourViewId);
			getViewFromDatabase = SelectedTourView == null;
		}
		
		if (getViewFromDatabase)
		{
			MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow(
				"sp_TourView_GetTourViewByTourViewId", "@TourId", this.Id, "@TourViewId", tourViewId, "@ThemeId", this.ThemeId);
			if (row == null)
				return null;
			int tourPageId = row.IntValue("TourPageId");
			SetSelectedTourPage(tourPageId);
			if (selectedTourPage == null)
				return null;
			selectedTourView = selectedTourPage.GetTourView(tourViewId);
		}

		MapsAliveState.SetSelectedTourView(selectedTourView);
		return selectedTourView;
	}

	public void SetState(TourState newState)
	{
		if (state == newState)
			return;

		TourBuilder tourBuilder = new TourBuilder(this);
		tourBuilder.SetTourState(newState, state);

		state = newState;
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateState",
			"@TourId", id,
			"@State", newState);
	}

	public static bool TourNameInUse(string name)
	{
		return MapsAliveDatabase.GetCount("sp_Tour_GetNewTourExistsByTourName", "@UserId", Utility.UserId, "@Name", name) != 0;
	}

	public static bool TourNameInUse(int tourId, string name)
	{
		// This method passes a tour Id to avoid comparing its own name.
		return MapsAliveDatabase.GetCount("sp_Tour_GetTourExistsByTourName", "@UserId", Utility.UserId, "@TourId", tourId, "@Name", name) != 0;
	}

	public void InsertIntoDatabase()
	{
		banner = new Banner(this);

		id = (int)MapsAliveDatabase.ReadScalar("sp_Tour_CreateTour",
			"@UserId", Utility.UserId,
			"@ThemeId", ThemeId,
			"@MajorVersion", MajorVersion,
			"@MinorVersion", MinorVersion
		);

		UpdateDatabase();

		MapsAliveState.Flush(MapsAliveObjectType.TourList);
	}

	public void RebuildTourTreeXml()
	{
		// Rebuild this tour's tree XML from the database.
		tourTreeXml = (string)MapsAliveDatabase.ReadScalar("sp_Tour_RebuildTourTreeXml", "@TourId", this.Id, "@ThemeId", ThemeId);
	}

	public void SwitchColorScheme()
	{
		SetColorSchemeChanged();
		UpdateDatabaseColorScheme();

		ColorScheme.SynchronizeColorsForDirectory(this);
		ColorScheme.SynchronizeColorsForPopup(this);
	}

	public bool TourIsWebAppCapable
	{
		get { return (runtimeTarget & Tour.MapViewerFlags.WebAppCapable) != 0; }
	}

	public static bool TourExists(int tourId)
	{
		MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow("sp_Tour_GetTourByTourId", "@TourId", tourId, "@ThemeId", 1);
		return row != null;
	}

	public ColorScheme ColorScheme
	{
		get
		{
			if (colorScheme == null)
				colorScheme = Account.GetCachedColorScheme(colorSchemeId);
			return colorScheme;
		}
		set
		{
			colorScheme = value;
			colorSchemeId = colorScheme.Id;
		}
	}

	public void UpdateDatabase()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateTour",
			"@TourId",					Id,
			"@ThemeId",					ThemeId,
			"@Name",					Name,
			"@PageWidth",				TourSize.Width,
			"@PageHeight",				TourSize.Height,
			"@AutoWidth",				MaxTourSize.Width,
			"@AutoHeight",				MaxTourSize.Height,
			"@WidthType",				WidthType,
			"@HeightType",				HeightType,
			"@AutoLayoutEnabled",		AutoLayoutEnabled,
			"@NavigationId",			MenuLocationId,
			"@FontSchemeId",			FontSchemeId,
			"@TourStyleId",				colorSchemeId,
			"@BodyBackgroundColor",		BodyBackgroundColor,
			"@BodyMargin",				BodyMargin,
			"@MenuStyleId",				MenuStyleId,
			"@MenuWidth",				MenuWidth,
			"@MenuScrolls",				MenuScrolls,
			"@HasPageTitle",			HasTitle,
			"@HasHeaderStripe",			HasHeaderStripe,
			"@HasFooterStripe",			HasFooterStripe,
			"@BrowserTitle",			BrowserTitle,
			"@CustomFooter",			CustomFooter,
			"@HasBanner",				HasBanner,
			"@HasDirectory",			HasDirectory,
			"@StartPageId",				FirstPageId,
			"@EmitUnbrandedPages",		CanAppearUnbranded,
			"@EmitTourXml",				ExportTourData,
			"@UseSoundManager",			UseSoundManager,
			"@IsPrivate",				IsPrivate,
			"@RemoteImportUrl",			RemoteImportUrl,
			"@CustomHtmlAbsolute",		CustomHtmlAbsolute,
			"@CustomHtmlBottom",		CustomHtmlBottom,
			"@CustomHtmlCss",			CustomHtmlCss,
			"@CustomHtmlJavaScript",	CustomHtmlJavaScript,
			"@CustomHtmlTop",			CustomHtmlTop,
			"@LeftAlignedInBrowser",	LeftAlignedInBrowser,
			"@RuntimeTarget",			RuntimeTarget,
			"@ChangeFlags",				(int)changed
		);
	}

	public void UpdateDatabaseColorScheme()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateTourStyleIdAndFlags",
			"@TourId", id,
			"@TourStyleId", ColorScheme.Id,
			"@ChangeFlags", (int)changed
		);
	}

	public void UpdateNextPageId()
	{
		// This method gets called following an import to set the database's NextPageId value
		// based on the imported tour's page numbering. We have to do this in case there is a
		// gap in the sequence due to a page(s) having been deleted in the original tour. If we
		// don't update the Id, the next page that gets added to this tour could have the page
		// number for an existing page because as we have added new pages to the imported tour,
		// the database copy of NextPageId has simply been the page count.
		int nextPageId = 0;
		foreach (TourPage tourPage in TourPages)
		{
			if (tourPage.PageNumber > nextPageId)
				nextPageId = tourPage.PageNumber;
		}

		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateNextPageId", "@TourId", id, "@NextPageId", nextPageId);
	}

	public bool UseTouchUiOnDesktop
	{
		get { return (runtimeTarget & Tour.MapViewerFlags.UseTouchUiOnDesktop) != 0; }
	}

	public bool VersionLessThan(int majorVersion, int minorVersion)
	{
		if (this.majorVersion == 0)
			return false;

		if (this.majorVersion < majorVersion)
			return true;
		
		if (this.majorVersion > majorVersion)
			return false;
		
		return this.minorVersion < minorVersion;
	}
	
	public MemoryStream XmlForPage(int pageId, TourBuilder tourBuilder)
	{
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		settings.IndentChars = ("\t");
		settings.OmitXmlDeclaration = true;

		MemoryStream xmlMemoryStream = new MemoryStream();

		using (XmlWriter xmlWriter = XmlWriter.Create(xmlMemoryStream, settings))
		{
			xmlWriter.WriteStartDocument();
			TourPageXmlWriter tourPageXmlWriter = new TourPageXmlWriter(xmlWriter, this, tourBuilder);
			tourPageXmlWriter.CreateTourXmlForPage(pageId);
			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();
		}

		xmlMemoryStream.Position = 0;

		if (App.DeveloperMode)
		{
			string dumpFileLocation = FileManager.PreviewFolderLocationAbsolute(id) + string.Format("\\_dump{0}.xml", pageId);
			using (System.IO.FileStream fileStream = new System.IO.FileStream(dumpFileLocation, System.IO.FileMode.Create))
			{
				xmlMemoryStream.WriteTo(fileStream);
				fileStream.Flush();
				fileStream.Close();
			}
		}

		return xmlMemoryStream;
	}
	#endregion

	#region ===== Protected =========================================================
	#endregion

	#region ===== Private ===========================================================

	private static Hashtable CacheTourOptions()
	{
		// Create a hash table of DataTable objects that each contain a list of option values.
		// Because these values rarely change, we can cache them the first time they are requested
		// and retrieve them from the cache after that.

		Hashtable tourOptions = new Hashtable();

		tourOptions.Add((int)TourOption.ColorScheme, DataTableForTourOption(TourOption.ColorScheme));
		tourOptions.Add((int)TourOption.FontScheme, DataTableForTourOption(TourOption.FontScheme));
		tourOptions.Add((int)TourOption.Navigation, DataTableForTourOption(TourOption.Navigation));
		tourOptions.Add((int)TourOption.MenuStyle, DataTableForTourOption(TourOption.MenuStyle));

		MapsAliveState.Persist(MapsAliveObjectType.TourOptions, tourOptions);
		
		return tourOptions;
	}

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

	private static DataTable DataTableForTourOption(TourOption option)
	{
		string sp = null;

		switch (option)
		{
			case TourOption.ColorScheme:
				sp = "sp_TourStyle_GetDescriptions";
				break;

			case TourOption.FontScheme:
				sp = "sp_TourFontScheme_GetDescriptions";
				break;

			case TourOption.Navigation:
				sp = "sp_TourNavigation_GetDescriptions";
				break;

			case TourOption.MenuStyle:
				sp = "sp_TourMenuStyle_GetDescriptions";
				break;

			default:
				Debug.Fail("Unrecognized TourOption " + option.ToString());
				break;
		}

		DataTable dataTable = MapsAliveDatabase.LoadDataTable(sp);
		MapsAliveDatabase.ReportDatabaseErrorIf(dataTable.Rows.Count == 0, "No rows returned from " + sp);
		return dataTable;
	}

	private void PageCountChanged(int change)
	{
        // This logic is not needed for V4 tours because they don't have a menu bar.
        if (V4)
            return;

        if (menuLocationId != (int)Tour.MenuLocation.AutoTop)
			return;

		int newCount = TourPages.Count;
		int oldCount = newCount - change;

		if (oldCount == 1 && newCount == 2 || oldCount == 2 && newCount == 1)
		{
			// The page count and switched between 1 and 2 which means the top menu
			// has either been added or removed. Update all the page layouts to
			// account for the presence or absence of the menu.
			UpdatePageLayouts();
		}
	}

	private static DataTable TourOptionDataTable(TourOption option)
	{
		Hashtable tourOptions = (Hashtable)MapsAliveState.Retrieve(MapsAliveObjectType.TourOptions);
		if (tourOptions == null)
			tourOptions = CacheTourOptions();
		return (DataTable)tourOptions[(int)option];
	}

	public void UpdateAppVersionInDatabase()
	{
		// Update the version in this Tour object...
		majorVersion = App.MajorVersion;
		minorVersion = App.MinorVersion;

		// ...and in the database.
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateVersion",
			"@TourId", id,
			"@MajorVersion", majorVersion,
			"@MinorVersion", minorVersion
		);
	}

	public void UpdateChangeFlagsInDatabase()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateChangeFlags",
			"@TourId", Id,
			"@ChangeFlags", (int)changed
		);
	}

	public void UpdatePageLayouts()
	{
        foreach (TourPage tourPage in TourPages)
		{
            tourPage.RebuildMap();

			tourPage.LayoutManager.PerformAutoLayoutForTourOptionChanges();
			tourPage.InvalidateThumbnail();
		}
	}

	public bool VersionNoLongerSupport()
	{
		return VersionLessThan(1, 38);
	}

    #endregion
}
