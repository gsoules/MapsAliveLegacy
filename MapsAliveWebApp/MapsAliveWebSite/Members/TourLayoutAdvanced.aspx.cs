// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Members_TourLayoutAdvanced : MemberPage
{
	private bool appearanceChanged;
    private string validBackgroundColor;
	private int validBodyMargin;
    private int validCustomLocationX;
    private int validCustomLocationY;
    private int validWidth;
	private int validHeight;
	private int validMenuWidth;

	protected override void InitControls(bool undo)
	{
		Size layoutArea = TourLayout.CalculateLayoutAreaSizeFromTourSize(tour, tour.TourSize);
		TourSizeHelp.Text = string.Format(AppContent.Topic("QuickHelpTourSize"), tour.TourSize.Width, tour.TourSize.Height, layoutArea.Width, layoutArea.Height);

        ColorSchemeComboBox.ResourceType = TourResourceType.TourStyle;
		ColorSchemeComboBox.SelectedResourceId = tour.ColorScheme.Id;
		ColorSchemeComboBox.OnClientSelectedIndexChangedScript = "maColorSchemeChanged();";
		
		ColorScheme colorScheme = tour.ColorScheme;
		EditColorSchemeControl.OnClickActionId = MemberPageActionId.EditColorScheme;
		EditColorSchemeControl.Title = "Edit";
		
		// Set the id to -1 to mean use the style for the current tour. We do this to circument the problem
		// of the user selecting a new style from the dropdown list and then clicking the Edit link for the
		// style. In that case, the control query string property does not get updated and so if it contained
		// the id for the current style, that one would get edited instead of the new one the user chose.
		EditColorSchemeControl.QueryString = "?id=-1";
      
        if (tour.V3CompatibilityEnabled)
        {
            DimensionsPanelV3.Visible = true;
            MenuOptionsV3.Visible = true;
        }
        else
        {
            DimensionsPanel.Visible = true;
            MenuOptions.Visible = true;
        }

        if (tourPage == null)
			CurrentPagePanel.Visible = false;
		else
			RenderAutoLayoutControls();

		if (!undo && IsPostBack)
			return;

		InitTourSizeControls();
		InitTitleAndStripeControls();
		InitMenuControls();
		InitTourBodyControls();

        if (tour.IsFlexMapTour)
        {
            FooterOption.Visible = false;
        }
        else
        {
		    CustomFooterText.Text = tour.CustomFooter;
		    if (!account.IsTrial)
			    AddChangeDetection(CustomFooterText);
		    else
			    CustomFooterText.Enabled = false;
        }

		if (tourPage != null)
			InitPreviewControls();
	}

	private void InitTourBodyControls()
	{
		BackgroundColorSwatch.ColorValue = tour.BodyBackgroundColor;

	    LeftAlignedCheckBox.Checked = tour.LeftAlignedInBrowser;
	    AddChangeDetection(LeftAlignedCheckBox);
		
		if (tour.IsFlexMapTour)
        {
            BodyMarginOption.Visible = false;
        }
        else
        {
            BodyMarginTextBox.Text = tour.BodyMargin.ToString();
		    AddChangeDetection(BodyMarginTextBox);
        }
	}

	private void InitPreviewControls()
	{
		SlideLayout slideLayout = tourPage.ActiveSlideLayout;
		
		// Show the dimensions of each area in the slide.
		int drawDimensions = tour.V3CompatibilityEnabled ? 1 : 0;

        // Draw the image to fit the available width.
        int thumbnailDimension = Math.Min(tour.TourSize.Width, TourBuilderPageContentRightWidth);

        PagePreviewImage.ImageUrl = string.Format("PageRenderer.ashx?args={0},{1},{2},{3},{4}&v={5}",
			(int)PreviewType.PagePreview,
			thumbnailDimension,
			tourPage.Id,
			drawDimensions,
			(int)slideLayout.Pattern,
			DateTime.Now.Ticks
		);
	}

	private void InitMenuControls()
	{
		if (tour.V3CompatibilityEnabled)
        {
            NavigationDropDownList.DataSource = Tour.OptionsForNavigation;
		    NavigationDropDownList.DataTextField = "Description";
		    NavigationDropDownList.DataValueField = "TourNavigationId";
		    NavigationDropDownList.DataBind();
		    NavigationDropDownList.SelectedValue = tour.MenuLocationId.ToString();
		    AddChangeDetectionForPreview(NavigationDropDownList);

		    MenuWidthTextBox.Text = tour.MenuWidth.ToString();
		    AddChangeDetectionForPreview(MenuWidthTextBox);

		    MenuScrollsCheckBox.Checked = tour.MenuScrolls;
		    AddChangeDetection(MenuScrollsCheckBox);

		    MenuStyleDropDownList.DataSource = Tour.OptionsForMenuStyle;
		    MenuStyleDropDownList.DataTextField = "Description";
		    MenuStyleDropDownList.DataValueField = "TourMenuStyleId";
		    MenuStyleDropDownList.DataBind();
		    MenuStyleDropDownList.SelectedValue = tour.MenuStyleId.ToString();
		    AddChangeDetection(MenuStyleDropDownList);
        }
        else
        {
            if (tour.HideMenu && !tour.HasDirectory)
            {
                NavOptionsTable.Visible = false;
                NoNavOptionsExplanation.Visible = true;
                NoNavOptionsExplanation.Text =
                    "This tour has no navigation options because <b>Show Directory</b> and " +
                    "<b>Show Menu</b> are unchecked on the Directory Options screen<br/>";
            }
            else
            {
                HideNavButtonCheckBox.Checked = tour.MenuLocationId == (int)Tour.MenuLocation.None;
                AddChangeDetection(HideNavButtonCheckBox);

                CustomLocationX.Text = tour.Directory.LocationX.ToString();
                CustomLocationY.Text = tour.Directory.LocationY.ToString();
                AddChangeDetection(CustomLocationX);
                AddChangeDetection(CustomLocationY);

                LocationDropDownList.Items.Clear();

                if (ShowTitleBarCheckBox.Checked)
                    LocationDropDownList.Items.Add(new ListItem("Title Bar", "4"));

                if (!(tour.HasDataSheet || tour.HasGallery))
                {
                    LocationDropDownList.Items.Add(new ListItem("Map Left", "2"));
                    LocationDropDownList.Items.Add(new ListItem("Map Right", "3"));
                }

                if (tour.HasBanner)
                {
                    LocationDropDownList.Items.Add(new ListItem("Banner Left", "10"));
                    LocationDropDownList.Items.Add(new ListItem("Banner Center", "11"));
                    LocationDropDownList.Items.Add(new ListItem("Banner Right", "12"));
                }

                LocationDropDownList.Items.Add(new ListItem("Above Left", "13"));
                LocationDropDownList.Items.Add(new ListItem("Above Center", "14"));
                LocationDropDownList.Items.Add(new ListItem("Above Right", "15"));

                string selectedLocation = ((int)tour.Directory.Location).ToString();

                LocationDropDownList.SelectedValue = selectedLocation;
                AddChangeDetection(LocationDropDownList);
            }
        }
    }

	private void InitTitleAndStripeControls()
	{
		if (tour.IsFlexMapTour)
        {
            TitleAndStripeOptions.Visible = false;
            return;
        }

        ShowTitleBarCheckBox.Checked = tour.HasTitle;
		AddChangeDetectionForPreview(ShowTitleBarCheckBox);

		ShowHeaderStripeCheckBox.Checked = tour.HasHeaderStripe;
		AddChangeDetectionForPreview(ShowHeaderStripeCheckBox);

		ShowFooterStripeCheckBox.Checked = tour.HasFooterStripe;
		AddChangeDetectionForPreview(ShowFooterStripeCheckBox);
	}

    private void InitTourSizeControls()
    {
        InitDimensions();

        if (tour.V3CompatibilityEnabled)
        {
            AddChangeDetectionForPreview(PageWidth);
            AddChangeDetectionForPreview(PageHeight);
            AddChangeDetectionForPreview(PageWidthDropDownList);
            AddChangeDetectionForPreview(PageHeightDropDownList);
        }
        else
        {
            AddChangeDetectionForPreview(TourWidth);
            AddChangeDetectionForPreview(TourHeight);
        }
    }

	private void InitDimensions()
	{
		if (tour.V3CompatibilityEnabled)
        {
            // Initialize the width and height dropdown lists.
		    InitDimensionList(PageWidthDropDownList, "Width", tour.HasBanner);
		    PageWidthDropDownList.SelectedValue = ((int)tour.WidthType).ToString();

		    InitDimensionList(PageHeightDropDownList, "Height", false);
		    PageHeightDropDownList.SelectedValue = ((int)tour.HeightType).ToString();

		    // Initialize the width and height fields.
		    PageWidth.Text = tour.WidthType == TourSizeType.Exact ? tour.TourSize.Width.ToString() : tour.MaxTourSize.Width.ToString();
		    PageHeight.Text = tour.HeightType == TourSizeType.Exact ? tour.TourSize.Height.ToString() : tour.MaxTourSize.Height.ToString();
        }
        else
        {
            TourWidth.Text = tour.TourSize.Width.ToString();
            TourHeight.Text = tour.TourSize.Height.ToString();
        }
    }

	private void InitDimensionList(DropDownList list, string dimension, bool exactOnly)
	{
		list.Items.Clear();
		
		ListItem item;

		item = new ListItem(string.Format("Exact Tour {0}", dimension), ((int)TourSizeType.Exact).ToString(), true);
		list.Items.Add(item);
		item = new ListItem(string.Format("Max Tour {0}", dimension), ((int)TourSizeType.Max).ToString(), !exactOnly);
		list.Items.Add(item);
		item = new ListItem(string.Format("Max Canvas {0}", dimension), ((int)TourSizeType.LayoutArea).ToString(), !exactOnly);
		list.Items.Add(item);
	}

	private void RenderAutoLayoutControls()
	{
		MemberPageActionId layoutPreviewActionId = MemberPageActionId.TourLayoutAdvanced;

        LayoutOptionsPanel.Visible = tour.V3CompatibilityEnabled;

        ToggleAutoLayoutControl.Visible = true;
		ToggleAutoLayoutControl.Title = string.Format("{0} Auto Layout", tour.AutoLayoutEnabled ? "Disable" : "Enable");
		ToggleAutoLayoutControl.OnClickActionId = layoutPreviewActionId;
		ToggleAutoLayoutControl.QueryString = string.Format("?disable={0}", tour.AutoLayoutEnabled ? "1" : "0&adjust=1");
		ToggleAutoLayoutControl.VeryImportant = !tour.AutoLayoutEnabled;

		DisableAutoLayout.Visible = tour.AutoLayoutEnabled;
		EnableAutoLayout.Visible = !tour.AutoLayoutEnabled;

		RunAutoLayoutControl.Title = "Run Auto Layout";
		RunAutoLayoutControl.OnClickActionId = layoutPreviewActionId;
		RunAutoLayoutControl.QueryString = "?adjust=2";
		RunAutoLayoutControl.WarningMessage = CreateAutoLayoutWarningMessage();
		if (!tour.AutoLayoutEnabled)
			RunAutoLayoutControl.ErrorMessage = Resources.Text.ErrorAutoPageLayoutDisabled;
		
		RestoreLayoutControl.Title = "Undo Layout Changes";
		RestoreLayoutControl.OnClickActionId = layoutPreviewActionId;
		RestoreLayoutControl.QueryString = "?restore=1";
		bool layoutChanged = tourPage.LayoutChanged;
		RestoreLayoutControl.AppearsEnabled = layoutChanged;
		RestoreLayoutControl.VeryImportant = layoutChanged;
		RestoreLayoutControl.WarningMessage = "Undo changes?";
	}

	private string CreateAutoLayoutWarningMessage()
	{
		string warningMessage = string.Empty;

		if (tour.TourWidthLocked)
			warningMessage += string.Format("\\n- Tour Width set to {0}", tour.TourSize.Width);

		if (tour.TourHeightLocked)
			warningMessage += string.Format("\\n- Tour Height set to {0}", tour.TourSize.Height);

		SlideLayout slideLayout = tourPage.ActiveSlideLayout;

		SlideLayoutSplitters splitters = slideLayout.Splitters;

		if (warningMessage != string.Empty)
			warningMessage = "\\n\\nNOTE: Auto Layout will not affect:" + warningMessage;

		int pageCount = tour.TourPages.Count;
		if (pageCount > 1 && !(tour.TourWidthLocked && tour.TourHeightLocked))
		{
			string pages = pageCount == 2 ? "both" : "all " + pageCount;
			warningMessage += "\\n\\n";
			warningMessage += string.Format("WARNING: This tour has {0} pages and\\n{1} will be set to the size for {2}.", pageCount, pages, tourPage.Name); ;
		}

		warningMessage = "Run Auto Layout?" + warningMessage;

		return warningMessage;
	}

	protected override void PageLoad()
	{
		Utility.RegisterColorChooserJavaScript(this);
		SetMasterPage(Master);
		SetPageTitle("Advanced Tour Layout");
		SetActionIdForPageAction(MemberPageActionId.TourLayoutAdvanced);
		GetSelectedTour();

        if (!IsPostBack && !IsReturnToTourBuilder && tourPage != null)
		{
			AutoLayout autoLayout = new AutoLayout(tourPage);
			autoLayout.HandleQueryOptions(Request.QueryString);

			// Remember the original layout to compare against on post back to see if Undo is allowed.
			// If we got here following an adjustment, the layout was just modified and is not the original.
			if (Request.QueryString["adjust"] == null)
				tourPage.AcceptLayoutChanges();
		}
    }

    protected override void PerformUpdate()
	{
        tour.UpdateDatabase();

        // In V4, some of the options on this page were on the Directory Options page in V3
        // and need to get written to the Directory table.
        if (tour.V4)
            tour.Directory.UpdateDatabase();

        if (appearanceChanged)
        {
            if (!ShowTitleBarCheckBox.Checked && tour.Directory.Location == TourDirectoryLocation.TitleBar)
                tour.Directory.Location = TourDirectoryLocation.AboveRight;

            tour.UpdatePageLayouts();
            tour.RebuildTourTreeXml();
            InitDimensions();
        }

        if (tour.V4)
            InitMenuControls();
    }

    protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

	protected override void ReadPageFields()
	{
		ReadTourSizeFields();
		ReadTitleAndStripeFields();
		ReadMenuFields();
		ReadTourBodyFields();

        if (!tour.IsFlexMapTour)
        {
            bool hadCustomFooter = tour.HasCustomFooter;
            tour.CustomFooter = CustomFooterText.Text;
            if (hadCustomFooter != tour.HasCustomFooter)
                appearanceChanged = true;
        }
    }

	private void ReadTourBodyFields()
	{
		int colorSchemeId = ColorSchemeComboBox.SelectedResourceId;
		if (tour.ColorScheme.Id != colorSchemeId)
		{
			tour.ColorScheme = Account.GetCachedColorScheme(colorSchemeId);
			appearanceChanged = true;
			ColorScheme.SynchronizeColorsForDirectory(tour);
			ColorScheme.SynchronizeColorsForPopup(tour);
		}

		tour.BodyBackgroundColor = validBackgroundColor;
        tour.LeftAlignedInBrowser = LeftAlignedCheckBox.Checked;

        if (!tour.IsFlexMapTour)
            tour.BodyMargin = validBodyMargin;
	}

	private void ReadMenuFields()
	{
        if (tour.V3CompatibilityEnabled)
        {
            int menuLocation = int.Parse(NavigationDropDownList.SelectedValue);

		    int menuStyleId = int.Parse(MenuStyleDropDownList.SelectedValue);
		    if (tour.MenuStyleId != menuStyleId)
		    {
			    tour.MenuStyleId = menuStyleId;
			    appearanceChanged = true;
		    }

		    if (tour.MenuWidth != validMenuWidth)
		    {
			    tour.MenuWidth = validMenuWidth;
			    appearanceChanged = true;
		    }
		
		    tour.MenuScrolls = MenuScrollsCheckBox.Checked;

            if (tour.MenuLocationId != menuLocation)
            {
                tour.MenuLocationId = menuLocation;
                appearanceChanged = true;
            }
        }
        else
        {
            int navButtonLocationId = HideNavButtonCheckBox.Checked ? (int)Tour.MenuLocation.None : (int)Tour.MenuLocation.AutoTop;

            TourDirectoryLocation directoryLocation = (TourDirectoryLocation)int.Parse(LocationDropDownList.SelectedValue);
            if (tour.V4 && tour.Directory.Location != directoryLocation)
            {
                // Handle the case where the user unchecks the title bar option and also chooses the title bar
                // for the nav button location. If that happens, use the original nav button location option. It
                // would be nice to have JavaScript that prevented this from happening, but it's an obscure case.
                if (directoryLocation == TourDirectoryLocation.TitleBar && !ShowTitleBarCheckBox.Checked)
                    directoryLocation = tour.Directory.Location;
                
                tour.Directory.Location = directoryLocation;
                appearanceChanged = true;
            }

            Point newCustomLocation = new Point(validCustomLocationX, validCustomLocationY);
            if (tour.Directory.LocationX != newCustomLocation.X || tour.Directory.LocationY != newCustomLocation.Y)
            {
                tour.Directory.LocationX = validCustomLocationX;
                tour.Directory.LocationY = validCustomLocationY;
                appearanceChanged = true;
            }

            if (tour.MenuLocationId != navButtonLocationId)
		    {
			    tour.MenuLocationId = navButtonLocationId;
			    appearanceChanged = true;
		    }
        }
    }

	private void ReadTitleAndStripeFields()
	{
        if (tour.IsFlexMapTour)
            return;

        if (tour.HasTitle != ShowTitleBarCheckBox.Checked)
		{
			tour.HasTitle = ShowTitleBarCheckBox.Checked;
			appearanceChanged = true;
		}

		if (tour.HasHeaderStripe != ShowHeaderStripeCheckBox.Checked)
		{
			tour.HasHeaderStripe = ShowHeaderStripeCheckBox.Checked;
			appearanceChanged = true;
		}

		if (tour.HasFooterStripe != ShowFooterStripeCheckBox.Checked)
		{
			tour.HasFooterStripe = ShowFooterStripeCheckBox.Checked;
			appearanceChanged = true;
		}
	}

	private void ReadTourSizeFields()
	{
		// Determine the width and height type from the dropdown lists.
		TourSizeType widthType;
		TourSizeType heightType;
        if (tour.V3CompatibilityEnabled)
        {
		    widthType = (TourSizeType)int.Parse(PageWidthDropDownList.SelectedValue);
		    heightType = (TourSizeType)int.Parse(PageHeightDropDownList.SelectedValue);
        }
        else
        {
            widthType = TourSizeType.Exact;
            heightType = TourSizeType.Exact;
        }

        // Determine if the exact tour size changed.
        int tourWidth;
        int tourHeight;
        if (tour.V3CompatibilityEnabled)
        {
		    tourWidth = widthType == TourSizeType.Exact ? validWidth : tour.TourSize.Width;
		    tourHeight = heightType == TourSizeType.Exact ? validHeight : tour.TourSize.Height;
        }
        else
        {
            tourWidth = validWidth;
            tourHeight = validHeight;
        }

        Size newTourSize = new Size(tourWidth, tourHeight);
		bool tourSizeChanged = newTourSize != tour.TourSize;

		// Determine if the width or height types changed.
		bool widthTypeChanged = widthType != tour.WidthType;
		bool heightTypeChanged = heightType != tour.HeightType;

		// Determine if the tour's max width or max height changed.
		int maxTourWidth = widthType == TourSizeType.Exact ? tour.MaxTourSize.Width : validWidth;
		int maxTourHeight = heightType == TourSizeType.Exact ? tour.MaxTourSize.Height : validHeight;
		Size newMaxTourSize = new Size(maxTourWidth, maxTourHeight);
		bool maxTourSizeChanged = newMaxTourSize != tour.MaxTourSize;

		// Adjust the layout for changes to the tour sizes or types.
		if (tourSizeChanged || maxTourSizeChanged || widthTypeChanged || heightTypeChanged)
		{
			tour.WidthType = widthType;
			tour.HeightType = heightType;
			tour.MaxTourSize = newMaxTourSize;
			if (tourSizeChanged)
				tour.SetTourSizeAndAdjustLayouts(newTourSize);
			tour.UpdateDatabase();

			PerformAutoLayoutOnAllPages();

			// Update the width and height fields so that if either is set to Max,
			// it will reflect the new page size they may have resulted from running auto layout.
			InitDimensions();
		}
	}

	private void PerformAutoLayoutOnAllPages()
	{
		if (tour.TourPageCount == 0)
			return;

		AutoLayoutOptions options = new AutoLayoutOptions();
		options.WidthType = tour.WidthType;
		options.HeightType = tour.HeightType;
		tourPage.LayoutManager.PerformAutoLayout(options);
		if (!tourPage.SlidesPopup)
			tourPage.InvalidateThumbnail();

		tourPage.LayoutManager.PerformAutoLayoutOnOtherPages(tourPage);
	}

	protected override void ValidatePage()
	{
		ClearErrors();

		if (!ValidateTourSizeOptions())
			return;

		if (!ValidateMenuOptions())
			return;

		if (!ValidateTourBodyOptions())
			return;
	}

	private bool ValidateTourBodyOptions()
	{
		validBackgroundColor = ValidateColorSwatch(BackgroundColorSwatch);

		if (!pageValid)
			return false;

		if (!tour.IsFlexMapTour)
            validBodyMargin = ValidateFieldInRange(BodyMarginTextBox, 0, 200, BodyMarginError);

		return pageValid;
	}

	private bool ValidateTourSizeOptions()
	{
		int min = 50;

		if (tour.V3CompatibilityEnabled)
        {
            validWidth = ValidateFieldInRange(PageWidth, min, 4096, PageWidthError);
		    validHeight = ValidateFieldInRange(PageHeight, min, 4096, PageHeightError);
        }
        else
        {
            validWidth = ValidateFieldInRange(TourWidth, min, 4096, TourWidthError);
            validHeight = ValidateFieldInRange(TourHeight, min, 4096, TourHeightError);
        }

        return pageValid;
	}

	private bool ValidateMenuOptions()
	{
		if (tour.V3CompatibilityEnabled)
        {
            int maxWidth = int.Parse(NavigationDropDownList.SelectedValue) == (int)Tour.MenuLocation.Left ? tour.TourSize.Width / 2 : int.MaxValue;
		    validMenuWidth = ValidateFieldInRange(MenuWidthTextBox, 16, maxWidth, MenuWidthError);
		    if (!pageValid)
			    return false;
        }
        else
        {
            // Custom location offset
            validCustomLocationX = ValidateFieldInRange(CustomLocationX, -1000, 1000, CustomLocationXError);
            if (!pageValid)
                return false;

            validCustomLocationY = ValidateFieldInRange(CustomLocationY, -1000, 1000, CustomLocationYError);
            if (!pageValid)
                return false;
        }

        return pageValid;
	}

	private void ClearErrors()
	{
		ClearErrors(
            CustomLocationXError,
            CustomLocationYError,
            BodyMarginError,
			MenuWidthError,
			PageWidthError,
			PageHeightError);
	}
}
