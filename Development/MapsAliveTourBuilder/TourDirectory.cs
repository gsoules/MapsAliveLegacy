// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;

// These values are known in the DB -- do not change.
// Location options that are new to V4 start at 10. Custom, MapLeft, and TopMenu are not used
// in V4. It's like this instead of having a separate enum for left/center/right to avoid having
// to add a new column to the DB. V3 has the alignContentRight flag, but since it can only store
// two values, it could not be used for this purpose and so is no longer used in V4, but could be
// use for something else if a new V4 directory flag is needed in the future.
public enum TourDirectoryLocation
{
	Custom = 1,
	MapLeft = 2,
	MapRight = 3,
	TitleBar = 4,
	TopMenu = 5,
    BannerLeft = 10,
    BannerCenter = 11,
    BannerRight = 12,
    AboveLeft = 13,
    AboveCenter = 14,
    AboveRight = 15
}

public partial class TourDirectory
{
	private bool alignContentRight;
	private bool autoCollapse;
	private string backgroundColor;
	private string borderColor;
	private int contentWidth;
	private string entryCountColor;
	private string entryTextColor;
	private string entryTextHoverColor;
	private bool groupByCategory;
	private bool groupByCategoryThenPage;
	private bool groupByPage;
	private bool groupByPageThenCategory;
	private string level1TextColor;
	private string level2TextColor;
	private TourDirectoryLocation location;
	private int locationX;
	private int locationY;
	private int maxHeight;
	private bool openExpanded;
	private string previewBackgroundColor;
	private string previewBorderColor;
	private string previewImageBorderColor;
	private int previewImageWidth;
	private bool previewOnRight;
	private string previewTextColor;
	private int previewWidth;
	private string searchResultBackgroundColor;
	private string searchResultTextColor;
	private bool showAllPages;
	private bool showImagePreview;
	private bool showSearch;
	private bool showTextPreview;
	private bool staysOpen;
	private string statusBackgroundColor;
	private string statusTextColor;
	private string textTitle;
	private string textAlphaSortTooltip;
	private string textClearButtonLabel;
	private string textGroupSortTooltip;
	private string textItemsShowingMessage;
	private string textSearchLabel;
	private string textSearchResultsMessage;
	private string titleBarColor;
	private int titleBarWidth;
	private string titleTextColor;
	private Tour tour;
	private bool useColorSchemeColors;

	public TourDirectory(Tour tour, bool loadFromDatabase)
	{
		this.tour = tour;
		if (loadFromDatabase)
			LoadFromDatabase();
		else
			LoadDefaults();
	}

	private void LoadDefaults()
	{
		alignContentRight = false;
		backgroundColor = "#ffffff";
		borderColor = "#cccccc";
		autoCollapse = true;
		contentWidth = 360;
		entryCountColor = "#555555";
		entryTextColor = "#000000";
		entryTextHoverColor = tour.V3CompatibilityEnabled ? "#ff0000" : "#777777";
		groupByCategory = false;
		groupByCategoryThenPage = false;
		groupByPage = true;
		groupByPageThenCategory = false;
		level1TextColor = "#333333";
		level2TextColor = "#555555";
		location = TourDirectoryLocation.TitleBar;
		locationX = 0;
		locationY = 0;
		maxHeight = 400;
		openExpanded = true;
		previewBackgroundColor = tour.V3CompatibilityEnabled ? "#eeeeee" : "#ffffff";
		previewBorderColor = "#cccccc";
		previewTextColor = "#000000";
		previewImageBorderColor = "#000000";
		previewWidth = 220;
		previewImageWidth = 220;
		previewOnRight = false;
		searchResultBackgroundColor = "#ffff00";
		searchResultTextColor = "#000000";
		showAllPages = true;
		showImagePreview = true;
		showSearch = true;
		showTextPreview = true;
		staysOpen = false;
		statusBackgroundColor = "#eeeeee";
		statusTextColor = "#000077";
		titleBarColor = "#eeeeee";
		titleBarWidth = 200;
		titleTextColor = "#333333";
		textTitle = "Directory";
		textAlphaSortTooltip = tour.V3CompatibilityEnabled ? "Sort Alphabetically" : DefaultAlphaSort;
		textGroupSortTooltip = "Show By Group";
		textItemsShowingMessage = "Items showing";
		textSearchLabel = tour.V3CompatibilityEnabled ? "Search: " : "Search";
        textClearButtonLabel = "Clear";
		textSearchResultsMessage = "Matches on";
		
		useColorSchemeColors = tour.V3CompatibilityEnabled ? true : false;

        if (tour.V4 && MapsAliveState.Account.IsPersonalPlan)
        {
            showAllPages = false;
            showImagePreview = false;
            showTextPreview = false;
            showSearch = false;
        }
    }

	private void LoadFromDatabase()
	{
		MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow("sp_TourDirectory_GetAllByTourId", "@TourId", tour.Id, "@ThemeId", tour.ThemeId);
		
		if (row == null)
		{
			// The tour does not have directory data yet. Create it. 
			LoadDefaults();
			InsertTourDirectoryIntoDatabase();
			UpdateDatabase();
			return;
		}

		InitializeDirectoryFromDataRecord(row);
	}

    public void InitializeDirectoryFromDataRecord(MapsAliveDataRecord record)
    {
        alignContentRight = record.BoolValue(Tag.alignContentRight);
        autoCollapse = record.BoolValue(Tag.autoCollapse);
        backgroundColor = record.StringValue(Tag.backgroundColor);
        borderColor = record.StringValue(Tag.borderColor);
        contentWidth = record.IntValue(Tag.contentWidth);
        entryCountColor = record.StringValue(Tag.entryCountColor);
        entryTextColor = record.StringValue(Tag.entryTextColor);
        entryTextHoverColor = record.StringValue(Tag.entryTextHoverColor);
        groupByCategory = record.BoolValue(Tag.groupByCategory);
        groupByCategoryThenPage = record.BoolValue(Tag.groupByCategoryThenPage);
        groupByPage = record.BoolValue(Tag.groupByPage);
        groupByPageThenCategory = record.BoolValue(Tag.groupByPageThenCategory);
        level1TextColor = record.StringValue(Tag.level1TextColor);
        level2TextColor = record.StringValue(Tag.level2TextColor);
        location = (TourDirectoryLocation)record.IntValue(Tag.location);
        locationX = record.IntValue(Tag.locationX);
        locationY = record.IntValue(Tag.locationY);
        maxHeight = record.IntValue(Tag.maxHeight);
        openExpanded = record.BoolValue(Tag.openExpanded);
        previewBackgroundColor = record.StringValue(Tag.previewBackgroundColor);
        previewBorderColor = record.StringValue(Tag.previewBorderColor);
        previewImageBorderColor = record.StringValue(Tag.previewImageBorderColor);
        previewImageWidth = record.IntValue(Tag.previewImageWidth);
        previewOnRight = record.BoolValue(Tag.previewOnRight);
        previewTextColor = record.StringValue(Tag.previewTextColor);
        previewWidth = record.IntValue(Tag.previewWidth);
        searchResultBackgroundColor = record.StringValue(Tag.searchResultBackgroundColor);
        searchResultTextColor = record.StringValue(Tag.searchResultTextColor);
        showAllPages = record.BoolValue(Tag.showAllPages);
        showImagePreview = record.BoolValue(Tag.showImagePreview);
        showSearch = record.BoolValue(Tag.showSearch);
        showTextPreview = record.BoolValue(Tag.showTextPreview);
        staysOpen = record.BoolValue(Tag.staysOpen);
        statusBackgroundColor = record.StringValue(Tag.statusBackgroundColor);
        statusTextColor = record.StringValue(Tag.statusTextColor);
        textAlphaSortTooltip = record.StringValue("AlphaSortTooltip", Tag.textAlphaSortTooltip);
        textClearButtonLabel = record.StringValue("ClearButtonLabel", Tag.textClearButtonLabel);
        textGroupSortTooltip = record.StringValue("GroupSortTooltip", Tag.textGroupSortTooltip);
        textItemsShowingMessage = record.StringValue("NoSearchMessage", Tag.textItemsShowingMessage);
        textSearchLabel = record.StringValue("SearchLabel", Tag.textSearchLabel);
        textSearchResultsMessage = record.StringValue("SearchResultsMessage");
        textTitle = record.StringValue("Title", Tag.textTitle);
        titleBarColor = record.StringValue(Tag.titleBarColor);
        titleBarWidth = record.IntValue(Tag.titleBarWidth);
        titleTextColor = record.StringValue(Tag.titleTextColor);
        useColorSchemeColors = record.BoolValue("UseTourStyleColors", Tag.useColorSchemeColors);

        if ((tour.MajorVersion == 3 || App.DeveloperMode) && tour.V4)
        {
            // This tour was built with V3 and is being opened in V4 for the first time.
            // Update the database to set some V3 fields to use V4 default values;

            if (textAlphaSortTooltip == "Sort Alphabetically")
                textAlphaSortTooltip = DefaultAlphaSort;

            if (textSearchLabel == "Search: ")
                textSearchLabel = "Search";

            if (contentWidth < 300)
                contentWidth = 300;

            // Convert a V3 directory location that is not supported as a V4 nav button location to a valid V4 location.
            bool changeLocationToTitle = false;
            if (location == TourDirectoryLocation.Custom || location == TourDirectoryLocation.TopMenu)
                changeLocationToTitle = true;
            else if ((tour.HasDataSheet || tour.HasGallery) && (location == TourDirectoryLocation.MapLeft || location == TourDirectoryLocation.MapRight))
                changeLocationToTitle = true;
            
            if (changeLocationToTitle)
                location = TourDirectoryLocation.TitleBar;

            UpdateDatabase();
        }
    }

    public enum Tag
	{
		alignContentRight,
		autoCollapse,
		backgroundColor,
		borderColor,
		contentWidth,
		entryCountColor,
		entryTextColor,
		entryTextHoverColor,
		groupByCategory,
		groupByCategoryThenPage,
		groupByPage,
		groupByPageThenCategory,
		level1TextColor,
		level2TextColor,
		location,
		locationX,
		locationY,
		maxHeight,
		openExpanded,
		previewBackgroundColor,
		previewBorderColor,
		previewImageBorderColor,
		previewImageWidth,
		previewOnRight,
		previewTextColor,
		previewWidth,
		searchResultBackgroundColor,
		searchResultTextColor,
		showAllPages,
		showImagePreview,
		showSearch,
		showTextPreview,
		staysOpen,
		statusBackgroundColor,
		statusTextColor,
		textAlphaSortTooltip,
		textClearButtonLabel,
		textGroupSortTooltip,
		textItemsShowingMessage,
		textSearchLabel,
		textSearchResultsMessage,
		textTitle,
		titleBarColor,
		titleBarWidth,
		titleTextColor,
		useColorSchemeColors
	}

	public string GetTagValue(int tagId)
	{
		Tag tag = (Tag)tagId;

		switch (tag)
		{
			case Tag.alignContentRight:
				return AlignContentRight.ToString();

			case Tag.autoCollapse:
				return AutoCollapse.ToString();

			case Tag.backgroundColor:
				return BackgroundColor;

			case Tag.borderColor:
				return BorderColor;

			case Tag.contentWidth:
				return ContentWidth.ToString();

			case Tag.entryCountColor:
				return EntryCountColor;

			case Tag.entryTextColor:
				return EntryTextColor;

			case Tag.entryTextHoverColor:
				return EntryTextHoverColor;

			case Tag.groupByCategory:
				return GroupByCategory.ToString();

			case Tag.groupByCategoryThenPage:
				return GroupByCategoryThenPage.ToString();

			case Tag.groupByPage:
				return GroupByPage.ToString();

			case Tag.groupByPageThenCategory:
				return groupByPageThenCategory.ToString();

			case Tag.level1TextColor:
				return Level1TextColor;

			case Tag.level2TextColor:
				return Level2TextColor;

			case Tag.location:
				return ((int)Location).ToString();

			case Tag.locationX:
				return LocationX.ToString();

			case Tag.locationY:
				return LocationY.ToString();

			case Tag.maxHeight:
				return MaxHeight.ToString();

			case Tag.openExpanded:
				return OpenExpanded.ToString();

			case Tag.previewBackgroundColor:
				return PreviewBackgroundColor;

			case Tag.previewBorderColor:
				return PreviewBorderColor;

			case Tag.previewImageBorderColor:
				return PreviewImageBorderColor;

			case Tag.previewImageWidth:
				return PreviewImageWidth.ToString();

			case Tag.previewOnRight:
				return PreviewOnRight.ToString();

			case Tag.previewTextColor:
				return PreviewTextColor;

			case Tag.previewWidth:
				return PreviewWidth.ToString();

			case Tag.searchResultBackgroundColor:
				return SearchResultBackgroundColor;

			case Tag.searchResultTextColor:
				return SearchResultTextColor;

			case Tag.showAllPages:
				return ShowAllPages.ToString();

			case Tag.showImagePreview:
				return ShowImagePreview.ToString();

			case Tag.showSearch:
				return ShowSearch.ToString();

			case Tag.showTextPreview:
				return ShowTextPreview.ToString();

			case Tag.staysOpen:
				return StaysOpen.ToString();

			case Tag.statusBackgroundColor:
				return StatusBackgroundColor;

			case Tag.statusTextColor:
				return StatusTextColor;

			case Tag.textAlphaSortTooltip:
				return TextAlphaSortTooltip;

			case Tag.textClearButtonLabel:
				return TextClearButtonLabel;

			case Tag.textGroupSortTooltip:
				return TextGroupSortTooltip;

			case Tag.textItemsShowingMessage:
				return TextNoSearchMessage;

			case Tag.textSearchLabel:
				return TextSearchLabel;

			case Tag.textSearchResultsMessage:
				return TextSearchResultsMessage;

			case Tag.textTitle:
				return TextTitle;

			case Tag.titleBarColor:
				return TitleBarColor;

			case Tag.titleBarWidth:
				return TitleBarWidth.ToString();

			case Tag.titleTextColor:
				return TitleTextColor;

			case Tag.useColorSchemeColors:
				return UseColorSchemeColors.ToString();

			default:
				Debug.Fail("Unsupported TourDirectory XML tag requested " + tag);
				return "???";
		}
	}

	public bool AlignContentRight
	{
		get { return alignContentRight; }
		set
		{
			if (alignContentRight != value)
			{
				tour.SetDirectoryChanged();
				alignContentRight = value;
			}
		}
	}

	public bool AutoCollapse
	{
		get { return autoCollapse; }
		set
		{
			if (autoCollapse != value)
			{
				tour.SetDirectoryChanged();
				autoCollapse = value;
			}
		}
	}

    private string DefaultAlphaSort
    {
        get { return "A-Z"; }
    }

	public string BackgroundColor
	{
		get { return backgroundColor; }
		set
		{
			if (backgroundColor != value)
			{
				tour.SetDirectoryChanged();
				backgroundColor = value;
			}
		}
	}

	public string BorderColor
	{
		get { return borderColor; }
		set
		{
			if (borderColor != value)
			{
				tour.SetDirectoryChanged();
				borderColor = value;
			}
		}
	}

	public int ContentWidth
	{
		get { return contentWidth; }
		set
		{
			if (contentWidth != value)
			{
				tour.SetDirectoryChanged();
				contentWidth = value;
			}
		}
	}

	public string EntryCountColor
	{
		get { return entryCountColor; }
		set
		{
			if (entryCountColor != value)
			{
				tour.SetDirectoryChanged();
				entryCountColor = value;
			}
		}
	}

	public int EntryDepth
	{
		get
		{
			if (groupByPageThenCategory || (groupByCategoryThenPage && (showAllPages || tour.V4)))
				return 3;
			else if (groupByCategory || groupByPage)
				return 2;
			else
				return 1;
		}
	}

	public string EntryTextColor
	{
		get { return entryTextColor; }
		set
		{
			if (entryTextColor != value)
			{
				tour.SetDirectoryChanged();
				entryTextColor = value;
			}
		}
	}

	public string EntryTextHoverColor
	{
		get { return entryTextHoverColor; }
		set
		{
			if (entryTextHoverColor != value)
			{
				tour.SetDirectoryChanged();
				entryTextHoverColor = value;
			}
		}
	}

	public bool GroupByCategory
	{
		get { return groupByCategory; }
		set
		{
			if (groupByCategory != value)
			{
				tour.SetDirectoryChanged();
				groupByCategory = value;
			}
		}
	}

	public bool GroupByCategoryThenPage
	{
		get { return groupByCategoryThenPage; }
		set
		{
			if (groupByCategoryThenPage != value)
			{
				tour.SetDirectoryChanged();
				groupByCategoryThenPage = value;
			}
		}
	}

	public bool GroupByPage
	{
		get { return groupByPage; }
		set
		{
			if (groupByPage != value)
			{
				tour.SetDirectoryChanged();
				groupByPage = value;
			}
		}
	}

	public bool GroupByPageThenCategory
	{
		get { return groupByPageThenCategory; }
		set
		{
			if (groupByPageThenCategory != value)
			{
				tour.SetDirectoryChanged();
				groupByPageThenCategory = value;
			}
		}
	}

	public string Level1TextColor
	{
		get { return level1TextColor; }
		set
		{
			if (level1TextColor != value)
			{
				tour.SetDirectoryChanged();
				level1TextColor = value;
			}
		}
	}

	public string Level2TextColor
	{
		get { return level2TextColor; }
		set
		{
			if (level2TextColor != value)
			{
				tour.SetDirectoryChanged();
				level2TextColor = value;
			}
		}
	}

	public TourDirectoryLocation Location
	{
		get
        {
            return location;
        }
		set
		{
			if (location != value)
			{
				tour.SetDirectoryChanged();
				location = value;
			}
		}
	}
	
	public int LocationX
	{
		get { return locationX; }
		set
		{
			if (locationX != value)
			{
				tour.SetDirectoryChanged();
				locationX = value;
			}
		}
	}

	public int LocationY
	{
		get { return locationY; }
		set
		{
			if (locationY != value)
			{
				tour.SetDirectoryChanged();
				locationY = value;
			}
		}
	}

	public int MaxHeight
	{
		get { return maxHeight; }
		set
		{
			if (maxHeight != value)
			{
				tour.SetDirectoryChanged();
				maxHeight = value;
			}
		}
	}

	public bool OpenExpanded
	{
		get { return openExpanded; }
		set
		{
			if (openExpanded != value)
			{
				tour.SetDirectoryChanged();
				openExpanded = value;
			}
		}
	}

	public string PreviewBackgroundColor
	{
		get { return previewBackgroundColor; }
		set
		{
			if (previewBackgroundColor != value)
			{
				tour.SetDirectoryChanged();
				previewBackgroundColor = value;
			}
		}
	}

	public string PreviewBorderColor
	{
		get { return previewBorderColor; }
		set
		{
			if (previewBorderColor != value)
			{
				tour.SetDirectoryChanged();
				previewBorderColor = value;
			}
		}
	}

	public string PreviewImageBorderColor
	{
		get { return previewImageBorderColor; }
		set
		{
			if (previewImageBorderColor != value)
			{
				tour.SetDirectoryChanged();
				previewImageBorderColor = value;
			}
		}
	}

	public int PreviewImageWidth
	{
		get { return previewImageWidth; }
		set
		{
			if (previewImageWidth != value)
			{
				tour.SetDirectoryChanged();
				previewImageWidth = value;
			}
		}
	}

	public bool PreviewOnRight
	{
		get { return previewOnRight; }
		set
		{
			if (previewOnRight != value)
			{
				tour.SetDirectoryChanged();
				previewOnRight = value;
			}
		}
	}

	public string PreviewTextColor
	{
		get { return previewTextColor; }
		set
		{
			if (previewTextColor != value)
			{
				tour.SetDirectoryChanged();
				previewTextColor = value;
			}
		}
	}

	public int PreviewWidth
	{
		get { return previewWidth; }
		set
		{
			if (previewWidth != value)
			{
				tour.SetDirectoryChanged();
				previewWidth = value;
			}
		}
	}

	public bool ShowAllPages
	{
		get { return showAllPages; }
		set
		{
			if (showAllPages != value)
			{
				tour.SetDirectoryChanged();
				showAllPages = value;
			}
		}
	}

	public string SearchResultBackgroundColor
	{
		get { return searchResultBackgroundColor; }
		set
		{
			if (searchResultBackgroundColor != value)
			{
				tour.SetDirectoryChanged();
				searchResultBackgroundColor = value;
			}
		}
	}

	public string SearchResultTextColor
	{
		get { return searchResultTextColor; }
		set
		{
			if (searchResultTextColor != value)
			{
				tour.SetDirectoryChanged();
				searchResultTextColor = value;
			}
		}
	}
	
	public bool ShowImagePreview
	{
		get { return showImagePreview; }
		set
		{
			if (showImagePreview != value)
			{
				tour.SetDirectoryChanged();
				showImagePreview = value;
			}
		}
	}

	public bool ShowSearch
	{
		get { return showSearch; }
		set
		{
			if (showSearch != value)
			{
				tour.SetDirectoryChanged();
				showSearch = value;
			}
		}
	}
	
	public bool ShowTextPreview
	{
		get { return showTextPreview; }
		set
		{
			if (showTextPreview != value)
			{
				tour.SetDirectoryChanged();
				showTextPreview = value;
			}
		}
	}

	public bool StaysOpen
	{
		get { return staysOpen; }
		set
		{
			if (staysOpen != value)
			{
				tour.SetDirectoryChanged();
				staysOpen = value;
			}
		}
	}

	public string StatusBackgroundColor
	{
		get { return statusBackgroundColor; }
		set
		{
			if (statusBackgroundColor != value)
			{
				tour.SetDirectoryChanged();
				statusBackgroundColor = value;
			}
		}
	}

	public string StatusTextColor
	{
		get { return statusTextColor; }
		set
		{
			if (statusTextColor != value)
			{
				tour.SetDirectoryChanged();
				statusTextColor = value;
			}
		}
	}

	public string TextTitle
	{
		get { return textTitle; }
		set
		{
			if (textTitle != value)
			{
				tour.SetDirectoryChanged();
				textTitle = value;
			}
		}
	}

	public string TextAlphaSortTooltip
	{
		get { return textAlphaSortTooltip; }
		set
		{
			if (textAlphaSortTooltip != value)
			{
				tour.SetDirectoryChanged();
				textAlphaSortTooltip = value;
			}
		}
	}

	public string TextClearButtonLabel
	{
		get { return textClearButtonLabel; }
		set
		{
			if (textClearButtonLabel != value)
			{
				tour.SetDirectoryChanged();
				textClearButtonLabel = value;
			}
		}
	}

	public string TextGroupSortTooltip
	{
		get { return textGroupSortTooltip; }
		set
		{
			if (textGroupSortTooltip != value)
			{
				tour.SetDirectoryChanged();
				textGroupSortTooltip = value;
			}
		}
	}

	public string TextNoSearchMessage
	{
		get { return textItemsShowingMessage; }
		set
		{
			if (textItemsShowingMessage != value)
			{
				tour.SetDirectoryChanged();
				textItemsShowingMessage = value;
			}
		}
	}

	public string TextSearchLabel
	{
		get { return textSearchLabel; }
		set
		{
			if (textSearchLabel != value)
			{
				tour.SetDirectoryChanged();
				textSearchLabel = value;
			}
		}
	}

	public string TextSearchResultsMessage
	{
		get { return textSearchResultsMessage; }
		set
		{
			if (textSearchResultsMessage != value)
			{
				tour.SetDirectoryChanged();
				textSearchResultsMessage = value;
			}
		}
	}

	public string TitleBarColor
	{
		get { return titleBarColor; }
		set
		{
			if (titleBarColor != value)
			{
				tour.SetDirectoryChanged();
				titleBarColor = value;
			}
		}
	}

	public int TitleBarWidth
	{
		get { return titleBarWidth; }
		set
		{
			if (titleBarWidth != value)
			{
				tour.SetDirectoryChanged();
				titleBarWidth = value;
			}
		}
	}

	public string TitleTextColor
	{
		get { return titleTextColor; }
		set
		{
			if (titleTextColor != value)
			{
				tour.SetDirectoryChanged();
				titleTextColor = value;
			}
		}
	}

	public bool UseColorSchemeColors
	{
		get { return useColorSchemeColors; }
		set
		{
			if (useColorSchemeColors != value)
			{
				tour.SetDirectoryChanged();
				useColorSchemeColors = value;
			}
		}
	}

	public void InsertTourDirectoryIntoDatabase()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourDirectory_CreateTourDirectory",
			"@TourId", tour.Id,
			"@ThemeId", tour.ThemeId);
	}

	public void UpdateDatabase()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourDirectory_UpdateTourDirectory",
			"@TourId", tour.Id,
			"@AlignContentRight", alignContentRight,
			"@AutoCollapse", autoCollapse,
			"@BackgroundColor", backgroundColor,
			"@BorderColor", borderColor,
			"@ContentWidth", contentWidth,
			"@EntryCountColor", entryCountColor,
			"@EntryTextColor", entryTextColor,
			"@EntryTextHoverColor", entryTextHoverColor,
			"@GroupByCategory", groupByCategory,
			"@GroupByCategoryThenPage", groupByCategoryThenPage,
			"@GroupByPage", groupByPage,
			"@GroupByPageThenCategory", groupByPageThenCategory,
			"@Level1TextColor", level1TextColor,
			"@Level2TextColor", level2TextColor,
			"@Location", (int)location,
			"@LocationX", locationX,
			"@LocationY", locationY,
			"@MaxHeight", maxHeight,
			"@OpenExpanded", openExpanded,
			"@PreviewBackgroundColor", previewBackgroundColor,
			"@PreviewBorderColor", previewBorderColor,
			"@PreviewOnRight", PreviewOnRight,
			"@PreviewTextColor", previewTextColor,
			"@PreviewImageBorderColor", previewImageBorderColor,
			"@PreviewImageWidth", previewImageWidth,
			"@PreviewWidth", previewWidth,
			"@TitleBarColor", titleBarColor,
			"@TitleBarWidth", titleBarWidth,
			"@TitleTextColor", titleTextColor,
			"@SearchResultBackgroundColor", searchResultBackgroundColor,
			"@SearchResultTextColor", searchResultTextColor,
			"@ShowAllPages", showAllPages,
			"@ShowImagePreview", showImagePreview,
			"@ShowSearch", showSearch,
			"@ShowTextPreview", showTextPreview,
			"@StaysOpen", staysOpen,
			"@StatusBackgroundColor", statusBackgroundColor,
			"@StatusTextColor", statusTextColor,
			"@ThemeId", tour.ThemeId,
			"@AlphaSortTooltip", textAlphaSortTooltip,
			"@ClearButtonLabel", textClearButtonLabel,
			"@GroupSortTooltip", textGroupSortTooltip,
			"@NoSearchMessage", textItemsShowingMessage,
			"@SearchLabel", textSearchLabel,
			"@SearchResultsMessage", textSearchResultsMessage,
			"@Title", textTitle,
			"@UseTourStyleColors", useColorSchemeColors
		);
	}
}
