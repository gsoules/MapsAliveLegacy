// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Members_Gallery : MemberPage
{
	private string validGalleryBackgroundColor;
	private int validMarginTop;
	private int validMarginLeft;
	private int validRowSpacing;
	private int validColumnSpacing;

	protected override void EmitJavaScript()
	{
        string loadingScript = AssignClientVar("actionIdEditView", (int)MemberPageActionId.EditHotspotContent);
        EmitJavaScript(loadingScript, "");

		string previewPath = tour.Url + "_";

		string version = "?v=" + tour.BuildId;

		// Emit the JavaScript include files.
		var scriptTag = new HtmlGenericControl { TagName = "script" };
		scriptTag.Attributes.Add("type", "module");
		
		string mapJsFileName = string.Format(TourBuilder.PatternForMapJsFile, tour.SelectedTourPage.PageNumber, tour.BuildId);
		scriptTag.Attributes.Add("src", ResolveUrl(string.Format("{0}/{1}{2}", previewPath, mapJsFileName, version)));
		
		this.Page.Header.Controls.Add(scriptTag);
	}

	protected void AddChangeDetection(CheckBox checkBox, string script)
	{
		checkBox.Attributes.Add("onclick", script);
	}

	protected override void InitControls(bool undo)
	{
		mapControls.IsGallery = true;

        // Hide V3 options that are not needed in V4 because the Map Editor has an Edit button
        if (tour.V4)
        {
            EditHotspotOption.Visible = false;
            EditMarkerOption.Visible = false;
        }

		HotspotOrderListBox.ButtonSettings.RenderButtonText = false;
		HotspotOrderListBox.ButtonSettings.Position = ListBoxButtonPosition.Right;
		
		if (tourPage.TourViews.Count > 1)
		{
			HotspotOrderListBox.AllowReorder = tourPage.TourViews.Count > 1;
		}
		else
		{
			HotspotOrderListBox.Height = 40;
			HotspotOrderListBox.EmptyMessage = "This gallery has no hotspots yet";
		}
		int offset = tourPage.MapAreaSize.Width + 16;
		HotspotListPanel.Style.Add(HtmlTextWriterStyle.Left, offset.ToString() + "px");
		HotspotListPanel.Style.Add(HtmlTextWriterStyle.Top, "-22px");

		EditHotspotControl.OnClickJavaScript = string.Format("maEdit(1,{0});", (int)MemberPageActionId.EditHotspotContent);
		EditMarkerControl.OnClickJavaScript = "maEdit(0);";
		
		if (!undo && IsPostBack)
			return;

		GalleryOptions options = tourPage.GalleryOptions;

		GalleryBackgroundColorSwatch.ColorValue = tourPage.MapPlaceholderColor;

		RowSpacing.Text = options.SpacingRow.ToString();
		AddChangeDetectionForPreview(RowSpacing);

		ColumnSpacing.Text = options.SpacingColumn.ToString();
		AddChangeDetectionForPreview(ColumnSpacing);
		
		MarginTop.Text = options.MarginTop.ToString();
		AddChangeDetectionForPreview(MarginTop);
		
		MarginLeft.Text = options.MarginLeft.ToString();
		AddChangeDetectionForPreview(MarginLeft);

		AutoSpacingRowCheckBox.Checked = options.AutoSpacingRow;
		AddChangeDetectionForPreview(AutoSpacingRowCheckBox);

		AutoSpacingColumnCheckBox.Checked = options.AutoSpacingColumn;
		AddChangeDetectionForPreview(AutoSpacingColumnCheckBox);

		UseFixedRowHeightCheckbox.Checked = options.UseFixedRowHeight;
		AddChangeDetectionForPreview(UseFixedRowHeightCheckbox);

		UseFixedColumnWidthCheckbox.Checked = options.UseFixedColumnWidth;
		AddChangeDetectionForPreview(UseFixedColumnWidthCheckbox);

		AlignVDropDownList.Items.Clear();
		AlignVDropDownList.Items.Add(new ListItem("Top", ((int)GalleryCellAlignV.Top).ToString()));
		AlignVDropDownList.Items.Add(new ListItem("Center", ((int)GalleryCellAlignV.Center).ToString()));
		AlignVDropDownList.Items.Add(new ListItem("Bottom", ((int)GalleryCellAlignV.Bottom).ToString()));
		AlignVDropDownList.SelectedValue = ((int)options.CellAlignV).ToString();
		AddChangeDetectionForPreview(AlignVDropDownList);

		AlignHDropDownList.Items.Clear();
		AlignHDropDownList.Items.Add(new ListItem("Left", ((int)GalleryCellAlignH.Left).ToString()));
		AlignHDropDownList.Items.Add(new ListItem("Center", ((int)GalleryCellAlignH.Center).ToString()));
		AlignHDropDownList.Items.Add(new ListItem("Right", ((int)GalleryCellAlignH.Right).ToString()));
		AlignHDropDownList.SelectedValue = ((int)options.CellAlignH).ToString();
		AddChangeDetectionForPreview(AlignHDropDownList);

		BackgroundImageDropDownList.Items.Clear();
		BackgroundImageDropDownList.Items.Add(new ListItem("Center", ((int)ImageExpansionType.Center).ToString()));
		BackgroundImageDropDownList.Items.Add(new ListItem("Repeat", ((int)ImageExpansionType.Repeat).ToString()));
		BackgroundImageDropDownList.Items.Add(new ListItem("Top Left", ((int)ImageExpansionType.UpperLeft).ToString()));
		BackgroundImageDropDownList.SelectedValue = ((int)options.BackgroundType).ToString();
		AddChangeDetectionForPreview(BackgroundImageDropDownList);

		LoadHotspotListBox();

		InitMapAreaSize();
	}
	private void InitMapAreaSize()
	{
        MapArea.Width = tourPage.MapAreaSize.Width;
        MapArea.Height = tourPage.MapAreaSize.Height;
		mapControls.MapCanZoom = tourPage.MapCanZoom;
	}

    protected string IsPlusOrProPlan
    {
        get { return account.IsPlusOrProPlan ? "true" : "false"; }
    }

    protected string DefaultMarkerId
    {
        get
        {
            int markerId = account.LastResourceId(TourResourceType.Marker);
            if (markerId == 0)
                markerId = account.DefaultResourceId(TourResourceType.Marker);
            return markerId.ToString();
        }
    }

    protected string DefaultMarkerStyleId
    {
        get
        {
            int markerStyleId = account.LastResourceId(TourResourceType.MarkerStyle);
            if (markerStyleId == 0)
                markerStyleId = account.DefaultResourceId(TourResourceType.MarkerStyle);
            return markerStyleId.ToString();
        }
    }

    private void LoadHotspotListBox()
	{
        // Empty the list in case this is a postback. The list has to be repopulated each time to
        // make sure that hotspots that don't currently fit on the map are marked with an asterisk.
        // Since the user could have reordered the list, the off-map hotspots could have changed.
        HotspotOrderListBox.Items.Clear();

        // Populate the hotspot order list. If a view's marker is in the gallery, but does not fit
        // on the map, prefix the view's title with an astersisk.
        foreach (TourView tourView in tourPage.TourViewsBySequence)
		{
			string value = string.Format("{0},{1}", tourView.Id, tourView.MarkerId);
            string title = (tourView.MarkerHasBeenPlacedOnMap ? "" : "*") + tourView.Title;
            HotspotOrderListBox.Items.Add(new RadListBoxItem(title, value));
        }
	}

	protected override void PageLoad()
	{
		Utility.RegisterMapEditorJavaScript(this);
		Utility.RegisterColorChooserJavaScript(this);

		SetMasterPage(Master);
		SetPageTitle("Gallery Editor");
		SetActionIdForPageAction(MemberPageActionId.Gallery);
		GetSelectedTourPage();

		if (tourPage.GallerySize != Size.Empty)
		{
			// Display the dimensions of the area within the map area that is occupied by the gallery.
			// The purpose is to know how to manually size the tour if you want it to better fit the
			// gallery. We have to test for an empty size because tourPage.GallerySize is not actually
			// updated until the gallery SWF gets constructed which does not happen until after this ASPX
			// page loads. Thus for a new gallery, the size won't be set, and for an updated gallery, the
			// size will be stale until the next time this page is loaded (in other words, the dimensions
			// we show will be wrong). What we really need is a way to dynamically update the client
			// after we know that the gallery's map swf has loaded. For now we live with what we have
			// since this is just nice-to-have information and is not critical.
			GallerySize.Text = string.Format("{0} x {1}", tourPage.GallerySize.Width, tourPage.GallerySize.Height);
		}

        // When the page first loads, force a rebuild of the tour to deal with the fact that when a user creates
        // or deletes a gallery hotspot, that hotspot needs to be added to or removed from the gallery and that
        // can only happen by rebuilding the gallery. This is similar to rebuilding the tour before displaying
        // the Tour Preview page if anything about the tour has changed. The difference here is that the rebuild
        // will happen when going to the Gallery Editor whether or not hotspots have been added or deleted. It's
        // like this because unlike maps where the user explicitly chooses to place a marker on the map and then
        // drag it to the desired location, or chooses to remove the marker from the map by clicking the X in the
        // Map Editor, gallery markers are arranged on the map server-side by the Tour Builder and for that
        // arrangment to occur, the tour must get rebuilt. Similarly, if while in the Gallery Editor the user
        // reorders a marker, or changes a gallery layout option, the tour gets rebuilt when the Gallery is saved.
        if (!IsPostBack)
        {
            TourBuilder tourBuilder = new TourBuilder(tour);
            tourBuilder.BuildTour();
        }
    }

    protected string PathTourFolder
    {
        get { return tour.Url + "_"; }
    }

    protected override void PerformUpdate()
	{
		tourPage.UpdateMarkerCoords(MarkerCoords.Value);
		PerformUpdateOfHotspotOrder();

        TourBuilder tourBuilder = new TourBuilder(tour);
        tourBuilder.BuildTour();

        tourPage.UpdateDatabase();

        LoadHotspotListBox();
    }

    private void PerformUpdateOfHotspotOrder()
	{
		int position = 0;
		foreach (RadListBoxItem item in HotspotOrderListBox.Items)
		{
			int tourViewId = int.Parse(item.Value.Split(',')[0]);
			TourView tourView = tourPage.GetTourView(tourViewId);
			position++;
			if (tourView.SequenceNumber != position)
			{
				tourView.SetSequenceNumber(position);
			}
            if (position == 1)
            {
                tourPage.SetFirstTourView(tourViewId);
            }
		}
	}

	protected override void ReadPageFields()
	{
		GalleryOptions options = tourPage.GalleryOptions;
		ImageExpansionType backgroundType = (ImageExpansionType)int.Parse(BackgroundImageDropDownList.SelectedValue);

		if (tourPage.MapPlaceholderColor != validGalleryBackgroundColor || options.BackgroundType != backgroundType)
		{
			tourPage.MapPlaceholderColor = validGalleryBackgroundColor;
			
			// Force a new background map image to be constructed.
			tourPage.SetMapImageChanged();
		}
		
		options.SpacingRow = validRowSpacing;
		options.SpacingColumn = validColumnSpacing;
		options.AutoSpacingRow = AutoSpacingRowCheckBox.Checked;
		options.AutoSpacingColumn = AutoSpacingColumnCheckBox.Checked;
		options.MarginTop = validMarginTop;
		options.MarginLeft = validMarginLeft;
		options.UseFixedRowHeight = UseFixedRowHeightCheckbox.Checked;
		options.UseFixedColumnWidth = UseFixedColumnWidthCheckbox.Checked;
		options.CellAlignH = (GalleryCellAlignH)int.Parse(AlignHDropDownList.SelectedValue);
		options.CellAlignV = (GalleryCellAlignV)int.Parse(AlignVDropDownList.SelectedValue);
		options.BackgroundType = backgroundType;
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
		ClearErrors();

		validGalleryBackgroundColor = ValidateColorSwatch(GalleryBackgroundColorSwatch);
		validMarginTop = ValidateFieldInRange(MarginTop, 0, 200, MarginTopError);
		validMarginLeft = ValidateFieldInRange(MarginLeft, 0, 200, MarginLeftError);
		validRowSpacing = ValidateFieldInRange(RowSpacing, 0, 200, RowSpacingError);
		validColumnSpacing = ValidateFieldInRange(ColumnSpacing, 0, 200, ColumnSpacingError);
	}

	private void ClearErrors()
	{
		ClearErrors(
			MarginTopError,
			MarginLeftError,
			RowSpacingError,
			ColumnSpacingError);
	}
    
    protected string StyleDefinitions
    {
        get { return MapPageStyleDefinitions; }
    }
}
