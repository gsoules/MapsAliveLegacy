// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Drawing;
using System.Web.UI.WebControls;

public partial class Members_DirectoryOptions : MemberPage
{
	private TourDirectory directory;
	private bool optionsChanged;
    private bool showBasicOptions;
	private string validBackgroundColor;
	private string validBorderColor;
	private int validContentWidth;
	private int validContentHeight;
	private int validCustomLocationX;
	private int validCustomLocationY;
	private string validLevel1TextColor;
	private string validLevel2TextColor;
	private string validEntryCountColor;
	private string validEntryTextColor;
	private string validEntryTextHoverColor;
	private string validPreviewBackgroundColor;
	private string validPreviewBorderColor;
	private string validPreviewImageBorderColor;
	private int validPreviewImageWidth;
	private string validPreviewTextColor;
	private int validPreviewWidth;
	private string validSearchResultsBackgroundColor;
	private string validSearchResultsTextColor;
	private string validStatusBackgroundColor;
	private string validStatusTextColor;
	private string validTitleBarColor;
	private int validTitleBarWidth;
	private string validTitleTextColor;

	protected void AddChangeDetectionForPreview(CheckBox checkBox, string script)
	{
		checkBox.Attributes.Add("onclick", script);
	}

	protected override void InitControls(bool undo)
	{
		TourDirectoryLocation location = directory.Location;
		
		if (!undo && IsPostBack)
			return;

        if (showBasicOptions)
        {
            BasicOptionsPanel.Visible = true;
            ShowDirectoryBasicCheckBox.Checked = tour.HasDirectory;
            AddChangeDetection(ShowDirectoryBasicCheckBox);
            
            if (tour.HasMoreThanOnePage)
            {
                ShowMenuBasicCheckBox.Checked = !tour.HideMenu;
                AddChangeDetection(ShowMenuBasicCheckBox);
            }
            else
            {
                ShowMenuBasicCheckBox.Checked = false;
                ShowMenuBasicCheckBox.Enabled = false;
            }

            return;
        }

        AllOptionsPanel.Visible = true;
        
        if (tour.V4)
        {
            TitleTextColorSwatch.Visible = false;
            CustomLocationPanel.Visible = false;
            AlignContentRightPanel.Visible = false;
            PreviewOnRightPanel.Visible = false;
            TitleOption.Visible = false;
            TitleBarWidthOption.Visible = false;
            SearchResultsMessageOption.Visible = false;
            ItemsMessageOption.Visible = false;
            ClearButtonOption.Visible = false;
            UseColorSchemeOption.Visible = false;
            
            BackgroundColorSwatch.Visible = false;
            TitleBarColorSwatch.Visible = false;
            BorderColorSwatch.Visible = false;
            PreviewBorderColorSwatch.Visible = false;
            StatusTextColorSwatch.Visible = false;
            SearchResultsTextColorSwatch.Visible = false;
            StatusBackgroundColorSwatch.Visible = false;
            PreviewTextColorSwatch.Visible = false;
            PreviewBackgroundColorSwatch.Visible = false;
            PreviewImageBorderColorSwatch.Visible = false;

            if (tour.HasMoreThanOnePage)
            {
                ShowMenuCheckBox.Checked = !tour.HideMenu;
                AddChangeDetection(ShowMenuCheckBox);
            }
            else
            {
                ShowMenuCheckBox.Checked = false;
                ShowMenuCheckBox.Enabled = false;
            }
        }

        ShowDirectoryCheckBox.Checked = tour.HasDirectory;
	    AddChangeDetection(ShowDirectoryCheckBox);

		if (tour.V3CompatibilityEnabled)
        {
            // Location option
		    LocationDropDownList.SelectedValue = ((int)location).ToString();
		    AddChangeDetection(LocationDropDownList);

		    CustomLocationX.Text = directory.LocationX.ToString();
		    CustomLocationY.Text = directory.LocationY.ToString();
		    AddChangeDetection(CustomLocationX);
		    AddChangeDetection(CustomLocationY);

            ShowMenuCheckBox.Visible = false;
            ShowMenu.Visible = false;
        }

		// Grouping option
		GroupingDropDownList.SelectedValue = Grouping();
		AddChangeDetection(GroupingDropDownList);

		// General options check boxes
		ShowAllPagesCheckBox.Checked = directory.ShowAllPages;
		AddChangeDetection(ShowAllPagesCheckBox);

		ShowSearchCheckBox.Checked = directory.ShowSearch;
		AddChangeDetection(ShowSearchCheckBox);

		ShowImagePreviewCheckBox.Checked = directory.ShowImagePreview;
		AddChangeDetection(ShowImagePreviewCheckBox);

		ShowTextPreviewCheckBox.Checked = directory.ShowTextPreview;
		AddChangeDetection(ShowTextPreviewCheckBox);

		StaysOpenCheckBox.Checked = directory.StaysOpen;
		AddChangeDetection(StaysOpenCheckBox);

		OpenExpandedCheckBox.Checked = directory.OpenExpanded;
		AddChangeDetection(OpenExpandedCheckBox);

		if (tour.V3CompatibilityEnabled)
        {
            AlignContentRightCheckBox.Checked = directory.AlignContentRight;
		    AddChangeDetection(AlignContentRightCheckBox);
        }
		
		AutoCollapseCheckBox.Checked = directory.AutoCollapse;
		AddChangeDetection(AutoCollapseCheckBox);

		PreviewOnRightCheckBox.Checked = directory.PreviewOnRight;
		AddChangeDetection(PreviewOnRightCheckBox);

		// Size options
		TitleBarWidthTextBox.Text = directory.TitleBarWidth.ToString();
		AddChangeDetection(TitleBarWidthTextBox);

		ContentWidthTextBox.Text = directory.ContentWidth.ToString();
		AddChangeDetection(ContentWidthTextBox);

		ContentHeightTextBox.Text = directory.MaxHeight.ToString();
		AddChangeDetection(ContentHeightTextBox);

		PreviewWidthTextBox.Text = directory.PreviewWidth.ToString();
		AddChangeDetection(PreviewWidthTextBox);

		PreviewImageWidthTextBox.Text = directory.PreviewImageWidth.ToString();
		AddChangeDetection(PreviewImageWidthTextBox);

		// Color options.
		UseColorSchemeColorsCheckBox.Checked = directory.UseColorSchemeColors;
		AddChangeDetectionForPreview(UseColorSchemeColorsCheckBox, "maOnShowColorWarning(this);");

		TitleTextColorSwatch.ColorValue = directory.TitleTextColor;
		TitleBarColorSwatch.ColorValue = directory.TitleBarColor;
		BackgroundColorSwatch.ColorValue = directory.BackgroundColor;
		BorderColorSwatch.ColorValue = directory.BorderColor;
		Level1TextColorSwatch.ColorValue = directory.Level1TextColor;
		Level2TextColorSwatch.ColorValue = directory.Level2TextColor;
		EntryCountColorSwatch.ColorValue = directory.EntryCountColor;
		EntryTextColorSwatch.ColorValue = directory.EntryTextColor;
		EntryTextHoverColorSwatch.ColorValue = directory.EntryTextHoverColor;
		PreviewTextColorSwatch.ColorValue = directory.PreviewTextColor;
		PreviewBackgroundColorSwatch.ColorValue = directory.PreviewBackgroundColor;
		PreviewBorderColorSwatch.ColorValue = directory.PreviewBorderColor;
		PreviewImageBorderColorSwatch.ColorValue = directory.PreviewImageBorderColor;
		SearchResultsTextColorSwatch.ColorValue = directory.SearchResultTextColor;
		SearchResultsBackgroundColorSwatch.ColorValue = directory.SearchResultBackgroundColor;
		StatusTextColorSwatch.ColorValue = directory.StatusTextColor;
		StatusBackgroundColorSwatch.ColorValue = directory.StatusBackgroundColor;

		// Text options
		TitleTextBox.Text = directory.TextTitle.ToString();
		AddChangeDetection(TitleTextBox);

		AlphaSortTooltipTextBox.Text = directory.TextAlphaSortTooltip.ToString();
		AddChangeDetection(AlphaSortTooltipTextBox);

		GroupSortTooltipTextBox.Text = directory.TextGroupSortTooltip.ToString();
		AddChangeDetection(GroupSortTooltipTextBox);

		SearchBoxLabelTextBox.Text = directory.TextSearchLabel.ToString();
		AddChangeDetection(SearchBoxLabelTextBox);

		SearchResultsMessageTextBox.Text = directory.TextSearchResultsMessage.ToString();
		AddChangeDetection(SearchResultsMessageTextBox);

		ItemsMessageTextBox.Text = directory.TextNoSearchMessage.ToString();
		AddChangeDetection(ItemsMessageTextBox);

		ClearButtonLabelTextBox.Text = directory.TextClearButtonLabel.ToString();
		AddChangeDetection(ClearButtonLabelTextBox);
	}

	protected override void PageLoad()
	{
		Utility.RegisterColorChooserJavaScript(this);

		SetMasterPage(Master);
		SetPageTitle(Resources.Text.DirectoryOptionsTitle);
		SetActionId(MemberPageActionId.DirectoryOptions);
		GetSelectedTour();

        showBasicOptions = tour.V4 && account.IsPersonalPlan;
        directory = tour.Directory;
	}

	protected override void PerformUpdate()
	{
		if (showBasicOptions)
        {
            if (tour.HasDirectory != ShowDirectoryBasicCheckBox.Checked)
            {
                tour.HasDirectory = ShowDirectoryBasicCheckBox.Checked;
                optionsChanged = true;
            }

            if (tour.HasMoreThanOnePage)
            {
                bool oldHideMenuOption = tour.HideMenu;
                if (ShowMenuBasicCheckBox.Checked)
                    tour.RuntimeTarget &= ~Tour.MapViewerFlags.HideMenu;
                else
                    tour.RuntimeTarget |= Tour.MapViewerFlags.HideMenu;
                if (oldHideMenuOption != tour.HideMenu)
                    optionsChanged = true;
            }
        }
        else
        {
            if (tour.HasDirectory != ShowDirectoryCheckBox.Checked)
		    {
			    tour.HasDirectory = ShowDirectoryCheckBox.Checked;
			    optionsChanged = true;
		    }

            if (tour.V4 && tour.HasMoreThanOnePage)
            {
                bool oldHideMenuOption = tour.HideMenu;
                if (ShowMenuCheckBox.Checked)
                    tour.RuntimeTarget &= ~Tour.MapViewerFlags.HideMenu;
                else
                    tour.RuntimeTarget |= Tour.MapViewerFlags.HideMenu;
                if (oldHideMenuOption != tour.HideMenu)
                    optionsChanged = true;
            }
        }

        if (optionsChanged)
		{
			tour.SetDirectoryChanged();
			tour.UpdateDatabase();
			directory.UpdateDatabase();
		}
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

	protected override void ReadPageFields()
	{
		if (showBasicOptions)
        {
            return;
        }

        // We test every option to see if it changed to avoid updating the directory unless
		// necessary.  Since the directory appears on every page, a change to it requres that
		// every page be rebuilt.

		optionsChanged = false;

        if (tour.V3CompatibilityEnabled)
        {
            // Location
            TourDirectoryLocation directoryLocation = (TourDirectoryLocation)int.Parse(LocationDropDownList.SelectedValue);
		    if (directory.Location != directoryLocation)
		    {
			    directory.Location = directoryLocation;
			    optionsChanged = true;
		    }

		    Point newCustomLocation = new Point(validCustomLocationX, validCustomLocationY);
		    if (directory.LocationX != newCustomLocation.X || directory.LocationY != newCustomLocation.Y)
		    {
			    directory.LocationX = validCustomLocationX;
			    directory.LocationY = validCustomLocationY;
			    optionsChanged = true;
		    }
        }
		
		// Grouping
		if (Grouping() != GroupingDropDownList.SelectedValue)
		{
			SetGrouping();
			optionsChanged = true;
		}

		// General options checkboxes
		if (directory.ShowAllPages != ShowAllPagesCheckBox.Checked)
		{
			directory.ShowAllPages = ShowAllPagesCheckBox.Checked;
			optionsChanged = true;
		}

		if (directory.ShowSearch != ShowSearchCheckBox.Checked)
		{
			directory.ShowSearch = ShowSearchCheckBox.Checked;
			optionsChanged = true;
		}

		if (directory.ShowImagePreview != ShowImagePreviewCheckBox.Checked)
		{
			directory.ShowImagePreview = ShowImagePreviewCheckBox.Checked;
			optionsChanged = true;
		}

		if (directory.ShowTextPreview != ShowTextPreviewCheckBox.Checked)
		{
			directory.ShowTextPreview = ShowTextPreviewCheckBox.Checked;
			optionsChanged = true;
		}

		if (directory.StaysOpen != StaysOpenCheckBox.Checked)
		{
			directory.StaysOpen = StaysOpenCheckBox.Checked;
			optionsChanged = true;
		}

		if (directory.OpenExpanded != OpenExpandedCheckBox.Checked)
		{
			directory.OpenExpanded = OpenExpandedCheckBox.Checked;
			optionsChanged = true;
		}

		if (tour.V3CompatibilityEnabled && directory.AlignContentRight != AlignContentRightCheckBox.Checked)
		{
			directory.AlignContentRight = AlignContentRightCheckBox.Checked;
			optionsChanged = true;
		}

		if (directory.AutoCollapse != AutoCollapseCheckBox.Checked)
		{
			directory.AutoCollapse = AutoCollapseCheckBox.Checked;
			optionsChanged = true;
		}

		if (directory.PreviewOnRight != PreviewOnRightCheckBox.Checked)
		{
			directory.PreviewOnRight = PreviewOnRightCheckBox.Checked;
			optionsChanged = true;
		}

		// Size options
		if (directory.TitleBarWidth != validTitleBarWidth)
		{
			directory.TitleBarWidth = validTitleBarWidth;
			optionsChanged = true;
		}
	
		if (directory.ContentWidth != validContentWidth)
		{
			directory.ContentWidth = validContentWidth;
			optionsChanged = true;
		}
	
		if (directory.MaxHeight != validContentHeight)
		{
			directory.MaxHeight = validContentHeight;
			optionsChanged = true;
		}
	
		if (directory.PreviewWidth != validPreviewWidth)
		{
			directory.PreviewWidth = validPreviewWidth;
			optionsChanged = true;
		}
	
		if (directory.PreviewImageWidth != validPreviewImageWidth)
		{
			directory.PreviewImageWidth = validPreviewImageWidth;
			optionsChanged = true;
		}

		// Color options

		if (directory.TitleTextColor != validTitleTextColor)
		{
			directory.TitleTextColor = validTitleTextColor;
			optionsChanged = true;
		}

		if (directory.Level1TextColor != validLevel1TextColor)
		{
			directory.Level1TextColor = validLevel1TextColor;
			optionsChanged = true;
		}

		if (directory.Level2TextColor != validLevel2TextColor)
		{
			directory.Level2TextColor = validLevel2TextColor;
			optionsChanged = true;
		}

		if (directory.EntryCountColor != validEntryCountColor)
		{
			directory.EntryCountColor = validEntryCountColor;
			optionsChanged = true;
		}

		if (directory.EntryTextColor != validEntryTextColor)
		{
			directory.EntryTextColor = validEntryTextColor;
			optionsChanged = true;
		}

		if (directory.EntryTextHoverColor != validEntryTextHoverColor)
		{
			directory.EntryTextHoverColor = validEntryTextHoverColor;
			optionsChanged = true;
		}

		if (directory.SearchResultBackgroundColor != validSearchResultsBackgroundColor)
		{
			directory.SearchResultBackgroundColor = validSearchResultsBackgroundColor;
			optionsChanged = true;
		}

        // The following colors are supported in V3, but not in V4.
        if (tour.V3CompatibilityEnabled)
        {
		    if (directory.UseColorSchemeColors != UseColorSchemeColorsCheckBox.Checked)
		    {
			    directory.UseColorSchemeColors = UseColorSchemeColorsCheckBox.Checked;
			    optionsChanged = true;
			    if (directory.UseColorSchemeColors)
				    ColorScheme.SynchronizeColorsForDirectory(tour);
		    }

		    if (directory.TitleBarColor != validTitleBarColor)
		    {
			    directory.TitleBarColor = validTitleBarColor;
			    optionsChanged = true;
		    }

		    if (directory.BackgroundColor != validBackgroundColor)
		    {
			    directory.BackgroundColor = validBackgroundColor;
			    optionsChanged = true;
		    }

		    if (directory.BorderColor != validBorderColor)
		    {
			    directory.BorderColor = validBorderColor;
			    optionsChanged = true;
		    }

		    if (directory.PreviewTextColor != validPreviewTextColor)
		    {
			    directory.PreviewTextColor = validPreviewTextColor;
			    optionsChanged = true;
		    }

		    if (directory.PreviewBackgroundColor != validPreviewBackgroundColor)
		    {
			    directory.PreviewBackgroundColor = validPreviewBackgroundColor;
			    optionsChanged = true;
		    }

		    if (directory.PreviewBorderColor != validPreviewBorderColor)
		    {
			    directory.PreviewBorderColor = validPreviewBorderColor;
			    optionsChanged = true;
		    }

		    if (directory.PreviewImageBorderColor != validPreviewImageBorderColor)
		    {
			    directory.PreviewImageBorderColor = validPreviewImageBorderColor;
			    optionsChanged = true;
		    }

		    if (directory.SearchResultTextColor != validSearchResultsTextColor)
		    {
			    directory.SearchResultTextColor = validSearchResultsTextColor;
			    optionsChanged = true;
		    }

		    if (directory.StatusTextColor != validStatusTextColor)
		    {
			    directory.StatusTextColor = validStatusTextColor;
			    optionsChanged = true;
		    }

		    if (directory.StatusBackgroundColor != validStatusBackgroundColor)
		    {
			    directory.StatusBackgroundColor = validStatusBackgroundColor;
			    optionsChanged = true;
		    }
        }

		// Text options
		if (directory.TextTitle != TitleTextBox.Text)
		{
			directory.TextTitle = TitleTextBox.Text;
			optionsChanged = true;
		}

		if (directory.TextAlphaSortTooltip != AlphaSortTooltipTextBox.Text)
		{
			directory.TextAlphaSortTooltip = AlphaSortTooltipTextBox.Text;
			optionsChanged = true;
		}

		if (directory.TextGroupSortTooltip != GroupSortTooltipTextBox.Text)
		{
			directory.TextGroupSortTooltip = GroupSortTooltipTextBox.Text;
			optionsChanged = true;
		}

		if (directory.TextSearchLabel != SearchBoxLabelTextBox.Text)
		{
			directory.TextSearchLabel = SearchBoxLabelTextBox.Text;
			optionsChanged = true;
		}

		if (directory.TextSearchResultsMessage != SearchResultsMessageTextBox.Text)
		{
			directory.TextSearchResultsMessage = SearchResultsMessageTextBox.Text;
			optionsChanged = true;
		}

		if (directory.TextNoSearchMessage != ItemsMessageTextBox.Text)
		{
			directory.TextNoSearchMessage = ItemsMessageTextBox.Text;
			optionsChanged = true;
		}

		if (directory.TextClearButtonLabel != ClearButtonLabelTextBox.Text)
		{
			directory.TextClearButtonLabel = ClearButtonLabelTextBox.Text;
			optionsChanged = true;
		}
	}

	protected override void Undo()
	{
		ClearErrors();
	}

	protected override void ValidatePage()
	{
        if (showBasicOptions)
            return;

        ClearErrors();

        if (tour.V3CompatibilityEnabled)
        {
            // Custom location offset
            validCustomLocationX = ValidateFieldInRange(CustomLocationX, -1000, 1000, CustomLocationXError);
		    if (!pageValid)
			    return;

		    validCustomLocationY = ValidateFieldInRange(CustomLocationY, -1000, 1000, CustomLocationYError);
		    if (!pageValid)
			    return;
        }

        // V4 has larger minimum values than V3, but allow V3 tours to continue using the smaller values.
        int min1 = tour.V3CompatibilityEnabled ? 40 : 100;
        int min2 = tour.V3CompatibilityEnabled ? 40 : 100;
        validContentWidth = ValidateFieldInRange(ContentWidthTextBox, min1, 800, ContentWidthError);
		validContentHeight = ValidateFieldInRange(ContentHeightTextBox, min1, 800, ContentHeightError);
		validPreviewImageWidth = ValidateFieldInRange(PreviewImageWidthTextBox, min2, 800, PreviewImageWidthError);
		validPreviewWidth = ValidateFieldInRange(PreviewWidthTextBox, min2, 800, PreviewWidthError);
		validTitleBarWidth = ValidateFieldInRange(TitleBarWidthTextBox, min2, 800, TitleBarWidthError);
		if (!pageValid)
			return;

		// Color options
        if (tour.V3CompatibilityEnabled)
        {
		    validBackgroundColor = ValidateColorSwatch(BackgroundColorSwatch);
		    validEntryCountColor = ValidateColorSwatch(EntryCountColorSwatch);
		    validEntryTextColor = ValidateColorSwatch(EntryTextColorSwatch);
		    validEntryTextHoverColor = ValidateColorSwatch(EntryTextHoverColorSwatch);
		    validLevel1TextColor = ValidateColorSwatch(Level1TextColorSwatch);
		    validLevel2TextColor = ValidateColorSwatch(Level2TextColorSwatch);
		    validPreviewImageBorderColor = ValidateColorSwatch(PreviewImageBorderColorSwatch);
		    validSearchResultsBackgroundColor = ValidateColorSwatch(SearchResultsBackgroundColorSwatch);
		    validSearchResultsTextColor = ValidateColorSwatch(SearchResultsTextColorSwatch);

		    if (UseColorSchemeColorsCheckBox.Checked)
		    {
			    ColorScheme colorScheme = tour.ColorScheme;
			    TitleTextColorSwatch.ColorValue = colorScheme.MenuSelectedTextColor;
			    TitleBarColorSwatch.ColorValue = colorScheme.MenuSelectedBackgroundColor;
			    BorderColorSwatch.ColorValue = colorScheme.MenuLineColor;
			    PreviewBorderColorSwatch.ColorValue = colorScheme.MenuLineColor;
			    PreviewTextColorSwatch.ColorValue = colorScheme.SlideTextColor;
			    PreviewBackgroundColorSwatch.ColorValue = colorScheme.SlideBackgroundColor;
			    StatusTextColorSwatch.ColorValue = colorScheme.MenuSelectedTextColor;
			    StatusBackgroundColorSwatch.ColorValue = colorScheme.MenuSelectedBackgroundColor;
		    }

		    validTitleTextColor = ValidateColorSwatch(TitleTextColorSwatch);
		    validTitleBarColor = ValidateColorSwatch(TitleBarColorSwatch);
		    validBorderColor = ValidateColorSwatch(BorderColorSwatch);
		    validPreviewBorderColor = ValidateColorSwatch(PreviewBorderColorSwatch);
		    validPreviewTextColor = ValidateColorSwatch(PreviewTextColorSwatch);
		    validPreviewBackgroundColor = ValidateColorSwatch(PreviewBackgroundColorSwatch);
		    validStatusTextColor = ValidateColorSwatch(StatusTextColorSwatch);
		    validStatusBackgroundColor = ValidateColorSwatch(StatusBackgroundColorSwatch);
        }
        else
        {
            validTitleTextColor = directory.TitleTextColor;
            validLevel1TextColor = ValidateColorSwatch(Level1TextColorSwatch);
            validLevel2TextColor = ValidateColorSwatch(Level2TextColorSwatch);
            validEntryCountColor = ValidateColorSwatch(EntryCountColorSwatch);
            validEntryTextColor = ValidateColorSwatch(EntryTextColorSwatch);
            validEntryTextHoverColor = ValidateColorSwatch(EntryTextHoverColorSwatch);
            validSearchResultsBackgroundColor = ValidateColorSwatch(SearchResultsBackgroundColorSwatch);
        }
    }

	private void ClearErrors()
	{
		ClearErrors(
			CustomLocationXError,
			CustomLocationYError,
			TitleBarWidthError,
			ContentWidthError,
			ContentHeightError,
			PreviewWidthError,
			PreviewImageWidthError
		);
	}

	private string Grouping()
	{
		int grouping = 0;
		if (directory.GroupByCategory)
		{
			grouping = directory.GroupByCategoryThenPage ? 2 : 1;
		}
		else if (directory.GroupByPage)
		{
			grouping = directory.GroupByPageThenCategory ? 4 : 3;
		}
		return grouping.ToString();
	}

	private void SetGrouping()
	{
		bool ByCategory = false;
		bool ByCategoryThenPage = false;
		bool ByPage = false;
		bool ByPageThenCategory = false;

		switch (GroupingDropDownList.SelectedValue)
		{
			case "1":
				ByCategory = true;
				break;

			case "2":
				ByCategory = true;
				ByCategoryThenPage = true;
				break;

			case "3":
				ByPage = true;
				break;

			case "4":
				ByPage = true;
				ByPageThenCategory = true;
				break;
		}

		directory.GroupByCategory = ByCategory;
		directory.GroupByCategoryThenPage = ByCategoryThenPage;
		directory.GroupByPage = ByPage;
		directory.GroupByPageThenCategory = ByPageThenCategory;
	}
}
