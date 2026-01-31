// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Members_MapSetup : MemberPage
{
	private bool isGallery;
	private bool isNewMap;
	private bool isNewTour;
	private bool mapNameChanged;
	private bool settingsChanged;
	private string validMapName;
    bool highlightTourNavigator;

    protected override void EmitJavaScript()
    {
        string loadedScript = string.Format("maHighlightTourNavigator({0});", highlightTourNavigator ? "true" : "false");
        EmitJavaScript(string.Empty, loadedScript);
    }

    protected void AddChangeDetection(CheckBox checkBox, string script)
    {
        checkBox.Attributes.Add("onclick", script);
    }

    protected override void InitControls(bool undo)
	{
		if (isNewMap && !isNewTour)
		{
			NewMapOptionsPanel.Visible = true;
			RadioButtonSetup.Checked = true;
			CopyMapPanel.Style.Add(HtmlTextWriterStyle.Display, "none");
			FirstHotspotPanel.Visible = false;
		}
		else
		{
			NewMapOptionsPanel.Visible = false;
			CopyMapPanel.Visible = false;
			FirstHotspotPanel.Visible = !isNewTour && tourPage.TourViews.Count >= 1;
        }

		if (isGallery)
		{
			MapZoomPanel.Visible = false;
			TourPageName.Title = "Gallery Name";
			TourPageName.Topic = "GalleryName";
			NewMapAppearance.Topic = "GalleryAppearance";
		}
        
        // Map zoom is automatic in V4 so don't show the option to enable it.
        if (tour.V4)
            MapZoomPanel.Visible = false;

		if (!undo && IsPostBack)
			return;

		TourPageNameTextBox.Text = isNewMap ? Tour.CreateNewTourPageName(null, isGallery, false, tour.NextMapId + 1) : tourPage.Name;

        if (tour.IsFlexMapTour)
            LayoutOptionsPanel.Visible = false;

        if (isNewMap)
		{
            RadioButtonPopup.Checked = true;

            if (!isNewTour)
			{
				InitOtherMapList();
				RadioButtonCopy.Text = string.Format("Copy settings from another {0} in this tour", isGallery ? "gallery" : "map");
			}
		}
		else
		{
			AddChangeDetection(TourPageNameTextBox);

			if (!isGallery)
			{
				MapCanZoomCheckBox.Checked = tourPage.MapCanZoom;
				AddChangeDetection(MapCanZoomCheckBox);
			}

			AddChangeDetection(RadioButtonPopup, "maOnEventSave();");
			AddChangeDetection(RadioButtonTiled, "maOnEventSave();");

			if (tourPage.SlidesPopup)
				RadioButtonPopup.Checked = true;
			else
				RadioButtonTiled.Checked = true;

			// Populate the first hotspot combo box.
			foreach (TourView tourView in tourPage.TourViewsBySequence)
			{
				RadComboBoxItem comboBoxItem = new RadComboBoxItem(tourView.Title, tourView.Id.ToString());
				FirstHotspotComboBox.Items.Add(comboBoxItem);
				if (tourView.Id == tourPage.FirstTourView.Id)
					comboBoxItem.Selected = true;
			}
		}

		ImageTiled.ImageUrl = isGallery ? "../Images/MapSetupTiledGallery.jpg" : "../Images/MapSetupTiled.jpg";
		ImagePopup.ImageUrl = isGallery ? "../Images/MapSetupPopupGallery.jpg" : "../Images/MapSetupPopup.jpg";
		RadioButtonTiled.Text = string.Format("Tiled<span style='color:#999;'> (content next to {0})</span>", isGallery ? "gallery" : "map");
		RadioButtonPopup.Text = string.Format("Popup<span style='color:#999;'> (content pops up over {0})", isGallery ? "gallery" : "map");
	}

	private void InitOtherMapList()
	{
		int mapCount = 0;
		foreach (TourPage page in tour.TourPages)
		{
			if (!ShowInMapList(page))
				continue;
			mapCount++;
		}

		if (mapCount > 0)
		{
			PageDropDownList.Items.Clear();
			foreach (TourPage page in tour.TourPages)
			{
				if (!ShowInMapList(page))
					continue;
				ListItem item = new ListItem(page.Name, page.Id.ToString());
				PageDropDownList.Items.Add(item);
			}

			PageDropDownList.SelectedIndex = PageDropDownList.Items.Count - 1;
		}
		else
		{
			NewMapOptionsPanel.Visible = false;
			SetupOptionsPanel.Style.Add(HtmlTextWriterStyle.Display, "block");
			RadioButtonPopup.Checked = true;
		}
	}

	private bool ShowInMapList(TourPage page)
	{
		if (page.IsDataSheet)
			return false;
		if (isGallery && !page.IsGallery)
			return false;
		if (!isGallery && page.IsGallery)
			return false;
		return true;
	}

	protected override void PageLoad()
	{
		Utility.RegisterColorChooserJavaScript(this);

		SetMasterPage(Master);

		isNewTour = Request.QueryString["newtour"] == "1";
		isNewMap = isNewTour || Request.QueryString["new"] == "1";

        string mapSetupTitle = MapsAliveState.SelectedTour.IsFlexMapTour ? "Flex Map" : "Map";

		if (isNewMap)
		{
			isGallery = Request.QueryString["gallery"] == "1";
			SetActionId(isGallery ? MemberPageActionId.GallerySetupNew : MemberPageActionId.MapSetupNew);
			SetPageTitle(isGallery ? "New Gallery Setup" : "Setup for New " + mapSetupTitle);
			SetPageReadOnly();
			GetSelectedTour();
		}
		else
		{
			GetSelectedTourPage();
			isGallery = tourPage.IsGallery;
			SetActionId(isGallery ? MemberPageActionId.GallerySetup : MemberPageActionId.MapSetup);
			SetPageTitle(isGallery ? "Gallery Setup" : mapSetupTitle + " Setup");
            ButtonPanel.Visible = false;
		}
    }

    protected override void PerformUpdate()
	{
		if (isNewMap)
			return;

		if (settingsChanged)
		{
			tourPage.UpdateDatabase();

			if (mapNameChanged)
			{
				tour.RebuildTourTreeXml();
			}
		}

		if (tourPage.TourViews.Count > 0)
		{
			int newFirstHotspotId = int.Parse(FirstHotspotComboBox.SelectedItem.Value);
			tourPage.SetFirstTourView(newFirstHotspotId);
		}
	}
	
	protected override void ReadPageFields()
	{
		if (isNewMap)
			return;

		if (validMapName != tourPage.Name)
		{
			settingsChanged = true;
			tourPage.Name = validMapName;
			mapNameChanged = true;
		}

		if ((tourPage.SlidesPopup && RadioButtonTiled.Checked) || (!tourPage.SlidesPopup && RadioButtonPopup.Checked))
		{
			AutoLayout.HandleTogglePopupRequest(tourPage);
		}

		if (tour.V3CompatibilityEnabled && tourPage.MapCanZoom != MapCanZoomCheckBox.Checked)
		{
			settingsChanged = true;
			tourPage.MapCanZoom = MapCanZoomCheckBox.Checked;
		}
        else
        {
            // All maps are zoomable in V4.
            tourPage.MapCanZoom = true;
        }
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

	protected void OnAddMap(object sender, EventArgs e)
	{
		if (!pageValid)
			return;

        tour.SetNothingSelected();

		// Create a new map page and insert it into the database.
		TourPage newTourPage = tour.CreateNewTourPage(isGallery, false, validMapName, RadioButtonPopup.Checked);

		if (RadioButtonSetup.Checked || isNewTour)
		{
			newTourPage.MapCanZoom = tour.V3CompatibilityEnabled ? MapCanZoomCheckBox.Checked : true;
			newTourPage.ShowSlideTitle = true;

			ColorScheme.SynchronizeColorsForPopup(newTourPage, tour.ColorScheme);
		}

		if (isGallery)
		{
			newTourPage.IsGallery = true;
			newTourPage.PopupOptions.SetGalleyDefaults();
			newTourPage.MapPlaceholderColor = "#eeeeee";
		}

		tour.AddTourPage(newTourPage, false);

		if (RadioButtonCopy.Checked)
		{
			// Clone the other map's page.
			int clonedTourPageId = int.Parse(PageDropDownList.SelectedValue);
			TourPage clonedTourPage = new TourPage(tour, clonedTourPageId);

			// Update the cloned page with the new page's Id and name.
			clonedTourPage.ChangeId(newTourPage.Id);
			clonedTourPage.Name = newTourPage.Name;
			clonedTourPage.PageId = newTourPage.PageId;

			// Detach the cloned page's map image from the clone.
			clonedTourPage.SetNoImage();
			clonedTourPage.SetMapImageChanged();

			// Overwrite the newly created page with the cloned page.
			clonedTourPage.UpdateDatabase();

			// Flush the newly created tour page from memory so that it won't get retrieved
			// from cache instead of the cloned page that got the new page's Id.
			tour.SetNothingSelected();
			tour.ReloadTourPages();

			// Set the cloned page as the selected page. This will cause it and all of
			// its dependent objects, such as popup options, to be constructed from the database.
			tour.SetSelectedTourPage(newTourPage.Id);

			// Update this member page's state so that it's referring to the new page.
			GetSelectedTourPage();
		}

		string targetPage = MemberPageAction.ActionPageTarget(isGallery ? MemberPageActionId.ImportHotspotPhotos : MemberPageActionId.UploadMap);
        targetPage += "?newmap=1";

        TransferToMemberPage(targetPage);
	}

	protected override bool SetStatus()
	{
        if (!(isNewTour || isNewMap))
            return ShowSmartStatusBox("map");

        StatusBox.Clear();

        if (isGallery)
        {
            StatusBox.LastAction = "Step-by-step instructions are not available for galleries";
            return true;
        }

        MemberPageActionId lastAction = Utility.LastAction;

        if (tourPage == null)
        {
            if (isNewTour)
            {
                StatusBox.LastAction = "Your tour has been created";
                highlightTourNavigator = true;
            }
            else
            {
                StatusBox.LastAction = "This tour has no map";
            }
        }
            
        StatusBox.NextAction = "Choose a map name";
		StatusBox.SetStep(1, "[Type a name] for your <a'ref-maps'>map</a>.");

        int step = 2;
        if (!isGallery && !tour.IsFlexMapTour)
        {
            StatusBox.SetStep(step, "Choose whether to show content in [popups] or using a [tiled layout].");
            step += 1;
        }

		StatusBox.SetStep(step, "Press the [Create Map] button.");
		return true;
	}

	protected override void Undo()
	{
		ClearErrors();
	}

	protected override void ValidatePage()
	{
		ClearErrors();

		validMapName = TourPageNameTextBox.Text.Trim();
		ValidateFieldNotBlank(validMapName, TourPageNameError, Resources.Text.ErrorTourPageNameRequired);

		if (fieldValid && (isNewMap || validMapName.ToLower() != tourPage.Name.ToLower()))
		{
			bool nameInUse = TourPage.TourPageNameInUse(tour, 0, validMapName);
			ValidateFieldCondition(!nameInUse, TourPageNameError, Resources.Text.ErrorTourPageNameInUse);
		}
	}

	private void ClearErrors()
	{
		ClearErrors(TourPageNameError);
	}
}
