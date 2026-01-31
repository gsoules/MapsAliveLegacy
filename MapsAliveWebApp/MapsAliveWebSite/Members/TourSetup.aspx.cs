// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Members_TourSetup : MemberPage
{
	private bool isGallery;
	private bool isNewTour;
	private int tourWidth;
	private int tourHeight;
	private string validTourName;

    protected void AddChangeDetection(CheckBox checkBox, string script)
    {
        checkBox.Attributes.Add("onclick", script);
    }

    protected override void InitControls(bool undo)
	{
		ColorSchemeComboBox.SelectedResourceId = tour == null ? account.DefaultColorSchemeId : tour.ColorScheme.Id;
		
		if (isNewTour)
		{
			EditColorSchemeControl.Visible = false;
            InitTourTypeRadioButtons();
        }
        else
		{
		    if (tour.V4)
                InitTourTypeRadioButtons();
			ColorSchemeComboBox.OnClientSelectedIndexChangedScript = "maColorSchemeChanged();";
			EditColorSchemeControl.OnClickActionId = MemberPageActionId.EditColorScheme;
			EditColorSchemeControl.Title = "Edit";
			EditColorSchemeControl.QueryString = "?id=" + tour.ColorScheme.Id;
		}
	
		if (!undo && IsPostBack)
			return;

		TourNameTextBox.Text = isNewTour ? Tour.CreateNewTourName() : tour.Name;


        if (isNewTour)
		{
            WebSiteOptionsPanel.Visible = true;
            WebSiteOptionsPanel.Style.Add(HtmlTextWriterStyle.Display, "none");
            RadioButtonWebSiteMap.Checked = true;

            AddChangeDetection(RadioButtonTypeFlexMap, "maShowWebSitePageTypes();");
            AddChangeDetection(RadioButtonTypeWebSite, "maShowWebSitePageTypes();");
        }
        else
		{
			AddChangeDetection(TourNameTextBox);
			
			if (tour.V3CompatibilityEnabled)
            {
                V4RadioButtons.Visible = false;
            }
            else
            {
                AddChangeDetection(RadioButtonTypeFlexMap, "maOnEventSave();");
			    AddChangeDetection(RadioButtonTypeWebSite, "maOnEventSave();");
            }
		}
	}

	private void InitTourTypeRadioButtons()
	{
        RadioButtonTypeFlexMap.Text = "Flex Map";
        RadioButtonTypeWebSite.Text = "Classic Tour";

        RadioButtonWebSiteMap.Text = "Map";
        RadioButtonWebSiteGallery.Text = "Gallery";
        RadioButtonWebSiteDataSheet.Text = "Data Sheet";

		if (isNewTour)
        {
            RadioButtonTypeFlexMap.Checked = true;
            ColorSchemePanel.Visible = false;
        }
		else
        {
            // V4 users set the color scheme on the Advanced Tour Options page.
            if (tour.V4)
            {
                ColorSchemePanel.Visible = false;
            }

            if (tour.CanBeFlexMapTour)
            {
                RadioButtonForTourType().Checked = true;
            }
            else
            {
                ArrayList list = tour.FlexMapTourDisqualifiers();
                string reasons = "";
                foreach (string reason in list)
                    reasons += "<li>" + reason + "</li>";

                if (reasons.Length > 0)
                {
                    FlexMapDisqualifierList.Visible = true;
                    string text = "The Flex Map option is disabled because this Classic tour<br/>uses the following features which are not used by Flex Map tours:";
                    text += "<ul>" + reasons + "</ul>";
                    FlexMapDisqualifierList.Text = text;
                }

                
                RadioButtonTypeFlexMap.Enabled = false;
                RadioButtonTypeWebSite.Checked = true;
            }
        }
	}

	private void AdjustTourHeight()
	{
		
		int optionsHeight = TourLayout.CalculateHeightOfTourOptions(tour, tourHeight);
		tourHeight -= optionsHeight;
	}

	private RadioButton RadioButtonForTourType()
	{
        return tour.IsFlexMapTour ? RadioButtonTypeFlexMap : RadioButtonTypeWebSite;
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);

		isNewTour = Request.QueryString["new"] == "1";

        Tour tour = MapsAliveState.SelectedTourOrNull;
        
		if (isNewTour)
		{
			SetPageTitle("New Tour Setup");
			SetPageReadOnly();
            SetActionId(MemberPageActionId.TourSetupNew);
            ButtonPanel.Visible = true;
            isGallery = false;
		}
		else
		{
			SetPageTitle("Tour Setup");
		    GetSelectedTour();
			SetActionId(MemberPageActionId.TourSetup);
			ButtonPanel.Visible = tour == null;
			if (tour.FirstPageId != 0)
				isGallery = tour.FirstPage.IsGallery;
		}

		ColorSchemeComboBox.ResourceType = TourResourceType.TourStyle;
	}

	protected override void PerformUpdate()
	{
		if (isNewTour)
            return;

		bool settingsChanged = false;
		bool tourNameChanged = false;

        if (validTourName != tour.Name)
		{
			tour.Name = validTourName;
			tourNameChanged = true;
			settingsChanged = true;
		}

		if (tour.V4 && !RadioButtonForTourType().Checked)
		{
            // The user changed the tour type.
            if (RadioButtonTypeFlexMap.Checked)
                tour.RuntimeTarget |= Tour.MapViewerFlags.IsFlexMapTour;
            else
                tour.RuntimeTarget &= ~Tour.MapViewerFlags.IsFlexMapTour;

            settingsChanged = true;
		}
		
		int colorSchemeId = ColorSchemeComboBox.SelectedResourceId;
		if (tour.ColorScheme.Id != colorSchemeId)
		{
			SetColorScheme(colorSchemeId);
			settingsChanged = true;
		}

		if (settingsChanged)
		{
            foreach (TourPage tourPage in tour.TourPages)
            {
                tourPage.InvalidateThumbnail();
                
                // Handle the case where the user changed from a Flex Map to a Classic tour, but has maps that
                // use the flex-focus feature (focus percent is -1). Since flex-focus is not supported for Classic
                // tours, change it to locked zoomed out (focus percent is 0).
                if (!tour.IsFlexMapTour && tourPage.MapFocusPercent == -1)
                {
                    tourPage.MapFocusPercent = 0;
                    tourPage.UpdateDatabase();
                }
            }

            tour.UpdateDatabase();
			if (tourNameChanged)
            {
			    tour.RebuildTourTreeXml();
				MapsAliveState.Flush(MapsAliveObjectType.TourList);
            }

            TourBuilder tourBuilder = new TourBuilder(tour);
            tourBuilder.BuildTour();
        }
    }

	protected override void ReadPageFields()
	{
        tourWidth = isNewTour ? 1200 : tour.TourSize.Width;
        tourHeight = isNewTour ? 800 : tour.TourSize.Height;
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}

	private void SetColorScheme(int ColorSchemeId)
	{
		tour.ColorScheme = Account.GetCachedColorScheme(ColorSchemeId);
		ColorScheme.SynchronizeColorsForDirectory(tour);
		ColorScheme.SynchronizeColorsForPopup(tour);
	}

	protected void OnCreateTour(object sender, EventArgs e)
	{
		if (!pageValid)
			return;

		bool noDirectory = false;
        bool noExtras = true;
		tour = Tour.CreateNewTour(validTourName, noExtras, noDirectory);

        TourDirectoryLocation directoryLocation;

        if (RadioButtonTypeFlexMap.Checked)
        {
            tour.RuntimeTarget |= Tour.MapViewerFlags.IsFlexMapTour;
            tour.HasTitle = false;
            directoryLocation = TourDirectoryLocation.MapLeft;
        }
        else
        {
            tour.HasTitle = true;
            directoryLocation = TourDirectoryLocation.TitleBar;
        }

        isGallery = RadioButtonTypeWebSite.Checked && RadioButtonWebSiteGallery.Checked;

        AdjustTourHeight();
		tour.MaxTourSize = new Size(tourWidth, tourHeight);
		tour.HeightType = isGallery ? TourSizeType.Exact : TourSizeType.LayoutArea;
		tour.WidthType = isGallery ? TourSizeType.Exact : TourSizeType.LayoutArea;
		tour.SetTourSizeAndAdjustLayouts(new Size(tourWidth, tourHeight));

		SetColorScheme(ColorSchemeComboBox.SelectedResourceId);

		tour.UpdateDatabase();

        tour.Directory.Location = directoryLocation;
        tour.Directory.UpdateDatabase();

        bool isDataSheet = RadioButtonTypeWebSite.Checked && RadioButtonWebSiteDataSheet.Checked;
        string targetPage = MemberPageAction.ActionPageTarget(isDataSheet ? MemberPageActionId.AddDataSheet : MemberPageActionId.MapSetup);

        targetPage += (isDataSheet ? "&" : "?") + "newtour=1";
		if (isGallery)
			targetPage += "&gallery=1";
		TransferToMemberPage(targetPage);
	}
		
	protected override bool SetStatus()
	{
        if (!account.ShowStepByStepHelp)
            return false;

        StatusBox.Clear();

        if (tour == null)
        {
            StatusBox.NextAction = "Set up a new tour";
		    StatusBox.SetStep(1, "[Type a name] for your tour.");
		    StatusBox.SetStep(2, "Press the [Create Tour] button.");
		    return true;
        }

        return ShowSmartStatusBox("tour's map");

	}

	protected override void Undo()
	{
		ClearErrors();
	}

	protected override void ValidatePage()
	{
		ClearErrors();

		validTourName = TourNameTextBox.Text.Trim();
		ValidateFieldNotBlank(validTourName, TourNameError, Resources.Text.ErrorTourNameRequired);

		if (fieldValid && (isNewTour || validTourName.ToLower() != tour.Name.ToLower()))
		{
			bool nameInUse = Tour.TourNameInUse(validTourName);
			ValidateFieldCondition(!nameInUse, TourNameError, Resources.Text.ErrorTourNameInUse);
		}
	}

	private void ClearErrors()
	{
		ClearErrors(TourNameError);
	}
}
