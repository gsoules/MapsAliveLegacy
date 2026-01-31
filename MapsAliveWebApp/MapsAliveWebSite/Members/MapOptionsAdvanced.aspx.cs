// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Members_MapOptionsAdvanced : MemberPage
{
    private bool mapCanZoom;
    private string validControlOffColor;
	private string validInsetColor;
	private string validInstructionsBackgroundColor;
	private string validInstructionsColor;
	private int validInstructionsWidth;
	private string validMapBackgroundColor;
	private int validMapZoomInsetSize;
	private string validPageId;
	private int validSelectedMarkerBlink;
	private string validSlideListInstructions;
	private int validVisitedMarkerAlpha;

	protected void AddChangeDetection(CheckBox checkBox, string script)
	{
		checkBox.Attributes.Add("onclick", script);
	}

	protected override void InitControls(bool undo)
	{
        if (tour.V3CompatibilityEnabled)
        {
            if (tourPage.IsGallery)
		    {
			    MapZoomPanel.Visible = false;
                MarkersZoomCheckBox.Visible = false;
                MarkerZoomType.Visible = false;
            }
		    else
		    {
                MapZoomDisabledPanel.Visible = !mapCanZoom;
			    MapZoomOptionsPanel.Visible = mapCanZoom;
		    }
        }

        TooltipStyleComboBox.SelectedResourceId = tourPage.TooltipStyle.Id;
        TooltipStyleComboBox.OnClientSelectedIndexChangedScript = "maTooltipStyleChanged();";

        ShowSlideNamesInMenuPanel.Visible = tour.V3CompatibilityEnabled;

        if (tour.V4)
        {
            ShowSlideListCheckBox.Visible = false;
            ShowSlideList.Visible = false;
            ShowSlideListOptionsPanel.Visible = false;
            MapZoomIsOnMessage.Visible = false;
        }

        if (tour.V3CompatibilityEnabled)
        {
            MapImageSharpening.Visible = false;
            MapImageSharpeningDropDownList.Visible = false;
        }

		InstructionsTextPanel.Style.Add(HtmlTextWriterStyle.Display, tourPage.ShowInstructions || ShowInstructionsCheckBox.Checked ? "block" : "none");
        if (account.IsPlusOrProPlan)
            InstructionsTextBox.CssClass = "HtmlEditor";

		ShowTitleCheckBox.Checked = tourPage.ShowSlideTitle;
		AddChangeDetection(ShowTitleCheckBox);

		ShowSlideListOptionsPanel.Style.Add(HtmlTextWriterStyle.Display, tourPage.ShowSlideList || ShowSlideListCheckBox.Checked ? "block" : "none");

		if (!tourPage.IsGallery)
		{
			HotspotOrderListBox.ButtonSettings.RenderButtonText = false;
			HotspotOrderListBox.ButtonSettings.Position = ListBoxButtonPosition.Right;
			HotspotOrderListBox.AllowReorder = tourPage.TourViews.Count > 1;
		}

		if (!undo && IsPostBack)
			return;

		if (tourPage.IsGallery)
		{
			MapBackgroundColorSwatch.Visible = false;
			MapBackgroundColor.Visible = false;
		}
		else
		{
			MapBackgroundColorSwatch.ColorValue = tourPage.MapPlaceholderColor;
		}

		InstructionsTextColorSwatch.ColorValue = tourPage.InstructionsColor;
		InstructionsBackgroundColorSwatch.ColorValue = tourPage.InstructionsBgColor;

		TourTitleTextBox.Text = tourPage.Title;
		AddChangeDetection(TourTitleTextBox);

		ShowInstructionsCheckBox.Checked = tourPage.ShowInstructions;
		AddChangeDetection(ShowInstructionsCheckBox, "maShowInstructionsText(this);");

		InstructionsTextBox.Text = tourPage.InstructionsText;
		AddChangeDetection(InstructionsTextBox);

		InstructionsTitleTextBox.Text = tourPage.InstructionsTitle;
		AddChangeDetection(InstructionsTitleTextBox);

		InstructionsWidthTextBox.Text = tourPage.InstructionsWidth.ToString();
		AddChangeDetection(InstructionsWidthTextBox);

		PageIdTextBox.Text = tourPage.PageId;
		AddChangeDetection(PageIdTextBox);

        MapZoomEnabledCheckbox.Checked = tourPage.ShowPanZoomControls;
		AddChangeDetection(MapZoomEnabledCheckbox);
        if (tour.V3CompatibilityEnabled)
            MapZoomEnabled.Title = "Show Zoom and Pan Controls";

        MapZoomInsetLocationDropDown.SelectedValue = tourPage.MapInsetLocation.ToString();
		AddChangeDetection(MapZoomInsetLocationDropDown);

        if (tour.V4)
        {
            MapImageSharpeningDropDownList.SelectedValue = tourPage.MapImageSharpening.ToString();
            AddChangeDetection(MapImageSharpeningDropDownList);
        }

        MapZoomInsetSizeTextBox.Text = tourPage.MapInsetSize.ToString();
		AddChangeDetection(MapZoomInsetSizeTextBox);

		MapInsetColorSwatch.ColorValue = tourPage.MapInsetColor;
		ControlOffColorSwatch.ColorValue = tourPage.PanZoomControlColorOff;

        MarkersZoomCheckBox.Checked = tourPage.MarkersZoom;
		AddChangeDetection(MarkersZoomCheckBox);

		ExcludeFromNavigationCheckbox.Checked = tourPage.ExcludeFromNavigation;
		AddChangeDetection(ExcludeFromNavigationCheckbox);

		if (tourPage.RoutesXml.Length > 0 && tour.V3CompatibilityEnabled)
		{
			RouteTestCheckBox.Checked = tourPage.ShowRouteList;
			AddChangeDetection(RouteTestCheckBox);
		}
		else
		{
			RouteTestingPanel.Visible = false;
		}

		InitHotspotOptions();

		ShowCodeSnippets();

		if (tourPage.IsGallery)
		{
			MapUrl.Topic = "GalleryUrl";
			TourPageTitle.Title = "Gallery Title";
			TourPageTitle.Topic = "GalleryTitle";
			PageId.Title = "Gallery Id";
			PageId.Topic = "GalleryId";
			ShowInstructions.Title = "Show Help for this Gallery";
			ShowInstructions.Topic = "ShowInstructionsGallery";
			RunSlideShow.Title = "Run Slide Show When Gallery Loads";
			RunSlideShow.Topic = "RunSlideShowGallery";
			ShowSlideTitle.Topic = "GalleryShowSlideTitle";
		}
	}

	private void InitHotspotOptions()
	{
		if (tour.V3CompatibilityEnabled)
        {
            ShowSlideListCheckBox.Checked = tourPage.ShowSlideList;
            ShowSlideListCheckBox.Enabled = tour.HasTitle;
        }

        AddChangeDetection(ShowSlideListCheckBox, "maEnableSlideListInstructions(this);");

		SlideListInstructionsTextBox.Text = tourPage.SlideListInstructions;
		AddChangeDetection(SlideListInstructionsTextBox);

		ShowSlideNamesInMenuCheckBox.Checked = tourPage.ShowSlideNamesInMenu;
		ShowSlideNamesInMenuCheckBox.Enabled = tour.MenuLocationIdEffective == (int)Tour.MenuLocation.Left;
		AddChangeDetection(ShowSlideNamesInMenuCheckBox);

		RunSlideShowCheckBox.Checked = tourPage.RunSlideShow;
		AddChangeDetection(RunSlideShowCheckBox);
		SlideShowDropDown.SelectedValue = (tourPage.SlideShowInterval / 1000).ToString();
		AddChangeDetection(SlideShowDropDown);

		SelectedMarkerBlinkTextBox.Text = tourPage.SelectedMarkerBlink.ToString();
		AddChangeDetection(SelectedMarkerBlinkTextBox);

		VisitedMarkerOpacityTextBox.Text = tourPage.VisitedMarkerAlpha.ToString();
		AddChangeDetection(VisitedMarkerOpacityTextBox);

		if (tourPage.IsGallery || tourPage.TourViews.Count == 0)
			HotspotListPanel.Visible = false;
		else
			LoadHotspotListBox();
	}

	private void LoadHotspotListBox()
	{
		if (tourPage.IsGallery || IsPostBack)
			return;

		// Populate the hotspot order list.
		foreach (TourView tourView in tourPage.TourViewsBySequence)
		{
			HotspotOrderListBox.Items.Add(new RadListBoxItem(tourView.Title, tourView.Id.ToString()));
		}
	}

	protected override void PageLoad()
	{
        Utility.RegisterHtmlEditorJavaScript(this);
        Utility.RegisterColorChooserJavaScript(this);

		SetMasterPage(Master);
		SetActionIdForPageAction(MemberPageActionId.MapOptionsAdvanced);
		GetSelectedTourPage();
		SetPageTitle(tourPage.IsGallery ? "Advanced Gallery Options" : "Advanced Map Options");
        // The map zoom options are allowed for all maps in V4, but limited to zoomable maps in V3.

        mapCanZoom = tourPage.MapCanZoom || tour.V4;

        TooltipStyleComboBox.ResourceType = TourResourceType.TooltipStyle;
    }

    protected override void PerformUpdate()
	{
		if (mapCanZoom && !tourPage.IsGallery)
		{
			bool rebuildMap = false;
			if (tourPage.MapInsetColor != validInsetColor)
			{
				rebuildMap = true;
				tourPage.MapInsetColor = validInsetColor;
			}

			if (tourPage.PanZoomControlColorOff != validControlOffColor)
			{
				rebuildMap = true;
				tourPage.PanZoomControlColorOff = validControlOffColor;
			}

            tourPage.PanZoomControlColorOn = validControlOffColor;

			if (rebuildMap)
			{
				tourPage.SetMapImageChanged();
				tourPage.RebuildMap();
			}
		}

		tourPage.ShowRouteList = RouteTestCheckBox.Checked;
		
		PerformUpdateOfHotspotOrder();
		
		tourPage.UpdateDatabase();
	}

	private void PerformUpdateOfHotspotOrder()
	{
		if (tourPage.IsGallery)
			return;

		int position = 0;
		bool orderChanged = false;

		foreach (RadListBoxItem item in HotspotOrderListBox.Items)
		{
			int tourViewId = int.Parse(item.Value);

			TourView tourView = tourPage.GetTourView(tourViewId);

			position++;
			if (tourView.SequenceNumber != position)
			{
				tourView.SetSequenceNumber(position);
				orderChanged = true;
			}
		}

		if (orderChanged)
		{
			tourPage.RebuildMap();
		}
	}
	
	protected override void ReadPageFields()
	{
		tourPage.Title = TourTitleTextBox.Text.Trim();
		tourPage.PageId = validPageId;
        
        if (tourPage.TooltipStyle.Id != TooltipStyleComboBox.SelectedResourceId)
            tourPage.TooltipStyle = Account.GetCachedTooltipStyle(TooltipStyleComboBox.SelectedResourceId);

        tourPage.ShowSlideTitle = ShowTitleCheckBox.Checked;

		if (!tourPage.IsGallery && tourPage.MapPlaceholderColor != validMapBackgroundColor)
		{
			tourPage.MapPlaceholderColor = validMapBackgroundColor;
			if (!tourPage.MapImage.HasFile)
			{
				// Force generation of a new background image.
				tourPage.SetMapImageChanged();
			}
		}

		tourPage.ShowInstructions = ShowInstructionsCheckBox.Checked;
		if (tourPage.ShowInstructions)
		{
			tourPage.InstructionsText = InstructionsTextBox.Text;
			tourPage.InstructionsTitle = InstructionsTitleTextBox.Text;
			tourPage.InstructionsColor = validInstructionsColor;
			tourPage.InstructionsBgColor = validInstructionsBackgroundColor;
			tourPage.InstructionsWidth = validInstructionsWidth;
			tourPage.InstructionsFont = "Arial";
			tourPage.InstructionsFontSize = 12;
		}

		if (mapCanZoom && !tourPage.IsGallery)
		{
			tourPage.ShowPanZoomControls = MapZoomEnabledCheckbox.Checked;
			tourPage.MapInsetLocation = int.Parse(MapZoomInsetLocationDropDown.SelectedValue);
			tourPage.MapInsetSize = validMapZoomInsetSize;
		}

        if (tour.V4)
            tourPage.MapImageSharpening = int.Parse(MapImageSharpeningDropDownList.SelectedValue);

		if (tourPage.MarkersZoom != MarkersZoomCheckBox.Checked)
		{
			tourPage.MarkersZoom = MarkersZoomCheckBox.Checked;
			tourPage.RebuildMap();
		}

		tourPage.ExcludeFromNavigation = ExcludeFromNavigationCheckbox.Checked;
		
        // This was a user option in V3 when published tours used Flash.
        tourPage.SaveMapStateChanges = false;

		ReadHotspotFields();
	}

	private void ReadHotspotFields()
	{
		tourPage.RunSlideShow = RunSlideShowCheckBox.Checked;
		tourPage.SlideShowInterval = int.Parse(SlideShowDropDown.SelectedValue) * 1000;

		tourPage.ShowSlideList = ShowSlideListCheckBox.Checked;
		if (tourPage.ShowSlideList)
			tourPage.SlideListInstructions = validSlideListInstructions;

		tourPage.ShowSlideNamesInMenu = ShowSlideNamesInMenuCheckBox.Checked;

		tourPage.SelectedMarkerBlink = validSelectedMarkerBlink;
		tourPage.VisitedMarkerAlpha = validVisitedMarkerAlpha;
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

	protected override void Undo()
	{
		ClearErrors();
	}

	protected override void ValidatePage()
	{
		// Show errors on the status line.  We validate groups of controls that use the same error
		// message and stop validating before going on to a different group if the current group
		// had an error (otherwise, the next group's error message would clobber the previous one).
		ClearErrors();

		validPageId = PageIdTextBox.Text.Trim();
		ValidateFieldNotBlank(validPageId, PageIdError, Resources.Text.ErrorPageNameRequired);
		if (!fieldValid)
			return;

		// Make sure the Id does not contain any disallowed characters.
		ValidateFieldIsValidFileName(validPageId, PageIdError, Resources.Text.ErrorIdContainsInvalidCharacters);
		if (!fieldValid)
			return;

		// Report an error it the user changed the Id and the new Id is already in use.
		if (fieldValid && tourPage.PageId != validPageId)
		{
			bool idInUse = TourPage.TourPagePageIdInUse(tour, tourPage.Id, validPageId);
			ValidateFieldCondition(!idInUse, PageIdError, Resources.Text.ErrorPageIdInUse);
			if (!fieldValid)
				return;
		}

		if (!tourPage.IsGallery)
		{
			validMapBackgroundColor = ValidateColorSwatch(MapBackgroundColorSwatch);
			if (!fieldValid)
				return;
		}

		if (ShowInstructionsCheckBox.Checked)
		{
			validInstructionsColor = ValidateColorSwatch(InstructionsTextColorSwatch);
			validInstructionsBackgroundColor = ValidateColorSwatch(InstructionsBackgroundColorSwatch);
			if (!pageValid)
				return;

			validInstructionsWidth = ValidateFieldInRange(InstructionsWidthTextBox, 60, tour.TourSize.Width, InstructionsWidthError);
			if (!pageValid)
				return;
		}

		if (mapCanZoom && !tourPage.IsGallery)
		{
			validMapZoomInsetSize = ValidateFieldInRange(MapZoomInsetSizeTextBox, 60, 400, MapZoomInsetSizeError);
			if (!pageValid)
				return;

			validInsetColor = ValidateColorSwatch(MapInsetColorSwatch);
			validControlOffColor = ValidateColorSwatch(ControlOffColorSwatch);
		}

		ValidateHotspotOptions();
	}

	private void ValidateHotspotOptions()
	{
		if (ShowSlideListCheckBox.Checked)
		{
			validSlideListInstructions = SlideListInstructionsTextBox.Text.Trim();
			ValidateFieldNotBlank(validSlideListInstructions, SlideListInstructionsError, Resources.Text.ErrorSlideListInstructionsRequired);
			if (!pageValid)
				return;
		}

		if (!pageValid)
			return;

		validSelectedMarkerBlink = ValidateFieldInRange(SelectedMarkerBlinkTextBox, 0, 1000, MarkerBlinkError);

		if (!pageValid)
			return;

		validVisitedMarkerAlpha = ValidateFieldInRange(VisitedMarkerOpacityTextBox, 0, 100, VisitedMarkerOpacityError);
		if (!pageValid)
			return;
	}

	private void ClearErrors()
	{
		ClearErrors(
			InstructionsWidthError,
			MarkerBlinkError,
			VisitedMarkerOpacityError,
			MapZoomInsetSizeError);
	}

	private void ShowCodeSnippets()
	{
		string url = string.Format("{0}/page{1}.htm", tour.Url, tourPage.PageNumber);

		if (tour.HasBeenPublished && !tour.HasChangedSinceLastPublished)
			url = string.Format("<a href='{0}' target='_blank'>{0}</a>", url);

		CodeSnippets.Text = url;
	}
}
